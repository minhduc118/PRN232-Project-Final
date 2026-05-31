// ============================================================
//  ADMIN — COURTS PAGE (FE-04)
//  pages/admin/courts/courts.js
// ============================================================

const COURT_TYPES = [
    { courtTypeId: 1, typeName: 'Pickleball' },
    { courtTypeId: 2, typeName: 'Cầu lông' },
    { courtTypeId: 3, typeName: 'Bóng đá mini' },
    { courtTypeId: 4, typeName: 'Tennis' },
];

const STATUS_MAP = {
    Available: { label: 'Hoạt động', cls: 'success' },
    Booked: { label: 'Đã đặt', cls: 'info' },
    InUse: { label: 'Đang sử dụng', cls: 'info' },
    Maintenance: { label: 'Bảo trì', cls: 'pending' },
    Inactive: { label: 'Ngưng hoạt động', cls: 'danger' },
};

let courts = [
    { courtId: 1, courtName: 'Sân Pickleball A1', courtCode: 'PCK-A1', courtTypeId: 1, description: 'Sân pickleball tiêu chuẩn quốc tế, mái che chống nắng, đèn LED chiếu sáng ban đêm.', location: 'Khu A - Tầng 1', capacity: 4, surface: 'Polymer', imageUrl: 'https://images.unsplash.com/photo-1554068865-24cecd4e34b8?w=800', status: 'Available', openTime: '06:00', closeTime: '22:00', pricePerHour: 100000, rating: 4.8, reviewCount: 124 },
    { courtId: 2, courtName: 'Sân Pickleball A2', courtCode: 'PCK-A2', courtTypeId: 1, description: 'Sân pickleball tiêu chuẩn, nền sân polymer cao cấp, không trơn trượt.', location: 'Khu A - Tầng 1', capacity: 4, surface: 'Polymer', imageUrl: 'https://images.unsplash.com/photo-1551698618-1dfe5d97d256?w=800', status: 'Available', openTime: '06:00', closeTime: '22:00', pricePerHour: 100000, rating: 4.6, reviewCount: 89 },
    { courtId: 3, courtName: 'Sân Pickleball VIP', courtCode: 'PCK-VIP', courtTypeId: 1, description: 'Sân VIP với khán đài 50 chỗ, điều hòa toàn sân, phù hợp thi đấu và sự kiện.', location: 'Khu B - Tầng 2', capacity: 8, surface: 'Polymer', imageUrl: 'https://images.unsplash.com/photo-1544298338-0ea56c26f56f?w=800', status: 'Available', openTime: '06:00', closeTime: '22:00', pricePerHour: 150000, rating: 4.9, reviewCount: 57 },
    { courtId: 4, courtName: 'Sân Cầu Lông B1', courtCode: 'CL-B1', courtTypeId: 2, description: 'Sân cầu lông tiêu chuẩn BWF, nền gỗ tự nhiên, lưới chuẩn.', location: 'Khu B - Tầng 1', capacity: 4, surface: 'Gỗ', imageUrl: 'https://images.unsplash.com/photo-1626224583764-f87db24ac4ea?w=800', status: 'Available', openTime: '06:00', closeTime: '22:00', pricePerHour: 80000, rating: 4.5, reviewCount: 201 },
    { courtId: 5, courtName: 'Sân Cầu Lông B2', courtCode: 'CL-B2', courtTypeId: 2, description: 'Sân cầu lông ngoài trời có mái che, đèn chiếu sáng cả ban ngày lẫn ban đêm.', location: 'Khu B - Ngoài trời', capacity: 4, surface: 'Gỗ', imageUrl: 'https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=800', status: 'Maintenance', openTime: '06:00', closeTime: '20:00', pricePerHour: 70000, rating: 4.3, reviewCount: 145 },
    { courtId: 6, courtName: 'Sân Bóng Đá Mini C1', courtCode: 'BD-C1', courtTypeId: 3, description: 'Sân bóng đá mini 5 người, cỏ nhân tạo thế hệ 4, đèn LED cao áp.', location: 'Khu C - Tầng trệt', capacity: 10, surface: 'Cỏ nhân tạo', imageUrl: 'https://images.unsplash.com/photo-1529900748604-07564a03e7a6?w=800', status: 'Available', openTime: '06:00', closeTime: '22:00', pricePerHour: 200000, rating: 4.7, reviewCount: 312 },
];

let editingId = null;
let deleteId = null;
let currentView = 'grid';

document.addEventListener('DOMContentLoaded', () => {
    populateTypeFilters();
    populateFormSelects();
    renderCourts();
    updateSummary();
    bindEvents();
});

function getTypeName(id) {
    return COURT_TYPES.find(t => t.courtTypeId === id)?.typeName || 'Khác';
}

function getStatusBadge(status) {
    const s = STATUS_MAP[status] || { label: status, cls: 'info' };
    return `<span class="badge-status ${s.cls}">${s.label}</span>`;
}

function getFilteredCourts() {
    const search = document.getElementById('filterSearch').value.trim().toLowerCase();
    const type = document.getElementById('filterType').value;
    const status = document.getElementById('filterStatus').value;

    return courts.filter(c => {
        const matchSearch = !search ||
            c.courtName.toLowerCase().includes(search) ||
            c.courtCode.toLowerCase().includes(search) ||
            c.location.toLowerCase().includes(search);
        const matchType = !type || String(c.courtTypeId) === type;
        const matchStatus = !status || c.status === status;
        return matchSearch && matchType && matchStatus;
    });
}

function updateSummary() {
    document.getElementById('sumTotal').textContent = courts.length;
    document.getElementById('sumActive').textContent = courts.filter(c => c.status === 'Available').length;
    document.getElementById('sumMaintenance').textContent = courts.filter(c => c.status === 'Maintenance').length;
    document.getElementById('sumInactive').textContent = courts.filter(c => c.status === 'Inactive').length;
}

function renderCourts() {
    const list = getFilteredCourts();
    const gridEl = document.getElementById('courtsGrid');
    const listEl = document.getElementById('courtsListBody');
    const emptyEl = document.getElementById('courtsEmpty');

    if (list.length === 0) {
        gridEl.innerHTML = '';
        listEl.innerHTML = '';
        emptyEl.style.display = 'block';
        return;
    }
    emptyEl.style.display = 'none';

    gridEl.innerHTML = list.map(c => `
        <article class="court-card" data-id="${c.courtId}">
            <div class="court-card-img">
                <img src="${c.imageUrl}" alt="${c.courtName}" loading="lazy" />
                <span class="court-type-tag">${getTypeName(c.courtTypeId)}</span>
                <span class="court-status">${getStatusBadge(c.status)}</span>
            </div>
            <div class="court-card-body">
                <div class="court-card-title">${c.courtName}</div>
                <div class="court-card-code">${c.courtCode}</div>
                <div class="court-card-meta">
                    <span><i class="fa-solid fa-location-dot"></i>${c.location}</span>
                    <span><i class="fa-solid fa-clock"></i>${c.openTime} – ${c.closeTime}</span>
                    <span><i class="fa-solid fa-users"></i>Sức chứa: ${c.capacity} người</span>
                    <span class="court-rating"><i class="fa-solid fa-star"></i>${c.rating} (${c.reviewCount})</span>
                </div>
                <div class="court-card-footer">
                    <div class="court-price">${formatVND(c.pricePerHour)}<small>/giờ</small></div>
                    <div class="court-actions">
                        <button class="btn-icon" title="Sửa" onclick="openEditModal(${c.courtId})"><i class="fa-solid fa-pen"></i></button>
                        <button class="btn-icon danger" title="Xóa" onclick="openDeleteModal(${c.courtId})"><i class="fa-solid fa-trash"></i></button>
                    </div>
                </div>
            </div>
        </article>
    `).join('');

    listEl.innerHTML = list.map(c => `
        <tr>
            <td><strong>${c.courtCode}</strong><br><span class="td-sub">${c.courtName}</span></td>
            <td>${getTypeName(c.courtTypeId)}</td>
            <td>${c.location}</td>
            <td>${c.openTime} – ${c.closeTime}</td>
            <td class="amount" style="color:var(--col-accent)">${formatVND(c.pricePerHour)}</td>
            <td>${getStatusBadge(c.status)}</td>
            <td>
                <button class="btn btn-ghost btn-sm" onclick="openEditModal(${c.courtId})"><i class="fa-solid fa-pen"></i></button>
                <button class="btn btn-ghost btn-sm" onclick="openDeleteModal(${c.courtId})"><i class="fa-solid fa-trash" style="color:var(--col-danger)"></i></button>
            </td>
        </tr>
    `).join('');
}

function populateTypeFilters() {
    const sel = document.getElementById('filterType');
    COURT_TYPES.forEach(t => {
        sel.innerHTML += `<option value="${t.courtTypeId}">${t.typeName}</option>`;
    });
}

function populateFormSelects() {
    const typeSel = document.getElementById('fieldType');
    typeSel.innerHTML = COURT_TYPES.map(t =>
        `<option value="${t.courtTypeId}">${t.typeName}</option>`
    ).join('');
}

function bindEvents() {
    ['filterSearch', 'filterType', 'filterStatus'].forEach(id => {
        document.getElementById(id).addEventListener('input', renderCourts);
        document.getElementById(id).addEventListener('change', renderCourts);
    });

    document.getElementById('btnAddCourt').addEventListener('click', openAddModal);
    document.getElementById('btnSaveCourt').addEventListener('click', saveCourt);

    document.querySelectorAll('.view-toggle-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.view-toggle-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            currentView = btn.dataset.view;
            document.getElementById('courtsGridView').classList.toggle('active', currentView === 'grid');
            document.getElementById('courtsListView').classList.toggle('active', currentView === 'list');
        });
    });

    document.querySelectorAll('.form-tab').forEach(tab => {
        tab.addEventListener('click', () => {
            document.querySelectorAll('.form-tab').forEach(t => t.classList.remove('active'));
            document.querySelectorAll('.form-panel').forEach(p => p.classList.remove('active'));
            tab.classList.add('active');
            document.getElementById(tab.dataset.panel).classList.add('active');
        });
    });

    document.querySelectorAll('#courtForm .form-group[data-validate] input, #courtForm .form-group[data-validate] select, #courtForm .form-group[data-validate] textarea').forEach(input => {
        input.addEventListener('blur', () => validateFormGroup(input.closest('.form-group')));
        input.addEventListener('input', () => {
            const g = input.closest('.form-group');
            if (g.classList.contains('invalid')) validateFormGroup(g);
        });
    });

    document.getElementById('btnConfirmDelete').addEventListener('click', confirmDelete);
}

function resetForm() {
    const form = document.getElementById('courtForm');
    form.reset();
    form.querySelectorAll('.form-group').forEach(g => {
        g.classList.remove('invalid', 'valid');
    });
    document.querySelectorAll('.form-tab')[0].click();
    document.getElementById('fieldStatus').value = 'Available';
    document.getElementById('fieldPeakMultiplier').value = '1.5';
}

function openAddModal() {
    editingId = null;
    resetForm();
    document.getElementById('modalTitle').textContent = 'Thêm sân mới';
    openModal('courtModal');
}

function openEditModal(id) {
    const c = courts.find(x => x.courtId === id);
    if (!c) return;
    editingId = id;
    resetForm();
    document.getElementById('modalTitle').textContent = 'Chỉnh sửa sân';
    document.getElementById('fieldName').value = c.courtName;
    document.getElementById('fieldCode').value = c.courtCode;
    document.getElementById('fieldType').value = c.courtTypeId;
    document.getElementById('fieldLocation').value = c.location;
    document.getElementById('fieldCapacity').value = c.capacity;
    document.getElementById('fieldSurface').value = c.surface || '';
    document.getElementById('fieldOpenTime').value = c.openTime;
    document.getElementById('fieldCloseTime').value = c.closeTime;
    document.getElementById('fieldPrice').value = c.pricePerHour;
    document.getElementById('fieldStatus').value = c.status;
    document.getElementById('fieldDescription').value = c.description || '';
    document.getElementById('fieldImageUrl').value = c.imageUrl || '';
    openModal('courtModal');
}

function saveCourt() {
    const form = document.getElementById('courtForm');
    if (!validateForm(form)) {
        showToast('Vui lòng kiểm tra lại các trường bắt buộc', 'error');
        return;
    }

    const openTime = document.getElementById('fieldOpenTime').value;
    const closeTime = document.getElementById('fieldCloseTime').value;
    if (openTime >= closeTime) {
        showToast('Giờ đóng cửa phải sau giờ mở cửa', 'error');
        return;
    }

    const code = document.getElementById('fieldCode').value.trim().toUpperCase();
    const duplicate = courts.find(c => c.courtCode === code && c.courtId !== editingId);
    if (duplicate) {
        showToast('Mã sân đã tồn tại trong hệ thống', 'error');
        return;
    }

    const data = {
        courtName: document.getElementById('fieldName').value.trim(),
        courtCode: code,
        courtTypeId: Number(document.getElementById('fieldType').value),
        location: document.getElementById('fieldLocation').value.trim(),
        capacity: Number(document.getElementById('fieldCapacity').value),
        surface: document.getElementById('fieldSurface').value.trim(),
        openTime,
        closeTime,
        pricePerHour: Number(document.getElementById('fieldPrice').value),
        status: document.getElementById('fieldStatus').value,
        description: document.getElementById('fieldDescription').value.trim(),
        imageUrl: document.getElementById('fieldImageUrl').value.trim() ||
            'https://images.unsplash.com/photo-1554068865-24cecd4e34b8?w=800',
    };

    if (editingId) {
        const idx = courts.findIndex(c => c.courtId === editingId);
        courts[idx] = { ...courts[idx], ...data };
        showToast('Cập nhật sân thành công');
    } else {
        courts.push({
            ...data,
            courtId: Math.max(...courts.map(c => c.courtId)) + 1,
            rating: 0,
            reviewCount: 0,
        });
        showToast('Thêm sân mới thành công');
    }

    closeModal('courtModal');
    renderCourts();
    updateSummary();
}

function openDeleteModal(id) {
    deleteId = id;
    const c = courts.find(x => x.courtId === id);
    document.getElementById('deleteCourtName').textContent = c?.courtName || '';
    openModal('deleteModal');
}

function confirmDelete() {
    if (deleteId) {
        courts = courts.filter(c => c.courtId !== deleteId);
        showToast('Đã xóa sân khỏi hệ thống');
        renderCourts();
        updateSummary();
    }
    deleteId = null;
    closeModal('deleteModal');
}
