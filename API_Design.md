# 📐 Sports Court Management System — REST API Design Spec

Tài liệu này đặc tả chi tiết thiết kế hệ thống **REST API** cho **Hệ Thống Quản Lý Sân Thể Thao** (Sports Court Management System). Thiết kế tuân thủ kiến trúc **Clean Architecture** và các nguyên tắc **RESTful API** sử dụng **ASP.NET Core Web API (.NET 8)**.

---

## 1. Thiết Kế Tổng Quan (General Architecture & Principles)

### 1.1 Base URL
- **Local:** `http://localhost:5000/api`
- **Production:** `https://api.sportscourt.com/api`

### 1.2 Headers
- `Content-Type: application/json`
- `Authorization: Bearer <JWT_Token>` (Dành cho các Endpoint yêu cầu xác thực)

### 1.3 Cấu Trúc Phản Hồi Chuẩn (Standard Response Format)
Tất cả các API Endpoints đều trả về định dạng JSON bọc trong lớp `ApiResponse<T>`:

#### Phản hồi thành công (2xx)
```json
{
  "success": true,
  "message": "Action completed successfully",
  "data": { ... },
  "errors": null,
  "statusCode": 200
}
```

#### Phản hồi lỗi (4xx/5xx)
```json
{
  "success": false,
  "message": "Validation failed / Unauthorized access / Resource not found",
  "data": null,
  "errors": [
    "Detailed error description 1",
    "Detailed error description 2"
  ],
  "statusCode": 400
}
```

### 1.4 Phân Trang & Tìm Kiếm (Standard Pagination, Filtering & Sorting)
Các endpoint dạng danh sách (GET) hỗ trợ các tham số Query sau:
- `pageNumber`: Số trang hiện tại (mặc định: `1`)
- `pageSize`: Số lượng item trên một trang (mặc định: `10`)
- `searchTerm`: Từ khóa tìm kiếm (mặc định: `null`)
- `sortBy`: Tên thuộc tính sắp xếp (mặc định: `CreatedAt`)
- `sortDescending`: `true` nếu sắp xếp giảm dần, `false` nếu tăng dần (mặc định: `false`)

**Cấu trúc dữ liệu trả về của Phân trang (`PagedResult<T>`):**
```json
{
  "items": [ ... ],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

## 2. Đặc Tả Chi Tiết API Endpoints (Detailed Endpoints)

```
Danh mục module:
├── 2.1 Module Xác Thực & Người Dùng (Auth & Users)
├── 2.2 Module Tổ Hợp & Sân Thể Thao (Complexes & Courts)
├── 2.3 Module Đặt Sân (Bookings & Recurring Bookings)
├── 2.4 Module Thanh Toán & Hóa Đơn (Payments & Invoices)
├── 2.5 Module Hàng Đợi Chờ Sân (Waitlist)
├── 2.6 Module Dịch Vụ & Kho Dụng Cụ (Services & Equipment Inventory)
├── 2.7 Module Đánh Giá & Phản Hồi (Reviews & Feedbacks)
├── 2.8 Module Khuyến Mãi (Promotions)
├── 2.9 Module Lịch Bảo Trì & Ca Trực (Maintenance & Staff Shifts)
├── 2.10 Module Tìm Đối Thủ / Đồng Đội (Player Matching)
└── 2.11 Module Báo Cáo & Thống Kê (Dashboard & Reports)
```

---

### 2.1 Module Xác Thực & Người Dùng (Auth & Users)

#### 1. Đăng ký khách hàng mới (Register)
* **Endpoint:** `POST /api/auth/register`
* **Xác thực:** ❌ Không yêu cầu
* **Mô tả:** Cho phép khách hàng đăng ký tài khoản mới bằng số điện thoại/email.
* **Request Body:**
  ```json
  {
    "fullName": "Nguyễn Văn A",
    "email": "nguyenvana@gmail.com",
    "phone": "0987654321",
    "password": "Password123@!",
    "confirmPassword": "Password123@!"
  }
  ```
* **Response (Status 201 Created):**
  ```json
  {
    "success": true,
    "message": "User registered successfully.",
    "data": {
      "userId": 101,
      "fullName": "Nguyễn Văn A",
      "email": "nguyenvana@gmail.com",
      "phone": "0987654321",
      "membershipTier": "Bronze"
    },
    "errors": null,
    "statusCode": 201
  }
  ```

#### 2. Đăng nhập (Login)
* **Endpoint:** `POST /api/auth/login`
* **Xác thực:** ❌ Không yêu cầu
* **Mô tả:** Đăng nhập hệ thống bằng email và mật khẩu.
* **Request Body:**
  ```json
  {
    "email": "nguyenvana@gmail.com",
    "password": "Password123@!"
  }
  ```
* **Response (Status 200 OK):**
  ```json
  {
    "success": true,
    "message": "Login successful.",
    "data": {
      "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "refreshToken": "7c9f8d1e-ba56-4d22-921a-e89ff52cb3fa",
      "user": {
        "userId": 101,
        "fullName": "Nguyễn Văn A",
        "email": "nguyenvana@gmail.com",
        "role": "Customer",
        "membershipTier": "Bronze",
        "complexId": null
      }
    },
    "errors": null,
    "statusCode": 200
  }
  ```

#### 3. Làm mới Token (Refresh Token)
* **Endpoint:** `POST /api/auth/refresh-token`
* **Xác thực:** ❌ Không yêu cầu
* **Request Body:**
  ```json
  {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "7c9f8d1e-ba56-4d22-921a-e89ff52cb3fa"
  }
  ```
* **Response (Status 200 OK):** Trả về bộ AccessToken và RefreshToken mới.

#### 4. Xem thông tin cá nhân (Get Profile)
* **Endpoint:** `GET /api/users/profile`
* **Xác thực:** ✅ Đã đăng nhập
* **Response (Status 200 OK):** Trả về thông tin cá nhân kèm theo lịch sử & hạng thành viên.

#### 5. Cập nhật thông tin cá nhân (Update Profile)
* **Endpoint:** `PUT /api/users/profile`
* **Xác thực:** ✅ Đã đăng nhập
* **Request Body:**
  ```json
  {
    "fullName": "Nguyễn Văn B",
    "phone": "0987654322",
    "avatarUrl": "https://cloudinary.com/avatar/user101.png"
  }
  ```
* **Response (Status 200 OK):** Trả về thông tin User đã cập nhật thành công.

#### 6. Danh sách khách hàng (Get Customers - Phục vụ Admin/Staff)
* **Endpoint:** `GET /api/users/customers`
* **Xác thực:** ✅ Có role `Admin` hoặc `Staff`
* **Query Params:** Hỗ trợ pagination (`pageNumber`, `pageSize`), tìm kiếm (`searchTerm` theo Tên, SĐT, Email).
* **Response (Status 200 OK):** Danh sách khách hàng phân trang (`PagedResult<CustomerDTO>`).

#### 7. Cập nhật hạng thành viên (Update Membership Tier - Phục vụ Admin)
* **Endpoint:** `PUT /api/users/{userId}/membership`
* **Xác thực:** ✅ Có role `Admin`
* **Request Body:**
  ```json
  {
    "membershipTierId": 3 // VD: Gold
  }
  ```
* **Response (Status 200 OK):** Xác nhận cập nhật thành công.

---

### 2.2 Module Tổ Hợp & Sân Thể Thao (Complexes & Courts)

#### 1. Lấy danh sách tổ hợp sân (Get Complexes)
* **Endpoint:** `GET /api/complexes`
* **Xác thực:** ❌ Không yêu cầu
* **Query Params:** `pageNumber`, `pageSize`, `searchTerm` (theo Tên, Địa chỉ)
* **Response (Status 200 OK):**
  ```json
  {
    "success": true,
    "message": "Complexes retrieved successfully.",
    "data": {
      "items": [
        {
          "complexId": 1,
          "complexName": "Tổ hợp sân vận động Cầu Giấy",
          "address": "Dịch Vọng, Cầu Giấy, Hà Nội",
          "managerName": "Trần Văn Quản Lý",
          "totalCourts": 12
        }
      ],
      "totalCount": 1,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 1,
      "hasNextPage": false,
      "hasPreviousPage": false
    },
    "statusCode": 200
  }
  ```

#### 2. Thêm tổ hợp sân mới (Create Complex)
* **Endpoint:** `POST /api/complexes`
* **Xác thực:** ✅ Có role `Admin`
* **Request Body:**
  ```json
  {
    "complexName": "Tổ hợp thể thao Thanh Xuân",
    "address": "Nguyễn Trãi, Thanh Xuân, Hà Nội",
    "managerId": 5
  }
  ```
* **Response (Status 201 Created)**

#### 3. Lấy danh sách sân thể thao có bộ lọc (Get Courts)
* **Endpoint:** `GET /api/courts`
* **Xác thực:** ❌ Không yêu cầu
* **Query Params:**
  - `complexId`: Filter theo tổ hợp sân (INT)
  - `courtTypeId`: Filter theo loại sân (1: Cầu lông, 2: Bóng đá, 3: Pickleball,...)
  - `status`: `Available`, `Booked`, `InUse`, `Maintenance`
  - `minPrice` / `maxPrice`: Bộ lọc giá thuê
  - `date` / `timeSlotId`: Filter sân trống vào thời gian cụ thể
* **Response (Status 200 OK):** Danh sách sân đã lọc và phân trang.

#### 4. Xem chi tiết thông tin sân (Get Court Detail)
* **Endpoint:** `GET /api/courts/{id}`
* **Xác thực:** ❌ Không yêu cầu
* **Response (Status 200 OK):** Trả về đầy đủ chi tiết sân, hình ảnh, loại sân, và mô tả chi tiết.

#### 5. Thêm sân mới (Create Court)
* **Endpoint:** `POST /api/courts`
* **Xác thực:** ✅ Có role `Admin` hoặc `Manager`
* **Request Body:**
  ```json
  {
    "complexId": 1,
    "courtName": "Sân Pickleball P3",
    "courtTypeId": 3,
    "description": "Sân pickleball chuẩn quốc tế, đèn chiếu sáng ban đêm cực tốt.",
    "location": "Khu A - Sân số 3",
    "openTime": "06:00:00",
    "closeTime": "23:00:00",
    "imageUrl": "https://res.cloudinary.com/court3.png"
  }
  ```
* **Response (Status 201 Created)**

#### 6. Kiểm tra lịch trống của sân theo ngày (Get Court Availability)
* **Endpoint:** `GET /api/courts/{id}/availability`
* **Xác thực:** ❌ Không yêu cầu
* **Query Params:**
  - `date`: `YYYY-MM-DD` (Yêu cầu nhập)
* **Mô tả:** Lấy danh sách khung giờ hoạt động của sân trong ngày kèm thông tin giá tiền và trạng thái chi tiết của từng khung giờ.
* **Response (Status 200 OK):**
  ```json
  {
    "success": true,
    "message": "Availability retrieved successfully.",
    "data": {
      "courtId": 5,
      "courtName": "Sân Pickleball P3",
      "date": "2026-06-01",
      "slots": [
        {
          "timeSlotId": 1,
          "startTime": "06:00",
          "endTime": "07:30",
          "price": 120000.0,
          "status": "Available" // Available | Booked | InUse | Maintenance
        },
        {
          "timeSlotId": 2,
          "startTime": "07:30",
          "endTime": "09:00",
          "price": 120000.0,
          "status": "Booked"
        }
      ]
    },
    "statusCode": 200
  }
  ```

---

### 2.3 Module Đặt Sân (Bookings & Recurring Bookings)

#### 1. Đặt sân lẻ thông thường (Create Booking)
* **Endpoint:** `POST /api/bookings`
* **Xác thực:** ✅ Đã đăng nhập
* **Mô tả:** Khách đặt sân đơn lẻ, có thể kèm theo dịch vụ phụ trợ (thuê dụng cụ, nước uống).
* **Request Body:**
  ```json
  {
    "courtId": 5,
    "bookingDate": "2026-06-01",
    "timeSlotIds": [1, 3], // Chọn 1 hoặc nhiều slot trống liên tiếp hoặc rời rạc
    "promotionCode": "DISCOUNT10", // Nullable
    "note": "Chuẩn bị thêm 2 vợt thuê ngoài sân",
    "services": [
      {
        "serviceId": 1, // Thuê vợt Pickleball
        "quantity": 2
      },
      {
        "serviceId": 4, // Nước Pocari
        "quantity": 3
      }
    ]
  }
  ```
* **Response (Status 201 Created):**
  ```json
  {
    "success": true,
    "message": "Booking created successfully. Please proceed to payment.",
    "data": {
      "bookingId": 10024,
      "courtName": "Sân Pickleball P3",
      "bookingDate": "2026-06-01",
      "slots": [
        { "startTime": "06:00", "endTime": "07:30" },
        { "startTime": "09:00", "endTime": "10:30" }
      ],
      "subTotalAmount": 290000.0,
      "discountAmount": 29000.0,
      "servicesAmount": 80000.0,
      "totalAmount": 341000.0,
      "status": "Pending", // Trạng thái ban đầu chờ thanh toán
      "createdAt": "2026-05-31T21:00:00Z"
    },
    "statusCode": 201
  }
  ```

#### 2. Đặt sân định kỳ dài hạn (Create Recurring Booking)
* **Endpoint:** `POST /api/bookings/recurring`
* **Xác thực:** ✅ Đã đăng nhập
* **Mô tả:** Đặt sân lặp lại theo tuần cho các ngày thứ nhất định, sinh tự động danh sách booking.
* **Request Body:**
  ```json
  {
    "courtId": 5,
    "startDate": "2026-06-01",
    "endDate": "2026-08-01",
    "daysOfWeek": [1, 3, 5], // Thứ 2, Thứ 4, Thứ 6 (0: Chủ Nhật, 1: Thứ 2,...)
    "timeSlotIds": [3], // Khung giờ đặt cố định
    "paymentOption": "PayEntire", // PayEntire: Thanh toán tất cả | PayPerSession: Thanh toán mỗi buổi trước giờ chơi
    "note": "Đặt cố định cho câu lạc bộ"
  }
  ```
* **Response (Status 201 Created):** Trả về danh sách các booking được tự động tạo kèm cảnh báo nếu có ngày bị trùng lịch.

#### 3. Xem danh sách đặt sân của tôi (Get My Bookings)
* **Endpoint:** `GET /api/bookings/my`
* **Xác thực:** ✅ Chỉ `Customer`
* **Query Params:** `status`, `pageNumber`, `pageSize`
* **Response (Status 200 OK):** Danh sách booking phân trang của người dùng đang đăng nhập.

#### 4. Xem danh sách đặt sân hệ thống (Get All Bookings - Phục vụ Quản trị)
* **Endpoint:** `GET /api/bookings`
* **Xác thực:** ✅ Có role `Admin` hoặc `Manager` hoặc `Staff`
* **Query Params:** `complexId`, `courtId`, `bookingDate`, `status`, `pageNumber`, `pageSize`
* **Response (Status 200 OK):** Danh sách toàn bộ booking phù hợp bộ lọc phân trang.

#### 5. Chi tiết một Booking (Get Booking Detail)
* **Endpoint:** `GET /api/bookings/{id}`
* **Xác thực:** ✅ Đăng nhập (Chỉ xem được booking của chính mình trừ phi là Admin/Staff/Manager)
* **Response (Status 200 OK):** Chi tiết đầy đủ của Booking kèm dịch vụ và trạng thái hóa đơn.

#### 6. Hủy đặt sân (Cancel Booking)
* **Endpoint:** `PUT /api/bookings/{id}/cancel`
* **Xác thực:** ✅ Khách hàng chủ booking hoặc Admin/Staff
* **Request Body:**
  ```json
  {
    "reason": "Tôi bận việc đột xuất không tham gia chơi được"
  }
  ```
* **Response (Status 200 OK):**
  ```json
  {
    "success": true,
    "message": "Booking cancelled successfully. Refund processed: 100%.",
    "data": {
      "bookingId": 10024,
      "refundAmount": 341000.0, // Hoàn 100% do hủy trước 24h
      "status": "Cancelled"
    },
    "statusCode": 200
  }
  ```

#### 7. Cập nhật trạng thái Booking (Update Booking Status)
* **Endpoint:** `PUT /api/bookings/{id}/status`
* **Xác thực:** ✅ Có role `Admin` hoặc `Manager` hoặc `Staff`
* **Request Body:**
  ```json
  {
    "status": "CheckIn" // CheckIn | InUse | Completed | Confirmed
  }
  ```
* **Response (Status 200 OK)**

---

### 2.4 Module Thanh Toán & Hóa Đơn (Payments & Invoices)

#### 1. Tạo link thanh toán trực tuyến (Create Payment Link)
* **Endpoint:** `POST /api/payments/create-link`
* **Xác thực:** ✅ Đã đăng nhập
* **Mô tả:** Sinh link thanh toán chuyển hướng sang cổng VNPay hoặc MoMo.
* **Request Body:**
  ```json
  {
    "bookingId": 10024,
    "paymentMethod": "VNPay" // VNPay | MoMo
  }
  ```
* **Response (Status 200 OK):**
  ```json
  {
    "success": true,
    "message": "Payment link generated.",
    "data": {
      "bookingId": 10024,
      "paymentMethod": "VNPay",
      "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Amount=34100000..."
    },
    "statusCode": 200
  }
  ```

#### 2. Callback nhận kết quả thanh toán VNPay (VNPay Callback)
* **Endpoint:** `GET /api/payments/vnpay/callback`
* **Xác thực:** ❌ Không yêu cầu (Public - được gọi bởi VNPay Gateway hoặc Frontend nhận chuyển hướng)
* **Query Params:** Các trường dữ liệu trả về từ VNPay IPN (`vnp_ResponseCode`, `vnp_TxnRef`, `vnp_SecureHash`,...)
* **Mô tả:** Hệ thống đối soát, cập nhật trạng thái bảng `Payments` thành `Success` và cập nhật `Bookings` thành `Paid` (hoặc `Confirmed`), đồng thời tự động gọi SignalR cập nhật trạng thái sân thời gian thực và sinh Hóa đơn.
* **Response (Status 200 OK):** Trả về cấu trúc JSON tương thích với yêu cầu phản hồi của VNPay (VD: `{"RspCode":"00","Message":"Confirm Success"}`).

#### 3. Xem chi tiết hóa đơn (Get Invoice Details)
* **Endpoint:** `GET /api/invoices/{bookingId}`
* **Xác thực:** ✅ Chỉ xem được hóa đơn của mình, hoặc Admin/Staff
* **Response (Status 200 OK):** Trả về đầy đủ thông tin hóa đơn định dạng chi tiết (`InvoiceId`, `VAT`, `TotalAmount`, `PaidAt`, `PaymentMethod`, list services,...).

#### 4. Xuất hóa đơn PDF (Export PDF Invoice)
* **Endpoint:** `GET /api/invoices/{bookingId}/pdf`
* **Xác thực:** ✅ Chỉ xem được hóa đơn của mình, hoặc Admin/Staff
* **Response (Status 200 OK):** Trả về file Stream nhị phân dạng PDF (`application/pdf`) để người dùng tải trực tiếp về máy.

---

### 2.5 Module Hàng Đợi Chờ Sân (Waitlist)

#### 1. Đăng ký vào danh sách chờ khi sân đầy (Join Waitlist)
* **Endpoint:** `POST /api/waitlists`
* **Xác thực:** ✅ Chỉ `Customer`
* **Request Body:**
  ```json
  {
    "courtId": 5,
    "bookingDate": "2026-06-01",
    "timeSlotId": 2 // Slot này đang có trạng thái "Booked"
  }
  ```
* **Response (Status 201 Created):**
  ```json
  {
    "success": true,
    "message": "Successfully joined the waitlist. You are at position #2.",
    "data": {
      "waitlistId": 45,
      "courtName": "Sân Pickleball P3",
      "bookingDate": "2026-06-01",
      "queuePosition": 2
    },
    "statusCode": 201
  }
  ```

#### 2. Hủy/Rời khỏi danh sách chờ (Leave Waitlist)
* **Endpoint:** `DELETE /api/waitlists/{id}`
* **Xác thực:** ✅ Chủ đăng ký hàng chờ
* **Response (Status 200 OK):** Đã xóa đăng ký hàng chờ thành công.

#### 3. Xem danh sách chờ của một khung giờ sân (Get Waitlist - Admin/Staff)
* **Endpoint:** `GET /api/waitlists/court/{courtId}`
* **Xác thực:** ✅ Có role `Admin` hoặc `Manager` hoặc `Staff`
* **Query Params:** `date=YYYY-MM-DD`, `timeSlotId=INT`
* **Response (Status 200 OK):** Danh sách hàng chờ theo thứ tự FIFO kèm thông tin liên lạc của khách.

---

### 2.6 Module Dịch Vụ & Kho Dụng Cụ (Services & Equipment Inventory)

#### 1. Lấy danh sách dịch vụ bổ sung (Get Services)
* **Endpoint:** `GET /api/services`
* **Xác thực:** ❌ Không yêu cầu
* **Query Params:** `serviceType` (VD: `EquipmentRent` | `Drink` | `Coaching` | `Event`)
* **Response (Status 200 OK):** Danh sách các dịch vụ hiện đang cung cấp kèm giá đơn vị.

#### 2. Tạo dịch vụ mới (Create Service)
* **Endpoint:** `POST /api/services`
* **Xác thực:** ✅ Có role `Admin` hoặc `Manager`
* **Request Body:**
  ```json
  {
    "serviceName": "Huấn luyện viên Pickleball - Khóa cơ bản",
    "serviceType": "Coaching",
    "price": 300000.0,
    "unit": "Giờ",
    "description": "HLV có chứng chỉ quốc tế kèm 1-1."
  }
  ```
* **Response (Status 201 Created)**

#### 3. Cập nhật dịch vụ (Update Service)
* **Endpoint:** `PUT /api/services/{id}`
* **Xác thực:** ✅ Có role `Admin` hoặc `Manager`

#### 4. Xem tồn kho dụng cụ (Get Equipment Inventory - Admin/Staff)
* **Endpoint:** `GET /api/equipment`
* **Xác thực:** ✅ Có role `Admin`, `Manager` hoặc `Staff`
* **Response (Status 200 OK):** Danh sách dụng cụ có sẵn trong kho, số lượng đang cho thuê và trạng thái.

#### 5. Cập nhật tồn kho dụng cụ (Update Equipment Inventory)
* **Endpoint:** `PUT /api/equipment/{id}`
* **Xác thực:** ✅ Có role `Admin`, `Manager` hoặc `Staff`
* **Request Body:**
  ```json
  {
    "totalQuantity": 50,
    "availableQuantity": 35,
    "condition": "Tốt", // Tốt | Hỏng | Đã thanh lý
    "note": "Bổ sung thêm 10 vợt Pickleball mới nhập khẩu"
  }
  ```
* **Response (Status 200 OK)**

---

### 2.7 Module Đánh Giá & Phản Hồi (Reviews & Feedbacks)

#### 1. Viết đánh giá sau khi hoàn thành chơi (Create Review)
* **Endpoint:** `POST /api/reviews`
* **Xác thực:** ✅ Chỉ `Customer` đã hoàn thành booking tương ứng
* **Request Body:**
  ```json
  {
    "bookingId": 10020,
    "courtId": 5,
    "rating": 5, // 1 đến 5 sao
    "comment": "Sân rất mới, lưới căng đẹp, nhân viên hỗ trợ nhiệt tình.",
    "imageUrl": "https://res.cloudinary.com/review12.png"
  }
  ```
* **Response (Status 201 Created):** Trả về thông tin review được lưu thành công.

#### 2. Lấy danh sách đánh giá của sân (Get Court Reviews)
* **Endpoint:** `GET /api/courts/{courtId}/reviews`
* **Xác thực:** ❌ Không yêu cầu
* **Query Params:** `pageNumber`, `pageSize`
* **Response (Status 200 OK):** Danh sách các đánh giá của sân kèm điểm rating trung bình.

#### 3. Xóa đánh giá (Delete Review)
* **Endpoint:** `DELETE /api/reviews/{id}`
* **Xác thực:** ✅ Chủ review hoặc Admin

---

### 2.8 Module Khuyến Mãi (Promotions)

#### 1. Tạo chương trình khuyến mãi mới (Create Promotion)
* **Endpoint:** `POST /api/promotions`
* **Xác thực:** ✅ Có role `Admin`
* **Request Body:**
  ```json
  {
    "promoCode": "HELLOJUNE",
    "discountType": "Percentage", // Percentage | FixedAmount
    "discountValue": 15.0, // 15%
    "maxDiscountAmount": 50000.0, // Tối đa giảm 50k
    "minOrderAmount": 200000.0, // Đơn hàng tối thiểu 200k
    "startDate": "2026-06-01T00:00:00Z",
    "endDate": "2026-06-30T23:59:59Z",
    "usageLimit": 100
  }
  ```
* **Response (Status 201 Created)**

#### 2. Lấy danh sách mã khuyến mãi đang kích hoạt (Get Active Promotions)
* **Endpoint:** `GET /api/promotions`
* **Xác thực:** ❌ Không yêu cầu
* **Response (Status 200 OK):** Trả về danh sách mã giảm giá phù hợp với thời gian hiện tại.

#### 3. Kiểm tra tính hợp lệ của mã giảm giá (Validate Promo Code)
* **Endpoint:** `POST /api/promotions/validate`
* **Xác thực:** ✅ Đã đăng nhập
* **Request Body:**
  ```json
  {
    "promoCode": "HELLOJUNE",
    "bookingAmount": 250000.0
  }
  ```
* **Response (Status 200 OK):**
  ```json
  {
    "success": true,
    "message": "Promotion code is valid.",
    "data": {
      "promoCode": "HELLOJUNE",
      "isValid": true,
      "discountAmount": 37500.0 // 15% của 250k
    },
    "statusCode": 200
  }
  ```

---

### 2.9 Module Lịch Bảo Trì & Ca Trực (Maintenance & Staff Shifts)

#### 1. Lên lịch bảo trì sân (Create Maintenance Schedule)
* **Endpoint:** `POST /api/maintenance`
* **Xác thực:** ✅ Có role `Admin` hoặc `Manager`
* **Mô tả:** Block sân trong thời gian bảo trì. Hệ thống sẽ tự động gửi thông báo hủy lịch & hoàn tiền cho khách hàng có booking trùng thời gian này.
* **Request Body:**
  ```json
  {
    "courtId": 5,
    "startDate": "2026-06-15T08:00:00Z",
    "endDate": "2026-06-15T17:00:00Z",
    "maintenanceType": "Routine", // Routine (Định kỳ) | Emergency (Đột xuất) | Upgrade (Nâng cấp)
    "notes": "Bảo trì định kỳ mặt sân và thay lưới mới"
  }
  ```
* **Response (Status 201 Created):**
  ```json
  {
    "success": true,
    "message": "Maintenance scheduled. Overlapping bookings resolved.",
    "data": {
      "scheduleId": 12,
      "courtId": 5,
      "cancelledBookingsCount": 2 // Số lượng booking bị ảnh hưởng và được hoàn tiền
    },
    "statusCode": 201
  }
  ```

#### 2. Phân ca làm việc cho nhân viên (Assign Staff Shift)
* **Endpoint:** `POST /api/shifts`
* **Xác thực:** ✅ Có role `Admin` hoặc `Manager`
* **Request Body:**
  ```json
  {
    "staffId": 12,
    "complexId": 1,
    "shiftDate": "2026-06-01",
    "shiftType": "Morning" // Morning (06:00-14:00) | Afternoon (14:00-22:00) | Night (18:00-23:00)
  }
  ```
* **Response (Status 201 Created)**

#### 3. Xem danh sách ca làm việc của tôi (Get My Shifts)
* **Endpoint:** `GET /api/shifts/my`
* **Xác thực:** ✅ Chỉ `Staff`
* **Query Params:** `startDate`, `endDate`
* **Response (Status 200 OK):** Lịch làm việc cá nhân của nhân viên.

---

### 2.10 Module Tìm Đối Thủ / Đồng Đội (Player Matching)

#### 1. Đăng tin tìm người chơi ghép (Create Player Request)
* **Endpoint:** `POST /api/player-requests`
* **Xác thực:** ✅ Chỉ `Customer` có booking đã thanh toán/xác nhận
* **Request Body:**
  ```json
  {
    "bookingId": 10024,
    "skillLevel": "Intermediate", // Beginner | Intermediate | Advanced
    "playersNeeded": 2,
    "note": "Cần ghép thêm 2 người chơi đôi nam nữ Pickleball, trình độ trung bình."
  }
  ```
* **Response (Status 201 Created):**
  ```json
  {
    "success": true,
    "message": "Player request published.",
    "data": {
      "requestId": 98,
      "bookingId": 10024,
      "status": "Open",
      "playersJoined": 0
    },
    "statusCode": 201
  }
  ```

#### 2. Xem bảng tin tìm đối thủ ghép (Get Player Requests)
* **Endpoint:** `GET /api/player-requests`
* **Xác thực:** ❌ Không yêu cầu
* **Query Params:** `complexId`, `courtTypeId`, `skillLevel`, `pageNumber`, `pageSize`
* **Response (Status 200 OK):** Danh sách các tin tìm đối thủ đang hoạt động (`status: Open`).

#### 3. Đăng ký xin ghép cùng (Join Player Request)
* **Endpoint:** `POST /api/player-requests/{id}/join`
* **Xác thực:** ✅ Chỉ `Customer`
* **Response (Status 200 OK):**
  ```json
  {
    "success": true,
    "message": "Application submitted. Waiting for host approval.",
    "data": {
      "joinRequestId": 340,
      "status": "Pending"
    },
    "statusCode": 200
  }
  ```

#### 4. Host duyệt/từ chối đơn xin ghép (Approve/Reject Join Request)
* **Endpoint:** `PUT /api/player-requests/{id}/join-requests/{joinRequestId}`
* **Xác thực:** ✅ Chỉ host của tin ghép sân
* **Request Body:**
  ```json
  {
    "status": "Approved" // Approved | Rejected
  }
  ```
* **Response (Status 200 OK):** Cập nhật trạng thái và tự động đóng tin ghép nếu đã đủ số lượng người chơi.

---

### 2.11 Module Báo Cáo & Thống Kê (Dashboard & Reports)

#### 1. Tổng hợp số liệu Dashboard nhanh (Get Dashboard Summary)
* **Endpoint:** `GET /api/reports/dashboard`
* **Xác thực:** ✅ Có role `Admin` hoặc `Manager`
* **Query Params:** `complexId` (Nếu Manager chọn thì chỉ xem khu vực của mình)
* **Response (Status 200 OK):**
  ```json
  {
    "success": true,
    "message": "Dashboard summary retrieved.",
    "data": {
      "totalRevenue": 154800000.0,
      "totalBookings": 482,
      "occupancyRate": 68.5, // %
      "activeCustomersCount": 182,
      "topRatedCourts": [
        { "courtId": 5, "courtName": "Sân Pickleball P3", "rating": 4.9 }
      ]
    },
    "statusCode": 200
  }
  ```

#### 2. Báo cáo doanh thu theo biểu đồ thời gian (Get Revenue Report)
* **Endpoint:** `GET /api/reports/revenue`
* **Xác thực:** ✅ Có role `Admin` hoặc `Manager`
* **Query Params:**
  - `complexId`: Lọc theo tổ hợp
  - `period`: `Day` | `Week` | `Month` | `Year` (Mặc định: `Month`)
  - `startDate` / `endDate`
* **Response (Status 200 OK):** Doanh thu chi tiết chia theo mốc thời gian, loại sân, và loại dịch vụ để vẽ biểu đồ trực quan.

#### 3. Báo cáo tần suất sử dụng sân (Get Court Occupancy Report)
* **Endpoint:** `GET /api/reports/court-usage`
* **Xác thực:** ✅ Có role `Admin` hoặc `Manager`
* **Query Params:** `startDate`, `endDate`, `courtId`
* **Response (Status 200 OK):** Tỷ lệ lấp đầy sân (%) chi tiết theo từng khung giờ trong ngày giúp phân tích khung giờ vàng (Peak Hours).

---

## 3. Các Mã Trạng Thái HTTP & Lỗi Thường Gặp (Common HTTP Status Codes)

Hệ thống tuân thủ nghiêm ngặt các mã trạng thái HTTP tiêu chuẩn:

| Status Code | Ý nghĩa | Kịch bản áp dụng |
| :--- | :--- | :--- |
| **`200 OK`** | Thành công | Thực hiện thành công các phương thức GET, PUT, DELETE. |
| **`201 Created`** | Tạo mới thành công | Đăng ký tài khoản, đặt sân thành công (POST). |
| **`400 Bad Request`** | Lỗi dữ liệu gửi lên | Dữ liệu đầu vào không hợp lệ (sai số điện thoại, mật khẩu yếu,...). |
| **`401 Unauthorized`** | Chưa xác thực | Chưa gửi kèm JWT Token hoặc token đã hết hạn. |
| **`403 Forbidden`** | Không có quyền | Khách hàng cố gắng gọi API của Admin hoặc Manager khu vực khác. |
| **`404 Not Found`** | Không tìm thấy | Sân hoặc Booking ID không tồn tại trên hệ thống. |
| **`409 Conflict`** | Xung đột dữ liệu | Đặt sân trùng slot đã có người đặt thành công trước đó. |
| **`500 Internal Error`** | Lỗi máy chủ | Lỗi không mong muốn phát sinh từ hệ thống (được ghi vào Audit Log). |
