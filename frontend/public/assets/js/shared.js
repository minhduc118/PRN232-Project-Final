// ============================================================
//  SHARED UTILITIES — Sports Court Management System
//  assets/js/shared.js
//  Import this in every page.
// ============================================================

/* ── Sidebar Toggle ──────────────────────────────────────── */
function toggleSidebar() {
    const layout = document.getElementById('layout');
    layout.classList.toggle('sidebar-collapsed');
}

/* ── Nav Active State ────────────────────────────────────── */
function initNavItems() {
    document.querySelectorAll('.nav-item').forEach(item => {
        item.addEventListener('click', function (e) {
            e.preventDefault();
            document.querySelectorAll('.nav-item').forEach(i => i.classList.remove('active'));
            this.classList.add('active');
        });
    });
}

/* ── Tab Buttons ─────────────────────────────────────────── */
function initTabButtons() {
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            this.closest('.card-actions').querySelectorAll('.tab-btn')
                .forEach(b => b.classList.remove('active'));
            this.classList.add('active');
        });
    });
}

/* ── Responsive Sidebar (Mobile) ─────────────────────────── */
function initResponsiveSidebar() {
    window.addEventListener('resize', () => {
        const layout = document.getElementById('layout');
        if (window.innerWidth <= 900) {
            layout.classList.remove('sidebar-collapsed');
        }
    });
}

/* ── Chart.js Default Theme ──────────────────────────────── */
const CHART_DEFAULTS = {
    tooltipBg: '#1e2130',
    tooltipTitle: '#e8eaf6',
    tooltipBody: '#8892a4',
    tooltipBorder: 'rgba(255,255,255,0.08)',
    gridColor: 'rgba(255,255,255,0.05)',
    tickColor: '#8892a4',
    font: 'Inter',
};

function applyChartDefaults() {
    if (typeof Chart === 'undefined') return;
    Chart.defaults.font.family = CHART_DEFAULTS.font;
    Chart.defaults.color = CHART_DEFAULTS.tickColor;
}

/* ── Format helpers ──────────────────────────────────────── */
function formatVND(num) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(num);
}
function formatDate(dateStr) {
    return new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }).format(new Date(dateStr));
}

/* ── Init on DOM Ready ───────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
    initNavItems();
    initTabButtons();
    initResponsiveSidebar();
    applyChartDefaults();
});
