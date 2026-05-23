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

| Vai trò                     | Mô tả                                                             |
| --------------------------- | ----------------------------------------------------------------- |
| **Khách hàng (Customer)**   | Tìm kiếm, đặt sân, thanh toán, đánh giá                           |
| **Quản trị viên (Admin)**   | Quản lý toàn bộ hệ thống                                          |
| **Quản lý (Manager)**       | Quản lý từng tổ hợp sân cụ thể, quản lý nhân viên thuộc tổ hợp đó |
| **Nhân viên (Staff)**       | Quản lý đặt sân, hỗ trợ khách hàng                                |
| **Huấn luyện viên (Coach)** | Quản lý lịch dạy, dịch vụ huấn luyện                              |

### 1.4 Công nghệ sử dụng

| Thành phần     | Công nghệ                       |
| -------------- | ------------------------------- |
| Backend        | ASP.NET Core Web API (.NET 7/8) |
| Frontend       | React.js / Blazor               |
| Database       | SQL Server                      |
| Authentication | JWT Token                       |
| Payment        | VNPay / MoMo API                |
| Realtime       | SignalR                         |
| Email          | SMTP / SendGrid                 |

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

+------------------+
|      Quản lý     |
+--------+---------+
         |
         |-- Quản lý nhân viên khu vực
         |-- Theo dõi lịch ca nhân viên
         |-- Xem báo cáo doanh thu khu vực
         |-- Quản lý tình trạng sân khu vực
         |-- Phê duyệt lịch đặt sân đặc biệt
         |-- Quản lý và giao việc cho nhân viên
```

---

## 3. Yêu cầu chức năng

### FE-01: Tìm kiếm và đặt sân thể thao

| Mục               | Chi tiết                                                                                 |
| ----------------- | ---------------------------------------------------------------------------------------- |
| **Mô tả**         | Cho phép khách hàng tìm kiếm và đặt sân dựa trên loại sân, ngày, giờ và tình trạng trống |
| **Actor**         | Khách hàng                                                                               |
| **Precondition**  | Khách hàng đã đăng nhập                                                                  |
| **Postcondition** | Hiển thị danh sách sân phù hợp                                                           |

**Luồng chính:**
1. Khách hàng chọn loại sân (bóng đá, pickleball, cầu lông,...).
2. Chọn ngày và khung giờ mong muốn.
3. Hệ thống hiển thị danh sách sân còn trống.
4. Khách hàng chọn sân và xác nhận đặt.

**Luồng ngoại lệ:**
- Không có sân trống → Hệ thống gợi ý khung giờ/ngày khác.

---

### FE-02: Quản lý đặt sân

| Mục               | Chi tiết                                         |
| ----------------- | ------------------------------------------------ |
| **Mô tả**         | Tạo mới, xem, chỉnh sửa, đổi lịch và hủy đặt sân |
| **Actor**         | Khách hàng, Nhân viên, Admin                     |
| **Precondition**  | Đã đăng nhập vào hệ thống                        |
| **Postcondition** | Thông tin đặt sân được cập nhật                  |

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

| Mục               | Chi tiết                                                |
| ----------------- | ------------------------------------------------------- |
| **Mô tả**         | Hỗ trợ thanh toán cho dịch vụ đặt sân và dịch vụ đi kèm |
| **Actor**         | Khách hàng                                              |
| **Precondition**  | Đã tạo booking thành công                               |
| **Postcondition** | Thanh toán được xử lý, hóa đơn được tạo                 |

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

| Mục               | Chi tiết                                                       |
| ----------------- | -------------------------------------------------------------- |
| **Mô tả**         | Quản lý loại sân, giá thuê, giờ hoạt động, trạng thái, bảo trì |
| **Actor**         | Admin                                                          |
| **Precondition**  | Đăng nhập với quyền Admin                                      |
| **Postcondition** | Thông tin sân được cập nhật                                    |

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

| Mục           | Chi tiết                                             |
| ------------- | ---------------------------------------------------- |
| **Mô tả**     | Cập nhật realtime trạng thái sân để tránh trùng lịch |
| **Actor**     | Hệ thống (tự động)                                   |
| **Công nghệ** | SignalR                                              |

**Trạng thái sân:**
- `Available` – Còn trống
- `Booked` – Đã đặt
- `InUse` – Đang sử dụng
- `Maintenance` – Đang bảo trì

---

### FE-06: Quản lý khách hàng

| Mục       | Chi tiết                                                                 |
| --------- | ------------------------------------------------------------------------ |
| **Mô tả** | Quản lý thông tin khách hàng, lịch sử, thành viên, khách hàng thân thiết |
| **Actor** | Admin, Nhân viên                                                         |

**Chức năng:**
- Xem danh sách khách hàng.
- Xem lịch sử đặt sân của khách hàng.
- Quản lý tài khoản thành viên.
- Phân hạng khách hàng thân thiết (Bronze, Silver, Gold, Platinum).

---

### FE-07: Quản lý dịch vụ bổ sung

| Mục       | Chi tiết                                                  |
| --------- | --------------------------------------------------------- |
| **Mô tả** | Quản lý thuê dụng cụ, nước uống, huấn luyện viên, sự kiện |
| **Actor** | Admin, Nhân viên                                          |

**Danh mục dịch vụ:**

| Dịch vụ         | Mô tả                        |
| --------------- | ---------------------------- |
| Thuê dụng cụ    | Vợt, bóng, giày, lưới,...    |
| Nước uống       | Nước suối, nước tăng lực,... |
| Huấn luyện viên | Đặt lịch dạy kèm             |
| Tổ chức sự kiện | Giải đấu, team building      |

---

### FE-08: Gửi thông báo

| Mục       | Chi tiết                                                |
| --------- | ------------------------------------------------------- |
| **Mô tả** | Gửi xác nhận đặt sân, hóa đơn, nhắc lịch, thông báo hủy |
| **Kênh**  | Email, In-app notification                              |

**Loại thông báo:**
- Xác nhận đặt sân thành công
- Hóa đơn thanh toán
- Nhắc lịch trước 1 giờ
- Thông báo hủy lịch
- Thông báo khuyến mãi

---

### FE-09: Thống kê và báo cáo

| Mục       | Chi tiết                                                 |
| --------- | -------------------------------------------------------- |
| **Mô tả** | Thống kê doanh thu, tần suất đặt sân, khung giờ cao điểm |
| **Actor** | Admin                                                    |

**Báo cáo bao gồm:**
- Doanh thu theo ngày / tuần / tháng / năm.
- Tỷ lệ sử dụng sân (%).
- Khung giờ cao điểm.
- Top khách hàng.
- Doanh thu theo loại sân.
- Biểu đồ xu hướng.

---

### FE-10: Quản lý vai trò và phân quyền

| Mục       | Chi tiết                            |
| --------- | ----------------------------------- |
| **Mô tả** | Phân quyền truy cập cho các vai trò |
| **Actor** | Admin                               |

**Ma trận phân quyền:**

| Chức năng          | Admin |   Manager   | Staff | Coach |   Customer   |
| ------------------ | :---: | :---------: | :---: | :---: | :----------: |
| Quản lý sân        |   ✅   | ✅ (khu vực) |   ❌   |   ❌   |      ❌       |
| Quản lý đặt sân    |   ✅   |      ✅      |   ✅   |   ❌   | ✅ (của mình) |
| Quản lý khách hàng |   ✅   |      ✅      |   ✅   |   ❌   |      ❌       |
| Thống kê doanh thu |   ✅   | ✅ (khu vực) |   ❌   |   ❌   |      ❌       |
| Quản lý dịch vụ    |   ✅   |      ✅      |   ✅   |   ❌   |      ❌       |
| Quản lý lịch dạy   |   ✅   |      ✅      |   ❌   |   ✅   |      ❌       |
| Đặt sân            |   ✅   |      ✅      |   ✅   |   ✅   |      ✅       |
| Đánh giá           |   ❌   |      ❌      |   ❌   |   ❌   |      ✅       |
| Quản lý khuyến mãi |   ✅   |      ❌      |   ❌   |   ❌   |      ❌       |
| Quản lý nhân viên  |   ✅   | ✅ (khu vực) |   ❌   |   ❌   |      ❌       |

---

### FE-11: Quản lý khuyến mãi

| Mục       | Chi tiết                                             |
| --------- | ---------------------------------------------------- |
| **Mô tả** | Quản lý mã giảm giá, ưu đãi theo mùa, gói thành viên |
| **Actor** | Admin                                                |

**Loại khuyến mãi:**
- Mã giảm giá (% hoặc số tiền cố định).
- Ưu đãi theo mùa / dịp lễ.
- Gói thành viên (tháng / quý / năm).
- Ưu đãi khách hàng thân thiết.

---

### FE-12: Đánh giá và phản hồi

| Mục              | Chi tiết                                                    |
| ---------------- | ----------------------------------------------------------- |
| **Mô tả**        | Khách hàng đánh giá chất lượng sân, dịch vụ sau khi sử dụng |
| **Actor**        | Khách hàng                                                  |
| **Precondition** | Đã hoàn thành booking                                       |

**Thông tin đánh giá:**
- Xếp hạng sao (1-5).
- Nhận xét văn bản.
- Hình ảnh đính kèm (tùy chọn).
- Thời gian đánh giá.

---

## 4. Yêu cầu phi chức năng

### 4.1 Hiệu năng

| Yêu cầu                     | Chỉ tiêu |
| --------------------------- | -------- |
| Thời gian phản hồi API      | < 2 giây |
| Hỗ trợ người dùng đồng thời | ≥ 500    |
| Uptime                      | ≥ 99.5%  |

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

| STT | Tên bảng        | Mô tả                              |
| --- | --------------- | ---------------------------------- |
| 1   | Users           | Thông tin người dùng               |
| 2   | Roles           | Vai trò trong hệ thống             |
| 3   | UserRoles       | Quan hệ User - Role                |
| 4   | CourtTypes      | Loại sân (bóng đá, pickleball,...) |
| 5   | Courts          | Thông tin sân                      |
| 6   | TimeSlots       | Khung giờ hoạt động                |
| 7   | CourtPricing    | Giá thuê theo khung giờ / ngày     |
| 8   | Bookings        | Thông tin đặt sân                  |
| 9   | BookingDetails  | Chi tiết đặt sân                   |
| 10  | Payments        | Thanh toán                         |
| 11  | Services        | Dịch vụ bổ sung                    |
| 12  | BookingServices | Dịch vụ đi kèm booking             |
| 13  | Reviews         | Đánh giá                           |
| 14  | Promotions      | Khuyến mãi                         |
| 15  | Notifications   | Thông báo                          |
| 16  | MembershipTiers | Hạng thành viên                    |

### 5.2 Sơ đồ ERD (Entity Relationship)

```
Users ──< UserRoles >── Roles
  │
  ├──< Bookings >── Courts ──> CourtTypes
  │       │              │
  │       ├──< BookingDetails >── TimeSlots
  │       │
  │       ├──< BookingServices >── Services ──< EquipmentInventory
  │       │
  │       ├──< Payments ──< Invoices
  │       │
  │       └── RecurringBookings
  │
  ├──< Waitlists >── Courts
  │
  ├──< Reviews >── Courts
  │
  ├──< PlayerRequests >── Bookings
  │
  ├──< StaffShifts (Staff only)
  │
  └──> MembershipTiers

Complexes ──< Courts
Complexes ──< Users (Manager/Staff)
Courts ──< CourtPricing >── TimeSlots
Courts ──< MaintenanceSchedules
Promotions ──< Bookings
```

### 5.3 Chi tiết bảng chính

#### Complexes
| Cột         | Kiểu dữ liệu  | Mô tả                                   |
| ----------- | ------------- | --------------------------------------- |
| ComplexId   | INT (PK)      | Mã tổ hợp sân                           |
| ComplexName | NVARCHAR(100) | Tên tổ hợp sân (VD: Khu A, Khu B)       |
| Address     | NVARCHAR(200) | Địa chỉ cụ thể                          |
| ManagerId   | INT (FK)      | Mã người quản lý (User có role Manager) |
| CreatedAt   | DATETIME      | Ngày tạo                                |

#### Users
| Cột              | Kiểu dữ liệu       | Mô tả                                       |
| ---------------- | ------------------ | ------------------------------------------- |
| UserId           | INT (PK)           | Mã người dùng                               |
| FullName         | NVARCHAR(100)      | Họ tên                                      |
| Email            | VARCHAR(100)       | Email (unique)                              |
| Phone            | VARCHAR(15)        | Số điện thoại                               |
| PasswordHash     | VARCHAR(255)       | Mật khẩu mã hóa                             |
| AvatarUrl        | VARCHAR(500)       | Ảnh đại diện                                |
| MembershipTierId | INT (FK)           | Hạng thành viên                             |
| ComplexId        | INT (FK, Nullable) | Tổ hợp sân quản lý (Dành cho Manager/Staff) |
| IsActive         | BIT                | Trạng thái tài khoản                        |
| CreatedAt        | DATETIME           | Ngày tạo                                    |

#### Courts
| Cột         | Kiểu dữ liệu  | Mô tả                |
| ----------- | ------------- | -------------------- |
| CourtId     | INT (PK)      | Mã sân               |
| ComplexId   | INT (FK)      | Thuộc tổ hợp sân nào |
| CourtName   | NVARCHAR(100) | Tên sân              |
| CourtTypeId | INT (FK)      | Loại sân             |
| Description | NVARCHAR(500) | Mô tả                |
| Location    | NVARCHAR(200) | Vị trí               |
| ImageUrl    | VARCHAR(500)  | Hình ảnh             |
| Status      | VARCHAR(20)   | Trạng thái           |
| OpenTime    | TIME          | Giờ mở cửa           |
| CloseTime   | TIME          | Giờ đóng cửa         |
| CreatedAt   | DATETIME      | Ngày tạo             |

#### Bookings
| Cột         | Kiểu dữ liệu       | Mô tả              |
| ----------- | ------------------ | ------------------ |
| BookingId   | INT (PK)           | Mã booking         |
| UserId      | INT (FK)           | Người đặt          |
| CourtId     | INT (FK)           | Sân được đặt       |
| BookingDate | DATE               | Ngày đặt sân       |
| TotalAmount | DECIMAL(18,2)      | Tổng tiền          |
| Status      | VARCHAR(20)        | Trạng thái booking |
| PromotionId | INT (FK, nullable) | Mã khuyến mãi      |
| Note        | NVARCHAR(500)      | Ghi chú            |
| CreatedAt   | DATETIME           | Ngày tạo           |

#### Payments
| Cột           | Kiểu dữ liệu  | Mô tả                |
| ------------- | ------------- | -------------------- |
| PaymentId     | INT (PK)      | Mã thanh toán        |
| BookingId     | INT (FK)      | Mã booking           |
| Amount        | DECIMAL(18,2) | Số tiền              |
| PaymentMethod | VARCHAR(50)   | Phương thức          |
| TransactionId | VARCHAR(100)  | Mã giao dịch         |
| Status        | VARCHAR(20)   | Trạng thái           |
| PaidAt        | DATETIME      | Thời gian thanh toán |

---

## 6. Giao diện hệ thống

### 6.1 Màn hình chính

| STT | Màn hình           | Mô tả                               |
| --- | ------------------ | ----------------------------------- |
| 1   | Trang chủ          | Giới thiệu, tìm kiếm sân nhanh      |
| 2   | Tìm kiếm sân       | Bộ lọc loại sân, ngày, giờ          |
| 3   | Chi tiết sân       | Thông tin, hình ảnh, giá, đánh giá  |
| 4   | Đặt sân            | Chọn khung giờ, dịch vụ, thanh toán |
| 5   | Lịch sử đặt sân    | Danh sách booking của khách hàng    |
| 6   | Dashboard Admin    | Tổng quan thống kê                  |
| 7   | Quản lý sân        | CRUD sân thể thao                   |
| 8   | Quản lý booking    | Danh sách và xử lý booking          |
| 9   | Báo cáo doanh thu  | Biểu đồ và bảng thống kê            |
| 10  | Quản lý người dùng | Danh sách và phân quyền             |

---

### FE-13: Đặt sân định kỳ (Recurring Booking)

| Mục               | Chi tiết                                                                     |
| ----------------- | ---------------------------------------------------------------------------- |
| **Mô tả**         | Cho phép khách hàng đặt sân lặp lại theo tuần trong khoảng thời gian cố định |
| **Actor**         | Khách hàng                                                                   |
| **Precondition**  | Đã đăng nhập, sân còn trống trong các slot được chọn                         |
| **Postcondition** | Hệ thống tự động tạo nhiều booking theo lịch đã thiết lập                    |

**Luồng chính:**
1. Khách chọn sân, khung giờ, ngày bắt đầu và ngày kết thúc.
2. Chọn các ngày trong tuần lặp lại (Thứ 2, Thứ 4, Thứ 6,...).
3. Hệ thống kiểm tra toàn bộ slot còn trống.
4. Khách xác nhận và thanh toán (toàn bộ hoặc từng buổi).
5. Hệ thống sinh tự động các `Booking` tương ứng.

**Quy tắc nghiệp vụ:**
- Nếu một slot bị trùng, hệ thống thông báo và hỏi bỏ qua hoặc hủy toàn bộ.
- Hủy recurring booking áp dụng chính sách hoàn tiền theo từng buổi.
- Cho phép hủy từng buổi riêng lẻ trong lịch định kỳ.

---

### FE-14: Danh sách chờ (Waitlist)

| Mục               | Chi tiết                                                                          |
| ----------------- | --------------------------------------------------------------------------------- |
| **Mô tả**         | Khi sân đã đầy, khách đăng ký hàng chờ và được tự động thông báo khi có chỗ trống |
| **Actor**         | Khách hàng, Hệ thống                                                              |
| **Precondition**  | Sân đã được đặt kín trong khung giờ mong muốn                                     |
| **Postcondition** | Khách vào hàng chờ, được thông báo khi booking bị hủy                             |

**Luồng chính:**
1. Khách tìm sân — hệ thống báo "Đã đặt kín".
2. Khách chọn "Vào danh sách chờ".
3. Hệ thống lưu vị trí trong hàng (FIFO).
4. Khi có booking bị hủy → hệ thống gửi thông báo cho người đầu hàng chờ.
5. Người nhận thông báo có **15 phút** để xác nhận đặt.
6. Nếu không xác nhận → chuyển sang người tiếp theo.

**Quy tắc nghiệp vụ:**
- Mỗi khách chỉ được vào waitlist 1 lần cho 1 slot.
- Waitlist tự động hết hạn khi qua ngày đặt sân.

---

### FE-15: Xuất hóa đơn (Invoice)

| Mục               | Chi tiết                                                   |
| ----------------- | ---------------------------------------------------------- |
| **Mô tả**         | Tạo và xuất hóa đơn chi tiết sau khi thanh toán thành công |
| **Actor**         | Hệ thống (tự động), Khách hàng                             |
| **Precondition**  | Thanh toán thành công                                      |
| **Postcondition** | Hóa đơn được tạo và gửi qua email                          |

**Thông tin hóa đơn:**
- Mã hóa đơn (INV-YYYYMMDD-XXXX)
- Thông tin khách hàng
- Chi tiết sân + dịch vụ kèm theo
- Thuế VAT (nếu có)
- Tổng tiền, số tiền đã giảm, số tiền thực trả
- Phương thức thanh toán, thời gian thanh toán

**Tính năng:**
- Xuất PDF hóa đơn.
- Gửi email tự động sau khi thanh toán.
- Khách có thể xem lại hóa đơn trong lịch sử.

---

### FE-16: Quản lý kho dụng cụ (Equipment Inventory)

| Mục               | Chi tiết                                         |
| ----------------- | ------------------------------------------------ |
| **Mô tả**         | Theo dõi số lượng và tình trạng dụng cụ cho thuê |
| **Actor**         | Admin, Staff                                     |
| **Precondition**  | Đăng nhập với quyền Admin hoặc Staff             |
| **Postcondition** | Tồn kho được cập nhật chính xác                  |

**Chức năng:**
- Xem số lượng tồn kho từng loại dụng cụ.
- Cập nhật tình trạng: `Tốt / Hỏng / Đã thanh lý`.
- Cảnh báo khi số lượng dưới ngưỡng tối thiểu.
- Tự động giảm tồn kho khi khách đặt dịch vụ thuê dụng cụ.
- Hoàn trả tồn kho khi booking bị hủy.

**Quy tắc nghiệp vụ:**
- Không cho phép đặt dịch vụ khi dụng cụ hết hàng.
- Staff có thể ghi chú lý do hỏng hóc.

---

### FE-17: Lịch bảo trì sân (Maintenance Schedule)

| Mục               | Chi tiết                                                                        |
| ----------------- | ------------------------------------------------------------------------------- |
| **Mô tả**         | Lên lịch bảo trì sân có kế hoạch, tự động block booking trong thời gian bảo trì |
| **Actor**         | Admin                                                                           |
| **Precondition**  | Đăng nhập với quyền Admin                                                       |
| **Postcondition** | Sân bị block, khách không thể đặt trong thời gian bảo trì                       |

**Luồng chính:**
1. Admin tạo lịch bảo trì: chọn sân, ngày bắt đầu/kết thúc, lý do.
2. Hệ thống kiểm tra có booking nào trong thời gian này không.
3. Nếu có booking → gửi thông báo hủy và hoàn tiền 100%.
4. Sân tự động chuyển trạng thái `Maintenance`.
5. Kết thúc bảo trì → sân tự động `Available`.

**Thông tin lịch bảo trì:**
- Loại bảo trì: Định kỳ / Đột xuất / Nâng cấp
- Nhân viên phụ trách
- Ghi chú / Kết quả bảo trì

---

### FE-18: Quản lý ca làm việc nhân viên (Staff Shift)

| Mục               | Chi tiết                                        |
| ----------------- | ----------------------------------------------- |
| **Mô tả**         | Phân ca và theo dõi lịch làm việc của nhân viên |
| **Actor**         | Admin                                           |
| **Precondition**  | Đăng nhập với quyền Admin                       |
| **Postcondition** | Lịch ca được lưu, nhân viên nhận thông báo      |

**Ca làm việc:**
| Ca       | Giờ           |
| -------- | ------------- |
| Ca sáng  | 06:00 – 14:00 |
| Ca chiều | 14:00 – 22:00 |
| Ca tối   | 18:00 – 23:00 |

**Chức năng:**
- Xếp ca theo tuần/tháng.
- Staff xem lịch ca của mình.
- Nhận thông báo khi có thay đổi ca.
- Thống kê giờ công theo tháng.

---

### FE-19: Tìm đối thủ / Đồng đội (Player Matching)

| Mục               | Chi tiết                                            |
| ----------------- | --------------------------------------------------- |
| **Mô tả**         | Khách đặt sân có thể đăng tin tìm người chơi cùng   |
| **Actor**         | Khách hàng                                          |
| **Precondition**  | Đã có booking được xác nhận                         |
| **Postcondition** | Đăng tin tìm người chơi, nhận đăng ký từ người khác |

**Thông tin đăng tin:**
- Loại sân, ngày giờ chơi.
- Trình độ yêu cầu: Mới bắt đầu / Trung bình / Nâng cao.
- Số người cần thêm.
- Ghi chú (nam/nữ, độ tuổi,...).

**Luồng:**
1. Khách tạo tin sau khi booking được xác nhận.
2. Người khác xem danh sách và đăng ký tham gia.
3. Chủ tin xác nhận / từ chối thành viên.
4. Khi đủ người → đóng tin tự động.

---

### FE-20: Quản lý khu vực sân và nhân viên (Manager Role)

| Mục               | Chi tiết                                                                                 |
| ----------------- | ---------------------------------------------------------------------------------------- |
| **Mô tả**         | Cho phép Quản lý điều hành các hoạt động kinh doanh và nhân sự tại một tổ hợp sân cụ thể |
| **Actor**         | Quản lý                                                                                  |
| **Precondition**  | Đăng nhập với quyền Quản lý, được gán cho một tổ hợp sân (Complex)                       |
| **Postcondition** | Thông tin nhân sự và vận hành khu vực được cập nhật                                      |

**Chức năng chính:**
1. **Quản lý nhân viên khu vực:**
   - Xem danh sách nhân viên thuộc tổ hợp sân mình quản lý.
   - Phân ca làm việc chi tiết cho nhân viên.
   - Theo dõi hiệu quả làm việc và thời gian check-in/check-out của nhân viên.
2. **Quản lý vận hành sân:**
   - Cập nhật tình trạng sân nhanh (Sửa chữa đột xuất, dọn dẹp,...).
   - Phê duyệt các yêu cầu đặt sân dài hạn hoặc sự kiện đặc biệt tại khu vực.
3. **Báo cáo và thống kê khu vực:**
   - Xem doanh thu chi tiết của tổ hợp sân phụ trách.
   - Tỷ lệ lấp đầy sân theo khung giờ tại khu vực.
   - Thống kê dịch vụ đi kèm (nước uống, dụng cụ) tại khu vực.
4. **Quản lý công việc (Task Management):**
   - **Giao việc thủ công (Manual Assignment):**
     - Quản lý tạo các đầu việc phát sinh (sửa chữa, vệ sinh đột xuất, xử lý khiếu nại).
     - Chỉ định nhân viên cụ thể thực hiện.
     - Kiểm tra và xác nhận hoàn thành (Review & Approval).
   - **Giao việc tự động (Automated Tasks - System):**
     - Hệ thống tự động tạo task vệ sinh khi một booking kết thúc.
     - Tự động giao task chuẩn bị dụng cụ/nước uống khi có đơn hàng dịch vụ.
     - Tự động nhắc lịch bảo trì định kỳ dựa trên cấu hình hệ thống.

---

## 5. Thiết kế cơ sở dữ liệu

### 5.1 Danh sách bảng chính

| STT | Tên bảng             | Mô tả                              |
| --- | -------------------- | ---------------------------------- |
| 1   | Users                | Thông tin người dùng               |
| 2   | Roles                | Vai trò trong hệ thống             |
| 3   | UserRoles            | Quan hệ User - Role                |
| 4   | CourtTypes           | Loại sân (bóng đá, pickleball,...) |
| 5   | Courts               | Thông tin sân                      |
| 6   | CourtImages          | Hình ảnh sân                       |
| 7   | TimeSlots            | Khung giờ hoạt động                |
| 8   | CourtPricing         | Giá thuê theo khung giờ / ngày     |
| 9   | Bookings             | Thông tin đặt sân                  |
| 10  | BookingDetails       | Chi tiết đặt sân                   |
| 11  | RecurringBookings    | Đặt sân định kỳ                    |
| 12  | Waitlists            | Danh sách chờ                      |
| 13  | Payments             | Thanh toán                         |
| 14  | Invoices             | Hóa đơn                            |
| 15  | Services             | Dịch vụ bổ sung                    |
| 16  | BookingServices      | Dịch vụ đi kèm booking             |
| 17  | EquipmentInventory   | Kho dụng cụ                        |
| 18  | Reviews              | Đánh giá                           |
| 19  | Promotions           | Khuyến mãi                         |
| 20  | Notifications        | Thông báo                          |
| 21  | MembershipTiers      | Hạng thành viên                    |
| 22  | MaintenanceSchedules | Lịch bảo trì sân                   |
| 23  | StaffShifts          | Ca làm việc nhân viên              |
| 24  | PlayerRequests       | Tin tìm đối thủ / đồng đội         |
| 25  | Complexes            | Thông tin tổ hợp sân (Khu vực)     |
| 26  | AuditLogs            | Lịch sử thao tác hệ thống          |

### 5.2 Sơ đồ ERD (Entity Relationship)

```
Users ──< UserRoles >── Roles
  │
  ├──< Bookings >── Courts ──> CourtTypes
  │       │              │
  │       ├──< BookingDetails >── TimeSlots
  │       │
  │       ├──< BookingServices >── Services ──< EquipmentInventory
  │       │
  │       ├──< Payments ──< Invoices
  │       │
  │       └── RecurringBookings
  │
  ├──< Waitlists >── Courts
  │
  ├──< Reviews >── Courts
  │
  ├──< PlayerRequests >── Bookings
  │
  ├──< StaffShifts (Staff only)
  │
  └──> MembershipTiers

Courts ──< CourtPricing >── TimeSlots
Courts ──< MaintenanceSchedules
Promotions ──< Bookings
```

---

*Tài liệu được cập nhật ngày 14/05/2026 — Phiên bản 2.0*
