# Tài liệu Danh sách API Endpoints: Khách hàng, Đặt sân & Thanh toán

Tài liệu này liệt kê chi tiết các đề xuất API endpoint dành cho các luồng nghiệp vụ của **Khách hàng**, quy trình **Đặt sân (Booking)** và **Thanh toán (Payment)** dựa trên cấu trúc hiện tại của dự án.

---

## 1. Nhóm API Xác thực & Tài khoản (Authentication & Profile)

Áp dụng tiền tố: `/api/v1/auth` hoặc `/api/v1/users`

| Phương thức | Endpoint | Yêu cầu xác thực | Mô tả | Chi tiết Request/Response |
| :--- | :--- | :--- | :--- | :--- |
| **POST** | `/auth/register` | Không | Đăng ký tài khoản khách hàng mới. | **Body:** `{ fullName, email, phone, password }`<br>**Response:** `201 Created` |
| **POST** | `/auth/login` | Không | Đăng nhập tài khoản bằng Email & Mật khẩu. | **Body:** `{ email, password }`<br>**Response:** `{ accessToken, refreshToken, user: { userId, fullName, email, phone, role } }` |
| **GET** | `/auth/me` | Có (Bearer Token) | Lấy thông tin tài khoản hiện tại dựa trên Token. | **Response:** `{ userId, fullName, email, phone, role, membershipTierId }` |
| **PUT** | `/users/profile` | Có (Bearer Token) | Cập nhật thông tin cá nhân. | **Body:** `{ fullName, phone, avatarUrl }`<br>**Response:** `200 OK` |

---

## 2. Nhóm API Tra cứu & Tìm kiếm Sân (Courts & Search)

Áp dụng tiền tố: `/api/v1/courts`

| Phương thức | Endpoint | Yêu cầu xác thực | Mô tả | Chi tiết Request/Response |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/courts` | Không | Lấy danh sách sân đấu kèm bộ lọc (theo loại sân, ngày chơi...). | **Query:** `?courtTypeId=1`<br>**Response:** Danh sách các sân bao gồm `{ courtId, courtName, courtTypeId, description, status, openTime, closeTime, pricePerHour, imageUrl }` |
| **GET** | `/courts/{id}` | Không | Xem thông tin chi tiết của một sân cụ thể theo ID. | **Response:** `{ courtId, courtName, courtTypeId, status, description, imageUrl, openTime, closeTime }` |
| **GET** | `/courts/{id}/slots` | Không | Lấy danh sách khung giờ trống của sân cụ thể trong ngày. | **Query:** `?date=2026-06-04`<br>**Response:** Danh sách các slot kèm trạng thái `{ slotId, slotName, startTime, endTime, isBooked, price }` |

---

## 3. Nhóm API Đặt sân (Booking Flow)

Áp dụng tiền tố: `/api/v1/bookings`

| Phương thức | Endpoint | Yêu cầu xác thực | Mô tả | Chi tiết Request/Response |
| :--- | :--- | :--- | :--- | :--- |
| **POST** | `/bookings` | Có (Customer) | Khởi tạo đơn đặt sân mới (Một lần hoặc định kỳ). | **Body:** `{ courtId, slotId, bookingDate, note, promotionCode, bookingType: "Single"\|"Recurring", recurringDaysOfWeek: [], recurringEndDate }`<br>**Response:** `{ bookingId, bookingCode, subTotal, discountAmount, totalAmount, status: "Pending" }` |
| **GET** | `/bookings/my` | Có (Customer) | Lấy lịch sử đặt sân của chính khách hàng đang đăng nhập. | **Response:** Danh sách các booking của user kèm trạng thái (`Pending`, `Confirmed`, `Cancelled`). |
| **GET** | `/bookings/{id}` | Có (Customer/Staff) | Xem chi tiết thông tin của một đơn đặt sân theo ID. | **Response:** Chi tiết đặt sân, dịch vụ đi kèm và hóa đơn/thanh toán. |
| **PUT** | `/bookings/{id}/cancel` | Có (Customer/Staff) | Hủy đơn đặt sân. Tự động hoàn tiền theo quy tắc nghiệp vụ (>24h: 100%, 12-24h: 50%, <12h: 0%). | **Response:** `{ message: "Hủy đặt sân thành công", refundAmount: 150000 }` |

---

## 4. Nhóm API Thanh toán (Payment Flow)

Áp dụng tiền tố: `/api/v1/payments`

### Sơ đồ Luồng thanh toán trực tuyến:
```
[Client] --- (1) POST /payments/create-url ---> [Backend]
                                                      |
                                             (Tạo link thanh toán)
                                                      |
[Client] <--- (2) Trả về VNPay/MoMo URL <--------------+
   |
(Khách thực hiện thanh toán trên cổng VNPay/MoMo)
   |
[VNPay/MoMo] --- (3) Redirect/IPN Callback ---> [Backend]
                                                      |
                                          (Xác thực chữ ký & Update DB)
                                                      |
[Client] <--- (4) Lắng nghe /payments/update <---------+
```

### Các Endpoint chi tiết:

| Phương thức | Endpoint | Yêu cầu xác thực | Mô tả | Chi tiết Request/Response |
| :--- | :--- | :--- | :--- | :--- |
| **POST** | `/payments/create-url` | Có (Customer) | Tạo đường dẫn thanh toán qua VNPay hoặc MoMo. | **Body:** `{ bookingId, paymentMethod: "VNPay"\|"MoMo" }`<br>**Response:** `{ paymentUrl, transactionCode }` |
| **GET** | `/payments/vnpay-return` | Không | Endpoint nhận Redirect của VNPay sau khi khách thanh toán xong (Front-end nhận để hiển thị kết quả). | **Query:** Các tham số trả về từ VNPay (`vnp_TxnRef`, `vnp_ResponseCode`...).<br>**Response:** Chuyển hướng hoặc trả về trạng thái giao dịch cho UI. |
| **POST** | `/payments/vnpay-ipn` | Không | Endpoint nhận IPN (Instant Payment Notification) từ máy chủ VNPay để cập nhật trạng thái đơn hàng bất đồng bộ (Đảm bảo an toàn giao dịch). | **Body/Query:** Tham số giao dịch từ VNPay.<br>**Response:** `{ RspCode: "00", Message: "Confirm Success" }` |
| **POST** | `/payments/update` | Hệ thống / Staff | Cập nhật trực tiếp kết quả giao dịch thanh toán (hỗ trợ chế độ Mock hoặc cho Staff thanh toán tiền mặt tại quầy). | **Body:** `{ bookingId, status: "Success"\|"Failed", method: "Cash"\|"BankTransfer", transactionId }`<br>**Response:** Chi tiết thông tin Booking kèm trạng thái mới. |
