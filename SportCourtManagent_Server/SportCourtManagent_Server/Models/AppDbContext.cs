using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
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
        public DbSet<PlayerRequest> PlayerRequests { get; set; } = null!;
        public DbSet<PlayerRequestMember> PlayerRequestMembers { get; set; } = null!;
        public DbSet<TaskItem> Tasks { get; set; } = null!;

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
        }
    }
}

