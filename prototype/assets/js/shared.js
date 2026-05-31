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
            const href = this.getAttribute('href');
            if (href && href !== '#') return;
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

function formatShortMoney(num) {
    if (num >= 1_000_000_000) return (num / 1_000_000_000).toFixed(1) + 'B';
    if (num >= 1_000_000) return (num / 1_000_000).toFixed(1) + 'M';
    if (num >= 1_000) return (num / 1_000).toFixed(0) + 'K';
    return String(num);
}

/* ── Modal ───────────────────────────────────────────────── */
function openModal(id) {
    const overlay = document.getElementById(id);
    if (overlay) overlay.classList.add('open');
    document.body.style.overflow = 'hidden';
}

function closeModal(id) {
    const overlay = document.getElementById(id);
    if (overlay) overlay.classList.remove('open');
    document.body.style.overflow = '';
}

function initModalClose() {
    document.querySelectorAll('.modal-overlay').forEach(overlay => {
        overlay.addEventListener('click', e => {
            if (e.target === overlay) closeModal(overlay.id);
        });
    });
    document.querySelectorAll('[data-close-modal]').forEach(btn => {
        btn.addEventListener('click', () => closeModal(btn.dataset.closeModal));
    });
}

/* ── Toast ─────────────────────────────────────────────────── */
function showToast(message, type = 'success') {
    let container = document.querySelector('.toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container';
        document.body.appendChild(container);
    }
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    const icon = type === 'success' ? 'fa-circle-check' : 'fa-circle-exclamation';
    toast.innerHTML = `<i class="fa-solid ${icon}"></i><span>${message}</span>`;
    container.appendChild(toast);
    setTimeout(() => toast.remove(), 3200);
}

/* ── Form validation helper ────────────────────────────────── */
function validateFormGroup(group) {
    const input = group.querySelector('.form-input, .form-textarea, .form-select-field');
    if (!input) return true;
    const valid = input.checkValidity();
    group.classList.toggle('invalid', !valid);
    group.classList.toggle('valid', valid && input.value.trim() !== '');
    return valid;
}

function validateForm(formEl) {
    const groups = formEl.querySelectorAll('.form-group[data-validate]');
    let ok = true;
    groups.forEach(g => {
        if (!validateFormGroup(g)) ok = false;
    });
    return ok;
}

/* ── Init on DOM Ready ───────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
    initNavItems();
    initTabButtons();
    initResponsiveSidebar();
    initModalClose();
    applyChartDefaults();
});
