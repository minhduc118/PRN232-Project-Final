# Tài liệu Use Case của Khách hàng (Customer Use Cases)

Tài liệu này mô tả chi tiết các Use Case liên quan đến vai trò **Khách hàng (Customer)** trong **Hệ thống Quản lý Sân Thể thao**.

---

## 1. Sơ đồ Use Case (Mermaid Diagram)

Dưới đây là sơ đồ Use Case của Khách hàng được mô tả bằng Mermaid. Bạn có thể xem biểu đồ trực quan trong các trình đọc Markdown hỗ trợ Mermaid (như GitHub, VS Code, v.v.).

```mermaid
usecaseDiagram
    actor Customer as "Khách hàng (Customer)"

    rect/tính năng tài khoản/
        Customer --> UC_Auth_Register
        Customer --> UC_Auth_Login
        Customer --> UC_Auth_Profile
    end

    rect/tính năng tìm kiếm và đặt sân/
        Customer --> UC_Search_Court
        Customer --> UC_View_Detail
        Customer --> UC_Book_Court
        Customer --> UC_Waitlist
        Customer --> UC_Player_Matching
    end

    rect/tính năng thanh toán và dịch vụ/
        Customer --> UC_Payment
        Customer --> UC_Service_Rent
        Customer --> UC_Promo
    end

    rect/tính năng quản lý lịch đặt/
        Customer --> UC_History
        Customer --> UC_Cancel_Reschedule
        Customer --> UC_Review
    end

    %% Các quan hệ phụ thuộc (include / extend)
    UC_Book_Court ..> UC_Auth_Login : <<include>>
    UC_Book_Court <.. UC_Promo : <<extend>>
    UC_Book_Court <.. UC_Service_Rent : <<extend>>
    UC_Book_Court ..> UC_Payment : <<include>>
    
    UC_Cancel_Reschedule ..> UC_Auth_Login : <<include>>
    UC_Waitlist ..> UC_Auth_Login : <<include>>
    UC_Review ..> UC_Auth_Login : <<include>>
    UC_Player_Matching ..> UC_Auth_Login : <<include>>

    %% Tên Use Cases
    UC_Auth_Register("Đăng ký tài khoản")
    UC_Auth_Login("Đăng nhập")
    UC_Auth_Profile("Quản lý thông tin cá nhân")
    
    UC_Search_Court("Tìm kiếm sân")
    UC_View_Detail("Xem chi tiết sân & đánh giá")
    
    UC_Book_Court("Đặt sân\n(Lẻ hoặc Định kỳ)")
    UC_Promo("Áp dụng mã giảm giá")
    UC_Service_Rent("Thuê dụng cụ & nước uống")
    UC_Payment("Thanh toán trực tuyến\n(VNPay, MoMo, CK)")
    
    UC_Waitlist("Đăng ký hàng chờ (Waitlist)")
    UC_Player_Matching("Tìm đối thủ / đồng đội")
    
    UC_History("Xem lịch sử đặt sân & hóa đơn")
    UC_Cancel_Reschedule("Hủy đặt sân / Đổi lịch")
    UC_Review("Viết đánh giá & phản hồi")
```

---

## 2. Đặc tả chi tiết các Use Case chính

### UC-01: Tìm kiếm và đặt sân (Book Court)
* **Tác nhân (Actor):** Khách hàng (Customer).
* **Mô tả:** Cho phép khách hàng tìm kiếm sân trống theo loại sân, ngày, giờ và tiến hành đặt sân.
* **Tiền điều kiện (Precondition):** Khách hàng đã đăng nhập tài khoản.
* **Luồng chính:**
  1. Khách hàng truy cập chức năng đặt sân từ trang chủ hoặc trang tìm kiếm.
  2. Chọn loại sân (bóng đá, pickleball, cầu lông, v.v.), ngày và khung giờ mong muốn.
  3. Hệ thống kiểm tra tình trạng sân theo thời gian thực và hiển thị danh sách sân trống.
  4. Khách hàng chọn sân và loại hình đặt (đặt lẻ hoặc đặt định kỳ theo tuần).
  5. Hệ thống hiển thị giao diện tùy chọn dịch vụ đi kèm (nước uống, thuê vợt/bóng) và mã giảm giá.
  6. Khách hàng điền thông tin bổ sung và nhấn xác nhận đặt sân.
  7. Hệ thống chuyển hướng khách hàng sang cổng thanh toán trực tuyến.
* **Luồng ngoại lệ:**
  - Không có sân trống trong khung giờ đã chọn: Hệ thống gợi ý khung giờ/ngày khác hoặc hiển thị tùy chọn **Vào danh sách chờ (Waitlist)**.

---

### UC-02: Đăng ký hàng chờ (Join Waitlist)
* **Tác nhân (Actor):** Khách hàng (Customer), Hệ thống.
* **Mô tả:** Khi một khung giờ sân đã bị đặt hết, khách hàng có thể đăng ký vào danh sách chờ. Khi có khách hủy sân, người đứng đầu hàng chờ sẽ nhận được thông báo để đặt sân.
* **Tiền điều kiện (Precondition):** Khách hàng đã đăng nhập, khung giờ sân mong muốn đã kín lịch.
* **Luồng chính:**
  1. Khách hàng chọn khung giờ đã hết sân và nhấn "Đăng ký hàng chờ".
  2. Hệ thống ghi nhận khách hàng vào danh sách chờ theo thứ tự FIFO (vào trước xếp trước).
  3. Khi có một khách hàng khác hủy đặt sân ở khung giờ đó:
     - Hệ thống tự động gửi thông báo (Email / In-app) cho người đầu tiên trong hàng chờ.
     - Khách hàng có **15 phút** để bấm xác nhận và thanh toán đặt sân.
  4. Nếu khách hàng xác nhận thành công, hệ thống tạo đặt sân và xóa khỏi hàng chờ.
* **Luồng ngoại lệ:**
  - Quá 15 phút khách hàng không xác nhận: Hệ thống tự động chuyển lượt sang người tiếp theo trong hàng chờ.

---

### UC-03: Đổi lịch / Hủy đặt sân (Cancel / Reschedule Booking)
* **Tác nhân (Actor):** Khách hàng (Customer).
* **Mô tả:** Cho phép khách hàng đổi khung giờ đặt hoặc hủy hoàn toàn booking đã thanh toán.
* **Tiền điều kiện (Precondition):** Khách hàng đã đăng nhập, booking đang ở trạng thái đã xác nhận/thành công.
* **Quy tắc nghiệp vụ hoàn tiền khi hủy:**
  - Hủy trước giờ chơi > 24 tiếng: Hoàn tiền **100%**.
  - Hủy trước giờ chơi từ 12 - 24 tiếng: Hoàn tiền **50%**.
  - Hủy dưới 12 tiếng: **Không hoàn tiền**.
* **Luồng chính:**
  1. Khách hàng vào mục **Lịch sử đặt sân**, chọn booking muốn hủy hoặc đổi lịch.
  2. **Trường hợp Hủy sân:**
     - Khách hàng nhấn "Hủy lịch".
     - Hệ thống tính toán số tiền được hoàn dựa trên quy tắc nghiệp vụ và hiển thị thông báo xác nhận.
     - Khách hàng đồng ý, hệ thống cập nhật trạng thái booking thành "Đã hủy" và tiến hành quy trình hoàn tiền tự động.
  3. **Trường hợp Đổi lịch:**
     - Khách hàng chọn "Đổi lịch" và chọn khung giờ mới trống.
     - Hệ thống kiểm tra điều kiện chênh lệch giá (nếu có) và yêu cầu thanh toán thêm hoặc hoàn phí thừa.
     - Xác nhận đổi lịch thành công.

---

### UC-04: Tìm đối thủ / Đồng đội (Player Matching)
* **Tác nhân (Actor):** Khách hàng (Customer).
* **Mô tả:** Cho phép khách hàng đã đặt sân đăng tin tuyển thêm thành viên hoặc tìm đối thủ giao hữu tại sân đó.
* **Tiền điều kiện (Precondition):** Khách hàng đã đăng nhập và có booking đã được xác nhận.
* **Luồng chính:**
  1. Khách hàng chọn một booking của mình và nhấn "Tìm người chơi cùng".
  2. Điền thông tin yêu cầu:
     - Số lượng người cần tìm.
     - Trình độ mong muốn (Mới chơi, Trung bình, Khá/Giỏi).
     - Ghi chú thêm (Độ tuổi, chia sẻ chi phí sân, v.v.).
  3. Nhấn "Đăng tin". Hệ thống hiển thị tin đăng trên bảng tin công cộng.
  4. Những người chơi khác truy cập bảng tin, nhấn "Đăng ký tham gia".
  5. Chủ sân nhận được thông báo duyệt yêu cầu tham gia (Chấp nhận / Từ chối).
  6. Khi đủ số người, hệ thống tự động đóng bài đăng.

---

### UC-05: Đánh giá và Phản hồi (Rate & Review)
* **Tác nhân (Actor):** Khách hàng (Customer).
* **Mô tả:** Khách hàng đánh giá chất lượng dịch vụ và cơ sở vật chất sau khi trải nghiệm xong.
* **Tiền điều kiện (Precondition):** Booking đã hoàn thành (quá khung giờ chơi).
* **Luồng chính:**
  1. Khách hàng nhận được thông báo mời đánh giá hoặc chủ động vào lịch sử đặt sân.
  2. Chọn booking đã hoàn thành và nhấn "Đánh giá".
  3. Khách hàng chọn số sao (1-5), viết nhận xét và tải lên hình ảnh thực tế (tùy chọn).
  4. Nhấn "Gửi đánh giá".
  5. Hệ thống hiển thị đánh giá công khai trên trang chi tiết của sân thể thao đó.
