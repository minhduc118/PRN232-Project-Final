# 🏟️ SportsPlex — Frontend Prototype

Thư mục `prototype/` chứa toàn bộ giao diện mẫu cho hệ thống quản lý sân thể thao.

---

## 📁 Cấu trúc thư mục

```
prototype/
├── assets/                       ← Tài nguyên dùng chung cho tất cả trang
│   ├── css/
│   │   └── shared.css            ← Design tokens, base layout, shared components
│   ├── js/
│   │   └── shared.js             ← Utilities, sidebar toggle, Chart.js defaults
│   └── images/                   ← Logo, avatar placeholder, icons
│
├── pages/                        ← Giao diện từng vai trò
│   ├── admin/                    ← Màn hình Admin
│   │   ├── dashboard/            ✅ Màn hình tổng quan
│   │   ├── courts/               🔜 Quản lý sân
│   │   ├── bookings/             🔜 Quản lý đặt sân
│   │   ├── users/                🔜 Quản lý người dùng
│   │   └── reports/              🔜 Báo cáo doanh thu
│   │
│   ├── manager/                  🔜 Màn hình Quản lý khu vực
│   │   ├── dashboard/
│   │   ├── staff/
│   │   └── tasks/
│   │
│   ├── staff/                    🔜 Màn hình Nhân viên
│   │   ├── dashboard/
│   │   └── tasks/
│   │
│   └── customer/                 🔜 Màn hình Khách hàng
│       ├── home/
│       ├── booking/
│       └── history/
│
└── README.md                     ← File này
```

---

## 📐 Quy ước tổ chức file

Mỗi trang trong `pages/` có cấu trúc:
```
pages/[role]/[page]/
├── index.html      ← Giao diện trang
├── [page].css      ← CSS riêng của trang
└── [page].js       ← JS riêng của trang
```

Trong `index.html` của mỗi trang:
```html
<!-- Shared CSS trước (bắt buộc) -->
<link rel="stylesheet" href="../../../assets/css/shared.css" />
<!-- Page CSS sau -->
<link rel="stylesheet" href="[page].css" />

<!-- Shared JS trước, page JS sau -->
<script src="../../../assets/js/shared.js"></script>
<script src="[page].js"></script>
```

---

## 🎨 Design System

| Token           | Giá trị                   | Mô tả                  |
| --------------- | ------------------------- | ---------------------- |
| `--col-primary` | `#4f6ef7`                 | Màu chính (xanh)       |
| `--col-accent`  | `#22d3a5`                 | Màu nhấn (xanh lá)     |
| `--col-danger`  | `#f75a5a`                 | Màu cảnh báo nguy hiểm |
| `--col-warn`    | `#f7b955`                 | Màu cảnh báo thường    |
| `--col-surface` | `#16181f`                 | Nền card               |
| `--col-bg`      | `#0f1117`                 | Nền trang              |
| `--radius-md`   | `14px`                    | Bo góc card            |
| `--transition`  | `0.22s cubic-bezier(...)` | Hiệu ứng hover         |

---

## ✅ Màn hình đã hoàn thành

| Trang           | Đường dẫn                          | Trạng thái   |
| --------------- | ---------------------------------- | ------------ |
| Admin Dashboard | `pages/admin/dashboard/index.html` | ✅ Hoàn thành |

## 🔜 Màn hình cần làm

| Trang               | Vai trò  | Ưu tiên      |
| ------------------- | -------- | ------------ |
| Quản lý sân (CRUD)  | Admin    | 🔴 Cao        |
| Quản lý đặt sân     | Admin    | 🔴 Cao        |
| Dashboard Manager   | Manager  | 🟠 Trung bình |
| Giao việc nhân viên | Manager  | 🟠 Trung bình |
| Danh sách task      | Staff    | 🟡 Thấp       |
| Trang tìm sân       | Customer | 🟡 Thấp       |

---

*Prototype sử dụng: HTML5, CSS3 (CSS Variables, Grid, Flexbox), Chart.js, FontAwesome 6*
