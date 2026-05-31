/* ============================================================
   COURTMANAGER PRO — ENTERPRISE DASHBOARD JS LOGIC
   pages/manager/dashboard/dashboard.js
   ============================================================ */

document.addEventListener('DOMContentLoaded', function() {
    // ── Revenue Chart Initialization (Chart.js) ──
    const ctxRevenue = document.getElementById('revenueChart');
    if (ctxRevenue) {
        new Chart(ctxRevenue.getContext('2d'), {
            type: 'line',
            data: {
                labels: ['08:00', '10:00', '12:00', '14:00', '16:00', '18:00', '20:00', '22:00'],
                datasets: [{
                    label: 'Revenue ($)',
                    data: [450, 800, 1200, 950, 1500, 2100, 1800, 1200],
                    borderColor: '#c0c1ff', // primary
                    backgroundColor: 'rgba(192, 193, 255, 0.05)',
                    fill: true,
                    tension: 0.4,
                    borderWidth: 2,
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    pointBackgroundColor: '#c0c1ff',
                    pointBorderColor: '#13131b'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: { 
                        grid: { color: 'rgba(144, 143, 160, 0.08)' },
                        ticks: { color: '#908fa0', font: { size: 10 } }
                    },
                    x: { 
                        grid: { display: false },
                        ticks: { color: '#908fa0', font: { size: 10 } }
                    }
                }
            }
        });
    }

    // ── Payment Distribution Chart Initialization (Chart.js) ──
    const ctxPayment = document.getElementById('paymentChart');
    if (ctxPayment) {
        new Chart(ctxPayment.getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: ['VNPay', 'MoMo', 'Cash', 'Other'],
                datasets: [{
                    data: [45, 35, 15, 5],
                    backgroundColor: [
                        '#c0c1ff', // primary
                        '#7bd0ff', // secondary
                        '#ffb783', // tertiary
                        '#464554'  // outline-variant
                    ],
                    borderWidth: 0,
                    hoverOffset: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '75%',
                plugins: {
                    legend: { display: false }
                }
            }
        });
    }

    // ── Atmospheric Glow Micro-interactions (Mouse Tracking) ──
    document.querySelectorAll('.glass-card').forEach(card => {
        card.addEventListener('mousemove', (e) => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            card.style.setProperty('--mouse-x', `${x}px`);
            card.style.setProperty('--mouse-y', `${y}px`);
        });
    });
});
