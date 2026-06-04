# Phân tích Thiết kế Cơ sở Dữ liệu (ERD Analysis)

Tài liệu này phân tích chi tiết sơ đồ Thực thể - Mối quan hệ (ERD) được cung cấp cho **Hệ thống Quản lý Sân Thể thao**.

---

## 1. Các Thực thể Chính và Mối Quan hệ (Entities & Relationships)

Sơ đồ ERD bao gồm 13 thực thể chính, được chia thành 4 nhóm nghiệp vụ chính:

### Nhóm 1: Quản lý Người dùng và Phân quyền (User & Auth)
*   **User**: Đại diện cho tất cả các tài khoản trong hệ thống (Khách hàng, Staff, Admin...).
*   **Roles**: Danh sách các vai trò (Ví dụ: Customer, Staff, Admin, Manager).
*   **UserRole**: Bảng trung gian giải quyết mối quan hệ Nhiều-Nhiều ($M:N$) giữa `User` và `Roles`.
    *   *Mối quan hệ:* Một `User` có thể có nhiều `UserRole` (1-n), một `Roles` có thể được gán cho nhiều `UserRole` (1-n). Điều này cho phép một tài khoản sở hữu nhiều vai trò cùng lúc.
*   **MembershipTiers**: Hạng thành viên của khách hàng (Đồng, Bạc, Vàng...).
    *   *Mối quan hệ:* Quan hệ 1-Nhiều ($1:N$) với `User` (`MembershipTiers` -> `User`). Một hạng thành viên áp dụng cho nhiều người dùng; mỗi người dùng chỉ thuộc về một hạng thành viên tại một thời điểm để hưởng ưu đãi.

### Nhóm 2: Quản lý Sân và Bảng giá (Courts & Pricing)
*   **Courts**: Thông tin các sân thể thao cụ thể trong hệ thống.
*   **CourtType**: Loại sân (Bóng đá, Cầu lông, Tennis, Pickleball...).
    *   *Mối quan hệ:* Quan hệ 1-Nhiều ($1:N$) với `Courts` (`CourtType` -> `Courts`). Mỗi sân thuộc về duy nhất một loại sân.
*   **TimeSlots**: Các khung giờ hoạt động (Ví dụ: 06:00-07:00, 17:00-18:00...).
*   **CourtPricing**: Bảng giá thuê sân theo khung giờ.
    *   *Mối quan hệ:* Bảng trung gian giải quyết mối quan hệ Nhiều-Nhiều ($M:N$) giữa `Courts` và `TimeSlots`. Mỗi sân tại mỗi khung giờ khác nhau sẽ có giá thuê khác nhau (`Courts` -> `CourtPricing` <- `TimeSlots`).

### Nhóm 3: Nghiệp vụ Đặt sân và Thanh toán (Booking & Payment)
*   **Bookings**: Thông tin giao dịch đặt sân.
    *   *Mối quan hệ:*
        *   `User` (1) - (N) `Bookings`: Một người dùng có thể thực hiện nhiều lượt đặt sân.
        *   `Courts` (1) - (N) `Bookings`: Một sân có thể được đặt nhiều lần (ở các thời điểm khác nhau).
        *   `TimeSlots` (1) - (N) `Bookings`: Một lượt đặt sân diễn ra trong một khung giờ cụ thể.
*   **Payments**: Thông tin thanh toán hóa đơn.
    *   *Mối quan hệ:* Quan hệ 1-Nhiều ($1:N$) từ `Bookings` sang `Payments`. Một booking có thể có nhiều lượt thanh toán (Ví dụ: đặt cọc trước 50% trực tuyến và trả 50% còn lại tại quầy, hoặc thanh toán lại khi giao dịch đầu bị lỗi).
*   **Promotions**: Mã giảm giá, chương trình ưu đãi.
    *   *Mối quan hệ:* Quan hệ 1-Nhiều ($1:N$) từ `Promotions` sang `Bookings`. Một mã giảm giá có thể áp dụng cho nhiều booking; mỗi booking áp dụng tối đa một mã giảm giá.

### Nhóm 4: Dịch vụ đi kèm và Tương tác (Services & Reviews)
*   **Services**: Các dịch vụ bổ sung như nước uống, thuê vợt, thuê bóng, giày.
*   **BookingService**: Bảng trung gian lưu thông tin các dịch vụ đi kèm với từng đơn đặt sân cụ thể.
    *   *Mối quan hệ:* Quan hệ Nhiều-Nhiều ($M:N$) giữa `Bookings` và `Services` thông qua `BookingService`. Một booking có thể gọi nhiều dịch vụ và một dịch vụ có thể xuất hiện trong nhiều booking khác nhau.
*   **Reviews**: Đánh giá và phản hồi của khách hàng.
    *   *Mối quan hệ:*
        *   `User` (1) - (N) `Reviews`: Một khách hàng có thể viết nhiều đánh giá.
        *   `Courts` (1) - (N) `Reviews`: Một sân có thể nhận được nhiều đánh giá từ các khách hàng khác nhau.
*   **Notifications**: Thông báo gửi cho người dùng.
    *   *Mối quan hệ:* Quan hệ 1-Nhiều ($1:N$) từ `User` sang `Notifications`. Một người dùng nhận được nhiều thông báo từ hệ thống.

---

## 2. Đánh giá ưu điểm của thiết kế ERD này

1.  **Thiết kế giá động linh hoạt (Flexibility in Pricing)**: Việc tách biệt bảng `CourtPricing` làm trung gian giữa `Courts` và `TimeSlots` là thiết kế rất chuẩn xác. Nó cho phép Admin cấu hình giá cực kỳ linh động (Ví dụ: Sân số 1 lúc 18h tối có giá cao hơn lúc 8h sáng, và sân Pickleball có giá khác sân Cầu lông).
2.  **Khả năng mở rộng vai trò người dùng (Multi-role support)**: Mối quan hệ qua bảng trung gian `UserRole` giúp hệ thống dễ dàng gán một tài khoản vừa làm **Khách hàng** vừa làm **Nhân viên/Huấn luyện viên** mà không cần tạo 2 tài khoản riêng biệt.
3.  **Tách biệt thanh toán và đặt sân (Decoupled Payment)**: Một `Booking` liên kết với nhiều `Payments` giúp hệ thống hỗ trợ tốt các nghiệp vụ thực tế như: đặt cọc trước, trả sau tại quầy, hoàn tiền một phần, hoặc thực hiện thanh toán lại nếu cổng thanh toán (VNPay/MoMo) gặp sự cố mà không làm mất thông tin Booking ban đầu.

---

## 3. Một số điểm lưu ý hoặc đề xuất cải tiến (Recommendations)

1.  **Thiếu tính liên kết ngày trong Đặt sân & Bảng giá**:
    *   *Vấn đề:* Bảng `CourtPricing` hiện tại mới liên kết `Courts` và `TimeSlots` nhưng chưa phân biệt được **Ngày trong tuần (Weekday)** và **Cuối tuần (Weekend) / Ngày lễ**. Thông thường giá sân cuối tuần sẽ cao hơn ngày thường.
    *   *Đề xuất:* Nên thêm thuộc tính `DayOfWeek` hoặc `IsHoliday` vào bảng `CourtPricing` để tối ưu hóa giá dynamic.
2.  **Mối quan hệ 1-Nhiều giữa Bookings và TimeSlots**:
    *   *Vấn đề:* Theo sơ đồ, `TimeSlots` (1) - (m) `Bookings`. Nghĩa là một Booking chỉ diễn ra trong đúng **1 TimeSlot** cố định (thường là 1 tiếng). Nếu khách muốn đặt liền lúc 2 tiếng (ví dụ từ 17:00 - 19:00 gồm 2 slot liên tiếp), hệ thống sẽ phải tạo ra 2 bản ghi `Bookings` riêng biệt, dẫn đến việc thanh toán và quản lý bị xé lẻ.
    *   *Đề xuất:* Chuyển sang mô hình: Một `Booking` có nhiều `BookingDetails`, và mỗi `BookingDetail` sẽ liên kết tới một `TimeSlot`. Như vậy khách hàng có thể đặt nhiều slot liền nhau hoặc đặt nhiều sân cùng lúc trong một lần thanh toán (một hóa đơn thanh toán duy nhất).
3.  **Lịch sử và Trạng thái bảo trì sân**:
    *   *Vấn đề:* Sơ đồ chưa thể hiện rõ cách hệ thống chặn đặt sân khi sân cần bảo trì (`Maintenance`).
    *   *Đề xuất:* Có thể thêm một bảng `MaintenanceSchedules` liên kết với `Courts` và `TimeSlots` để khóa các khung giờ bảo trì.
