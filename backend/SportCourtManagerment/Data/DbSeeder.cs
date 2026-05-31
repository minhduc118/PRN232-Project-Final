using SportCourtManagerment.Enums;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.Data;

/// <summary>Seeds initial reference and demo data if the database is empty.</summary>
public static class DbSeeder
{
  /// <summary>Entry point — runs all seed operations in dependency order.</summary>
  public static async Task SeedAsync(ApplicationDbContext context)
  {
    if (context.Roles.Any()) return; // Skip if already seeded

    await SeedRolesAsync(context);
    await SeedMembershipTiersAsync(context);
    await SeedCourtTypesAsync(context);
    await SeedTimeSlotsAsync(context);
    await SeedUsersAsync(context);
    await SeedCourtsAsync(context);
    await SeedCourtPricingsAsync(context);
    await SeedServicesAsync(context);
    await SeedEquipmentInventoryAsync(context);
    await SeedPromotionsAsync(context);
    await SeedStaffShiftsAsync(context);
    await context.SaveChangesAsync();

    Console.WriteLine("=== SportsCourtDB seeded successfully! ===");
  }

  private static async Task SeedRolesAsync(ApplicationDbContext context)
  {
    var roles = new[]
    {
      new Role { RoleName = "Admin",    Description = "Quản trị toàn bộ hệ thống" },
      new Role { RoleName = "Staff",    Description = "Nhân viên hỗ trợ vận hành" },
      new Role { RoleName = "Coach",    Description = "Huấn luyện viên thể thao" },
      new Role { RoleName = "Customer", Description = "Khách hàng đặt sân" }
    };
    await context.Roles.AddRangeAsync(roles);
    await context.SaveChangesAsync();
  }

  private static async Task SeedMembershipTiersAsync(ApplicationDbContext context)
  {
    var tiers = new[]
    {
      new MembershipTier { TierName = "Bronze",   MinPoints = 0,    DiscountPercent = 0,  Description = "Thành viên cơ bản" },
      new MembershipTier { TierName = "Silver",   MinPoints = 500,  DiscountPercent = 5,  Description = "Giảm 5% mỗi booking" },
      new MembershipTier { TierName = "Gold",     MinPoints = 2000, DiscountPercent = 10, Description = "Giảm 10% + ưu tiên đặt sân" },
      new MembershipTier { TierName = "Platinum", MinPoints = 5000, DiscountPercent = 15, Description = "Giảm 15% + dịch vụ VIP" }
    };
    await context.MembershipTiers.AddRangeAsync(tiers);
    await context.SaveChangesAsync();
  }

  private static async Task SeedCourtTypesAsync(ApplicationDbContext context)
  {
    var types = new[]
    {
      new CourtType { TypeName = "Cầu lông",   Description = "Sân cầu lông tiêu chuẩn BWF",     IsActive = true },
      new CourtType { TypeName = "Bóng đá",    Description = "Sân bóng đá mini 5v5 / 7v7",      IsActive = true },
      new CourtType { TypeName = "Pickleball", Description = "Sân pickleball tiêu chuẩn",        IsActive = true },
      new CourtType { TypeName = "Tennis",     Description = "Sân tennis mặt cứng / đất nện",    IsActive = true },
      new CourtType { TypeName = "Bóng rổ",    Description = "Sân bóng rổ 3x3 / 5v5",           IsActive = true }
    };
    await context.CourtTypes.AddRangeAsync(types);
    await context.SaveChangesAsync();
  }

  private static async Task SeedTimeSlotsAsync(ApplicationDbContext context)
  {
    var slots = new[]
    {
      new TimeSlot { SlotName = "Sáng sớm",        StartTime = new TimeOnly(5,0),  EndTime = new TimeOnly(7,0),  DayType = DayType.Weekday, IsActive = true },
      new TimeSlot { SlotName = "Buổi sáng",       StartTime = new TimeOnly(7,0),  EndTime = new TimeOnly(11,0), DayType = DayType.Weekday, IsActive = true },
      new TimeSlot { SlotName = "Buổi trưa",       StartTime = new TimeOnly(11,0), EndTime = new TimeOnly(13,0), DayType = DayType.Weekday, IsActive = true },
      new TimeSlot { SlotName = "Buổi chiều",      StartTime = new TimeOnly(13,0), EndTime = new TimeOnly(17,0), DayType = DayType.Weekday, IsActive = true },
      new TimeSlot { SlotName = "Giờ vàng",        StartTime = new TimeOnly(17,0), EndTime = new TimeOnly(21,0), DayType = DayType.Weekday, IsActive = true },
      new TimeSlot { SlotName = "Tối muộn",        StartTime = new TimeOnly(21,0), EndTime = new TimeOnly(23,0), DayType = DayType.Weekday, IsActive = true },
      new TimeSlot { SlotName = "Cuối tuần sáng",  StartTime = new TimeOnly(6,0),  EndTime = new TimeOnly(12,0), DayType = DayType.Weekend, IsActive = true },
      new TimeSlot { SlotName = "Cuối tuần chiều", StartTime = new TimeOnly(12,0), EndTime = new TimeOnly(18,0), DayType = DayType.Weekend, IsActive = true },
      new TimeSlot { SlotName = "Cuối tuần tối",   StartTime = new TimeOnly(18,0), EndTime = new TimeOnly(23,0), DayType = DayType.Weekend, IsActive = true }
    };
    await context.TimeSlots.AddRangeAsync(slots);
    await context.SaveChangesAsync();
  }

  private static async Task SeedUsersAsync(ApplicationDbContext context)
  {
    var platinum = context.MembershipTiers.First(t => t.TierName == "Platinum");
    var bronze   = context.MembershipTiers.First(t => t.TierName == "Bronze");
    var silver   = context.MembershipTiers.First(t => t.TierName == "Silver");

    var users = new[]
    {
      new User { FullName = "System Admin",    Email = "admin@sportscourtms.vn", Phone = "0900000001",
                 PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                 IsActive = true, IsEmailVerified = true, MembershipTierId = platinum.TierId },
      new User { FullName = "Nguyễn Văn An",   Email = "staff@sportscourtms.vn", Phone = "0900000002",
                 PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                 IsActive = true, IsEmailVerified = true, MembershipTierId = bronze.TierId },
      new User { FullName = "Trần Thị Bình",   Email = "coach@sportscourtms.vn", Phone = "0900000003",
                 PasswordHash = BCrypt.Net.BCrypt.HashPassword("Coach@123"),
                 IsActive = true, IsEmailVerified = true, MembershipTierId = silver.TierId },
      new User { FullName = "Lê Văn Cường",    Email = "customer@gmail.com",     Phone = "0912345678",
                 PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                 IsActive = true, IsEmailVerified = true, MembershipTierId = bronze.TierId }
    };
    await context.Users.AddRangeAsync(users);
    await context.SaveChangesAsync();

    var roles = context.Roles.ToList();
    var userRoles = new[]
    {
      new UserRole { UserId = users[0].UserId, RoleId = roles.First(r => r.RoleName == "Admin").RoleId },
      new UserRole { UserId = users[1].UserId, RoleId = roles.First(r => r.RoleName == "Staff").RoleId },
      new UserRole { UserId = users[2].UserId, RoleId = roles.First(r => r.RoleName == "Coach").RoleId },
      new UserRole { UserId = users[3].UserId, RoleId = roles.First(r => r.RoleName == "Customer").RoleId }
    };
    await context.UserRoles.AddRangeAsync(userRoles);
    await context.SaveChangesAsync();
  }

  private static async Task SeedCourtsAsync(ApplicationDbContext context)
  {
    var typeMap = context.CourtTypes.ToDictionary(ct => ct.TypeName, ct => ct.CourtTypeId);

    var courts = new[]
    {
      new Court { CourtName = "Sân Cầu Lông A1", CourtCode = "CL-A1", CourtTypeId = typeMap["Cầu lông"],
                  Description = "Sân cầu lông tiêu chuẩn, sàn gỗ, điều hòa",
                  Location = "Tầng 1 Khu A", Capacity = 4, Surface = "Gỗ",
                  OpenTime = new TimeOnly(6,0), CloseTime = new TimeOnly(22,0), Status = CourtStatus.Available },
      new Court { CourtName = "Sân Cầu Lông A2", CourtCode = "CL-A2", CourtTypeId = typeMap["Cầu lông"],
                  Description = "Sân cầu lông tiêu chuẩn, sàn nhựa PVC",
                  Location = "Tầng 1 Khu A", Capacity = 4, Surface = "Nhựa PVC",
                  OpenTime = new TimeOnly(6,0), CloseTime = new TimeOnly(22,0), Status = CourtStatus.Available },
      new Court { CourtName = "Sân Bóng Đá B1",  CourtCode = "BD-B1", CourtTypeId = typeMap["Bóng đá"],
                  Description = "Sân 5v5 cỏ nhân tạo thế hệ 3",
                  Location = "Ngoài trời Khu B", Capacity = 10, Surface = "Cỏ nhân tạo",
                  OpenTime = new TimeOnly(6,0), CloseTime = new TimeOnly(22,0), Status = CourtStatus.Available },
      new Court { CourtName = "Sân Pickleball C1", CourtCode = "PK-C1", CourtTypeId = typeMap["Pickleball"],
                  Description = "Sân pickleball tiêu chuẩn",
                  Location = "Tầng 2 Khu C", Capacity = 4, Surface = "Nhựa",
                  OpenTime = new TimeOnly(6,0), CloseTime = new TimeOnly(22,0), Status = CourtStatus.Available },
      new Court { CourtName = "Sân Tennis D1",    CourtCode = "TN-D1", CourtTypeId = typeMap["Tennis"],
                  Description = "Sân mặt cứng, đèn cao áp",
                  Location = "Ngoài trời Khu D", Capacity = 4, Surface = "Mặt cứng",
                  OpenTime = new TimeOnly(6,0), CloseTime = new TimeOnly(22,0), Status = CourtStatus.Available }
    };
    await context.Courts.AddRangeAsync(courts);
    await context.SaveChangesAsync();
  }

  private static async Task SeedCourtPricingsAsync(ApplicationDbContext context)
  {
    var courtCL = context.Courts.First(c => c.CourtCode == "CL-A1");
    var courtBD = context.Courts.First(c => c.CourtCode == "BD-B1");
    var slots   = context.TimeSlots.ToList();

    int slotIdOf(string name) => slots.First(s => s.SlotName == name).SlotId;

    var pricings = new[]
    {
      // Cầu lông A1
      new CourtPricing { CourtId = courtCL.CourtId, SlotId = slotIdOf("Sáng sớm"),   Price = 80000,  PeakMultiplier = 1.0m, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
      new CourtPricing { CourtId = courtCL.CourtId, SlotId = slotIdOf("Buổi sáng"),  Price = 100000, PeakMultiplier = 1.0m, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
      new CourtPricing { CourtId = courtCL.CourtId, SlotId = slotIdOf("Buổi trưa"),  Price = 90000,  PeakMultiplier = 1.0m, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
      new CourtPricing { CourtId = courtCL.CourtId, SlotId = slotIdOf("Buổi chiều"), Price = 100000, PeakMultiplier = 1.0m, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
      new CourtPricing { CourtId = courtCL.CourtId, SlotId = slotIdOf("Giờ vàng"),   Price = 150000, PeakMultiplier = 1.5m, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
      new CourtPricing { CourtId = courtCL.CourtId, SlotId = slotIdOf("Tối muộn"),   Price = 120000, PeakMultiplier = 1.2m, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
      // Bóng đá B1
      new CourtPricing { CourtId = courtBD.CourtId, SlotId = slotIdOf("Buổi sáng"),       Price = 300000, PeakMultiplier = 1.0m, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
      new CourtPricing { CourtId = courtBD.CourtId, SlotId = slotIdOf("Buổi chiều"),      Price = 300000, PeakMultiplier = 1.0m, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
      new CourtPricing { CourtId = courtBD.CourtId, SlotId = slotIdOf("Giờ vàng"),        Price = 500000, PeakMultiplier = 1.5m, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
      new CourtPricing { CourtId = courtBD.CourtId, SlotId = slotIdOf("Cuối tuần sáng"),  Price = 400000, PeakMultiplier = 1.2m, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
      new CourtPricing { CourtId = courtBD.CourtId, SlotId = slotIdOf("Cuối tuần chiều"), Price = 400000, PeakMultiplier = 1.2m, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) },
      new CourtPricing { CourtId = courtBD.CourtId, SlotId = slotIdOf("Cuối tuần tối"),   Price = 600000, PeakMultiplier = 1.5m, EffectiveFrom = DateOnly.FromDateTime(DateTime.Today) }
    };
    await context.CourtPricings.AddRangeAsync(pricings);
    await context.SaveChangesAsync();
  }

  private static async Task SeedServicesAsync(ApplicationDbContext context)
  {
    var services = new[]
    {
      new Service { ServiceName = "Thuê vợt cầu lông",   Category = "Equipment", Price = 30000,   Unit = "cây/giờ", Description = "Vợt Yonex tiêu chuẩn",              MinStock = 5,  IsActive = true },
      new Service { ServiceName = "Thuê bóng cầu lông",  Category = "Equipment", Price = 10000,   Unit = "ống",     Description = "Hộp 12 quả",                         MinStock = 10, IsActive = true },
      new Service { ServiceName = "Thuê giày thể thao",  Category = "Equipment", Price = 20000,   Unit = "đôi/giờ", Description = "Size 36-44",                          MinStock = 8,  IsActive = true },
      new Service { ServiceName = "Nước suối",           Category = "Drink",     Price = 10000,   Unit = "chai",    Description = "Aquafina 500ml",                      MinStock = 20, IsActive = true },
      new Service { ServiceName = "Nước tăng lực",       Category = "Drink",     Price = 20000,   Unit = "chai",    Description = "Redbull/Sting",                       MinStock = 15, IsActive = true },
      new Service { ServiceName = "Huấn luyện cơ bản",   Category = "Coach",     Price = 200000,  Unit = "buổi",    Description = "1 giờ với HLV cơ bản",               MinStock = 0,  IsActive = true },
      new Service { ServiceName = "Huấn luyện nâng cao", Category = "Coach",     Price = 400000,  Unit = "buổi",    Description = "1 giờ với HLV chuyên nghiệp",        MinStock = 0,  IsActive = true },
      new Service { ServiceName = "Tổ chức giải đấu",    Category = "Event",     Price = 2000000, Unit = "lần",     Description = "Trọn gói tổ chức giải",              MinStock = 0,  IsActive = true }
    };
    await context.Services.AddRangeAsync(services);
    await context.SaveChangesAsync();
  }

  private static async Task SeedEquipmentInventoryAsync(ApplicationDbContext context)
  {
    var vot   = context.Services.First(s => s.ServiceName == "Thuê vợt cầu lông");
    var bong  = context.Services.First(s => s.ServiceName == "Thuê bóng cầu lông");
    var giay  = context.Services.First(s => s.ServiceName == "Thuê giày thể thao");

    var items = new[]
    {
      new EquipmentInventory { ServiceId = vot.ServiceId,  ItemCode = "VOT-001",  Condition = EquipmentCondition.Good,    PurchaseDate = new DateOnly(2026,1,1), PurchasePrice = 500000, IsAvailable = true },
      new EquipmentInventory { ServiceId = vot.ServiceId,  ItemCode = "VOT-002",  Condition = EquipmentCondition.Good,    PurchaseDate = new DateOnly(2026,1,1), PurchasePrice = 500000, IsAvailable = true },
      new EquipmentInventory { ServiceId = vot.ServiceId,  ItemCode = "VOT-003",  Condition = EquipmentCondition.Damaged, PurchaseDate = new DateOnly(2026,1,1), PurchasePrice = 500000, IsAvailable = false },
      new EquipmentInventory { ServiceId = bong.ServiceId, ItemCode = "BONG-001", Condition = EquipmentCondition.Good,    PurchaseDate = new DateOnly(2026,1,1), PurchasePrice = 150000, IsAvailable = true },
      new EquipmentInventory { ServiceId = bong.ServiceId, ItemCode = "BONG-002", Condition = EquipmentCondition.Good,    PurchaseDate = new DateOnly(2026,1,1), PurchasePrice = 150000, IsAvailable = true },
      new EquipmentInventory { ServiceId = giay.ServiceId, ItemCode = "GIAY-001", Condition = EquipmentCondition.Good,    PurchaseDate = new DateOnly(2026,1,15), PurchasePrice = 300000, IsAvailable = true },
      new EquipmentInventory { ServiceId = giay.ServiceId, ItemCode = "GIAY-002", Condition = EquipmentCondition.Good,    PurchaseDate = new DateOnly(2026,1,15), PurchasePrice = 300000, IsAvailable = true }
    };
    await context.EquipmentInventories.AddRangeAsync(items);
    await context.SaveChangesAsync();
  }

  private static async Task SeedPromotionsAsync(ApplicationDbContext context)
  {
    var promos = new[]
    {
      new Promotion { PromoCode = "WELCOME10", PromoName = "Chào mừng thành viên mới", DiscountType = DiscountType.Percent,
                      DiscountValue = 10, MinOrderAmount = 0,      StartDate = new DateTime(2026,1,1),  EndDate = new DateTime(2026,12,31), IsActive = true },
      new Promotion { PromoCode = "SUMMER20",  PromoName = "Khuyến mãi hè 2026",       DiscountType = DiscountType.Percent,
                      DiscountValue = 20, MinOrderAmount = 200000, StartDate = new DateTime(2026,6,1),  EndDate = new DateTime(2026,8,31),  IsActive = true },
      new Promotion { PromoCode = "FIXED50K",  PromoName = "Giảm 50k đơn từ 300k",    DiscountType = DiscountType.FixedAmount,
                      DiscountValue = 50000, MinOrderAmount = 300000, StartDate = new DateTime(2026,5,1), EndDate = new DateTime(2026,7,31), IsActive = true }
    };
    await context.Promotions.AddRangeAsync(promos);
    await context.SaveChangesAsync();
  }

  private static async Task SeedStaffShiftsAsync(ApplicationDbContext context)
  {
    var staff = context.Users.First(u => u.Email == "staff@sportscourtms.vn");
    var shifts = new[]
    {
      new StaffShift { StaffId = staff.UserId, ShiftDate = new DateOnly(2026,5,14), ShiftType = ShiftType.Morning,   StartTime = new TimeOnly(6,0),  EndTime = new TimeOnly(14,0) },
      new StaffShift { StaffId = staff.UserId, ShiftDate = new DateOnly(2026,5,15), ShiftType = ShiftType.Afternoon, StartTime = new TimeOnly(14,0), EndTime = new TimeOnly(22,0) },
      new StaffShift { StaffId = staff.UserId, ShiftDate = new DateOnly(2026,5,16), ShiftType = ShiftType.Morning,   StartTime = new TimeOnly(6,0),  EndTime = new TimeOnly(14,0) }
    };
    await context.StaffShifts.AddRangeAsync(shifts);
    await context.SaveChangesAsync();
  }
}
