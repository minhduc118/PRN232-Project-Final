// MOCK DATA
const COURTS = [                                                               
    { id: 'badminton_1', name: 'Sân Cầu Lông A1', type: 'Badminton' },
    { id: 'badminton_2', name: 'Sân Cầu Lông A2', type: 'Badminton' },
    { id: 'tennis_1', name: 'Sân Tennis T1', type: 'Tennis' }
];

const BOOKINGS = [
    {
        id: 'BK001', customerName: 'Nguyễn Văn Nam', customerPhone: '0901234567', courtId: 'badminton_1',
        date: '2026-05-25', start: '08:00', end: '09:30', status: 'confirmed', total: 235000
    },
    {
        id: 'BK002', customerName: 'Trần Thị Thuỷ', customerPhone: '0987654321', courtId: 'tennis_1',
        date: '2026-05-25', start: '09:00', end: '11:00', status: 'pending', total: 520000
    }
];

// INIT PAGE
document.addEventListener("DOMContentLoaded", () => {
    renderCalendar();
    renderBookingsTable();
});

// TABS LOGIC
function switchTab(tabId) {
    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
    
    document.querySelector(`button[data-target="${tabId}"]`).classList.add('active');
    document.getElementById(tabId).classList.add('active');
}

// CALENDAR RENDER
function renderCalendar(dataToRender = BOOKINGS) {
    const timeAxis = document.getElementById('timeAxis');
    const courtsGrid = document.getElementById('courtsGrid');
    
    // Render time slots 06:00 to 22:00
    let timeHTML = '';
    for (let h = 6; h <= 21; h++) {
        timeHTML += `<div class="time-slot">${String(h).padStart(2, '0')}:00</div>`;
        timeHTML += `<div class="time-slot">${String(h).padStart(2, '0')}:30</div>`;
    }
    timeAxis.innerHTML = timeHTML;

    // Render Court Columns
    let gridHTML = '';
    COURTS.forEach(court => {
        gridHTML += `
            <div class="court-column">
                <div class="court-header">${court.name}</div>
                <div class="court-slots-bg"></div>
                ${generateBookingBlocks(court.id, dataToRender)}
            </div>
        `;
    });
    courtsGrid.innerHTML = gridHTML;
}

// Calculate position based on time
function timeToRow(timeStr) {
    const [h, m] = timeStr.split(':').map(Number);
    const minsFrom6AM = (h * 60 + m) - (6 * 60);
    return (minsFrom6AM / 30) * 40; // 40px per 30 mins
}

function generateBookingBlocks(courtId, dataToRender) {
    let blocksHTML = '';
    const courtBookings = dataToRender.filter(b => b.courtId === courtId);
    
    courtBookings.forEach(b => {
        const top = timeToRow(b.start);
        const height = timeToRow(b.end) - top;
        blocksHTML += `
            <div class="booking-block ${b.status}" style="top: ${top + 40}px; height: ${height}px;" onclick="viewBooking('${b.id}')">
                <b>${b.customerName}</b><br/>
                ${b.start} - ${b.end}
            </div>
        `;
    });
    return blocksHTML;
}

// TABLE RENDER
function renderBookingsTable(dataToRender = BOOKINGS) {
    const tbody = document.getElementById('bookingsTableBody');
    let rows = '';
    
    if (dataToRender.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">Không tìm thấy kết quả</td></tr>';
        return;
    }

    dataToRender.forEach(b => {
        const court = COURTS.find(c => c.id === b.courtId)?.name || b.courtId;
        const badgeClass = b.status === 'confirmed' ? 'success' : (b.status === 'pending' ? 'warning' : 'danger');
        let badgeText = '';
        if (b.status === 'confirmed') badgeText = 'Đã xác nhận';
        else if (b.status === 'pending') badgeText = 'Chờ xác nhận';
        else if (b.status === 'completed') badgeText = 'Đã hoàn thành';
        else badgeText = 'Đã hủy';

        rows += `
            <tr>
                <td><b>${b.id}</b></td>
                <td>${b.customerName}</td>
                <td>${court}</td>
                <td>${b.start} - ${b.end}</td>
                <td>${b.total.toLocaleString()}đ</td>
                <td><span class="badge ${badgeClass}">${badgeText}</span></td>
                <td><button class="btn btn-secondary" onclick="viewBooking('${b.id}')">Xem/Sửa</button></td>
            </tr>
        `;
    });
    tbody.innerHTML = rows;
}

// DRAWER LOGIC
function openDrawer() {
    document.getElementById('drawerTitle').innerText = 'Tạo Đặt Sân Mới';
    document.getElementById('bookingForm').reset();
    document.getElementById('drawerOverlay').classList.add('open');
    document.getElementById('bookingDrawer').classList.add('open');
}

function closeDrawer() {
    document.getElementById('drawerOverlay').classList.remove('open');
    document.getElementById('bookingDrawer').classList.remove('open');
}

function viewBooking(id) {
    const b = BOOKINGS.find(x => x.id === id);
    if(b) {
        document.getElementById('drawerTitle').innerText = 'Chi tiết: ' + b.id;
        document.getElementById('customerName').value = b.customerName;
        document.getElementById('customerPhone').value = b.customerPhone || '';
        document.getElementById('courtSelect').value = b.courtId;
        document.getElementById('timeStart').value = b.start;
        document.getElementById('timeEnd').value = b.end;
        document.getElementById('statusSelect').value = b.status;
        document.getElementById('drawerOverlay').classList.add('open');
        document.getElementById('bookingDrawer').classList.add('open');
    }
}

function saveBooking() {
    alert("Lưu đặt sân thành công (Mock)!");
    closeDrawer();
}

// ---- FILTERING LOGIC ----
function filterBookings() {
    let date = document.getElementById('filterDate').value;
    let courtType = document.getElementById('filterCourt').value;
    let status = document.getElementById('filterStatus').value;
    
    let searchInput = document.getElementById('bookingSearch');
    let kw = searchInput ? searchInput.value.toLowerCase() : '';

    let filtered = BOOKINGS.filter(b => {
        let matchDate = !date || b.date === date;
        let matchCourt = (courtType === 'all') || b.courtId.startsWith(courtType);
        let matchStatus = (status === 'all') || (b.status === status);
        
        let matchKw = true;
        if (kw) {
            matchKw = b.customerName.toLowerCase().includes(kw) || 
                      b.id.toLowerCase().includes(kw) || 
                      (b.customerPhone && b.customerPhone.includes(kw));
        }
        return matchDate && matchCourt && matchStatus && matchKw;
    });

    renderCalendar(filtered);
    renderBookingsTable(filtered);
}

function resetBookingFilters() {
    document.getElementById('filterDate').value = '2026-05-25';
    document.getElementById('filterCourt').value = 'all';
    document.getElementById('filterStatus').value = 'all';
    let searchInput = document.getElementById('bookingSearch');
    if (searchInput) searchInput.value = '';
    
    renderCalendar(BOOKINGS);
    renderBookingsTable(BOOKINGS);
}