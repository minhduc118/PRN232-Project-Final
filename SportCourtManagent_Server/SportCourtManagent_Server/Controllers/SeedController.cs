using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Enums;
using SportCourtManagent_Server.Models;

namespace SportCourtManagent_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeedController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("sample-data")]
        public async Task<IActionResult> SeedSampleData()
        {
            try
            {
                // 1. Seed Roles & Default Users if not exist
                var defaultUser = await _context.Users.FirstOrDefaultAsync();
                if (defaultUser == null)
                {
                    var tier = await _context.MembershipTiers.FirstOrDefaultAsync();
                    if (tier == null)
                    {
                        tier = new MembershipTier { TierName = "Standard", DiscountPercent = 0, MinPoints = 0 };
                        _context.MembershipTiers.Add(tier);
                        await _context.SaveChangesAsync();
                    }

                    defaultUser = new User
                    {
                        FullName = "Admin Sport Court",
                        Email = "admin@sportcourt.com",
                        PasswordHash = "$2a$11$somerandomhashedpasswordhere",
                        Phone = "0901234567",
                        IsActive = true,
                        MembershipTierId = tier.TierId
                    };
                    _context.Users.Add(defaultUser);
                    await _context.SaveChangesAsync();
                }

                // 2. Normalize & Seed CourtType
                var legacyMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Cầu lông", "Sân Cầu Lông" },
                    { "Bóng rổ", "Sân Bóng Rổ" },
                    { "Tennis", "Sân Tennis" },
                    { "Pickleball", "Sân Pickleball" },
                    { "Bóng đá", "Sân Bóng Đá Mini" }
                };
                var existingTypes = await _context.CourtTypes.ToListAsync();
                foreach (var legacy in legacyMappings)
                {
                    var oldType = existingTypes.FirstOrDefault(t => t.TypeName.Equals(legacy.Key, StringComparison.OrdinalIgnoreCase) && t.TypeName != legacy.Value);
                    if (oldType != null)
                    {
                        var targetType = existingTypes.FirstOrDefault(t => t.TypeName == legacy.Value);
                        if (targetType != null)
                        {
                            var oldCourts = await _context.Courts.Where(c => c.CourtTypeId == oldType.CourtTypeId).ToListAsync();
                            foreach (var c in oldCourts) c.CourtTypeId = targetType.CourtTypeId;
                            _context.CourtTypes.Remove(oldType);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            oldType.TypeName = legacy.Value;
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                var typeNames = new[] { "Sân Cầu Lông", "Sân Bóng Rổ", "Sân Tennis", "Sân Pickleball", "Sân Bóng Đá Mini" };
                foreach (var tName in typeNames)
                {
                    if (!await _context.CourtTypes.AnyAsync(ct => ct.TypeName == tName))
                    {
                        _context.CourtTypes.Add(new CourtType { TypeName = tName, IsActive = true });
                    }
                }
                await _context.SaveChangesAsync();

                var courtTypes = await _context.CourtTypes.ToListAsync();
                var badminType = courtTypes.FirstOrDefault(t => t.TypeName.Contains("Cầu Lông")) ?? courtTypes.First();
                var basketType = courtTypes.FirstOrDefault(t => t.TypeName.Contains("Bóng Rổ")) ?? courtTypes.First();
                var tennisType = courtTypes.FirstOrDefault(t => t.TypeName.Contains("Tennis")) ?? courtTypes.First();
                var pickleType = courtTypes.FirstOrDefault(t => t.TypeName.Contains("Pickleball")) ?? courtTypes.First();
                var soccerType = courtTypes.FirstOrDefault(t => t.TypeName.Contains("Bóng Đá")) ?? courtTypes.First();

                // 3. Seed CourtComplex
                var complex = await _context.CourtComplexes.FirstOrDefaultAsync();
                if (complex == null)
                {
                    complex = new CourtComplex
                    {
                        ComplexName = "Trung Tâm Thể Thao Olympic",
                        Address = "Số 1 Đường Lê Duẩn, Quận 1, TP. Hồ Chí Minh",
                        ManagerId = defaultUser.UserId,
                        Description = "Khu thể thao phức hợp hiện đại bậc nhất thành phố"
                    };
                    _context.CourtComplexes.Add(complex);
                    await _context.SaveChangesAsync();
                }

                // 4. Seed TimeSlots
                if (!await _context.TimeSlots.AnyAsync())
                {
                    _context.TimeSlots.AddRange(
                        new TimeSlot { SlotName = "Ca Sáng 1 (07:00 - 09:00)", StartTime = new TimeSpan(7, 0, 0), EndTime = new TimeSpan(9, 0, 0), DayType = DayType.Weekday },
                        new TimeSlot { SlotName = "Ca Sáng 2 (09:00 - 11:00)", StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(11, 0, 0), DayType = DayType.Weekday },
                        new TimeSlot { SlotName = "Ca Chiều (14:00 - 17:00)", StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(17, 0, 0), DayType = DayType.Weekday },
                        new TimeSlot { SlotName = "Ca Tối Vàng (17:30 - 21:00)", StartTime = new TimeSpan(17, 30, 0), EndTime = new TimeSpan(21, 0, 0), DayType = DayType.Weekday }
                    );
                    await _context.SaveChangesAsync();
                }
                var slots = await _context.TimeSlots.ToListAsync();

                // 5. Seed Courts & CourtPricing
                if (!await _context.Courts.AnyAsync())
                {
                    var courts = new List<Court>
                    {
                        new Court { CourtName = "Sân Cầu Lông A1 (VIP)", CourtCode = "CL-A1", CourtTypeId = badminType.CourtTypeId, ComplexId = complex.ComplexId, PricePerHour = 120000m, OpenTime = new TimeSpan(6, 0, 0), CloseTime = new TimeSpan(22, 0, 0) },
                        new Court { CourtName = "Sân Cầu Lông A2", CourtCode = "CL-A2", CourtTypeId = badminType.CourtTypeId, ComplexId = complex.ComplexId, PricePerHour = 100000m, OpenTime = new TimeSpan(6, 0, 0), CloseTime = new TimeSpan(22, 0, 0) },
                        new Court { CourtName = "Sân Cầu Lông A3", CourtCode = "CL-A3", CourtTypeId = badminType.CourtTypeId, ComplexId = complex.ComplexId, PricePerHour = 100000m, OpenTime = new TimeSpan(6, 0, 0), CloseTime = new TimeSpan(22, 0, 0) },
                        new Court { CourtName = "Sân Bóng Rổ Trong Nhà B1", CourtCode = "BR-B1", CourtTypeId = basketType.CourtTypeId, ComplexId = complex.ComplexId, PricePerHour = 350000m, OpenTime = new TimeSpan(6, 0, 0), CloseTime = new TimeSpan(22, 0, 0) },
                        new Court { CourtName = "Sân Bóng Rổ Trong Nhà B2", CourtCode = "BR-B2", CourtTypeId = basketType.CourtTypeId, ComplexId = complex.ComplexId, PricePerHour = 350000m, OpenTime = new TimeSpan(6, 0, 0), CloseTime = new TimeSpan(22, 0, 0) },
                        new Court { CourtName = "Sân Tennis Trung Tâm T1", CourtCode = "TN-T1", CourtTypeId = tennisType.CourtTypeId, ComplexId = complex.ComplexId, PricePerHour = 250000m, OpenTime = new TimeSpan(6, 0, 0), CloseTime = new TimeSpan(22, 0, 0) },
                        new Court { CourtName = "Sân Pickleball P1", CourtCode = "PK-P1", CourtTypeId = pickleType.CourtTypeId, ComplexId = complex.ComplexId, PricePerHour = 150000m, OpenTime = new TimeSpan(6, 0, 0), CloseTime = new TimeSpan(22, 0, 0) },
                        new Court { CourtName = "Sân Bóng Đá Cỏ Nhân Tạo S1", CourtCode = "BD-S1", CourtTypeId = soccerType.CourtTypeId, ComplexId = complex.ComplexId, PricePerHour = 400000m, OpenTime = new TimeSpan(6, 0, 0), CloseTime = new TimeSpan(22, 0, 0) }
                    };
                    _context.Courts.AddRange(courts);
                    await _context.SaveChangesAsync();

                    // Create CourtPricings for all courts and slots
                    foreach (var c in courts)
                    {
                        foreach (var s in slots)
                        {
                            decimal multiplier = s.SlotName.Contains("Tối Vàng") ? 1.5m : 1.0m;
                            _context.CourtPricings.Add(new CourtPricing
                            {
                                CourtId = c.CourtId,
                                SlotId = s.SlotId,
                                Price = c.PricePerHour * 2 * multiplier, // price for slot duration
                                EffectiveFrom = DateTime.Today.AddMonths(-1)
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Ensure courts for all seeded types exist
                var missingCourts = new List<Court>();
                if (!await _context.Courts.AnyAsync(c => c.CourtCode == "TN-T1"))
                    missingCourts.Add(new Court { CourtName = "Sân Tennis Trung Tâm T1", CourtCode = "TN-T1", CourtTypeId = tennisType.CourtTypeId, ComplexId = complex.ComplexId, PricePerHour = 250000m, OpenTime = new TimeSpan(6, 0, 0), CloseTime = new TimeSpan(22, 0, 0) });
                if (!await _context.Courts.AnyAsync(c => c.CourtCode == "PK-P1"))
                    missingCourts.Add(new Court { CourtName = "Sân Pickleball P1", CourtCode = "PK-P1", CourtTypeId = pickleType.CourtTypeId, ComplexId = complex.ComplexId, PricePerHour = 150000m, OpenTime = new TimeSpan(6, 0, 0), CloseTime = new TimeSpan(22, 0, 0) });
                if (!await _context.Courts.AnyAsync(c => c.CourtCode == "BD-S1"))
                    missingCourts.Add(new Court { CourtName = "Sân Bóng Đá Cỏ Nhân Tạo S1", CourtCode = "BD-S1", CourtTypeId = soccerType.CourtTypeId, ComplexId = complex.ComplexId, PricePerHour = 400000m, OpenTime = new TimeSpan(6, 0, 0), CloseTime = new TimeSpan(22, 0, 0) });

                if (missingCourts.Count > 0)
                {
                    _context.Courts.AddRange(missingCourts);
                    await _context.SaveChangesAsync();
                    foreach (var c in missingCourts)
                    {
                        foreach (var s in slots)
                        {
                            decimal multiplier = s.SlotName.Contains("Tối Vàng") ? 1.5m : 1.0m;
                            _context.CourtPricings.Add(new CourtPricing
                            {
                                CourtId = c.CourtId,
                                SlotId = s.SlotId,
                                Price = c.PricePerHour * 2 * multiplier,
                                EffectiveFrom = DateTime.Today.AddMonths(-1)
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // 6. Seed Services (Add missing sport services)
                var desiredServices = new List<Service>
                {
                    // Cầu Lông
                    new Service { ServiceName = "Thuê Vợt Cầu Lông Yonex Pro", Category = "Thiết bị", Price = 50000, StockQty = 30 },
                    new Service { ServiceName = "Ống Cầu Lông Thành Công (12 quả)", Category = "Dụng cụ", Price = 240000, StockQty = 100 },
                    // Bóng Rổ
                    new Service { ServiceName = "Thuê Bóng Rổ Spalding NBA", Category = "Dụng cụ", Price = 80000, StockQty = 20 },
                    new Service { ServiceName = "Bảng điểm điện tử & Đồng hồ 24s", Category = "Thiết bị", Price = 300000, StockQty = 10 },
                    // Tennis
                    new Service { ServiceName = "Thuê Vợt Tennis Wilson Pro", Category = "Thiết bị", Price = 100000, StockQty = 15 },
                    new Service { ServiceName = "Hộp Bóng Tennis Dunlop (4 quả)", Category = "Dụng cụ", Price = 180000, StockQty = 50 },
                    // Pickleball
                    new Service { ServiceName = "Thuê Vợt Pickleball Joola Carbon", Category = "Thiết bị", Price = 70000, StockQty = 25 },
                    new Service { ServiceName = "Hộp Bóng Pickleball Franklin (6 quả)", Category = "Dụng cụ", Price = 150000, StockQty = 40 },
                    // Bóng Đá
                    new Service { ServiceName = "Thuê Bóng Đá Động Lực FIFA Quality", Category = "Dụng cụ", Price = 60000, StockQty = 20 },
                    new Service { ServiceName = "Thuê Bộ Áo Bíp Phân Đội (10 áo)", Category = "Thiết bị", Price = 50000, StockQty = 30 },
                    // Dùng Chung
                    new Service { ServiceName = "Thùng Nước Khoáng Pocari Sweat", Category = "Giải khát", Price = 350000, StockQty = 50 },
                    new Service { ServiceName = "Thùng Nước Suối Lavie (24 chai)", Category = "Giải khát", Price = 120000, StockQty = 80 },
                    new Service { ServiceName = "Thuê Trọng Tài Quốc Gia (Theo ca)", Category = "Nhân sự", Price = 500000, StockQty = 10 },
                    new Service { ServiceName = "Y tế & Cứu thương tại sân", Category = "Nhân sự", Price = 400000, StockQty = 5 },
                    new Service { ServiceName = "In ấn Banner & Cờ phướn giải đấu", Category = "Sự kiện", Price = 450000, StockQty = 20 },
                    new Service { ServiceName = "Cúp & Bộ Huy Chương (Vàng, Bạc, Đồng)", Category = "Sự kiện", Price = 800000, StockQty = 15 }
                };

                foreach (var ds in desiredServices)
                {
                    if (!await _context.Services.AnyAsync(s => s.ServiceName == ds.ServiceName))
                    {
                        _context.Services.Add(ds);
                    }
                }
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Đã khởi tạo thành công dữ liệu mẫu Sân thi đấu, Khung giờ và Dịch vụ đi kèm!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi khởi tạo dữ liệu mẫu.", error = ex.Message });
            }
        }
    }
}
