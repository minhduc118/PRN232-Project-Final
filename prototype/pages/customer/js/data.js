/* ============================================================
   Customer Pages — Mock Data & Helpers
   ============================================================ */

// ── Court Types ──────────────────────────────────────────────
const COURT_TYPES = [
  { courtTypeId: 1, typeName: 'Pickleball',    icon: '🏓', isActive: true },
  { courtTypeId: 2, typeName: 'Cầu lông',      icon: '🏸', isActive: true },
  { courtTypeId: 3, typeName: 'Bóng đá mini',  icon: '⚽', isActive: true },
  { courtTypeId: 4, typeName: 'Tennis',         icon: '🎾', isActive: true },
];

// ── Courts ────────────────────────────────────────────────────
const COURTS = [
  {
    courtId: 1, courtName: 'Sân Pickleball A1', courtCode: 'PCK-A1',
    courtTypeId: 1,
    description: 'Sân pickleball triệt tiêu rung chấn tiêu chuẩn quốc tế, mái che chống nắng, đèn LED chuyên nghiệp ban đêm. Nền sân polymer cao cấp, chống trơn trượt tuyệt đối, phù hợp thi đấu và luyện tập.',
    location: 'Khu A - Tầng 1', address: '123 Nguyễn Huệ, Q.1, TP.HCM',
    imageUrl: '../../assets/images/pickleball.png',
    images: [
      '../../assets/images/pickleball.png',
      '../../assets/images/pickleball.png',
      '../../assets/images/pickleball.png',
    ],
    status: 'Available', openTime: '06:00', closeTime: '22:00',
    pricePerHour: 100000, rating: 4.8, reviewCount: 124,
    amenities: ['💡 Đèn LED ban đêm','🏠 Mái che chống nắng','🚿 Phòng thay đồ','🅿️ Bãi xe miễn phí','❄️ Quạt làm mát','🎥 Camera an ninh','🏓 Cho thuê vợt','🥤 Nước uống'],
    weekendSurcharge: 20,
    featuredBadge: '🔥 Hot',
  },
  {
    courtId: 2, courtName: 'Sân Pickleball A2', courtCode: 'PCK-A2',
    courtTypeId: 1,
    description: 'Sân pickleball ngoài trời thoáng đãng, nền sân polymer cao cấp chống trơn trượt. Ánh sáng đầy đủ chuẩn thi đấu cho cả ngày lẫn đêm.',
    location: 'Khu A - Tầng 1', address: '123 Nguyễn Huệ, Q.1, TP.HCM',
    imageUrl: '../../assets/images/pickleball.png',
    images: [
      '../../assets/images/pickleball.png',
      '../../assets/images/pickleball.png',
    ],
    status: 'Available', openTime: '06:00', closeTime: '22:00',
    pricePerHour: 100000, rating: 4.6, reviewCount: 89,
    amenities: ['💡 Đèn LED ban đêm','🏠 Mái che chống nắng','🅿️ Bãi xe miễn phí','🎥 Camera an ninh','🏓 Cho thuê vợt'],
    weekendSurcharge: 20,
  },
  {
    courtId: 3, courtName: 'Sân Pickleball VIP', courtCode: 'PCK-VIP',
    courtTypeId: 1,
    description: 'Sân VIP có khán đài 50 chỗ ngồi, điều hòa toàn diện, phù hợp các trận thư hùng và sự kiện lớn. Trang bị cao cấp bậc nhất.',
    location: 'Khu B - Tầng 2', address: '123 Nguyễn Huệ, Q.1, TP.HCM',
    imageUrl: '../../assets/images/pickleball.png',
    images: [
      '../../assets/images/pickleball.png',
      '../../assets/images/pickleball.png',
    ],
    status: 'Available', openTime: '06:00', closeTime: '22:00',
    pricePerHour: 150000, rating: 4.9, reviewCount: 57,
    amenities: ['💡 Đèn LED ban đêm','❄️ Điều hòa toàn sân','🏟️ Khán đài 50 chỗ','🚿 Phòng thay đồ VIP','🅿️ Bãi xe miễn phí','🎥 Camera an ninh','🏓 Cho thuê vợt','🥤 Nước uống miễn phí'],
    weekendSurcharge: 30,
    featuredBadge: '🏆 VIP',
  },
  {
    courtId: 4, courtName: 'Sân Cầu Lông B1', courtCode: 'CL-B1',
    courtTypeId: 2,
    description: 'Sân cầu lông thảm gỗ tự nhiên tiêu chuẩn BWF, hệ thống lưới căng chuẩn chỉ. Thích hợp cho dân phủi lẫn chuyên nghiệp.',
    location: 'Khu B - Tầng 1', address: '123 Nguyễn Huệ, Q.1, TP.HCM',
    imageUrl: '../../assets/images/badminton.png',
    images: [
      '../../assets/images/badminton.png',
      '../../assets/images/badminton.png',
    ],
    status: 'Available', openTime: '06:00', closeTime: '22:00',
    pricePerHour: 80000, rating: 4.5, reviewCount: 201,
    amenities: ['💡 Đèn LED ban đêm','🏠 Mái che','🚿 Phòng thay đồ','🅿️ Bãi xe','🏸 Cho thuê vợt cầu lông'],
    weekendSurcharge: 15,
  },
  {
    courtId: 5, courtName: 'Sân Cầu Lông B2', courtCode: 'CL-B2',
    courtTypeId: 2,
    description: 'Sân cầu lông thảm PVC ngoài trời có mái che tiện lợi. Không gian thoáng rộng thích hợp tập luyện kỹ thuật nhóm.',
    location: 'Khu B - Ngoài trời', address: '123 Nguyễn Huệ, Q.1, TP.HCM',
    imageUrl: '../../assets/images/badminton.png',
    images: [
      '../../assets/images/badminton.png',
      '../../assets/images/badminton.png',
    ],
    status: 'Maintenance', openTime: '06:00', closeTime: '20:00',
    pricePerHour: 70000, rating: 4.3, reviewCount: 145,
    amenities: ['💡 Đèn chiếu sáng','🏠 Mái che một phần','🅿️ Bãi xe','🏸 Cho thuê vợt cầu lông'],
    weekendSurcharge: 15,
  },
  {
    courtId: 6, courtName: 'Sân Bóng Đá Mini C1', courtCode: 'BD-C1',
    courtTypeId: 3,
    description: 'Sân phủi 5 người, cỏ nhân tạo nhập khẩu thế hệ mới nhất, dàn đèn cao áp rực rỡ. Bề mặt đàn hồi tốt giúp bảo vệ đầu gối người chơi.',
    location: 'Khu C - Tầng trệt', address: '123 Nguyễn Huệ, Q.1, TP.HCM',
    imageUrl: '../../assets/images/football.png',
    images: [
      '../../assets/images/football.png',
      '../../assets/images/football.png',
    ],
    status: 'Available', openTime: '06:00', closeTime: '22:00',
    pricePerHour: 200000, rating: 4.7, reviewCount: 312,
    amenities: ['💡 Đèn LED cao áp','⚽ Cỏ nhân tạo thế hệ 4','🚿 Phòng thay đồ','🅿️ Bãi xe rộng','🎥 Camera an ninh','⚽ Cho thuê bóng','🥤 Nước uống'],
    weekendSurcharge: 25,
    featuredBadge: '⚡ Đặt nhiều',
  },
  {
    courtId: 7, courtName: 'Sân Tennis C2', courtCode: 'TEN-C2',
    courtTypeId: 4,
    description: 'Sân tennis đất nện xanh chuẩn quốc tế, bề mặt láng mịn, đèn cao áp chiếu sáng xuyên thấu đêm. Đầy đủ dụng cụ cho thuê giá rẻ.',
    location: 'Khu C - Ngoài trời', address: '123 Nguyễn Huệ, Q.1, TP.HCM',
    imageUrl: '../../assets/images/tennis.png',
    images: [
      '../../assets/images/tennis.png',
      '../../assets/images/tennis.png',
    ],
    status: 'Available', openTime: '06:00', closeTime: '22:00',
    pricePerHour: 220000, rating: 4.8, reviewCount: 94,
    amenities: ['💡 Đèn LED cao áp','🎾 Sân tennis chuẩn','🚿 Phòng thay đồ','🅿️ Bãi xe rộng','🎾 Cho thuê vợt & bóng'],
    weekendSurcharge: 25,
    featuredBadge: '✨ Mới',
  },
];

// ── Time Slots ────────────────────────────────────────────────
const TIME_SLOTS = [
  { slotId: 1,  slotName: 'Ca 06:00–07:00', startTime: '06:00', endTime: '07:00', dayType: 'Weekday' },
  { slotId: 2,  slotName: 'Ca 07:00–08:00', startTime: '07:00', endTime: '08:00', dayType: 'Weekday' },
  { slotId: 3,  slotName: 'Ca 08:00–09:00', startTime: '08:00', endTime: '09:00', dayType: 'Weekday' },
  { slotId: 4,  slotName: 'Ca 09:00–10:00', startTime: '09:00', endTime: '10:00', dayType: 'Weekday' },
  { slotId: 5,  slotName: 'Ca 10:00–11:00', startTime: '10:00', endTime: '11:00', dayType: 'Weekday' },
  { slotId: 6,  slotName: 'Ca 11:00–12:00', startTime: '11:00', endTime: '12:00', dayType: 'Weekday' },
  { slotId: 7,  slotName: 'Ca 13:00–14:00', startTime: '13:00', endTime: '14:00', dayType: 'Weekday' },
  { slotId: 8,  slotName: 'Ca 14:00–15:00', startTime: '14:00', endTime: '15:00', dayType: 'Weekday' },
  { slotId: 9,  slotName: 'Ca 15:00–16:00', startTime: '15:00', endTime: '16:00', dayType: 'Weekday' },
  { slotId: 10, slotName: 'Ca 16:00–17:00', startTime: '16:00', endTime: '17:00', dayType: 'Weekday' },
  { slotId: 11, slotName: 'Ca 17:00–18:00', startTime: '17:00', endTime: '18:00', dayType: 'Weekday' },
  { slotId: 12, slotName: 'Ca 18:00–19:00', startTime: '18:00', endTime: '19:00', dayType: 'Weekday' },
  { slotId: 13, slotName: 'Ca 19:00–20:00', startTime: '19:00', endTime: '20:00', dayType: 'Weekday' },
  { slotId: 14, slotName: 'Ca 20:00–21:00', startTime: '20:00', endTime: '21:00', dayType: 'Weekday' },
  { slotId: 15, slotName: 'Ca 21:00–22:00', startTime: '21:00', endTime: '22:00', dayType: 'Weekday' },
];

// ── Promotions ────────────────────────────────────────────────
const PROMOTIONS = [
  {
    promotionId: 1, promoCode: 'WELCOME10', promoName: 'Chào mừng thành viên mới',
    discountType: 'Percentage', discountValue: 10,
    minBookingAmount: 100000, maxDiscountAmount: 50000,
    endDate: '2026-12-31', isActive: true,
  },
  {
    promotionId: 2, promoCode: 'SUMMER25', promoName: 'Ưu đãi mùa hè 25%',
    discountType: 'Percentage', discountValue: 25,
    minBookingAmount: 200000, maxDiscountAmount: 100000,
    endDate: '2026-08-31', isActive: true,
  },
  {
    promotionId: 3, promoCode: 'FLAT50K', promoName: 'Giảm thẳng 50.000đ',
    discountType: 'FixedAmount', discountValue: 50000,
    minBookingAmount: 150000, maxDiscountAmount: 50000,
    endDate: '2026-06-30', isActive: true,
  },
];

// ── Reviews ───────────────────────────────────────────────────
const REVIEWS = [
  { reviewId: 1, courtId: 1, userId: 10, userFullName: 'Nguyễn Văn A', initials: 'N', avatarColor: 'linear-gradient(135deg,#16a34a,#0f766e)', rating: 5, comment: 'Sân rất đẹp, sạch sẽ và thoáng mát. Nhân viên nhiệt tình, hỗ trợ rất tốt. Chắc chắn sẽ quay lại!', createdAt: '2026-05-20T09:00:00Z' },
  { reviewId: 2, courtId: 1, userId: 11, userFullName: 'Trần Thị Bích', initials: 'T', avatarColor: 'linear-gradient(135deg,#2563eb,#7c3aed)', rating: 5, comment: 'Sân Pickleball A1 chất lượng cao, đèn chiếu sáng ban đêm rất tốt. Giá cả hợp lý cho chất lượng như vậy.', createdAt: '2026-05-18T14:30:00Z' },
  { reviewId: 3, courtId: 1, userId: 12, userFullName: 'Lê Minh Khoa', initials: 'L', avatarColor: 'linear-gradient(135deg,#d97706,#dc2626)', rating: 4, comment: 'Sân tốt, vị trí thuận lợi. Có một điểm nhỏ là bãi xe hơi chật vào giờ cao điểm nhưng tổng thể rất hài lòng.', createdAt: '2026-05-15T11:00:00Z' },
  { reviewId: 4, courtId: 1, userId: 13, userFullName: 'Phạm Thanh Hương', initials: 'P', avatarColor: 'linear-gradient(135deg,#0891b2,#059669)', rating: 5, comment: 'Tuyệt vời! Nền sân polymer không trơn, rất an toàn. Mái che giúp chơi thoải mái dù trời nắng. 5 sao!', createdAt: '2026-05-10T16:45:00Z' },
  { reviewId: 5, courtId: 2, userId: 14, userFullName: 'Hoàng Đức Nam', initials: 'H', avatarColor: 'linear-gradient(135deg,#7c3aed,#db2777)', rating: 4, comment: 'Sân A2 ổn, nhưng không bằng A1 về ánh sáng. Dịch vụ cho thuê vợt tiện lợi.', createdAt: '2026-05-08T10:00:00Z' },
  { reviewId: 6, courtId: 3, userId: 15, userFullName: 'Vũ Thị Lan', initials: 'V', avatarColor: 'linear-gradient(135deg,#16a34a,#2563eb)', rating: 5, comment: 'Sân VIP đỉnh thật! Điều hòa mát lạnh, khán đài rộng rãi. Phù hợp tổ chức giải đấu. Nhân viên chuyên nghiệp.', createdAt: '2026-05-05T15:30:00Z' },
  { reviewId: 7, courtId: 4, userId: 16, userFullName: 'Đặng Quốc Bảo', initials: 'Đ', avatarColor: 'linear-gradient(135deg,#d97706,#16a34a)', rating: 5, comment: 'Sân cầu lông nền gỗ rất đẹp và chuẩn. Cảm giác chơi rất sướng, không bị nảy bất thường.', createdAt: '2026-05-02T09:30:00Z' },
  { reviewId: 8, courtId: 6, userId: 17, userFullName: 'Bùi Thị Thu', initials: 'B', avatarColor: 'linear-gradient(135deg,#dc2626,#9333ea)', rating: 5, comment: 'Sân cỏ nhân tạo thế hệ 4 rất tốt, mềm mại và bền. Bóng cho thuê chất lượng. Giá xứng đáng!', createdAt: '2026-04-28T17:00:00Z' },
];

// ── Helper Functions ──────────────────────────────────────────
function formatCurrency(amount) {
  return amount.toLocaleString('vi-VN') + 'đ';
}

function formatDate(dateStr) {
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

function renderStars(rating, max = 5) {
  let html = '';
  for (let i = 1; i <= max; i++) {
    html += `<i class="${i <= Math.round(rating) ? '★' : '☆'}" style="color:${i <= Math.round(rating) ? '#fbbf24' : '#475569'}">★</i>`;
  }
  return html;
}

function renderStarsText(rating, max = 5) {
  let s = '';
  for (let i = 1; i <= max; i++) {
    s += i <= Math.round(rating) ? '★' : '☆';
  }
  return s;
}

function getStatusBadge(status) {
  const map = {
    Available:   { cls: 'badge-green', label: 'Còn trống' },
    Booked:      { cls: 'badge-gray',  label: 'Đã đặt' },
    InUse:       { cls: 'badge-blue',  label: 'Đang dùng' },
    Maintenance: { cls: 'badge-red',   label: 'Bảo trì' },
  };
  const s = map[status] || { cls: 'badge-gray', label: status };
  return `<span class="badge ${s.cls}">${s.label}</span>`;
}

function getTypeBadge(courtTypeId) {
  const t = COURT_TYPES.find(x => x.courtTypeId === courtTypeId);
  if (!t) return '';
  return `<span class="badge badge-blue">${t.icon} ${t.typeName}</span>`;
}

function getTypeName(courtTypeId) {
  const t = COURT_TYPES.find(x => x.courtTypeId === courtTypeId);
  return t ? t.typeName : '';
}

function getTypeIcon(courtTypeId) {
  const t = COURT_TYPES.find(x => x.courtTypeId === courtTypeId);
  return t ? t.icon : '';
}

function getCourtReviews(courtId) {
  return REVIEWS.filter(r => r.courtId === courtId);
}

function getRelativeTime(dateStr) {
  const diff = Date.now() - new Date(dateStr).getTime();
  const days = Math.floor(diff / 86400000);
  if (days < 1) return 'Hôm nay';
  if (days < 7) return `${days} ngày trước`;
  if (days < 30) return `${Math.floor(days/7)} tuần trước`;
  return `${Math.floor(days/30)} tháng trước`;
}

function getPromoLabel(promo) {
  if (promo.discountType === 'Percentage') return `${promo.discountValue}%`;
  return formatCurrency(promo.discountValue);
}

// ── URL Helpers ───────────────────────────────────────────────
function getUrlParam(name) {
  const params = new URLSearchParams(window.location.search);
  return params.get(name);
}

// ── Logo SVG Helper ──────────────────────────────────────────
function getLogoSVG() {
  return `<img src="../../assets/images/logo.png" class="nav-logo-img" alt="SportsCourt" />`;
}

// ── Navbar HTML ───────────────────────────────────────────────
function renderNavbar(activePage = 'home') {
  const links = [
    { id: 'home',   label: 'Trang chủ',  href: 'home.html' },
    { id: 'courts', label: 'Tìm sân',    href: 'courts.html' },
    { id: 'promo',  label: 'Khuyến mãi', href: 'home.html#promos' },
  ];
  const linksHTML = links.map(l =>
    `<a href="${l.href}" class="nav-link ${l.id === activePage ? 'active' : ''}">${l.label}</a>`
  ).join('');

  return `
  <nav class="navbar" role="navigation" aria-label="Main navigation">
    <div class="navbar-inner">
      <a href="home.html" class="nav-logo" style="display:flex; align-items:center; gap:8px">
        <div class="nav-logo-icon" style="display:flex; align-items:center">${getLogoSVG(24)}</div>
        <span style="font-weight:800; background:linear-gradient(90deg, #ffffff 60%, var(--col-accent) 100%); -webkit-background-clip:text; -webkit-text-fill-color:transparent; background-clip:text">SportsCourt</span>
      </a>
      <div class="nav-links">${linksHTML}</div>
      <div class="nav-sep" aria-hidden="true"></div>
      <div class="nav-right">
        <a href="#" class="btn btn-ghost btn-sm" id="btn-login">Đăng nhập</a>
        <a href="#" class="btn btn-primary btn-sm" id="btn-register">Đăng ký</a>
      </div>
      <button class="nav-hamburger" id="hamburger-btn"
        aria-label="Mở menu" aria-expanded="false">
        <span></span><span></span><span></span>
      </button>
    </div>
  </nav>
  <div class="nav-drawer" id="nav-drawer" role="dialog" aria-modal="true">
    <div class="nav-drawer-overlay" id="nav-drawer-overlay"></div>
    <div class="nav-drawer-panel">
      <div class="nav-drawer-head">
        <div class="nav-logo" style="font-size:20px; display:flex; align-items:center; gap:10px">
          <div class="nav-logo-icon" style="width:48px; height:48px; border-radius:12px; display:flex; align-items:center; justify-content:center; overflow:hidden">${getLogoSVG()}</div>
          <span>SportsCourt</span>
        </div>
        <button class="nav-drawer-close" id="nav-drawer-close">✕</button>
      </div>
      ${links.map(l => `<a href="${l.href}" class="nav-link ${l.id === activePage ? 'active' : ''}">${l.label}</a>`).join('')}
      <div class="nav-right" style="flex-direction:column;margin-left:0;margin-top:16px;gap:8px">
        <a href="#" class="btn btn-ghost btn-sm" style="width:100%;justify-content:center">Đăng nhập</a>
        <a href="#" class="btn btn-primary btn-sm" style="width:100%;justify-content:center">Đăng ký</a>
      </div>
    </div>
  </div>`;
}

// ── Footer HTML ───────────────────────────────────────────────
function renderFooter() {
  return `
  <footer class="footer" role="contentinfo">
    <div class="container">
      <div class="footer-grid">
        <div class="footer-brand">
          <div class="logo" style="display:flex; align-items:center; gap:12px">
            <div class="nav-logo-icon" style="display:flex; align-items:center">${getLogoSVG()}</div>
            <span style="font-weight:900; font-size:23px; letter-spacing:-0.5px; background:linear-gradient(90deg, #ffffff 60%, var(--col-accent) 100%); -webkit-background-clip:text; -webkit-text-fill-color:transparent; background-clip:text">SportsCourt</span>
          </div>
          <p>Nền tảng đặt sân thể thao trực tuyến hàng đầu Việt Nam. Nhanh chóng, tiện lợi, minh bạch.</p>
          <div class="footer-social">
            <a href="#" title="Facebook">f</a>
            <a href="#" title="Instagram">📸</a>
            <a href="#" title="YouTube">▶</a>
            <a href="#" title="Zalo">z</a>
          </div>
        </div>
        <div class="footer-col">
          <h4>Dịch vụ</h4>
          <ul>
            <li><a href="courts.html">Tìm sân thể thao</a></li>
            <li><a href="courts.html?type=1">Sân Pickleball</a></li>
            <li><a href="courts.html?type=2">Sân Cầu lông</a></li>
            <li><a href="courts.html?type=3">Sân Bóng đá mini</a></li>
            <li><a href="courts.html?type=4">Sân Tennis</a></li>
          </ul>
        </div>
        <div class="footer-col">
          <h4>Hỗ trợ</h4>
          <ul>
            <li><a href="#">Hướng dẫn đặt sân</a></li>
            <li><a href="#">Chính sách hoàn tiền</a></li>
            <li><a href="#">Điều khoản sử dụng</a></li>
            <li><a href="#">Chính sách bảo mật</a></li>
          </ul>
        </div>
        <div class="footer-col">
          <h4>Liên hệ</h4>
          <ul>
            <li><a href="#">📍 123 Nguyễn Huệ, Q.1, TP.HCM</a></li>
            <li><a href="tel:19001234">📞 1900 1234</a></li>
            <li><a href="mailto:support@sportscourt.vn">✉️ support@sportscourt.vn</a></li>
            <li><a href="#">⏰ 06:00–22:00 mỗi ngày</a></li>
          </ul>
        </div>
      </div>
      <div class="footer-bottom">
        <p>© 2026 SportsCourt. Bản quyền thuộc về SportsCourt Vietnam.</p>
        <p>Xây dựng bởi Team PRN232 — FPT University</p>
      </div>
    </div>
  </footer>`;
}

// ── Init Navbar Events ────────────────────────────────────────
function initNavbar() {
  const hamburger = document.getElementById('hamburger-btn');
  const drawer    = document.getElementById('nav-drawer');
  const overlay   = document.getElementById('nav-drawer-overlay');
  const closeBtn  = document.getElementById('nav-drawer-close');

  const openDrawer = () => {
    drawer.classList.add('open');
    hamburger?.setAttribute('aria-expanded', 'true');
    document.body.style.overflow = 'hidden';
  };
  const closeDrawer = () => {
    drawer.classList.remove('open');
    hamburger?.setAttribute('aria-expanded', 'false');
    document.body.style.overflow = '';
  };

  hamburger?.addEventListener('click', openDrawer);
  overlay?.addEventListener('click', closeDrawer);
  closeBtn?.addEventListener('click', closeDrawer);
}

// ── Shared Card Renderers ─────────────────────────────────────
function courtCardHTML(c, url) {
  const type = COURT_TYPES.find(t => t.courtTypeId === c.courtTypeId);
  const targetUrl = url || `court-detail.html?id=${c.courtId}`;
  
  const statusMap = {
    Available:   { cls: 'badge-success', lbl: 'Còn trống' },
    Maintenance: { cls: 'badge-danger',  lbl: 'Bảo trì' },
    Booked:      { cls: 'badge-gray',    lbl: 'Đã đặt' },
    InUse:       { cls: 'badge-primary', lbl: 'Đang dùng' }
  };
  const st = statusMap[c.status] || { cls: 'badge-gray', lbl: c.status };
  
  // Stars helper
  const starsHTML = Array.from({length: 5}, (_, i) => 
    `<span style="color:${i < Math.round(c.rating) ? '#f59e0b' : '#1f2538'}; font-size:12px">★</span>`
  ).join('');

  const badgeHTML = c.featuredBadge 
    ? `<div class="cc-badge-featured" style="position:absolute; top:10px; left:10px; z-index:6"><span class="badge badge-warn">${c.featuredBadge}</span></div>` 
    : '';
    
  const glowClass = `glow-sport-${c.courtTypeId}`;

  return `
  <article class="court-card ${glowClass}" onclick="location.href='${targetUrl}'" tabindex="0"
    onkeydown="if(event.key==='Enter')location.href='${targetUrl}'">
    <div class="court-card-img">
      <img src="${c.imageUrl}" alt="${c.courtName}" loading="lazy"/>
      <div class="court-card-overlay" aria-hidden="true"></div>
      <div class="cc-badges" style="position:absolute; top:10px; left:${c.featuredBadge ? '72px' : '10px'}; z-index:5"><span class="badge badge-primary">${type ? type.icon + ' ' + type.typeName : ''}</span></div>
      ${badgeHTML}
      <div class="cc-status" style="position:absolute; top:10px; right:10px; z-index:5"><span class="badge ${st.cls}">${st.lbl}</span></div>
    </div>
    <div class="court-card-body">
      <div class="court-card-name-row" style="display:flex; justify-content:space-between; align-items:flex-start; gap:8px; margin-bottom:6px">
        <h4 class="court-card-name" style="font-size:15px; font-weight:700; color:var(--col-text); overflow:hidden; text-overflow:ellipsis; white-space:nowrap">${c.courtName}</h4>
        <span class="court-card-code" style="font-size:10.5px; color:var(--col-subtext); margin-top:2px; font-weight:600; background:rgba(255,255,255,0.04); padding:2px 6px; border-radius:4px">📋 ${c.courtCode}</span>
      </div>
      <div class="court-card-loc" style="display:flex; align-items:center; gap:5px; color:var(--col-subtext); font-size:12.5px; margin-bottom:4px">
        📍 <span>${c.location}</span>
      </div>
      <div class="court-card-hours" style="display:flex; align-items:center; gap:5px; color:var(--col-subtext); font-size:12.5px; margin-bottom:12px">
        ⏰ <span>${c.openTime}–${c.closeTime}</span>
      </div>
      <div class="court-card-divider" style="height:1px; background:var(--col-border); margin-bottom:12px"></div>
      <div class="court-card-footer" style="display:flex; align-items:center; justify-content:space-between">
        <div class="court-card-rating" style="display:flex; align-items:center; gap:5px; font-size:12.5px">
          <div class="cc-stars" style="display:flex; gap:1px">${starsHTML}</div>
          <strong style="color:var(--col-text); font-size:13px">${c.rating}</strong>
          <span style="color:var(--col-subtext); font-size:11.5px">(${c.reviewCount})</span>
        </div>
        <div class="court-card-price" style="font-size:15.5px; font-weight:800; color:var(--col-accent)">
          ${c.pricePerHour.toLocaleString('vi-VN')}đ<small style="font-size:11px; font-weight:400; color:var(--col-subtext)">/giờ</small>
        </div>
      </div>
    </div>
  </article>`;
}

function promoCardHTML(p) {
  const valText = p.discountType === 'Percentage' ? `${p.discountValue}%` : `${p.discountValue.toLocaleString('vi-VN')}đ`;
  const expDate = new Date(p.endDate).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  
  return `
  <div class="promo-card">
    <div class="promo-value">${valText}</div>
    <div class="promo-name">${p.promoName}</div>
    <div class="promo-desc">Đơn đặt sân từ ${p.minBookingAmount.toLocaleString('vi-VN')}đ</div>
    <div class="promo-code-row">
      <span class="promo-code" id="pc-${p.promotionId}">${p.promoCode}</span>
      <button class="promo-copy" onclick="copyPromoCode('${p.promoCode}', '${p.promotionId}')">📋 Copy</button>
    </div>
    <div class="promo-exp">⏳ HSD: ${expDate}</div>
  </div>`;
}
