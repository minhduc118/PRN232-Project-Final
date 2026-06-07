# Tài liệu Use Case của Nhân viên (Staff Use Cases)

Tài liệu này mô tả chi tiết các Use Case liên quan đến vai trò **Nhân viên (Staff)** trong **Hệ thống Quản lý Sân Thể thao**.

---

## 1. Sơ đồ Use Case (Mermaid Diagram)

Dưới đây là sơ đồ Use Case của Nhân viên với đầy đủ các mối quan hệ phụ thuộc `<<include>>` và `<<extend>>`.

```mermaid
usecaseDiagram
    actor Staff as "Nhân viên (Staff)"

    rect/hệ thống & ca làm việc/
        Staff --> UC_Auth_Login
        Staff --> UC_View_Shifts
        Staff --> UC_Perform_Tasks
    end

    rect/quản lý đặt sân & dịch vụ tại quầy/
        Staff --> UC_Walkin_Booking
        Staff --> UC_Manage_Booking
    end

    rect/quản lý kho & hỗ trợ khách hàng/
        Staff --> UC_Inventory
        Staff --> UC_Support_Customer
    end

    %% Các quan hệ phụ thuộc (include / extend)
    UC_Walkin_Booking ..> UC_Auth_Login : <<include>>
    UC_Walkin_Booking ..> UC_InPerson_Payment : <<include>>
    UC_Walkin_Booking <.. UC_Service_Rent : <<extend>>
    
    UC_Manage_Booking ..> UC_Auth_Login : <<include>>
    UC_Manage_Booking <.. UC_Cancel_Booking : <<extend>>
    UC_Manage_Booking <.. UC_Reschedule_Booking : <<extend>>
    
    UC_Inventory ..> UC_Auth_Login : <<include>>
    UC_Inventory <.. UC_Update_Damaged : <<extend>>
    
    UC_Support_Customer ..> UC_Auth_Login : <<include>>
    UC_Support_Customer ..> UC_View_Customer_History : <<include>>
    
    UC_Perform_Tasks ..> UC_Auth_Login : <<include>>
    UC_View_Shifts ..> UC_Auth_Login : <<include>>

    %% Tên Use Cases
    UC_Auth_Login("Đăng nhập hệ thống")
    UC_View_Shifts("Xem lịch ca làm việc")
    UC_Perform_Tasks("Nhận & thực hiện nhiệm vụ\n(Chuẩn bị sân, dọn dẹp)")
    
    UC_Walkin_Booking("Đặt sân trực tiếp tại quầy\n(Walk-in)")
    UC_InPerson_Payment("Xử lý thanh toán trực tiếp\n(Tiền mặt, quét mã QR)")
    UC_Service_Rent("Bán nước uống & cho thuê dụng cụ")
    
    UC_Manage_Booking("Quản lý đặt sân")
    UC_Cancel_Booking("Hủy booking hộ khách")
    UC_Reschedule_Booking("Đổi lịch đặt sân hộ khách")
    
    UC_Inventory("Quản lý kho dụng cụ")
    UC_Update_Damaged("Báo cáo/Cập nhật dụng cụ hỏng")
    
    UC_Support_Customer("Xem & Hỗ trợ khách hàng")
    UC_View_Customer_History("Tra cứu thông tin & lịch sử khách")
```

---

## 2. Đặc tả chi tiết các Use Case chính của Staff

### UC-STAFF-01: Đăng nhập hệ thống (Staff Login)
* **Tác nhân:** Nhân viên (Staff).
* **Mô tả:** Nhân viên đăng nhập vào trang quản trị để bắt đầu ca làm việc.
* **Tiền điều kiện:** Đã được Admin cấp tài khoản với quyền Staff.
* **Luồng chính:**
  1. Staff truy cập đường dẫn trang quản lý và điền email/mật khẩu.
  2. Hệ thống kiểm tra quyền, nếu đúng vai trò Staff thì chuyển hướng vào giao diện quản lý của Staff.

---

### UC-STAFF-02: Đặt sân trực tiếp tại quầy (Walk-in Booking)
* **Tác nhân:** Nhân viên (Staff).
* **Mô tả:** Hỗ trợ khách hàng đến đặt sân trực tiếp tại quầy.
* **Tiền điều kiện:** Staff đã đăng nhập vào hệ thống.
* **Luồng chính:**
  1. Khách hàng trực tiếp yêu cầu đặt sân.
  2. Staff kiểm tra sơ đồ sân (Grid View) để tìm sân và khung giờ trống.
  3. Staff khởi tạo đơn đặt sân trực tiếp (`UC_Walkin_Booking`).
  4. **Quan hệ Extend (`<<extend>>` UC_Service_Rent):** Nếu khách muốn thuê thêm vợt hoặc mua nước, Staff chọn thêm dịch vụ bổ sung vào hóa đơn.
  5. **Quan hệ Include (`<<include>>` UC_InPerson_Payment):** Staff thực hiện xử lý thanh toán trực tiếp tại quầy (thu tiền mặt hoặc hướng dẫn quét QR).
  6. Hệ thống in hóa đơn và cập nhật trạng thái sân sang `Booked`.

---

### UC-STAFF-03: Xem lịch ca làm việc & Nhận nhiệm vụ (View Shifts & Tasks)
* **Tác nhân:** Nhân viên (Staff).
* **Mô tả:** Nhân viên theo dõi lịch làm việc được chia bởi Admin/Manager và nhận các công việc được giao trong ngày.
* **Tiền điều kiện:** Staff đã đăng nhập.
* **Luồng chính:**
  1. Staff vào mục "Lịch làm việc" để xem lịch ca trực tuần/tháng.
  2. Staff vào mục "Nhiệm vụ hàng ngày" (`UC_Perform_Tasks`):
     - Xem các đầu việc hệ thống tạo tự động (Ví dụ: "Chuẩn bị nước/vợt cho booking số #1023", "Dọn dẹp vệ sinh sân số 2 lúc 18:00").
     - Hoặc xem các công việc được Quản lý (Manager) chỉ định thủ công.
  3. Sau khi thực hiện xong (ví dụ lau dọn xong sân), Staff nhấn "Hoàn thành nhiệm vụ" để cập nhật trạng thái lên hệ thống.

---

### UC-STAFF-04: Quản lý đặt sân (Manage Booking)
* **Tác nhân:** Nhân viên (Staff).
* **Mô tả:** Hỗ trợ khách hàng đổi ca chơi hoặc hủy lịch đặt theo yêu cầu qua hotline/quầy.
* **Tiền điều kiện:** Staff đã đăng nhập.
* **Luồng chính:**
  1. Staff tìm kiếm thông tin booking của khách bằng Mã đặt sân hoặc Số điện thoại.
  2. **Quan hệ Extend (`<<extend>>` UC_Reschedule_Booking):** Đổi lịch chơi sang khung giờ mới trống theo yêu cầu của khách.
  3. **Quan hệ Extend (`<<extend>>` UC_Cancel_Booking):** Hủy lịch đặt sân của khách (hệ thống tự động tính toán hoàn tiền theo chính sách).

