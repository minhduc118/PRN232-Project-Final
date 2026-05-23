// ============================================================
//  SPORTS COURT MANAGEMENT — ADMIN DASHBOARD
//  main.js — Interactivity & Charts
// ============================================================

/* ── Sidebar Toggle ──────────────────────────────────────── */
function toggleSidebar() {
    const layout = document.getElementById('layout');
    layout.classList.toggle('sidebar-collapsed');
}

/* ── Tab Buttons ─────────────────────────────────────────── */
document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        this.closest('.card-actions').querySelectorAll('.tab-btn')
            .forEach(b => b.classList.remove('active'));
        this.classList.add('active');
    });
});

/* ── Nav Items ───────────────────────────────────────────── */
document.querySelectorAll('.nav-item').forEach(item => {
    item.addEventListener('click', function (e) {
        e.preventDefault();
        document.querySelectorAll('.nav-item').forEach(i => i.classList.remove('active'));
        this.classList.add('active');
    });
});

/* ── Revenue Chart (Line) ────────────────────────────────── */
const revenueCtx = document.getElementById('revenueChart').getContext('2d');

const gradient = revenueCtx.createLinearGradient(0, 0, 0, 220);
gradient.addColorStop(0, 'rgba(79,110,247,0.3)');
gradient.addColorStop(1, 'rgba(79,110,247,0)');

const gradient2 = revenueCtx.createLinearGradient(0, 0, 0, 220);
gradient2.addColorStop(0, 'rgba(34,211,165,0.2)');
gradient2.addColorStop(1, 'rgba(34,211,165,0)');

new Chart(revenueCtx, {
    type: 'line',
    data: {
        labels: ['T12/2025', 'T1/2026', 'T2/2026', 'T3/2026', 'T4/2026', 'T5/2026'],
        datasets: [
            {
                label: 'Doanh thu (triệu đồng)',
                data: [88, 95, 102, 118, 130, 148.5],
                borderColor: '#4f6ef7',
                backgroundColor: gradient,
                borderWidth: 2.5,
                pointBackgroundColor: '#4f6ef7',
                pointBorderColor: '#0f1117',
                pointBorderWidth: 2,
                pointRadius: 5,
                tension: 0.45,
                fill: true,
            },
            {
                label: 'Số lượng booking',
                data: [55, 62, 70, 80, 75, 87],
                borderColor: '#22d3a5',
                backgroundColor: gradient2,
                borderWidth: 2,
                pointBackgroundColor: '#22d3a5',
                pointBorderColor: '#0f1117',
                pointBorderWidth: 2,
                pointRadius: 4,
                tension: 0.45,
                fill: true,
                yAxisID: 'y2',
            },
        ],
    },
    options: {
        responsive: true,
        maintainAspectRatio: false,
        interaction: { mode: 'index', intersect: false },
        plugins: {
            legend: {
                position: 'top',
                align: 'end',
                labels: {
                    color: '#8892a4',
                    font: { size: 11, family: 'Inter' },
                    boxWidth: 12,
                    padding: 16,
                },
            },
            tooltip: {
                backgroundColor: '#1e2130',
                titleColor: '#e8eaf6',
                bodyColor: '#8892a4',
                borderColor: 'rgba(255,255,255,0.08)',
                borderWidth: 1,
                padding: 12,
                cornerRadius: 10,
                titleFont: { size: 12, family: 'Inter', weight: '600' },
                bodyFont: { size: 12, family: 'Inter' },
            },
        },
        scales: {
            x: {
                grid: { color: 'rgba(255,255,255,0.05)', drawBorder: false },
                ticks: { color: '#8892a4', font: { size: 11, family: 'Inter' } },
            },
            y: {
                grid: { color: 'rgba(255,255,255,0.05)', drawBorder: false },
                ticks: {
                    color: '#8892a4',
                    font: { size: 11, family: 'Inter' },
                    callback: v => v + 'M',
                },
                position: 'left',
            },
            y2: {
                grid: { display: false },
                ticks: { color: '#8892a4', font: { size: 11, family: 'Inter' } },
                position: 'right',
            },
        },
    },
});

/* ── Court Type Doughnut ─────────────────────────────────── */
const courtCtx = document.getElementById('courtTypeChart').getContext('2d');

new Chart(courtCtx, {
    type: 'doughnut',
    data: {
        labels: ['Pickleball', 'Cầu lông', 'Bóng đá'],
        datasets: [{
            data: [42, 31, 27],
            backgroundColor: ['#4f6ef7', '#22d3a5', '#f7b955'],
            hoverBackgroundColor: ['#6b85f8', '#34d9b3', '#f8c46b'],
            borderWidth: 0,
            hoverOffset: 6,
        }],
    },
    options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '70%',
        plugins: {
            legend: { display: false },
            tooltip: {
                backgroundColor: '#1e2130',
                titleColor: '#e8eaf6',
                bodyColor: '#8892a4',
                borderColor: 'rgba(255,255,255,0.08)',
                borderWidth: 1,
                padding: 10,
                cornerRadius: 10,
                callbacks: {
                    label: ctx => ` ${ctx.label}: ${ctx.parsed}% booking`,
                },
            },
        },
    },
});

/* ── Responsive Sidebar (Mobile) ─────────────────────────── */
function handleResize() {
    const layout = document.getElementById('layout');
    if (window.innerWidth <= 900) {
        layout.classList.remove('sidebar-collapsed');
    }
}
window.addEventListener('resize', handleResize);
