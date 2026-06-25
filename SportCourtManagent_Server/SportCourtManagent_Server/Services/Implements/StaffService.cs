using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Staff;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportCourtManagent_Server.Services.Implements
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IStaffShiftRepository _staffShiftRepository;
        private readonly ICourtComplexRepository _complexRepository;
        private readonly INotificationRepository _notificationRepository;

        private static readonly Dictionary<ShiftType, (TimeOnly Start, TimeOnly End)> ShiftTimes = new()
        {
            [ShiftType.Morning] = (new TimeOnly(6, 0), new TimeOnly(14, 0)),
            [ShiftType.Afternoon] = (new TimeOnly(14, 0), new TimeOnly(22, 0)),
            [ShiftType.Evening] = (new TimeOnly(22, 0), new TimeOnly(6, 0)) // qua đêm
        };

        public StaffService(
            IStaffRepository staffRepository,
            IStaffShiftRepository staffShiftRepository,
            ICourtComplexRepository complexRepository,
            INotificationRepository notificationRepository)
        {
            _staffRepository = staffRepository;
            _staffShiftRepository = staffShiftRepository;
            _complexRepository = complexRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<StaffShiftResponse> CheckInShiftAsync(int staffId, int shiftId)
        {
            var shift = await _staffShiftRepository.GetByIdAsync(shiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy ca trực với Id {shiftId}");
            }

            if (shift.StaffId != staffId)
            {
                throw new ArgumentException("Ca trực này không thuộc về nhân viên.");
            }

            if (shift.ShiftDate != DateOnly.FromDateTime(DateTime.Today))
            {
                throw new InvalidOperationException("Chỉ được phép chấm công ca trực của ngày hôm nay.");
            }

            if (shift.CheckInTime.HasValue)
            {
                throw new InvalidOperationException("Ca trực này đã được chấm công.");
            }

            shift.CheckInTime = DateTime.Now;
            await _staffShiftRepository.UpdateAsync(shift);
            return MapToResponse(shift);
        }

        public async Task<StaffShiftResponse> CheckOutShiftAsync(int staffId, int shiftId)
        {
            var shift = await _staffShiftRepository.GetByIdAsync(shiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy ca trực với Id {shiftId}");
            }

            if (shift.StaffId != staffId)
            {
                throw new ArgumentException("Ca trực này không thuộc về nhân viên.");
            }

            if (!shift.CheckInTime.HasValue)
            {
                throw new InvalidOperationException("Nhân viên chưa thực hiện chấm công vào cho ca trực này.");
            }

            if (shift.CheckOutTime.HasValue)
            {
                throw new InvalidOperationException("Ca trực này đã được chấm công ra.");
            }

            shift.CheckOutTime = DateTime.Now;
            await _staffShiftRepository.UpdateAsync(shift);
            return MapToResponse(shift);
        }

        public async Task<StaffShiftResponse> CreateShiftAsync(int complexId, CreateShiftRequest request)
        {
            var complex = _complexRepository.GetById(complexId);
            if (complex == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy cơ sở với Id {complexId}");
            }

            var staffUser = await _staffRepository.GetStaffWithRolesAsync(request.StaffId);
            if (staffUser == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy nhân viên với Id {request.StaffId}");
            }

            if (!staffUser.IsActive)
            {
                throw new InvalidOperationException($"Nhân viên {staffUser.FullName} đang bị khóa/ngưng hoạt động.");
            }

            var exists = await _staffShiftRepository.ExistsAsync(request.StaffId, request.ShiftDate, request.ShiftType);
            if (exists)
            {
                throw new InvalidOperationException($"Nhân viên {staffUser.FullName} đã được xếp ca {request.ShiftType} ngày {request.ShiftDate} trước đó.");
            }

            var (startTime, endTime) = GetShiftTimes(request.ShiftType);
            var shift = new StaffShift
            {
                StaffId = request.StaffId,
                ComplexId = complexId,
                ShiftDate = request.ShiftDate,
                ShiftType = request.ShiftType,
                StartTime = startTime,
                EndTime = endTime
            };

            var createdShift = await _staffShiftRepository.CreateAsync(shift);
            return MapToResponse(createdShift, staffUser, complex);
        }

        public async Task<BulkCreateShiftResponse> CreateShiftBulkAsync(int complexId, BulkCreateShiftRequest request)
        {
            var complex = _complexRepository.GetById(complexId);
            if (complex == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy cơ sở với Id {complexId}");
            }

            var response = new BulkCreateShiftResponse();
            var shiftsToCreate = new List<StaffShift>();
            var validShiftsMap = new List<(StaffShift Shift, User Staff)>();

            foreach (var shiftReq in request.Shifts)
            {
                var staffUser = await _staffRepository.GetStaffWithRolesAsync(shiftReq.StaffId);
                if (staffUser == null)
                {
                    response.Skipped++;
                    response.Errors.Add($"Nhân viên với Id {shiftReq.StaffId} không tồn tại.");
                    continue;
                }

                if (!staffUser.IsActive)
                {
                    response.Skipped++;
                    response.Errors.Add($"Nhân viên {staffUser.FullName} đang bị khóa/ngưng hoạt động.");
                    continue;
                }

                var existsInDb = await _staffShiftRepository.ExistsAsync(shiftReq.StaffId, shiftReq.ShiftDate, shiftReq.ShiftType);
                if (existsInDb)
                {
                    response.Skipped++;
                    response.Errors.Add($"Nhân viên {staffUser.FullName} đã được xếp ca {shiftReq.ShiftType} ngày {shiftReq.ShiftDate} trước đó.");
                    continue;
                }

                var existsInBatch = shiftsToCreate.Any(s => s.StaffId == shiftReq.StaffId && s.ShiftDate == shiftReq.ShiftDate && s.ShiftType == shiftReq.ShiftType);
                if (existsInBatch)
                {
                    response.Skipped++;
                    response.Errors.Add($"Nhân viên {staffUser.FullName} có ca {shiftReq.ShiftType} ngày {shiftReq.ShiftDate} bị trùng lặp trong danh sách gửi lên.");
                    continue;
                }

                var (startTime, endTime) = GetShiftTimes(shiftReq.ShiftType);
                var shift = new StaffShift
                {
                    StaffId = shiftReq.StaffId,
                    ComplexId = complexId,
                    ShiftDate = shiftReq.ShiftDate,
                    ShiftType = shiftReq.ShiftType,
                    StartTime = startTime,
                    EndTime = endTime
                };

                shiftsToCreate.Add(shift);
                validShiftsMap.Add((shift, staffUser));
            }

            if (shiftsToCreate.Any())
            {
                var createdShifts = await _staffShiftRepository.CreateBulkAsync(shiftsToCreate);
                response.Created = createdShifts.Count;

                foreach (var shift in createdShifts)
                {
                    var staffUser = validShiftsMap.First(x => x.Shift == shift).Staff;
                    response.CreatedShifts.Add(MapToResponse(shift, staffUser, complex));
                }
            }

            return response;
        }

        public async Task DeleteShiftAsync(int complexId, int shiftId)
        {
            var shift = await _staffShiftRepository.GetByIdAsync(shiftId);
            if (shift == null || shift.ComplexId != complexId)
            {
                throw new KeyNotFoundException($"Không tìm thấy ca trực với Id {shiftId} tại cơ sở này.");
            }

            if (shift.CheckInTime.HasValue)
            {
                throw new InvalidOperationException("Không thể xóa ca làm việc đã chấm công.");
            }

            await _staffShiftRepository.DeleteAsync(shift);
        }

        public async Task<List<StaffShiftResponse>> GetAttendanceReportAsync(int complexId, DateOnly? dateFrom, DateOnly? dateTo, int? staffId = null)
        {
            var from = dateFrom ?? new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
            var to = dateTo ?? DateOnly.FromDateTime(DateTime.Today);

            var shifts = await _staffShiftRepository.GetShiftsByComplexAndDateRangeAsync(complexId, from, to);
            if (staffId.HasValue)
            {
                shifts = shifts.Where(s => s.StaffId == staffId.Value).ToList();
            }

            return shifts
                .OrderByDescending(s => s.ShiftDate)
                .ThenBy(s => s.StartTime)
                .Select(s => MapToResponse(s))
                .ToList();
        }

        public async Task<StaffShiftResponse> GetShiftByIdAsync(int shiftId)
        {
            var shift = await _staffShiftRepository.GetByIdAsync(shiftId);
            if (shift == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy ca trực với Id {shiftId}");
            }

            return MapToResponse(shift);
        }

        public async Task<PagedStaffResponse> GetStaffListAsync(int complexId, string? search = null, bool? isActive = null, int page = 1, int pageSize = 10)
        {
            var (staffList, totalCount) = await _staffRepository.GetStaffByComplexAsync(complexId, search, isActive, page, pageSize);

            var items = new List<StaffSummaryResponse>();
            foreach (var staff in staffList)
            {
                var shiftsThisWeek = await _staffShiftRepository.CountShiftsThisWeekAsync(staff.UserId);
                var todayShift = await _staffShiftRepository.GetTodayShiftAsync(staff.UserId);

                items.Add(new StaffSummaryResponse
                {
                    UserId = staff.UserId,
                    FullName = staff.FullName,
                    Email = staff.Email,
                    Phone = staff.Phone,
                    AvatarUrl = staff.AvatarUrl,
                    IsActive = staff.IsActive,
                    ShiftsThisWeek = shiftsThisWeek,
                    TodayShift = todayShift != null ? MapToSummaryResponse(todayShift) : null
                });
            }

            return new PagedStaffResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<WeeklyScheduleResponse> GetWeeklyScheduleAsync(int complexId, DateOnly weekStart)
        {
            var weekEnd = weekStart.AddDays(6);
            var shifts = await _staffShiftRepository.GetShiftsByComplexAndDateRangeAsync(complexId, weekStart, weekEnd);

            var days = new List<DailyShiftGroupResponse>();
            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);
                var dailyShifts = shifts
                    .Where(s => s.ShiftDate == date)
                    .OrderBy(s => s.StartTime)
                    .Select(s => MapToResponse(s))
                    .ToList();

                days.Add(new DailyShiftGroupResponse
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    DayName = GetVietnameseDayName(date.DayOfWeek),
                    Shifts = dailyShifts
                });
            }

            return new WeeklyScheduleResponse
            {
                WeekStart = weekStart.ToString("yyyy-MM-dd"),
                WeekEnd = weekEnd.ToString("yyyy-MM-dd"),
                Days = days
            };
        }

        public async Task<StaffShiftResponse> UpdateShiftAsync(int complexId, int shiftId, UpdateShiftRequest request)
        {
            var complex = _complexRepository.GetById(complexId);
            if (complex == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy cơ sở với Id {complexId}");
            }

            var shift = await _staffShiftRepository.GetByIdAsync(shiftId);
            if (shift == null || shift.ComplexId != complexId)
            {
                throw new KeyNotFoundException($"Không tìm thấy ca trực với Id {shiftId} tại cơ sở này.");
            }

            if (shift.CheckInTime.HasValue)
            {
                throw new InvalidOperationException("Không thể sửa ca làm việc đã chấm công.");
            }

            if (request.ShiftType != shift.ShiftType)
            {
                var exists = await _staffShiftRepository.ExistsAsync(shift.StaffId, shift.ShiftDate, request.ShiftType);
                if (exists)
                {
                    throw new InvalidOperationException($"Nhân viên đã được xếp ca {request.ShiftType} ngày {shift.ShiftDate} trước đó.");
                }

                shift.ShiftType = request.ShiftType;
                var (startTime, endTime) = GetShiftTimes(request.ShiftType);
                shift.StartTime = startTime;
                shift.EndTime = endTime;
            }

            await _staffShiftRepository.UpdateAsync(shift);
            return MapToResponse(shift, shift.Staff, complex);
        }

        private (TimeOnly StartTime, TimeOnly EndTime) GetShiftTimes(ShiftType shiftType)
        {
            if (ShiftTimes.TryGetValue(shiftType, out var times))
            {
                return times;
            }
            return (new TimeOnly(6, 0), new TimeOnly(14, 0));
        }

        private string GetVietnameseDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ Hai",
                DayOfWeek.Tuesday => "Thứ Ba",
                DayOfWeek.Wednesday => "Thứ Tư",
                DayOfWeek.Thursday => "Thứ Năm",
                DayOfWeek.Friday => "Thứ Sáu",
                DayOfWeek.Saturday => "Thứ Bảy",
                DayOfWeek.Sunday => "Chủ Nhật",
                _ => dayOfWeek.ToString()
            };
        }

        private StaffShiftResponse MapToResponse(StaffShift shift, User? staff = null, CourtComplex? complex = null)
        {
            var staffUser = staff ?? shift.Staff;
            var complexEntity = complex ?? shift.Complex;

            var expectedStart = shift.ShiftDate.ToDateTime(shift.StartTime);
            var expectedEnd = shift.ShiftDate.ToDateTime(shift.EndTime);

            // Xử lý ca đêm (Evening shift): Giờ kết thúc nhỏ hơn giờ bắt đầu
            if (shift.EndTime < shift.StartTime)
            {
                expectedEnd = expectedEnd.AddDays(1);
            }

            int lateMinutes = 0;
            if (shift.CheckInTime.HasValue)
            {
                var checkIn = shift.CheckInTime.Value;
                if (checkIn > expectedStart)
                {
                    lateMinutes = (int)Math.Max(0, (checkIn - expectedStart).TotalMinutes);
                }
            }

            int earlyLeaveMinutes = 0;
            if (shift.CheckOutTime.HasValue)
            {
                var checkOut = shift.CheckOutTime.Value;
                if (checkOut < expectedEnd)
                {
                    earlyLeaveMinutes = (int)Math.Max(0, (expectedEnd - checkOut).TotalMinutes);
                }
            }

            return new StaffShiftResponse
            {
                ShiftId = shift.ShiftId,
                StaffId = shift.StaffId,
                StaffName = staffUser?.FullName ?? string.Empty,
                StaffEmail = staffUser?.Email ?? string.Empty,
                AvatarUrl = staffUser?.AvatarUrl,
                ShiftDate = shift.ShiftDate.ToString("yyyy-MM-dd"),
                ShiftType = shift.ShiftType.ToString(),
                StartTime = shift.StartTime.ToString("HH:mm"),
                EndTime = shift.EndTime.ToString("HH:mm"),
                CheckInTime = shift.CheckInTime,
                CheckOutTime = shift.CheckOutTime,
                ComplexId = shift.ComplexId,
                ComplexName = complexEntity?.ComplexName ?? string.Empty,
                Note = null,
                CreatedAt = shift.ShiftDate.ToDateTime(TimeOnly.MinValue),
                LateMinutes = lateMinutes,
                EarlyLeaveMinutes = earlyLeaveMinutes
            };
        }

        private ShiftSummaryResponse MapToSummaryResponse(StaffShift shift, CourtComplex? complex = null)
        {
            var complexEntity = complex ?? shift.Complex;

            var expectedStart = shift.ShiftDate.ToDateTime(shift.StartTime);
            var expectedEnd = shift.ShiftDate.ToDateTime(shift.EndTime);

            // Xử lý ca đêm (Evening shift): Giờ kết thúc nhỏ hơn giờ bắt đầu
            if (shift.EndTime < shift.StartTime)
            {
                expectedEnd = expectedEnd.AddDays(1);
            }

            int lateMinutes = 0;
            if (shift.CheckInTime.HasValue)
            {
                var checkIn = shift.CheckInTime.Value;
                if (checkIn > expectedStart)
                {
                    lateMinutes = (int)Math.Max(0, (checkIn - expectedStart).TotalMinutes);
                }
            }

            int earlyLeaveMinutes = 0;
            if (shift.CheckOutTime.HasValue)
            {
                var checkOut = shift.CheckOutTime.Value;
                if (checkOut < expectedEnd)
                {
                    earlyLeaveMinutes = (int)Math.Max(0, (expectedEnd - checkOut).TotalMinutes);
                }
            }

            return new ShiftSummaryResponse
            {
                ShiftId = shift.ShiftId,
                ShiftType = shift.ShiftType.ToString(),
                StartTime = shift.StartTime.ToString("HH:mm"),
                EndTime = shift.EndTime.ToString("HH:mm"),
                CheckInTime = shift.CheckInTime,
                CheckOutTime = shift.CheckOutTime,
                ComplexId = shift.ComplexId,
                ComplexName = complexEntity?.ComplexName ?? string.Empty,
                LateMinutes = lateMinutes,
                EarlyLeaveMinutes = earlyLeaveMinutes
            };
        }
    }
}
