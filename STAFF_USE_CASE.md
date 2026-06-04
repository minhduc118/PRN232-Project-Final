# Tài liệu Use Case của Nhân viên (Staff Use Cases)

Tài liệu này mô tả chi tiết các Use Case liên quan đến vai trò **Nhân viên (Staff)** trong **Hệ thống Quản lý Sân Thể thao**.

---

## 1. Sơ đồ Use Case (Mermaid Diagram)

```mermaid
usecaseDiagram
    actor Staff as "Nhân viên (Staff)"

    rect/quản lý đặt sân & dịch vụ tại quầy/
        Staff --> UC_Manage_Booking
        Staff --> UC_Walkin_Booking
        Staff --> UC_Support_Customer
        Staff --> UC_Inventory
    end

    %% Tên Use Cases cho Staff
    UC_Manage_Booking("Quản lý đặt sân\n(Xem/Duyệt/Hủy/Đổi)")
    UC_Walkin_Booking("Đặt sân trực tiếp tại quầy\n(Walk-in)")
    UC_Support_Customer("Xem & hỗ trợ khách hàng")
    UC_Inventory("Quản lý kho dụng cụ\n(Kiểm kho/Cập nhật hỏng)")
```

---

## 2. Đặc tả chi tiết các Use Case chính của Staff

### UC-STAFF-01: Đặt sân trực tiếp tại quầy (Walk-in Booking)
* **Tác nhân (Actor):** Nhân viên (Staff).
* **Mô tả:** Hỗ trợ khách hàng đến đặt sân trực tiếp tại quầy mà không qua ứng dụng.
* **Tiền điều kiện (Precondition):** Staff đã đăng nhập vào hệ thống quản trị.
* **Luồng chính:**
  1. Khách hàng đến quầy yêu cầu đặt sân trực tiếp.
  2. Staff kiểm tra danh sách sân trống trên màn hình sơ đồ sân trực quan (Grid View).
  3. Chọn sân, ngày và khung giờ trống theo yêu cầu của khách.
  4. Nhập thông tin số điện thoại khách hàng (Hệ thống tự tra cứu, nếu là khách mới thì Staff tạo nhanh thông tin cơ bản).
  5. Chọn thêm dịch vụ đi kèm nếu khách yêu cầu (nước uống, thuê vợt).
  6. Chọn hình thức thanh toán tại quầy (Tiền mặt, quét mã QR chuyển khoản).
  7. Xác nhận đặt sân, hệ thống tự động in hóa đơn tại quầy và cập nhật trạng thái sân thành `Booked`.

---

### UC-STAFF-02: Quản lý kho dụng cụ (Equipment Inventory)
* **Tác nhân (Actor):** Nhân viên (Staff).
* **Mô tả:** Kiểm tra và cập nhật số lượng dụng cụ (vợt, bóng, giày...) dùng để cho thuê hoặc bán.
* **Tiền điều kiện (Precondition):** Staff đăng nhập hệ thống.
* **Luồng chính:**
  1. Staff vào mục **Quản lý kho dụng cụ**.
  2. Hệ thống hiển thị danh sách các dụng cụ kèm số lượng tồn thực tế.
  3. Khi có dụng cụ bị hỏng hoặc mất mát trong ca trực, Staff cập nhật trạng thái (`Tốt / Hỏng / Đã thanh lý`) và số lượng tương ứng.
  4. Hệ thống ghi nhận lịch sử thay đổi kho kèm tên Staff thực hiện.
* **Quy tắc nghiệp vụ:**
  - Nếu số lượng dụng cụ còn lại dưới ngưỡng tối thiểu, hệ thống hiển thị cảnh báo màu đỏ để Staff/Admin biết và nhập thêm hàng.
