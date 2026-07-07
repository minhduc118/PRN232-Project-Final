-- ===================================================================
-- SQL SEED DATA SCRIPT - SPORT COURT MANAGEMENT SYSTEM
-- ===================================================================

USE [PRN232_SCM_DB]; -- Ensure you are using the correct database name
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Safe, re-runnable clean up phase (deletes existing records in dependency order)
IF OBJECT_ID('dbo.Tasks', 'U') IS NOT NULL DELETE FROM [dbo].[Tasks];
IF OBJECT_ID('dbo.ComplexCourtTypeServices', 'U') IS NOT NULL DELETE FROM [dbo].[ComplexCourtTypeServices];
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
INSERT INTO [Roles] ([RoleId], [RoleName], [Description]) VALUES
(1, 'Admin', N'Quản trị toàn bộ hệ thống'),
(2, 'Staff', N'Nhân viên hỗ trợ vận hành'),
(3, 'Coach', N'Huấn luyện viên thể thao'),
(4, 'Customer', N'Khách hàng đặt sân');
SET IDENTITY_INSERT [Roles] OFF;
GO

-- 2. Insert MembershipTiers
SET IDENTITY_INSERT [MembershipTiers] ON;
INSERT INTO [MembershipTiers] ([TierId], [TierName], [MinPoints], [DiscountPercent]) VALUES
(1, 'Bronze', 0, 0.00),
(2, 'Silver', 500, 5.00),
(3, 'Gold', 2000, 10.00),
(4, 'Platinum', 5000, 15.00);
SET IDENTITY_INSERT [MembershipTiers] OFF;
GO

-- 3. Insert CourtTypes
SET IDENTITY_INSERT [CourtTypes] ON;
INSERT INTO [CourtTypes] ([CourtTypeId], [TypeName], [IsActive]) VALUES
(1, N'Cầu lông', 1),
(2, N'Bóng đá', 1),
(3, N'Pickleball', 1),
(4, N'Tennis', 1),
(5, N'Bóng rổ', 1);
SET IDENTITY_INSERT [CourtTypes] OFF;
GO

-- 4. Insert TimeSlots
SET IDENTITY_INSERT [TimeSlots] ON;
INSERT INTO [TimeSlots] ([SlotId], [SlotName], [StartTime], [EndTime], [DayType]) VALUES
(1, N'Sáng sớm', '05:00:00', '07:00:00', 0),
(2, N'Buổi sáng', '07:00:00', '11:00:00', 0),
(3, N'Buổi trưa', '11:00:00', '13:00:00', 0),
(4, N'Buổi chiều', '13:00:00', '17:00:00', 0),
(5, N'Giờ vàng', '17:00:00', '21:00:00', 0),
(6, N'Tối muộn', '21:00:00', '23:00:00', 0),
(7, N'Cuối tuần sáng', '06:00:00', '12:00:00', 1),
(8, N'Cuối tuần chiều', '12:00:00', '18:00:00', 1),
(9, N'Cuối tuần tối', '18:00:00', '23:00:00', 1);
SET IDENTITY_INSERT [TimeSlots] OFF;
GO

-- 5. Insert Users
SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserId], [FullName], [Email], [Phone], [PasswordHash], [IsActive], [MembershipTierId], [CreatedAt], [LoyaltyPoints], [Gender], [SkillLevel]) VALUES
(1, 'System Admin', 'admin@sportscourtms.vn', '0900000001', '$2a$11$B3X9ngp8IGqoU2H3yEZhZ.66WJnUtRTu5pfwbfGw3h7TAbBE/8PCi', 1, 4, GETDATE(), 0, 0, 0),
(2, 'Nguyễn Văn An', 'staff@sportscourtms.vn', '0900000002', '$2a$11$3bpecDWLBV.A7PedIsc9TOIFFOlEZuiHfr2TB9okofi1IUIsfQBpS', 1, 1, GETDATE(), 0, 0, 0),
(3, 'Trần Thị Bình', 'coach@sportscourtms.vn', '0900000003', '$2a$11$j/0Dmeuu2TKr7ITShicISuR1VdpMlQSf1qIyHzV9ItuvVh/JWCa.a', 1, 2, GETDATE(), 0, 1, 0),
(4, 'Lê Văn Cường', 'customer@gmail.com', '0912345678', '$2a$11$p2.A.zWVick1.hl/qpK75uzMPZQB34oyad054MbL01NwaUe2GBKjq', 1, 1, GETDATE(), 0, 0, 0);
SET IDENTITY_INSERT [Users] OFF;
GO

-- 6. Insert UserRoles
SET IDENTITY_INSERT [UserRoles] ON;
INSERT INTO [UserRoles] ([UserRoleId], [UserId], [RoleId]) VALUES
(1, 1, 1), -- Admin
(2, 2, 2), -- Staff
(3, 3, 3), -- Coach
(4, 4, 4); -- Customer
SET IDENTITY_INSERT [UserRoles] OFF;
GO

-- 7. Insert CourtComplexes (30 tổ hợp sân toàn Hà Nội)
SET IDENTITY_INSERT [CourtComplexes] ON;
INSERT INTO [CourtComplexes] ([ComplexId], [ComplexName], [Address], [ManagerId], [Description], [ImageUrl], [IsDeleted], [CreatedAt]) VALUES
(1,  N'Tổ hợp thể thao Cầu Giấy',         N'Dịch Vọng, Cầu Giấy, Hà Nội',           1, N'Tổ hợp thể thao hiện đại hàng đầu tại Cầu Giấy với các sân trong nhà điều hòa.', 'https://images.unsplash.com/photo-1545224497-5d750c673417?q=80&w=800', 0, GETDATE()),
(2,  N'Tổ hợp thể thao Thanh Xuân',        N'Nguyễn Trãi, Thanh Xuân, Hà Nội',        2, N'Khu phức hợp thể thao ngoài trời và trong nhà đa năng tại Thanh Xuân.',            'https://images.unsplash.com/photo-1526232761682-d26e03ac148e?q=80&w=800', 0, GETDATE()),
(3,  N'SportZone Đống Đa',                  N'Khâm Thiên, Đống Đa, Hà Nội',            1, N'Khu thể thao đa năng tại trung tâm Đống Đa, tiện đi lại.',                          'https://images.unsplash.com/photo-1574629810360-7efbbe195018?q=80&w=800', 0, GETDATE()),
(4,  N'ActiveHub Hoàng Mai',                N'Tam Trinh, Hoàng Mai, Hà Nội',            2, N'Chuỗi sân thể thao chất lượng cao phục vụ khu vực phía Nam Hà Nội.',                 'https://images.unsplash.com/photo-1567025822912-efefe9cf4ac4?q=80&w=800', 0, GETDATE()),
(5,  N'ProSport Hà Đông',                   N'Quang Trung, Hà Đông, Hà Nội',           1, N'Tổ hợp thể thao chuyên nghiệp tại Hà Đông, mở cửa từ 5h sáng.',                    'https://images.unsplash.com/photo-1515923256482-1c04580b477a?q=80&w=800', 0, GETDATE()),
(6,  N'Tổ hợp thể thao Long Biên',         N'Bồ Đề, Long Biên, Hà Nội',               2, N'Khu thể thao hiện đại nhất phía Đông Hà Nội.',                                      'https://images.unsplash.com/photo-1559056199-641a0ac8b55e?q=80&w=800', 0, GETDATE()),
(7,  N'FitArena Tây Hồ',                    N'Xuân La, Tây Hồ, Hà Nội',               1, N'Sân thể thao cao cấp ven Hồ Tây, view đẹp, không gian thoáng.',                     'https://images.unsplash.com/photo-1489944440615-453fc2b6a9a9?q=80&w=800', 0, GETDATE()),
(8,  N'SportMax Bắc Từ Liêm',              N'Cổ Nhuế, Bắc Từ Liêm, Hà Nội',          2, N'Khu thể thao khép kín diện tích lớn nhất quận Bắc Từ Liêm.',                        'https://images.unsplash.com/photo-1519861531473-9200262188bf?q=80&w=800', 0, GETDATE()),
(9,  N'Tổ hợp thể thao Nam Từ Liêm',       N'Trần Hữu Dực, Nam Từ Liêm, Hà Nội',      1, N'Tổ hợp thể thao hiện đại tại trung tâm phát triển mới Nam Từ Liêm.',                 'https://images.unsplash.com/photo-1590227531827-a70e50f21f16?q=80&w=800', 0, GETDATE()),
(10, N'GreenSport Gia Lâm',                 N'Ninh Hiệp, Gia Lâm, Hà Nội',            2, N'Sân thể thao sinh thái, không gian xanh mát tại huyện Gia Lâm.',                    'https://images.unsplash.com/photo-1591035897819-f4bdf739f446?q=80&w=800', 0, GETDATE()),
(11, N'EliteSport Hai Bà Trưng',            N'Trương Định, Hai Bà Trưng, Hà Nội',      1, N'Tổ hợp thể thao cao cấp tại quận Hai Bà Trưng với HLV chuyên nghiệp.',              'https://images.unsplash.com/photo-1535131749006-b7f58c99034b?q=80&w=800', 0, GETDATE()),
(12, N'CityPlay Hoàn Kiếm',                 N'Hàng Bài, Hoàn Kiếm, Hà Nội',           2, N'Sân tập thể thao ngay trung tâm phố cổ, phù hợp cho nhân viên văn phòng.',           'https://images.unsplash.com/photo-1551958219-acbc595b85e4?q=80&w=800', 0, GETDATE()),
(13, N'Tổ hợp thể thao Thanh Trì',         N'Linh Đàm, Thanh Trì, Hà Nội',           1, N'Khu liên hợp thể thao lớn nhất phía Nam Hà Nội, nhiều sân đa dạng.',                'https://images.unsplash.com/photo-1579758629938-03607ccdbaba?q=80&w=800', 0, GETDATE()),
(14, N'VentureSport Ba Đình',               N'Kim Mã, Ba Đình, Hà Nội',               2, N'Tổ hợp thể thao gần trung tâm Ba Đình, phục vụ cán bộ công chức.',                  'https://images.unsplash.com/photo-1519863512547-ab547415a8b8?q=80&w=800', 0, GETDATE()),
(15, N'MaxFit Đan Phượng',                  N'Tân Lập, Đan Phượng, Hà Nội',           1, N'Khu thể thao hiện đại nhất huyện Đan Phượng, diện tích rộng.',                       'https://images.unsplash.com/photo-1571902943202-507ec2618e8f?q=80&w=800', 0, GETDATE()),
(16, N'SportVillage Mê Linh',               N'Tiền Phong, Mê Linh, Hà Nội',           2, N'Tổ hợp thể thao kết hợp khu nghỉ dưỡng tại Mê Linh.',                               'https://images.unsplash.com/photo-1575650772416-eb9b0b72b29e?q=80&w=800', 0, GETDATE()),
(17, N'ActivePark Sóc Sơn',                 N'Minh Phú, Sóc Sơn, Hà Nội',            1, N'Khu thể thao ngoài trời lớn nhất huyện Sóc Sơn, gần sân bay Nội Bài.',             'https://images.unsplash.com/photo-1590488398561-f59d40adae5d?q=80&w=800', 0, GETDATE()),
(18, N'SportHub Thường Tín',                N'Văn Bình, Thường Tín, Hà Nội',          2, N'Tổ hợp thể thao tại huyện Thường Tín phục vụ cư dân ngoại thành.',                  'https://images.unsplash.com/photo-1570129477492-45c003edd2be?q=80&w=800', 0, GETDATE()),
(19, N'ProCourt Phúc Thọ',                  N'Vân Hà, Phúc Thọ, Hà Nội',             1, N'Sân thể thao chuyên nghiệp kết hợp hồ bơi tại huyện Phúc Thọ.',                    'https://images.unsplash.com/photo-1600548063393-c1e61dba8e6a?q=80&w=800', 0, GETDATE()),
(20, N'MegaFit Quốc Oai',                   N'Quốc Oai, Quốc Oai, Hà Nội',           2, N'Tổ hợp thể thao diện tích lớn nhất tại huyện Quốc Oai.',                            'https://images.unsplash.com/photo-1560272564-c83b66b1ad12?q=80&w=800', 0, GETDATE()),
(21, N'PowerZone Thạch Thất',               N'Thạch Thất, Thạch Thất, Hà Nội',        1, N'Tổ hợp thể thao hiện đại mới khai trương tại huyện Thạch Thất.',                    'https://images.unsplash.com/photo-1604313483578-7bb6aa51c5a8?q=80&w=800', 0, GETDATE()),
(22, N'SportCenter Ba Vì',                   N'Tản Đà, Ba Vì, Hà Nội',                2, N'Khu thể thao dã ngoại ven sông Đà, không khí trong lành tuyệt vời.',                 'https://images.unsplash.com/photo-1553692459-f8f49db820c2?q=80&w=800', 0, GETDATE()),
(23, N'Tổ hợp thể thao Hoài Đức',           N'Trạm Trôi, Hoài Đức, Hà Nội',           1, N'Khu phức hợp thể thao đa năng gồm sân bóng đá mini và cụm sân cầu lông tiêu chuẩn.', 'https://images.unsplash.com/photo-1508098682722-e99c43a406b2?q=80&w=800', 0, GETDATE()),
(24, N'SportyClub Đông Anh',                 N'Cao Lỗ, Đông Anh, Hà Nội',              2, N'Tổ hợp thể thao hiện đại bậc nhất khu vực Đông Anh với hệ thống mái che toàn diện.',  'https://images.unsplash.com/photo-1517649763962-0c623066013b?q=80&w=800', 0, GETDATE()),
(25, N'DeltaArena Chương Mỹ',                N'Chúc Sơn, Chương Mỹ, Hà Nội',           1, N'Khu liên hợp thể thao rộng lớn phục vụ hoạt động tập luyện và thi đấu phong trào.', 'https://images.unsplash.com/photo-1530541930197-ff16ac917b0e?q=80&w=800', 0, GETDATE()),
(26, N'KingSport Thanh Oai',                 N'Kim Bài, Thanh Oai, Hà Nội',            2, N'Khu thể thao chuyên nghiệp với mặt sân cỏ thế hệ mới và dàn đèn cao áp hiện đại.',  'https://images.unsplash.com/photo-1541534741688-6078c6bfb5c5?q=80&w=800', 0, GETDATE()),
(27, N'VietSport Mỹ Đức',                    N'Đại Nghĩa, Mỹ Đức, Hà Nội',             1, N'Tổ hợp sân thể thao cộng đồng thoáng đãng, cơ sở vật chất đầy đủ, tiện nghi.',     'https://images.unsplash.com/photo-1519766304817-4f37bda74a27?q=80&w=800', 0, GETDATE()),
(28, N'RoyalCourt Phú Xuyên',                N'Phú Xuyên, Phú Xuyên, Hà Nội',          2, N'Hệ thống sân bóng đá và cầu lông chất lượng cao, phục vụ cư dân Phú Xuyên.',        'https://images.unsplash.com/photo-1461896836934-ffe607ba8211?q=80&w=800', 0, GETDATE()),
(29, N'OlympicHub Ứng Hòa',                  N'Vân Đình, Ứng Hòa, Hà Nội',             1, N'Trung tâm thể thao đa năng chất lượng, điểm hẹn lý tưởng cho những người đam mê thể thao.', 'https://images.unsplash.com/photo-1574629810360-7efbbe195018?q=80&w=800', 0, GETDATE()),
(30, N'GoldenSport Sơn Tây',                 N'Lê Lợi, Sơn Tây, Hà Nội',               2, N'Tổ hợp thể thao cao cấp tại thị xã Sơn Tây, trang bị đầy đủ khu dịch vụ phụ trợ.',  'https://images.unsplash.com/photo-1505250469613-27bac4f40014?q=80&w=800', 0, GETDATE()),
(31, N'Vinhomes Ocean Park Arena',           N'Đa Tốn, Gia Lâm, Hà Nội',               1, N'Khu phức hợp thể thao ngoài trời hiện đại tại đại đô thị Ocean Park.',              'https://images.unsplash.com/photo-1545224497-5d750c673417?q=80&w=800', 0, GETDATE()),
(32, N'Tổ hợp thể thao Nghĩa Tân',           N'Nghĩa Tân, Cầu Giấy, Hà Nội',           2, N'Cụm sân cầu lông và bóng rổ hoạt động sôi nổi lâu đời.',                            'https://images.unsplash.com/photo-1526232761682-d26e03ac148e?q=80&w=800', 0, GETDATE()),
(33, N'Bách Khoa Sport Center',              N'Tạ Quang Bửu, Hai Bà Trưng, Hà Nội',    1, N'Sân vận động và tổ hợp thể thao phục vụ sinh viên và cư dân.',                       'https://images.unsplash.com/photo-1574629810360-7efbbe195018?q=80&w=800', 0, GETDATE()),
(34, N'Mỹ Đình Stadium Complex',             N'Lê Đức Thọ, Nam Từ Liêm, Hà Nội',      2, N'Cụm sân phụ quanh sân vận động quốc gia Mỹ Đình, đạt chuẩn thi đấu.',                'https://images.unsplash.com/photo-1567025822912-efefe9cf4ac4?q=80&w=800', 0, GETDATE()),
(35, N'Hoàng Mai Football Club',             N'Đền Lừ, Hoàng Mai, Hà Nội',             1, N'Hệ thống sân bóng đá cỏ nhân tạo chất lượng cao mở cửa cả ngày.',                    'https://images.unsplash.com/photo-1515923256482-1c04580b477a?q=80&w=800', 0, GETDATE()),
(36, N'Tây Hồ Club & Spa',                   N'Quảng An, Tây Hồ, Hà Nội',              2, N'Khu thể thao kết hợp nghỉ dưỡng cao cấp ven Hồ Tây.',                               'https://images.unsplash.com/photo-1559056199-641a0ac8b55e?q=80&w=800', 0, GETDATE()),
(37, N'Thanh Xuân Club',                     N'Ngụy Như Kon Tum, Thanh Xuân, Hà Nội',  1, N'Sân chơi thể thao năng động cho giới văn phòng và người trẻ.',                       'https://images.unsplash.com/photo-1489944440615-453fc2b6a9a9?q=80&w=800', 0, GETDATE()),
(38, N'Đống Đa Arena',                       N'Đặng Tiến Đông, Đống Đa, Hà Nội',       2, N'Tổ hợp thể thao đa năng gồm bóng rổ, cầu lông và pickleball.',                       'https://images.unsplash.com/photo-1519861531473-9200262188bf?q=80&w=800', 0, GETDATE()),
(39, N'Long Biên Golf & Sports',             N'Phúc Đồng, Long Biên, Hà Nội',          1, N'Khu liên hợp thể thao và dịch vụ cao cấp nhất quận Long Biên.',                      'https://images.unsplash.com/photo-1590227531827-a70e50f21f16?q=80&w=800', 0, GETDATE()),
(40, N'Bắc Từ Liêm Hub',                     N'Đức Thắng, Bắc Từ Liêm, Hà Nội',        2, N'Sân bóng đá cỏ nhân tạo và sân cầu lông trong nhà rộng rãi.',                        'https://images.unsplash.com/photo-1591035897819-f4bdf739f446?q=80&w=800', 0, GETDATE()),
(41, N'Tổ hợp thể thao Gia Lâm',             N'Trâu Quỳ, Gia Lâm, Hà Nội',             1, N'Sân tập luyện đa năng phục vụ cư dân huyện Gia Lâm.',                                'https://images.unsplash.com/photo-1535131749006-b7f58c99034b?q=80&w=800', 0, GETDATE()),
(42, N'Đông Anh Arena',                      N'Cao Lỗ, Đông Anh, Hà Nội',              2, N'Cụm sân thể thao mái che khẩu độ lớn, không ngại thời tiết mưa nắng.',              'https://images.unsplash.com/photo-1551958219-acbc595b85e4?q=80&w=800', 0, GETDATE()),
(43, N'Sóc Sơn Sporty',                      N'Đền Sóc, Sóc Sơn, Hà Nội',              1, N'Địa điểm lý tưởng cho các trận bóng đá và cầu lông giao hữu cuối tuần.',             'https://images.unsplash.com/photo-1579758629938-03607ccdbaba?q=80&w=800', 0, GETDATE()),
(44, N'Mê Linh Club',                        N'Đại Thịnh, Mê Linh, Hà Nội',            2, N'Khu thể thao gia đình kết hợp vui chơi giải trí thoáng mát.',                        'https://images.unsplash.com/photo-1519863512547-ab547415a8b8?q=80&w=800', 0, GETDATE()),
(45, N'Sơn Tây Stadium Side',                N'Phú Thịnh, Sơn Tây, Hà Nội',            1, N'Tổ hợp sân thể thao cạnh sân vận động thị xã Sơn Tây.',                              'https://images.unsplash.com/photo-1571902943202-507ec2618e8f?q=80&w=800', 0, GETDATE()),
(46, N'Ba Vì Green Field',                   N'Tản Lĩnh, Ba Vì, Hà Nội',               2, N'Sân cỏ tự nhiên view núi rừng Ba Vì, không khí trong lành.',                         'https://images.unsplash.com/photo-1575650772416-eb9b0b72b29e?q=80&w=800', 0, GETDATE()),
(47, N'Phúc Thọ Sport Hub',                  N'Phúc Thọ, Phúc Thọ, Hà Nội',            1, N'Sân cầu lông phong trào chất lượng tốt, thảm PVC tiêu chuẩn.',                       'https://images.unsplash.com/photo-1590488398561-f59d40adae5d?q=80&w=800', 0, GETDATE()),
(48, N'Thạch Thất Arena',                    N'Liên Quan, Thạch Thất, Hà Nội',         2, N'Sân bóng đá mini cỏ nhân tạo chất lượng cao phục vụ thanh thiếu niên.',              'https://images.unsplash.com/photo-1570129477492-45c003edd2be?q=80&w=800', 0, GETDATE()),
(49, N'Quốc Oai Club',                       N'Quốc Oai, Quốc Oai, Hà Nội',            1, N'Tổ hợp thể thao mới đầu tư, trang thiết bị hiện đại.',                               'https://images.unsplash.com/photo-1600548063393-c1e61dba8e6a?q=80&w=800', 0, GETDATE()),
(50, N'Chương Mỹ Sporty',                    N'Chúc Sơn, Chương Mỹ, Hà Nội',           2, N'Khu sân tập trung tâm huyện Chương Mỹ với bãi đỗ xe rộng rãi.',                      'https://images.unsplash.com/photo-1560272564-c83b66b1ad12?q=80&w=800', 0, GETDATE()),
(51, N'Đan Phượng Arena',                    N'Phùng, Đan Phượng, Hà Nội',             1, N'Sân bóng rổ và cầu lông chất lượng hàng đầu khu vực Đan Phượng.',                    'https://images.unsplash.com/photo-1604313483578-7bb6aa51c5a8?q=80&w=800', 0, GETDATE()),
(52, N'Hoài Đức Sport Park',                 N'Trạm Trôi, Hoài Đức, Hà Nội',           2, N'Khu công viên thể thao đa năng, mát mẻ nhiều cây xanh.',                             'https://images.unsplash.com/photo-1553692459-f8f49db820c2?q=80&w=800', 0, GETDATE()),
(53, N'Thanh Oai Hub',                       N'Kim Bài, Thanh Oai, Hà Nội',            1, N'Cụm sân bóng cỏ nhân tạo phục vụ bóng đá phong trào.',                               'https://images.unsplash.com/photo-1508098682722-e99c43a406b2?q=80&w=800', 0, GETDATE()),
(54, N'Mỹ Đức Arena',                        N'Đại Nghĩa, Mỹ Đức, Hà Nội',             2, N'Sân tập cầu lông mái che kiên cố, ánh sáng chuẩn thi đấu.',                          'https://images.unsplash.com/photo-1517649763962-0c623066013b?q=80&w=800', 0, GETDATE()),
(55, N'Ứng Hòa Sports',                      N'Vân Đình, Ứng Hòa, Hà Nội',             1, N'Trung tâm thể dục thể thao quận huyện, nhiều hoạt động sôi nổi.',                    'https://images.unsplash.com/photo-1530541930197-ff16ac917b0e?q=80&w=800', 0, GETDATE()),
(56, N'Thường Tín Club',                     N'Thường Tín, Thường Tín, Hà Nội',        2, N'Địa điểm giao lưu bóng đá và tennis hàng đầu Thường Tín.',                           'https://images.unsplash.com/photo-1541534741688-6078c6bfb5c5?q=80&w=800', 0, GETDATE()),
(57, N'Phú Xuyên Arena',                     N'Phú Xuyên, Phú Xuyên, Hà Nội',          1, N'Sân thể thao cộng đồng khang trang, phục vụ cư dân địa phương.',                      'https://images.unsplash.com/photo-1519766304817-4f37bda74a27?q=80&w=800', 0, GETDATE()),
(58, N'Cầu Giấy Premium',                    N'Trung Hòa, Cầu Giấy, Hà Nội',           2, N'Sân pickleball trong nhà điều hòa cao cấp, dịch vụ nước uống miễn phí.',             'https://images.unsplash.com/photo-1461896836934-ffe607ba8211?q=80&w=800', 0, GETDATE()),
(59, N'Tổ hợp thể thao Yên Hòa',             N'Yên Hòa, Cầu Giấy, Hà Nội',             1, N'Sân bóng đá mini 7 người đông đúc, náo nhiệt.',                                      'https://images.unsplash.com/photo-1574629810360-7efbbe195018?q=80&w=800', 0, GETDATE()),
(60, N'Hà Đông Premium Center',              N'Mộ Lao, Hà Đông, Hà Nội',               2, N'Khu phức hợp thể thao đẳng cấp cao, hồ bơi bốn mùa bên cạnh.',                       'https://images.unsplash.com/photo-1505250469613-27bac4f40014?q=80&w=800', 0, GETDATE()),
(61, N'Thanh Xuân Nam Center',               N'Thanh Xuân Nam, Thanh Xuân, Hà Nội',    1, N'Sân cầu lông sàn gỗ tự nhiên chống trơn trượt.',                                     'https://images.unsplash.com/photo-1545224497-5d750c673417?q=80&w=800', 0, GETDATE()),
(62, N'Đống Đa Central Park',                N'Láng Hạ, Đống Đa, Hà Nội',              2, N'Khu thể thao văn phòng tiện lợi, phục vụ các giải đấu công sở.',                     'https://images.unsplash.com/photo-1526232761682-d26e03ac148e?q=80&w=800', 0, GETDATE()),
(63, N'Hai Bà Trưng Hub',                    N'Minh Khai, Hai Bà Trưng, Hà Nội',       1, N'Sân bóng rổ ngoài trời đạt chuẩn thi đấu FIBA.',                                     'https://images.unsplash.com/photo-1574629810360-7efbbe195018?q=80&w=800', 0, GETDATE()),
(64, N'Hoàn Kiếm Lake View',                 N'Tràng Tiền, Hoàn Kiếm, Hà Nội',         2, N'Sân cầu lông tầng thượng với tầm nhìn hướng ra Hồ Gươm.',                            'https://images.unsplash.com/photo-1567025822912-efefe9cf4ac4?q=80&w=800', 0, GETDATE()),
(65, N'Ba Đình Sport Center',                N'Giảng Võ, Ba Đình, Hà Nội',             1, N'Nhà thi đấu đa năng quy mô lớn, sức chứa khán đài 500 người.',                       'https://images.unsplash.com/photo-1515923256482-1c04580b477a?q=80&w=800', 0, GETDATE()),
(66, N'Tây Hồ Water Sport',                  N'Nhật Tân, Tây Hồ, Hà Nội',              2, N'Khu thể thao bãi biển ngoài trời độc đáo.',                                          'https://images.unsplash.com/photo-1559056199-641a0ac8b55e?q=80&w=800', 0, GETDATE()),
(67, N'Hoàng Mai Lakeside',                  N'Linh Đàm, Hoàng Mai, Hà Nội',           1, N'Cụm sân bóng đá cỏ nhân tạo ven hồ Linh Đàm thơ mộng.',                              'https://images.unsplash.com/photo-1489944440615-453fc2b6a9a9?q=80&w=800', 0, GETDATE()),
(68, N'Long Biên Air Sports',                N'Gia Thụy, Long Biên, Hà Nội',           2, N'Khu sân thể thao liên kết hàng không, dịch vụ chuyên nghiệp.',                       'https://images.unsplash.com/photo-1519861531473-9200262188bf?q=80&w=800', 0, GETDATE()),
(69, N'Nam Từ Liêm Arena',                   N'Mỹ Đình, Nam Từ Liêm, Hà Nội',          1, N'Tổ hợp sân tennis và pickleball ngoài trời có đèn đêm cực sáng.',                    'https://images.unsplash.com/photo-1590227531827-a70e50f21f16?q=80&w=800', 0, GETDATE()),
(70, N'Bắc Từ Liêm Campus',                  N'Cổ Nhuế, Bắc Từ Liêm, Hà Nội',          2, N'Tổ hợp thể thao học đường mở cửa cho người dân vào sinh hoạt.',                      'https://images.unsplash.com/photo-1591035897819-f4bdf739f446?q=80&w=800', 0, GETDATE());
SET IDENTITY_INSERT [CourtComplexes] OFF;
GO

-- 8. Insert Courts (10 sân mỗi tổ hợp cho tất cả 70 tổ hợp = 700 sân)
SET IDENTITY_INSERT [Courts] ON;
DECLARE @ComplexId INT = 1;
DECLARE @CourtCounter INT = 1;
DECLARE @CourtTypeName NVARCHAR(50);
DECLARE @CourtTypeId INT;
DECLARE @Price DECIMAL(18,2);
DECLARE @Size NVARCHAR(50);
DECLARE @CodePrefix NVARCHAR(5);
DECLARE @CourtId INT = 1;

WHILE @ComplexId <= 70
BEGIN
    SET @CourtCounter = 1;
    WHILE @CourtCounter <= 10
    BEGIN
        SET @CourtTypeId = ((@ComplexId + @CourtCounter) % 5) + 1;
        
        IF @CourtTypeId = 1
        BEGIN
            SET @CourtTypeName = N'Sân Cầu Lông';
            SET @CodePrefix = N'CL';
            SET @Price = 100000.00;
            SET @Size = N'Tiêu chuẩn';
        END
        ELSE IF @CourtTypeId = 2
        BEGIN
            SET @CourtTypeName = N'Sân Bóng Đá';
            SET @CodePrefix = N'BD';
            SET @Price = 300000.00;
            SET @Size = N'Sân 5 người';
        END
        ELSE IF @CourtTypeId = 3
        BEGIN
            SET @CourtTypeName = N'Sân Pickleball';
            SET @CodePrefix = N'PB';
            SET @Price = 150000.00;
            SET @Size = N'Tiêu chuẩn';
        END
        ELSE IF @CourtTypeId = 4
        BEGIN
            SET @CourtTypeName = N'Sân Tennis';
            SET @CodePrefix = N'TN';
            SET @Price = 250000.00;
            SET @Size = N'Tiêu chuẩn';
        END
        ELSE
        BEGIN
            SET @CourtTypeId = 5;
            SET @CourtTypeName = N'Sân Bóng Rổ';
            SET @CodePrefix = N'BR';
            SET @Price = 120000.00;
            SET @Size = N'Tiêu chuẩn';
        END

        INSERT INTO [Courts] ([CourtId], [CourtName], [CourtCode], [CourtTypeId], [ComplexId], [PricePerHour], [CourtSize], [OpenTime], [CloseTime], [Status], [IsDeleted])
        VALUES (
            @CourtId,
            @CourtTypeName + N' ' + CAST(@ComplexId AS NVARCHAR(10)) + N'-' + CAST(@CourtCounter AS NVARCHAR(10)),
            @CodePrefix + N'-' + CAST(@ComplexId AS NVARCHAR(10)) + N'0' + CAST(@CourtCounter AS NVARCHAR(10)),
            @CourtTypeId,
            @ComplexId,
            @Price,
            @Size,
            '06:00:00',
            '22:00:00',
            0, -- Available
            0  -- Not Deleted
        );

        SET @CourtId = @CourtId + 1;
        SET @CourtCounter = @CourtCounter + 1;
    END
    SET @ComplexId = @ComplexId + 1;
END
SET IDENTITY_INSERT [Courts] OFF;
GO

-- 9. Insert CourtImages (Tự động gán 1 ảnh chất lượng cao cho mỗi sân)
DECLARE @InsertedCourtId INT;
DECLARE @TypeId INT;
DECLARE @ImgUrl NVARCHAR(500);

DECLARE court_cursor CURSOR FOR 
SELECT CourtId, CourtTypeId FROM [Courts] WHERE IsDeleted = 0;

OPEN court_cursor;
FETCH NEXT FROM court_cursor INTO @InsertedCourtId, @TypeId;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF @TypeId = 1
        SET @ImgUrl = 'https://images.unsplash.com/photo-1626224583764-f87db24ac4ea?q=80&w=600';
    ELSE IF @TypeId = 2
        SET @ImgUrl = 'https://images.unsplash.com/photo-1508098682722-e99c43a406b2?q=80&w=600';
    ELSE IF @TypeId = 3
        SET @ImgUrl = 'https://images.unsplash.com/photo-1459865264687-595d652de67e?q=80&w=600';
    ELSE IF @TypeId = 4
        SET @ImgUrl = 'https://images.unsplash.com/photo-1595435934249-5df7ed86e1c0?q=80&w=600';
    ELSE
        SET @ImgUrl = 'https://images.unsplash.com/photo-1519766304817-4f37bda74a27?q=80&w=600';

    INSERT INTO [CourtImages] ([CourtId], [ImageUrl], [IsPrimary])
    VALUES (@InsertedCourtId, @ImgUrl, 1);

    FETCH NEXT FROM court_cursor INTO @InsertedCourtId, @TypeId;
END

CLOSE court_cursor;
DEALLOCATE court_cursor;
GO
GO

-- 10. Insert CourtPricing
SET IDENTITY_INSERT [CourtPricing] ON;
INSERT INTO [CourtPricing] ([PricingId], [CourtId], [SlotId], [Price], [EffectiveFrom]) VALUES
(1, 1, 1, 80000.00, CAST(GETDATE() AS DATE)),
(2, 1, 2, 100000.00, CAST(GETDATE() AS DATE)),
(3, 1, 3, 90000.00, CAST(GETDATE() AS DATE)),
(4, 1, 4, 100000.00, CAST(GETDATE() AS DATE)),
(5, 1, 5, 150000.00, CAST(GETDATE() AS DATE)),
(6, 1, 6, 120000.00, CAST(GETDATE() AS DATE)),
(7, 3, 2, 300000.00, CAST(GETDATE() AS DATE)),
(8, 3, 4, 300000.00, CAST(GETDATE() AS DATE)),
(9, 3, 5, 500000.00, CAST(GETDATE() AS DATE)),
(10, 3, 7, 400000.00, CAST(GETDATE() AS DATE)),
(11, 3, 8, 400000.00, CAST(GETDATE() AS DATE)),
(12, 3, 9, 600000.00, CAST(GETDATE() AS DATE));
SET IDENTITY_INSERT [CourtPricing] OFF;
GO

-- 11. Insert Services
SET IDENTITY_INSERT [Services] ON;
INSERT INTO [Services] ([ServiceId], [ServiceName], [Category], [Price], [Unit], [Description], [StockQty], [IsActive], [CreatedAt]) VALUES
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
INSERT INTO [EquipmentInventory] ([InventoryId], [ServiceId], [ItemCode], [Condition], [PurchaseDate], [PurchasePrice], [IsAvailable]) VALUES
(1, 1, 'VOT-001', 0, '2026-01-01', 500000.00, 1),
(2, 1, 'VOT-002', 0, '2026-01-01', 500000.00, 1),
(3, 1, 'VOT-003', 1, '2026-01-01', 500000.00, 0),
(4, 2, 'BONG-001', 0, '2026-01-01', 150000.00, 1),
(5, 2, 'BONG-002', 0, '2026-01-01', 150000.00, 1),
(6, 3, 'GIAY-001', 0, '2026-01-15', 300000.00, 1),
(7, 3, 'GIAY-002', 0, '2026-01-15', 300000.00, 1);
SET IDENTITY_INSERT [EquipmentInventory] OFF;
GO

-- 13. Insert Promotions
SET IDENTITY_INSERT [Promotions] ON;
INSERT INTO [Promotions] ([PromotionId], [PromoCode], [PromoName], [DiscountType], [DiscountValue], [MinOrderAmount], [StartDate], [EndDate], [IsActive], [CreatedAt], [UsedCount]) VALUES
(1, 'WELCOME10', N'Chào mừng thành viên mới', 0, 10.00, 0.00, '2026-01-01', '2026-12-31', 1, GETDATE(), 0),
(2, 'SUMMER20', N'Khuyến mãi hè 2026', 0, 20.00, 200000.00, '2026-06-01', '2026-08-31', 1, GETDATE(), 0),
(3, 'FIXED50K', N'Giảm 50k đơn từ 300k', 1, 50000.00, 300000.00, '2026-05-01', '2026-07-31', 1, GETDATE(), 0);
SET IDENTITY_INSERT [Promotions] OFF;
GO

-- 14. Insert StaffShifts
SET IDENTITY_INSERT [StaffShifts] ON;
INSERT INTO [StaffShifts] ([ShiftId], [StaffId], [ShiftDate], [ShiftType], [StartTime], [EndTime]) VALUES
(1, 2, '2026-05-14', 0, '06:00:00', '14:00:00'),
(2, 2, '2026-05-15', 1, '14:00:00', '22:00:00'),
(3, 2, '2026-05-16', 0, '06:00:00', '14:00:00');
SET IDENTITY_INSERT [StaffShifts] OFF;
GO

-- 15. Insert ComplexCourtTypeServices (Các dịch vụ cung cấp theo tổ hợp và loại sân)
-- Không cần SET IDENTITY_INSERT ON
DELETE FROM [ComplexCourtTypeServices];

DECLARE @CompId INT = 1;
DECLARE @CTypeId INT = 1;

WHILE @CompId <= 70
BEGIN
    SET @CTypeId = 1;
    WHILE @CTypeId <= 5
    BEGIN
        -- Gán dịch vụ Nước suối (ServiceId = 4) làm Included cho tất cả các loại sân
        INSERT INTO [ComplexCourtTypeServices] ([ComplexId], [CourtTypeId], [ServiceId], [Price], [StockQty], [ServiceMode], [IsActive], [CreatedAt])
        VALUES (@CompId, @CTypeId, 4, 0.00, 100, 0, 1, GETDATE());

        -- Gán dịch vụ Nước tăng lực (ServiceId = 5) làm Optional cho tất cả
        INSERT INTO [ComplexCourtTypeServices] ([ComplexId], [CourtTypeId], [ServiceId], [Price], [StockQty], [ServiceMode], [IsActive], [CreatedAt])
        VALUES (@CompId, @CTypeId, 5, 20000.00, 50, 1, 1, GETDATE());

        -- Gán dịch vụ Thuê giày (ServiceId = 3) làm Optional cho tất cả
        INSERT INTO [ComplexCourtTypeServices] ([ComplexId], [CourtTypeId], [ServiceId], [Price], [StockQty], [ServiceMode], [IsActive], [CreatedAt])
        VALUES (@CompId, @CTypeId, 3, 20000.00, 20, 1, 1, GETDATE());

        -- Gán dịch vụ HLV cơ bản (ServiceId = 6) làm Optional cho tất cả
        INSERT INTO [ComplexCourtTypeServices] ([ComplexId], [CourtTypeId], [ServiceId], [Price], [StockQty], [ServiceMode], [IsActive], [CreatedAt])
        VALUES (@CompId, @CTypeId, 6, 200000.00, 5, 1, 1, GETDATE());

        -- Gán dịch vụ HLV nâng cao (ServiceId = 7) làm Optional cho tất cả
        INSERT INTO [ComplexCourtTypeServices] ([ComplexId], [CourtTypeId], [ServiceId], [Price], [StockQty], [ServiceMode], [IsActive], [CreatedAt])
        VALUES (@CompId, @CTypeId, 7, 400000.00, 3, 1, 1, GETDATE());

        -- Nếu là Cầu lông (CourtTypeId = 1) -> thêm Thuê vợt (1) và Thuê bóng cầu lông (2)
        IF @CTypeId = 1
        BEGIN
            INSERT INTO [ComplexCourtTypeServices] ([ComplexId], [CourtTypeId], [ServiceId], [Price], [StockQty], [ServiceMode], [IsActive], [CreatedAt])
            VALUES (@CompId, @CTypeId, 1, 30000.00, 20, 1, 1, GETDATE());

            INSERT INTO [ComplexCourtTypeServices] ([ComplexId], [CourtTypeId], [ServiceId], [Price], [StockQty], [ServiceMode], [IsActive], [CreatedAt])
            VALUES (@CompId, @CTypeId, 2, 10000.00, 50, 1, 1, GETDATE());
        END
        -- Nếu là Bóng đá, Pickleball, Tennis, Bóng rổ -> thêm dịch vụ Tổ chức giải đấu (8)
        ELSE
        BEGIN
            INSERT INTO [ComplexCourtTypeServices] ([ComplexId], [CourtTypeId], [ServiceId], [Price], [StockQty], [ServiceMode], [IsActive], [CreatedAt])
            VALUES (@CompId, @CTypeId, 8, 2000000.00, 1, 1, 1, GETDATE());
        END

        SET @CTypeId = @CTypeId + 1;
    END
    SET @CompId = @CompId + 1;
END
GO

PRINT '=== Temporary Seed Data Inserted Successfully ===';
