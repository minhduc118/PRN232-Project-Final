using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.DTOs;
using SportCourtManagent_Server.DTOs.Court;
using SportCourtManagent_Server.DTOs.Review;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportCourtManagent_Server.Services.Implements
{
    public class CourtService : ICourtService
    {
        private readonly ICourtRepository _courtRepo;
        private readonly IReviewRepository _reviewRepo;
        private readonly AppDbContext _db;

        public CourtService(
            ICourtRepository courtRepo,
            IReviewRepository reviewRepo,
            AppDbContext db)
        {
            _courtRepo = courtRepo;
            _reviewRepo = reviewRepo;
            _db = db;
        }


        public async Task<PagedResult<CourtListDto>> SearchCourtsAsync(CourtSearchParams p)
        {
            var query = _courtRepo.GetCourtsQueryable();

            // Filters
            if (p.CourtTypeId.HasValue)
                query = query.Where(c => c.CourtTypeId == p.CourtTypeId.Value);

            if (!string.IsNullOrWhiteSpace(p.Status)
                && Enum.TryParse<CourtStatus>(p.Status, true, out var status))
                query = query.Where(c => c.Status == status);

            if (!string.IsNullOrWhiteSpace(p.SearchTerm))
            {
                var term = p.SearchTerm.Trim().ToLower();
                query = query.Where(c => c.CourtName.ToLower().Contains(term));
            }

            if (p.MinPrice.HasValue)
                query = query.Where(c => c.CourtPricings.Any(cp => cp.Price >= p.MinPrice.Value));

            if (p.MaxPrice.HasValue)
                query = query.Where(c => c.CourtPricings.Any(cp => cp.Price <= p.MaxPrice.Value));

            if (p.TimeSlotId.HasValue)
            {
                query = query.Where(c => c.CourtPricings.Any(cp => cp.SlotId == p.TimeSlotId.Value));
            }

            // Filter by availability on a specific date + time slot
            if (p.Date.HasValue)
            {
                var date = p.Date.Value.Date;
                query = query.Where(c =>
                    c.Status == CourtStatus.Available &&
                    // Exclude courts that have a booking for this date/slot
                    !c.Bookings.Any(b =>
                        b.BookingDate.Date == date &&
                        b.Status != BookingStatus.Cancelled &&
                        (!p.TimeSlotId.HasValue || b.SlotId == p.TimeSlotId.Value)));
            }

            
            var projected = query.Select(c => new CourtListDto
            {
                CourtId = c.CourtId,
                CourtName = c.CourtName,
                CourtCode = c.CourtCode,
                CourtTypeName = c.CourtType.TypeName,
                CourtTypeId = c.CourtTypeId,
                CourtSize = c.CourtSize,
                Location = c.Complex.ComplexName + " - " + c.Complex.Address,

                ImageUrl = c.CourtImages.OrderByDescending(ci => ci.IsPrimary).Select(ci => ci.ImageUrl).FirstOrDefault(),
                Status = c.Status.ToString(),
                OpenTime = c.OpenTime,
                CloseTime = c.CloseTime,
                PricePerHour = c.PricePerHour,
                MinPrice = c.CourtPricings.Any() ? c.CourtPricings.Min(cp => cp.Price) : null,
                MaxPrice = c.CourtPricings.Any() ? c.CourtPricings.Max(cp => cp.Price) : null,
                AverageRating = c.Reviews.Any(r => r.IsVisible)
                                    ? c.Reviews.Where(r => r.IsVisible).Average(r => (double)r.Rating)
                                    : null,
                ReviewCount = c.Reviews.Count(r => r.IsVisible),
            });

            //Sorting
            projected = (p.SortBy?.ToLower()) switch
            {
                "price" => p.SortDescending
                    ? projected.OrderByDescending(c => c.MinPrice)
                    : projected.OrderBy(c => c.MinPrice),
                "rating" => p.SortDescending
                    ? projected.OrderByDescending(c => c.AverageRating)
                    : projected.OrderBy(c => c.AverageRating),
                "name" => p.SortDescending
                    ? projected.OrderByDescending(c => c.CourtName)
                    : projected.OrderBy(c => c.CourtName),
                _ => projected.OrderBy(c => c.CourtName),

            };

            //Pagination
            var totalCount = await projected.CountAsync();
            var items = await projected
                .Skip((p.PageNumber - 1) * p.PageSize)
                .Take(p.PageSize)
                .ToListAsync();

            return new PagedResult<CourtListDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = p.PageNumber,
                PageSize = p.PageSize,
            };
        }

       
        public async Task<CourtDetailDto?> GetCourtDetailAsync(int courtId)
        {
            var court = await _courtRepo.GetCourtDetailAsync(courtId);
            if (court is null) return null;

            // Get full rating summary
            var (avgRating, totalCount, distribution) =
                await _reviewRepo.GetCourtRatingSummaryAsync(courtId);

            return new CourtDetailDto
            {
                CourtId = court.CourtId,
                CourtName = court.CourtName,
                CourtCode = court.CourtCode,
                CourtSize = court.CourtSize,
                Location = court.Complex != null ? (court.Complex.ComplexName + " - " + court.Complex.Address) : "",

                ImageUrl = court.CourtImages.OrderByDescending(ci => ci.IsPrimary).Select(ci => ci.ImageUrl).FirstOrDefault(),
                OpenTime = court.OpenTime,
                CloseTime = court.CloseTime,
                PricePerHour = court.PricePerHour,
                Status = court.Status.ToString(),
                CourtType = court.CourtType != null ? new CourtTypeDto
                {
                    CourtTypeId = court.CourtType.CourtTypeId,
                    TypeName = court.CourtType.TypeName,
                    IsActive = court.CourtType.IsActive,
                    CourtCount = 0, // not needed in detail view
                } : new CourtTypeDto { CourtTypeId = court.CourtTypeId, TypeName = "", IsActive = true },
                Images = court.CourtImages.Select(ci => new CourtImageDto
                {
                    CourtImageId = ci.CourtImageId,
                    ImageUrl = ci.ImageUrl,
                    IsPrimary = ci.IsPrimary,
                }).ToList(),
                Pricings = court.CourtPricings
                    .Where(cp => cp.TimeSlot == null || (cp.TimeSlot.StartTime >= court.OpenTime && cp.TimeSlot.EndTime <= court.CloseTime))
                    .Select(cp => new CourtPricingDto
                    {
                        PricingId = cp.PricingId,
                        SlotId = cp.SlotId,
                        SlotName = cp.TimeSlot?.SlotName ?? "Không xác định",
                        StartTime = cp.TimeSlot?.StartTime ?? TimeSpan.Zero,
                        EndTime = cp.TimeSlot?.EndTime ?? TimeSpan.Zero,
                        DayType = cp.TimeSlot?.DayType.ToString() ?? "Weekday",

                        Price = cp.Price,
                    }).OrderBy(p => p.StartTime).ToList(),
                ReviewSummary = new CourtReviewSummaryDto
                {
                    AverageRating = avgRating,
                    TotalReviews = totalCount,
                    RatingDistribution = distribution,
                },
            };
        }

        
        public async Task<CourtAvailabilityDto?> GetCourtAvailabilityAsync(int courtId, DateTime date)
        {
            var court = await _courtRepo.GetCourtWithPricingsAsync(courtId);
            if (court is null) return null;

            // Get all bookings for this court on the given date (non-cancelled and non-expired)
            var targetDate = date.Date;
            var now = DateTime.UtcNow;
            var activeBookings = await _db.Bookings
                .AsNoTracking()
                .Where(b => b.CourtId == courtId
                             && b.BookingDate.Date == targetDate
                             && b.Status != BookingStatus.Cancelled
                             && (b.Status != BookingStatus.Pending || !b.ExpiredAt.HasValue || b.ExpiredAt > now))
                .Select(b => new { b.SlotId, b.Status })
                .ToListAsync();

            // Availability slots filter by court's operating hours (OpenTime & CloseTime)
            var timeSlots = await _db.TimeSlots
                .AsNoTracking()
                .Where(slot => slot.StartTime >= court.OpenTime && slot.EndTime <= court.CloseTime)
                .OrderBy(slot => slot.StartTime)
                .ToListAsync();

            if (!timeSlots.Any())
            {
                timeSlots = await _db.TimeSlots
                    .AsNoTracking()
                    .OrderBy(slot => slot.StartTime)
                    .ToListAsync();
            }

            // Court-wide statuses take precedence over individual booking statuses.
            var isUnderMaintenance = court.Status == CourtStatus.Maintenance;
            var isInactive = court.Status == CourtStatus.Inactive;

            // Scheduled maintenance windows that overlap this date
            var dayStart = targetDate;
            var dayEnd = targetDate.AddDays(1);
            var maintenanceWindows = await _db.MaintenanceSchedules
                .AsNoTracking()
                .Where(m => m.CourtId == courtId
                    && (m.Status == MaintenanceStatus.Scheduled || m.Status == MaintenanceStatus.InProgress)
                    && m.StartDateTime < dayEnd
                    && m.EndDateTime > dayStart)
                .Select(m => new { m.StartDateTime, m.EndDateTime })
                .ToListAsync();

            var pricingBySlot = court.CourtPricings
                .GroupBy(pricing => pricing.SlotId)
                .ToDictionary(group => group.Key, group => group.First());

            var slots = timeSlots
                .Select(slot =>
                {
                    pricingBySlot.TryGetValue(slot.SlotId, out var pricing);
                    var durationHours = (decimal)(slot.EndTime - slot.StartTime).TotalHours;
                    var booking = activeBookings.FirstOrDefault(item => item.SlotId == slot.SlotId);

                    var slotStart = targetDate.Add(slot.StartTime);
                    var slotEnd = targetDate.Add(slot.EndTime);
                    var overlapsMaintenance = isUnderMaintenance || maintenanceWindows.Any(m =>
                        slotStart < m.EndDateTime && slotEnd > m.StartDateTime);

                    return new AvailabilitySlotDto
                    {
                        SlotId = slot.SlotId,
                        SlotName = slot.SlotName,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        Price = pricing?.Price ?? court.PricePerHour * durationHours,
                        Status = isInactive
                                    ? "Inactive"
                                    : overlapsMaintenance
                                        ? "Maintenance"
                                        : booking != null
                                            ? booking.Status == BookingStatus.Pending ? "Held" : "Booked"
                                            : "Available",
                    };
                })
                .ToList();

            return new CourtAvailabilityDto
            {
                CourtId = court.CourtId,
                CourtName = court.CourtName,
                Date = date,
                Slots = slots,
            };
        }

      
        public IQueryable<CourtListDto> GetCourtsODataQueryable()
        {
            return _courtRepo.GetCourtsQueryable()
                .Select(c => new CourtListDto
                {
                    CourtId = c.CourtId,
                    CourtName = c.CourtName,
                    CourtCode = c.CourtCode,
                    CourtTypeName = c.CourtType.TypeName,
                    CourtTypeId = c.CourtTypeId,
                    CourtSize = c.CourtSize,
                    Location = c.Complex.ComplexName + " - " + c.Complex.Address,

                    ImageUrl = c.CourtImages.OrderByDescending(ci => ci.IsPrimary).Select(ci => ci.ImageUrl).FirstOrDefault(),
                    Status = c.Status.ToString(),
                    OpenTime = c.OpenTime,
                    CloseTime = c.CloseTime,
                    PricePerHour = c.PricePerHour,
                    MinPrice = c.CourtPricings.Any() ? c.CourtPricings.Min(cp => cp.Price) : null,
                    MaxPrice = c.CourtPricings.Any() ? c.CourtPricings.Max(cp => cp.Price) : null,
                    AverageRating = c.Reviews.Any(r => r.IsVisible)
                                        ? c.Reviews.Where(r => r.IsVisible).Average(r => (double)r.Rating)
                                        : null,
                    ReviewCount = c.Reviews.Count(r => r.IsVisible),
                });
        }

        // --- Admin CRUD methods from feature/model-admin ---

        public async Task<IEnumerable<CourtDto>> GetAllAsync(int? complexId, string? status)
        {
            var courts = await _courtRepo.GetAllWithDetailsAsync(complexId, status);
            return courts.Select(MapToDto).ToList();
        }

        public async Task<CourtDto?> GetByIdAsync(int id)
        {
            var court = await _courtRepo.GetByIdWithDetailsAsync(id);
            return court == null ? null : MapToDto(court);
        }

        public async Task<CourtDto> CreateAsync(CourtDto dto)
        {
            if (!System.TimeSpan.TryParse(dto.OpenTime, out var openTime) ||
                !System.TimeSpan.TryParse(dto.CloseTime, out var closeTime))
            {
                throw new System.ArgumentException("Giờ mở/đóng cửa không đúng định dạng.");
            }

            if (!System.Enum.TryParse<CourtStatus>(dto.Status, true, out var status))
            {
                status = CourtStatus.Available;
            }

            var court = new Court
            {
                CourtName = dto.CourtName,
                CourtCode = dto.CourtCode,
                CourtTypeId = dto.CourtTypeId,
                ComplexId = dto.ComplexId,
                Status = status,
                OpenTime = openTime,
                CloseTime = closeTime,
                PricePerHour = dto.PricePerHour,
                CourtSize = dto.CourtSize,
                IsDeleted = false
            };

            // Handle multiple images if provided in ImageUrls, otherwise fallback to ImageUrl
            var urlsToSave = dto.ImageUrls != null && dto.ImageUrls.Any()
                ? dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)).ToList()
                : (!string.IsNullOrWhiteSpace(dto.ImageUrl) ? new List<string> { dto.ImageUrl } : new List<string>());

            bool isFirst = true;
            foreach (var url in urlsToSave)
            {
                court.CourtImages.Add(new CourtImage
                {
                    ImageUrl = url,
                    IsPrimary = isFirst
                });
                isFirst = false;
            }

            await _courtRepo.AddAsync(court);

            var loaded = await _courtRepo.GetByIdWithDetailsAsync(court.CourtId);
            return loaded == null ? dto : MapToDto(loaded);
        }

        public async Task UpdateAsync(int id, CourtDto dto)
        {
            var court = await _courtRepo.GetByIdWithDetailsAsync(id);
            if (court == null) throw new System.Collections.Generic.KeyNotFoundException("Không tìm thấy sân.");

            if (!System.TimeSpan.TryParse(dto.OpenTime, out var openTime) ||
                !System.TimeSpan.TryParse(dto.CloseTime, out var closeTime))
            {
                throw new System.ArgumentException("Giờ mở/đóng cửa không đúng định dạng.");
            }

            if (!System.Enum.TryParse<CourtStatus>(dto.Status, true, out var status))
            {
                status = CourtStatus.Available;
            }

            court.CourtName = dto.CourtName;
            court.CourtCode = dto.CourtCode;
            court.CourtTypeId = dto.CourtTypeId;
            court.ComplexId = dto.ComplexId;
            court.Status = status;
            court.OpenTime = openTime;
            court.CloseTime = closeTime;
            court.PricePerHour = dto.PricePerHour;
            court.CourtSize = dto.CourtSize;

            var urlsToSave = dto.ImageUrls != null && dto.ImageUrls.Any()
                ? dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)).ToList()
                : (!string.IsNullOrWhiteSpace(dto.ImageUrl) ? new List<string> { dto.ImageUrl } : new List<string>());

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl) && urlsToSave.Contains(dto.ImageUrl.Trim()))
            {
                var primaryUrl = dto.ImageUrl.Trim();
                urlsToSave.Remove(primaryUrl);
                urlsToSave.Insert(0, primaryUrl);
            }

            if (urlsToSave.Any())
            {
                court.CourtImages.Clear();
                bool isFirst = true;
                foreach (var url in urlsToSave)
                {
                    court.CourtImages.Add(new CourtImage
                    {
                        CourtId = court.CourtId,
                        ImageUrl = url,
                        IsPrimary = isFirst
                    });
                    isFirst = false;
                }
            }

            await _courtRepo.UpdateAsync(court);

            // Save or update CourtPricings for all TimeSlots
            var allSlots = await _db.TimeSlots.ToListAsync();
            var existingPricings = await _db.CourtPricings
                .Where(cp => cp.CourtId == court.CourtId)
                .ToListAsync();

            foreach (var slot in allSlots)
            {
                var inputPricing = dto.Pricings?.FirstOrDefault(p => p.SlotId == slot.SlotId);
                var existing = existingPricings.FirstOrDefault(cp => cp.SlotId == slot.SlotId);

                decimal targetPrice;
                if (inputPricing != null && inputPricing.Price > 0)
                {
                    targetPrice = inputPricing.Price;
                }
                else
                {
                    var durationHours = (decimal)(slot.EndTime - slot.StartTime).TotalHours;
                    targetPrice = court.PricePerHour * (durationHours > 0 ? durationHours : 1.5m);
                }

                if (existing != null)
                {
                    existing.Price = targetPrice;
                    _db.CourtPricings.Update(existing);
                }
                else
                {
                    _db.CourtPricings.Add(new CourtPricing
                    {
                        CourtId = court.CourtId,
                        SlotId = slot.SlotId,
                        Price = targetPrice
                    });
                }
            }
            await _db.SaveChangesAsync();
        }

        public async Task<CourtLifecycleResultDto> DeactivateAsync(int id)
        {
            var court = await _courtRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Không tìm thấy sân.");

            if (court.IsDeleted)
                throw new InvalidOperationException("Sân đã bị xóa khỏi hệ thống.");

            var activeCount = await _db.Bookings.CountAsync(b =>
                b.CourtId == id &&
                (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));

            if (activeCount > 0)
            {
                throw new InvalidOperationException(
                    $"Sân đang có {activeCount} lịch đặt (Pending/Confirmed). Không thể ngưng hoạt động — chỉ được chuyển Bảo trì.");
            }

            court.Status = CourtStatus.Inactive;
            await _courtRepo.UpdateAsync(court);

            return new CourtLifecycleResultDto
            {
                CourtId = court.CourtId,
                CourtName = court.CourtName,
                Status = CourtStatus.Inactive.ToString(),
                Message = "Đã chuyển sân sang Ngưng hoạt động."
            };
        }

        public async Task<CourtLifecycleResultDto> RestoreAsync(int id)
        {
            var court = await _courtRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Không tìm thấy sân.");

            if (court.IsDeleted)
                throw new InvalidOperationException("Sân đã bị xóa khỏi hệ thống, không thể khôi phục.");

            court.Status = CourtStatus.Available;
            await _courtRepo.UpdateAsync(court);

            // Đóng các lịch bảo trì đang mở của sân (nếu có)
            var openMaint = await _db.MaintenanceSchedules
                .Where(m => m.CourtId == id &&
                    (m.Status == MaintenanceStatus.Scheduled || m.Status == MaintenanceStatus.InProgress))
                .ToListAsync();
            foreach (var m in openMaint)
            {
                m.Status = MaintenanceStatus.Cancelled;
                m.Result = "Đã hủy khi Admin khôi phục sân.";
            }
            if (openMaint.Count > 0)
                await _db.SaveChangesAsync();

            return new CourtLifecycleResultDto
            {
                CourtId = court.CourtId,
                CourtName = court.CourtName,
                Status = CourtStatus.Available.ToString(),
                Message = "Đã khôi phục sân về trạng thái Hoạt động."
            };
        }

        public async Task<MaintenanceConflictPreviewDto> PreviewMaintenanceConflictsAsync(
            int courtId, DateTime start, DateTime end)
        {
            ValidateMaintenanceWindow(start, end);

            var court = await _courtRepo.GetByIdAsync(courtId)
                ?? throw new KeyNotFoundException("Không tìm thấy sân.");

            var conflicts = await GetOverlappingActiveBookingsAsync(courtId, start, end);

            return new MaintenanceConflictPreviewDto
            {
                CourtId = court.CourtId,
                CourtName = court.CourtName,
                StartDateTime = start,
                EndDateTime = end,
                ConflictCount = conflicts.Count,
                TotalRefundAmount = conflicts.Sum(c => c.RefundAmount),
                Conflicts = conflicts
            };
        }

        public async Task<CourtLifecycleResultDto> ScheduleMaintenanceAsync(
            int courtId, ScheduleCourtMaintenanceRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateMaintenanceWindow(request.StartDateTime, request.EndDateTime);

            var court = await _courtRepo.GetByIdAsync(courtId)
                ?? throw new KeyNotFoundException("Không tìm thấy sân.");

            if (court.IsDeleted)
                throw new InvalidOperationException("Sân đã bị xóa khỏi hệ thống.");

            if (court.Status == CourtStatus.Inactive)
                throw new InvalidOperationException("Sân đang Ngưng hoạt động. Hãy khôi phục trước khi lên lịch bảo trì.");

            var conflicts = await GetOverlappingActiveBookingsEntitiesAsync(
                courtId, request.StartDateTime, request.EndDateTime);

            if (conflicts.Count > 0 && !request.ConfirmRefund)
            {
                throw new InvalidOperationException(
                    $"Có {conflicts.Count} lịch đặt bị trùng khung bảo trì. Vui lòng xác nhận hoàn tiền (ConfirmRefund=true).");
            }

            var now = DateTime.UtcNow;
            var cancelled = 0;
            decimal totalRefunded = 0;

            if (conflicts.Count > 0)
            {
                var reason = string.IsNullOrWhiteSpace(request.Reason)
                    ? "Sân bảo trì theo lịch Admin."
                    : $"Sân bảo trì: {request.Reason.Trim()}";

                foreach (var booking in conflicts)
                {
                    var refunded = await CancelBookingWithRefundAsync(
                        booking, reason, $"Hoàn tiền do sân bảo trì (Booking #{booking.BookingId})", now);
                    cancelled++;
                    totalRefunded += refunded;
                }
            }

            var isOngoing = request.StartDateTime <= now && now < request.EndDateTime;
            var schedule = new MaintenanceSchedule
            {
                CourtId = courtId,
                MaintenanceType = MaintenanceType.Routine,
                StartDateTime = request.StartDateTime,
                EndDateTime = request.EndDateTime,
                Reason = request.Reason?.Trim(),
                Status = isOngoing ? MaintenanceStatus.InProgress : MaintenanceStatus.Scheduled
            };
            _db.MaintenanceSchedules.Add(schedule);

            if (isOngoing || conflicts.Count > 0)
            {
                court.Status = CourtStatus.Maintenance;
                await _courtRepo.UpdateAsync(court);
            }

            await _db.SaveChangesAsync();

            return new CourtLifecycleResultDto
            {
                CourtId = court.CourtId,
                CourtName = court.CourtName,
                Status = court.Status.ToString(),
                Message = conflicts.Count > 0
                    ? $"Đã lên lịch bảo trì. Đã hủy {cancelled} lịch đặt và hoàn {totalRefunded:N0}đ."
                    : "Đã lên lịch bảo trì. Không có lịch đặt bị ảnh hưởng.",
                CancelledBookings = cancelled,
                TotalRefunded = totalRefunded
            };
        }

        /// <summary>Giữ tương thích: chuyển sang Ngưng hoạt động thay vì soft-delete.</summary>
        public Task DeleteAsync(int id) => DeactivateAsync(id);

        private static void ValidateMaintenanceWindow(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new ArgumentException("Thời gian kết thúc bảo trì phải sau thời gian bắt đầu.");
            if ((end - start).TotalDays > 30)
                throw new ArgumentException("Khung bảo trì tối đa 30 ngày.");
        }

        private async Task<List<MaintenanceConflictBookingDto>> GetOverlappingActiveBookingsAsync(
            int courtId, DateTime start, DateTime end)
        {
            var bookings = await GetOverlappingActiveBookingsEntitiesAsync(courtId, start, end);
            return bookings.Select(b => new MaintenanceConflictBookingDto
            {
                BookingId = b.BookingId,
                BookingCode = b.BookingCode,
                CustomerName = b.User?.FullName ?? $"User #{b.UserId}",
                BookingDate = b.BookingDate,
                StartTime = b.StartTime.ToString(@"hh\:mm"),
                EndTime = b.EndTime.ToString(@"hh\:mm"),
                RefundAmount = GetRefundableAmount(b),
                Status = b.Status.ToString()
            }).ToList();
        }

        private async Task<List<Booking>> GetOverlappingActiveBookingsEntitiesAsync(
            int courtId, DateTime start, DateTime end)
        {
            // Lấy booking active trong khoảng ngày rồi lọc overlap theo giờ ở memory
            // (StartTime/EndTime là TimeSpan, khó so sánh trực tiếp với DateTime trên SQL mọi provider)
            var fromDate = start.Date;
            var toDate = end.Date;

            var candidates = await _db.Bookings
                .Include(b => b.Payment)
                .Include(b => b.User)
                .Where(b => b.CourtId == courtId
                    && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                    && b.BookingDate.Date >= fromDate
                    && b.BookingDate.Date <= toDate)
                .ToListAsync();

            return candidates.Where(b =>
            {
                var bookingStart = b.BookingDate.Date.Add(b.StartTime);
                var bookingEnd = b.BookingDate.Date.Add(b.EndTime);
                return bookingStart < end && bookingEnd > start;
            }).ToList();
        }

        /// <summary>
        /// Số tiền có thể hoàn: ưu tiên Payment.Success; fallback TotalAmount
        /// (đặt sân ví cũ có thể không có bản ghi Payment).
        /// </summary>
        private static decimal GetRefundableAmount(Booking booking)
        {
            if (booking.Payment != null)
            {
                if (booking.Payment.Status == PaymentStatus.Success)
                    return booking.Payment.Amount;
                if (booking.Payment.Status == PaymentStatus.PartialRefund)
                    return Math.Max(0, booking.Payment.Amount - booking.Payment.RefundAmount);
                // Refunded / Failed / Pending → không hoàn thêm từ Payment
                if (booking.Payment.Status == PaymentStatus.Refunded)
                    return 0;
            }

            // Confirmed đã trừ ví nhưng thiếu Payment row → dùng TotalAmount
            if (booking.Status == BookingStatus.Confirmed && booking.TotalAmount > 0)
                return booking.TotalAmount;

            return 0;
        }

        private async Task<decimal> CancelBookingWithRefundAsync(
            Booking booking, string cancelReason, string refundDescription, DateTime now)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancelReason = cancelReason;

            var refunded = GetRefundableAmount(booking);
            if (refunded > 0)
            {
                var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == booking.UserId);
                if (wallet == null)
                {
                    wallet = new Wallet { UserId = booking.UserId, Balance = 0, CreatedAt = now, UpdatedAt = now };
                    _db.Wallets.Add(wallet);
                    await _db.SaveChangesAsync();
                }

                wallet.Balance += refunded;
                wallet.UpdatedAt = now;

                _db.WalletTransactions.Add(new WalletTransaction
                {
                    WalletId = wallet.WalletId,
                    Amount = refunded,
                    Type = WalletTransactionType.Refund,
                    BookingId = booking.BookingId,
                    Description = refundDescription,
                    CreatedAt = now
                });

                if (booking.Payment != null)
                {
                    booking.Payment.Status = PaymentStatus.Refunded;
                    booking.Payment.RefundAmount = refunded;
                }
                else
                {
                    // Ghi nhận Payment refund để đồng bộ audit (booking cũ thiếu Payment)
                    _db.Payments.Add(new Payment
                    {
                        BookingId = booking.BookingId,
                        Amount = refunded,
                        PaymentMethod = PaymentMethod.Wallet,
                        TransactionId = $"RF-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                        Status = PaymentStatus.Refunded,
                        RefundAmount = refunded,
                        PaidAt = now
                    });
                }
            }

            _db.Notifications.Add(new Notification
            {
                UserId = booking.UserId,
                Title = $"Booking #{booking.BookingId} đã bị hủy" +
                        (refunded > 0 ? $". Đã hoàn {refunded:N0}đ vào ví." : "."),
                Type = NotificationType.BookingCancel,
                IsRead = false,
                CreatedAt = now
            });

            return refunded;
        }

        public async Task<bool> ExistsByCodeAsync(string courtCode, int? excludeCourtId = null)
        {
            return await _courtRepo.ExistsByCodeAsync(courtCode, excludeCourtId);
        }

        private static CourtDto MapToDto(Court c) => new()
        {
            CourtId = c.CourtId,
            CourtName = c.CourtName,
            CourtCode = c.CourtCode,
            CourtTypeId = c.CourtTypeId,
            CourtTypeName = c.CourtType?.TypeName ?? "",
            ComplexId = c.ComplexId,
            ComplexName = c.Complex?.ComplexName,
            Status = c.Status.ToString(),
            OpenTime = c.OpenTime.ToString(@"hh\:mm"),
            CloseTime = c.CloseTime.ToString(@"hh\:mm"),
            PricePerHour = c.PricePerHour,
            CourtSize = c.CourtSize,
            ImageUrl = c.CourtImages.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.CourtImageId).Select(i => i.ImageUrl).FirstOrDefault(),
            ImageUrls = c.CourtImages.Select(i => i.ImageUrl).ToList(),
            Pricings = c.CourtPricings?.Select(cp => new CourtPricingInputDto
            {
                SlotId = cp.SlotId,
                SlotName = cp.TimeSlot?.SlotName,
                StartTime = cp.TimeSlot?.StartTime.ToString(@"hh\:mm"),
                EndTime = cp.TimeSlot?.EndTime.ToString(@"hh\:mm"),
                Price = cp.Price
            }).ToList()
        };
    }
}
