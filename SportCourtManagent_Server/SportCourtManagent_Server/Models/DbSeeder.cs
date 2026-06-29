using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // 1. Seed Roles
            if (!await context.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Role { RoleName = "Admin", Description = "Quản trị toàn bộ hệ thống" },
                    new Role { RoleName = "Staff", Description = "Nhân viên hỗ trợ vận hành" },
                    new Role { RoleName = "Coach", Description = "Huấn luyện viên thể thao" },
                    new Role { RoleName = "Customer", Description = "Khách hàng đặt sân" },
                };
                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // 2. Seed Membership Tiers
            if (!await context.MembershipTiers.AnyAsync())
            {
                var tiers = new[]
                {
                    new MembershipTier { TierName = "Bronze", MinPoints = 0, DiscountPercent = 0m },
                    new MembershipTier { TierName = "Silver", MinPoints = 500, DiscountPercent = 5m },
                    new MembershipTier { TierName = "Gold", MinPoints = 2000, DiscountPercent = 10m },
                    new MembershipTier { TierName = "Platinum", MinPoints = 5000, DiscountPercent = 15m },
                };
                await context.MembershipTiers.AddRangeAsync(tiers);
                await context.SaveChangesAsync();
            }

            // 3. Seed Services catalog
            if (!await context.Services.AnyAsync())
            {
                await context.Services.AddRangeAsync(GetDefaultServices());
                await context.SaveChangesAsync();
            }
            else
            {
                await EnsureServicesExistAsync(context);
            }

            // 4. Seed Users
            if (!await context.Users.AnyAsync())
            {
                var bronze = await context.MembershipTiers.FirstAsync(t => t.TierName == "Bronze");
                var silver = await context.MembershipTiers.FirstAsync(t => t.TierName == "Silver");
                var platinum = await context.MembershipTiers.FirstAsync(t => t.TierName == "Platinum");

                var users = new[]
                {
                    new User
                    {
                        FullName = "Admin System",
                        Email = "admin@sportcourt.vn",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        Phone = "0901000001",
                        AvatarUrl = "https://api.dicebear.com/8.x/avataaars/svg?seed=admin",
                        DateOfBirth = new DateOnly(1985, 5, 15),
                        LoyaltyPoints = 9999,
                        MembershipTierId = platinum.TierId,
                        Gender = Gender.Male,
                        SkillLevel = SkillLevel.Advanced,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new User
                    {
                        FullName = "Trần Thị Mai",
                        Email = "staff@sportcourt.vn",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                        Phone = "0901000003",
                        AvatarUrl = "https://api.dicebear.com/8.x/avataaars/svg?seed=mai",
                        DateOfBirth = new DateOnly(1992, 11, 20),
                        LoyaltyPoints = 320,
                        MembershipTierId = bronze.TierId,
                        Gender = Gender.Female,
                        SkillLevel = SkillLevel.Intermediate,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new User
                    {
                        FullName = "Nguyễn Văn Khách",
                        Email = "customer@sportcourt.vn",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("customer123"),
                        Phone = "0901000002",
                        AvatarUrl = "https://api.dicebear.com/8.x/avataaars/svg?seed=khach",
                        DateOfBirth = new DateOnly(1998, 3, 12),
                        LoyaltyPoints = 1250,
                        MembershipTierId = silver.TierId,
                        Gender = Gender.Male,
                        SkillLevel = SkillLevel.Beginner,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new User
                    {
                        FullName = "Lê Minh Tuấn",
                        Email = "coach@sportcourt.vn",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("coach123"),
                        Phone = "0901000004",
                        AvatarUrl = "https://api.dicebear.com/8.x/avataaars/svg?seed=tuan",
                        DateOfBirth = new DateOnly(1988, 8, 25),
                        LoyaltyPoints = 500,
                        MembershipTierId = bronze.TierId,
                        Gender = Gender.Male,
                        SkillLevel = SkillLevel.Advanced,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();

                var roles = await context.Roles.ToListAsync();
                var adminRole = roles.First(r => r.RoleName == "Admin");
                var staffRole = roles.First(r => r.RoleName == "Staff");
                var customerRole = roles.First(r => r.RoleName == "Customer");
                var coachRole = roles.First(r => r.RoleName == "Coach");

                var userRoles = new[]
                {
                    new UserRole { UserId = users[0].UserId, RoleId = adminRole.RoleId },
                    new UserRole { UserId = users[1].UserId, RoleId = staffRole.RoleId },
                    new UserRole { UserId = users[2].UserId, RoleId = customerRole.RoleId },
                    new UserRole { UserId = users[3].UserId, RoleId = coachRole.RoleId },
                };

                await context.UserRoles.AddRangeAsync(userRoles);
                await context.SaveChangesAsync();
            }

            // 5. Seed Court Types
            if (!await context.CourtTypes.AnyAsync())
            {
                var types = new[]
                {
                    new CourtType { TypeName = "Cầu lông", IsActive = true },
                    new CourtType { TypeName = "Pickleball", IsActive = true },
                    new CourtType { TypeName = "Bóng đá", IsActive = true },
                };
                await context.CourtTypes.AddRangeAsync(types);
                await context.SaveChangesAsync();
            }

            // 6. Seed Court Complex + Courts
            if (!await context.CourtComplexes.AnyAsync())
            {
                var staff = await context.Users.FirstAsync(u => u.Email == "staff@sportcourt.vn");
                var badminton = await context.CourtTypes.FirstAsync(t => t.TypeName == "Cầu lông");
                var pickleball = await context.CourtTypes.FirstAsync(t => t.TypeName == "Pickleball");

                var complex = new CourtComplex
                {
                    ComplexName = "Tổ hợp thể thao Cầu Giấy",
                    Address = "Dịch Vọng, Cầu Giấy, Hà Nội",
                    ManagerId = staff.UserId,
                    Description = "Tổ hợp thể thao hiện đại với sân trong nhà điều hòa.",
                    ImageUrl = "https://images.unsplash.com/photo-1545224497-5d750c673417?q=80&w=800",
                    CreatedAt = DateTime.UtcNow
                };
                await context.CourtComplexes.AddAsync(complex);
                await context.SaveChangesAsync();

                var courts = new[]
                {
                    new Court
                    {
                        CourtName = "Sân Cầu Lông A1",
                        CourtCode = "CL-A1",
                        CourtTypeId = badminton.CourtTypeId,
                        ComplexId = complex.ComplexId,
                        Status = CourtStatus.Available,
                        OpenTime = new TimeSpan(6, 0, 0),
                        CloseTime = new TimeSpan(22, 0, 0),
                        PricePerHour = 120000m,
                        CourtSize = "13.4m x 6.1m"
                    },
                    new Court
                    {
                        CourtName = "Sân Pickleball P1",
                        CourtCode = "PK-P1",
                        CourtTypeId = pickleball.CourtTypeId,
                        ComplexId = complex.ComplexId,
                        Status = CourtStatus.Available,
                        OpenTime = new TimeSpan(6, 0, 0),
                        CloseTime = new TimeSpan(23, 0, 0),
                        PricePerHour = 150000m,
                        CourtSize = "13.4m x 6.1m"
                    }
                };
                await context.Courts.AddRangeAsync(courts);
                await context.SaveChangesAsync();
            }

            // 7. Seed complex court type service offerings
            if (!await context.ComplexCourtTypeServices.AnyAsync())
            {
                var complex = await context.CourtComplexes.FirstAsync();
                var badminton = await context.CourtTypes.FirstAsync(t => t.TypeName == "Cầu lông");
                var pickleball = await context.CourtTypes.FirstAsync(t => t.TypeName == "Pickleball");

                var water = await context.Services.FirstAsync(s => s.ServiceName == "Nước suối");
                var towel = await context.Services.FirstAsync(s => s.ServiceName == "Khăn lạnh");
                var racketBadminton = await context.Services.FirstAsync(s => s.ServiceName == "Thuê vợt cầu lông");
                var racketPickle = await context.Services.FirstAsync(s => s.ServiceName == "Thuê vợt Pickleball");
                var ball = await context.Services.FirstAsync(s => s.ServiceName == "Thuê bóng");
                var coach = await context.Services.FirstAsync(s => s.ServiceName == "Huấn luyện viên");

                var offerings = new[]
                {
                    new ComplexCourtTypeService { ComplexId = complex.ComplexId, CourtTypeId = badminton.CourtTypeId, ServiceId = water.ServiceId, Price = 0, StockQty = 100, ServiceMode = ServiceMode.Included, IsActive = true },
                    new ComplexCourtTypeService { ComplexId = complex.ComplexId, CourtTypeId = badminton.CourtTypeId, ServiceId = racketBadminton.ServiceId, Price = 30000, StockQty = 20, ServiceMode = ServiceMode.Optional, IsActive = true },
                    new ComplexCourtTypeService { ComplexId = complex.ComplexId, CourtTypeId = pickleball.CourtTypeId, ServiceId = towel.ServiceId, Price = 0, StockQty = 50, ServiceMode = ServiceMode.Included, IsActive = true },
                    new ComplexCourtTypeService { ComplexId = complex.ComplexId, CourtTypeId = pickleball.CourtTypeId, ServiceId = ball.ServiceId, Price = 0, StockQty = 30, ServiceMode = ServiceMode.Included, IsActive = true },
                    new ComplexCourtTypeService { ComplexId = complex.ComplexId, CourtTypeId = pickleball.CourtTypeId, ServiceId = racketPickle.ServiceId, Price = 35000, StockQty = 15, ServiceMode = ServiceMode.Optional, IsActive = true },
                    new ComplexCourtTypeService { ComplexId = complex.ComplexId, CourtTypeId = pickleball.CourtTypeId, ServiceId = coach.ServiceId, Price = 200000, StockQty = 5, ServiceMode = ServiceMode.Optional, IsActive = true }
                };
                await context.ComplexCourtTypeServices.AddRangeAsync(offerings);
                await context.SaveChangesAsync();
            }

            // 8. Seed Equipment Inventory
            if (!await context.EquipmentInventories.AnyAsync())
            {
                var racket = await context.Services.FirstAsync(s => s.ServiceName == "Thuê vợt cầu lông");
                var ball = await context.Services.FirstAsync(s => s.ServiceName == "Thuê bóng");

                var equipment = new[]
                {
                    new EquipmentInventory { ServiceId = racket.ServiceId, ItemCode = "EQ-001", Condition = EquipmentCondition.Good, PurchaseDate = DateTime.Now.AddMonths(-3), PurchasePrice = 450000m, IsAvailable = true },
                    new EquipmentInventory { ServiceId = racket.ServiceId, ItemCode = "EQ-002", Condition = EquipmentCondition.Good, PurchaseDate = DateTime.Now.AddMonths(-3), PurchasePrice = 450000m, IsAvailable = true },
                    new EquipmentInventory { ServiceId = ball.ServiceId, ItemCode = "EQ-003", Condition = EquipmentCondition.Good, PurchaseDate = DateTime.Now.AddMonths(-1), PurchasePrice = 200000m, IsAvailable = true }
                };

                await context.EquipmentInventories.AddRangeAsync(equipment);
                await context.SaveChangesAsync();
            }
        }

        private static Service[] GetDefaultServices() =>
        [
            new Service { ServiceName = "Thuê vợt cầu lông", Category = "Equipment", Price = 30000m, Unit = "cây/giờ", Description = "Vợt Yonex tiêu chuẩn", IsActive = true },
            new Service { ServiceName = "Thuê vợt Pickleball", Category = "Equipment", Price = 35000m, Unit = "cây/giờ", Description = "Vợt carbon cao cấp", IsActive = true },
            new Service { ServiceName = "Thuê bóng", Category = "Equipment", Price = 10000m, Unit = "quả", Description = "Bóng tập tiêu chuẩn", IsActive = true },
            new Service { ServiceName = "Nước suối", Category = "Drink", Price = 10000m, Unit = "chai", Description = "Aquafina 500ml", IsActive = true },
            new Service { ServiceName = "Nước tăng lực", Category = "Drink", Price = 20000m, Unit = "chai", Description = "Redbull/Sting", IsActive = true },
            new Service { ServiceName = "Huấn luyện viên", Category = "Coach", Price = 200000m, Unit = "giờ", Description = "HLV có chứng chỉ", IsActive = true },
            new Service { ServiceName = "Tổ chức giải đấu", Category = "Event", Price = 5000000m, Unit = "buổi", Description = "Hỗ trợ tổ chức giải mini", IsActive = true },
            new Service { ServiceName = "Khăn lạnh", Category = "Drink", Price = 5000m, Unit = "khăn", Description = "Khăn lạnh sau tập", IsActive = true }
        ];

        private static async Task EnsureServicesExistAsync(AppDbContext context)
        {
            var existingNames = await context.Services
                .Select(s => s.ServiceName)
                .ToListAsync();

            var missing = GetDefaultServices()
                .Where(s => !existingNames.Contains(s.ServiceName))
                .ToList();

            if (missing.Count == 0)
                return;

            await context.Services.AddRangeAsync(missing);
            await context.SaveChangesAsync();
        }
    }
}
