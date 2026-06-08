# 🗄️ Database Design Document
# Sports Court Management System — PRN232

---

## ERD Diagram

```mermaid
erDiagram
    Users {
        int UserId PK
        nvarchar FullName
        varchar Email UK
        varchar Phone
        varchar PasswordHash
        varchar AvatarUrl
        int LoyaltyPoints
        int MembershipTierId FK
        bit IsActive
        datetime CreatedAt
    }

    Roles {
        int RoleId PK
        nvarchar RoleName UK
        nvarchar Description
    }

    UserRoles {
        int UserRoleId PK
        int UserId FK
        int RoleId FK
    }

    MembershipTiers {
        int TierId PK
        nvarchar TierName
        int MinPoints
        decimal DiscountPercent
    }

    CourtTypes {
        int CourtTypeId PK
        nvarchar TypeName UK
        bit IsActive
    }

    Courts {
        int CourtId PK
        nvarchar CourtName
        varchar CourtCode UK
        int CourtTypeId FK
        varchar Status
        time OpenTime
        time CloseTime
    }

    TimeSlots {
        int SlotId PK
        nvarchar SlotName
        time StartTime
        time EndTime
        varchar DayType
    }

    CourtPricing {
        int PricingId PK
        int CourtId FK
        int SlotId FK
        decimal Price
        date EffectiveFrom
    }

    Bookings {
        int BookingId PK
        varchar BookingCode UK
        int UserId FK
        int CourtId FK
        int SlotId FK
        date BookingDate
        decimal TotalAmount
        varchar Status
        int PromotionId FK
    }

    Services {
        int ServiceId PK
        nvarchar ServiceName
        varchar Category
        decimal Price
        int StockQty
    }

    BookingServices {
        int BookingServiceId PK
        int BookingId FK
        int ServiceId FK
        int Quantity
        decimal TotalPrice
    }

    Payments {
        int PaymentId PK
        int BookingId FK
        decimal Amount
        varchar PaymentMethod
        varchar TransactionId UK
        varchar Status
        datetime PaidAt
    }

    Reviews {
        int ReviewId PK
        int BookingId FK UK
        int UserId FK
        int CourtId FK
        tinyint Rating
        nvarchar Comment
        bit IsVisible
    }

    Promotions {
        int PromotionId PK
        varchar PromoCode UK
        nvarchar PromoName
        varchar DiscountType
        decimal DiscountValue
        datetime StartDate
        datetime EndDate
    }

    Notifications {
        int NotificationId PK
        int UserId FK
        nvarchar Title
        varchar Type
        bit IsRead
    }

    CoachSchedules {
        int ScheduleId PK
        int CoachId FK
        int CourtId FK
        int SlotId FK
        date ScheduleDate
        decimal Price
        bit IsBooked
    }

    CourtComplexes {
        int ComplexId PK
        nvarchar ComplexName
        nvarchar Address
        int ManagerId FK
        nvarchar Description
        varchar ImageUrl
        bit IsDeleted
        datetime CreatedAt
    }

    Tasks {
        int TaskId PK
        nvarchar Title
        nvarchar Description
        varchar TaskType
        varchar Category
        varchar Priority
        varchar Status
        int ComplexId FK
        int AssignedStaffId FK
        int CreatedById FK
        int BookingId FK
        datetime DueDate
        datetime CreatedAt
        datetime CompletedAt
    }

    %% Relationships
    Users         ||--o{ UserRoles       : "có"
    Roles         ||--o{ UserRoles       : "thuộc"
    MembershipTiers ||--o{ Users         : "phân hạng"
    CourtTypes    ||--o{ Courts          : "loại sân"
    Courts        ||--o{ CourtPricing    : "giá thuê"
    TimeSlots     ||--o{ CourtPricing    : "khung giờ"
    Users         ||--o{ Bookings        : "đặt sân"
    Courts        ||--o{ Bookings        : "được đặt"
    TimeSlots     ||--o{ Bookings        : "khung giờ"
    Promotions    ||--o{ Bookings        : "áp dụng"
    Bookings      ||--o{ BookingServices : "dịch vụ kèm"
    Services      ||--o{ BookingServices : "được chọn"
    Bookings      ||--|| Payments        : "thanh toán"
    Bookings      ||--o| Reviews         : "đánh giá"
    Users         ||--o{ Reviews         : "viết"
    Courts        ||--o{ Reviews         : "nhận đánh giá"
    Users         ||--o{ Notifications   : "nhận"
    Users         ||--o{ CoachSchedules  : "lịch dạy"
    Courts        ||--o{ CoachSchedules  : "địa điểm"
    CourtComplexes ||--o{ Courts          : "chứa"
    CourtComplexes ||--o{ Tasks           : "có"
    Users         ||--o{ Tasks           : "thực hiện"
    Users         ||--o{ Tasks           : "giao việc"
    Bookings      ||--o{ Tasks           : "liên kết"
    Users         ||--o{ CourtComplexes  : "quản lý"
```

---

## Danh sách bảng

| STT | Tên bảng | Mô tả | Phiên bản |
|-----|----------|-------|-----------|
| 1 | `Roles` | Vai trò hệ thống | v1.0 |
| 2 | `MembershipTiers` | Hạng thành viên | v1.0 |
| 3 | `Users` | Người dùng | v1.0 |
| 4 | `UserRoles` | Phân quyền user | v1.0 |
| 5 | `CourtTypes` | Loại sân | v1.0 |
| 6 | `Courts` | Thông tin sân | v1.0 |
| 7 | `CourtImages` | Hình ảnh sân | v1.0 |
| 8 | `TimeSlots` | Khung giờ | v1.0 |
| 9 | `CourtPricing` | Giá thuê theo khung giờ | v1.0 |
| 10 | `Promotions` | Mã khuyến mãi | v1.0 |
| 11 | `Bookings` | Đặt sân | v1.0 |
| 12 | `Services` | Dịch vụ bổ sung | v1.0 |
| 13 | `BookingServices` | Dịch vụ đi kèm booking | v1.0 |
| 14 | `Payments` | Thanh toán | v1.0 |
| 15 | `Reviews` | Đánh giá | v1.0 |
| 16 | `Notifications` | Thông báo | v1.0 |
| 17 | `CoachSchedules` | Lịch HLV | v1.0 |
| 18 | `AuditLogs` | Lịch sử thao tác | v1.0 |
| 19 | `RecurringBookings` | Đặt sân định kỳ | **v2.0** |
| 20 | `Waitlists` | Danh sách chờ | **v2.0** |
| 21 | `Invoices` | Hóa đơn | **v2.0** |
| 22 | `EquipmentInventory` | Kho dụng cụ | **v2.0** |
| 23 | `MaintenanceSchedules` | Lịch bảo trì sân | **v2.0** |
| 24 | `StaffShifts` | Ca làm việc nhân viên | **v2.0** |
| 25 | `PlayerRequests` | Tin tìm đối thủ | **v2.0** |
| 26 | `PlayerRequestMembers` | Thành viên tham gia | **v2.0** |
| 27 | `CourtComplexes` | Tổ hợp sân (Khu vực) | **v2.0** |
| 28 | `Tasks` | Quản lý công việc | **v2.0** |

---

## Chi tiết bảng quan trọng

### `Users`
| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| UserId | INT | PK, IDENTITY | Mã người dùng |
| FullName | NVARCHAR(100) | NOT NULL | Họ tên |
| Email | VARCHAR(100) | UNIQUE, NOT NULL | Email đăng nhập |
| Phone | VARCHAR(15) | NULL | Số điện thoại |
| PasswordHash | VARCHAR(255) | NOT NULL | BCrypt hash |
| LoyaltyPoints | INT | DEFAULT 0 | Điểm tích lũy |
| MembershipTierId | INT | FK | Hạng thành viên |
| RefreshToken | VARCHAR(500) | NULL | JWT Refresh Token |
| IsActive | BIT | DEFAULT 1 | Trạng thái |

### `Bookings`
| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| BookingId | INT | PK | Mã đặt sân |
| BookingCode | VARCHAR(20) | UNIQUE | Mã tham chiếu |
| UserId | INT | FK | Người đặt |
| CourtId | INT | FK | Sân được đặt |
| SlotId | INT | FK | Khung giờ |
| BookingDate | DATE | NOT NULL | Ngày đặt |
| StartTime | TIME | NOT NULL | Giờ bắt đầu |
| EndTime | TIME | NOT NULL | Giờ kết thúc |
| SubTotal | DECIMAL(18,2) | NOT NULL | Tiền trước giảm giá |
| DiscountAmount | DECIMAL(18,2) | DEFAULT 0 | Số tiền giảm |
| TotalAmount | DECIMAL(18,2) | NOT NULL | Tổng phải trả |
| Status | VARCHAR(30) | CHECK | Pending/Confirmed/Cancelled/Completed |
| PromotionId | INT | FK, NULL | Mã khuyến mãi áp dụng |

### `Payments`
| Cột | Kiểu | Ràng buộc | Mô tả |
|-----|------|-----------|-------|
| PaymentId | INT | PK | Mã thanh toán |
| BookingId | INT | FK | Booking liên quan |
| Amount | DECIMAL(18,2) | NOT NULL | Số tiền |
| PaymentMethod | VARCHAR(50) | CHECK | VNPay/MoMo/Cash/... |
| TransactionId | VARCHAR(200) | UNIQUE | Mã giao dịch cổng TT |
| GatewayResponse | NVARCHAR(MAX) | NULL | JSON response |
| Status | VARCHAR(20) | CHECK | Pending/Success/Failed/Refunded |
| RefundAmount | DECIMAL(18,2) | DEFAULT 0 | Số tiền hoàn |

---

## Quy tắc nghiệp vụ (Business Rules)

### Booking Status Flow
```
Pending → Confirmed → Completed
    │
    └─→ Cancelled (hoàn tiền theo chính sách)

NoShow (không đến, không hủy trước)
```

### Chính sách hoàn tiền
| Thời gian trước giờ chơi | Tỷ lệ hoàn |
|--------------------------|-----------|
| ≥ 24 giờ | 100% |
| 12 – 24 giờ | 50% |
| < 12 giờ | 0% |

### Trạng thái sân (Real-time)
| Status | Mô tả |
|--------|-------|
| `Available` | Sân đang trống |
| `Booked` | Đã có người đặt |
| `InUse` | Đang sử dụng |
| `Maintenance` | Đang bảo trì |
| `Inactive` | Ngưng hoạt động |

### Hạng thành viên
| Hạng | Điểm tối thiểu | Ưu đãi |
|------|---------------|--------|
| Bronze | 0 | Không |
| Silver | 500 điểm | Giảm 5% |
| Gold | 2,000 điểm | Giảm 10% + ưu tiên đặt |
| Platinum | 5,000 điểm | Giảm 15% + VIP |

> **Tích điểm:** 1,000 VNĐ thanh toán = 1 điểm

---

## Indexes

| Index | Bảng | Cột | Lý do |
|-------|------|-----|-------|
| IX_Bookings_UserId | Bookings | UserId | Query lịch sử user |
| IX_Bookings_CourtId | Bookings | CourtId | Kiểm tra lịch sân |
| IX_Bookings_BookingDate | Bookings | BookingDate | Filter theo ngày |
| IX_Bookings_Status | Bookings | Status | Filter trạng thái |
| IX_Payments_BookingId | Payments | BookingId | Join payment |
| IX_Courts_Status | Courts | Status | Filter sân trống |
| IX_Users_Email | Users | Email | Login lookup |
| IX_Notifications_UserId | Notifications | UserId, IsRead | Load thông báo |

| IX_RecurringBookings_UserId | RecurringBookings | UserId | Query lịch cố định |
| IX_Waitlists_CourtSlotDate | Waitlists | CourtId, SlotId, WaitDate | Kiểm tra hàng chờ |
| IX_MaintenanceSchedules_Court | MaintenanceSchedules | CourtId, Status | Lịch bảo trì |
| IX_StaffShifts_StaffDate | StaffShifts | StaffId, ShiftDate | Lịch ca nhân viên |
| IX_PlayerRequests_Status | PlayerRequests | Status | Danh sách tin mở |

---

## Bảng mới — v2.0

### `RecurringBookings` — Đặt sân định kỳ
| Cột | Kiểu | Mô tả |
|-----|------|-------|
| RecurringId | INT PK | Mã lịch định kỳ |
| UserId | INT FK | Người đặt |
| CourtId | INT FK | Sân |
| SlotId | INT FK | Khung giờ |
| StartDate / EndDate | DATE | Khoảng thời gian |
| DaysOfWeek | VARCHAR | '1,3,5' = T2, T4, T6 |
| Status | VARCHAR | Active/Paused/Cancelled |

### `Waitlists` — Danh sách chờ
| Cột | Kiểu | Mô tả |
|-----|------|-------|
| WaitlistId | INT PK | Mã hàng chờ |
| UserId | INT FK | Người chờ |
| CourtId + SlotId + WaitDate | FK + DATE | Slot cần chờ |
| Position | INT | Thứ tự trong hàng (FIFO) |
| Status | VARCHAR | Waiting/Notified/Confirmed/Expired |
| NotifiedAt | DATETIME | Thời điểm gửi thông báo |
| ExpiredAt | DATETIME | Hết hạn xác nhận (15 phút) |

### `Invoices` — Hóa đơn
| Cột | Kiểu | Mô tả |
|-----|------|-------|
| InvoiceId | INT PK | Mã hóa đơn |
| InvoiceNumber | VARCHAR UNIQUE | INV-20260514-0001 |
| BookingId + PaymentId | FK | Liên kết giao dịch |
| SubTotal | DECIMAL | Tiền trước giảm |
| DiscountAmount | DECIMAL | Số tiền giảm |
| VatPercent + VatAmount | DECIMAL | Thuế VAT |
| TotalAmount | DECIMAL | Tổng thực trả |
| PdfUrl | VARCHAR | Link file PDF |
| IsEmailSent | BIT | Đã gửi email chưa |

### `EquipmentInventory` — Kho dụng cụ
| Cột | Kiểu | Mô tả |
|-----|------|-------|
| InventoryId | INT PK | Mã item |
| ServiceId | INT FK | Dịch vụ liên quan |
| ItemCode | VARCHAR UNIQUE | Mã định danh vật phẩm |
| Condition | VARCHAR | Good/Damaged/Retired |
| PurchaseDate | DATE | Ngày mua |
| PurchasePrice | DECIMAL | Giá mua |
| IsAvailable | BIT | Đang có thể cho thuê |

### `MaintenanceSchedules` — Lịch bảo trì
| Cột | Kiểu | Mô tả |
|-----|------|-------|
| MaintenanceId | INT PK | Mã lịch bảo trì |
| CourtId | INT FK | Sân cần bảo trì |
| MaintenanceType | VARCHAR | Routine/Emergency/Upgrade |
| StartDateTime / EndDateTime | DATETIME | Thời gian bảo trì |
| AssignedStaffId | INT FK | Nhân viên phụ trách |
| Reason | NVARCHAR | Lý do bảo trì |
| Result | NVARCHAR | Kết quả sau bảo trì |
| Status | VARCHAR | Scheduled/InProgress/Completed |

### `StaffShifts` — Ca làm việc
| Cột | Kiểu | Mô tả |
|-----|------|-------|
| ShiftId | INT PK | Mã ca |
| StaffId | INT FK | Nhân viên |
| ShiftDate | DATE | Ngày làm |
| ShiftType | VARCHAR | Morning/Afternoon/Evening |
| StartTime / EndTime | TIME | Giờ ca |
| CheckInTime / CheckOutTime | DATETIME | Chấm công thực tế |

### `PlayerRequests` + `PlayerRequestMembers` — Tìm đối thủ
| Cột | Kiểu | Mô tả |
|-----|------|-------|
| RequestId | INT PK | Mã tin đăng |
| BookingId | INT FK | Booking gốc |
| HostUserId | INT FK | Người đăng tin |
| SkillLevel | VARCHAR | Beginner/Intermediate/Advanced |
| RequiredPlayers | INT | Số người cần thêm |
| GenderPref | VARCHAR | Male/Female/Any |
| Status | VARCHAR | Open/Full/Closed/Cancelled |

### `CourtComplexes` — Tổ hợp sân (Khu vực)
| Cột | Kiểu | Mô tả |
|-----|------|-------|
| ComplexId | INT PK | Mã tổ hợp sân |
| ComplexName | NVARCHAR(150) | Tên tổ hợp sân |
| Address | NVARCHAR(300) | Địa chỉ |
| ManagerId | INT FK | Quản lý tổ hợp (User) |
| Description | NVARCHAR(1000) | Mô tả |
| ImageUrl | VARCHAR(500) | Ảnh tổ hợp |
| IsDeleted | BIT | Trạng thái xóa mềm |
| CreatedAt | DATETIME | Ngày tạo |

### `Tasks` — Quản lý công việc
| Cột | Kiểu | Mô tả |
|-----|------|-------|
| TaskId | INT PK | Mã công việc |
| Title | NVARCHAR(150) | Tiêu đề công việc |
| Description | NVARCHAR(500) | Mô tả chi tiết |
| TaskType | VARCHAR(20) | Phân loại: Manual |
| Category | VARCHAR(30) | Danh mục: Cleanup/ServicePrep/Repair/Complaint |
| Priority | VARCHAR(20) | Độ ưu tiên: Urgent/High/Medium/Low |
| Status | VARCHAR(20) | Trạng thái: Pending/InProgress/Completed/Approved |
| ComplexId | INT FK | Thuộc tổ hợp sân (CourtComplexes) |
| AssignedStaffId | INT FK | Nhân viên thực hiện |
| CreatedById | INT FK | Người giao việc (Manager) |
| BookingId | INT FK | Liên kết booking |
| DueDate | DATETIME | Hạn hoàn thành |
| CreatedAt | DATETIME | Thời gian tạo |
| CompletedAt | DATETIME | Thời điểm hoàn thành |

---

## Cách chạy database

```bash
# Kết nối SQL Server và chạy toàn bộ
sqlcmd -S localhost -U sa -P YourPassword -i database/schema.sql

# Hoặc dùng EF Core Migration (Code First)
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

*Database Design — PRN232 Sports Court Management System — v2.0 (28 tables)*

