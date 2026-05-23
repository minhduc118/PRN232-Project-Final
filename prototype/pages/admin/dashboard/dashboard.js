// ============================================================
//  ADMIN — DASHBOARD PAGE
//  pages/admin/dashboard/dashboard.js
//  Requires: ../../../assets/js/shared.js to be loaded first
// ============================================================

document.addEventListener('DOMContentLoaded', () => {
    initRevenueChart();
    initCourtTypeChart();
});

/* ── Revenue Chart (Line / Area) ─────────────────────────── */
function initRevenueChart() {
    const ctx = document.getElementById('revenueChart').getContext('2d');

    const gradBlue = ctx.createLinearGradient(0, 0, 0, 220);
    gradBlue.addColorStop(0, 'rgba(79,110,247,0.3)');
    gradBlue.addColorStop(1, 'rgba(79,110,247,0)');

    const gradGreen = ctx.createLinearGradient(0, 0, 0, 220);
    gradGreen.addColorStop(0, 'rgba(34,211,165,0.2)');
    gradGreen.addColorStop(1, 'rgba(34,211,165,0)');

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['T12/2025', 'T1/2026', 'T2/2026', 'T3/2026', 'T4/2026', 'T5/2026'],
            datasets: [
                {
                    label: 'Doanh thu (triệu đồng)',
                    data: [88, 95, 102, 118, 130, 148.5],
                    borderColor: '#4f6ef7',
                    backgroundColor: gradBlue,
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
                    backgroundColor: gradGreen,
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
                    position: 'top', align: 'end',
                    labels: { color: CHART_DEFAULTS.tickColor, font: { size: 11, family: CHART_DEFAULTS.font }, boxWidth: 12, padding: 16 },
                },
                tooltip: {
                    backgroundColor: CHART_DEFAULTS.tooltipBg,
                    titleColor: CHART_DEFAULTS.tooltipTitle,
                    bodyColor: CHART_DEFAULTS.tooltipBody,
                    borderColor: CHART_DEFAULTS.tooltipBorder,
                    borderWidth: 1,
                    padding: 12,
                    cornerRadius: 10,
                },
            },
            scales: {
                x: {
                    grid: { color: CHART_DEFAULTS.gridColor, drawBorder: false },
                    ticks: { color: CHART_DEFAULTS.tickColor, font: { size: 11 } },
                },
                y: {
                    grid: { color: CHART_DEFAULTS.gridColor, drawBorder: false },
                    ticks: { color: CHART_DEFAULTS.tickColor, font: { size: 11 }, callback: v => v + 'M' },
                    position: 'left',
                },
                y2: {
                    grid: { display: false },
                    ticks: { color: CHART_DEFAULTS.tickColor, font: { size: 11 } },
                    position: 'right',
                },
            },
        },
    });
}

/* ── Court Type Doughnut ─────────────────────────────────── */
function initCourtTypeChart() {
    const ctx = document.getElementById('courtTypeChart').getContext('2d');

    new Chart(ctx, {
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
                    backgroundColor: CHART_DEFAULTS.tooltipBg,
                    titleColor: CHART_DEFAULTS.tooltipTitle,
                    bodyColor: CHART_DEFAULTS.tooltipBody,
                    borderColor: CHART_DEFAULTS.tooltipBorder,
                    borderWidth: 1,
                    padding: 10,
                    cornerRadius: 10,
                    callbacks: { label: ctx => ` ${ctx.label}: ${ctx.parsed}%` },
                },
            },
        },
    });
}
