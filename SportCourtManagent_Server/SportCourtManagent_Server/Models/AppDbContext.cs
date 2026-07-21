using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Database.SetCommandTimeout(180);
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<MembershipTier> MembershipTiers { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<CourtType> CourtTypes { get; set; } = null!;
        public DbSet<CourtComplex> CourtComplexes { get; set; } = null!;
        public DbSet<Court> Courts { get; set; } = null!;
        public DbSet<CourtImage> CourtImages { get; set; } = null!;
        public DbSet<TimeSlot> TimeSlots { get; set; } = null!;
        public DbSet<CourtPricing> CourtPricings { get; set; } = null!;
        public DbSet<Promotion> Promotions { get; set; } = null!;
        public DbSet<Tournament> Tournaments { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<ComplexCourtTypeService> ComplexCourtTypeServices { get; set; } = null!;
        public DbSet<BookingService> BookingServices { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<CoachSchedule> CoachSchedules { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<RecurringBooking> RecurringBookings { get; set; } = null!;
        public DbSet<Waitlist> Waitlists { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<EquipmentInventory> EquipmentInventories { get; set; } = null!;
        public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; } = null!;
        public DbSet<StaffShift> StaffShifts { get; set; } = null!;
        public DbSet<StaffComplex> StaffComplexes { get; set; } = null!;
        public DbSet<PlayerRequest> PlayerRequests { get; set; } = null!;
        public DbSet<PlayerRequestMember> PlayerRequestMembers { get; set; } = null!;
        public DbSet<TaskItem> Tasks { get; set; } = null!;
        public DbSet<Wallet> Wallets { get; set; } = null!;
        public DbSet<WalletTransaction> WalletTransactions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Unique Constraints ---
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            modelBuilder.Entity<CourtType>()
                .HasIndex(ct => ct.TypeName)
                .IsUnique();

            modelBuilder.Entity<Court>()
                .HasIndex(c => c.CourtCode)
                .IsUnique();

            modelBuilder.Entity<Promotion>()
                .HasIndex(p => p.PromoCode)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingCode)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.CourtId, b.SlotId, b.BookingDate }, "IX_Booking_Court_Slot_Date")
                .IsUnique()
                .HasFilter("[Status] != 2"); // 2 is BookingStatus.Cancelled

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            modelBuilder.Entity<EquipmentInventory>()
                .HasIndex(ei => ei.ItemCode)
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.BookingId)
                .IsUnique();

            modelBuilder.Entity<PlayerRequestMember>()
                .HasIndex(prm => new { prm.RequestId, prm.UserId })
                .IsUnique();

            // Wallet 1-1 with User
            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.User)
                .WithOne(u => u.Wallet)
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // WalletTransaction
            modelBuilder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Wallet)
                .WithMany(w => w.WalletTransactions)
                .HasForeignKey(wt => wt.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Booking)
                .WithMany()
                .HasForeignKey(wt => wt.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Relationships and Cascades to avoid Cycles ---

            // UserRole
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> MembershipTier
            modelBuilder.Entity<User>()
                .HasOne(u => u.MembershipTier)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.MembershipTierId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourtComplex -> Manager (User)
            modelBuilder.Entity<CourtComplex>()
                .HasOne(cc => cc.Manager)
                .WithMany(u => u.ManagedComplexes)
                .HasForeignKey(cc => cc.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Court
            modelBuilder.Entity<Court>()
                .HasOne(c => c.CourtType)
                .WithMany(ct => ct.Courts)
                .HasForeignKey(c => c.CourtTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Court>()
                .HasOne(c => c.Complex)
                .WithMany(cc => cc.Courts)
                .HasForeignKey(c => c.ComplexId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourtImage
            modelBuilder.Entity<CourtImage>()
                .HasOne(ci => ci.Court)
                .WithMany(c => c.CourtImages)
                .HasForeignKey(ci => ci.CourtId)
                .OnDelete(DeleteBehavior.Cascade);

            // CourtPricing
            modelBuilder.Entity<CourtPricing>()
                .HasOne(cp => cp.Court)
                .WithMany(c => c.CourtPricings)
                .HasForeignKey(cp => cp.CourtId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourtPricing>()
                .HasOne(cp => cp.TimeSlot)
                .WithMany(ts => ts.CourtPricings)
                .HasForeignKey(cp => cp.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Court)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.TimeSlot)
                .WithMany(ts => ts.Bookings)
                .HasForeignKey(b => b.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Promotion)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.PromotionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Tournament)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            // BookingService
            modelBuilder.Entity<BookingService>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingServices)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookingService>()
                .HasOne(bs => bs.Service)
                .WithMany(s => s.BookingServices)
                .HasForeignKey(bs => bs.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComplexCourtTypeService>()
                .HasIndex(o => new { o.ComplexId, o.CourtTypeId, o.ServiceId })
                .IsUnique();

            modelBuilder.Entity<ComplexCourtTypeService>()
                .HasOne(o => o.Complex)
                .WithMany()
                .HasForeignKey(o => o.ComplexId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComplexCourtTypeService>()
                .HasOne(o => o.CourtType)
                .WithMany()
                .HasForeignKey(o => o.CourtTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComplexCourtTypeService>()
                .HasOne(o => o.Service)
                .WithMany(s => s.ComplexCourtTypeServices)
                .HasForeignKey(o => o.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Booking)
                .WithOne(b => b.Review)
                .HasForeignKey<Review>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Court)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CoachSchedule
            modelBuilder.Entity<CoachSchedule>()
                .HasOne(cs => cs.Coach)
                .WithMany()
                .HasForeignKey(cs => cs.CoachId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoachSchedule>()
                .HasOne(cs => cs.Court)
                .WithMany(c => c.CoachSchedules)
                .HasForeignKey(cs => cs.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoachSchedule>()
                .HasOne(cs => cs.TimeSlot)
                .WithMany(ts => ts.CoachSchedules)
                .HasForeignKey(cs => cs.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // AuditLog
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // RecurringBooking
            modelBuilder.Entity<RecurringBooking>()
                .HasOne(rb => rb.User)
                .WithMany(u => u.RecurringBookings)
                .HasForeignKey(rb => rb.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RecurringBooking>()
                .HasOne(rb => rb.Court)
                .WithMany(c => c.RecurringBookings)
                .HasForeignKey(rb => rb.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RecurringBooking>()
                .HasOne(rb => rb.TimeSlot)
                .WithMany(ts => ts.RecurringBookings)
                .HasForeignKey(rb => rb.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Waitlist
            modelBuilder.Entity<Waitlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Waitlists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Waitlist>()
                .HasOne(w => w.Court)
                .WithMany(c => c.Waitlists)
                .HasForeignKey(w => w.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Waitlist>()
                .HasOne(w => w.TimeSlot)
                .WithMany(ts => ts.Waitlists)
                .HasForeignKey(w => w.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Invoice
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Booking)
                .WithOne(b => b.Invoice)
                .HasForeignKey<Invoice>(i => i.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Payment)
                .WithMany(p => p.Invoices)
                .HasForeignKey(i => i.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            // EquipmentInventory
            modelBuilder.Entity<EquipmentInventory>()
                .HasOne(ei => ei.Service)
                .WithMany(s => s.EquipmentInventories)
                .HasForeignKey(ei => ei.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // MaintenanceSchedule
            modelBuilder.Entity<MaintenanceSchedule>()
                .HasOne(ms => ms.Court)
                .WithMany(c => c.MaintenanceSchedules)
                .HasForeignKey(ms => ms.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceSchedule>()
                .HasOne(ms => ms.AssignedStaff)
                .WithMany()
                .HasForeignKey(ms => ms.AssignedStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            // StaffShift
            modelBuilder.Entity<StaffShift>()
                .HasOne(ss => ss.Staff)
                .WithMany()
                .HasForeignKey(ss => ss.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffShift>()
                .HasOne(ss => ss.Complex)
                .WithMany(cc => cc.StaffShifts)
                .HasForeignKey(ss => ss.ComplexId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StaffShift>()
                .HasIndex(ss => new { ss.StaffId, ss.ShiftDate, ss.ShiftType })
                .IsUnique();

            modelBuilder.Entity<StaffShift>()
                .HasIndex(ss => new { ss.ComplexId, ss.ShiftDate });

            // StaffComplex (junction: Staff ↔ CourtComplex)
            modelBuilder.Entity<StaffComplex>()
                .HasOne(sc => sc.Staff)
                .WithMany(u => u.ComplexAssignments)
                .HasForeignKey(sc => sc.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StaffComplex>()
                .HasOne(sc => sc.Complex)
                .WithMany(cc => cc.StaffAssignments)
                .HasForeignKey(sc => sc.ComplexId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique: mỗi staff chỉ được assign vào 1 complex 1 lần
            modelBuilder.Entity<StaffComplex>()
                .HasIndex(sc => new { sc.StaffId, sc.ComplexId })
                .IsUnique();

            // PlayerRequest
            modelBuilder.Entity<PlayerRequest>()
                .HasOne(pr => pr.Booking)
                .WithMany(b => b.PlayerRequests)
                .HasForeignKey(pr => pr.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayerRequest>()
                .HasOne(pr => pr.HostUser)
                .WithMany(u => u.PlayerRequests)
                .HasForeignKey(pr => pr.HostUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PlayerRequestMember
            modelBuilder.Entity<PlayerRequestMember>()
                .HasOne(prm => prm.PlayerRequest)
                .WithMany(pr => pr.PlayerRequestMembers)
                .HasForeignKey(prm => prm.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerRequestMember>()
                .HasOne(prm => prm.User)
                .WithMany(u => u.PlayerRequestMembers)
                .HasForeignKey(prm => prm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskItem
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Complex)
                .WithMany(cc => cc.Tasks)
                .HasForeignKey(t => t.ComplexId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.AssignedStaff)
                .WithMany(u => u.TasksAssigned)
                .HasForeignKey(t => t.AssignedStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.CreatedBy)
                .WithMany(u => u.TasksCreated)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Booking)
                .WithMany(b => b.Tasks)
                .HasForeignKey(t => t.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Seed Data ---

            // 1. MembershipTiers
            modelBuilder.Entity<MembershipTier>().HasData(
                new MembershipTier { TierId = 1, TierName = "Bronze", MinPoints = 0, DiscountPercent = 0.00m },
                new MembershipTier { TierId = 2, TierName = "Silver", MinPoints = 100, DiscountPercent = 5.00m },
                new MembershipTier { TierId = 3, TierName = "Gold", MinPoints = 500, DiscountPercent = 10.00m }
            );

            // 2. Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin", Description = "System Administrator" },
                new Role { RoleId = 2, RoleName = "Manager", Description = "Complex Manager" },
                new Role { RoleId = 3, RoleName = "Staff", Description = "Staff member" },
                new Role { RoleId = 4, RoleName = "Customer", Description = "End Customer" }
            );

            // 3. Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    FullName = "System Administrator",
                    Email = "admin@sportcourt.com",
                    Phone = "0987654321",
                    PasswordHash = "$2a$11$dCl.7VPYZf4SDJIoHKhfauRS9u37k0XyFJWiIfqDD61ESgsEMedS2",
                    LoyaltyPoints = 0,
                    MembershipTierId = null,
                    IsActive = true,
                    Gender = Gender.Other,
                    SkillLevel = SkillLevel.Advanced,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    UserId = 2,
                    FullName = "Complex Manager",
                    Email = "manager@sportcourt.com",
                    Phone = "0987654322",
                    PasswordHash = "$2a$11$dCl.7VPYZf4SDJIoHKhfauRS9u37k0XyFJWiIfqDD61ESgsEMedS2",
                    LoyaltyPoints = 0,
                    MembershipTierId = null,
                    IsActive = true,
                    Gender = Gender.Male,
                    SkillLevel = SkillLevel.Intermediate,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    UserId = 3,
                    FullName = "Staff Member",
                    Email = "staff@sportcourt.com",
                    Phone = "0987654323",
                    PasswordHash = "$2a$11$dCl.7VPYZf4SDJIoHKhfauRS9u37k0XyFJWiIfqDD61ESgsEMedS2",
                    LoyaltyPoints = 0,
                    MembershipTierId = null,
                    IsActive = true,
                    Gender = Gender.Female,
                    SkillLevel = SkillLevel.Beginner,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new User
                {
                    UserId = 4,
                    FullName = "John Doe",
                    Email = "customer@sportcourt.com",
                    Phone = "0987654324",
                    PasswordHash = "$2a$11$dCl.7VPYZf4SDJIoHKhfauRS9u37k0XyFJWiIfqDD61ESgsEMedS2",
                    LoyaltyPoints = 50,
                    MembershipTierId = 1,
                    IsActive = true,
                    Gender = Gender.Other,
                    SkillLevel = SkillLevel.Beginner,
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );

            // 4. UserRoles
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserRoleId = 1, UserId = 1, RoleId = 1 },
                new UserRole { UserRoleId = 2, UserId = 2, RoleId = 2 },
                new UserRole { UserRoleId = 3, UserId = 3, RoleId = 3 },
                new UserRole { UserRoleId = 4, UserId = 4, RoleId = 4 }
            );

            // 5. CourtComplexes
            modelBuilder.Entity<CourtComplex>().HasData(
                new CourtComplex
                {
                    ComplexId = 1,
                    ComplexName = "Tổ hợp thể thao Cầu Giấy",
                    Address = "Dịch Vọng, Cầu Giấy, Hà Nội",
                    ManagerId = 2,
                    Description = "Tổ hợp thể thao hiện đại bậc nhất khu vực Cầu Giấy với nhiều loại sân khác nhau.",
                    ImageUrl = "https://example.com/complex1.jpg",
                    IsDeleted = false,
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );

            // 4b. StaffComplexes – assign tất cả Staff (UserId=3) vào ComplexId=1
            modelBuilder.Entity<StaffComplex>().HasData(
                new StaffComplex { StaffComplexId = 1, StaffId = 3, ComplexId = 1, AssignedAt = new DateTime(2026, 1, 1) }
            );

            // 6. CourtTypes
            modelBuilder.Entity<CourtType>().HasData(
                new CourtType { CourtTypeId = 1, TypeName = "Pickleball", IsActive = true },
                new CourtType { CourtTypeId = 2, TypeName = "Badminton", IsActive = true },
                new CourtType { CourtTypeId = 3, TypeName = "Football", IsActive = true }
            );

            // 7. Courts
            modelBuilder.Entity<Court>().HasData(
                new Court
                {
                    CourtId = 1,
                    CourtName = "Sân Pickleball P1",
                    CourtCode = "PB-P1",
                    CourtTypeId = 1,
                    ComplexId = 1,
                    Status = CourtStatus.Available,
                    OpenTime = new TimeSpan(6, 0, 0),
                    CloseTime = new TimeSpan(22, 0, 0),
                    PricePerHour = 150000.00m,
                    CourtSize = "20x44 feet",
                    IsDeleted = false
                },
                new Court
                {
                    CourtId = 2,
                    CourtName = "Sân Cầu Lông B1",
                    CourtCode = "BM-B1",
                    CourtTypeId = 2,
                    ComplexId = 1,
                    Status = CourtStatus.Available,
                    OpenTime = new TimeSpan(6, 0, 0),
                    CloseTime = new TimeSpan(22, 0, 0),
                    PricePerHour = 100000.00m,
                    CourtSize = "6.1x13.4 meters",
                    IsDeleted = false
                },
                new Court
                {
                    CourtId = 3,
                    CourtName = "Sân Bóng Đá F1",
                    CourtCode = "FB-F1",
                    CourtTypeId = 3,
                    ComplexId = 1,
                    Status = CourtStatus.Available,
                    OpenTime = new TimeSpan(6, 0, 0),
                    CloseTime = new TimeSpan(22, 0, 0),
                    PricePerHour = 300000.00m,
                    CourtSize = "5-a-side",
                    IsDeleted = false
                }
            );

            // 8. Services
            modelBuilder.Entity<Service>().HasData(
                new Service { ServiceId = 1, ServiceName = "Thuê vợt Pickleball", Category = "EquipmentRent", Price = 30000.00m, StockQty = 20 },
                new Service { ServiceId = 2, ServiceName = "Thuê vợt cầu lông", Category = "EquipmentRent", Price = 20000.00m, StockQty = 30 },
                new Service { ServiceId = 3, ServiceName = "Nước uống Pocari", Category = "Drink", Price = 15000.00m, StockQty = 100 },
                new Service { ServiceId = 4, ServiceName = "Nước suối Aquafina", Category = "Drink", Price = 10000.00m, StockQty = 150 }
            );

            // 9. TimeSlots
            modelBuilder.Entity<TimeSlot>().HasData(
                new TimeSlot { SlotId = 1, SlotName = "Slot 1 (06:00 - 07:30)", StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(7, 30, 0), DayType = DayType.Weekday },
                new TimeSlot { SlotId = 2, SlotName = "Slot 2 (07:30 - 09:00)", StartTime = new TimeSpan(7, 30, 0), EndTime = new TimeSpan(9, 0, 0), DayType = DayType.Weekday },
                new TimeSlot { SlotId = 3, SlotName = "Slot 3 (09:00 - 10:30)", StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), DayType = DayType.Weekday },
                new TimeSlot { SlotId = 4, SlotName = "Slot 4 (15:00 - 16:30)", StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(16, 30, 0), DayType = DayType.Weekday },
                new TimeSlot { SlotId = 5, SlotName = "Slot 5 (16:30 - 18:00)", StartTime = new TimeSpan(16, 30, 0), EndTime = new TimeSpan(18, 0, 0), DayType = DayType.Weekday },
                new TimeSlot { SlotId = 6, SlotName = "Slot 6 (18:00 - 19:30)", StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(19, 30, 0), DayType = DayType.Weekday },
                new TimeSlot { SlotId = 7, SlotName = "Slot 7 (19:30 - 21:00)", StartTime = new TimeSpan(19, 30, 0), EndTime = new TimeSpan(21, 0, 0), DayType = DayType.Weekday },
                new TimeSlot { SlotId = 8, SlotName = "Slot 8 (21:00 - 22:30)", StartTime = new TimeSpan(21, 0, 0), EndTime = new TimeSpan(22, 30, 0), DayType = DayType.Weekday }
            );

            // 10. CourtPricing
            modelBuilder.Entity<CourtPricing>().HasData(
                new CourtPricing { PricingId = 1, CourtId = 1, SlotId = 1, Price = 120000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 2, CourtId = 1, SlotId = 2, Price = 120000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 3, CourtId = 1, SlotId = 3, Price = 120000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 4, CourtId = 1, SlotId = 4, Price = 150000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 5, CourtId = 1, SlotId = 5, Price = 150000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 6, CourtId = 1, SlotId = 6, Price = 180000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 7, CourtId = 1, SlotId = 7, Price = 180000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 8, CourtId = 1, SlotId = 8, Price = 180000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                
                new CourtPricing { PricingId = 9, CourtId = 2, SlotId = 1, Price = 80000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 10, CourtId = 2, SlotId = 2, Price = 80000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 11, CourtId = 2, SlotId = 3, Price = 80000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 12, CourtId = 2, SlotId = 4, Price = 100000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 13, CourtId = 2, SlotId = 5, Price = 100000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 14, CourtId = 2, SlotId = 6, Price = 120000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 15, CourtId = 2, SlotId = 7, Price = 120000.00m, EffectiveFrom = new DateTime(2026, 1, 1) },
                new CourtPricing { PricingId = 16, CourtId = 2, SlotId = 8, Price = 120000.00m, EffectiveFrom = new DateTime(2026, 1, 1) }
            );

            // 11. MaintenanceSchedules
            modelBuilder.Entity<MaintenanceSchedule>().HasData(
                new MaintenanceSchedule
                {
                    MaintenanceId = 1,
                    CourtId = 1,
                    MaintenanceType = MaintenanceType.Routine,
                    StartDateTime = new DateTime(2026, 7, 20, 8, 0, 0),
                    EndDateTime = new DateTime(2026, 7, 20, 10, 0, 0),
                    AssignedStaffId = 3,
                    Reason = "Bảo trì định kỳ mặt sân Pickleball P1",
                    Result = "Đã lau chùi mặt sân và căng lại lưới",
                    ImageProof = "https://pos.nvncdn.com/3c8244-211061/art/artCT/20240812_0rmC0gAF.jpg",
                    Status = MaintenanceStatus.Completed
                },
                new MaintenanceSchedule
                {
                    MaintenanceId = 2,
                    CourtId = 2,
                    MaintenanceType = MaintenanceType.Emergency,
                    StartDateTime = new DateTime(2026, 7, 22, 14, 0, 0),
                    EndDateTime = new DateTime(2026, 7, 22, 16, 0, 0),
                    AssignedStaffId = 3,
                    Reason = "Sửa chữa sự cố hệ thống đèn chiếu sáng tại sân B1",
                    Result = null,
                    ImageProof = null,
                    Status = MaintenanceStatus.Scheduled
                }
            );

            // 12. Tasks (TaskItem)
            modelBuilder.Entity<TaskItem>().HasData(
                new TaskItem
                {
                    TaskId = 1,
                    Title = "Vệ sinh khu vực thay đồ & nhà vệ sinh",
                    Description = "Lau dọn sạch sẽ khu vực nhà vệ sinh nam nữ và bổ sung xà phòng",
                    TaskType = TaskType.Manual,
                    Category = TaskCategory.Cleanup,
                    Priority = TaskPriority.High,
                    Status = TaskItemStatus.Completed,
                    ComplexId = 1,
                    AssignedStaffId = 3,
                    CreatedById = 2,
                    DueDate = new DateTime(2026, 7, 21, 18, 0, 0),
                    CreatedAt = new DateTime(2026, 7, 21, 7, 0, 0),
                    CompletedAt = new DateTime(2026, 7, 21, 10, 30, 0),
                    ImageProof = "https://pos.nvncdn.com/3c8244-211061/art/artCT/20240812_0rmC0gAF.jpg"
                },
                new TaskItem
                {
                    TaskId = 2,
                    Title = "Kiểm tra và nạp bổ sung nước uống Pocari",
                    Description = "Kiểm tra kho và bổ sung 50 chai Pocari vào tủ mát",
                    TaskType = TaskType.Manual,
                    Category = TaskCategory.ServicePrep,
                    Priority = TaskPriority.Medium,

                    Status = TaskItemStatus.Pending,
                    ComplexId = 1,
                    AssignedStaffId = 3,
                    CreatedById = 2,
                    DueDate = new DateTime(2026, 7, 22, 12, 0, 0),
                    CreatedAt = new DateTime(2026, 7, 21, 8, 0, 0),
                    CompletedAt = null,
                    ImageProof = null
                }
            );
        }
    }
}


