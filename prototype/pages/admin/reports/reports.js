// ============================================================
//  ADMIN — REPORTS PAGE (FE-09)
//  pages/admin/reports/reports.js
// ============================================================

const REPORT_DATA = {
    month: {
        labels: ['T12/25', 'T1/26', 'T2/26', 'T3/26', 'T4/26', 'T5/26'],
        revenue: [88000000, 95000000, 102000000, 118000000, 130000000, 148500000],
        bookings: [420, 455, 490, 560, 520, 587],
        courtType: { labels: ['Pickleball', 'Cầu lông', 'Bóng đá mini'], values: [62000000, 46000000, 40500000] },
        peakHours: { labels: ['6-8h', '8-10h', '10-12h', '12-14h', '14-16h', '16-18h', '18-20h', '20-22h'], values: [12, 28, 35, 22, 30, 48, 72, 55] },
        topCustomers: [
            { name: 'Nguyễn Văn A', phone: '0912 345 678', bookings: 24, spent: 4800000 },
            { name: 'Trần Thị B', phone: '0987 654 321', bookings: 19, spent: 3920000 },
            { name: 'Lê Minh C', phone: '0901 234 567', bookings: 17, spent: 5100000 },
            { name: 'Phạm Thị D', phone: '0938 765 432', bookings: 15, spent: 2850000 },
            { name: 'Hoàng Văn E', phone: '0977 111 222', bookings: 14, spent: 3360000 },
        ],
        utilization: [
            { name: 'Pickleball', pct: 78 },
            { name: 'Cầu lông', pct: 65 },
            { name: 'Bóng đá mini', pct: 82 },
            { name: 'Tennis', pct: 45 },
        ],
    },
    week: {
        labels: ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'],
        revenue: [18500000, 21000000, 19800000, 22500000, 24200000, 31000000, 28000000],
        bookings: [85, 92, 88, 95, 102, 128, 115],
        courtType: { labels: ['Pickleball', 'Cầu lông', 'Bóng đá mini'], values: [8200000, 6100000, 5400000] },
        peakHours: { labels: ['6-8h', '8-10h', '10-12h', '12-14h', '14-16h', '16-18h', '18-20h', '20-22h'], values: [8, 18, 22, 15, 20, 32, 48, 38] },
        topCustomers: [
            { name: 'Nguyễn Văn A', phone: '0912 345 678', bookings: 5, spent: 980000 },
            { name: 'Trần Thị B', phone: '0987 654 321', bookings: 4, spent: 820000 },
            { name: 'Lê Minh C', phone: '0901 234 567', bookings: 4, spent: 1100000 },
        ],
        utilization: [
            { name: 'Pickleball', pct: 72 },
            { name: 'Cầu lông', pct: 58 },
            { name: 'Bóng đá mini', pct: 88 },
            { name: 'Tennis', pct: 40 },
        ],
    },
    day: {
        labels: ['06h', '08h', '10h', '12h', '14h', '16h', '18h', '20h', '22h'],
        revenue: [1200000, 2800000, 3500000, 2100000, 3200000, 4800000, 6200000, 5100000, 2800000],
        bookings: [4, 9, 12, 7, 11, 16, 22, 18, 10],
        courtType: { labels: ['Pickleball', 'Cầu lông', 'Bóng đá mini'], values: [12000000, 8500000, 9800000] },
        peakHours: { labels: ['6-8h', '8-10h', '10-12h', '12-14h', '14-16h', '16-18h', '18-20h', '20-22h'], values: [5, 12, 15, 9, 11, 18, 28, 20] },
        topCustomers: [
            { name: 'Hoàng Văn E', phone: '0977 111 222', bookings: 2, spent: 560000 },
            { name: 'Nguyễn Văn A', phone: '0912 345 678', bookings: 1, spent: 180000 },
        ],
        utilization: [
            { name: 'Pickleball', pct: 85 },
            { name: 'Cầu lông', pct: 70 },
            { name: 'Bóng đá mini', pct: 90 },
            { name: 'Tennis', pct: 35 },
        ],
    },
    year: {
        labels: ['2022', '2023', '2024', '2025', '2026'],
        revenue: [680000000, 820000000, 1050000000, 1280000000, 1485000000],
        bookings: [3200, 3850, 4620, 5400, 5870],
        courtType: { labels: ['Pickleball', 'Cầu lông', 'Bóng đá mini'], values: [620000000, 460000000, 405000000] },
        peakHours: { labels: ['6-8h', '8-10h', '10-12h', '12-14h', '14-16h', '16-18h', '18-20h', '20-22h'], values: [120, 280, 350, 220, 300, 480, 720, 550] },
        topCustomers: [
            { name: 'Nguyễn Văn A', phone: '0912 345 678', bookings: 156, spent: 31200000 },
            { name: 'Lê Minh C', phone: '0901 234 567', bookings: 142, spent: 42600000 },
            { name: 'Trần Thị B', phone: '0987 654 321', bookings: 128, spent: 25600000 },
            { name: 'Phạm Thị D', phone: '0938 765 432', bookings: 115, spent: 21850000 },
            { name: 'Hoàng Văn E', phone: '0977 111 222', bookings: 108, spent: 25920000 },
        ],
        utilization: [
            { name: 'Pickleball', pct: 75 },
            { name: 'Cầu lông', pct: 62 },
            { name: 'Bóng đá mini', pct: 80 },
            { name: 'Tennis', pct: 48 },
        ],
    },
};

let currentPeriod = 'month';
let charts = {};

document.addEventListener('DOMContentLoaded', () => {
    initDateInputs();
    bindEvents();
    refreshReport();
});

function initDateInputs() {
    const today = new Date('2026-05-31');
    const monthStart = new Date(today.getFullYear(), today.getMonth(), 1);
    document.getElementById('dateFrom').value = formatInputDate(monthStart);
    document.getElementById('dateTo').value = formatInputDate(today);
}

function formatInputDate(d) {
    return d.toISOString().slice(0, 10);
}

function bindEvents() {
    document.querySelectorAll('#periodTabs .tab-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('#periodTabs .tab-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            currentPeriod = btn.dataset.period;
            refreshReport();
        });
    });

    document.getElementById('btnApplyFilter').addEventListener('click', () => {
        showToast('Đã áp dụng bộ lọc ngày tháng');
        refreshReport();
    });

    document.getElementById('btnExport').addEventListener('click', () => {
        showToast('Đang xuất báo cáo Excel... (mock)');
    });
}

function getData() {
    return REPORT_DATA[currentPeriod] || REPORT_DATA.month;
}

function refreshReport() {
    const data = getData();
    updateKPIs(data);
    renderTopCustomers(data.topCustomers);
    renderUtilization(data.utilization);
    updateCharts(data);
}

function updateKPIs(data) {
    const totalRev = data.revenue.reduce((a, b) => a + b, 0);
    const totalBk = data.bookings.reduce((a, b) => a + b, 0);
    const avgUtil = Math.round(data.utilization.reduce((a, b) => a + b.pct, 0) / data.utilization.length);
    const aov = totalBk > 0 ? Math.round(totalRev / totalBk) : 0;

    document.getElementById('kpiRevenue').textContent = formatShortMoney(totalRev) + '₫';
    document.getElementById('kpiBookings').textContent = totalBk.toLocaleString('vi-VN');
    document.getElementById('kpiUtilization').textContent = avgUtil + '%';
    document.getElementById('kpiAov').textContent = formatVND(aov);

    const periodLabel = { day: 'hôm nay', week: 'tuần này', month: 'tháng này', year: 'năm nay' }[currentPeriod];
    document.getElementById('kpiRevenueSub').textContent = `Tổng ${periodLabel}`;
    document.getElementById('kpiBookingsSub').textContent = `Tổng ${periodLabel}`;
}

function renderTopCustomers(list) {
    document.getElementById('topCustomersBody').innerHTML = list.map((c, i) => `
        <tr>
            <td><span class="rank-badge ${i < 3 ? 'top' : ''}">${i + 1}</span></td>
            <td>
                <div class="td-name">${c.name}</div>
                <div class="td-sub">${c.phone}</div>
            </td>
            <td>${c.bookings}</td>
            <td class="amount" style="color:var(--col-accent)">${formatVND(c.spent)}</td>
        </tr>
    `).join('');
}

function renderUtilization(list) {
    const colors = ['#4f6ef7', '#22d3a5', '#f7b955', '#a855f7'];
    document.getElementById('utilizationBars').innerHTML = list.map((u, i) => `
        <div class="util-bar">
            <span class="util-bar-label">${u.name}</span>
            <div class="util-bar-track">
                <div class="util-bar-fill" style="width:${u.pct}%;background:${colors[i % colors.length]}"></div>
            </div>
            <span class="util-bar-value">${u.pct}%</span>
        </div>
    `).join('');
}

function updateCharts(data) {
    destroyCharts();
    initRevenueChart(data);
    initCourtTypeChart(data);
    initPeakChart(data);
}

function destroyCharts() {
    Object.values(charts).forEach(c => c?.destroy());
    charts = {};
}

function initRevenueChart(data) {
    const ctx = document.getElementById('revenueReportChart').getContext('2d');
    const grad = ctx.createLinearGradient(0, 0, 0, 280);
    grad.addColorStop(0, 'rgba(79,110,247,0.3)');
    grad.addColorStop(1, 'rgba(79,110,247,0)');

    charts.revenue = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.labels,
            datasets: [{
                label: 'Doanh thu',
                data: data.revenue.map(v => v / 1_000_000),
                backgroundColor: grad,
                borderColor: '#4f6ef7',
                borderWidth: 2,
                borderRadius: 6,
            }],
        },
        options: chartOptions('triệu ₫', v => v + 'M'),
    });
}

function initCourtTypeChart(data) {
    const ctx = document.getElementById('courtTypeReportChart').getContext('2d');
    charts.courtType = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: data.courtType.labels,
            datasets: [{
                data: data.courtType.values,
                backgroundColor: ['#4f6ef7', '#22d3a5', '#f7b955'],
                borderWidth: 0,
                hoverOffset: 6,
            }],
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '65%',
            plugins: {
                legend: { position: 'bottom', labels: { color: CHART_DEFAULTS.tickColor, font: { size: 11 }, boxWidth: 10 } },
                tooltip: {
                    backgroundColor: CHART_DEFAULTS.tooltipBg,
                    callbacks: { label: ctx => ` ${ctx.label}: ${formatVND(ctx.parsed)}` },
                },
            },
        },
    });
}

function initPeakChart(data) {
    const ctx = document.getElementById('peakHoursChart').getContext('2d');
    charts.peak = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.peakHours.labels,
            datasets: [{
                label: 'Số booking',
                data: data.peakHours.values,
                backgroundColor: 'rgba(34,211,165,0.7)',
                borderColor: '#22d3a5',
                borderWidth: 1,
                borderRadius: 4,
            }],
        },
        options: {
            ...chartOptions('booking'),
            indexAxis: 'y',
        },
    });
}

function chartOptions(yLabel, tickCb) {
    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: { display: false },
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
                ticks: {
                    color: CHART_DEFAULTS.tickColor,
                    font: { size: 11 },
                    callback: tickCb || (v => v),
                },
            },
        },
    };
}
