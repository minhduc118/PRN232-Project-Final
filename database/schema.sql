-- =============================================
-- Sports Court Management System - SQL Server
-- Version: 3.0 | PRN232 | 28 Tables (Aligned with EF Core Migrations)
-- Database Name: PRN232_SCM_DB
-- =============================================

USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'PRN232_SCM_DB')
BEGIN
    ALTER DATABASE PRN232_SCM_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PRN232_SCM_DB;
END
GO
CREATE DATABASE PRN232_SCM_DB COLLATE Vietnamese_CI_AS;
GO
USE PRN232_SCM_DB;
GO

-- =============================================
-- CREATE TABLES (In order of dependencies)
-- =============================================

-- 1. ROLES
CREATE TABLE Roles (
    RoleId      INT PRIMARY KEY IDENTITY(1,1),
    RoleName    NVARCHAR(50)  NOT NULL,
    Description NVARCHAR(255) NULL
);
GO
CREATE UNIQUE INDEX IX_Roles_RoleName ON Roles(RoleName);
GO

-- 2. MEMBERSHIP TIERS
CREATE TABLE MembershipTiers (
    TierId          INT PRIMARY KEY IDENTITY(1,1),
    TierName        NVARCHAR(50)   NOT NULL,
    MinPoints       INT            NOT NULL,
    DiscountPercent DECIMAL(18,2)  NOT NULL
);
GO

-- 3. PROMOTIONS
CREATE TABLE Promotions (
    PromotionId    INT PRIMARY KEY IDENTITY(1,1),
    PromoCode      NVARCHAR(50)   NOT NULL,
    PromoName      NVARCHAR(100)  NOT NULL,
    DiscountType   INT            NOT NULL, -- 0 = Percent, 1 = FixedAmount
    DiscountValue  DECIMAL(18,2)  NOT NULL,
    StartDate      DATETIME2      NOT NULL,
    EndDate        DATETIME2      NOT NULL
);
GO
CREATE UNIQUE INDEX IX_Promotions_PromoCode ON Promotions(PromoCode);
GO

-- 4. COURT TYPES
CREATE TABLE CourtTypes (
    CourtTypeId INT PRIMARY KEY IDENTITY(1,1),
    TypeName    NVARCHAR(50) NOT NULL,
    IsActive    BIT          NOT NULL
);
GO
CREATE UNIQUE INDEX IX_CourtTypes_TypeName ON CourtTypes(TypeName);
GO

-- 5. TIME SLOTS
CREATE TABLE TimeSlots (
    SlotId    INT PRIMARY KEY IDENTITY(1,1),
    SlotName  NVARCHAR(50) NOT NULL,
    StartTime TIME         NOT NULL,
    EndTime   TIME         NOT NULL,
    DayType   INT          NOT NULL -- 0 = Weekday, 1 = Weekend, 2 = Holiday
);
GO

-- 6. SERVICES
CREATE TABLE Services (
    ServiceId   INT PRIMARY KEY IDENTITY(1,1),
    ServiceName NVARCHAR(100) NOT NULL,
    Category    NVARCHAR(50)  NOT NULL,
    Price       DECIMAL(18,2) NOT NULL,
    StockQty    INT           NOT NULL
);
GO

-- 7. USERS
CREATE TABLE Users (
    UserId             INT PRIMARY KEY IDENTITY(1,1),
    FullName           NVARCHAR(100) NOT NULL,
    Email              NVARCHAR(100) NOT NULL,
    Phone              NVARCHAR(15)  NULL,
    PasswordHash       NVARCHAR(255) NOT NULL,
    AvatarUrl          NVARCHAR(500) NULL,
    LoyaltyPoints      INT           NOT NULL,
    MembershipTierId   INT           NULL,
    RefreshToken       NVARCHAR(500) NULL,
    IsActive           BIT           NOT NULL,
    Gender             INT           NOT NULL, -- 0 = Male, 1 = Female, 2 = Other
    SkillLevel         INT           NOT NULL, -- 0 = Beginner, 1 = Intermediate, 2 = Advanced
    CreatedAt          DATETIME2     NOT NULL,
    FOREIGN KEY (MembershipTierId) REFERENCES MembershipTiers(TierId) ON DELETE NO ACTION
);
GO
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
GO
CREATE INDEX IX_Users_MembershipTierId ON Users(MembershipTierId);
GO

-- 8. EQUIPMENT INVENTORY
CREATE TABLE EquipmentInventory (
    InventoryId   INT PRIMARY KEY IDENTITY(1,1),
    ServiceId     INT           NOT NULL,
    ItemCode      NVARCHAR(50)  NOT NULL,
    Condition     INT           NOT NULL, -- 0 = Good, 1 = Damaged, 2 = Retired
    PurchaseDate  DATETIME2     NOT NULL,
    PurchasePrice DECIMAL(18,2) NOT NULL,
    IsAvailable   BIT           NOT NULL,
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE NO ACTION
);
GO
CREATE UNIQUE INDEX IX_EquipmentInventory_ItemCode ON EquipmentInventory(ItemCode);
GO
CREATE INDEX IX_EquipmentInventory_ServiceId ON EquipmentInventory(ServiceId);
GO

-- 9. AUDIT LOGS
CREATE TABLE AuditLogs (
    LogId     INT PRIMARY KEY IDENTITY(1,1),
    UserId    INT           NULL,
    Action    NVARCHAR(100) NOT NULL,
    TableName NVARCHAR(100) NULL,
    Timestamp DATETIME2     NOT NULL,
    Details   NVARCHAR(MAX) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
GO

-- 10. COURT COMPLEXES
CREATE TABLE CourtComplexes (
    ComplexId   INT PRIMARY KEY IDENTITY(1,1),
    ComplexName NVARCHAR(150) NOT NULL,
    Address     NVARCHAR(300) NOT NULL,
    ManagerId   INT           NOT NULL,
    Description NVARCHAR(1000) NULL,
    ImageUrl    NVARCHAR(500) NULL,
    IsDeleted   BIT           NOT NULL,
    CreatedAt   DATETIME2     NOT NULL,
    FOREIGN KEY (ManagerId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_CourtComplexes_ManagerId ON CourtComplexes(ManagerId);
GO

-- 11. NOTIFICATIONS
CREATE TABLE Notifications (
    NotificationId INT PRIMARY KEY IDENTITY(1,1),
    UserId         INT            NOT NULL,
    Title          NVARCHAR(200)  NOT NULL,
    Type           INT            NOT NULL, -- NotificationType Enum
    IsRead         BIT            NOT NULL,
    CreatedAt      DATETIME2      NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
GO
CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
GO

-- 12. STAFF SHIFTS
CREATE TABLE StaffShifts (
    ShiftId      INT PRIMARY KEY IDENTITY(1,1),
    StaffId      INT          NOT NULL,
    ShiftDate    DATETIME2    NOT NULL,
    ShiftType    INT          NOT NULL, -- 0 = Morning, 1 = Afternoon, 2 = Evening
    StartTime    TIME         NOT NULL,
    EndTime      TIME         NOT NULL,
    CheckInTime  DATETIME2    NULL,
    CheckOutTime DATETIME2    NULL,
    FOREIGN KEY (StaffId) REFERENCES Users(UserId) ON DELETE CASCADE
);
GO
CREATE INDEX IX_StaffShifts_StaffId ON StaffShifts(StaffId);
GO

-- 13. USER ROLES
CREATE TABLE UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId     INT NOT NULL,
    RoleId     INT NOT NULL,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
GO
CREATE INDEX IX_UserRoles_RoleId ON UserRoles(RoleId);
GO
CREATE INDEX IX_UserRoles_UserId ON UserRoles(UserId);
GO

-- 14. COURTS
CREATE TABLE Courts (
    CourtId      INT PRIMARY KEY IDENTITY(1,1),
    CourtName    NVARCHAR(100)  NOT NULL,
    CourtCode    NVARCHAR(20)   NOT NULL,
    CourtTypeId  INT            NOT NULL,
    ComplexId    INT            NOT NULL,
    Status       INT            NOT NULL, -- 0 = Available, 1 = Booked, etc.
    OpenTime     TIME           NOT NULL,
    CloseTime    TIME           NOT NULL,
    PricePerHour DECIMAL(18,2)  NOT NULL,
    CourtSize    NVARCHAR(50)   NULL,
    IsDeleted    BIT            NOT NULL,
    FOREIGN KEY (ComplexId) REFERENCES CourtComplexes(ComplexId) ON DELETE NO ACTION,
    FOREIGN KEY (CourtTypeId) REFERENCES CourtTypes(CourtTypeId) ON DELETE NO ACTION
);
GO
CREATE UNIQUE INDEX IX_Courts_CourtCode ON Courts(CourtCode);
GO
CREATE INDEX IX_Courts_ComplexId ON Courts(ComplexId);
GO
CREATE INDEX IX_Courts_CourtTypeId ON Courts(CourtTypeId);
GO

-- 15. BOOKINGS
CREATE TABLE Bookings (
    BookingId      INT PRIMARY KEY IDENTITY(1,1),
    BookingCode    NVARCHAR(20)   NOT NULL,
    UserId         INT            NOT NULL,
    CourtId        INT            NOT NULL,
    SlotId         INT            NOT NULL,
    BookingDate    DATETIME2      NOT NULL,
    StartTime      TIME           NOT NULL,
    EndTime        TIME           NOT NULL,
    SubTotal       DECIMAL(18,2)  NOT NULL,
    DiscountAmount DECIMAL(18,2)  NOT NULL,
    TotalAmount    DECIMAL(18,2)  NOT NULL,
    Status         INT            NOT NULL, -- BookingStatus Enum
    PromotionId    INT            NULL,
    FOREIGN KEY (CourtId) REFERENCES Courts(CourtId) ON DELETE NO ACTION,
    FOREIGN KEY (PromotionId) REFERENCES Promotions(PromotionId) ON DELETE NO ACTION,
    FOREIGN KEY (SlotId) REFERENCES TimeSlots(SlotId) ON DELETE NO ACTION,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO
CREATE UNIQUE INDEX IX_Bookings_BookingCode ON Bookings(BookingCode);
GO
CREATE INDEX IX_Bookings_CourtId ON Bookings(CourtId);
GO
CREATE INDEX IX_Bookings_PromotionId ON Bookings(PromotionId);
GO
CREATE INDEX IX_Bookings_SlotId ON Bookings(SlotId);
GO
CREATE INDEX IX_Bookings_UserId ON Bookings(UserId);
GO

-- 16. COACH SCHEDULES
CREATE TABLE CoachSchedules (
    ScheduleId   INT PRIMARY KEY IDENTITY(1,1),
    CoachId      INT           NOT NULL,
    CourtId      INT           NOT NULL,
    SlotId       INT           NOT NULL,
    ScheduleDate DATETIME2     NOT NULL,
    Price        DECIMAL(18,2) NOT NULL,
    IsBooked     BIT           NOT NULL,
    FOREIGN KEY (CourtId) REFERENCES Courts(CourtId) ON DELETE NO ACTION,
    FOREIGN KEY (SlotId) REFERENCES TimeSlots(SlotId) ON DELETE NO ACTION,
    FOREIGN KEY (CoachId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_CoachSchedules_CoachId ON CoachSchedules(CoachId);
GO
CREATE INDEX IX_CoachSchedules_CourtId ON CoachSchedules(CourtId);
GO
CREATE INDEX IX_CoachSchedules_SlotId ON CoachSchedules(SlotId);
GO

-- 17. COURT IMAGES
CREATE TABLE CourtImages (
    CourtImageId INT PRIMARY KEY IDENTITY(1,1),
    CourtId      INT            NOT NULL,
    ImageUrl     NVARCHAR(500)  NOT NULL,
    IsPrimary    BIT            NOT NULL,
    FOREIGN KEY (CourtId) REFERENCES Courts(CourtId) ON DELETE CASCADE
);
GO
CREATE INDEX IX_CourtImages_CourtId ON CourtImages(CourtId);
GO

-- 18. COURT PRICING
CREATE TABLE CourtPricing (
    PricingId     INT PRIMARY KEY IDENTITY(1,1),
    CourtId       INT           NOT NULL,
    SlotId        INT           NOT NULL,
    Price         DECIMAL(18,2) NOT NULL,
    EffectiveFrom DATETIME2     NOT NULL,
    FOREIGN KEY (CourtId) REFERENCES Courts(CourtId) ON DELETE CASCADE,
    FOREIGN KEY (SlotId) REFERENCES TimeSlots(SlotId) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_CourtPricing_CourtId ON CourtPricing(CourtId);
GO
CREATE INDEX IX_CourtPricing_SlotId ON CourtPricing(SlotId);
GO

-- 19. MAINTENANCE SCHEDULES
CREATE TABLE MaintenanceSchedules (
    MaintenanceId   INT PRIMARY KEY IDENTITY(1,1),
    CourtId         INT           NOT NULL,
    MaintenanceType INT           NOT NULL, -- MaintenanceType Enum
    StartDateTime   DATETIME2     NOT NULL,
    EndDateTime     DATETIME2     NOT NULL,
    AssignedStaffId INT           NULL,
    Reason          NVARCHAR(500) NULL,
    Result          NVARCHAR(500) NULL,
    Status          INT           NOT NULL, -- MaintenanceStatus Enum
    FOREIGN KEY (CourtId) REFERENCES Courts(CourtId) ON DELETE NO ACTION,
    FOREIGN KEY (AssignedStaffId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_MaintenanceSchedules_AssignedStaffId ON MaintenanceSchedules(AssignedStaffId);
GO
CREATE INDEX IX_MaintenanceSchedules_CourtId ON MaintenanceSchedules(CourtId);
GO

-- 20. RECURRING BOOKINGS
CREATE TABLE RecurringBookings (
    RecurringId   INT PRIMARY KEY IDENTITY(1,1),
    UserId        INT          NOT NULL,
    CourtId       INT          NOT NULL,
    SlotId        INT          NOT NULL,
    StartDate     DATETIME2    NOT NULL,
    EndDate       DATETIME2    NOT NULL,
    DaysOfWeek    NVARCHAR(50) NOT NULL,
    Status        INT          NOT NULL, -- RecurringBookingStatus Enum
    FOREIGN KEY (CourtId) REFERENCES Courts(CourtId) ON DELETE NO ACTION,
    FOREIGN KEY (SlotId) REFERENCES TimeSlots(SlotId) ON DELETE NO ACTION,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_RecurringBookings_CourtId ON RecurringBookings(CourtId);
GO
CREATE INDEX IX_RecurringBookings_SlotId ON RecurringBookings(SlotId);
GO
CREATE INDEX IX_RecurringBookings_UserId ON RecurringBookings(UserId);
GO

-- 21. WAITLISTS
CREATE TABLE Waitlists (
    WaitlistId INT PRIMARY KEY IDENTITY(1,1),
    UserId     INT       NOT NULL,
    CourtId    INT       NOT NULL,
    SlotId     INT       NOT NULL,
    WaitDate   DATETIME2 NOT NULL,
    Position   INT       NOT NULL,
    Status     INT       NOT NULL, -- WaitlistStatus Enum
    NotifiedAt DATETIME2 NULL,
    ExpiredAt  DATETIME2 NULL,
    FOREIGN KEY (CourtId) REFERENCES Courts(CourtId) ON DELETE NO ACTION,
    FOREIGN KEY (SlotId) REFERENCES TimeSlots(SlotId) ON DELETE NO ACTION,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_Waitlists_CourtId ON Waitlists(CourtId);
GO
CREATE INDEX IX_Waitlists_SlotId ON Waitlists(SlotId);
GO
CREATE INDEX IX_Waitlists_UserId ON Waitlists(UserId);
GO

-- 22. BOOKING SERVICES
CREATE TABLE BookingServices (
    BookingServiceId INT PRIMARY KEY IDENTITY(1,1),
    BookingId        INT           NOT NULL,
    ServiceId        INT           NOT NULL,
    Quantity         INT           NOT NULL,
    TotalPrice       DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_BookingServices_BookingId ON BookingServices(BookingId);
GO
CREATE INDEX IX_BookingServices_ServiceId ON BookingServices(ServiceId);
GO

-- 23. PAYMENTS
CREATE TABLE Payments (
    PaymentId       INT PRIMARY KEY IDENTITY(1,1),
    BookingId       INT           NOT NULL,
    Amount          DECIMAL(18,2) NOT NULL,
    PaymentMethod   INT           NOT NULL, -- PaymentMethod Enum
    TransactionId   NVARCHAR(200) NOT NULL,
    GatewayResponse NVARCHAR(MAX) NULL,
    Status          INT           NOT NULL, -- PaymentStatus Enum
    RefundAmount    DECIMAL(18,2) NOT NULL,
    PaidAt          DATETIME2     NULL,
    FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId) ON DELETE CASCADE
);
GO
CREATE UNIQUE INDEX IX_Payments_BookingId ON Payments(BookingId);
GO
CREATE UNIQUE INDEX IX_Payments_TransactionId ON Payments(TransactionId);
GO

-- 24. PLAYER REQUESTS
CREATE TABLE PlayerRequests (
    RequestId       INT PRIMARY KEY IDENTITY(1,1),
    BookingId       INT NOT NULL,
    HostUserId      INT NOT NULL,
    SkillLevel      INT NOT NULL, -- SkillLevel Enum
    RequiredPlayers INT NOT NULL,
    GenderPref      INT NOT NULL, -- Gender Enum
    Status          INT NOT NULL, -- PlayerRequestStatus Enum
    FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId) ON DELETE NO ACTION,
    FOREIGN KEY (HostUserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_PlayerRequests_BookingId ON PlayerRequests(BookingId);
GO
CREATE INDEX IX_PlayerRequests_HostUserId ON PlayerRequests(HostUserId);
GO

-- 25. REVIEWS
CREATE TABLE Reviews (
    ReviewId  INT PRIMARY KEY IDENTITY(1,1),
    BookingId INT            NOT NULL,
    UserId    INT            NOT NULL,
    CourtId   INT            NOT NULL,
    Rating    TINYINT        NOT NULL,
    Comment   NVARCHAR(1000) NULL,
    IsVisible BIT            NOT NULL,
    FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId) ON DELETE NO ACTION,
    FOREIGN KEY (CourtId) REFERENCES Courts(CourtId) ON DELETE NO ACTION,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO
CREATE UNIQUE INDEX IX_Reviews_BookingId ON Reviews(BookingId);
GO
CREATE INDEX IX_Reviews_CourtId ON Reviews(CourtId);
GO
CREATE INDEX IX_Reviews_UserId ON Reviews(UserId);
GO

-- 26. TASKS
CREATE TABLE Tasks (
    TaskId          INT PRIMARY KEY IDENTITY(1,1),
    Title           NVARCHAR(150) NOT NULL,
    Description     NVARCHAR(500) NULL,
    TaskType        INT           NOT NULL, -- TaskType Enum
    Category        INT           NOT NULL, -- TaskCategory Enum
    Priority        INT           NOT NULL, -- TaskPriority Enum
    Status          INT           NOT NULL, -- TaskItemStatus Enum
    ComplexId       INT           NOT NULL,
    AssignedStaffId INT           NULL,
    CreatedById     INT           NULL,
    BookingId       INT           NULL,
    DueDate         DATETIME2     NOT NULL,
    CreatedAt       DATETIME2     NOT NULL,
    CompletedAt     DATETIME2     NULL,
    FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId) ON DELETE NO ACTION,
    FOREIGN KEY (ComplexId) REFERENCES CourtComplexes(ComplexId) ON DELETE CASCADE,
    FOREIGN KEY (AssignedStaffId) REFERENCES Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO
CREATE INDEX IX_Tasks_AssignedStaffId ON Tasks(AssignedStaffId);
GO
CREATE INDEX IX_Tasks_BookingId ON Tasks(BookingId);
GO
CREATE INDEX IX_Tasks_ComplexId ON Tasks(ComplexId);
GO
CREATE INDEX IX_Tasks_CreatedById ON Tasks(CreatedById);
GO

-- 27. INVOICES
CREATE TABLE Invoices (
    InvoiceId      INT PRIMARY KEY IDENTITY(1,1),
    InvoiceNumber  NVARCHAR(50)  NOT NULL,
    BookingId      INT           NOT NULL,
    PaymentId      INT           NOT NULL,
    SubTotal       DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL,
    VatPercent     DECIMAL(18,2) NOT NULL,
    VatAmount      DECIMAL(18,2) NOT NULL,
    TotalAmount    DECIMAL(18,2) NOT NULL,
    PdfUrl         NVARCHAR(500) NULL,
    IsEmailSent    BIT           NOT NULL,
    CreatedAt      DATETIME2     NOT NULL,
    FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId) ON DELETE NO ACTION,
    FOREIGN KEY (PaymentId) REFERENCES Payments(PaymentId) ON DELETE NO ACTION
);
GO
CREATE UNIQUE INDEX IX_Invoices_BookingId ON Invoices(BookingId);
GO
CREATE UNIQUE INDEX IX_Invoices_InvoiceNumber ON Invoices(InvoiceNumber);
GO
CREATE INDEX IX_Invoices_PaymentId ON Invoices(PaymentId);
GO

-- 28. PLAYER REQUEST MEMBERS
CREATE TABLE PlayerRequestMembers (
    PlayerRequestMemberId INT PRIMARY KEY IDENTITY(1,1),
    RequestId             INT       NOT NULL,
    UserId                INT       NOT NULL,
    Status                INT       NOT NULL, -- MemberRequestStatus Enum
    JoinedAt              DATETIME2 NOT NULL,
    FOREIGN KEY (RequestId) REFERENCES PlayerRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO
CREATE UNIQUE INDEX IX_PlayerRequestMembers_RequestId_UserId ON PlayerRequestMembers(RequestId, UserId);
GO
CREATE INDEX IX_PlayerRequestMembers_UserId ON PlayerRequestMembers(UserId);
GO


-- =============================================
-- INSERT SEED DATA (Realistic Mock Data)
-- =============================================

-- 1. Roles
INSERT INTO Roles (RoleName, Description) VALUES
(N'Admin', N'Quản trị viên toàn hệ thống'),
(N'Staff', N'Nhân viên điều hành, quản lý sân và lịch trình'),
(N'Coach', N'Huấn luyện viên chuyên môn hỗ trợ học viên'),
(N'Customer', N'Khách hàng đặt sân và dịch vụ');

-- 2. Membership Tiers
INSERT INTO MembershipTiers (TierName, MinPoints, DiscountPercent) VALUES
(N'Bronze', 0, 0.00),
(N'Silver', 500, 5.00),
(N'Gold', 2000, 10.00),
(N'Platinum', 5000, 15.00);

-- 3. Promotions
INSERT INTO Promotions (PromoCode, PromoName, DiscountType, DiscountValue, StartDate, EndDate) VALUES
(N'WELCOME10', N'Khuyến mãi chào mừng thành viên mới', 0, 10.00, '2026-01-01 00:00:00', '2026-12-31 23:59:59'),
(N'SUMMER26', N'Khuyến mãi hè rực rỡ 2026', 0, 20.00, '2026-06-01 00:00:00', '2026-08-31 23:59:59'),
(N'GIAM50K', N'Mã giảm giá trực tiếp 50,000 VNĐ', 1, 50000.00, '2026-05-01 00:00:00', '2026-07-31 23:59:59'),
(N'MIDYEAR15', N'Ưu đãi giữa năm cực hot', 0, 15.00, '2026-06-15 00:00:00', '2026-07-15 23:59:59');

-- 4. Court Types
INSERT INTO CourtTypes (TypeName, IsActive) VALUES
(N'Cầu lông', 1),
(N'Bóng đá', 1),
(N'Pickleball', 1),
(N'Tennis', 1),
(N'Bóng rổ', 1);

-- 5. Time Slots
INSERT INTO TimeSlots (SlotName, StartTime, EndTime, DayType) VALUES
(N'Sáng sớm (K01)', '05:00:00', '07:00:00', 0),
(N'Buổi sáng (K02)', '07:00:00', '11:00:00', 0),
(N'Buổi trưa (K03)', '11:00:00', '13:00:00', 0),
(N'Buổi chiều (K04)', '13:00:00', '17:00:00', 0),
(N'Giờ vàng tối (K05)', '17:00:00', '21:00:00', 0),
(N'Tối muộn (K06)', '21:00:00', '23:00:00', 0),
(N'Cuối tuần sáng (C01)', '06:00:00', '12:00:00', 1),
(N'Cuối tuần chiều (C02)', '12:00:00', '18:00:00', 1),
(N'Cuối tuần tối (C03)', '18:00:00', '23:00:00', 1);

-- 6. Services
INSERT INTO Services (ServiceName, Category, Price, StockQty) VALUES
(N'Thuê vợt cầu lông Yonex', N'Equipment', 30000.00, 20),
(N'Hộp bóng cầu lông (12 quả)', N'Equipment', 150000.00, 50),
(N'Thuê vợt Pickleball cao cấp', N'Equipment', 40000.00, 15),
(N'Thuê bóng Pickleball (set 3 quả)', N'Equipment', 30000.00, 100),
(N'Nước suối Aquafina 500ml', N'Drink', 10000.00, 200),
(N'Nước điện giải Revive 500ml', N'Drink', 15000.00, 150),
(N'Thuê giày thể thao Lining', N'Equipment', 25000.00, 30),
(N'Thuê bóng rổ Molten', N'Equipment', 35000.00, 10),
(N'Dịch vụ HLV Cầu lông cơ bản', N'Coach', 150000.00, 999),
(N'Dịch vụ HLV Pickleball nâng cao', N'Coach', 250000.00, 999);

-- 7. Users
-- Note: Password is 'Password@123' for all seeded users.
-- The hash below is a valid BCrypt hash for 'Password@123'
DECLARE @DefaultHash NVARCHAR(255) = N'$2a$12$K.zM93V65pLh/p5N7Fp4ZexUleGvPvx79K7mJcK.mJ68Q7F1.n1.a';

INSERT INTO Users (FullName, Email, Phone, PasswordHash, AvatarUrl, LoyaltyPoints, MembershipTierId, RefreshToken, IsActive, Gender, SkillLevel, CreatedAt) VALUES
(N'System Admin', N'admin@sportscourt.com', N'0901234567', @DefaultHash, N'https://i.pravatar.cc/150?img=33', 5500, 4, NULL, 1, 0, 2, '2026-01-15 08:30:00'),
(N'Nguyễn Hoàng Nam (Staff)', N'nam.staff@sportscourt.com', N'0912345678', @DefaultHash, N'https://i.pravatar.cc/150?img=12', 150, 1, NULL, 1, 0, 1, '2026-02-10 09:15:00'),
(N'Trần Quốc Bảo (Coach)', N'bao.coach@sportscourt.com', N'0923456789', @DefaultHash, N'https://i.pravatar.cc/150?img=60', 1200, 2, NULL, 1, 0, 2, '2026-03-01 14:00:00'),
(N'Lê Thị Mai (Coach)', N'mai.coach@sportscourt.com', N'0934567890', @DefaultHash, N'https://i.pravatar.cc/150?img=47', 800, 2, NULL, 1, 1, 2, '2026-03-05 10:20:00'),
(N'Phạm Minh Đức', N'duc.customer@gmail.com', N'0945678901', @DefaultHash, N'https://i.pravatar.cc/150?img=11', 2500, 3, NULL, 1, 0, 1, '2026-04-12 16:45:00'),
(N'Hoàng Thúy Vy', N'vy.customer@gmail.com', N'0956789012', @DefaultHash, N'https://i.pravatar.cc/150?img=5', 600, 2, NULL, 1, 1, 0, '2026-05-01 11:10:00'),
(N'Đỗ Hữu Hùng', N'hung.customer@gmail.com', N'0967890123', @DefaultHash, N'https://i.pravatar.cc/150?img=15', 50, 1, NULL, 1, 0, 0, '2026-06-01 08:00:00'),
(N'Vũ Thị Hồng', N'hong.customer@gmail.com', N'0978901234', @DefaultHash, N'https://i.pravatar.cc/150?img=48', 3100, 3, NULL, 1, 1, 2, '2026-06-10 13:22:00');

-- 8. User Roles
INSERT INTO UserRoles (UserId, RoleId) VALUES
(1, 1), -- Admin
(2, 2), -- Staff
(3, 3), -- Coach
(4, 3), -- Coach
(5, 4), -- Customer
(6, 4), -- Customer
(7, 4), -- Customer
(8, 4); -- Customer

-- 9. Court Complexes
INSERT INTO CourtComplexes (ComplexName, Address, ManagerId, Description, ImageUrl, IsDeleted, CreatedAt) VALUES
(N'Tổ hợp Thể thao Phú Thọ Q11', N'219 Lý Thường Kiệt, Phường 15, Quận 11, TP. HCM', 2, N'Khu liên hợp thể thao quy mô lớn với đầy đủ sân cầu lông, bóng đá cỏ nhân tạo và sân pickleball trong nhà hiện đại.', N'https://images.unsplash.com/photo-1545224497-5d24f378a74f?q=80', 0, '2026-04-01 07:00:00'),
(N'Nhà Thi Đấu Nguyễn Du Quận 1', N'116 Nguyễn Du, Phường Bến Thành, Quận 1, TP. HCM', 2, N'Vị trí trung tâm Quận 1, sàn thi đấu chuẩn quốc tế, phù hợp cho các buổi giao lưu chất lượng cao và giải đấu phong trào.', N'https://images.unsplash.com/photo-1518063319789-7217e6706b04?q=80', 0, '2026-04-10 07:00:00');

-- 10. Courts
INSERT INTO Courts (CourtName, CourtCode, CourtTypeId, ComplexId, Status, OpenTime, CloseTime, PricePerHour, CourtSize, IsDeleted) VALUES
(N'Sân Cầu Lông A1', N'PT-CL-A1', 1, 1, 0, '05:00:00', '23:00:00', 80000.00, N'13.4m x 6.1m', 0),
(N'Sân Cầu Lông A2', N'PT-CL-A2', 1, 1, 0, '05:00:00', '23:00:00', 80000.00, N'13.4m x 6.1m', 0),
(N'Sân Cầu Lông A3', N'PT-CL-A3', 1, 1, 3, '05:00:00', '23:00:00', 80000.00, N'13.4m x 6.1m', 0), -- Maintenance
(N'Sân Bóng Đá Mini F1', N'PT-BD-F1', 2, 1, 0, '06:00:00', '22:00:00', 350000.00, N'40m x 20m (Sân 5)', 0),
(N'Sân Pickleball P1', N'ND-PK-P1', 3, 2, 0, '05:00:00', '22:00:00', 120000.00, N'13.4m x 6.1m', 0),
(N'Sân Pickleball P2', N'ND-PK-P2', 3, 2, 0, '05:00:00', '22:00:00', 120000.00, N'13.4m x 6.1m', 0),
(N'Sân Tennis Standard T1', N'ND-TN-T1', 4, 2, 0, '06:00:00', '22:00:00', 220000.00, N'23.77m x 10.97m', 0),
(N'Sân Bóng Rổ B1', N'PT-BR-B1', 5, 1, 0, '06:00:00', '22:00:00', 150000.00, N'28m x 15m', 0);

-- 11. Court Images
INSERT INTO CourtImages (CourtId, ImageUrl, IsPrimary) VALUES
(1, N'https://images.unsplash.com/photo-1626224583764-f87db24ac4ea?q=80', 1),
(2, N'https://images.unsplash.com/photo-1626224583764-f87db24ac4ea?q=80', 1),
(3, N'https://images.unsplash.com/photo-1599447421416-3414500d18a5?q=80', 1),
(4, N'https://images.unsplash.com/photo-1508098682722-e99c43a406b2?q=80', 1),
(5, N'https://images.unsplash.com/photo-1595435934249-5df7ed86e1c0?q=80', 1),
(6, N'https://images.unsplash.com/photo-1595435934249-5df7ed86e1c0?q=80', 1),
(7, N'https://images.unsplash.com/photo-1622279457486-62dcc4a431d6?q=80', 1),
(8, N'https://images.unsplash.com/photo-1546519638-68e109498ffc?q=80', 1);

-- 12. Court Pricing
INSERT INTO CourtPricing (CourtId, SlotId, Price, EffectiveFrom) VALUES
-- Sân Cầu Lông A1: Các Slot
(1, 1, 60000.00, '2026-04-15 00:00:00'),
(1, 2, 80000.00, '2026-04-15 00:00:00'),
(1, 3, 70000.00, '2026-04-15 00:00:00'),
(1, 4, 80000.00, '2026-04-15 00:00:00'),
(1, 5, 120000.00, '2026-04-15 00:00:00'), -- Giờ vàng
(1, 6, 90000.00, '2026-04-15 00:00:00'),
-- Sân Cầu Lông A2
(2, 1, 60000.00, '2026-04-15 00:00:00'),
(2, 2, 80000.00, '2026-04-15 00:00:00'),
(2, 5, 120000.00, '2026-04-15 00:00:00'),
-- Sân Bóng Đá Mini F1
(4, 2, 300000.00, '2026-04-15 00:00:00'),
(4, 5, 450000.00, '2026-04-15 00:00:00'),
(4, 8, 400000.00, '2026-04-15 00:00:00'),
-- Sân Pickleball P1
(5, 2, 120000.00, '2026-04-15 00:00:00'),
(5, 5, 180000.00, '2026-04-15 00:00:00'),
(5, 8, 160000.00, '2026-04-15 00:00:00'),
-- Sân Tennis Standard T1
(7, 2, 200000.00, '2026-04-15 00:00:00'),
(7, 5, 300000.00, '2026-04-15 00:00:00'),
(7, 9, 280000.00, '2026-04-15 00:00:00');

-- 13. Equipment Inventory
INSERT INTO EquipmentInventory (ServiceId, ItemCode, Condition, PurchaseDate, PurchasePrice, IsAvailable) VALUES
(1, N'YONEX-001', 0, '2026-01-10 00:00:00', 1200000.00, 1),
(1, N'YONEX-002', 0, '2026-01-10 00:00:00', 1200000.00, 1),
(1, N'YONEX-003', 1, '2026-01-10 00:00:00', 1200000.00, 0), -- Damaged
(3, N'PKL-PAD-01', 0, '2026-04-05 00:00:00', 800000.00, 1),
(3, N'PKL-PAD-02', 0, '2026-04-05 00:00:00', 800000.00, 1),
(7, N'SHOE-40-01', 0, '2026-02-15 00:00:00', 950000.00, 1),
(7, N'SHOE-42-01', 0, '2026-02-15 00:00:00', 950000.00, 1),
(8, N'BB-MOLTEN-01', 0, '2026-03-20 00:00:00', 650000.00, 1);

-- 14. Staff Shifts
INSERT INTO StaffShifts (StaffId, ShiftDate, ShiftType, StartTime, EndTime, CheckInTime, CheckOutTime) VALUES
(2, '2026-06-28 00:00:00', 0, '06:00:00', '14:00:00', '2026-06-28 05:55:00', '2026-06-28 14:05:00'),
(2, '2026-06-29 00:00:00', 1, '14:00:00', '22:00:00', NULL, NULL),
(2, '2026-06-30 00:00:00', 0, '06:00:00', '14:00:00', NULL, NULL);

-- 15. Coach Schedules
INSERT INTO CoachSchedules (CoachId, CourtId, SlotId, ScheduleDate, Price, IsBooked) VALUES
(3, 1, 2, '2026-06-29 00:00:00', 150000.00, 0),
(3, 1, 5, '2026-06-29 00:00:00', 200000.00, 1), -- Booked
(4, 5, 2, '2026-06-29 00:00:00', 250000.00, 0),
(4, 5, 5, '2026-06-30 00:00:00', 250000.00, 0);

-- 16. Recurring Bookings
INSERT INTO RecurringBookings (UserId, CourtId, SlotId, StartDate, EndDate, DaysOfWeek, Status) VALUES
(5, 1, 5, '2026-06-01 00:00:00', '2026-08-31 00:00:00', N'2,4,6', 0), -- Mon, Wed, Fri
(8, 7, 9, '2026-06-01 00:00:00', '2026-07-31 00:00:00', N'7,8', 0); -- Sat, Sun

-- 17. Bookings
-- Pending=0, Confirmed=1, Cancelled=2, Completed=3, NoShow=4
INSERT INTO Bookings (BookingCode, UserId, CourtId, SlotId, BookingDate, StartTime, EndTime, SubTotal, DiscountAmount, TotalAmount, Status, PromotionId) VALUES
(N'BK-20260628001', 5, 1, 5, '2026-06-28 00:00:00', '17:00:00', '21:00:00', 120000.00, 12000.00, 108000.00, 1, 1), -- Confirmed
(N'BK-20260628002', 6, 5, 5, '2026-06-28 00:00:00', '17:00:00', '21:00:00', 180000.00, 0.00, 180000.00, 3, NULL), -- Completed
(N'BK-20260628003', 7, 4, 2, '2026-06-28 00:00:00', '07:00:00', '11:00:00', 300000.00, 50000.00, 250000.00, 2, 3), -- Cancelled
(N'BK-20260629001', 8, 7, 2, '2026-06-29 00:00:00', '07:00:00', '11:00:00', 200000.00, 30000.00, 170000.00, 0, 4); -- Pending

-- 18. Booking Services
INSERT INTO BookingServices (BookingId, ServiceId, Quantity, TotalPrice) VALUES
(1, 1, 2, 60000.00),  -- Thuê vợt cầu lông
(1, 5, 4, 40000.00),  -- Nước suối
(2, 3, 2, 80000.00),  -- Thuê vợt pickleball
(2, 6, 2, 30000.00);  -- Nước bù khoáng

-- 19. Payments
-- Pending=0, Success=1, Failed=2, Refunded=3
INSERT INTO Payments (BookingId, Amount, PaymentMethod, TransactionId, GatewayResponse, Status, RefundAmount, PaidAt) VALUES
(1, 108000.00, 0, N'VNPAY20260628001', N'{"RspCode":"00","Message":"Confirm Success"}', 1, 0.00, '2026-06-28 10:10:00'),
(2, 180000.00, 1, N'MOMO20260628002', N'{"errorCode":0,"message":"Successful"}', 1, 0.00, '2026-06-28 15:30:00'),
(3, 250000.00, 2, N'BANK20260628003', N'Customer requested cancel', 3, 250000.00, '2026-06-28 08:20:00'),
(4, 170000.00, 0, N'VNPAY20260629001', NULL, 0, 0.00, NULL);

-- 20. Invoices
INSERT INTO Invoices (InvoiceNumber, BookingId, PaymentId, SubTotal, DiscountAmount, VatPercent, VatAmount, TotalAmount, PdfUrl, IsEmailSent, CreatedAt) VALUES
(N'INV-260628001', 1, 1, 120000.00, 12000.00, 10.00, 10800.00, 118800.00, N'https://sportscourt.storage/invoices/inv-260628001.pdf', 1, '2026-06-28 10:15:00'),
(N'INV-260628002', 2, 2, 180000.00, 0.00, 10.00, 18000.00, 198000.00, N'https://sportscourt.storage/invoices/inv-260628002.pdf', 1, '2026-06-28 15:35:00');

-- 21. Reviews
INSERT INTO Reviews (BookingId, UserId, CourtId, Rating, Comment, IsVisible) VALUES
(2, 6, 5, 5, N'Sân pickleball Nguyễn Du mặt sơn rất đẹp và nảy chuẩn. Đèn chiếu sáng ban đêm cực tốt, dịch vụ chuyên nghiệp!', 1);

-- 22. Waitlists
-- Waiting=0, Notified=1, Confirmed=2, Expired=3, Cancelled=4
INSERT INTO Waitlists (UserId, CourtId, SlotId, WaitDate, Position, Status, NotifiedAt, ExpiredAt) VALUES
(5, 5, 5, '2026-07-01 00:00:00', 1, 0, NULL, NULL),
(6, 5, 5, '2026-07-01 00:00:00', 2, 0, NULL, NULL);

-- 23. Notifications
-- BookingConfirm=0, BookingCancel=1, PaymentSuccess=2, PaymentFail=3, Reminder=4, Promotion=5, Waitlist=6, System=7
INSERT INTO Notifications (UserId, Title, Type, IsRead, CreatedAt) VALUES
(5, N'Đơn đặt sân BK-20260628001 của bạn đã được xác nhận!', 0, 1, '2026-06-28 10:10:05'),
(5, N'Hóa đơn thanh toán của bạn đã được gửi qua email.', 2, 0, '2026-06-28 10:15:02'),
(7, N'Hủy sân thành công! Số tiền hoàn trả đã được xử lý về tài khoản của bạn.', 1, 1, '2026-06-28 12:45:00');

-- 24. Maintenance Schedules
-- Scheduled=0, InProgress=1, Completed=2, Cancelled=3
INSERT INTO MaintenanceSchedules (CourtId, MaintenanceType, StartDateTime, EndDateTime, AssignedStaffId, Reason, Result, Status) VALUES
(3, 0, '2026-06-30 08:00:00', '2026-06-30 17:00:00', 2, N'Bảo trì định kỳ thảm trải sàn và thay mới lưới bị rách.', NULL, 0);

-- 25. Player Requests
-- SkillLevel Enum: Beginner=0, Intermediate=1, Advanced=2
-- Gender Enum: Male=0, Female=1, Any=2 (Other=2)
-- PlayerRequestStatus Enum: Open=0, Full=1, Closed=2, Cancelled=3
INSERT INTO PlayerRequests (BookingId, HostUserId, SkillLevel, RequiredPlayers, GenderPref, Status) VALUES
(1, 5, 1, 2, 2, 0), -- Host Đức cần thêm 2 người chơi trình độ trung bình
(2, 6, 0, 1, 1, 1); -- Host Vy cần thêm 1 bạn nữ chơi cùng, đã đủ người

-- 26. PlayerRequestMembers
-- MemberRequestStatus Enum: Pending=0, Accepted=1, Rejected=2
INSERT INTO PlayerRequestMembers (RequestId, UserId, Status, JoinedAt) VALUES
(1, 7, 0, '2026-06-28 11:00:00'), -- Hùng xin tham gia group của Đức (Pending)
(2, 8, 1, '2026-06-28 16:00:00'); -- Hồng tham gia group của Vy (Accepted)

-- 27. Tasks
-- TaskType: Manual=0
-- TaskCategory: Cleanup=0, ServicePrep=1, Repair=2, Complaint=3
-- TaskPriority: Urgent=0, High=1, Medium=2, Low=3
-- TaskItemStatus: Pending=0, InProgress=1, Completed=2, Approved=3
INSERT INTO Tasks (Title, Description, TaskType, Category, Priority, Status, ComplexId, AssignedStaffId, CreatedById, BookingId, DueDate, CreatedAt, CompletedAt) VALUES
(N'Lau dọn sân cầu lông A1', N'Dọn dẹp chai nước suối cũ và lau sạch mồ hôi trên sàn đấu sau ca chơi tối.', 0, 0, 2, 2, 1, 2, 1, 1, '2026-06-28 21:15:00', '2026-06-28 21:00:00', '2026-06-28 21:10:00'),
(N'Chuẩn bị vợt thuê Pickleball cho ca sáng', N'Căng lại dây và lau cán vợt của set vợt PKL-PAD-01 và 02.', 0, 1, 2, 0, 2, 2, 1, NULL, '2026-06-29 07:00:00', '2026-06-28 22:00:00', NULL);

-- 28. Audit Logs
INSERT INTO AuditLogs (UserId, Action, TableName, Timestamp, Details) VALUES
(1, N'Insert User', N'Users', '2026-06-28 08:35:00', N'Admin added new staff user: Nguyễn Hoàng Nam'),
(5, N'Create Booking', N'Bookings', '2026-06-28 10:09:50', N'Customer created booking code BK-20260628001');

PRINT '=== PRN232_SCM_DB v3.0 created successfully! (28 Tables + Real-world Seed Data) ===';
GO
