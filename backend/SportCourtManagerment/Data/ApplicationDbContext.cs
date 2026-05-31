using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Enums;
using SportCourtManagerment.Models;

namespace SportCourtManagerment.Data;

/// <summary>Main EF Core DbContext — Sports Court Management System (26 tables).</summary>
public class ApplicationDbContext : DbContext
{
  /// <inheritdoc/>
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

  // ==================== DbSets ====================
  public DbSet<Role> Roles { get; set; }
  public DbSet<MembershipTier> MembershipTiers { get; set; }
  public DbSet<User> Users { get; set; }
  public DbSet<UserRole> UserRoles { get; set; }
  public DbSet<CourtType> CourtTypes { get; set; }
  public DbSet<Court> Courts { get; set; }
  public DbSet<CourtImage> CourtImages { get; set; }
  public DbSet<TimeSlot> TimeSlots { get; set; }
  public DbSet<CourtPricing> CourtPricings { get; set; }
  public DbSet<Promotion> Promotions { get; set; }
  public DbSet<RecurringBooking> RecurringBookings { get; set; }
  public DbSet<Booking> Bookings { get; set; }
  public DbSet<Waitlist> Waitlists { get; set; }
  public DbSet<Service> Services { get; set; }
  public DbSet<EquipmentInventory> EquipmentInventories { get; set; }
  public DbSet<BookingService> BookingServices { get; set; }
  public DbSet<Payment> Payments { get; set; }
  public DbSet<Invoice> Invoices { get; set; }
  public DbSet<Review> Reviews { get; set; }
  public DbSet<Notification> Notifications { get; set; }
  public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
  public DbSet<StaffShift> StaffShifts { get; set; }
  public DbSet<CoachSchedule> CoachSchedules { get; set; }
  public DbSet<PlayerRequest> PlayerRequests { get; set; }
  public DbSet<PlayerRequestMember> PlayerRequestMembers { get; set; }
  public DbSet<AuditLog> AuditLogs { get; set; }

  /// <inheritdoc/>
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    ConfigureRoles(modelBuilder);
    ConfigureMembershipTiers(modelBuilder);
    ConfigureUsers(modelBuilder);
    ConfigureUserRoles(modelBuilder);
    ConfigureCourtTypes(modelBuilder);
    ConfigureCourts(modelBuilder);
    ConfigureCourtImages(modelBuilder);
    ConfigureTimeSlots(modelBuilder);
    ConfigureCourtPricings(modelBuilder);
    ConfigurePromotions(modelBuilder);
    ConfigureRecurringBookings(modelBuilder);
    ConfigureBookings(modelBuilder);
    ConfigureWaitlists(modelBuilder);
    ConfigureServices(modelBuilder);
    ConfigureEquipmentInventories(modelBuilder);
    ConfigureBookingServices(modelBuilder);
    ConfigurePayments(modelBuilder);
    ConfigureInvoices(modelBuilder);
    ConfigureReviews(modelBuilder);
    ConfigureNotifications(modelBuilder);
    ConfigureMaintenanceSchedules(modelBuilder);
    ConfigureStaffShifts(modelBuilder);
    ConfigureCoachSchedules(modelBuilder);
    ConfigurePlayerRequests(modelBuilder);
    ConfigurePlayerRequestMembers(modelBuilder);
    ConfigureAuditLogs(modelBuilder);
  }

  // ==================== Configuration methods ====================

  private static void ConfigureRoles(ModelBuilder mb)
  {
    mb.Entity<Role>(e =>
    {
      e.HasKey(r => r.RoleId);
      e.Property(r => r.RoleName).IsRequired().HasMaxLength(50);
      e.Property(r => r.Description).HasMaxLength(200);
      e.Property(r => r.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(r => r.RoleName).IsUnique();
    });
  }

  private static void ConfigureMembershipTiers(ModelBuilder mb)
  {
    mb.Entity<MembershipTier>(e =>
    {
      e.HasKey(t => t.TierId);
      e.Property(t => t.TierName).IsRequired().HasMaxLength(50);
      e.Property(t => t.MinPoints).HasDefaultValue(0);
      e.Property(t => t.DiscountPercent).HasPrecision(5, 2).HasDefaultValue(0m);
      e.Property(t => t.Description).HasMaxLength(300);
      e.Property(t => t.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(t => t.TierName).IsUnique();
    });
  }

  private static void ConfigureUsers(ModelBuilder mb)
  {
    mb.Entity<User>(e =>
    {
      e.HasKey(u => u.UserId);
      e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
      e.Property(u => u.Email).IsRequired().HasMaxLength(100);
      e.Property(u => u.Phone).HasMaxLength(15);
      e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
      e.Property(u => u.AvatarUrl).HasMaxLength(500);
      e.Property(u => u.LoyaltyPoints).HasDefaultValue(0);
      e.Property(u => u.IsActive).HasDefaultValue(true);
      e.Property(u => u.IsEmailVerified).HasDefaultValue(false);
      e.Property(u => u.RefreshToken).HasMaxLength(500);
      e.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.Property(u => u.Gender).HasConversion<string>().HasMaxLength(10);
      e.HasIndex(u => u.Email).IsUnique();

      e.HasOne(u => u.MembershipTier)
        .WithMany(t => t.Users)
        .HasForeignKey(u => u.MembershipTierId)
        .OnDelete(DeleteBehavior.SetNull);
    });
  }

  private static void ConfigureUserRoles(ModelBuilder mb)
  {
    mb.Entity<UserRole>(e =>
    {
      e.HasKey(ur => ur.UserRoleId);
      e.Property(ur => ur.AssignedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();

      e.HasOne(ur => ur.User)
        .WithMany(u => u.UserRoles)
        .HasForeignKey(ur => ur.UserId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasOne(ur => ur.Role)
        .WithMany(r => r.UserRoles)
        .HasForeignKey(ur => ur.RoleId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureCourtTypes(ModelBuilder mb)
  {
    mb.Entity<CourtType>(e =>
    {
      e.HasKey(ct => ct.CourtTypeId);
      e.Property(ct => ct.TypeName).IsRequired().HasMaxLength(100);
      e.Property(ct => ct.IconUrl).HasMaxLength(500);
      e.Property(ct => ct.Description).HasMaxLength(300);
      e.Property(ct => ct.IsActive).HasDefaultValue(true);
      e.Property(ct => ct.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(ct => ct.TypeName).IsUnique();
    });
  }

  private static void ConfigureCourts(ModelBuilder mb)
  {
    mb.Entity<Court>(e =>
    {
      e.HasKey(c => c.CourtId);
      e.Property(c => c.CourtName).IsRequired().HasMaxLength(100);
      e.Property(c => c.CourtCode).IsRequired().HasMaxLength(20);
      e.Property(c => c.Description).HasMaxLength(1000);
      e.Property(c => c.Location).HasMaxLength(300);
      e.Property(c => c.Surface).HasMaxLength(100);
      e.Property(c => c.ImageUrl).HasMaxLength(500);
      e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20)
        .HasDefaultValue(CourtStatus.Available);
      e.Property(c => c.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(c => c.CourtCode).IsUnique();
      e.HasIndex(c => c.Status);

      e.HasOne(c => c.CourtType)
        .WithMany(ct => ct.Courts)
        .HasForeignKey(c => c.CourtTypeId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureCourtImages(ModelBuilder mb)
  {
    mb.Entity<CourtImage>(e =>
    {
      e.HasKey(ci => ci.ImageId);
      e.Property(ci => ci.ImageUrl).IsRequired().HasMaxLength(500);
      e.Property(ci => ci.IsPrimary).HasDefaultValue(false);
      e.Property(ci => ci.SortOrder).HasDefaultValue(0);
      e.Property(ci => ci.CreatedAt).HasDefaultValueSql("GETDATE()");

      e.HasOne(ci => ci.Court)
        .WithMany(c => c.CourtImages)
        .HasForeignKey(ci => ci.CourtId)
        .OnDelete(DeleteBehavior.Cascade);
    });
  }

  private static void ConfigureTimeSlots(ModelBuilder mb)
  {
    mb.Entity<TimeSlot>(e =>
    {
      e.HasKey(ts => ts.SlotId);
      e.Property(ts => ts.SlotName).IsRequired().HasMaxLength(50);
      e.Property(ts => ts.DayType).HasConversion<string>().HasMaxLength(20)
        .HasDefaultValue(DayType.Weekday);
      e.Property(ts => ts.IsActive).HasDefaultValue(true);
    });
  }

  private static void ConfigureCourtPricings(ModelBuilder mb)
  {
    mb.Entity<CourtPricing>(e =>
    {
      e.ToTable("CourtPricing");
      e.HasKey(cp => cp.PricingId);
      e.Property(cp => cp.Price).HasPrecision(18, 2);
      e.Property(cp => cp.PeakMultiplier).HasPrecision(4, 2).HasDefaultValue(1.0m);
      e.Property(cp => cp.EffectiveFrom).HasDefaultValueSql("GETDATE()");
      e.Property(cp => cp.CreatedAt).HasDefaultValueSql("GETDATE()");

      e.HasOne(cp => cp.Court)
        .WithMany(c => c.CourtPricings)
        .HasForeignKey(cp => cp.CourtId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(cp => cp.TimeSlot)
        .WithMany(ts => ts.CourtPricings)
        .HasForeignKey(cp => cp.SlotId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigurePromotions(ModelBuilder mb)
  {
    mb.Entity<Promotion>(e =>
    {
      e.HasKey(p => p.PromotionId);
      e.Property(p => p.PromoCode).IsRequired().HasMaxLength(50);
      e.Property(p => p.PromoName).IsRequired().HasMaxLength(200);
      e.Property(p => p.Description).HasMaxLength(500);
      e.Property(p => p.DiscountType).HasConversion<string>().HasMaxLength(20);
      e.Property(p => p.DiscountValue).HasPrecision(18, 2);
      e.Property(p => p.MinOrderAmount).HasPrecision(18, 2).HasDefaultValue(0m);
      e.Property(p => p.MaxDiscount).HasPrecision(18, 2);
      e.Property(p => p.UsedCount).HasDefaultValue(0);
      e.Property(p => p.IsActive).HasDefaultValue(true);
      e.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(p => p.PromoCode).IsUnique();
    });
  }

  private static void ConfigureRecurringBookings(ModelBuilder mb)
  {
    mb.Entity<RecurringBooking>(e =>
    {
      e.HasKey(rb => rb.RecurringId);
      e.Property(rb => rb.DaysOfWeek).IsRequired().HasMaxLength(20);
      e.Property(rb => rb.TotalSessions).HasDefaultValue(0);
      e.Property(rb => rb.Status).HasConversion<string>().HasMaxLength(20)
        .HasDefaultValue(RecurringBookingStatus.Active);
      e.Property(rb => rb.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(rb => rb.UserId);

      e.HasOne(rb => rb.User)
        .WithMany(u => u.RecurringBookings)
        .HasForeignKey(rb => rb.UserId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(rb => rb.Court)
        .WithMany(c => c.RecurringBookings)
        .HasForeignKey(rb => rb.CourtId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(rb => rb.TimeSlot)
        .WithMany(ts => ts.RecurringBookings)
        .HasForeignKey(rb => rb.SlotId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureBookings(ModelBuilder mb)
  {
    mb.Entity<Booking>(e =>
    {
      e.HasKey(b => b.BookingId);
      e.Property(b => b.BookingCode).IsRequired().HasMaxLength(20);
      e.Property(b => b.SubTotal).HasPrecision(18, 2);
      e.Property(b => b.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0m);
      e.Property(b => b.TotalAmount).HasPrecision(18, 2);
      e.Property(b => b.Status).HasConversion<string>().HasMaxLength(30)
        .HasDefaultValue(BookingStatus.Pending);
      e.Property(b => b.CancelReason).HasMaxLength(500);
      e.Property(b => b.Note).HasMaxLength(500);
      e.Property(b => b.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(b => b.BookingCode).IsUnique();
      e.HasIndex(b => b.UserId);
      e.HasIndex(b => b.CourtId);
      e.HasIndex(b => b.BookingDate);
      e.HasIndex(b => b.Status);
      e.HasIndex(b => b.RecurringId);

      e.HasOne(b => b.User)
        .WithMany(u => u.Bookings)
        .HasForeignKey(b => b.UserId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(b => b.Court)
        .WithMany(c => c.Bookings)
        .HasForeignKey(b => b.CourtId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(b => b.TimeSlot)
        .WithMany(ts => ts.Bookings)
        .HasForeignKey(b => b.SlotId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(b => b.RecurringBooking)
        .WithMany(rb => rb.Bookings)
        .HasForeignKey(b => b.RecurringId)
        .OnDelete(DeleteBehavior.SetNull);

      e.HasOne(b => b.Promotion)
        .WithMany(p => p.Bookings)
        .HasForeignKey(b => b.PromotionId)
        .OnDelete(DeleteBehavior.SetNull);
    });
  }

  private static void ConfigureWaitlists(ModelBuilder mb)
  {
    mb.Entity<Waitlist>(e =>
    {
      e.HasKey(w => w.WaitlistId);
      e.Property(w => w.Status).HasConversion<string>().HasMaxLength(20)
        .HasDefaultValue(WaitlistStatus.Waiting);
      e.Property(w => w.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(w => new { w.UserId, w.CourtId, w.SlotId, w.WaitDate }).IsUnique();
      e.HasIndex(w => new { w.CourtId, w.SlotId, w.WaitDate });

      e.HasOne(w => w.User)
        .WithMany(u => u.Waitlists)
        .HasForeignKey(w => w.UserId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(w => w.Court)
        .WithMany(c => c.Waitlists)
        .HasForeignKey(w => w.CourtId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(w => w.TimeSlot)
        .WithMany(ts => ts.Waitlists)
        .HasForeignKey(w => w.SlotId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureServices(ModelBuilder mb)
  {
    mb.Entity<Service>(e =>
    {
      e.HasKey(s => s.ServiceId);
      e.Property(s => s.ServiceName).IsRequired().HasMaxLength(100);
      e.Property(s => s.Category).IsRequired().HasMaxLength(50);
      e.Property(s => s.Price).HasPrecision(18, 2);
      e.Property(s => s.Unit).IsRequired().HasMaxLength(30);
      e.Property(s => s.Description).HasMaxLength(300);
      e.Property(s => s.ImageUrl).HasMaxLength(500);
      e.Property(s => s.MinStock).HasDefaultValue(0);
      e.Property(s => s.IsActive).HasDefaultValue(true);
      e.Property(s => s.CreatedAt).HasDefaultValueSql("GETDATE()");
    });
  }

  private static void ConfigureEquipmentInventories(ModelBuilder mb)
  {
    mb.Entity<EquipmentInventory>(e =>
    {
      e.ToTable("EquipmentInventory");
      e.HasKey(ei => ei.InventoryId);
      e.Property(ei => ei.ItemCode).IsRequired().HasMaxLength(50);
      e.Property(ei => ei.Condition).HasConversion<string>().HasMaxLength(20)
        .HasDefaultValue(EquipmentCondition.Good);
      e.Property(ei => ei.PurchasePrice).HasPrecision(18, 2);
      e.Property(ei => ei.Note).HasMaxLength(300);
      e.Property(ei => ei.IsAvailable).HasDefaultValue(true);
      e.Property(ei => ei.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(ei => ei.ItemCode).IsUnique();

      e.HasOne(ei => ei.Service)
        .WithMany(s => s.EquipmentInventories)
        .HasForeignKey(ei => ei.ServiceId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureBookingServices(ModelBuilder mb)
  {
    mb.Entity<BookingService>(e =>
    {
      e.HasKey(bs => bs.BookingServiceId);
      e.Property(bs => bs.Quantity).HasDefaultValue(1);
      e.Property(bs => bs.UnitPrice).HasPrecision(18, 2);
      e.Property(bs => bs.TotalPrice).HasPrecision(18, 2);

      e.HasOne(bs => bs.Booking)
        .WithMany(b => b.BookingServices)
        .HasForeignKey(bs => bs.BookingId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasOne(bs => bs.Service)
        .WithMany(s => s.BookingServices)
        .HasForeignKey(bs => bs.ServiceId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigurePayments(ModelBuilder mb)
  {
    mb.Entity<Payment>(e =>
    {
      e.HasKey(p => p.PaymentId);
      e.Property(p => p.Amount).HasPrecision(18, 2);
      e.Property(p => p.PaymentMethod).HasConversion<string>().HasMaxLength(50);
      e.Property(p => p.TransactionId).HasMaxLength(200);
      e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20)
        .HasDefaultValue(PaymentStatus.Pending);
      e.Property(p => p.RefundAmount).HasPrecision(18, 2).HasDefaultValue(0m);
      e.Property(p => p.RefundNote).HasMaxLength(300);
      e.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(p => p.TransactionId).IsUnique();
      e.HasIndex(p => p.BookingId);
      e.HasIndex(p => p.Status);

      e.HasOne(p => p.Booking)
        .WithMany(b => b.Payments)
        .HasForeignKey(p => p.BookingId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureInvoices(ModelBuilder mb)
  {
    mb.Entity<Invoice>(e =>
    {
      e.HasKey(i => i.InvoiceId);
      e.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(30);
      e.Property(i => i.SubTotal).HasPrecision(18, 2);
      e.Property(i => i.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0m);
      e.Property(i => i.VatPercent).HasPrecision(5, 2).HasDefaultValue(0m);
      e.Property(i => i.VatAmount).HasPrecision(18, 2).HasDefaultValue(0m);
      e.Property(i => i.TotalAmount).HasPrecision(18, 2);
      e.Property(i => i.PdfUrl).HasMaxLength(500);
      e.Property(i => i.IsEmailSent).HasDefaultValue(false);
      e.Property(i => i.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(i => i.InvoiceNumber).IsUnique();

      e.HasOne(i => i.Booking)
        .WithOne(b => b.Invoice)
        .HasForeignKey<Invoice>(i => i.BookingId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(i => i.Payment)
        .WithOne(p => p.Invoice)
        .HasForeignKey<Invoice>(i => i.PaymentId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureReviews(ModelBuilder mb)
  {
    mb.Entity<Review>(e =>
    {
      e.HasKey(r => r.ReviewId);
      e.Property(r => r.Comment).HasMaxLength(1000);
      e.Property(r => r.ImageUrl).HasMaxLength(500);
      e.Property(r => r.IsVisible).HasDefaultValue(true);
      e.Property(r => r.AdminReply).HasMaxLength(500);
      e.Property(r => r.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(r => r.CourtId);

      e.HasOne(r => r.Booking)
        .WithOne(b => b.Review)
        .HasForeignKey<Review>(r => r.BookingId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(r => r.User)
        .WithMany(u => u.Reviews)
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(r => r.Court)
        .WithMany(c => c.Reviews)
        .HasForeignKey(r => r.CourtId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureNotifications(ModelBuilder mb)
  {
    mb.Entity<Notification>(e =>
    {
      e.HasKey(n => n.NotificationId);
      e.Property(n => n.Title).IsRequired().HasMaxLength(200);
      e.Property(n => n.Body).IsRequired().HasMaxLength(1000);
      e.Property(n => n.Type).HasConversion<string>().HasMaxLength(50);
      e.Property(n => n.IsRead).HasDefaultValue(false);
      e.Property(n => n.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(n => new { n.UserId, n.IsRead });

      e.HasOne(n => n.User)
        .WithMany(u => u.Notifications)
        .HasForeignKey(n => n.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    });
  }

  private static void ConfigureMaintenanceSchedules(ModelBuilder mb)
  {
    mb.Entity<MaintenanceSchedule>(e =>
    {
      e.HasKey(ms => ms.MaintenanceId);
      e.Property(ms => ms.MaintenanceType).HasConversion<string>().HasMaxLength(30);
      e.Property(ms => ms.Reason).IsRequired().HasMaxLength(500);
      e.Property(ms => ms.Result).HasMaxLength(500);
      e.Property(ms => ms.Status).HasConversion<string>().HasMaxLength(20)
        .HasDefaultValue(MaintenanceStatus.Scheduled);
      e.Property(ms => ms.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(ms => new { ms.CourtId, ms.Status });

      e.HasOne(ms => ms.Court)
        .WithMany(c => c.MaintenanceSchedules)
        .HasForeignKey(ms => ms.CourtId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(ms => ms.AssignedStaff)
        .WithMany(u => u.AssignedMaintenances)
        .HasForeignKey(ms => ms.AssignedStaffId)
        .OnDelete(DeleteBehavior.SetNull);
    });
  }

  private static void ConfigureStaffShifts(ModelBuilder mb)
  {
    mb.Entity<StaffShift>(e =>
    {
      e.HasKey(ss => ss.ShiftId);
      e.Property(ss => ss.ShiftType).HasConversion<string>().HasMaxLength(20);
      e.Property(ss => ss.Note).HasMaxLength(300);
      e.Property(ss => ss.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(ss => new { ss.StaffId, ss.ShiftDate }).HasDatabaseName("IX_StaffShifts_StaffDate");
      e.HasIndex(ss => new { ss.StaffId, ss.ShiftDate, ss.ShiftType }).IsUnique();

      e.HasOne(ss => ss.Staff)
        .WithMany(u => u.StaffShifts)
        .HasForeignKey(ss => ss.StaffId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureCoachSchedules(ModelBuilder mb)
  {
    mb.Entity<CoachSchedule>(e =>
    {
      e.HasKey(cs => cs.ScheduleId);
      e.Property(cs => cs.MaxStudents).HasDefaultValue(1);
      e.Property(cs => cs.Price).HasPrecision(18, 2);
      e.Property(cs => cs.Note).HasMaxLength(300);
      e.Property(cs => cs.IsBooked).HasDefaultValue(false);
      e.Property(cs => cs.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(cs => new { cs.CoachId, cs.CourtId, cs.SlotId, cs.ScheduleDate }).IsUnique();

      e.HasOne(cs => cs.Coach)
        .WithMany(u => u.CoachSchedules)
        .HasForeignKey(cs => cs.CoachId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(cs => cs.Court)
        .WithMany(c => c.CoachSchedules)
        .HasForeignKey(cs => cs.CourtId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(cs => cs.TimeSlot)
        .WithMany(ts => ts.CoachSchedules)
        .HasForeignKey(cs => cs.SlotId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigurePlayerRequests(ModelBuilder mb)
  {
    mb.Entity<PlayerRequest>(e =>
    {
      e.HasKey(pr => pr.RequestId);
      e.Property(pr => pr.SkillLevel).HasConversion<string>().HasMaxLength(20)
        .HasDefaultValue(SkillLevel.Beginner);
      e.Property(pr => pr.RequiredPlayers).HasDefaultValue(1);
      e.Property(pr => pr.GenderPref).HasConversion<string>().HasMaxLength(10);
      e.Property(pr => pr.Description).HasMaxLength(500);
      e.Property(pr => pr.Status).HasConversion<string>().HasMaxLength(20)
        .HasDefaultValue(PlayerRequestStatus.Open);
      e.Property(pr => pr.CreatedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(pr => pr.Status);

      e.HasOne(pr => pr.Booking)
        .WithOne(b => b.PlayerRequest)
        .HasForeignKey<PlayerRequest>(pr => pr.BookingId)
        .OnDelete(DeleteBehavior.Restrict);

      e.HasOne(pr => pr.HostUser)
        .WithMany(u => u.PlayerRequests)
        .HasForeignKey(pr => pr.HostUserId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigurePlayerRequestMembers(ModelBuilder mb)
  {
    mb.Entity<PlayerRequestMember>(e =>
    {
      e.HasKey(prm => prm.MemberId);
      e.Property(prm => prm.Status).HasConversion<string>().HasMaxLength(20)
        .HasDefaultValue(MemberRequestStatus.Pending);
      e.Property(prm => prm.JoinedAt).HasDefaultValueSql("GETDATE()");
      e.HasIndex(prm => new { prm.RequestId, prm.UserId }).IsUnique();

      e.HasOne(prm => prm.PlayerRequest)
        .WithMany(pr => pr.Members)
        .HasForeignKey(prm => prm.RequestId)
        .OnDelete(DeleteBehavior.Cascade);

      e.HasOne(prm => prm.User)
        .WithMany(u => u.PlayerRequestMembers)
        .HasForeignKey(prm => prm.UserId)
        .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureAuditLogs(ModelBuilder mb)
  {
    mb.Entity<AuditLog>(e =>
    {
      e.HasKey(al => al.LogId);
      e.Property(al => al.Action).IsRequired().HasMaxLength(100);
      e.Property(al => al.TableName).IsRequired().HasMaxLength(100);
      e.Property(al => al.IpAddress).HasMaxLength(50);
      e.Property(al => al.CreatedAt).HasDefaultValueSql("GETDATE()");

      e.HasOne(al => al.User)
        .WithMany()
        .HasForeignKey(al => al.UserId)
        .OnDelete(DeleteBehavior.SetNull);
    });
  }
}
