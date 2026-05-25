/**
 * STAFF SCHEDULER MANAGEMENT MOCK DATA & LOGIC
 */

// --- MOCK DATA ---
const employees = [
    { id: 'E01', name: 'Nguyễn Văn A', hoursTarget: 32 },
    { id: 'E02', name: 'Trần Minh B', hoursTarget: 24 },
    { id: 'E03', name: 'Lê Hoàng C', hoursTarget: 40 },
    { id: 'E04', name: 'Phạm Thị D', hoursTarget: 20 }
];

const shiftTypes = {
    'morning': { label: 'Ca Sáng', time: '06:00 - 14:00', hours: 8, cls: 'morning' },
    'afternoon': { label: 'Ca Chiều', time: '14:00 - 22:00', hours: 8, cls: 'afternoon' },
    'night': { label: 'Ca Tối', time: '18:00 - 00:00', hours: 6, cls: 'night' }
};

const daysOfWeek = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật'];

let currentWeek = 'W22'; // Tuần mặc định ban đầu là W22

// Dữ liệu theo tuần (Mock cho phép chuyển tuần)
let schedulesByWeek = {
    'W21': [
        // Tuần lấy làm mẫu để Copy
        { empId: 'E01', day: 'Thứ 2', shiftCode: 'morning', warning: false },
        { empId: 'E01', day: 'Thứ 4', shiftCode: 'morning', warning: false },
        { empId: 'E02', day: 'Thứ 3', shiftCode: 'afternoon', warning: false },
        { empId: 'E02', day: 'Thứ 6', shiftCode: 'afternoon', warning: false },
        { empId: 'E03', day: 'Thứ 5', shiftCode: 'night', warning: false },
    ],
    'W22': [
        // Tuần hiện tại đang thao tác
        { empId: 'E01', day: 'Thứ 2', shiftCode: 'morning', warning: false },
        { empId: 'E01', day: 'Thứ 3', shiftCode: 'afternoon', warning: false },
        { empId: 'E01', day: 'Thứ 4', shiftCode: 'night', warning: true }, // Cố tình cảnh báo
        { empId: 'E02', day: 'Thứ 3', shiftCode: 'morning', warning: false },
        { empId: 'E02', day: 'Thứ 5', shiftCode: 'afternoon', warning: false },
        { empId: 'E02', day: 'Thứ 7', shiftCode: 'night', warning: false },
        { empId: 'E03', day: 'Thứ 2', shiftCode: 'morning', warning: false },
        { empId: 'E03', day: 'Thứ 3', shiftCode: 'morning', warning: false },
        { empId: 'E03', day: 'Thứ 4', shiftCode: 'afternoon', warning: false },
    ],
    'W23': [] // Tuần tiếp theo (Trống để bạn test tính năng Copy lịch)
};

const todayList = [
    { empName: 'Nguyễn Văn A', shiftName: 'Ca Sáng (06:00 - 14:00)', branch: 'Quận 1', checkIn: '06:02', status: 'Đã vào ca', statusCls: 'success' },
    { empName: 'Lê Hoàng C', shiftName: 'Ca Sáng (06:00 - 14:00)', branch: 'Quận 1', checkIn: '05:58', status: 'Đã vào ca', statusCls: 'success' },
    { empName: 'Trần Minh B', shiftName: 'Ca Chiều (14:00 - 22:00)', branch: 'Quận 7', checkIn: '---', status: 'Chưa Check-in', statusCls: 'warning' },
    { empName: 'Phạm Thị D', shiftName: 'Ca Tối (18:00 - 00:00)', branch: 'Thủ Đức', checkIn: '---', status: 'Xin nghỉ', statusCls: 'danger' }
];

let currentEditing = { empId: null, day: null };

// --- INIT APP ---
document.addEventListener("DOMContentLoaded", () => {
    if (typeof renderSidebar === 'function') {
        renderSidebar('staff');
    }
    renderApp();
});

function renderApp() {
    renderWeeklyBoard();
    renderTodayList();
    renderStats();
}

// --- LOGIC ĐỔI TUẦN & COPY LỊCH ---
function changeWeek(weekKey) {
    currentWeek = weekKey;
    renderApp();
}

function copyLastWeek() {
    let previousWeek = '';
    if (currentWeek === 'W22') previousWeek = 'W21';
    if (currentWeek === 'W23') previousWeek = 'W22';

    if (!previousWeek || !schedulesByWeek[previousWeek]) {
        alert('Không có dữ liệu của tuần trước để copy!');
        return;
    }

    if (schedulesByWeek[currentWeek].length > 0) {
        const confirmCopy = confirm('Tuần này đã có dữ liệu xếp ca. Tiếp tục sao chép sẽ GHI ĐÈ dữ liệu hiện tại. Bạn có chắc chắn không?');
        if (!confirmCopy) return;
    }

    // Clone dữ liệu sâu sang mảng của tuần hiện tại
    schedulesByWeek[currentWeek] = JSON.parse(JSON.stringify(schedulesByWeek[previousWeek]));
    alert('Đã copy lịch từ tuần trước thành công!');
    renderApp();
}

// --- RENDER LOGIC ---
function renderWeeklyBoard() {
    const grid = document.getElementById('scheduleGrid');
    const scheduleArray = schedulesByWeek[currentWeek];
    
    // Header Row
    let html = `
        <div class="grid-header">Nhân viên</div>
        ${daysOfWeek.map(d => `<div class="grid-header">${d}</div>`).join('')}
    `;

    // Employee Rows
    employees.forEach(emp => {
        // Calculate assigned hours
        const assignedShiftsForEmp = scheduleArray.filter(s => s.empId === emp.id);
        const totalAssignedHours = assignedShiftsForEmp.reduce((sum, s) => sum + shiftTypes[s.shiftCode].hours, 0);

        html += `
            <div class="employee-cell">
                <div class="employee-name">${emp.name}</div>
                <div class="employee-hours">${totalAssignedHours}h / ${emp.hoursTarget}h tuần</div>
            </div>
        `;

        // Shift Cells
        daysOfWeek.forEach(day => {
            const shiftEntry = assignedShiftsForEmp.find(s => s.day === day);
            if (shiftEntry) {
                const shiftDef = shiftTypes[shiftEntry.shiftCode];
                const warningCls = shiftEntry.warning ? 'warning' : '';
                html += `
                    <div class="schedule-cell ${warningCls}" onclick="openShiftDrawer('${emp.id}', '${day}')">
                        <div class="shift ${shiftDef.cls}">
                            ${shiftDef.label}<br>
                            ${shiftDef.time}
                        </div>
                    </div>
                `;
            } else {
                html += `<div class="schedule-cell" onclick="openShiftDrawer('${emp.id}', '${day}')"></div>`;
            }
        });
    });

    grid.innerHTML = html;
    renderSummaryRow();
}

function renderSummaryRow() {
    const summaryRow = document.getElementById('summaryRow');
    const scheduleArray = schedulesByWeek[currentWeek];
    
    let html = `<div class="summary-title">Thống kê Người trực</div>`;
    daysOfWeek.forEach(day => {
        const count = scheduleArray.filter(s => s.day === day).length;
        html += `<div class="summary-item">${count} / ${employees.length} Người</div>`;
    });
    
    summaryRow.innerHTML = html;
}

function renderTodayList() {
    const tbody = document.getElementById('todayListTableBody');
    tbody.innerHTML = todayList.map(item => `
        <tr>
            <td><b>${item.empName}</b></td>
            <td>${item.shiftName}</td>
            <td>${item.branch}</td>
            <td>${item.checkIn}</td>
            <td><span class="badge ${item.statusCls}">${item.status}</span></td>
        </tr>
    `).join('');
}

function renderStats() {
    const scheduleArray = schedulesByWeek[currentWeek];
    
    document.getElementById('totalStaff').innerText = employees.length;
    document.getElementById('assignedShifts').innerText = scheduleArray.length;
    document.getElementById('pendingLeave').innerText = 3;
    document.getElementById('overworkAlerts').innerText = scheduleArray.filter(s => s.warning).length;
}

// --- TABS LOGIC ---
function switchTab(tabId) {
    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
    
    document.querySelector(`button[data-target="${tabId}"]`).classList.add('active');
    document.getElementById(tabId).classList.add('active');
}

// --- DRAWER LOGIC ---
function openShiftDrawer(empId, day) {
    currentEditing = { empId, day };
    const emp = employees.find(e => e.id === empId);
    
    document.getElementById('drawerEmpName').innerText = emp.name;
    document.getElementById('drawerDate').innerText = `${day} - Tuần ${currentWeek.replace('W','')}`;
    
    const scheduleArray = schedulesByWeek[currentWeek];
    const existingShift = scheduleArray.find(s => s.empId === empId && s.day === day);
    
    // Render Option list dynamically
    const optionsContainer = document.getElementById('shiftOptionsContainer');
    let optionsHtml = '';
    
    Object.entries(shiftTypes).forEach(([code, detail]) => {
        const isSelected = (existingShift && existingShift.shiftCode === code) ? 'selected' : '';
        optionsHtml += `
            <div class="shift-option ${isSelected}" data-code="${code}" onclick="selectShiftOption(this)">
                <div>
                    <strong style="color:var(--col-text);">${detail.label}</strong><br>
                    <span style="font-size:12px; color:var(--col-subtext);">${detail.time}</span>
                </div>
                <span class="shift-hours">${detail.hours}h</span>
            </div>
        `;
    });
    optionsContainer.innerHTML = optionsHtml;

    const warningDiv = document.getElementById('drawerWarning');
    if (existingShift && existingShift.warning) {
        warningDiv.style.display = 'block';
    } else {
        warningDiv.style.display = 'none';
    }

    document.getElementById('drawerOverlay').classList.add('open');
    document.getElementById('shiftDrawer').classList.add('open');
}

function closeShiftDrawer() {
    document.getElementById('drawerOverlay').classList.remove('open');
    document.getElementById('shiftDrawer').classList.remove('open');
    currentEditing = { empId: null, day: null };
}

function selectShiftOption(elem) {
    const isCurrentlySelected = elem.classList.contains('selected');
    document.querySelectorAll('.shift-option').forEach(opt => opt.classList.remove('selected'));
    
    if (!isCurrentlySelected) {
        elem.classList.add('selected');
    }
}

function saveShift() {
    const selectedOption = document.querySelector('.shift-option.selected');
    const newShiftCode = selectedOption ? selectedOption.getAttribute('data-code') : null;
    
    let scheduleArray = schedulesByWeek[currentWeek];
    
    // Xóa ca cũ
    scheduleArray = scheduleArray.filter(s => !(s.empId === currentEditing.empId && s.day === currentEditing.day));
    
    // Insert mới
    if (newShiftCode) {
        const isWarning = (currentEditing.empId === 'E01' && currentEditing.day === 'Thứ 4' && newShiftCode === 'night');
        
        scheduleArray.push({
            empId: currentEditing.empId,
            day: currentEditing.day,
            shiftCode: newShiftCode,
            warning: isWarning
        });
    }
    
    schedulesByWeek[currentWeek] = scheduleArray;
    
    renderApp();
    closeShiftDrawer();
}