# 📋 Kế hoạch chia Task — Prototype Frontend (2 Ngày)

Tài liệu này phân chia công việc cho 6 thành viên để hoàn thiện bản Prototype HTML/CSS cho hệ thống quản lý sân thể thao, dựa trên cấu trúc đã thiết lập.

---

## 🏗️ Nguyên tắc chung
- **Sử dụng tài nguyên dùng chung:** Tất cả trang PHẢI import `assets/css/shared.css` và `assets/js/shared.js`.
- **Nhất quán giao diện:** Bám sát Design Tokens (màu sắc, bo góc, transition) trong `shared.css`.
- **Cấu trúc trang:** Mỗi trang nằm trong thư mục riêng `pages/[role]/[page]/` (đã tạo sẵn).
- **Thời hạn:** 2 ngày (Day 1: Xây dựng layout & HTML/CSS; Day 2: Hoàn thiện dữ liệu & JS cơ bản).

---

## 👥 Phân chia công việc

### 🔴 Member 1: Quản trị Hệ thống & Sân (Trưởng nhóm - Code cứng)
- **Nhiệm vụ:**
  - `pages/admin/courts/`: Danh sách sân (Grid view), Form Thêm/Sửa sân (FE-04).
  - `pages/admin/reports/`: Báo cáo doanh thu chi tiết, bộ lọc ngày tháng (FE-09).
- **Yêu cầu:** Đảm bảo các form nhập liệu có validation CSS đẹp.

### 🔴 Member 2: Quản trị Đặt sân & Dịch vụ (Code cứng)
- **Nhiệm vụ:**
  - `pages/admin/bookings/`: Quản lý danh sách đặt sân (List & Calendar view), chi tiết booking (FE-02, FE-13).
  - `pages/admin/promotions/`: Quản lý mã giảm giá, khuyến mãi (FE-11).
- **Yêu cầu:** Giao diện Calendar mockup sử dụng bảng hoặc CSS Grid.

### 🟠 Member 3: Quản lý khu vực (Manager Role)
- **Nhiệm vụ:**
  - `pages/manager/dashboard/`: Dashboard riêng cho Manager (FE-20).
  - `pages/manager/staff/`: Danh sách nhân viên khu vực, giao diện xếp ca (FE-18).
  - `pages/manager/tasks/`: Giao diện giao việc cho nhân viên (Manual vs Automated task list).
- **Yêu cầu:** Hiển thị rõ sự khác biệt giữa quyền Quản lý khu vực và Admin tổng.

### 🟠 Member 4: Khách hàng - Tìm kiếm & Khám phá
- **Nhiệm vụ:**
  - `pages/customer/home/`: Landing page (Banner, giới thiệu, tìm kiếm nhanh FE-01).
  - `pages/customer/search/`: Trang kết quả tìm kiếm với bộ lọc (Filter sidebar).
  - `pages/customer/court-detail/`: Trang chi tiết sân (Hình ảnh, tiện ích, đánh giá FE-12).
- **Yêu cầu:** Giao diện cần "WOW" và thân thiện với người dùng cuối.

### 🟠 Member 5: Khách hàng - Đặt sân & Luồng thanh toán
- **Nhiệm vụ:**
  - `pages/customer/booking/`: Luồng chọn slot giờ, chọn dịch vụ kèm theo (FE-01).
  - `pages/customer/payment/`: Mockup màn hình thanh toán VNPay/MoMo/Chuyển khoản (FE-03).
  - `pages/customer/waitlist/`: Giao diện đăng ký hàng chờ (FE-14).
- **Yêu cầu:** Chú trọng vào trải nghiệm UX (các bước đặt sân 1-2-3).

### 🟢 Member 6: Tài khoản & Chăm sóc khách hàng (Lượng việc nhẹ hơn)
- **Nhiệm vụ:**
  - `pages/auth/`: Đăng nhập, Đăng ký, Quên mật khẩu.
  - `pages/customer/history/`: Lịch sử đặt sân của tôi, trạng thái hóa đơn (FE-15).
  - `pages/customer/profile/`: Thông tin cá nhân, hạng thành viên (Membership Tiers).
  - `pages/shared/notifications/`: Mockup mẫu Email/Thông báo gửi cho khách (FE-08).
- **Yêu cầu:** Tập trung vào sự chỉn chu của các form và thông tin cá nhân.

---

## 🛠️ Hướng dẫn kỹ thuật cho Team

1. **Bước 1: Link CSS/JS chung**
   ```html
   <link rel="stylesheet" href="../../../assets/css/shared.css">
   <!-- Code CSS riêng của bạn bên dưới -->
   <link rel="stylesheet" href="page-name.css">
   ```

2. **Bước 2: Sử dụng Components có sẵn**
   - Nút bấm: `<button class="btn btn-primary">`
   - Trạng thái: `<span class="badge-status success">`
   - Thẻ nội dung: `<div class="card">`

3. **Bước 3: Icons**
   - Sử dụng thư viện **FontAwesome 6** (đã có link trong script).
   - Ví dụ: `<i class="fa-solid fa-calendar"></i>`

---

## 📅 Roadmap 48h
- **0h-12h:** Clone repo, đọc kỹ `shared.css`. Tạo file HTML thô cho các màn hình.
- **12h-24h:** Hoàn thiện CSS cho các màn hình phụ trách. Đảm bảo Responsive.
- **24h-36h:** Thêm JS cơ bản cho các nút bấm, chuyển trang mockup, Chart.js.
- **36h-48h:** Review chéo, sửa lỗi hiển thị, chuẩn bị demo.

*Tài liệu được tạo tự động bởi Antigravity dựa trên SRS v2.0.*
