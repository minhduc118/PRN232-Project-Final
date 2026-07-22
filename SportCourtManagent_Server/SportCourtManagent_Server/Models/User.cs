using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SportCourtManagent_Server.Enums;

namespace SportCourtManagent_Server.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [MaxLength(15)]
        public string? Phone { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = null!;

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public int LoyaltyPoints { get; set; } = 0;

        public int? MembershipTierId { get; set; }

        [ForeignKey("MembershipTierId")]
        public MembershipTier? MembershipTier { get; set; }

        [MaxLength(500)]
        public string? RefreshToken { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public Gender Gender { get; set; } = Gender.Other;

        [Required]
        public SkillLevel SkillLevel { get; set; } = SkillLevel.Beginner;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Wallet? Wallet { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<RecurringBooking> RecurringBookings { get; set; } = new List<RecurringBooking>();
        public ICollection<Waitlist> Waitlists { get; set; } = new List<Waitlist>();
        public ICollection<TaskItem> TasksAssigned { get; set; } = new List<TaskItem>();
        public ICollection<TaskItem> TasksCreated { get; set; } = new List<TaskItem>();
        public ICollection<CourtComplex> ManagedComplexes { get; set; } = new List<CourtComplex>();
        public ICollection<PlayerRequest> PlayerRequests { get; set; } = new List<PlayerRequest>();
        public ICollection<PlayerRequestMember> PlayerRequestMembers { get; set; } = new List<PlayerRequestMember>();
        public ICollection<StaffComplex> ComplexAssignments { get; set; } = new List<StaffComplex>();
    }
}

