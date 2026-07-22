using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DTOs.Maintenance;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportCourtManagent_Server.Services.Implements
{
    public class MaintenanceScheduleService : IMaintenanceScheduleService
    {
        private readonly IMaintenanceScheduleRepository _maintenanceRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly ICourtComplexRepository _complexRepository;
        private readonly IStaffRepository _staffRepository;

        public MaintenanceScheduleService(
            IMaintenanceScheduleRepository maintenanceRepository,
            ICourtRepository courtRepository,
            ICourtComplexRepository complexRepository,
            IStaffRepository staffRepository)
        {
            _maintenanceRepository = maintenanceRepository;
            _courtRepository = courtRepository;
            _complexRepository = complexRepository;
            _staffRepository = staffRepository;
        }

        public async Task<MaintenanceResponse> CreateMaintenanceAsync(int complexId, CreateMaintenanceRequest request)
        {
            if (!request.CourtId.HasValue || !request.MaintenanceType.HasValue || !request.StartDateTime.HasValue || !request.EndDateTime.HasValue)
            {
                throw new ArgumentException("Thông tin bảo trì không hợp lệ (thiếu CourtId, MaintenanceType, StartDateTime hoặc EndDateTime).");
            }

            if (request.EndDateTime <= request.StartDateTime)
            {
                throw new ArgumentException("Thời gian kết thúc phải sau thời gian bắt đầu.");
            }

            var complex = await _complexRepository.GetByIdWithDetailsAsync(complexId);
            if (complex == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy cơ sở với Id {complexId}");
            }

            var court = _courtRepository.GetById(request.CourtId.Value);
            if (court == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy sân với Id {request.CourtId.Value}");
            }

            if (court.ComplexId != complexId)
            {
                throw new ArgumentException("Sân được chọn không thuộc cơ sở này.");
            }

            if (!request.AssignedStaffId.HasValue || request.AssignedStaffId.Value <= 0)
            {
                throw new ArgumentException("Vui lòng chọn nhân viên phụ trách.");
            }

            var staff = await _staffRepository.GetStaffWithRolesAsync(request.AssignedStaffId.Value);
            if (staff == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy nhân viên với Id {request.AssignedStaffId.Value}");
            }
            if (!staff.IsActive)
            {
                throw new InvalidOperationException($"Nhân viên {staff.FullName} đang bị khóa/ngưng hoạt động.");
            }


            var schedule = new MaintenanceSchedule
            {
                CourtId = request.CourtId.Value,
                MaintenanceType = request.MaintenanceType.Value,
                StartDateTime = request.StartDateTime.Value,
                EndDateTime = request.EndDateTime.Value,
                AssignedStaffId = request.AssignedStaffId,
                Reason = request.Reason,
                Status = request.Status,
                Result = null
            };

            if (schedule.Status == MaintenanceStatus.InProgress)
            {
                court.Status = CourtStatus.Maintenance;
                _courtRepository.Update(court);
            }

            var created = await _maintenanceRepository.CreateAsync(schedule);
            return MapToResponse(created, court, complex, staff);
        }

        public async Task<MaintenanceResponse> UpdateMaintenanceAsync(int complexId, int maintenanceId, UpdateMaintenanceRequest request)
        {
            if (!request.CourtId.HasValue || !request.MaintenanceType.HasValue || !request.StartDateTime.HasValue || !request.EndDateTime.HasValue || !request.Status.HasValue)
            {
                throw new ArgumentException("Thông tin cập nhật bảo trì không hợp lệ (thiếu CourtId, MaintenanceType, StartDateTime, EndDateTime hoặc Status).");
            }

            if (request.EndDateTime <= request.StartDateTime)
            {
                throw new ArgumentException("Thời gian kết thúc phải sau thời gian bắt đầu.");
            }

            var schedule = await _maintenanceRepository.GetByIdAsync(maintenanceId);
            if (schedule == null || schedule.Court.ComplexId != complexId)
            {
                throw new KeyNotFoundException($"Không tìm thấy lịch bảo trì với Id {maintenanceId} tại cơ sở này.");
            }

            if (schedule.Status == MaintenanceStatus.Completed)
            {
                throw new ArgumentException("Lịch bảo trì đã hoàn thành, không thể chỉnh sửa.");
            }

            var court = _courtRepository.GetById(request.CourtId.Value);
            if (court == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy sân với Id {request.CourtId.Value}");
            }

            if (court.ComplexId != complexId)
            {
                throw new ArgumentException("Sân được chọn không thuộc cơ sở này.");
            }

            if (!request.AssignedStaffId.HasValue || request.AssignedStaffId.Value <= 0)
            {
                throw new ArgumentException("Vui lòng chọn nhân viên phụ trách.");
            }

            var staff = await _staffRepository.GetStaffWithRolesAsync(request.AssignedStaffId.Value);
            if (staff == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy nhân viên với Id {request.AssignedStaffId.Value}");
            }


            var oldStatus = schedule.Status;
            var oldCourtId = schedule.CourtId;

            schedule.CourtId = request.CourtId.Value;
            schedule.MaintenanceType = request.MaintenanceType.Value;
            schedule.StartDateTime = request.StartDateTime.Value;
            schedule.EndDateTime = request.EndDateTime.Value;
            schedule.AssignedStaffId = request.AssignedStaffId;
            schedule.Reason = request.Reason;
            schedule.Status = request.Status.Value;
            schedule.Result = request.Result;
            schedule.ImageProof = request.ImageProof;

            // Cập nhật trạng thái sân tương ứng
            if (schedule.Status == MaintenanceStatus.InProgress)
            {
                court.Status = CourtStatus.Maintenance;
                _courtRepository.Update(court);
            }
            else if (schedule.Status == MaintenanceStatus.Completed || schedule.Status == MaintenanceStatus.Cancelled)
            {
                if (court.Status == CourtStatus.Maintenance)
                {
                    court.Status = CourtStatus.Available;
                    _courtRepository.Update(court);
                }

                if (oldCourtId != court.CourtId)
                {
                    var oldCourt = _courtRepository.GetById(oldCourtId);
                    if (oldCourt != null && oldCourt.Status == CourtStatus.Maintenance)
                    {
                        oldCourt.Status = CourtStatus.Available;
                        _courtRepository.Update(oldCourt);
                    }
                }
            }

            await _maintenanceRepository.UpdateAsync(schedule);
            return MapToResponse(schedule, court, court.Complex, staff);
        }

        public async Task<MaintenanceResponse> VerifyMaintenanceAsync(int complexId, int maintenanceId, VerifyMaintenanceRequest request)
        {
            if (!request.IsApproved.HasValue)
            {
                throw new ArgumentException("Thông tin xác nhận duyệt không hợp lệ.");
            }

            var schedule = await _maintenanceRepository.GetByIdAsync(maintenanceId);
            if (schedule == null || schedule.Court.ComplexId != complexId)
            {
                throw new KeyNotFoundException($"Không tìm thấy lịch bảo trì với Id {maintenanceId} tại cơ sở này.");
            }

            if (request.IsApproved.Value)
            {
                schedule.Status = MaintenanceStatus.Completed;
                schedule.Result = string.IsNullOrWhiteSpace(request.Note) ? "Đã hoàn thành bảo trì (Duyệt bởi Manager)" : request.Note;

                var court = _courtRepository.GetById(schedule.CourtId);
                if (court != null && court.Status == CourtStatus.Maintenance)
                {
                    court.Status = CourtStatus.Available;
                    _courtRepository.Update(court);
                }
            }
            else
            {
                schedule.Status = MaintenanceStatus.Cancelled;
                schedule.Result = string.IsNullOrWhiteSpace(request.Note) ? "Bị từ chối bảo trì bởi Manager" : $"Từ chối: {request.Note}";

                var court = _courtRepository.GetById(schedule.CourtId);
                if (court != null && court.Status == CourtStatus.Maintenance)
                {
                    court.Status = CourtStatus.Available;
                    _courtRepository.Update(court);
                }
            }

            await _maintenanceRepository.UpdateAsync(schedule);
            return MapToResponse(schedule);
        }

        public async Task<PagedMaintenanceResponse> GetMaintenanceListAsync(int complexId, MaintenanceStatus? status = null, int page = 1, int pageSize = 10)
        {
            var (items, totalCount) = await _maintenanceRepository.GetByComplexAsync(complexId, status, page, pageSize);

            return new PagedMaintenanceResponse
            {
                Items = items.Select(ms => MapToResponse(ms)).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<MaintenanceResponse> GetMaintenanceByIdAsync(int maintenanceId)
        {
            var schedule = await _maintenanceRepository.GetByIdAsync(maintenanceId);
            if (schedule == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy lịch bảo trì với Id {maintenanceId}");
            }
            return MapToResponse(schedule);
        }

        public async Task DeleteMaintenanceAsync(int complexId, int maintenanceId)
        {
            var schedule = await _maintenanceRepository.GetByIdAsync(maintenanceId);
            if (schedule == null || schedule.Court.ComplexId != complexId)
            {
                throw new KeyNotFoundException($"Không tìm thấy lịch bảo trì với Id {maintenanceId} tại cơ sở này.");
            }

            if (schedule.Status == MaintenanceStatus.Completed)
            {
                throw new ArgumentException("Lịch bảo trì đã hoàn thành, không thể xóa.");
            }

            var court = _courtRepository.GetById(schedule.CourtId);
            if (court != null && court.Status == CourtStatus.Maintenance)
            {
                court.Status = CourtStatus.Available;
                _courtRepository.Update(court);
            }

            await _maintenanceRepository.DeleteAsync(schedule);
        }

        public async Task<IEnumerable<MaintenanceCourtResponse>> GetCourtsForMaintenanceAsync(int complexId)
        {
            var complex = await _complexRepository.GetByIdWithDetailsAsync(complexId);
            if (complex == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy cơ sở với Id {complexId}");
            }

            var courts = await _courtRepository.GetCourtsByComplexAsync(complexId);
            return courts
                .Where(c => c.Status == CourtStatus.Available)
                .Select(c => new MaintenanceCourtResponse
                {
                    CourtId = c.CourtId,
                    CourtName = c.CourtName,
                    CourtCode = c.CourtCode,
                    CourtTypeName = c.CourtType?.TypeName ?? string.Empty,
                    Status = c.Status.ToString()
                });
        }

        private MaintenanceResponse MapToResponse(MaintenanceSchedule schedule, Court? court = null, CourtComplex? complex = null, User? staff = null)
        {
            var courtEntity = court ?? schedule.Court;
            var complexEntity = complex ?? (courtEntity != null ? courtEntity.Complex : null);
            var staffEntity = staff ?? schedule.AssignedStaff;

            return new MaintenanceResponse
            {
                MaintenanceId = schedule.MaintenanceId,
                CourtId = schedule.CourtId,
                CourtName = courtEntity?.CourtName ?? string.Empty,
                ComplexId = courtEntity?.ComplexId ?? 0,
                ComplexName = complexEntity?.ComplexName ?? string.Empty,
                MaintenanceType = schedule.MaintenanceType.ToString(),
                StartDateTime = schedule.StartDateTime,
                EndDateTime = schedule.EndDateTime,
                AssignedStaffId = schedule.AssignedStaffId,
                AssignedStaffName = staffEntity?.FullName,
                Reason = schedule.Reason,
                Result = schedule.Result,
                ImageProof = schedule.ImageProof,
                Status = schedule.Status.ToString(),
                CreatedAt = DateTime.Now
            };
        }
    }
}
