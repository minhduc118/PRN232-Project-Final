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

        /// <summary>Seed 50+ services với đúng enum Category cho test paging</summary>
        [HttpPost("seed-services")]
        public async Task<IActionResult> SeedServices()
        {
            try
            {
                var services = new List<Service>
                {
                    // ── Equipment (Dụng cụ) ──────────────────────────
                    new() { ServiceName = "Thuê vợt cầu lông Yonex Astrox",   Category = "Equipment", Price = 50_000,  Unit = "Ca",   StockQty = 30,  IsActive = true,  Description = "Vợt carbon cao cấp, phù hợp tấn công" },
                    new() { ServiceName = "Thuê vợt cầu lông Victor TK-HMR",  Category = "Equipment", Price = 45_000,  Unit = "Ca",   StockQty = 25,  IsActive = true,  Description = "Vợt đa năng, thích hợp người mới" },
                    new() { ServiceName = "Ống cầu Thành Công RS-9 (12 quả)", Category = "Equipment", Price = 240_000, Unit = "Ống",  StockQty = 100, IsActive = true,  Description = "Cầu lông tiêu chuẩn thi đấu quốc nội" },
                    new() { ServiceName = "Thuê vợt tennis Wilson Pro Staff",  Category = "Equipment", Price = 100_000, Unit = "Ca",   StockQty = 15,  IsActive = true,  Description = "Wilson Pro Staff 97 RF - phiên bản Federer" },
                    new() { ServiceName = "Hộp bóng tennis Dunlop ATP (4q)",  Category = "Equipment", Price = 180_000, Unit = "Hộp",  StockQty = 50,  IsActive = true,  Description = "Bóng áp lực cao dùng cho sân cứng" },
                    new() { ServiceName = "Thuê vợt Pickleball Joola Carbon",  Category = "Equipment", Price = 70_000,  Unit = "Ca",   StockQty = 25,  IsActive = true,  Description = "Vợt graphite nhẹ, kiểm soát tốt" },
                    new() { ServiceName = "Hộp bóng Pickleball Franklin (6q)", Category = "Equipment", Price = 150_000, Unit = "Hộp",  StockQty = 40,  IsActive = true,  Description = "Bóng nhựa dùng ngoài trời chuẩn USAPA" },
                    new() { ServiceName = "Thuê bóng đá Động Lực FIFA",       Category = "Equipment", Price = 60_000,  Unit = "Ca",   StockQty = 20,  IsActive = true,  Description = "Kích thước 5, tiêu chuẩn FIFA Quality" },
                    new() { ServiceName = "Bộ áo bíp phân đội (10 áo)",       Category = "Equipment", Price = 50_000,  Unit = "Bộ",   StockQty = 30,  IsActive = true,  Description = "Áo phân đội màu huỳnh quang dễ nhận biết" },
                    new() { ServiceName = "Thuê bóng rổ Spalding NBA",         Category = "Equipment", Price = 80_000,  Unit = "Ca",   StockQty = 20,  IsActive = true,  Description = "Size 7 composite leather" },
                    new() { ServiceName = "Bộ dụng cụ bảo vệ cổ tay/đầu gối", Category = "Equipment", Price = 30_000,  Unit = "Bộ",   StockQty = 50,  IsActive = true,  Description = "Bảo vệ khớp khi vận động mạnh" },
                    new() { ServiceName = "Lưới cầu lông di động",             Category = "Equipment", Price = 40_000,  Unit = "Ca",   StockQty = 10,  IsActive = true,  Description = "Lưới tiêu chuẩn BWF, dễ lắp ráp" },
                    new() { ServiceName = "Thuê giày cầu lông Victor (số 40-45)", Category = "Equipment", Price = 35_000, Unit = "Ca", StockQty = 30,  IsActive = true,  Description = "Nhiều size, đế non-marking" },
                    new() { ServiceName = "Bộ phao bơi người lớn",             Category = "Equipment", Price = 20_000,  Unit = "Cái",  StockQty = 40,  IsActive = true,  Description = "An toàn theo tiêu chuẩn CE" },
                    new() { ServiceName = "Đồng hồ bấm giờ thi đấu",          Category = "Equipment", Price = 25_000,  Unit = "Ca",   StockQty = 15,  IsActive = true,  Description = "Đồng hồ điện tử hiển thị to" },

                    // ── Drink (Đồ uống) ──────────────────────────────
                    new() { ServiceName = "Nước khoáng Lavie 500ml",           Category = "Drink",     Price = 10_000,  Unit = "Chai",  StockQty = 200, IsActive = true,  Description = "Nước tinh khiết thiên nhiên" },
                    new() { ServiceName = "Pocari Sweat 500ml",                Category = "Drink",     Price = 18_000,  Unit = "Chai",  StockQty = 150, IsActive = true,  Description = "Bù điện giải sau vận động" },
                    new() { ServiceName = "Nước tăng lực Redbull 250ml",       Category = "Drink",     Price = 22_000,  Unit = "Lon",   StockQty = 100, IsActive = true,  Description = "Tăng năng lượng tức thì" },
                    new() { ServiceName = "Nước dừa tươi đóng hộp 330ml",      Category = "Drink",     Price = 25_000,  Unit = "Hộp",   StockQty = 80,  IsActive = true,  Description = "Bù khoáng chất tự nhiên" },
                    new() { ServiceName = "Nước ép cam tươi 300ml",            Category = "Drink",     Price = 30_000,  Unit = "Ly",    StockQty = 60,  IsActive = true,  Description = "Vitamin C tự nhiên giúp hồi phục" },
                    new() { ServiceName = "Sữa chua uống Vinamilk 180ml",      Category = "Drink",     Price = 15_000,  Unit = "Hộp",   StockQty = 120, IsActive = true,  Description = "Protein và canxi sau tập luyện" },
                    new() { ServiceName = "Cà phê lon Highlands 250ml",        Category = "Drink",     Price = 25_000,  Unit = "Lon",   StockQty = 80,  IsActive = true,  Description = "Tỉnh táo trước trận" },
                    new() { ServiceName = "Thùng nước Pocari Sweat (24 chai)", Category = "Drink",     Price = 380_000, Unit = "Thùng", StockQty = 20,  IsActive = true,  Description = "Cho sự kiện/giải đấu nhóm" },
                    new() { ServiceName = "Protein shake vani 300ml",          Category = "Drink",     Price = 45_000,  Unit = "Ly",    StockQty = 40,  IsActive = true,  Description = "25g protein sau workout" },
                    new() { ServiceName = "Nước chanh muối đá",                Category = "Drink",     Price = 20_000,  Unit = "Ly",    StockQty = 50,  IsActive = true,  Description = "Bù điện giải dân gian hiệu quả" },

                    // ── Coach (Huấn luyện viên) ──────────────────────
                    new() { ServiceName = "HLV Cầu lông cơ bản (1 buổi)",     Category = "Coach",     Price = 200_000, Unit = "Buổi", StockQty = 10,  IsActive = true,  Description = "Lý thuyết + thực hành 90 phút cho người mới" },
                    new() { ServiceName = "HLV Cầu lông nâng cao (1 buổi)",   Category = "Coach",     Price = 350_000, Unit = "Buổi", StockQty = 8,   IsActive = true,  Description = "Kỹ thuật smash, drop shot, footwork" },
                    new() { ServiceName = "HLV Tennis cơ bản (1 buổi)",       Category = "Coach",     Price = 300_000, Unit = "Buổi", StockQty = 6,   IsActive = true,  Description = "Forehand, backhand, serve cơ bản" },
                    new() { ServiceName = "HLV Tennis chuyên sâu (1 buổi)",   Category = "Coach",     Price = 500_000, Unit = "Buổi", StockQty = 4,   IsActive = true,  Description = "Kỹ thuật slice, volley, topspin nâng cao" },
                    new() { ServiceName = "HLV Bóng rổ cơ bản (1 buổi)",      Category = "Coach",     Price = 250_000, Unit = "Buổi", StockQty = 8,   IsActive = true,  Description = "Dribble, pass, lay-up cho người mới" },
                    new() { ServiceName = "HLV Pickleball (1 buổi)",          Category = "Coach",     Price = 220_000, Unit = "Buổi", StockQty = 10,  IsActive = true,  Description = "Kỹ thuật dink, drive, lob" },
                    new() { ServiceName = "HLV Bóng đá (1 buổi)",             Category = "Coach",     Price = 280_000, Unit = "Buổi", StockQty = 6,   IsActive = true,  Description = "Kỹ thuật dẫn bóng, sút cầu môn" },
                    new() { ServiceName = "Gói HLV Cầu lông 10 buổi",         Category = "Coach",     Price = 1_800_000, Unit = "Gói", StockQty = 5, IsActive = true,  Description = "Tiết kiệm 10% so với đặt lẻ" },
                    new() { ServiceName = "Gói HLV Tennis 10 buổi",           Category = "Coach",     Price = 2_800_000, Unit = "Gói", StockQty = 3, IsActive = true,  Description = "Tiết kiệm 12% so với đặt lẻ" },
                    new() { ServiceName = "Trọng tài trận đấu (per trận)",    Category = "Coach",     Price = 500_000, Unit = "Trận", StockQty = 10,  IsActive = true,  Description = "Trọng tài có chứng chỉ quốc gia" },

                    // ── Event (Sự kiện) ──────────────────────────────
                    new() { ServiceName = "Tổ chức giải đấu nội bộ (< 32 người)", Category = "Event", Price = 2_000_000, Unit = "Giải", StockQty = 5, IsActive = true, Description = "Bao gồm điều phối, bảng đấu, giám sát" },
                    new() { ServiceName = "Tổ chức giải đấu mở rộng (< 64 người)", Category = "Event", Price = 4_000_000, Unit = "Giải", StockQty = 3, IsActive = true, Description = "Cờ hiệu, bảng điểm điện tử, camera stream" },
                    new() { ServiceName = "Thuê sảnh VIP sinh nhật / công ty",Category = "Event",     Price = 3_000_000, Unit = "Buổi", StockQty = 2, IsActive = true, Description = "Sảnh 100m², máy chiếu, âm thanh" },
                    new() { ServiceName = "Gói chụp ảnh thể thao chuyên nghiệp", Category = "Event", Price = 1_500_000, Unit = "Buổi", StockQty = 4, IsActive = true, Description = "RAW + JPEG, giao file trong 48h" },
                    new() { ServiceName = "Livestream trận đấu HD",            Category = "Event",     Price = 800_000, Unit = "Trận",  StockQty = 5,  IsActive = true,  Description = "2 camera góc độc, kỹ thuật viên chuyên nghiệp" },
                    new() { ServiceName = "Cúp vàng + Huy chương bộ 3",        Category = "Event",     Price = 800_000, Unit = "Bộ",   StockQty = 15,  IsActive = true,  Description = "Cúp pha lê + huy chương mạ vàng" },
                    new() { ServiceName = "In banner dọc khổ lớn (90x200cm)", Category = "Event",     Price = 250_000, Unit = "Cái",   StockQty = 20,  IsActive = true,  Description = "In UV, sắc nét, giao trong 24h" },
                    new() { ServiceName = "Thuê máy phát điện dự phòng 5kW",  Category = "Event",     Price = 500_000, Unit = "Ngày",  StockQty = 3,   IsActive = true,  Description = "Đảm bảo điện liên tục cho sự kiện lớn" },
                    new() { ServiceName = "Dịch vụ ăn nhẹ buffet (per người)",Category = "Event",     Price = 150_000, Unit = "Người", StockQty = 100, IsActive = true,  Description = "Trái cây, bánh ngọt, nước uống" },
                    new() { ServiceName = "Đặt phòng họp chiến thuật (2h)",   Category = "Event",     Price = 400_000, Unit = "Ca",    StockQty = 5,   IsActive = true,  Description = "Phòng 20 người, bảng trắng, máy chiếu" },
                    new() { ServiceName = "Gói team building thể thao (1 ngày)", Category = "Event",  Price = 15_000_000, Unit = "Gói", StockQty = 2, IsActive = true, Description = "Tổ chức trọn gói cho 20-50 người" },
                    new() { ServiceName = "Bảo vệ an ninh sự kiện (4h)",      Category = "Event",     Price = 600_000, Unit = "Ca",    StockQty = 8,   IsActive = true,  Description = "2 nhân viên bảo vệ có kinh nghiệm" },
                };

                int added = 0;
                foreach (var svc in services)
                {
                    bool exists = await _context.Services.AnyAsync(s => s.ServiceName == svc.ServiceName);
                    if (!exists)
                    {
                        _context.Services.Add(svc);
                        added++;
                    }
                }
                await _context.SaveChangesAsync();

                int total = await _context.Services.CountAsync();
                return Ok(new { success = true, message = $"Seed hoàn tất! Thêm mới: {added}, Tổng dịch vụ trong DB: {total}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi seed dịch vụ.", error = ex.Message });
            }
        }
    }
}

