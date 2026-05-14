# Software Requirements Specification (SRS)
# Hệ Thống Quản Lý Sân Thể Thao (Sports Court Management System)

**Phiên bản:** 1.0  
**Ngày:** 13/05/2026  
**Môn học:** PRN232  

---

## Mục Lục

1. [Giới thiệu](#1-giới-thiệu)
2. [Mô tả tổng quan](#2-mô-tả-tổng-quan)
3. [Yêu cầu chức năng](#3-yêu-cầu-chức-năng)
4. [Yêu cầu phi chức năng](#4-yêu-cầu-phi-chức-năng)
5. [Thiết kế cơ sở dữ liệu](#5-thiết-kế-cơ-sở-dữ-liệu)
6. [Giao diện hệ thống](#6-giao-diện-hệ-thống)

---

## 1. Giới thiệu

### 1.1 Mục đích
Tài liệu này mô tả chi tiết các yêu cầu phần mềm cho **Hệ thống Quản lý Sân Thể thao**. Hệ thống nhằm số hóa toàn bộ quy trình quản lý và đặt sân thể thao (bóng đá, pickleball, cầu lông,...), giúp tối ưu vận hành, giảm sai sót và nâng cao trải nghiệm khách hàng.

### 1.2 Phạm vi
Hệ thống bao gồm:
- Quản lý thông tin sân, loại sân, khung giờ, giá thuê, trạng thái sân.
- Đặt sân trực tuyến, thanh toán, gửi xác nhận.
- Quản trị: quản lý sân, khách hàng, dịch vụ, doanh thu, phân quyền.

### 1.3 Đối tượng sử dụng

| Vai trò | Mô tả |
|---------|-------|
| **Khách hàng (Customer)** | Tìm kiếm, đặt sân, thanh toán, đánh giá |
| **Quản trị viên (Admin)** | Quản lý toàn bộ hệ thống |
| **Nhân viên (Staff)** | Quản lý đặt sân, hỗ trợ khách hàng |
| **Huấn luyện viên (Coach)** | Quản lý lịch dạy, dịch vụ huấn luyện |

### 1.4 Công nghệ sử dụng

| Thành phần | Công nghệ |
|------------|-----------|
| Backend | ASP.NET Core Web API (.NET 7/8) |
| Frontend | React.js / Blazor |
| Database | SQL Server |
| Authentication | JWT Token |
| Payment | VNPay / MoMo API |
| Realtime | SignalR |
| Email | SMTP / SendGrid |

---

## 2. Mô tả tổng quan

### 2.1 Bối cảnh
Nhiều cơ sở thể thao vẫn quản lý thủ công qua sổ sách, tin nhắn hoặc cuộc gọi, gây ra:
- Trùng lịch đặt sân
- Khó kiểm soát tình trạng sân
- Sai sót trong thanh toán
- Mất thời gian vận hành

### 2.2 Mục tiêu
- Tối ưu quy trình vận hành cho cơ sở thể thao.
- Giảm thiểu sai sót trong quản lý đặt sân và thanh toán.
- Nâng cao trải nghiệm khách hàng khi đặt sân trực tuyến.
- Tăng hiệu quả kinh doanh thông qua thống kê, báo cáo.

### 2.3 Sơ đồ Use Case tổng quan

```
+------------------+
|    Khách hàng    |
+--------+---------+
         |
         |-- Đăng ký / Đăng nhập
         |-- Tìm kiếm sân
         |-- Đặt sân
         |-- Thanh toán
         |-- Xem lịch sử đặt sân
         |-- Đánh giá / Phản hồi
         |-- Sử dụng mã giảm giá

+------------------+
|   Quản trị viên  |
+--------+---------+
         |
         |-- Quản lý sân thể thao
         |-- Quản lý giá thuê / khung giờ
         |-- Quản lý đặt sân
         |-- Quản lý khách hàng
         |-- Quản lý dịch vụ bổ sung
         |-- Thống kê doanh thu
         |-- Phân quyền người dùng
         |-- Quản lý khuyến mãi
```

---

## 3. Yêu cầu chức năng

### FE-01: Tìm kiếm và đặt sân thể thao

| Mục | Chi tiết |
|-----|----------|
| **Mô tả** | Cho phép khách hàng tìm kiếm và đặt sân dựa trên loại sân, ngày, giờ và tình trạng trống |
| **Actor** | Khách hàng |
| **Precondition** | Khách hàng đã đăng nhập |
| **Postcondition** | Hiển thị danh sách sân phù hợp |

**Luồng chính:**
1. Khách hàng chọn loại sân (bóng đá, pickleball, cầu lông,...).
2. Chọn ngày và khung giờ mong muốn.
3. Hệ thống hiển thị danh sách sân còn trống.
4. Khách hàng chọn sân và xác nhận đặt.

**Luồng ngoại lệ:**
- Không có sân trống → Hệ thống gợi ý khung giờ/ngày khác.

---

### FE-02: Quản lý đặt sân

| Mục | Chi tiết |
|-----|----------|
| **Mô tả** | Tạo mới, xem, chỉnh sửa, đổi lịch và hủy đặt sân |
| **Actor** | Khách hàng, Nhân viên, Admin |
| **Precondition** | Đã đăng nhập vào hệ thống |
| **Postcondition** | Thông tin đặt sân được cập nhật |

**Luồng chính:**
1. Người dùng xem danh sách booking của mình.
2. Chọn booking cần thao tác (xem/sửa/hủy/đổi lịch).
3. Hệ thống kiểm tra điều kiện (thời gian hủy, sân trống,...).
4. Cập nhật trạng thái booking.

**Quy tắc nghiệp vụ:**
- Hủy đặt sân trước 24h: hoàn tiền 100%.
- Hủy trước 12h: hoàn tiền 50%.
- Hủy dưới 12h: không hoàn tiền.

---

### FE-03: Thanh toán trực tuyến

| Mục | Chi tiết |
|-----|----------|
| **Mô tả** | Hỗ trợ thanh toán cho dịch vụ đặt sân và dịch vụ đi kèm |
| **Actor** | Khách hàng |
| **Precondition** | Đã tạo booking thành công |
| **Postcondition** | Thanh toán được xử lý, hóa đơn được tạo |

**Phương thức thanh toán:**
- VNPay
- MoMo
- Chuyển khoản ngân hàng
- Thanh toán tại quầy

**Luồng chính:**
1. Hệ thống hiển thị tổng chi phí (sân + dịch vụ).
2. Khách hàng chọn phương thức thanh toán.
3. Thực hiện thanh toán qua cổng thanh toán.
4. Hệ thống xác nhận và tạo hóa đơn.

---

### FE-04: Quản lý thông tin sân thể thao

| Mục | Chi tiết |
|-----|----------|
| **Mô tả** | Quản lý loại sân, giá thuê, giờ hoạt động, trạng thái, bảo trì |
| **Actor** | Admin |
| **Precondition** | Đăng nhập với quyền Admin |
| **Postcondition** | Thông tin sân được cập nhật |

**Thông tin sân bao gồm:**
- Tên sân, mã sân
- Loại sân (bóng đá, pickleball, cầu lông,...)
- Kích thước, sức chứa
- Giờ hoạt động
- Giá thuê theo khung giờ
- Trạng thái: Hoạt động / Bảo trì / Ngưng hoạt động
- Hình ảnh, mô tả

---

### FE-05: Cập nhật trạng thái sân theo thời gian thực

| Mục | Chi tiết |
|-----|----------|
| **Mô tả** | Cập nhật realtime trạng thái sân để tránh trùng lịch |
| **Actor** | Hệ thống (tự động) |
| **Công nghệ** | SignalR |

**Trạng thái sân:**
- `Available` – Còn trống
- `Booked` – Đã đặt
- `InUse` – Đang sử dụng
- `Maintenance` – Đang bảo trì

---

### FE-06: Quản lý khách hàng

| Mục | Chi tiết |
|-----|----------|
| **Mô tả** | Quản lý thông tin khách hàng, lịch sử, thành viên, khách hàng thân thiết |
| **Actor** | Admin, Nhân viên |

**Chức năng:**
- Xem danh sách khách hàng.
- Xem lịch sử đặt sân của khách hàng.
- Quản lý tài khoản thành viên.
- Phân hạng khách hàng thân thiết (Bronze, Silver, Gold, Platinum).

---

### FE-07: Quản lý dịch vụ bổ sung

| Mục | Chi tiết |
|-----|----------|
| **Mô tả** | Quản lý thuê dụng cụ, nước uống, huấn luyện viên, sự kiện |
| **Actor** | Admin, Nhân viên |

**Danh mục dịch vụ:**

| Dịch vụ | Mô tả |
|---------|-------|
| Thuê dụng cụ | Vợt, bóng, giày, lưới,... |
| Nước uống | Nước suối, nước tăng lực,... |
| Huấn luyện viên | Đặt lịch dạy kèm |
| Tổ chức sự kiện | Giải đấu, team building |

---

### FE-08: Gửi thông báo

| Mục | Chi tiết |
|-----|----------|
| **Mô tả** | Gửi xác nhận đặt sân, hóa đơn, nhắc lịch, thông báo hủy |
| **Kênh** | Email, In-app notification |

**Loại thông báo:**
- Xác nhận đặt sân thành công
- Hóa đơn thanh toán
- Nhắc lịch trước 1 giờ
- Thông báo hủy lịch
- Thông báo khuyến mãi

---

### FE-09: Thống kê và báo cáo

| Mục | Chi tiết |
|-----|----------|
| **Mô tả** | Thống kê doanh thu, tần suất đặt sân, khung giờ cao điểm |
| **Actor** | Admin |

**Báo cáo bao gồm:**
- Doanh thu theo ngày / tuần / tháng / năm.
- Tỷ lệ sử dụng sân (%).
- Khung giờ cao điểm.
- Top khách hàng.
- Doanh thu theo loại sân.
- Biểu đồ xu hướng.

---

### FE-10: Quản lý vai trò và phân quyền

| Mục | Chi tiết |
|-----|----------|
| **Mô tả** | Phân quyền truy cập cho các vai trò |
| **Actor** | Admin |

**Ma trận phân quyền:**

| Chức năng | Admin | Staff | Coach | Customer |
|-----------|:-----:|:-----:|:-----:|:--------:|
| Quản lý sân | ✅ | ❌ | ❌ | ❌ |
| Quản lý đặt sân | ✅ | ✅ | ❌ | ✅ (của mình) |
| Quản lý khách hàng | ✅ | ✅ | ❌ | ❌ |
| Thống kê doanh thu | ✅ | ❌ | ❌ | ❌ |
| Quản lý dịch vụ | ✅ | ✅ | ❌ | ❌ |
| Quản lý lịch dạy | ✅ | ❌ | ✅ | ❌ |
| Đặt sân | ✅ | ✅ | ✅ | ✅ |
| Đánh giá | ❌ | ❌ | ❌ | ✅ |
| Quản lý khuyến mãi | ✅ | ❌ | ❌ | ❌ |

---

### FE-11: Quản lý khuyến mãi

| Mục | Chi tiết |
|-----|----------|
| **Mô tả** | Quản lý mã giảm giá, ưu đãi theo mùa, gói thành viên |
| **Actor** | Admin |

**Loại khuyến mãi:**
- Mã giảm giá (% hoặc số tiền cố định).
- Ưu đãi theo mùa / dịp lễ.
- Gói thành viên (tháng / quý / năm).
- Ưu đãi khách hàng thân thiết.

---

### FE-12: Đánh giá và phản hồi

| Mục | Chi tiết |
|-----|----------|
| **Mô tả** | Khách hàng đánh giá chất lượng sân, dịch vụ sau khi sử dụng |
| **Actor** | Khách hàng |
| **Precondition** | Đã hoàn thành booking |

**Thông tin đánh giá:**
- Xếp hạng sao (1-5).
- Nhận xét văn bản.
- Hình ảnh đính kèm (tùy chọn).
- Thời gian đánh giá.

---

## 4. Yêu cầu phi chức năng

### 4.1 Hiệu năng

| Yêu cầu | Chỉ tiêu |
|----------|----------|
| Thời gian phản hồi API | < 2 giây |
| Hỗ trợ người dùng đồng thời | ≥ 500 |
| Uptime | ≥ 99.5% |

### 4.2 Bảo mật
- Mã hóa mật khẩu bằng BCrypt.
- Xác thực bằng JWT Token.
- Phân quyền dựa trên vai trò (RBAC).
- HTTPS cho toàn bộ API.
- Bảo vệ chống SQL Injection, XSS, CSRF.

### 4.3 Khả năng mở rộng
- Kiến trúc microservices-ready.
- Database có thể scale theo nhu cầu.
- Hỗ trợ thêm loại sân mới mà không cần thay đổi cấu trúc.

### 4.4 Tính khả dụng
- Giao diện responsive (Desktop, Tablet, Mobile).
- Hỗ trợ tiếng Việt và tiếng Anh.
- UI/UX thân thiện, dễ sử dụng.

---

## 5. Thiết kế cơ sở dữ liệu

### 5.1 Danh sách bảng chính

| STT | Tên bảng | Mô tả |
|-----|----------|-------|
| 1 | Users | Thông tin người dùng |
| 2 | Roles | Vai trò trong hệ thống |
| 3 | UserRoles | Quan hệ User - Role |
| 4 | CourtTypes | Loại sân (bóng đá, pickleball,...) |
| 5 | Courts | Thông tin sân |
| 6 | TimeSlots | Khung giờ hoạt động |
| 7 | CourtPricing | Giá thuê theo khung giờ / ngày |
| 8 | Bookings | Thông tin đặt sân |
| 9 | BookingDetails | Chi tiết đặt sân |
| 10 | Payments | Thanh toán |
| 11 | Services | Dịch vụ bổ sung |
| 12 | BookingServices | Dịch vụ đi kèm booking |
| 13 | Reviews | Đánh giá |
| 14 | Promotions | Khuyến mãi |
| 15 | Notifications | Thông báo |
| 16 | MembershipTiers | Hạng thành viên |

### 5.2 Sơ đồ ERD (Entity Relationship)

```
Users ──< UserRoles >── Roles
  │
  ├──< Bookings >── Courts ──> CourtTypes
  │       │              │
  │       ├──< BookingDetails >── TimeSlots
  │       │
  │       ├──< BookingServices >── Services
  │       │
  │       └──< Payments
  │
  ├──< Reviews >── Courts
  │
  └──> MembershipTiers

Courts ──< CourtPricing >── TimeSlots

Promotions ──< Bookings
```

### 5.3 Chi tiết bảng chính

#### Users
| Cột | Kiểu dữ liệu | Mô tả |
|-----|---------------|-------|
| UserId | INT (PK) | Mã người dùng |
| FullName | NVARCHAR(100) | Họ tên |
| Email | VARCHAR(100) | Email (unique) |
| Phone | VARCHAR(15) | Số điện thoại |
| PasswordHash | VARCHAR(255) | Mật khẩu mã hóa |
| AvatarUrl | VARCHAR(500) | Ảnh đại diện |
| MembershipTierId | INT (FK) | Hạng thành viên |
| IsActive | BIT | Trạng thái tài khoản |
| CreatedAt | DATETIME | Ngày tạo |

#### Courts
| Cột | Kiểu dữ liệu | Mô tả |
|-----|---------------|-------|
| CourtId | INT (PK) | Mã sân |
| CourtName | NVARCHAR(100) | Tên sân |
| CourtTypeId | INT (FK) | Loại sân |
| Description | NVARCHAR(500) | Mô tả |
| Location | NVARCHAR(200) | Vị trí |
| ImageUrl | VARCHAR(500) | Hình ảnh |
| Status | VARCHAR(20) | Trạng thái |
| OpenTime | TIME | Giờ mở cửa |
| CloseTime | TIME | Giờ đóng cửa |
| CreatedAt | DATETIME | Ngày tạo |

#### Bookings
| Cột | Kiểu dữ liệu | Mô tả |
|-----|---------------|-------|
| BookingId | INT (PK) | Mã booking |
| UserId | INT (FK) | Người đặt |
| CourtId | INT (FK) | Sân được đặt |
| BookingDate | DATE | Ngày đặt sân |
| TotalAmount | DECIMAL(18,2) | Tổng tiền |
| Status | VARCHAR(20) | Trạng thái booking |
| PromotionId | INT (FK, nullable) | Mã khuyến mãi |
| Note | NVARCHAR(500) | Ghi chú |
| CreatedAt | DATETIME | Ngày tạo |

#### Payments
| Cột | Kiểu dữ liệu | Mô tả |
|-----|---------------|-------|
| PaymentId | INT (PK) | Mã thanh toán |
| BookingId | INT (FK) | Mã booking |
| Amount | DECIMAL(18,2) | Số tiền |
| PaymentMethod | VARCHAR(50) | Phương thức |
| TransactionId | VARCHAR(100) | Mã giao dịch |
| Status | VARCHAR(20) | Trạng thái |
| PaidAt | DATETIME | Thời gian thanh toán |

---

## 6. Giao diện hệ thống

### 6.1 Màn hình chính

| STT | Màn hình | Mô tả |
|-----|----------|-------|
| 1 | Trang chủ | Giới thiệu, tìm kiếm sân nhanh |
| 2 | Tìm kiếm sân | Bộ lọc loại sân, ngày, giờ |
| 3 | Chi tiết sân | Thông tin, hình ảnh, giá, đánh giá |
| 4 | Đặt sân | Chọn khung giờ, dịch vụ, thanh toán |
| 5 | Lịch sử đặt sân | Danh sách booking của khách hàng |
| 6 | Dashboard Admin | Tổng quan thống kê |
| 7 | Quản lý sân | CRUD sân thể thao |
| 8 | Quản lý booking | Danh sách và xử lý booking |
| 9 | Báo cáo doanh thu | Biểu đồ và bảng thống kê |
| 10 | Quản lý người dùng | Danh sách và phân quyền |

---

*Tài liệu được tạo ngày 13/05/2026 — Phiên bản 1.0*
