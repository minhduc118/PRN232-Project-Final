# Tài liệu Use Case của Quản trị viên (Admin Use Cases)

Tài liệu này mô tả chi tiết các Use Case liên quan đến vai trò **Quản trị viên (Admin)** trong **Hệ thống Quản lý Sân Thể thao**.

---

## 1. Sơ đồ Use Case (Mermaid Diagram)

```mermaid
usecaseDiagram
    actor Admin as "Quản trị viên (Admin)"

    rect/quản lý vận hành hệ thống/
        Admin --> UC_Manage_Courts
        Admin --> UC_Manage_Pricing
        Admin --> UC_Manage_Users
        Admin --> UC_Manage_Promotions
        Admin --> UC_Maintenance
        Admin --> UC_Staff_Shifts
        Admin --> UC_Statistics
    end

    %% Tên Use Cases cho Admin
    UC_Manage_Courts("Quản lý danh sách & loại sân")
    UC_Manage_Pricing("Cấu hình giá theo khung giờ/ngày")
    UC_Manage_Users("Quản lý người dùng & phân quyền")
    UC_Manage_Promotions("Quản lý chương trình khuyến mãi")
    UC_Maintenance("Quản lý & lên lịch bảo trì sân")
    UC_Staff_Shifts("Chia ca & xếp lịch làm việc")
    UC_Statistics("Thống kê doanh thu & báo cáo")
```

---

## 2. Đặc tả chi tiết các Use Case chính của Admin

### UC-ADMIN-01: Cấu hình giá thuê theo khung giờ và ngày (Court Pricing Config)
* **Tác nhân (Actor):** Quản trị viên (Admin).
* **Mô tả:** Thiết lập bảng giá linh hoạt cho các khung giờ khác nhau (giờ vàng/giờ thường) hoặc các ngày khác nhau (ngày thường/ngày lễ/cuối tuần).
* **Tiền điều kiện (Precondition):** Admin đăng nhập hệ thống.
* **Luồng chính:**
  1. Admin chọn chức năng **Cấu hình giá sân**.
  2. Chọn loại sân (Ví dụ: Sân Pickleball) hoặc chọn áp dụng cho một sân cụ thể.
  3. Thiết lập khung giờ:
     - Giờ thường (Ví dụ: 06:00 - 15:00): Giá 100,000đ/giờ.
     - Giờ vàng (Ví dụ: 15:00 - 22:00): Giá 180,000đ/giờ.
  4. Thiết lập ngày áp dụng: Ngày thường (Thứ 2 - Thứ 6) hoặc Cuối tuần (Thứ 7 - CN, phụ thu thêm %).
  5. Nhấn lưu cấu hình. Hệ thống áp dụng bảng giá mới cho tất cả các lượt đặt sân phát sinh sau thời điểm lưu.

---

### UC-ADMIN-02: Quản lý và lên lịch bảo trì sân (Maintenance Scheduling)
* **Tác nhân (Actor):** Quản trị viên (Admin).
* **Mô tả:** Thiết lập lịch bảo trì định kỳ hoặc đột xuất cho một hoặc nhiều sân. Khóa sân không cho khách đặt trong thời gian bảo trì.
* **Tiền điều kiện (Precondition):** Admin đăng nhập hệ thống.
* **Luồng chính:**
  1. Admin chọn chức năng **Quản lý lịch bảo trì**.
  2. Tạo lịch bảo trì mới: Chọn sân cần bảo trì, lý do, thời gian bắt đầu và kết thúc.
  3. Hệ thống tự động kiểm tra xem có booking nào bị trùng trong thời gian bảo trì hay không.
     - Nếu có trùng: Hệ thống hiển thị danh sách các booking bị ảnh hưởng và gửi thông báo tự động hủy lịch, hoàn tiền 100% cho khách hàng.
  4. Admin xác nhận, hệ thống khóa sân đó trên giao diện đặt sân của khách hàng, hiển thị trạng thái sân là `Maintenance`.
  5. Sau khi hết thời gian bảo trì, trạng thái sân tự động chuyển lại thành `Available`.

---

### UC-ADMIN-03: Thống kê doanh thu và báo cáo (Analytics & Reporting)
* **Tác nhân (Actor):** Quản trị viên (Admin).
* **Mô tả:** Xem báo cáo trực quan về tình hình kinh doanh, doanh thu và hiệu suất sử dụng sân.
* **Tiền điều kiện (Precondition):** Admin đăng nhập hệ thống.
* **Luồng chính:**
  1. Admin truy cập màn hình **Dashboard Thống kê**.
  2. Chọn bộ lọc: Xem theo ngày/tuần/tháng/năm hoặc khoảng thời gian tùy chọn, hoặc lọc theo chi nhánh/tổ hợp sân.
  3. Hệ thống hiển thị:
     - Tổng doanh thu (Doanh thu đặt sân + Doanh thu dịch vụ đi kèm).
     - Biểu đồ đường thể hiện xu hướng doanh thu.
     - Tỷ lệ lấp đầy sân (%).
     - Khung giờ cao điểm có lượng đặt sân nhiều nhất.
     - Top khách hàng chi tiêu nhiều nhất.
  4. Admin có thể xuất báo cáo ra file Excel hoặc PDF để lưu trữ.
