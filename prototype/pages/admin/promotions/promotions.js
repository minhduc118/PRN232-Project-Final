let PROMOS = [
    { id: 1, code: 'CHAOHE2026', discount: '15%', limit: 100, used: 72, expiry: '30/06/2026', status: 'Đang chạy' },
    { id: 2, code: 'VIPMEM100K', discount: '100.000đ', limit: 50, used: 48, expiry: '31/12/2026', status: 'Đang chạy' },
    { id: 3, code: 'EXPIRED10', discount: '10%', limit: 200, used: 200, expiry: '01/01/2026', status: 'Đã hết hạn' }
];

let editId = null;

// INIT PAGE
document.addEventListener("DOMContentLoaded", () => {
    renderPromos();
});

function toggleSidebar() {
    const sb = document.getElementById('sidebar');
    if (sb) sb.classList.toggle('collapsed');
}

function filterPromos() {
    let st = document.getElementById('filterStatus').value;
    let kw = document.getElementById('promoSearch').value.toLowerCase();
    
    let filtered = PROMOS.filter(p => {
        let matchStatus = (st === 'all') || (p.status === st);
        let matchKw = p.code.toLowerCase().includes(kw);
        return matchStatus && matchKw;
    });
    renderPromos(filtered);
}

function resetPromoFilters() {
    document.getElementById('filterStatus').value = 'all';
    document.getElementById('filterCourt').value = 'all';
    document.getElementById('promoSearch').value = '';
    renderPromos(PROMOS);
}

function renderPromos(dataToRender = PROMOS) {
    let tbody = document.getElementById('promoTableBody');
    let html = '';
    
    if(dataToRender.length === 0) { 
        tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;">Không tìm thấy kết quả</td></tr>'; 
        return; 
    } 
    
    dataToRender.forEach(p => {
        let badgeCls = p.status === 'Đang chạy' ? 'success' : (p.status === 'Đã hết hạn' ? 'warning' : 'danger');
        
        html += `
            <tr>
                <td><b>${p.code}</b></td>
                <td><span style="color: var(--col-primary); font-weight: bold;">${p.discount}</span></td>
                <td>${p.used} / ${p.limit}</td>
                <td>${p.expiry}</td>
                <td><span class="badge ${badgeCls}">${p.status}</span></td>
                <td>
                    <div class="td-actions">
                        <button class="btn btn-secondary" style="padding: 6px 12px; font-size: 13px; color: white;" onclick="openPromoModal('edit', ${p.id})">
                            <i class="fa-solid fa-pen"></i> Sửa
                        </button>
                        <button class="btn btn-secondary" style="padding: 6px 12px; font-size: 13px; color: var(--col-danger);" onclick="deletePromo(${p.id})">
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
    });
    tbody.innerHTML = html;
}

function openPromoModal(mode, id = null) {
    document.getElementById('promoModal').classList.add('open');
    if (mode === 'edit') {
        document.getElementById('modalTitle').innerText = 'Chỉnh sửa Khuyến Mãi';
        editId = id;
        let promo = PROMOS.find(x => x.id === id);
        if(promo) {
            document.getElementById('promoCode').value = promo.code;
            // Format parse (Mocking value mapping)
            document.getElementById('promoValue').value = promo.discount.replace(/[^0-9]/g,'');
            document.getElementById('promoLimit').value = promo.limit;
            document.getElementById('promoStatus').value = promo.status === 'Đã hết hạn' ? 'Đã khóa' : promo.status;
        }
    } else {
        document.getElementById('modalTitle').innerText = 'Tạo Mã Khuyến Mãi Mới';
        editId = null;
        document.getElementById('promoForm').reset();
    }
}

function closePromoModal() { 
    document.getElementById('promoModal').classList.remove('open'); 
}

function savePromo() {
    let code = document.getElementById('promoCode').value;
    let val = document.getElementById('promoValue').value;
    let type = document.getElementById('promoType').value;
    let limit = document.getElementById('promoLimit').value;
    let status = document.getElementById('promoStatus').value;

    if(!code) return alert("Vui lòng nhập mã code");
    
    let formattedDiscount = type === 'percentage' ? val + '%' : parseInt(val).toLocaleString('vi-VN') + 'đ';

    if (editId) {
        let p = PROMOS.find(x => x.id === editId);
        if(p) {
            p.code = code.toUpperCase();
            p.discount = formattedDiscount;
            p.limit = limit;
            p.status = status;
        }
    } else {
        PROMOS.unshift({
            id: Date.now(),
            code: code.toUpperCase(), 
            discount: formattedDiscount, 
            limit: limit, 
            used: 0, 
            expiry: '31/12/2026', 
            status: status
        });
    }
    
    renderPromos();
    closePromoModal();
}

function deletePromo(id) {
    if(confirm("Bạn có chắc chắn muốn xóa mã khuyến mãi này?")) {
        PROMOS = PROMOS.filter(x => x.id !== id);
        renderPromos();
    }
}