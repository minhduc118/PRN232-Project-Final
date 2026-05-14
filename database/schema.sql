-- =============================================
-- Sports Court Management System - SQL Server
-- Version: 2.0 | PRN232 | 26 Tables
-- =============================================

USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'SportsCourtDB')
    DROP DATABASE SportsCourtDB;
GO
CREATE DATABASE SportsCourtDB COLLATE Vietnamese_CI_AS;
GO
USE SportsCourtDB;
GO

-- 1. ROLES
CREATE TABLE Roles (
    RoleId      INT PRIMARY KEY IDENTITY(1,1),
    RoleName    NVARCHAR(50)  NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL,
    CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- 2. MEMBERSHIP TIERS
CREATE TABLE MembershipTiers (
    TierId          INT PRIMARY KEY IDENTITY(1,1),
    TierName        NVARCHAR(50)   NOT NULL UNIQUE,
    MinPoints       INT            NOT NULL DEFAULT 0,
    DiscountPercent DECIMAL(5,2)   NOT NULL DEFAULT 0,
    Description     NVARCHAR(300)  NULL,
    CreatedAt       DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

-- 3. USERS
CREATE TABLE Users (
    UserId             INT PRIMARY KEY IDENTITY(1,1),
    FullName           NVARCHAR(100) NOT NULL,
    Email              VARCHAR(100)  NOT NULL UNIQUE,
    Phone              VARCHAR(15)   NULL,
    PasswordHash       VARCHAR(255)  NOT NULL,
    AvatarUrl          VARCHAR(500)  NULL,
    DateOfBirth        DATE          NULL,
    Gender             VARCHAR(10)   NULL CHECK (Gender IN ('Male','Female','Other')),
    LoyaltyPoints      INT           NOT NULL DEFAULT 0,
    MembershipTierId   INT           NULL,
    IsActive           BIT           NOT NULL DEFAULT 1,
    IsEmailVerified    BIT           NOT NULL DEFAULT 0,
    RefreshToken       VARCHAR(500)  NULL,
    RefreshTokenExpiry DATETIME      NULL,
    CreatedAt          DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdatedAt          DATETIME      NULL,
    FOREIGN KEY (MembershipTierId) REFERENCES MembershipTiers(TierId)
);
GO

-- 4. USER ROLES
CREATE TABLE UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId     INT NOT NULL,
    RoleId     INT NOT NULL,
    AssignedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    UNIQUE (UserId, RoleId)
);
GO

-- 5. COURT TYPES
CREATE TABLE CourtTypes (
    CourtTypeId INT PRIMARY KEY IDENTITY(1,1),
    TypeName    NVARCHAR(100) NOT NULL UNIQUE,
    IconUrl     VARCHAR(500)  NULL,
    Description NVARCHAR(300) NULL,
    IsActive    BIT           NOT NULL DEFAULT 1,
    CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- 6. COURTS
CREATE TABLE Courts (
    CourtId     INT PRIMARY KEY IDENTITY(1,1),
    CourtName   NVARCHAR(100)  NOT NULL,
    CourtCode   VARCHAR(20)    NOT NULL UNIQUE,
    CourtTypeId INT            NOT NULL,
    Description NVARCHAR(1000) NULL,
    Location    NVARCHAR(300)  NULL,
    Capacity    INT            NULL,
    Surface     NVARCHAR(100)  NULL,
    ImageUrl    VARCHAR(500)   NULL,
    OpenTime    TIME           NOT NULL DEFAULT '06:00',
    CloseTime   TIME           NOT NULL DEFAULT '22:00',
    Status      VARCHAR(20)    NOT NULL DEFAULT 'Available'
                CHECK (Status IN ('Available','Booked','InUse','Maintenance','Inactive')),
    CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt   DATETIME       NULL,
    FOREIGN KEY (CourtTypeId) REFERENCES CourtTypes(CourtTypeId)
);
GO

-- 7. COURT IMAGES
CREATE TABLE CourtImages (
    ImageId   INT PRIMARY KEY IDENTITY(1,1),
    CourtId   INT          NOT NULL,
    ImageUrl  VARCHAR(500) NOT NULL,
    IsPrimary BIT          NOT NULL DEFAULT 0,
    SortOrder INT          NOT NULL DEFAULT 0,
    CreatedAt DATETIME     NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CourtId) REFERENCES Courts(CourtId) ON DELETE CASCADE
);
GO

-- 8. TIME SLOTS
CREATE TABLE TimeSlots (
    SlotId    INT PRIMARY KEY IDENTITY(1,1),
    SlotName  NVARCHAR(50) NOT NULL,
    StartTime TIME         NOT NULL,
    EndTime   TIME         NOT NULL,
    DayType   VARCHAR(20)  NOT NULL DEFAULT 'Weekday'
              CHECK (DayType IN ('Weekday','Weekend','Holiday')),
    IsActive  BIT          NOT NULL DEFAULT 1
);
GO

-- 9. COURT PRICING
CREATE TABLE CourtPricing (
    PricingId     INT PRIMARY KEY IDENTITY(1,1),
    CourtId       INT           NOT NULL,
    SlotId        INT           NOT NULL,
    Price         DECIMAL(18,2) NOT NULL,
    PeakMultiplier DECIMAL(4,2) NOT NULL DEFAULT 1.0,
    EffectiveFrom DATE          NOT NULL DEFAULT GETDATE(),
    EffectiveTo   DATE          NULL,
    CreatedAt     DATETIME      NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CourtId) REFERENCES Courts(CourtId),
    FOREIGN KEY (SlotId)  REFERENCES TimeSlots(SlotId)
);
GO

-- 10. PROMOTIONS
CREATE TABLE Promotions (
    PromotionId    INT PRIMARY KEY IDENTITY(1,1),
    PromoCode      VARCHAR(50)   NOT NULL UNIQUE,
    PromoName      NVARCHAR(200) NOT NULL,
    Description    NVARCHAR(500) NULL,
    DiscountType   VARCHAR(20)   NOT NULL CHECK (DiscountType IN ('Percent','FixedAmount')),
    DiscountValue  DECIMAL(18,2) NOT NULL,
    MinOrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaxDiscount    DECIMAL(18,2) NULL,
    UsageLimit     INT           NULL,
    UsedCount      INT           NOT NULL DEFAULT 0,
    StartDate      DATETIME      NOT NULL,
    EndDate        DATETIME      NOT NULL,
    IsActive       BIT           NOT NULL DEFAULT 1,
    CreatedAt      DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- 11. RECURRING BOOKINGS
CREATE TABLE RecurringBookings (
    RecurringId   INT PRIMARY KEY IDENTITY(1,1),
    UserId        INT          NOT NULL,
    CourtId       INT          NOT NULL,
    SlotId        INT          NOT NULL,
    StartDate     DATE         NOT NULL,
    EndDate       DATE         NOT NULL,
    DaysOfWeek    VARCHAR(20)  NOT NULL, -- '1,3,5' = Mon,Wed,Fri
    StartTime     TIME         NOT NULL,
    EndTime       TIME         NOT NULL,
    TotalSessions INT          NOT NULL DEFAULT 0,
    Status        VARCHAR(20)  NOT NULL DEFAULT 'Active'
                  CHECK (Status IN ('Active','Paused','Cancelled','Completed')),
    CreatedAt     DATETIME     NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId)  REFERENCES Users(UserId),
    FOREIGN KEY (CourtId) REFERENCES Courts(CourtId),
    FOREIGN KEY (SlotId)  REFERENCES TimeSlots(SlotId)
);
GO

-- 12. BOOKINGS
CREATE TABLE Bookings (
    BookingId      INT PRIMARY KEY IDENTITY(1,1),
    BookingCode    VARCHAR(20)   NOT NULL UNIQUE,
    UserId         INT           NOT NULL,
    CourtId        INT           NOT NULL,
    SlotId         INT           NOT NULL,
    RecurringId    INT           NULL,
    BookingDate    DATE          NOT NULL,
    StartTime      TIME          NOT NULL,
    EndTime        TIME          NOT NULL,
    SubTotal       DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount    DECIMAL(18,2) NOT NULL,
    PromotionId    INT           NULL,
    Status         VARCHAR(30)   NOT NULL DEFAULT 'Pending'
                   CHECK (Status IN ('Pending','Confirmed','Cancelled','Completed','NoShow')),
    CancelReason   NVARCHAR(500) NULL,
    CancelledAt    DATETIME      NULL,
    Note           NVARCHAR(500) NULL,
    CreatedAt      DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdatedAt      DATETIME      NULL,
    FOREIGN KEY (UserId)       REFERENCES Users(UserId),
    FOREIGN KEY (CourtId)      REFERENCES Courts(CourtId),
    FOREIGN KEY (SlotId)       REFERENCES TimeSlots(SlotId),
    FOREIGN KEY (RecurringId)  REFERENCES RecurringBookings(RecurringId),
    FOREIGN KEY (PromotionId)  REFERENCES Promotions(PromotionId)
);
GO

-- 13. WAITLISTS
CREATE TABLE Waitlists (
    WaitlistId INT PRIMARY KEY IDENTITY(1,1),
    UserId     INT         NOT NULL,
    CourtId    INT         NOT NULL,
    SlotId     INT         NOT NULL,
    WaitDate   DATE        NOT NULL,
    Position   INT         NOT NULL,
    Status     VARCHAR(20) NOT NULL DEFAULT 'Waiting'
               CHECK (Status IN ('Waiting','Notified','Confirmed','Expired','Cancelled')),
    NotifiedAt DATETIME    NULL,
    ExpiredAt  DATETIME    NULL,
    CreatedAt  DATETIME    NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId)  REFERENCES Users(UserId),
    FOREIGN KEY (CourtId) REFERENCES Courts(CourtId),
    FOREIGN KEY (SlotId)  REFERENCES TimeSlots(SlotId),
    UNIQUE (UserId, CourtId, SlotId, WaitDate)
);
GO

-- 14. SERVICES
CREATE TABLE Services (
    ServiceId   INT PRIMARY KEY IDENTITY(1,1),
    ServiceName NVARCHAR(100) NOT NULL,
    Category    NVARCHAR(50)  NOT NULL,
    Price       DECIMAL(18,2) NOT NULL,
    Unit        NVARCHAR(30)  NOT NULL DEFAULT N'cái',
    Description NVARCHAR(300) NULL,
    ImageUrl    VARCHAR(500)  NULL,
    MinStock    INT           NOT NULL DEFAULT 0,
    IsActive    BIT           NOT NULL DEFAULT 1,
    CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- 15. EQUIPMENT INVENTORY
CREATE TABLE EquipmentInventory (
    InventoryId   INT PRIMARY KEY IDENTITY(1,1),
    ServiceId     INT           NOT NULL,
    ItemCode      VARCHAR(50)   NOT NULL UNIQUE,
    Condition     VARCHAR(20)   NOT NULL DEFAULT 'Good'
                  CHECK (Condition IN ('Good','Damaged','Retired')),
    PurchaseDate  DATE          NULL,
    PurchasePrice DECIMAL(18,2) NULL,
    Note          NVARCHAR(300) NULL,
    IsAvailable   BIT           NOT NULL DEFAULT 1,
    CreatedAt     DATETIME      NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId)
);
GO

-- 16. BOOKING SERVICES
CREATE TABLE BookingServices (
    BookingServiceId INT PRIMARY KEY IDENTITY(1,1),
    BookingId        INT           NOT NULL,
    ServiceId        INT           NOT NULL,
    Quantity         INT           NOT NULL DEFAULT 1,
    UnitPrice        DECIMAL(18,2) NOT NULL,
    TotalPrice       DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(ServiceId)
);
GO

-- 17. PAYMENTS
CREATE TABLE Payments (
    PaymentId       INT PRIMARY KEY IDENTITY(1,1),
    BookingId       INT           NOT NULL,
    Amount          DECIMAL(18,2) NOT NULL,
    PaymentMethod   VARCHAR(50)   NOT NULL
                    CHECK (PaymentMethod IN ('VNPay','MoMo','BankTransfer','Cash','Wallet')),
    TransactionId   VARCHAR(200)  NULL UNIQUE,
    GatewayResponse NVARCHAR(MAX) NULL,
    Status          VARCHAR(20)   NOT NULL DEFAULT 'Pending'
                    CHECK (Status IN ('Pending','Success','Failed','Refunded','PartialRefund')),
    RefundAmount    DECIMAL(18,2) NOT NULL DEFAULT 0,
    RefundedAt      DATETIME      NULL,
    RefundNote      NVARCHAR(300) NULL,
    PaidAt          DATETIME      NULL,
    CreatedAt       DATETIME      NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId)
);
GO

-- 18. INVOICES
CREATE TABLE Invoices (
    InvoiceId     INT PRIMARY KEY IDENTITY(1,1),
    InvoiceNumber VARCHAR(30)   NOT NULL UNIQUE,
    BookingId     INT           NOT NULL,
    PaymentId     INT           NOT NULL,
    SubTotal      DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    VatPercent    DECIMAL(5,2)  NOT NULL DEFAULT 0,
    VatAmount     DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalAmount   DECIMAL(18,2) NOT NULL,
    PdfUrl        VARCHAR(500)  NULL,
    IsEmailSent   BIT           NOT NULL DEFAULT 0,
    EmailSentAt   DATETIME      NULL,
    CreatedAt     DATETIME      NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId),
    FOREIGN KEY (PaymentId) REFERENCES Payments(PaymentId)
);
GO

-- 19. REVIEWS
CREATE TABLE Reviews (
    ReviewId   INT PRIMARY KEY IDENTITY(1,1),
    BookingId  INT            NOT NULL UNIQUE,
    UserId     INT            NOT NULL,
    CourtId    INT            NOT NULL,
    Rating     TINYINT        NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment    NVARCHAR(1000) NULL,
    ImageUrl   VARCHAR(500)   NULL,
    IsVisible  BIT            NOT NULL DEFAULT 1,
    AdminReply NVARCHAR(500)  NULL,
    RepliedAt  DATETIME       NULL,
    CreatedAt  DATETIME       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId),
    FOREIGN KEY (UserId)    REFERENCES Users(UserId),
    FOREIGN KEY (CourtId)   REFERENCES Courts(CourtId)
);
GO

-- 20. NOTIFICATIONS
CREATE TABLE Notifications (
    NotificationId INT PRIMARY KEY IDENTITY(1,1),
    UserId         INT            NOT NULL,
    Title          NVARCHAR(200)  NOT NULL,
    Body           NVARCHAR(1000) NOT NULL,
    Type           VARCHAR(50)    NOT NULL
                   CHECK (Type IN ('BookingConfirm','BookingCancel','PaymentSuccess',
                                   'PaymentFail','Reminder','Promotion','Waitlist','System')),
    ReferenceId    INT            NULL,
    IsRead         BIT            NOT NULL DEFAULT 0,
    ReadAt         DATETIME       NULL,
    CreatedAt      DATETIME       NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
GO

-- 21. MAINTENANCE SCHEDULES
CREATE TABLE MaintenanceSchedules (
    MaintenanceId   INT PRIMARY KEY IDENTITY(1,1),
    CourtId         INT           NOT NULL,
    MaintenanceType VARCHAR(30)   NOT NULL
                    CHECK (MaintenanceType IN ('Routine','Emergency','Upgrade')),
    StartDateTime   DATETIME      NOT NULL,
    EndDateTime     DATETIME      NOT NULL,
    AssignedStaffId INT           NULL,
    Reason          NVARCHAR(500) NOT NULL,
    Result          NVARCHAR(500) NULL,
    Status          VARCHAR(20)   NOT NULL DEFAULT 'Scheduled'
                    CHECK (Status IN ('Scheduled','InProgress','Completed','Cancelled')),
    CreatedAt       DATETIME      NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CourtId)         REFERENCES Courts(CourtId),
    FOREIGN KEY (AssignedStaffId) REFERENCES Users(UserId)
);
GO

-- 22. STAFF SHIFTS
CREATE TABLE StaffShifts (
    ShiftId      INT PRIMARY KEY IDENTITY(1,1),
    StaffId      INT          NOT NULL,
    ShiftDate    DATE         NOT NULL,
    ShiftType    VARCHAR(20)  NOT NULL
                 CHECK (ShiftType IN ('Morning','Afternoon','Evening')),
    StartTime    TIME         NOT NULL,
    EndTime      TIME         NOT NULL,
    CheckInTime  DATETIME     NULL,
    CheckOutTime DATETIME     NULL,
    Note         NVARCHAR(300) NULL,
    CreatedAt    DATETIME     NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (StaffId) REFERENCES Users(UserId),
    UNIQUE (StaffId, ShiftDate, ShiftType)
);
GO

-- 23. COACH SCHEDULES
CREATE TABLE CoachSchedules (
    ScheduleId   INT PRIMARY KEY IDENTITY(1,1),
    CoachId      INT           NOT NULL,
    CourtId      INT           NOT NULL,
    SlotId       INT           NOT NULL,
    ScheduleDate DATE          NOT NULL,
    MaxStudents  INT           NOT NULL DEFAULT 1,
    Price        DECIMAL(18,2) NOT NULL,
    Note         NVARCHAR(300) NULL,
    IsBooked     BIT           NOT NULL DEFAULT 0,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CoachId)  REFERENCES Users(UserId),
    FOREIGN KEY (CourtId)  REFERENCES Courts(CourtId),
    FOREIGN KEY (SlotId)   REFERENCES TimeSlots(SlotId),
    UNIQUE (CoachId, CourtId, SlotId, ScheduleDate)
);
GO

-- 24. PLAYER REQUESTS
CREATE TABLE PlayerRequests (
    RequestId       INT PRIMARY KEY IDENTITY(1,1),
    BookingId       INT           NOT NULL,
    HostUserId      INT           NOT NULL,
    SkillLevel      VARCHAR(20)   NOT NULL DEFAULT 'Beginner'
                    CHECK (SkillLevel IN ('Beginner','Intermediate','Advanced')),
    RequiredPlayers INT           NOT NULL DEFAULT 1,
    GenderPref      VARCHAR(10)   NULL CHECK (GenderPref IN ('Male','Female','Any')),
    AgeMin          INT           NULL,
    AgeMax          INT           NULL,
    Description     NVARCHAR(500) NULL,
    Status          VARCHAR(20)   NOT NULL DEFAULT 'Open'
                    CHECK (Status IN ('Open','Full','Closed','Cancelled')),
    ExpiresAt       DATETIME      NULL,
    CreatedAt       DATETIME      NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (BookingId)  REFERENCES Bookings(BookingId),
    FOREIGN KEY (HostUserId) REFERENCES Users(UserId)
);
GO

-- 25. PLAYER REQUEST MEMBERS
CREATE TABLE PlayerRequestMembers (
    MemberId  INT PRIMARY KEY IDENTITY(1,1),
    RequestId INT         NOT NULL,
    UserId    INT         NOT NULL,
    Status    VARCHAR(20) NOT NULL DEFAULT 'Pending'
              CHECK (Status IN ('Pending','Accepted','Rejected')),
    JoinedAt  DATETIME    NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (RequestId) REFERENCES PlayerRequests(RequestId) ON DELETE CASCADE,
    FOREIGN KEY (UserId)    REFERENCES Users(UserId),
    UNIQUE (RequestId, UserId)
);
GO

-- 26. AUDIT LOGS
CREATE TABLE AuditLogs (
    LogId     INT PRIMARY KEY IDENTITY(1,1),
    UserId    INT           NULL,
    Action    VARCHAR(100)  NOT NULL,
    TableName VARCHAR(100)  NOT NULL,
    RecordId  INT           NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    IpAddress VARCHAR(50)   NULL,
    CreatedAt DATETIME      NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- =============================================
-- INDEXES
-- =============================================
CREATE INDEX IX_Users_Email                ON Users(Email);
CREATE INDEX IX_Bookings_UserId            ON Bookings(UserId);
CREATE INDEX IX_Bookings_CourtId           ON Bookings(CourtId);
CREATE INDEX IX_Bookings_BookingDate       ON Bookings(BookingDate);
CREATE INDEX IX_Bookings_Status            ON Bookings(Status);
CREATE INDEX IX_Bookings_RecurringId       ON Bookings(RecurringId);
CREATE INDEX IX_Payments_BookingId         ON Payments(BookingId);
CREATE INDEX IX_Payments_Status            ON Payments(Status);
CREATE INDEX IX_Reviews_CourtId            ON Reviews(CourtId);
CREATE INDEX IX_Notifications_UserId       ON Notifications(UserId, IsRead);
CREATE INDEX IX_Courts_Status              ON Courts(Status);
CREATE INDEX IX_RecurringBookings_UserId   ON RecurringBookings(UserId);
CREATE INDEX IX_Waitlists_CourtSlotDate    ON Waitlists(CourtId, SlotId, WaitDate);
CREATE INDEX IX_MaintenanceSchedules_Court ON MaintenanceSchedules(CourtId, Status);
CREATE INDEX IX_StaffShifts_StaffDate      ON StaffShifts(StaffId, ShiftDate);
CREATE INDEX IX_PlayerRequests_Status      ON PlayerRequests(Status);
GO

-- =============================================
-- VIEWS
-- =============================================

CREATE VIEW vw_BookingDetails AS
SELECT
    b.BookingId, b.BookingCode,
    u.FullName AS CustomerName, u.Email, u.Phone,
    c.CourtName, c.CourtCode, ct.TypeName AS CourtType,
    b.BookingDate, b.StartTime, b.EndTime,
    b.SubTotal, b.DiscountAmount, b.TotalAmount,
    b.Status AS BookingStatus,
    p.PaymentMethod, p.Status AS PaymentStatus, p.PaidAt,
    i.InvoiceNumber, i.PdfUrl,
    b.CreatedAt
FROM Bookings b
    JOIN Users u       ON b.UserId  = u.UserId
    JOIN Courts c      ON b.CourtId = c.CourtId
    JOIN CourtTypes ct ON c.CourtTypeId = ct.CourtTypeId
    LEFT JOIN Payments p  ON b.BookingId = p.BookingId
    LEFT JOIN Invoices i  ON b.BookingId = i.BookingId;
GO

CREATE VIEW vw_DailyRevenue AS
SELECT
    CAST(b.BookingDate AS DATE) AS RevenueDate,
    ct.TypeName AS CourtType,
    COUNT(b.BookingId) AS TotalBookings,
    SUM(b.TotalAmount) AS TotalRevenue,
    SUM(CASE WHEN b.Status='Completed' THEN b.TotalAmount ELSE 0 END) AS ConfirmedRevenue
FROM Bookings b
    JOIN Courts c      ON b.CourtId = c.CourtId
    JOIN CourtTypes ct ON c.CourtTypeId = ct.CourtTypeId
WHERE b.Status != 'Cancelled'
GROUP BY CAST(b.BookingDate AS DATE), ct.TypeName;
GO

CREATE VIEW vw_CourtRatings AS
SELECT
    c.CourtId, c.CourtName, c.CourtCode,
    COUNT(r.ReviewId) AS TotalReviews,
    AVG(CAST(r.Rating AS DECIMAL(3,2))) AS AverageRating
FROM Courts c
    LEFT JOIN Reviews r ON c.CourtId = r.CourtId AND r.IsVisible = 1
GROUP BY c.CourtId, c.CourtName, c.CourtCode;
GO

CREATE VIEW vw_WaitlistQueue AS
SELECT
    w.WaitlistId, w.Position,
    u.FullName AS CustomerName, u.Email, u.Phone,
    c.CourtName, ts.SlotName, ts.StartTime, ts.EndTime,
    w.WaitDate, w.Status, w.NotifiedAt, w.ExpiredAt
FROM Waitlists w
    JOIN Users u      ON w.UserId  = u.UserId
    JOIN Courts c     ON w.CourtId = c.CourtId
    JOIN TimeSlots ts ON w.SlotId  = ts.SlotId;
GO

CREATE VIEW vw_EquipmentStock AS
SELECT
    s.ServiceId, s.ServiceName, s.Category,
    COUNT(e.InventoryId)                                     AS TotalItems,
    SUM(CASE WHEN e.IsAvailable=1 AND e.Condition='Good' THEN 1 ELSE 0 END) AS AvailableItems,
    SUM(CASE WHEN e.Condition='Damaged' THEN 1 ELSE 0 END)  AS DamagedItems,
    s.MinStock,
    CASE WHEN SUM(CASE WHEN e.IsAvailable=1 AND e.Condition='Good' THEN 1 ELSE 0 END) <= s.MinStock
         THEN 1 ELSE 0 END AS IsLowStock
FROM Services s
    LEFT JOIN EquipmentInventory e ON s.ServiceId = e.ServiceId
GROUP BY s.ServiceId, s.ServiceName, s.Category, s.MinStock;
GO

-- =============================================
-- STORED PROCEDURES
-- =============================================

CREATE PROCEDURE sp_CheckCourtAvailability
    @CourtId INT, @BookingDate DATE, @StartTime TIME, @EndTime TIME
AS
BEGIN
    SELECT COUNT(*) AS ConflictCount
    FROM Bookings
    WHERE CourtId = @CourtId
      AND BookingDate = @BookingDate
      AND Status NOT IN ('Cancelled')
      AND StartTime < @EndTime AND EndTime > @StartTime;
END;
GO

CREATE PROCEDURE sp_CalculateRefund
    @BookingId INT,
    @RefundPercent DECIMAL(5,2) OUTPUT
AS
BEGIN
    DECLARE @BookingDate DATE, @StartTime TIME, @HoursDiff FLOAT;
    SELECT @BookingDate = BookingDate, @StartTime = StartTime
    FROM Bookings WHERE BookingId = @BookingId;
    SET @HoursDiff = DATEDIFF(HOUR, GETDATE(),
        CAST(CAST(@BookingDate AS VARCHAR) + ' ' + CAST(@StartTime AS VARCHAR) AS DATETIME));
    SET @RefundPercent = CASE
        WHEN @HoursDiff >= 24 THEN 100
        WHEN @HoursDiff >= 12 THEN 50
        ELSE 0 END;
END;
GO

CREATE PROCEDURE sp_ProcessWaitlist
    @CourtId INT, @SlotId INT, @BookingDate DATE
AS
BEGIN
    DECLARE @NextUserId INT, @WaitlistId INT;
    SELECT TOP 1 @WaitlistId = WaitlistId, @NextUserId = UserId
    FROM Waitlists
    WHERE CourtId=@CourtId AND SlotId=@SlotId AND WaitDate=@BookingDate AND Status='Waiting'
    ORDER BY Position ASC;

    IF @NextUserId IS NOT NULL
    BEGIN
        UPDATE Waitlists
        SET Status='Notified', NotifiedAt=GETDATE(), ExpiredAt=DATEADD(MINUTE,15,GETDATE())
        WHERE WaitlistId=@WaitlistId;

        INSERT INTO Notifications(UserId, Title, Body, Type, ReferenceId)
        VALUES (@NextUserId,
                N'Sân trống - Cơ hội đặt sân!',
                N'Sân bạn chờ đã có chỗ trống. Xác nhận trong 15 phút.',
                'Waitlist', @WaitlistId);
    END;
END;
GO

-- =============================================
-- SEED DATA
-- =============================================

INSERT INTO Roles (RoleName, Description) VALUES
    ('Admin',    N'Quản trị toàn bộ hệ thống'),
    ('Staff',    N'Nhân viên hỗ trợ vận hành'),
    ('Coach',    N'Huấn luyện viên thể thao'),
    ('Customer', N'Khách hàng đặt sân');

INSERT INTO MembershipTiers (TierName, MinPoints, DiscountPercent, Description) VALUES
    ('Bronze',   0,    0,  N'Thành viên cơ bản'),
    ('Silver',   500,  5,  N'Giảm 5% mỗi booking'),
    ('Gold',     2000, 10, N'Giảm 10% + ưu tiên đặt sân'),
    ('Platinum', 5000, 15, N'Giảm 15% + dịch vụ VIP');

INSERT INTO CourtTypes (TypeName, Description) VALUES
    (N'Cầu lông',   N'Sân cầu lông tiêu chuẩn BWF'),
    (N'Bóng đá',    N'Sân bóng đá mini 5v5 / 7v7'),
    (N'Pickleball', N'Sân pickleball tiêu chuẩn'),
    (N'Tennis',     N'Sân tennis mặt cứng / đất nện'),
    (N'Bóng rổ',    N'Sân bóng rổ 3x3 / 5v5');

INSERT INTO TimeSlots (SlotName, StartTime, EndTime, DayType) VALUES
    (N'Sáng sớm',        '05:00','07:00','Weekday'),
    (N'Buổi sáng',       '07:00','11:00','Weekday'),
    (N'Buổi trưa',       '11:00','13:00','Weekday'),
    (N'Buổi chiều',      '13:00','17:00','Weekday'),
    (N'Giờ vàng',        '17:00','21:00','Weekday'),
    (N'Tối muộn',        '21:00','23:00','Weekday'),
    (N'Cuối tuần sáng',  '06:00','12:00','Weekend'),
    (N'Cuối tuần chiều', '12:00','18:00','Weekend'),
    (N'Cuối tuần tối',   '18:00','23:00','Weekend');

-- Admin (password: Admin@123)
INSERT INTO Users (FullName,Email,Phone,PasswordHash,IsActive,IsEmailVerified,MembershipTierId)
VALUES (N'System Admin','admin@sportscourtms.vn','0900000001','$2a$12$adminHash',1,1,4);
INSERT INTO UserRoles(UserId,RoleId) VALUES (1,1);

-- Staff
INSERT INTO Users (FullName,Email,Phone,PasswordHash,IsActive,IsEmailVerified,MembershipTierId)
VALUES (N'Nguyễn Văn An','staff@sportscourtms.vn','0900000002','$2a$12$staffHash',1,1,1);
INSERT INTO UserRoles(UserId,RoleId) VALUES (2,2);

-- Coach
INSERT INTO Users (FullName,Email,Phone,PasswordHash,IsActive,IsEmailVerified,MembershipTierId)
VALUES (N'Trần Thị Bình','coach@sportscourtms.vn','0900000003','$2a$12$coachHash',1,1,2);
INSERT INTO UserRoles(UserId,RoleId) VALUES (3,3);

-- Customer demo
INSERT INTO Users (FullName,Email,Phone,PasswordHash,IsActive,IsEmailVerified,MembershipTierId)
VALUES (N'Lê Văn Cường','customer@gmail.com','0912345678','$2a$12$customerHash',1,1,1);
INSERT INTO UserRoles(UserId,RoleId) VALUES (4,4);

INSERT INTO Courts (CourtName,CourtCode,CourtTypeId,Description,Location,Capacity,Surface,OpenTime,CloseTime) VALUES
    (N'Sân Cầu Lông A1','CL-A1',1,N'Sân cầu lông tiêu chuẩn, sàn gỗ, điều hòa',N'Tầng 1 Khu A',4,N'Gỗ','06:00','22:00'),
    (N'Sân Cầu Lông A2','CL-A2',1,N'Sân cầu lông tiêu chuẩn, sàn nhựa PVC',N'Tầng 1 Khu A',4,N'Nhựa PVC','06:00','22:00'),
    (N'Sân Bóng Đá B1', 'BD-B1',2,N'Sân 5v5 cỏ nhân tạo thế hệ 3',N'Ngoài trời Khu B',10,N'Cỏ nhân tạo','06:00','22:00'),
    (N'Sân Pickleball C1','PK-C1',3,N'Sân pickleball tiêu chuẩn',N'Tầng 2 Khu C',4,N'Nhựa','06:00','22:00'),
    (N'Sân Tennis D1',  'TN-D1',4,N'Sân mặt cứng, đèn cao áp',N'Ngoài trời Khu D',4,N'Mặt cứng','06:00','22:00');

-- Pricing: Cầu lông A1
INSERT INTO CourtPricing (CourtId,SlotId,Price,PeakMultiplier) VALUES
    (1,1,80000,1.0),(1,2,100000,1.0),(1,3,90000,1.0),
    (1,4,100000,1.0),(1,5,150000,1.5),(1,6,120000,1.2);

-- Pricing: Bóng đá B1
INSERT INTO CourtPricing (CourtId,SlotId,Price,PeakMultiplier) VALUES
    (3,2,300000,1.0),(3,4,300000,1.0),(3,5,500000,1.5),
    (3,7,400000,1.2),(3,8,400000,1.2),(3,9,600000,1.5);

INSERT INTO Services (ServiceName,Category,Price,Unit,Description,MinStock) VALUES
    (N'Thuê vợt cầu lông',   'Equipment',30000, N'cây/giờ',N'Vợt Yonex tiêu chuẩn',5),
    (N'Thuê bóng cầu lông',  'Equipment',10000, N'ống',    N'Hộp 12 quả',10),
    (N'Thuê giày thể thao',  'Equipment',20000, N'đôi/giờ',N'Size 36-44',8),
    (N'Nước suối',           'Drink',    10000, N'chai',   N'Aquafina 500ml',20),
    (N'Nước tăng lực',       'Drink',    20000, N'chai',   N'Redbull/Sting',15),
    (N'Huấn luyện cơ bản',   'Coach',    200000,N'buổi',  N'1 giờ với HLV cơ bản',0),
    (N'Huấn luyện nâng cao', 'Coach',    400000,N'buổi',  N'1 giờ với HLV chuyên nghiệp',0),
    (N'Tổ chức giải đấu',    'Event',    2000000,N'lần',  N'Trọn gói tổ chức giải',0);

-- Equipment inventory samples
INSERT INTO EquipmentInventory (ServiceId,ItemCode,Condition,PurchaseDate,PurchasePrice) VALUES
    (1,'VOT-001','Good','2026-01-01',500000),
    (1,'VOT-002','Good','2026-01-01',500000),
    (1,'VOT-003','Damaged','2026-01-01',500000),
    (2,'BONG-001','Good','2026-01-01',150000),
    (2,'BONG-002','Good','2026-01-01',150000),
    (3,'GIAY-001','Good','2026-01-15',300000),
    (3,'GIAY-002','Good','2026-01-15',300000);

INSERT INTO Promotions (PromoCode,PromoName,DiscountType,DiscountValue,MinOrderAmount,StartDate,EndDate) VALUES
    ('WELCOME10',N'Chào mừng thành viên mới','Percent',    10,     0,     '2026-01-01','2026-12-31'),
    ('SUMMER20', N'Khuyến mãi hè 2026',      'Percent',    20,     200000,'2026-06-01','2026-08-31'),
    ('FIXED50K', N'Giảm 50k đơn từ 300k',   'FixedAmount',50000,  300000,'2026-05-01','2026-07-31');

-- Staff shifts
INSERT INTO StaffShifts (StaffId,ShiftDate,ShiftType,StartTime,EndTime) VALUES
    (2,'2026-05-14','Morning',  '06:00','14:00'),
    (2,'2026-05-15','Afternoon','14:00','22:00'),
    (2,'2026-05-16','Morning',  '06:00','14:00');

PRINT '=== SportsCourtDB v2.0 created successfully! (26 tables) ===';
GO
