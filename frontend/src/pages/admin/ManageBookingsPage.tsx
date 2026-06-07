import { useState, useEffect, useMemo } from 'react';
import { Calendar as CalendarIcon, List, Search, Plus, X, Loader2, Save, RefreshCw } from 'lucide-react';
import toast from 'react-hot-toast';
import { getAdminBookings, createBookingFromAdmin, updateBookingStatus } from '@/api/bookingApi';
import { getCourts, getCourtTypes } from '@/api/courtApi';
import type { Court } from '@/types/court.types';
import { userApi } from '@/api/userApi';
import type { UserSummary } from '@/api/userApi';
import { timeSlotApi } from '@/api/timeSlotApi';
import type { TimeSlot } from '@/api/timeSlotApi';
import type { Booking } from '@/types/booking.types';

/**
 * Calculate pixel offset from 06:00 baseline.
 * Each 30-min slot = 40px (matching prototype bookings.css).
 */
const timeToTopPos = (timeStr: string): number => {
  if (!timeStr) return 0;
  const [h, m] = timeStr.split(':').map(Number);
  const minsFrom6AM = (h * 60 + m) - (6 * 60);
  return (minsFrom6AM / 30) * 40;
};

/** Returns current date as YYYY-MM-DD using local timezone */
const todayLocal = (): string => {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
};

export default function ManageBookingsPage() {
  const [viewMode, setViewMode] = useState<'list' | 'calendar'>('calendar');
  const [loading, setLoading] = useState(true);

  // ── Data ──
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [courts, setCourts] = useState<Court[]>([]);
  const [courtTypes, setCourtTypes] = useState<{ courtTypeId: number; typeName: string }[]>([]);
  const [users, setUsers] = useState<UserSummary[]>([]);
  const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([]);

  // ── Filters ──
  const [search, setSearch] = useState('');
  const [filterDate, setFilterDate] = useState(todayLocal);
  const [filterCourtType, setFilterCourtType] = useState('all');
  const [filterStatus, setFilterStatus] = useState('all');

  // ── Drawer ──
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [drawerBookingId, setDrawerBookingId] = useState<number | null>(null);
  const [viewingBooking, setViewingBooking] = useState<Booking | null>(null);

  // ── Form ──
  const [userId, setUserId] = useState('');
  const [userSearch, setUserSearch] = useState('');
  const [courtId, setCourtId] = useState('');
  const [slotId, setSlotId] = useState('');
  const [bookingDate, setBookingDate] = useState('');
  const [promotionCode, setPromotionCode] = useState('');
  const [paymentStatus, setPaymentStatus] = useState('Pending');
  const [paymentMethod, setPaymentMethod] = useState('Cash');

  useEffect(() => { loadAllData(); }, []);

  const loadAllData = async () => {
    try {
      setLoading(true);
      const [bData, cData, ctData, uData, tData] = await Promise.all([
        getAdminBookings(),
        getCourts(),
        getCourtTypes(),
        userApi.getAll(),
        timeSlotApi.getAll(),
      ]);
      setBookings(bData);
      setCourts(cData);
      setCourtTypes(ctData);
      setUsers(uData);
      setTimeSlots(tData);
    } catch {
      toast.error('Lỗi khi tải dữ liệu trang đặt sân');
    } finally {
      setLoading(false);
    }
  };

  const resetFilters = () => {
    setFilterDate(todayLocal());
    setFilterCourtType('all');
    setFilterStatus('all');
    setSearch('');
  };

  const openCreateDrawer = () => {
    setDrawerBookingId(null);
    setUserId('');
    setUserSearch('');
    setCourtId(courts.length > 0 ? String(courts[0].courtId) : '');
    setSlotId('');
    setBookingDate(filterDate);
    setPromotionCode('');
    setPaymentStatus('Pending');
    setPaymentMethod('Cash');
    setIsDrawerOpen(true);
  };

  const openViewDrawer = (b: Booking) => {
    setDrawerBookingId(b.bookingId);
    setViewingBooking(b);
    setUserId(String(b.userId));
    setCourtId(String(b.courtId));
    setSlotId(String(b.slotId));
    setBookingDate(b.bookingDate ? b.bookingDate.split('T')[0] : '');
    setIsDrawerOpen(true);
  };

  const handleSaveBooking = async () => {
    if (!userId || !courtId || !slotId || !bookingDate) {
      toast.error('Vui lòng điền đầy đủ thông tin bắt buộc');
      return;
    }
    const selectedSlot = timeSlots.find(t => t.slotId === Number(slotId));
    if (!selectedSlot) { toast.error('Không tìm thấy khung giờ'); return; }

    setSaving(true);
    try {
      await createBookingFromAdmin({
        userId: Number(userId),
        courtId: Number(courtId),
        slotId: Number(slotId),
        bookingDate,
        startTime: selectedSlot.startTime,
        endTime: selectedSlot.endTime,
        promotionCode: promotionCode || undefined,
        paymentStatus,
        paymentMethod,
      });
      toast.success('Tạo đơn đặt sân thành công');
      setIsDrawerOpen(false);
      setBookings(await getAdminBookings());
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Có lỗi xảy ra khi tạo đặt sân');
    } finally {
      setSaving(false);
    }
  };

  const handleUpdateStatus = async (bookingId: number, status: string) => {
    try {
      let payload: { status: string; cancelReason?: string } = { status };
      if (status === 'Cancelled') {
        const reason = window.prompt('Nhập lý do hủy đơn:');
        if (!reason) return;
        payload.cancelReason = reason;
      }
      await updateBookingStatus(bookingId, payload);
      toast.success('Cập nhật trạng thái thành công');
      const updatedList = await getAdminBookings();
      setBookings(updatedList);
      // Cập nhật lại booking đang xem trong Drawer
      const updatedBooking = updatedList.find(b => b.bookingId === bookingId) ?? null;
      setViewingBooking(updatedBooking);
    } catch {
      toast.error('Lỗi khi cập nhật trạng thái');
    }
  };

  // ── Derived data ──
  const filteredBookings = bookings.filter(b => {
    const dStr = b.bookingDate ? b.bookingDate.split('T')[0] : '';
    const matchDate = !filterDate || dStr === filterDate;
    const matchCourtType = filterCourtType === 'all' || String((b as any).courtTypeId) === filterCourtType;
    const matchStatus = filterStatus === 'all' || b.status === filterStatus;
    const matchSearch = !search || (
      b.bookingCode.toLowerCase().includes(search.toLowerCase()) ||
      (b.customerName || '').toLowerCase().includes(search.toLowerCase()) ||
      (b.customerPhone || '').includes(search)
    );
    return matchDate && matchCourtType && matchStatus && matchSearch;
  });

  const filteredCourts = useMemo(() =>
    courts.filter(c => filterCourtType === 'all' || String(c.courtTypeId) === filterCourtType),
    [courts, filterCourtType]
  );

  const filteredUsers = useMemo(() => {
    if (!userSearch) return users.slice(0, 50);
    const lower = userSearch.toLowerCase();
    return users.filter(u =>
      u.fullName.toLowerCase().includes(lower) ||
      u.email.toLowerCase().includes(lower) ||
      (u.phone && u.phone.includes(lower))
    ).slice(0, 50);
  }, [users, userSearch]);

  const formatVND = (amount: number) => amount.toLocaleString('vi-VN') + 'đ';

  /** Badge styles by status */
  const statusBadge = (status: string) => {
    switch (status) {
      case 'Confirmed': return 'bg-blue-500/10 text-blue-400 border border-blue-500/20';
      case 'Completed': return 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20';
      case 'Cancelled': return 'bg-red-500/10 text-red-400 border border-red-500/20';
      default:          return 'bg-amber-500/10 text-amber-400 border border-amber-500/20';
    }
  };

  const statusLabel = (status: string) => ({
    Pending: 'Chờ xác nhận', Confirmed: 'Đã xác nhận',
    Completed: 'Đã hoàn thành', Cancelled: 'Đã hủy',
  }[status] ?? status);

  /** Calendar block color by status */
  const blockColor = (status: string) => {
    switch (status) {
      case 'Confirmed': return 'bg-emerald-500/20 border-emerald-500 text-emerald-300';
      case 'Completed': return 'bg-blue-500/20 border-blue-500 text-blue-300';
      case 'Cancelled': return 'bg-red-500/10 border-red-500 text-red-300 opacity-60';
      default:          return 'bg-amber-500/20 border-amber-500 text-amber-300';
    }
  };

  // ── Render ──
  return (
    <div className="flex-1 h-full flex flex-col overflow-hidden">

      {/* Scrollable page content */}
      <div className="flex-1 flex flex-col overflow-y-auto">
        <div className="p-6 flex flex-col gap-5 min-h-full">

          {/* ── Page Header ── */}
          <div className="flex items-start justify-between gap-4 flex-shrink-0">
            <div>
              <h1 className="text-xl font-bold text-white">Quản lý Đặt Sân</h1>
              <p className="text-sm text-slate-500 mt-0.5">Theo dõi lịch nhận sân, xếp thời gian và quản lý thanh toán.</p>
            </div>
            <button onClick={openCreateDrawer} className="btn-primary flex-shrink-0">
              <Plus className="w-4 h-4" /> Đặt sân mới
            </button>
          </div>

          {/* ── Filter Bar ── */}
          <div className="bg-surface-card border border-surface-border rounded-xl p-4 flex flex-wrap items-end gap-4 flex-shrink-0">
            <div className="flex-1 min-w-[160px]">
              <label className="block text-xs font-medium text-slate-400 mb-1.5">Ngày</label>
              <input type="date" className="input-field w-full" value={filterDate}
                onChange={e => setFilterDate(e.target.value)} />
            </div>
            <div className="flex-1 min-w-[140px]">
              <label className="block text-xs font-medium text-slate-400 mb-1.5">Loại sân</label>
              <select className="input-field w-full" value={filterCourtType} onChange={e => setFilterCourtType(e.target.value)}>
                <option value="all">Tất cả loại sân</option>
                {courtTypes.map(ct => <option key={ct.courtTypeId} value={ct.courtTypeId}>{ct.typeName}</option>)}
              </select>
            </div>
            <div className="flex-1 min-w-[140px]">
              <label className="block text-xs font-medium text-slate-400 mb-1.5">Trạng thái</label>
              <select className="input-field w-full" value={filterStatus} onChange={e => setFilterStatus(e.target.value)}>
                <option value="all">Tất cả</option>
                <option value="Pending">Chờ xác nhận</option>
                <option value="Confirmed">Đã xác nhận</option>
                <option value="Completed">Đã hoàn thành</option>
                <option value="Cancelled">Đã hủy</option>
              </select>
            </div>
            <div className="flex-1 min-w-[180px]">
              <label className="block text-xs font-medium text-slate-400 mb-1.5">Tìm kiếm</label>
              <div className="relative">
                <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
                <input type="text" className="input-field w-full pl-9" placeholder="Mã booking, tên..."
                  value={search} onChange={e => setSearch(e.target.value)} />
              </div>
            </div>
            <button onClick={resetFilters} className="btn-secondary h-10 px-4 flex items-center gap-2 flex-shrink-0">
              <RefreshCw className="w-4 h-4" /> Làm mới
            </button>
          </div>

          {/* ── View Tabs ── */}
          <div className="flex gap-2 border-b border-surface-border pb-3 flex-shrink-0">
            <button
              className={`px-4 py-2 flex items-center gap-2 rounded-md text-sm font-medium transition-colors
                ${viewMode === 'calendar' ? 'bg-primary-500 text-white' : 'text-slate-400 hover:text-white hover:bg-slate-800'}`}
              onClick={() => setViewMode('calendar')}
            >
              <CalendarIcon className="w-4 h-4" /> Lịch sân
            </button>
            <button
              className={`px-4 py-2 flex items-center gap-2 rounded-md text-sm font-medium transition-colors
                ${viewMode === 'list' ? 'bg-primary-500 text-white' : 'text-slate-400 hover:text-white hover:bg-slate-800'}`}
              onClick={() => setViewMode('list')}
            >
              <List className="w-4 h-4" /> Danh sách
            </button>
          </div>

          {/* ── Content ── */}
          {loading ? (
            <div className="flex-1 flex justify-center items-center py-20">
              <Loader2 className="w-8 h-8 text-primary-500 animate-spin" />
            </div>
          ) : viewMode === 'list' ? (

            /* LIST VIEW */
            <div className="rounded-xl border border-surface-border bg-surface-card overflow-auto">
              <table className="w-full text-left">
                <thead>
                  <tr className="border-b border-surface-border bg-slate-800/60 text-xs text-slate-500 uppercase tracking-wider">
                    <th className="py-3 px-4 whitespace-nowrap">Mã Đơn</th>
                    <th className="py-3 px-4 whitespace-nowrap">Khách Hàng</th>
                    <th className="py-3 px-4 whitespace-nowrap">Sân</th>
                    <th className="py-3 px-4 whitespace-nowrap">Thời gian</th>
                    <th className="py-3 px-4 whitespace-nowrap">Tổng tiền</th>
                    <th className="py-3 px-4 whitespace-nowrap">Trạng thái</th>
                    <th className="py-3 px-4 text-center whitespace-nowrap">Thao tác</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-surface-border/60">
                  {filteredBookings.length === 0 && (
                    <tr>
                      <td colSpan={7} className="py-12 text-center text-slate-500">
                        Không tìm thấy kết quả nào
                      </td>
                    </tr>
                  )}
                  {filteredBookings.map(b => (
                    <tr key={b.bookingId} className="hover:bg-slate-800/40 transition-colors">
                      <td className="py-3 px-4 font-semibold text-white">{b.bookingCode}</td>
                      <td className="py-3 px-4">
                        <div className="text-sm font-medium text-slate-200">{b.customerName}</div>
                        <div className="text-xs text-slate-500">{b.customerPhone}</div>
                      </td>
                      <td className="py-3 px-4 text-sm text-primary-400 font-medium">{b.courtName}</td>
                      <td className="py-3 px-4 text-xs text-slate-400">
                        <div>{b.bookingDate?.split('T')[0]}</div>
                        <div>{b.startTime?.slice(0, 5)} – {b.endTime?.slice(0, 5)}</div>
                      </td>
                      <td className="py-3 px-4 font-semibold text-emerald-400 whitespace-nowrap">{formatVND(b.totalAmount)}</td>
                      <td className="py-3 px-4">
                        <span className={`inline-flex px-2 py-1 rounded-full text-xs font-medium ${statusBadge(b.status)}`}>
                          {statusLabel(b.status)}
                        </span>
                      </td>
                      <td className="py-3 px-4 text-center">
                        <button onClick={() => openViewDrawer(b)} className="btn-secondary px-3 py-1.5 text-xs">
                          Xem / Sửa
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

          ) : (

            /* CALENDAR VIEW — khớp đúng cấu trúc prototype bookings.css */
            <div className="rounded-xl border border-surface-border bg-surface-card overflow-auto">
              {/*
                Toàn bộ lịch nằm trong 1 flex container để cuộn đồng bộ ngang + dọc.
                time-axis sticky left, court-header sticky top → giống cố định 2 chiều.
              */}
              <div className="flex" style={{ minWidth: `${60 + filteredCourts.length * 160}px` }}>

                {/* Trục thời gian — sticky left */}
                <div
                  className="flex-shrink-0 border-r border-surface-border bg-slate-900/80"
                  style={{ width: 60, position: 'sticky', left: 0, zIndex: 20 }}
                >
                  {/* Ô góc trống (đồng cao với court-header) */}
                  <div className="h-[40px] border-b border-surface-border" />
                  {/* 32 slot × 40px: 06:00 → 21:30 */}
                  {Array.from({ length: 32 }).map((_, i) => {
                    const h = Math.floor(i / 2) + 6;
                    const m = (i % 2) * 30;
                    return (
                      <div
                        key={i}
                        className="h-[40px] flex items-start justify-end pr-2 pt-1"
                        style={{ borderBottom: m === 0 ? '1px solid rgba(51,65,85,0.8)' : '1px solid rgba(51,65,85,0.3)' }}
                      >
                        {m === 0 && (
                          <span className="text-[11px] text-slate-400 font-medium leading-none">
                            {String(h).padStart(2, '0')}:00
                          </span>
                        )}
                      </div>
                    );
                  })}
                </div>

                {/* Court columns */}
                {filteredCourts.length === 0 ? (
                  <div className="flex-1 flex items-center justify-center py-20 text-slate-500 text-sm">
                    Không có sân nào phù hợp với bộ lọc
                  </div>
                ) : (
                  filteredCourts.map(court => {
                    const courtBookings = filteredBookings.filter(b => b.courtId === court.courtId);
                    return (
                      <div
                        key={court.courtId}
                        className="flex-1 border-r border-surface-border last:border-r-0"
                        style={{ minWidth: 160, position: 'relative' }}
                      >
                        {/* Court header — sticky top */}
                        <div
                          className="h-[40px] border-b border-surface-border bg-slate-800/70 flex items-center justify-center text-xs font-semibold text-slate-200 px-2 text-center"
                          style={{ position: 'sticky', top: 0, zIndex: 10 }}
                        >
                          {court.courtName}
                        </div>

                        {/* Background grid (matching .court-slots-bg from prototype) */}
                        <div
                          style={{
                            position: 'absolute',
                            top: 40,
                            left: 0,
                            right: 0,
                            height: 1280,
                            backgroundImage: 'linear-gradient(to bottom, transparent 39px, rgba(51,65,85,0.5) 40px)',
                            backgroundSize: '100% 40px',
                            pointerEvents: 'none',
                          }}
                        />

                        {/* Container for booking blocks: header(40) + grid(1280) */}
                        <div style={{ position: 'relative', height: 1320 }}>
                          {courtBookings.map(b => {
                            // top = court-header height (40px) + time offset
                            const top = 40 + timeToTopPos(b.startTime || '06:00');
                            const height = Math.max(
                              timeToTopPos(b.endTime || '06:00') - timeToTopPos(b.startTime || '06:00'),
                              28
                            );
                            return (
                              <div
                                key={b.bookingId}
                                onClick={() => openViewDrawer(b)}
                                className={`absolute left-1 right-1 rounded border-l-4 p-1.5 overflow-hidden shadow-md cursor-pointer hover:brightness-125 transition-all duration-150 ${blockColor(b.status)}`}
                                style={{ top, height }}
                              >
                                <div className="font-semibold text-[11px] leading-tight truncate">{b.customerName}</div>
                                {height > 30 && (
                                  <div className="text-[10px] opacity-70 leading-tight mt-0.5">
                                    {b.startTime?.slice(0, 5)} – {b.endTime?.slice(0, 5)}
                                  </div>
                                )}
                              </div>
                            );
                          })}
                        </div>
                      </div>
                    );
                  })
                )}
              </div>
            </div>

          )}
        </div>
      </div>

      {/* ── DRAWER ── */}
      {isDrawerOpen && (
        <div
          className="fixed inset-0 z-50 flex justify-end bg-black/60 backdrop-blur-sm"
          onClick={() => setIsDrawerOpen(false)}
        >
          <div
            className="w-[440px] bg-surface-card border-l border-surface-border shadow-2xl h-full flex flex-col"
            onClick={e => e.stopPropagation()}
          >
            {/* Drawer header */}
            <div className="px-6 py-4 border-b border-surface-border flex items-center justify-between bg-slate-800/60 flex-shrink-0">
              <h2 className="text-base font-bold text-white">
                {drawerBookingId ? `Chi tiết Booking #${drawerBookingId}` : 'Tạo Đặt Sân Mới'}
              </h2>
              <button onClick={() => setIsDrawerOpen(false)} className="text-slate-400 hover:text-red-400 p-1 transition-colors">
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Drawer body */}
            <div className="flex-1 overflow-y-auto p-6 space-y-5">

              {/* Khách hàng */}
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-1.5">Khách hàng *</label>
                {!drawerBookingId && (
                  <input type="text" className="input-field w-full mb-2"
                    placeholder="Tìm tên, SĐT, Email khách..."
                    value={userSearch} onChange={e => setUserSearch(e.target.value)} />
                )}
                <select className="input-field w-full" value={userId}
                  onChange={e => setUserId(e.target.value)} disabled={!!drawerBookingId}>
                  <option value="">-- Chọn Khách Hàng --</option>
                  {filteredUsers.map(u => (
                    <option key={u.userId} value={u.userId}>{u.fullName} — {u.phone || u.email}</option>
                  ))}
                </select>
              </div>

              {/* Sân */}
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-1.5">Chọn Sân *</label>
                <select className="input-field w-full" value={courtId}
                  onChange={e => setCourtId(e.target.value)} disabled={!!drawerBookingId}>
                  <option value="">-- Chọn Sân --</option>
                  {courts.map(c => <option key={c.courtId} value={c.courtId}>{c.courtName}</option>)}
                </select>
              </div>

              {/* Ngày */}
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-1.5">Ngày đặt *</label>
                <input type="date" className="input-field w-full" value={bookingDate}
                  min={todayLocal()}
                  onChange={e => setBookingDate(e.target.value)} disabled={!!drawerBookingId} />
              </div>

              {/* Khung giờ */}
              <div>
                <label className="block text-sm font-medium text-slate-300 mb-1.5">Khung giờ *</label>
                <select className="input-field w-full" value={slotId}
                  onChange={e => setSlotId(e.target.value)} disabled={!!drawerBookingId}>
                  <option value="">-- Chọn Khung Giờ --</option>
                  {timeSlots.map(t => (
                    <option key={t.slotId} value={t.slotId}>
                      {t.slotName} ({t.startTime.slice(0, 5)} – {t.endTime.slice(0, 5)})
                    </option>
                  ))}
                </select>
              </div>

              {/* Extras — chỉ hiện khi tạo mới */}
              {!drawerBookingId && (
                <>
                  <div>
                    <label className="block text-sm font-medium text-slate-300 mb-1.5">Mã Khuyến Mãi</label>
                    <input type="text" className="input-field w-full uppercase"
                      placeholder="Nhập mã (nếu có)" value={promotionCode}
                      onChange={e => setPromotionCode(e.target.value)} />
                  </div>

                  <div className="flex gap-3">
                    <div className="flex-1">
                      <label className="block text-sm font-medium text-slate-300 mb-1.5">Thanh toán</label>
                      <select className="input-field w-full" value={paymentStatus} onChange={e => setPaymentStatus(e.target.value)}>
                        <option value="Pending">Chưa thanh toán</option>
                        <option value="Success">Đã thanh toán</option>
                      </select>
                    </div>
                    <div className="flex-1">
                      <label className="block text-sm font-medium text-slate-300 mb-1.5">Phương thức</label>
                      <select className="input-field w-full" value={paymentMethod} onChange={e => setPaymentMethod(e.target.value)}>
                        <option value="Cash">Tiền mặt</option>
                        <option value="BankTransfer">Chuyển khoản</option>
                      </select>
                    </div>
                  </div>
                </>
              )}

              {/* Action buttons — khi xem booking, chỉ hiện nút hợp lệ theo trạng thái */}
              {drawerBookingId && viewingBooking && (() => {
                const s = viewingBooking.status;
                return (
                  <div className="pt-4 border-t border-surface-border space-y-3">
                    <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Cập nhật trạng thái</p>

                    {/* Trạng thái hiện tại */}
                    <div className={`flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-semibold ${
                      s === 'Confirmed'  ? 'bg-blue-500/10 text-blue-400 border border-blue-500/20' :
                      s === 'Completed'  ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' :
                      s === 'Cancelled'  ? 'bg-red-500/10 text-red-400 border border-red-500/20' :
                      'bg-amber-500/10 text-amber-400 border border-amber-500/20'
                    }`}>
                      <span className="w-2 h-2 rounded-full flex-shrink-0" style={{ backgroundColor: 'currentColor' }} />
                      Hiện tại: {statusLabel(s)}
                    </div>

                    {/* Cancelled — đơn hủy không làm gì được */}
                    {s === 'Cancelled' && (
                      <p className="text-xs text-slate-500 italic">
                        Đơn đã hủy không thể thay đổi trạng thái.
                      </p>
                    )}

                    {/* Pending → có thể Xác nhận hoặc Hủy */}
                    {s === 'Pending' && (
                      <>
                        <button onClick={() => handleUpdateStatus(drawerBookingId, 'Confirmed')}
                          className="btn-primary w-full justify-center" style={{ background: '#2563eb' }}>
                          ✓ Xác nhận đơn
                        </button>
                        <button onClick={() => handleUpdateStatus(drawerBookingId, 'Cancelled')}
                          className="btn-primary w-full justify-center" style={{ background: '#dc2626' }}>
                          ✕ Hủy đơn
                        </button>
                      </>
                    )}

                    {/* Confirmed → có thể Hoàn thành hoặc Hủy */}
                    {s === 'Confirmed' && (
                      <>
                        <button onClick={() => handleUpdateStatus(drawerBookingId, 'Completed')}
                          className="btn-primary w-full justify-center" style={{ background: '#059669' }}>
                          ✓ Đánh dấu hoàn thành
                        </button>
                        <button onClick={() => handleUpdateStatus(drawerBookingId, 'Cancelled')}
                          className="btn-primary w-full justify-center" style={{ background: '#dc2626' }}>
                          ✕ Hủy đơn
                        </button>
                      </>
                    )}

                    {/* Completed → chỉ có thể hoàn tác về Confirmed */}
                    {s === 'Completed' && (
                      <>
                        <p className="text-xs text-slate-500">Đơn đã hoàn thành. Chỉ hoàn tác nếu nhập nhầm.</p>
                        <button onClick={() => handleUpdateStatus(drawerBookingId, 'Confirmed')}
                          className="btn-secondary w-full justify-center">
                          ↩ Hoàn tác về "Đã xác nhận"
                        </button>
                      </>
                    )}
                  </div>
                );
              })()}
            </div>

            {/* Drawer footer — chỉ hiện khi tạo mới */}
            {!drawerBookingId && (
              <div className="p-5 border-t border-surface-border bg-slate-800/60 flex justify-end gap-3 flex-shrink-0">
                <button onClick={() => setIsDrawerOpen(false)} className="btn-secondary">Hủy</button>
                <button onClick={handleSaveBooking} disabled={saving} className="btn-primary min-w-[130px] justify-center">
                  {saving ? <Loader2 className="w-4 h-4 animate-spin" /> : <Save className="w-4 h-4" />}
                  Tạo Đặt Sân
                </button>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
