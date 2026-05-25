document.addEventListener("DOMContentLoaded", () => {
    const container = document.getElementById("sidebar-container");
    if (!container) return;

    // Lấy thông tin trang hiện tại để active menu
    const activePage = container.getAttribute("data-active") || "dashboard";

    const sidebarHTML = `
    <aside class="sidebar" id="sidebar">
        <div class="sidebar-logo">
            <div class="logo-icon">🏟️</div>
            <div class="logo-text">
                <h2>SportsPlex</h2>
                <span>Management System</span>
            </div>
        </div>

        <div class="sidebar-section">
            <div class="sidebar-section-label">Tổng quan</div>
            <a href="../dashboard/index.html" class="nav-item ${activePage === 'dashboard' ? 'active' : ''}">
                <i class="fa-solid fa-chart-pie"></i>
                <span class="nav-label">Dashboard</span>
            </a>
            <a href="../bookings/index.html" class="nav-item ${activePage === 'bookings' ? 'active' : ''}">
                <i class="fa-solid fa-calendar-check"></i>
                <span class="nav-label">Đặt sân</span>
                <span class="badge">12</span>
            </a>
            <a href="../courts/index.html" class="nav-item ${activePage === 'courts' ? 'active' : ''}">
                <i class="fa-solid fa-table-tennis-paddle-ball"></i>
                <span class="nav-label">Quản lý sân</span>
            </a>
        </div>

        <div class="sidebar-section">
            <div class="sidebar-section-label">Quản lý</div>
            <a href="#" class="nav-item ${activePage === 'customers' ? 'active' : ''}">
                <i class="fa-solid fa-users"></i>
                <span class="nav-label">Khách hàng</span>
            </a>
            <a href="#" class="nav-item ${activePage === 'staff' ? 'active' : ''}">
                <i class="fa-solid fa-user-tie"></i>
                <span class="nav-label">Nhân viên</span>
            </a>
            <a href="#" class="nav-item ${activePage === 'manager' ? 'active' : ''}">
                <i class="fa-solid fa-user-gear"></i>
                <span class="nav-label">Quản lý (Manager)</span>
            </a>
            <a href="#" class="nav-item ${activePage === 'coach' ? 'active' : ''}">
                <i class="fa-solid fa-whistle"></i>
                <span class="nav-label">Huấn luyện viên</span>
            </a>
            <a href="../promotions/index.html" class="nav-item ${activePage === 'promotions' ? 'active' : ''}">
                <i class="fa-solid fa-gift"></i>
                <span class="nav-label">Khuyến mãi</span>
            </a>
            <a href="#" class="nav-item ${activePage === 'inventory' ? 'active' : ''}">
                <i class="fa-solid fa-toolbox"></i>
                <span class="nav-label">Dịch vụ & Kho</span>
            </a>
        </div>

        <div class="sidebar-section">
            <div class="sidebar-section-label">Báo cáo</div>
            <a href="#" class="nav-item ${activePage === 'reports-revenue' ? 'active' : ''}">
                <i class="fa-solid fa-chart-line"></i>
                <span class="nav-label">Doanh thu</span>
            </a>
            <a href="#" class="nav-item ${activePage === 'reports-invoices' ? 'active' : ''}">
                <i class="fa-solid fa-file-invoice-dollar"></i>
                <span class="nav-label">Hóa đơn</span>
            </a>
            <a href="#" class="nav-item ${activePage === 'system-logs' ? 'active' : ''}">
                <i class="fa-solid fa-clipboard-list"></i>
                <span class="nav-label">Nhật ký hệ thống</span>
            </a>
        </div>

        <div class="sidebar-section">
            <div class="sidebar-section-label">Hệ thống</div>
            <a href="#" class="nav-item ${activePage === 'settings' ? 'active' : ''}">
                <i class="fa-solid fa-gear"></i>
                <span class="nav-label">Cài đặt</span>
            </a>
            <a href="#" class="nav-item ${activePage === 'roles' ? 'active' : ''}">
                <i class="fa-solid fa-shield-halved"></i>
                <span class="nav-label">Phân quyền</span>
            </a>
        </div>

        <div class="sidebar-footer">
            <div class="avatar avatar-md" style="background:linear-gradient(135deg,#4f6ef7,#a855f7)">A</div>
            <div class="footer-info">
                <h4>Super Admin</h4>
                <span><i class="fa-solid fa-right-from-bracket"></i> Đăng xuất</span>
            </div>
        </div>
    </aside>
    `;

    // Thay thế div container bằng đoạn HTML thật của Sidebar
    container.outerHTML = sidebarHTML;
});