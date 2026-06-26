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

            // 3. Seed Services
            if (!await context.Services.AnyAsync())
            {
                var services = new[]
                {
                    new Service { ServiceName = "Thuê vợt cầu lông", Category = "Equipment", Price = 30000m, StockQty = 10 },
                    new Service { ServiceName = "Thuê bóng tennis", Category = "Equipment", Price = 15000m, StockQty = 20 },
                    new Service { ServiceName = "Thuê giày thể thao", Category = "Equipment", Price = 20000m, StockQty = 8 }
                };
                await context.Services.AddRangeAsync(services);
                await context.SaveChangesAsync();
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
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff123"),
                        Phone = "0901000003",
                        AvatarUrl = "https://api.dicebear.com/8.x/avataaars/svg?seed=mai",
                        DateOfBirth = new DateOnly(1992, 10, 20),
                        LoyaltyPoints = 150,
                        MembershipTierId = bronze.TierId,
                        Gender = Gender.Female,
                        SkillLevel = SkillLevel.Intermediate,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new User
                    {
                        FullName = "Nguyễn Văn Hùng",
                        Email = "customer@sportcourt.vn",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("customer123"),
                        Phone = "0901000002",
                        AvatarUrl = "https://api.dicebear.com/8.x/avataaars/svg?seed=hung",
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

                // Seed UserRoles mapping
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

            // 5. Seed Equipment Inventory
            if (!await context.EquipmentInventories.AnyAsync())
            {
                var racket = await context.Services.FirstAsync(s => s.ServiceName == "Thuê vợt cầu lông");
                var ball = await context.Services.FirstAsync(s => s.ServiceName == "Thuê bóng tennis");

                var equipment = new[]
                {
                    new EquipmentInventory { ServiceId = racket.ServiceId, ItemCode = "EQ-001", Condition = EquipmentCondition.Good, PurchaseDate = DateTime.Now.AddMonths(-3), PurchasePrice = 450000m, IsAvailable = true },
                    new EquipmentInventory { ServiceId = racket.ServiceId, ItemCode = "EQ-002", Condition = EquipmentCondition.Good, PurchaseDate = DateTime.Now.AddMonths(-3), PurchasePrice = 450000m, IsAvailable = true },
                    new EquipmentInventory { ServiceId = racket.ServiceId, ItemCode = "EQ-003", Condition = EquipmentCondition.Damaged, PurchaseDate = DateTime.Now.AddMonths(-2), PurchasePrice = 450000m, IsAvailable = false },
                    new EquipmentInventory { ServiceId = ball.ServiceId, ItemCode = "EQ-004", Condition = EquipmentCondition.Good, PurchaseDate = DateTime.Now.AddMonths(-1), PurchasePrice = 200000m, IsAvailable = true },
                    new EquipmentInventory { ServiceId = ball.ServiceId, ItemCode = "EQ-005", Condition = EquipmentCondition.Retired, PurchaseDate = DateTime.Now.AddMonths(-6), PurchasePrice = 180000m, IsAvailable = false }
                };

                await context.EquipmentInventories.AddRangeAsync(equipment);
                await context.SaveChangesAsync();
            }
        }
    }
}
