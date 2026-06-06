-- ===================================================================
-- SQL SEED DATA SCRIPT - SPORT COURT MANAGEMENT SYSTEM
-- ===================================================================

USE [SportsCourtDB]; -- Ensure you are using the correct database name
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Safe, re-runnable clean up phase (deletes existing records in dependency order)
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DELETE FROM [dbo].[AuditLogs];
IF OBJECT_ID('dbo.PlayerRequestMembers', 'U') IS NOT NULL DELETE FROM [dbo].[PlayerRequestMembers];
IF OBJECT_ID('dbo.PlayerRequests', 'U') IS NOT NULL DELETE FROM [dbo].[PlayerRequests];
IF OBJECT_ID('dbo.CoachSchedules', 'U') IS NOT NULL DELETE FROM [dbo].[CoachSchedules];
IF OBJECT_ID('dbo.StaffShifts', 'U') IS NOT NULL DELETE FROM [dbo].[StaffShifts];
IF OBJECT_ID('dbo.MaintenanceSchedules', 'U') IS NOT NULL DELETE FROM [dbo].[MaintenanceSchedules];
IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL DELETE FROM [dbo].[Notifications];
IF OBJECT_ID('dbo.Reviews', 'U') IS NOT NULL DELETE FROM [dbo].[Reviews];
IF OBJECT_ID('dbo.Invoices', 'U') IS NOT NULL DELETE FROM [dbo].[Invoices];
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DELETE FROM [dbo].[Payments];
IF OBJECT_ID('dbo.BookingServices', 'U') IS NOT NULL DELETE FROM [dbo].[BookingServices];
IF OBJECT_ID('dbo.EquipmentInventory', 'U') IS NOT NULL DELETE FROM [dbo].[EquipmentInventory];
IF OBJECT_ID('dbo.Waitlists', 'U') IS NOT NULL DELETE FROM [dbo].[Waitlists];
IF OBJECT_ID('dbo.Bookings', 'U') IS NOT NULL DELETE FROM [dbo].[Bookings];
IF OBJECT_ID('dbo.RecurringBookings', 'U') IS NOT NULL DELETE FROM [dbo].[RecurringBookings];
IF OBJECT_ID('dbo.CourtPricing', 'U') IS NOT NULL DELETE FROM [dbo].[CourtPricing];
IF OBJECT_ID('dbo.CourtImages', 'U') IS NOT NULL DELETE FROM [dbo].[CourtImages];
IF OBJECT_ID('dbo.Courts', 'U') IS NOT NULL DELETE FROM [dbo].[Courts];
IF OBJECT_ID('dbo.CourtComplexes', 'U') IS NOT NULL DELETE FROM [dbo].[CourtComplexes];
IF OBJECT_ID('dbo.UserRoles', 'U') IS NOT NULL DELETE FROM [dbo].[UserRoles];
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DELETE FROM [dbo].[Users];
IF OBJECT_ID('dbo.Services', 'U') IS NOT NULL DELETE FROM [dbo].[Services];
IF OBJECT_ID('dbo.Promotions', 'U') IS NOT NULL DELETE FROM [dbo].[Promotions];
IF OBJECT_ID('dbo.TimeSlots', 'U') IS NOT NULL DELETE FROM [dbo].[TimeSlots];
IF OBJECT_ID('dbo.CourtTypes', 'U') IS NOT NULL DELETE FROM [dbo].[CourtTypes];
IF OBJECT_ID('dbo.MembershipTiers', 'U') IS NOT NULL DELETE FROM [dbo].[MembershipTiers];
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL DELETE FROM [dbo].[Roles];
GO


-- 1. Insert Roles
SET IDENTITY_INSERT [Roles] ON;
INSERT INTO [Roles] ([RoleId], [RoleName], [Description], [CreatedAt]) VALUES
(1, 'Admin', N'Quản trị toàn bộ hệ thống', GETDATE()),
(2, 'Staff', N'Nhân viên hỗ trợ vận hành', GETDATE()),
(3, 'Coach', N'Huấn luyện viên thể thao', GETDATE()),
(4, 'Customer', N'Khách hàng đặt sân', GETDATE());
SET IDENTITY_INSERT [Roles] OFF;
GO

-- 2. Insert MembershipTiers
SET IDENTITY_INSERT [MembershipTiers] ON;
INSERT INTO [MembershipTiers] ([TierId], [TierName], [MinPoints], [DiscountPercent], [Description], [CreatedAt]) VALUES
(1, 'Bronze', 0, 0.00, N'Thành viên cơ bản', GETDATE()),
(2, 'Silver', 500, 5.00, N'Giảm 5% mỗi booking', GETDATE()),
(3, 'Gold', 2000, 10.00, N'Giảm 10% + ưu tiên đặt sân', GETDATE()),
(4, 'Platinum', 5000, 15.00, N'Giảm 15% + dịch vụ VIP', GETDATE());
SET IDENTITY_INSERT [MembershipTiers] OFF;
GO

-- 3. Insert CourtTypes
SET IDENTITY_INSERT [CourtTypes] ON;
INSERT INTO [CourtTypes] ([CourtTypeId], [TypeName], [Description], [IsActive], [CreatedAt]) VALUES
(1, N'Cầu lông', N'Sân cầu lông tiêu chuẩn BWF', 1, GETDATE()),
(2, N'Bóng đá', N'Sân bóng đá mini 5v5 / 7v7', 1, GETDATE()),
(3, N'Pickleball', N'Sân pickleball tiêu chuẩn', 1, GETDATE()),
(4, N'Tennis', N'Sân tennis mặt cứng / đất nện', 1, GETDATE()),
(5, N'Bóng rổ', N'Sân bóng rổ 3x3 / 5v5', 1, GETDATE());
SET IDENTITY_INSERT [CourtTypes] OFF;
GO

-- 4. Insert TimeSlots
SET IDENTITY_INSERT [TimeSlots] ON;
INSERT INTO [TimeSlots] ([SlotId], [SlotName], [StartTime], [EndTime], [DayType], [IsActive]) VALUES
(1, N'Sáng sớm', '05:00:00', '07:00:00', 'Weekday', 1),
(2, N'Buổi sáng', '07:00:00', '11:00:00', 'Weekday', 1),
(3, N'Buổi trưa', '11:00:00', '13:00:00', 'Weekday', 1),
(4, N'Buổi chiều', '13:00:00', '17:00:00', 'Weekday', 1),
(5, N'Giờ vàng', '17:00:00', '21:00:00', 'Weekday', 1),
(6, N'Tối muộn', '21:00:00', '23:00:00', 'Weekday', 1),
(7, N'Cuối tuần sáng', '06:00:00', '12:00:00', 'Weekend', 1),
(8, N'Cuối tuần chiều', '12:00:00', '18:00:00', 'Weekend', 1),
(9, N'Cuối tuần tối', '18:00:00', '23:00:00', 'Weekend', 1);
SET IDENTITY_INSERT [TimeSlots] OFF;
GO

-- 5. Insert Users (Passwords are BCrypt hashes of: Admin@123, Staff@123, Coach@123, Customer@123)
SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserId], [FullName], [Email], [Phone], [PasswordHash], [IsActive], [IsEmailVerified], [MembershipTierId], [CreatedAt], [LoyaltyPoints], [Gender]) VALUES
(1, 'System Admin', 'admin@sportscourtms.vn', '0900000001', '$2a$11$B3X9ngp8IGqoU2H3yEZhZ.66WJnUtRTu5pfwbfGw3h7TAbBE/8PCi', 1, 1, 4, GETDATE(), 0, 'Male'),
(2, 'Nguyễn Văn An', 'staff@sportscourtms.vn', '0900000002', '$2a$11$3bpecDWLBV.A7PedIsc9TOIFFOlEZuiHfr2TB9okofi1IUIsfQBpS', 1, 1, 1, GETDATE(), 0, 'Male'),
(3, 'Trần Thị Bình', 'coach@sportscourtms.vn', '0900000003', '$2a$11$j/0Dmeuu2TKr7ITShicISuR1VdpMlQSf1qIyHzV9ItuvVh/JWCa.a', 1, 1, 2, GETDATE(), 0, 'Female'),
(4, 'Lê Văn Cường', 'customer@gmail.com', '0912345678', '$2a$11$p2.A.zWVick1.hl/qpK75uzMPZQB34oyad054MbL01NwaUe2GBKjq', 1, 1, 1, GETDATE(), 0, 'Male');
SET IDENTITY_INSERT [Users] OFF;
GO

-- 6. Insert UserRoles
SET IDENTITY_INSERT [UserRoles] ON;
INSERT INTO [UserRoles] ([UserRoleId], [UserId], [RoleId], [AssignedAt]) VALUES
(1, 1, 1, GETDATE()), -- Admin
(2, 2, 2, GETDATE()), -- Staff
(3, 3, 3, GETDATE()), -- Coach
(4, 4, 4, GETDATE()); -- Customer
SET IDENTITY_INSERT [UserRoles] OFF;
GO

-- 7. Insert CourtComplexes (22 tổ hợp sân toàn Hà Nội)
SET IDENTITY_INSERT [CourtComplexes] ON;
INSERT INTO [CourtComplexes] ([ComplexId], [ComplexName], [Address], [Phone], [ManagerName], [ManagerId], [Description], [ImageUrl], [IsDeleted], [CreatedAt]) VALUES
(1,  N'Tổ hợp thể thao Cầu Giấy',         N'Dịch Vọng, Cầu Giấy, Hà Nội',           '0912345678', 'System Admin',   1, N'Tổ hợp thể thao hiện đại hàng đầu tại Cầu Giấy với các sân trong nhà điều hòa.', 'https://images.unsplash.com/photo-1545224497-5d750c673417?q=80&w=800', 0, GETDATE()),
(2,  N'Tổ hợp thể thao Thanh Xuân',        N'Nguyễn Trãi, Thanh Xuân, Hà Nội',        '0987654321', 'Nguyễn Văn An',  2, N'Khu phức hợp thể thao ngoài trời và trong nhà đa năng tại Thanh Xuân.',            'https://images.unsplash.com/photo-1526232761682-d26e03ac148e?q=80&w=800', 0, GETDATE()),
(3,  N'SportZone Đống Đa',                  N'Khâm Thiên, Đống Đa, Hà Nội',            '0901111222', 'System Admin',   1, N'Khu thể thao đa năng tại trung tâm Đống Đa, tiện đi lại.',                          'https://images.unsplash.com/photo-1574629810360-7efbbe195018?q=80&w=800', 0, GETDATE()),
(4,  N'ActiveHub Hoàng Mai',                N'Tam Trinh, Hoàng Mai, Hà Nội',            '0902222333', 'Nguyễn Văn An',  2, N'Chuỗi sân thể thao chất lượng cao phục vụ khu vực phía Nam Hà Nội.',                 'https://images.unsplash.com/photo-1567025822912-efefe9cf4ac4?q=80&w=800', 0, GETDATE()),
(5,  N'ProSport Hà Đông',                   N'Quang Trung, Hà Đông, Hà Nội',           '0903333444', 'System Admin',   1, N'Tổ hợp thể thao chuyên nghiệp tại Hà Đông, mở cửa từ 5h sáng.',                    'https://images.unsplash.com/photo-1515923256482-1c04580b477a?q=80&w=800', 0, GETDATE()),
(6,  N'Tổ hợp thể thao Long Biên',         N'Bồ Đề, Long Biên, Hà Nội',               '0904444555', 'Nguyễn Văn An',  2, N'Khu thể thao hiện đại nhất phía Đông Hà Nội.',                                      'https://images.unsplash.com/photo-1559056199-641a0ac8b55e?q=80&w=800', 0, GETDATE()),
(7,  N'FitArena Tây Hồ',                    N'Xuân La, Tây Hồ, Hà Nội',               '0905555666', 'System Admin',   1, N'Sân thể thao cao cấp ven Hồ Tây, view đẹp, không gian thoáng.',                     'https://images.unsplash.com/photo-1489944440615-453fc2b6a9a9?q=80&w=800', 0, GETDATE()),
(8,  N'SportMax Bắc Từ Liêm',              N'Cổ Nhuế, Bắc Từ Liêm, Hà Nội',          '0906666777', 'Nguyễn Văn An',  2, N'Khu thể thao khép kín diện tích lớn nhất quận Bắc Từ Liêm.',                        'https://images.unsplash.com/photo-1519861531473-9200262188bf?q=80&w=800', 0, GETDATE()),
(9,  N'Tổ hợp thể thao Nam Từ Liêm',       N'Trần Hữu Dực, Nam Từ Liêm, Hà Nội',      '0907777888', 'System Admin',   1, N'Tổ hợp thể thao hiện đại tại trung tâm phát triển mới Nam Từ Liêm.',                 'https://images.unsplash.com/photo-1590227531827-a70e50f21f16?q=80&w=800', 0, GETDATE()),
(10, N'GreenSport Gia Lâm',                 N'Ninh Hiệp, Gia Lâm, Hà Nội',            '0908888999', 'Nguyễn Văn An',  2, N'Sân thể thao sinh thái, không gian xanh mát tại huyện Gia Lâm.',                    'https://images.unsplash.com/photo-1591035897819-f4bdf739f446?q=80&w=800', 0, GETDATE()),
(11, N'EliteSport Hai Bà Trưng',            N'Trương Định, Hai Bà Trưng, Hà Nội',      '0911222333', 'System Admin',   1, N'Tổ hợp thể thao cao cấp tại quận Hai Bà Trưng với HLV chuyên nghiệp.',              'https://images.unsplash.com/photo-1535131749006-b7f58c99034b?q=80&w=800', 0, GETDATE()),
(12, N'CityPlay Hoàn Kiếm',                 N'Hàng Bài, Hoàn Kiếm, Hà Nội',           '0912333444', 'Nguyễn Văn An',  2, N'Sân tập thể thao ngay trung tâm phố cổ, phù hợp cho nhân viên văn phòng.',           'https://images.unsplash.com/photo-1551958219-acbc595b85e4?q=80&w=800', 0, GETDATE()),
(13, N'Tổ hợp thể thao Thanh Trì',         N'Linh Đàm, Thanh Trì, Hà Nội',           '0913444555', 'System Admin',   1, N'Khu liên hợp thể thao lớn nhất phía Nam Hà Nội, nhiều sân đa dạng.',                'https://images.unsplash.com/photo-1579758629938-03607ccdbaba?q=80&w=800', 0, GETDATE()),
(14, N'VentureSport Ba Đình',               N'Kim Mã, Ba Đình, Hà Nội',               '0914555666', 'Nguyễn Văn An',  2, N'Tổ hợp thể thao gần trung tâm Ba Đình, phục vụ cán bộ công chức.',                  'https://images.unsplash.com/photo-1519863512547-ab547415a8b8?q=80&w=800', 0, GETDATE()),
(15, N'MaxFit Đan Phượng',                  N'Tân Lập, Đan Phượng, Hà Nội',           '0915666777', 'System Admin',   1, N'Khu thể thao hiện đại nhất huyện Đan Phượng, diện tích rộng.',                       'https://images.unsplash.com/photo-1571902943202-507ec2618e8f?q=80&w=800', 0, GETDATE()),
(16, N'SportVillage Mê Linh',               N'Tiền Phong, Mê Linh, Hà Nội',           '0916777888', 'Nguyễn Văn An',  2, N'Tổ hợp thể thao kết hợp khu nghỉ dưỡng tại Mê Linh.',                               'https://images.unsplash.com/photo-1575650772416-eb9b0b72b29e?q=80&w=800', 0, GETDATE()),
(17, N'ActivePark Sóc Sơn',                 N'Minh Phú, Sóc Sơn, Hà Nội',            '0917888999', 'System Admin',   1, N'Khu thể thao ngoài trời lớn nhất huyện Sóc Sơn, gần sân bay Nội Bài.',             'https://images.unsplash.com/photo-1590488398561-f59d40adae5d?q=80&w=800', 0, GETDATE()),
(18, N'SportHub Thường Tín',                N'Văn Bình, Thường Tín, Hà Nội',          '0918999111', 'Nguyễn Văn An',  2, N'Tổ hợp thể thao tại huyện Thường Tín phục vụ cư dân ngoại thành.',                  'https://images.unsplash.com/photo-1570129477492-45c003edd2be?q=80&w=800', 0, GETDATE()),
(19, N'ProCourt Phúc Thọ',                  N'Vân Hà, Phúc Thọ, Hà Nội',             '0919111222', 'System Admin',   1, N'Sân thể thao chuyên nghiệp kết hợp hồ bơi tại huyện Phúc Thọ.',                    'https://images.unsplash.com/photo-1600548063393-c1e61dba8e6a?q=80&w=800', 0, GETDATE()),
(20, N'MegaFit Quốc Oai',                   N'Quốc Oai, Quốc Oai, Hà Nội',           '0920222333', 'Nguyễn Văn An',  2, N'Tổ hợp thể thao diện tích lớn nhất tại huyện Quốc Oai.',                            'https://images.unsplash.com/photo-1560272564-c83b66b1ad12?q=80&w=800', 0, GETDATE()),
(21, N'PowerZone Thạch Thất',               N'Thạch Thất, Thạch Thất, Hà Nội',        '0921333444', 'System Admin',   1, N'Tổ hợp thể thao hiện đại mới khai trương tại huyện Thạch Thất.',                    'https://images.unsplash.com/photo-1604313483578-7bb6aa51c5a8?q=80&w=800', 0, GETDATE()),
(22, N'SportCenter Ba Vì',                   N'Tản Đà, Ba Vì, Hà Nội',                '0922444555', 'Nguyễn Văn An',  2, N'Khu thể thao dã ngoại ven sông Đà, không khí trong lành tuyệt vời.',                 'https://images.unsplash.com/photo-1553692459-f8f49db820c2?q=80&w=800', 0, GETDATE());
SET IDENTITY_INSERT [CourtComplexes] OFF;
GO

-- 8. Insert Courts (35 sân, trải đều các loại, các trạng thái)
SET IDENTITY_INSERT [Courts] ON;
INSERT INTO [Courts] ([CourtId], [CourtName], [CourtCode], [CourtTypeId], [ComplexId], [PricePerHour], [CourtSize], [Description], [Location], [Capacity], [Surface], [OpenTime], [CloseTime], [Status], [IsDeleted], [CreatedAt]) VALUES
-- Complex 1 (Cầu Giấy): 2 Cầu lông + 1 Pickleball
(1,  N'Sân Cầu Lông A1',     'CL-A1',   1, 1, 100000.00, N'Tiêu chuẩn', N'Sân cầu lông tiêu chuẩn, sàn gỗ, điều hòa',         N'Tầng 1 Khu A', 4,  N'Gỗ',       '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
(2,  N'Sân Cầu Lông A2',     'CL-A2',   1, 1, 100000.00, N'Tiêu chuẩn', N'Sân cầu lông tiêu chuẩn, sàn nhựa PVC',              N'Tầng 1 Khu A', 4,  N'Nhựa PVC', '06:00:00', '22:00:00', 'Maintenance', 0, GETDATE()),
(3,  N'Sân Pickleball C1',   'PK-C1',   3, 1, 150000.00, N'Tiêu chuẩn', N'Sân pickleball tiêu chuẩn trong nhà điều hòa',       N'Tầng 2 Khu C', 4,  N'Nhựa',     '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
-- Complex 2 (Thanh Xuân): 1 Bóng đá + 1 Tennis
(4,  N'Sân Bóng Đá B1',      'BD-B1',   2, 2, 300000.00, N'Sân 5 người', N'Sân 5v5 cỏ nhân tạo thế hệ 3 cao cấp',            N'Ngoài trời Khu B', 10, N'Cỏ nhân tạo', '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
(5,  N'Sân Tennis D1',       'TN-D1',   4, 2, 250000.00, N'Tiêu chuẩn', N'Sân tennis mặt cứng, đèn cao áp',                   N'Ngoài trời Khu D', 4,  N'Mặt cứng', '06:00:00', '22:00:00', 'Inactive',    0, GETDATE()),
-- Complex 3 (Đống Đa): Cầu lông + Bóng rổ
(6,  N'Sân Cầu Lông DD1',    'CL-DD1',  1, 3, 90000.00,  N'Tiêu chuẩn', N'Sân cầu lông tại Đống Đa, giao thông thuận tiện',   N'Tầng 1',       4,  N'Gỗ',       '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
(7,  N'Sân Bóng Rổ DD1',     'BR-DD1',  5, 3, 120000.00, N'3x3',        N'Sân bóng rổ 3x3 trong nhà',                          N'Tầng 2',       6,  N'Nhựa',     '07:00:00', '21:00:00', 'Available',   0, GETDATE()),
-- Complex 4 (Hoàng Mai): Bóng đá lớn + Cầu lông
(8,  N'Sân Bóng Đá HM1',     'BD-HM1',  2, 4, 500000.00, N'Sân 7 người', N'Sân bóng đá 7v7 cỏ nhân tạo cao cấp',             N'Khu A Ngoài trời', 14, N'Cỏ nhân tạo', '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
(9,  N'Sân Cầu Lông HM1',    'CL-HM1',  1, 4, 95000.00,  N'Tiêu chuẩn', N'Sân cầu lông trong nhà điều hòa',                   N'Khu B',        4,  N'PVC',      '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
-- Complex 5 (Hà Đông): Tennis + Pickleball
(10, N'Sân Tennis HD1',       'TN-HD1',  4, 5, 220000.00, N'Tiêu chuẩn', N'Sân tennis đất nện kiểu Pháp',                      N'Khu A',        4,  N'Đất nện',  '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
(11, N'Sân Pickleball HD1',   'PK-HD1',  3, 5, 140000.00, N'Tiêu chuẩn', N'Sân pickleball ngoài trời có mái che',               N'Khu B',        4,  N'Nhựa',     '06:00:00', '22:00:00', 'Maintenance', 0, GETDATE()),
-- Complex 6 (Long Biên): Bóng đá + Tennis
(12, N'Sân Bóng Đá LB1',     'BD-LB1',  2, 6, 350000.00, N'Sân 5 người', N'Sân mini bóng đá 5v5 mới khai trương',             N'Khu A',        10, N'Cỏ nhân tạo', '05:00:00', '23:00:00', 'Available',   0, GETDATE()),
(13, N'Sân Tennis LB1',       'TN-LB1',  4, 6, 240000.00, N'Tiêu chuẩn', N'Sân tennis mặt cứng tiêu chuẩn',                    N'Khu B',        4,  N'Mặt cứng', '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
-- Complex 7 (Tây Hồ): Cầu lông + Pickleball
(14, N'Sân Cầu Lông TH1',    'CL-TH1',  1, 7, 110000.00, N'Tiêu chuẩn', N'Sân cầu lông view Hồ Tây tuyệt đẹp',               N'Tầng 1',       4,  N'Gỗ',       '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
(15, N'Sân Pickleball TH1',   'PK-TH1',  3, 7, 160000.00, N'Tiêu chuẩn', N'Sân pickleball cao cấp gần Hồ Tây',                 N'Tầng 2',       4,  N'Nhựa',     '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
-- Complex 8 (Bắc Từ Liêm): Bóng rổ + Cầu lông
(16, N'Sân Bóng Rổ BTL1',    'BR-BTL1', 5, 8, 130000.00, N'5v5',        N'Sân bóng rổ 5v5 trong nhà tiêu chuẩn',               N'Khu A',        10, N'Gỗ',       '07:00:00', '21:00:00', 'Available',   0, GETDATE()),
(17, N'Sân Cầu Lông BTL1',   'CL-BTL1', 1, 8, 95000.00,  N'Tiêu chuẩn', N'Sân cầu lông đôi, sàn nhựa PVC',                    N'Khu B',        4,  N'PVC',      '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
-- Complex 9 (Nam Từ Liêm): Bóng đá + Tennis
(18, N'Sân Bóng Đá NTL1',    'BD-NTL1', 2, 9, 420000.00, N'Sân 7 người', N'Sân 7v7 cỏ nhân tạo tại Nam Từ Liêm',             N'Khu A',        14, N'Cỏ nhân tạo', '05:30:00', '22:30:00', 'Available',   0, GETDATE()),
(19, N'Sân Tennis NTL1',      'TN-NTL1', 4, 9, 230000.00, N'Tiêu chuẩn', N'Sân tennis mặt cứng, chiếu sáng ban đêm',           N'Khu B',        4,  N'Mặt cứng', '06:00:00', '22:00:00', 'Maintenance', 0, GETDATE()),
-- Complex 10 (Gia Lâm): Cầu lông + Bóng rổ
(20, N'Sân Cầu Lông GL1',    'CL-GL1',  1, 10, 85000.00, N'Tiêu chuẩn', N'Sân cầu lông sinh thái không gian xanh mát',        N'Khu A',        4,  N'PVC',      '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
(21, N'Sân Bóng Rổ GL1',     'BR-GL1',  5, 10, 110000.00, N'3x3',       N'Sân bóng rổ ngoài trời',                             N'Khu B',        6,  N'Nhựa',     '06:00:00', '21:00:00', 'Available',   0, GETDATE()),
-- Complex 11 (Hai Bà Trưng): Tennis + Pickleball
(22, N'Sân Tennis HBT1',      'TN-HBT1', 4, 11, 260000.00, N'Tiêu chuẩn', N'Sân tennis đất nện cao cấp với HLV',              N'Khu A',        4,  N'Đất nện',  '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
(23, N'Sân Pickleball HBT1',  'PK-HBT1', 3, 11, 155000.00, N'Tiêu chuẩn', N'Sân pickleball trong nhà điều hòa',               N'Khu B',        4,  N'Nhựa',     '06:00:00', '22:00:00', 'Inactive',    0, GETDATE()),
-- Complex 12 (Hoàn Kiếm): Cầu lông (văn phòng)
(24, N'Sân Cầu Lông HK1',    'CL-HK1',  1, 12, 120000.00, N'Tiêu chuẩn', N'Sân cầu lông tại trung tâm Hoàn Kiếm',            N'Tầng 2',       4,  N'PVC',      '06:30:00', '21:00:00', 'Available',   0, GETDATE()),
-- Complex 13 (Thanh Trì): Bóng đá + Cầu lông
(25, N'Sân Bóng Đá TT1',     'BD-TT1',  2, 13, 280000.00, N'Sân 5 người', N'Sân 5v5 cỏ nhân tạo tại Thanh Trì',              N'Khu A',        10, N'Cỏ nhân tạo', '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
(26, N'Sân Cầu Lông TT1',    'CL-TT1',  1, 13, 80000.00,  N'Tiêu chuẩn', N'Sân cầu lông phục vụ khu dân cư Linh Đàm',        N'Khu B',        4,  N'Gỗ',       '06:00:00', '21:00:00', 'Available',   0, GETDATE()),
-- Complex 14 (Ba Đình): Tennis + Bóng rổ
(27, N'Sân Tennis BĐ1',       'TN-BD1',  4, 14, 270000.00, N'Tiêu chuẩn', N'Sân tennis cao cấp tại trung tâm Ba Đình',        N'Khu A',        4,  N'Mặt cứng', '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
(28, N'Sân Bóng Rổ BĐ1',     'BR-BD1',  5, 14, 125000.00, N'5v5',        N'Sân bóng rổ trong nhà tiêu chuẩn',                 N'Khu B',        10, N'Gỗ',       '07:00:00', '21:00:00', 'Available',   0, GETDATE()),
-- Complex 15 (Đan Phượng): Bóng đá 11 người
(29, N'Sân Bóng Đá DP1',     'BD-DP1',  2, 15, 800000.00, N'Sân 11 người', N'Sân bóng đá 11 người cỏ tự nhiên tại Đan Phượng', N'Khu A',      22, N'Cỏ tự nhiên', '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
-- Complex 16 (Mê Linh): Pickleball + Cầu lông
(30, N'Sân Pickleball ML1',   'PK-ML1',  3, 16, 140000.00, N'Tiêu chuẩn', N'Sân pickleball kết hợp khu nghỉ dưỡng',           N'Khu A',        4,  N'Nhựa',     '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
(31, N'Sân Cầu Lông ML1',    'CL-ML1',  1, 16, 90000.00,  N'Tiêu chuẩn', N'Sân cầu lông ngoài trời có mái che',               N'Khu B',        4,  N'PVC',      '06:00:00', '21:00:00', 'Maintenance', 0, GETDATE()),
-- Complex 17 (Sóc Sơn): Bóng đá ngoài trời
(32, N'Sân Bóng Đá SS1',     'BD-SS1',  2, 17, 200000.00, N'Sân 5 người', N'Sân bóng đá ngoài trời không khí trong lành',    N'Khu A',        10, N'Cỏ nhân tạo', '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
-- Complex 18 (Thường Tín): Tennis
(33, N'Sân Tennis ThT1',      'TN-THT1', 4, 18, 200000.00, N'Tiêu chuẩn', N'Sân tennis mặt cứng phục vụ huyện Thường Tín',   N'Khu A',        4,  N'Mặt cứng', '06:00:00', '21:00:00', 'Available',   0, GETDATE()),
-- Complex 19 (Phúc Thọ): Cầu lông + Bóng rổ
(34, N'Sân Cầu Lông PT1',    'CL-PT1',  1, 19, 85000.00,  N'Tiêu chuẩn', N'Sân cầu lông mới khai trương tại Phúc Thọ',       N'Khu A',        4,  N'PVC',      '06:00:00', '22:00:00', 'Available',   0, GETDATE()),
-- Complex 20 (Quốc Oai): Bóng đá
(35, N'Sân Bóng Đá QO1',     'BD-QO1',  2, 20, 250000.00, N'Sân 7 người', N'Sân bóng đá tại huyện Quốc Oai phục vụ cư dân', N'Khu A',        14, N'Cỏ nhân tạo', '06:00:00', '22:00:00', 'Available',   0, GETDATE());
SET IDENTITY_INSERT [Courts] OFF;
GO


-- 9. Insert CourtImages
SET IDENTITY_INSERT [CourtImages] ON;
INSERT INTO [CourtImages] ([ImageId], [CourtId], [ImageUrl], [IsPrimary], [SortOrder], [CreatedAt]) VALUES
(1, 1, 'https://images.unsplash.com/photo-1626224583764-f87db24ac4ea?q=80&w=600', 0, 1, GETDATE()),
(2, 1, 'https://images.unsplash.com/photo-1521412644187-c49fa049e84d?q=80&w=600', 0, 2, GETDATE()),
(3, 3, 'https://images.unsplash.com/photo-1459865264687-595d652de67e?q=80&w=600', 0, 1, GETDATE()),
(4, 3, 'https://images.unsplash.com/photo-1508098682722-e99c43a406b2?q=80&w=600', 0, 2, GETDATE());
SET IDENTITY_INSERT [CourtImages] OFF;
GO

-- 10. Insert CourtPricing
SET IDENTITY_INSERT [CourtPricing] ON;
INSERT INTO [CourtPricing] ([PricingId], [CourtId], [SlotId], [Price], [PeakMultiplier], [EffectiveFrom], [CreatedAt]) VALUES
(1, 1, 1, 80000.00, 1.00, CAST(GETDATE() AS DATE), GETDATE()),
(2, 1, 2, 100000.00, 1.00, CAST(GETDATE() AS DATE), GETDATE()),
(3, 1, 3, 90000.00, 1.00, CAST(GETDATE() AS DATE), GETDATE()),
(4, 1, 4, 100000.00, 1.00, CAST(GETDATE() AS DATE), GETDATE()),
(5, 1, 5, 150000.00, 1.50, CAST(GETDATE() AS DATE), GETDATE()),
(6, 1, 6, 120000.00, 1.20, CAST(GETDATE() AS DATE), GETDATE()),
(7, 3, 2, 300000.00, 1.00, CAST(GETDATE() AS DATE), GETDATE()),
(8, 3, 4, 300000.00, 1.00, CAST(GETDATE() AS DATE), GETDATE()),
(9, 3, 5, 500000.00, 1.50, CAST(GETDATE() AS DATE), GETDATE()),
(10, 3, 7, 400000.00, 1.20, CAST(GETDATE() AS DATE), GETDATE()),
(11, 3, 8, 400000.00, 1.20, CAST(GETDATE() AS DATE), GETDATE()),
(12, 3, 9, 600000.00, 1.50, CAST(GETDATE() AS DATE), GETDATE());
SET IDENTITY_INSERT [CourtPricing] OFF;
GO

-- 11. Insert Services
SET IDENTITY_INSERT [Services] ON;
INSERT INTO [Services] ([ServiceId], [ServiceName], [Category], [Price], [Unit], [Description], [MinStock], [IsActive], [CreatedAt]) VALUES
(1, N'Thuê vợt cầu lông', 'Equipment', 30000.00, N'cây/giờ', N'Vợt Yonex tiêu chuẩn', 5, 1, GETDATE()),
(2, N'Thuê bóng cầu lông', 'Equipment', 10000.00, N'ống', N'Hộp 12 quả', 10, 1, GETDATE()),
(3, N'Thuê giày thể thao', 'Equipment', 20000.00, N'đôi/giờ', N'Size 36-44', 8, 1, GETDATE()),
(4, N'Nước suối', 'Drink', 10000.00, N'chai', N'Aquafina 500ml', 20, 1, GETDATE()),
(5, N'Nước tăng lực', 'Drink', 20000.00, N'chai', N'Redbull/Sting', 15, 1, GETDATE()),
(6, N'Huấn luyện cơ bản', 'Coach', 200000.00, N'buổi', N'1 giờ với HLV cơ bản', 0, 1, GETDATE()),
(7, N'Huấn luyện nâng cao', 'Coach', 400000.00, N'buổi', N'1 giờ với HLV chuyên nghiệp', 0, 1, GETDATE()),
(8, N'Tổ chức giải đấu', 'Event', 2000000.00, N'lần', N'Trọn gói tổ chức giải', 0, 1, GETDATE());
SET IDENTITY_INSERT [Services] OFF;
GO

-- 12. Insert EquipmentInventory
SET IDENTITY_INSERT [EquipmentInventory] ON;
INSERT INTO [EquipmentInventory] ([InventoryId], [ServiceId], [ItemCode], [Condition], [PurchaseDate], [PurchasePrice], [IsAvailable], [CreatedAt]) VALUES
(1, 1, 'VOT-001', 'Good', '2026-01-01', 500000.00, 1, GETDATE()),
(2, 1, 'VOT-002', 'Good', '2026-01-01', 500000.00, 1, GETDATE()),
(3, 1, 'VOT-003', 'Damaged', '2026-01-01', 500000.00, 0, GETDATE()),
(4, 2, 'BONG-001', 'Good', '2026-01-01', 150000.00, 1, GETDATE()),
(5, 2, 'BONG-002', 'Good', '2026-01-01', 150000.00, 1, GETDATE()),
(6, 3, 'GIAY-001', 'Good', '2026-01-15', 300000.00, 1, GETDATE()),
(7, 3, 'GIAY-002', 'Good', '2026-01-15', 300000.00, 1, GETDATE());
SET IDENTITY_INSERT [EquipmentInventory] OFF;
GO

-- 13. Insert Promotions
SET IDENTITY_INSERT [Promotions] ON;
INSERT INTO [Promotions] ([PromotionId], [PromoCode], [PromoName], [DiscountType], [DiscountValue], [MinOrderAmount], [StartDate], [EndDate], [IsActive], [CreatedAt], [UsedCount]) VALUES
(1, 'WELCOME10', N'Chào mừng thành viên mới', 'Percent', 10.00, 0.00, '2026-01-01', '2026-12-31', 1, GETDATE(), 0),
(2, 'SUMMER20', N'Khuyến mãi hè 2026', 'Percent', 20.00, 200000.00, '2026-06-01', '2026-08-31', 1, GETDATE(), 0),
(3, 'FIXED50K', N'Giảm 50k đơn từ 300k', 'FixedAmount', 50000.00, 300000.00, '2026-05-01', '2026-07-31', 1, GETDATE(), 0);
SET IDENTITY_INSERT [Promotions] OFF;
GO

-- 14. Insert StaffShifts
SET IDENTITY_INSERT [StaffShifts] ON;
INSERT INTO [StaffShifts] ([ShiftId], [StaffId], [ShiftDate], [ShiftType], [StartTime], [EndTime], [CreatedAt]) VALUES
(1, 2, '2026-05-14', 'Morning', '06:00:00', '14:00:00', GETDATE()),
(2, 2, '2026-05-15', 'Afternoon', '14:00:00', '22:00:00', GETDATE()),
(3, 2, '2026-05-16', 'Morning', '06:00:00', '14:00:00', GETDATE());
SET IDENTITY_INSERT [StaffShifts] OFF;
GO

PRINT '=== Temporary Seed Data Inserted Successfully ===';
