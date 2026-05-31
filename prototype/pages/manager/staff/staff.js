/* ============================================================
   COURTMANAGER PRO — STAFF & SHIFT MANAGEMENT JS
   pages/manager/staff/staff.js
   ============================================================ */

document.addEventListener('DOMContentLoaded', function() {
    
    // ── Tab Switching Logic ──
    const tabWeeklySchedule = document.getElementById('tabWeeklySchedule');
    const tabStaffDirectory = document.getElementById('tabStaffDirectory');
    const tabAttendanceLogs = document.getElementById('tabAttendanceLogs');

    const schedulerContainer = document.getElementById('schedulerContainer');
    const directoryContainer = document.getElementById('directoryContainer');
    const attendanceContainer = document.getElementById('attendanceContainer');

    const tabs = [
        { btn: tabWeeklySchedule, content: schedulerContainer },
        { btn: tabStaffDirectory, content: directoryContainer },
        { btn: tabAttendanceLogs, content: attendanceContainer }
    ];

    tabs.forEach(tab => {
        if (tab.btn) {
            tab.btn.addEventListener('click', function() {
                // Reset all tabs styles
                tabs.forEach(t => {
                    if (t.btn) {
                        t.btn.classList.remove('text-primary', 'border-primary', 'font-semibold');
                        t.btn.classList.add('text-on-surface-variant');
                    }
                    if (t.content) {
                        t.content.classList.add('hidden');
                    }
                });

                // Apply active styles to clicked tab
                tab.btn.classList.add('text-primary', 'border-primary', 'font-semibold');
                tab.btn.classList.remove('text-on-surface-variant');
                if (tab.content) {
                    tab.content.classList.remove('hidden');
                }
            });
        }
    });

    // ── Side Navigation Active Styles Click Handler ──
    document.querySelectorAll('nav a').forEach(item => {
        item.addEventListener('click', function(e) {
            // Keep the default link navigation unless it's a dummy link
            if (this.getAttribute('href') === '#') {
                e.preventDefault();
                document.querySelectorAll('nav a').forEach(a => {
                    a.classList.remove('text-primary', 'bg-primary/10', 'border-r-2', 'border-primary');
                    a.classList.add('text-on-surface-variant');
                });
                this.classList.add('text-primary', 'bg-primary/10', 'border-r-2', 'border-primary');
                this.classList.remove('text-on-surface-variant');
            }
        });
    });

    // ── Atmospheric Glow Micro-interactions (Mouse Tracking) ──
    document.querySelectorAll('.glass-panel').forEach(panel => {
        panel.addEventListener('mousemove', (e) => {
            const rect = panel.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            panel.style.setProperty('--mouse-x', `${x}px`);
            panel.style.setProperty('--mouse-y', `${y}px`);
        });
    });

    // ── Add New Staff Action Handler ──
    const addStaffBtn = document.getElementById('addStaffBtn');
    if (addStaffBtn) {
        addStaffBtn.addEventListener('click', function() {
            alert('Chức năng "Thêm nhân viên mới" đang được kết nối với cơ sở dữ liệu. Cửa sổ nhập liệu sẽ sớm hiển thị!');
        });
    }
});
