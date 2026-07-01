using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.Services.Interfaces;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Services.Implements
{
    public class StaffShiftService : IStaffShiftService
    {
        private readonly IStaffShiftRepository _staffShiftRepository;
        private readonly AppDbContext _context;

        public StaffShiftService(IStaffShiftRepository staffShiftRepository, AppDbContext context)
        {
            _staffShiftRepository = staffShiftRepository;
            _context = context;
        }

        public IEnumerable<StaffShift> GetTodayShifts()
        {
            var today = DateTime.Today;
            return _context.StaffShifts
                .Include(s => s.Staff)
                .Where(s => s.ShiftDate.Date == today)
                .ToList();
        }

        public StaffShift? CheckIn(int shiftId, string photoBase64)
        {
            var shift = _staffShiftRepository.GetById(shiftId);
            if (shift == null) return null;

            shift.CheckInTime = DateTime.Now;
            _staffShiftRepository.Update(shift);

            SavePhoto(shiftId, photoBase64, "checkin");

            return shift;
        }

        public StaffShift? CheckOut(int shiftId, string photoBase64)
        {
            var shift = _staffShiftRepository.GetById(shiftId);
            if (shift == null) return null;

            shift.CheckOutTime = DateTime.Now;
            _staffShiftRepository.Update(shift);

            SavePhoto(shiftId, photoBase64, "checkout");

            return shift;
        }

        private void SavePhoto(int shiftId, string photoBase64, string prefix)
        {
            if (string.IsNullOrEmpty(photoBase64)) return;

            try
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "checkins");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var base64Data = photoBase64;
                if (photoBase64.Contains(","))
                {
                    base64Data = photoBase64.Split(',')[1];
                }

                var imageBytes = Convert.FromBase64String(base64Data);
                var fileName = $"shift_{shiftId}_{prefix}.jpg";
                var filePath = Path.Combine(folderPath, fileName);
                File.WriteAllBytes(filePath, imageBytes);
            }
            catch (Exception)
            {
                // Ignore or log error
            }
        }

        public void SeedDemoData()
        {
            // 1. Ensure Roles
            var staffRole = _context.Roles.FirstOrDefault(r => r.RoleName == "Staff");
            if (staffRole == null)
            {
                staffRole = new Role { RoleName = "Staff", Description = "Staff Member" };
                _context.Roles.Add(staffRole);
                _context.SaveChanges();
            }

            // 2. Ensure Users (Staff)
            var staff1 = _context.Users.FirstOrDefault(u => u.Email == "staff1@sportcourt.com");
            if (staff1 == null)
            {
                staff1 = new User
                {
                    FullName = "Nguyễn Văn A",
                    Email = "staff1@sportcourt.com",
                    Phone = "0987654321",
                    PasswordHash = "hashedpassword",
                    IsActive = true,
                    Gender = Gender.Male,
                    SkillLevel = SkillLevel.Intermediate,
                    CreatedAt = DateTime.Now
                };
                _context.Users.Add(staff1);
                _context.SaveChanges();

                _context.UserRoles.Add(new UserRole { UserId = staff1.UserId, RoleId = staffRole.RoleId });
                _context.SaveChanges();
            }

            var staff2 = _context.Users.FirstOrDefault(u => u.Email == "staff2@sportcourt.com");
            if (staff2 == null)
            {
                staff2 = new User
                {
                    FullName = "Trần Thị B",
                    Email = "staff2@sportcourt.com",
                    Phone = "0912345678",
                    PasswordHash = "hashedpassword",
                    IsActive = true,
                    Gender = Gender.Female,
                    SkillLevel = SkillLevel.Beginner,
                    CreatedAt = DateTime.Now
                };
                _context.Users.Add(staff2);
                _context.SaveChanges();

                _context.UserRoles.Add(new UserRole { UserId = staff2.UserId, RoleId = staffRole.RoleId });
                _context.SaveChanges();
            }

            // 3. Ensure Shifts for Today
            var today = DateTime.Today;
            var existingShifts = _context.StaffShifts.Where(s => s.ShiftDate.Date == today).ToList();
            if (!existingShifts.Any())
            {
                var shift1 = new StaffShift
                {
                    StaffId = staff1.UserId,
                    ShiftDate = today,
                    ShiftType = ShiftType.Morning,
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(12, 0, 0),
                    CheckInTime = null,
                    CheckOutTime = null
                };

                var shift2 = new StaffShift
                {
                    StaffId = staff2.UserId,
                    ShiftDate = today,
                    ShiftType = ShiftType.Afternoon,
                    StartTime = new TimeSpan(13, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    CheckInTime = null,
                    CheckOutTime = null
                };

                var shift3 = new StaffShift
                {
                    StaffId = staff1.UserId,
                    ShiftDate = today,
                    ShiftType = ShiftType.Evening,
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0),
                    CheckInTime = null,
                    CheckOutTime = null
                };

                _context.StaffShifts.AddRange(shift1, shift2, shift3);
                _context.SaveChanges();
            }
        }
    }
}
