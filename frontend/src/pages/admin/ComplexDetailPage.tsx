import { useState, useEffect, useMemo, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Plus, Edit2, Trash2, MapPin, Clock, DollarSign, Users, X, Save,
  Loader2, AlertTriangle, ChevronLeft, Building2, Search, Image as ImageIcon,
  CheckCircle, Wrench, Ban, Circle, Phone, Mail, UserCheck,
  History, CalendarDays, Filter, ChevronLeft as PrevIcon, ChevronRight as NextIcon,
} from 'lucide-react';
import toast from 'react-hot-toast';
import {
  getComplexById, getCourts, createCourt, updateCourt, deleteCourt,
  getCourtTypes, getManagerById, getBookingsByComplexId,
} from '@/api/courtApi';
import type {
  CourtComplex, Court, CourtFormData, CourtStatus, ManagerUser, CourtBookingRecord,
} from '@/types/court.types';

// ─────────────────────────────────────────────
// Helpers
// ─────────────────────────────────────────────
function fmtPrice(n: number) {
  return new Intl.NumberFormat('vi-VN').format(n) + '₫';
}
function fmtDate(s: string) {
  return new Date(s).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

const STATUS_MAP: Record<CourtStatus, { label: string; color: string; icon: React.ReactNode }> = {
  Available:   { label: 'Hoạt động',       color: 'text-emerald-400 bg-emerald-400/10 border-emerald-400/30', icon: <CheckCircle className="w-3 h-3" /> },
  Booked:      { label: 'Đã đặt',          color: 'text-blue-400 bg-blue-400/10 border-blue-400/30',         icon: <Circle className="w-3 h-3 fill-current" /> },
  InUse:       { label: 'Đang sử dụng',    color: 'text-violet-400 bg-violet-400/10 border-violet-400/30',   icon: <Circle className="w-3 h-3 fill-current" /> },
  Maintenance: { label: 'Bảo trì',         color: 'text-amber-400 bg-amber-400/10 border-amber-400/30',      icon: <Wrench className="w-3 h-3" /> },
  Inactive:    { label: 'Ngưng hoạt động', color: 'text-slate-400 bg-slate-400/10 border-slate-400/30',      icon: <Ban className="w-3 h-3" /> },
};

const BOOKING_STATUS_MAP: Record<string, { label: string; color: string }> = {
  Pending:   { label: 'Chờ xác nhận', color: 'text-amber-400 bg-amber-400/10 border-amber-400/30' },
  Confirmed: { label: 'Đã xác nhận',  color: 'text-emerald-400 bg-emerald-400/10 border-emerald-400/30' },
  Completed: { label: 'Hoàn thành',   color: 'text-blue-400 bg-blue-400/10 border-blue-400/30' },
  Cancelled: { label: 'Đã hủy',       color: 'text-red-400 bg-red-400/10 border-red-400/30' },
  NoShow:    { label: 'Không đến',    color: 'text-slate-400 bg-slate-400/10 border-slate-400/30' },
};

function StatusBadge({ status }: { status: CourtStatus }) {
  const s = STATUS_MAP[status] ?? STATUS_MAP.Inactive;
  return (
    <span className={`inline-flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-[11px] font-semibold ${s.color}`}>
      {s.icon} {s.label}
    </span>
  );
}

function BookingStatusBadge({ status }: { status: string }) {
  const s = BOOKING_STATUS_MAP[status] ?? { label: status, color: 'text-slate-400 bg-slate-400/10 border-slate-400/30' };
  return (
    <span className={`inline-flex items-center rounded-full border px-2.5 py-0.5 text-[11px] font-semibold ${s.color}`}>
      {s.label}
    </span>
  );
}

// ─────────────────────────────────────────────
// Manager Card — gọi API theo managerId
// ─────────────────────────────────────────────
function ManagerCard({ manager }: { manager: ManagerUser }) {
  return (
    <div className="flex items-center gap-3 rounded-xl border border-primary-500/20 bg-primary-500/5 px-4 py-3">
      <img
        src={manager.avatarUrl ?? `https://api.dicebear.com/8.x/avataaars/svg?seed=${manager.userId}`}
        alt={manager.fullName}
        className="w-10 h-10 rounded-full bg-slate-700 flex-shrink-0 border border-surface-border"
      />
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 flex-wrap">
          <p className="text-sm font-semibold text-white">{manager.fullName}</p>
          <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-semibold bg-primary-600/20 text-primary-400 border border-primary-600/30">
            <UserCheck className="w-2.5 h-2.5" /> Manager #{manager.userId}
          </span>
          {!manager.isActive && (
            <span className="px-2 py-0.5 rounded-full text-[10px] font-semibold bg-red-500/10 text-red-400 border border-red-500/20">
              Không hoạt động
            </span>
          )}
        </div>
        <div className="flex flex-wrap gap-x-4 gap-y-0.5 mt-1">
          <span className="flex items-center gap-1 text-xs text-slate-400">
            <Mail className="w-3 h-3 text-slate-600" />{manager.email}
          </span>
          {manager.phone && (
            <span className="flex items-center gap-1 text-xs text-slate-400">
              <Phone className="w-3 h-3 text-slate-600" />{manager.phone}
            </span>
          )}
        </div>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────
// Field wrapper
// ─────────────────────────────────────────────
function Field({ label, icon, children }: { label: string; icon?: React.ReactNode; children: React.ReactNode }) {
  return (
    <div>
      <label className="flex items-center gap-1.5 text-xs font-medium text-slate-400 mb-1.5">
        {icon} {label}
      </label>
      {children}
    </div>
  );
}

// ─────────────────────────────────────────────
// Modal: Thêm / Sửa Sân
// ─────────────────────────────────────────────
interface CourtModalProps {
  mode: 'create' | 'edit';
  initial?: Court | null;
  complexId: number;
  courtTypes: { courtTypeId: number; typeName: string }[];
  onClose: () => void;
  onSave: (data: CourtFormData, courtId?: number) => Promise<void>;
}

function CourtModal({ mode, initial, complexId, courtTypes, onClose, onSave }: CourtModalProps) {
  const [form, setForm] = useState<CourtFormData>({
    courtName:    initial?.courtName    ?? '',
    courtCode:    initial?.courtCode    ?? '',
    courtTypeId:  initial?.courtTypeId  ?? courtTypes[0]?.courtTypeId ?? 1,
    complexId,
    description:  initial?.description  ?? '',
    location:     initial?.location     ?? '',
    capacity:     initial?.capacity     ?? 4,
    surface:      initial?.surface      ?? '',
    imageUrl:     initial?.imageUrl     ?? '',
    status:       initial?.status       ?? 'Available',
    openTime:     initial?.openTime     ?? '06:00',
    closeTime:    initial?.closeTime    ?? '22:00',
    pricePerHour: initial?.pricePerHour ?? 100000,
    courtSize:    initial?.courtSize    ?? 'Tiêu chuẩn',
    imageUrls:    initial?.imageUrls    ?? [],
  });
  const [saving, setSaving] = useState(false);
  const [tab, setTab] = useState<'info' | 'schedule'>('info');

  const set = <K extends keyof CourtFormData>(key: K, val: CourtFormData[K]) =>
    setForm((prev) => ({ ...prev, [key]: val }));

  const handleSubmit = async () => {
    if (!form.courtName.trim() || !form.courtCode.trim()) {
      toast.error('Tên sân và mã sân là bắt buộc!'); return;
    }
    if (form.closeTime <= form.openTime) {
      toast.error('Giờ đóng cửa phải sau giờ mở cửa!'); return;
    }
    setSaving(true);
    try { await onSave(form, initial?.courtId); onClose(); }
    finally { setSaving(false); }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={onClose} />
      <div className="relative bg-surface-card border border-surface-border rounded-2xl w-full max-w-2xl shadow-2xl animate-fade-in max-h-[90vh] flex flex-col">
        <div className="flex items-center justify-between px-6 py-4 border-b border-surface-border flex-shrink-0">
          <h2 className="text-base font-semibold text-white">
            {mode === 'create' ? 'Thêm sân mới' : `Sửa sân — ${initial?.courtName}`}
          </h2>
          <button onClick={onClose} className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors">
            <X className="w-4 h-4" />
          </button>
        </div>

        <div className="flex gap-1 px-6 pt-4 flex-shrink-0">
          {(['info', 'schedule'] as const).map((t) => (
            <button key={t} onClick={() => setTab(t)}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                tab === t ? 'bg-primary-600/15 text-primary-400 border border-primary-600/30' : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800'
              }`}
            >
              {t === 'info' ? 'Thông tin cơ bản' : 'Lịch & Giá'}
            </button>
          ))}
        </div>

        <div className="px-6 py-5 overflow-y-auto flex-1">
          {tab === 'info' ? (
            <div className="grid grid-cols-2 gap-4">
              <Field label="Tên sân *">
                <input className="input-field" placeholder="VD: Sân Pickleball A1" value={form.courtName} onChange={(e) => set('courtName', e.target.value)} />
              </Field>
              <Field label="Mã sân *">
                <input className="input-field" placeholder="VD: PCK-A1" value={form.courtCode} onChange={(e) => set('courtCode', e.target.value.toUpperCase())} />
              </Field>
              <Field label="Loại sân *">
                <select className="input-field" value={form.courtTypeId} onChange={(e) => set('courtTypeId', Number(e.target.value))}>
                  {courtTypes.map((t) => <option key={t.courtTypeId} value={t.courtTypeId}>{t.typeName}</option>)}
                </select>
              </Field>
              <Field label="Trạng thái *">
                <select className="input-field" value={form.status} onChange={(e) => set('status', e.target.value as CourtStatus)}>
                  <option value="Available">Hoạt động</option>
                  <option value="Maintenance">Bảo trì</option>
                  <option value="Inactive">Ngưng hoạt động</option>
                </select>
              </Field>
              <Field label="Vị trí *">
                <input className="input-field" placeholder="VD: Khu A - Tầng 1" value={form.location} onChange={(e) => set('location', e.target.value)} />
              </Field>
              <Field label="Sức chứa (người)">
                <input type="number" className="input-field" min={1} max={100} value={form.capacity} onChange={(e) => set('capacity', Number(e.target.value))} />
              </Field>
              <Field label="Bề mặt sân">
                <input className="input-field" placeholder="VD: Polymer, Gỗ, Cỏ nhân tạo" value={form.surface} onChange={(e) => set('surface', e.target.value)} />
              </Field>
              <div className="col-span-2">
                <Field label="Quy mô / Kích thước">
                  <input className="input-field" placeholder="VD: Tiêu chuẩn, Sân đơn, Sân đôi" value={form.courtSize ?? ''} onChange={(e) => set('courtSize', e.target.value)} />
                </Field>
              </div>
              <div className="col-span-2">
                <Field label="URL ảnh đại diện" icon={<ImageIcon className="w-4 h-4" />}>
                  <input className="input-field" placeholder="https://..." value={form.imageUrl} onChange={(e) => set('imageUrl', e.target.value)} />
                </Field>
              </div>
              <div className="col-span-2">
                <Field label="Album ảnh phụ (cách nhau bằng xuống dòng)" icon={<ImageIcon className="w-4 h-4" />}>
                  <textarea className="input-field resize-none" rows={3}
                    placeholder="https://image1.jpg&#10;https://image2.jpg"
                    value={form.imageUrls?.join('\n') ?? ''}
                    onChange={(e) => { const urls = e.target.value.split(/[\n,]+/).map((u) => u.trim()).filter(Boolean); set('imageUrls', urls); }}
                  />
                </Field>
              </div>
              <div className="col-span-2">
                <Field label="Mô tả">
                  <textarea className="input-field resize-none" rows={2} placeholder="Mô tả tiện ích, quy cách sân..." value={form.description} onChange={(e) => set('description', e.target.value)} />
                </Field>
              </div>
            </div>
          ) : (
            <div className="space-y-5">
              <div className="grid grid-cols-2 gap-4">
                <Field label="Giờ mở cửa *" icon={<Clock className="w-4 h-4" />}>
                  <input type="time" className="input-field" value={form.openTime} onChange={(e) => set('openTime', e.target.value)} />
                </Field>
                <Field label="Giờ đóng cửa *" icon={<Clock className="w-4 h-4" />}>
                  <input type="time" className="input-field" value={form.closeTime} onChange={(e) => set('closeTime', e.target.value)} />
                </Field>
              </div>
              <Field label="Giá cơ bản / giờ (VND) *" icon={<DollarSign className="w-4 h-4" />}>
                <input type="number" className="input-field" min={10000} step={5000} value={form.pricePerHour} onChange={(e) => set('pricePerHour', Number(e.target.value))} />
                <p className="mt-1 text-xs text-slate-500">Hiện tại: <span className="text-primary-400 font-medium">{fmtPrice(form.pricePerHour)}</span>/giờ</p>
              </Field>
              <div className="rounded-xl border border-surface-border bg-slate-800/30 p-4">
                <p className="text-xs text-slate-500 mb-3">💡 Bảng giá tham khảo theo khung giờ</p>
                <table className="w-full text-sm">
                  <thead><tr className="border-b border-surface-border text-xs text-slate-500 font-medium"><th className="text-left py-2">Khung giờ</th><th className="text-right py-2">Ngày thường</th><th className="text-right py-2">Cuối tuần</th></tr></thead>
                  <tbody className="text-slate-300">
                    <tr className="border-b border-surface-border/50"><td className="py-2">06:00 – 12:00</td><td className="text-right">{fmtPrice(form.pricePerHour)}</td><td className="text-right">{fmtPrice(Math.round(form.pricePerHour * 1.2))}</td></tr>
                    <tr className="border-b border-surface-border/50"><td className="py-2">12:00 – 17:00</td><td className="text-right">{fmtPrice(form.pricePerHour)}</td><td className="text-right">{fmtPrice(Math.round(form.pricePerHour * 1.2))}</td></tr>
                    <tr><td className="py-2">17:00 – {form.closeTime}</td><td className="text-right text-amber-400">{fmtPrice(Math.round(form.pricePerHour * 1.5))}</td><td className="text-right text-amber-400">{fmtPrice(Math.round(form.pricePerHour * 1.8))}</td></tr>
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>

        <div className="flex justify-end gap-3 px-6 py-4 border-t border-surface-border flex-shrink-0">
          <button onClick={onClose} className="btn-secondary">Hủy</button>
          <button onClick={handleSubmit} disabled={saving} className="btn-primary">
            {saving ? <Loader2 className="w-4 h-4 animate-spin" /> : <Save className="w-4 h-4" />}
            {mode === 'create' ? 'Thêm sân' : 'Lưu thay đổi'}
          </button>
        </div>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────
// Modal: Xác nhận xóa
// ─────────────────────────────────────────────
function DeleteModal({ title, message, onClose, onConfirm }: {
  title: string; message: string; onClose: () => void; onConfirm: () => Promise<void>;
}) {
  const [deleting, setDeleting] = useState(false);
  const handle = async () => { setDeleting(true); try { await onConfirm(); onClose(); } finally { setDeleting(false); } };
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={onClose} />
      <div className="relative bg-surface-card border border-surface-border rounded-2xl w-full max-w-sm shadow-2xl p-6">
        <div className="flex items-center gap-3 mb-4">
          <div className="w-10 h-10 rounded-full bg-red-500/10 border border-red-500/30 flex items-center justify-center flex-shrink-0">
            <AlertTriangle className="w-5 h-5 text-red-400" />
          </div>
          <h3 className="text-base font-semibold text-white">{title}</h3>
        </div>
        <p className="text-sm text-slate-400 mb-6">{message}</p>
        <div className="flex justify-end gap-3">
          <button onClick={onClose} className="btn-secondary">Hủy</button>
          <button onClick={handle} disabled={deleting} className="btn-danger">
            {deleting ? <Loader2 className="w-4 h-4 animate-spin" /> : <Trash2 className="w-4 h-4" />} Xóa
          </button>
        </div>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────
// Booking History Tab
// ─────────────────────────────────────────────
const HISTORY_PAGE_SIZE = 10;

function BookingHistoryTab({ complexId, courts }: {
  complexId: number;
  courts: Court[];
}) {
  const [bookings, setBookings]       = useState<CourtBookingRecord[]>([]);
  const [loading, setLoading]         = useState(true);
  const [filterCourtId, setFilterCourtId] = useState('');
  const [filterStatus, setFilterStatus]   = useState('');
  const [dateFrom, setDateFrom]           = useState('');
  const [dateTo, setDateTo]               = useState('');
  const [page, setPage]               = useState(1);

  const fetchBookings = useCallback(async () => {
    setLoading(true);
    try {
      const data = await getBookingsByComplexId(complexId, {
        courtId: filterCourtId ? Number(filterCourtId) : undefined,
        status:  filterStatus || undefined,
        dateFrom: dateFrom || undefined,
        dateTo:   dateTo   || undefined,
      });
      setBookings(data);
      setPage(1);
    } catch { toast.error('Không thể tải lịch sử thuê.'); }
    finally { setLoading(false); }
  }, [complexId, filterCourtId, filterStatus, dateFrom, dateTo]);

  useEffect(() => { fetchBookings(); }, [fetchBookings]);

  const totalPages = Math.ceil(bookings.length / HISTORY_PAGE_SIZE);
  const paged = bookings.slice((page - 1) * HISTORY_PAGE_SIZE, page * HISTORY_PAGE_SIZE);

  // KPI
  const kpi = useMemo(() => ({
    total:     bookings.length,
    confirmed: bookings.filter((b) => b.status === 'Confirmed' || b.status === 'Completed').length,
    cancelled: bookings.filter((b) => b.status === 'Cancelled').length,
    revenue:   bookings.filter((b) => b.status !== 'Cancelled').reduce((s, b) => s + b.totalAmount, 0),
  }), [bookings]);

  return (
    <div className="space-y-5">
      {/* Filter bar */}
      <div className="flex flex-wrap items-end gap-3 p-4 rounded-xl border border-surface-border bg-slate-800/30">
        <div className="flex flex-col gap-1">
          <span className="text-[10px] text-slate-500 uppercase font-semibold">Sân</span>
          <select className="input-field py-2 min-w-[160px]" value={filterCourtId} onChange={(e) => setFilterCourtId(e.target.value)}>
            <option value="">Tất cả sân</option>
            {courts.map((c) => <option key={c.courtId} value={c.courtId}>{c.courtName}</option>)}
          </select>
        </div>
        <div className="flex flex-col gap-1">
          <span className="text-[10px] text-slate-500 uppercase font-semibold">Trạng thái</span>
          <select className="input-field py-2 min-w-[140px]" value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)}>
            <option value="">Tất cả</option>
            <option value="Pending">Chờ xác nhận</option>
            <option value="Confirmed">Đã xác nhận</option>
            <option value="Completed">Hoàn thành</option>
            <option value="Cancelled">Đã hủy</option>
            <option value="NoShow">Không đến</option>
          </select>
        </div>
        <div className="flex flex-col gap-1">
          <span className="text-[10px] text-slate-500 uppercase font-semibold">Từ ngày</span>
          <input type="date" className="input-field py-2" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} />
        </div>
        <div className="flex flex-col gap-1">
          <span className="text-[10px] text-slate-500 uppercase font-semibold">Đến ngày</span>
          <input type="date" className="input-field py-2" value={dateTo} onChange={(e) => setDateTo(e.target.value)} />
        </div>
        {(filterCourtId || filterStatus || dateFrom || dateTo) && (
          <button
            onClick={() => { setFilterCourtId(''); setFilterStatus(''); setDateFrom(''); setDateTo(''); }}
            className="btn-ghost text-xs py-2 self-end"
          >
            <X className="w-3.5 h-3.5" /> Xóa lọc
          </button>
        )}
      </div>

      {/* KPI row */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
        {[
          { label: 'Tổng booking',   value: kpi.total,     color: 'text-slate-100' },
          { label: 'Xác nhận/HT',    value: kpi.confirmed, color: 'text-emerald-400' },
          { label: 'Đã hủy',         value: kpi.cancelled, color: 'text-red-400' },
          { label: 'Doanh thu',      value: fmtPrice(kpi.revenue), color: 'text-primary-400' },
        ].map((k) => (
          <div key={k.label} className="rounded-xl border border-surface-border bg-surface-card p-3 text-center">
            <p className="text-xs text-slate-500 mb-1">{k.label}</p>
            <p className={`text-lg font-bold ${k.color}`}>{k.value}</p>
          </div>
        ))}
      </div>

      {/* Table */}
      {loading ? (
        <div className="flex items-center justify-center py-14">
          <Loader2 className="w-7 h-7 text-primary-500 animate-spin" />
        </div>
      ) : bookings.length === 0 ? (
        <div className="rounded-xl border border-dashed border-slate-700 bg-slate-800/20 py-14 text-center">
          <History className="w-10 h-10 text-slate-600 mx-auto mb-3" />
          <h3 className="text-sm font-semibold text-slate-400">Chưa có lịch sử thuê sân</h3>
          <p className="text-xs text-slate-600 mt-1">Thử thay đổi bộ lọc hoặc khoảng ngày</p>
        </div>
      ) : (
        <>
          <div className="rounded-xl border border-surface-border bg-surface-card overflow-hidden">
            <table className="w-full">
              <thead>
                <tr className="border-b border-surface-border bg-slate-800/60 text-xs text-slate-500 font-semibold uppercase tracking-wider">
                  <th className="py-3 pl-5 pr-4 text-left">Mã booking</th>
                  <th className="py-3 px-4 text-left">Sân</th>
                  <th className="py-3 px-4 text-left">Khách hàng</th>
                  <th className="py-3 px-4 text-left">Ngày thuê</th>
                  <th className="py-3 px-4 text-left">Giờ</th>
                  <th className="py-3 px-4 text-right">Tổng tiền</th>
                  <th className="py-3 px-4 text-left">Thanh toán</th>
                  <th className="py-3 pl-4 pr-5 text-left">Trạng thái</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-surface-border/60">
                {paged.map((b) => (
                  <tr key={b.bookingId} className="hover:bg-slate-800/40 transition-colors">
                    <td className="py-3 pl-5 pr-4">
                      <span className="text-xs font-mono text-primary-400">{b.bookingCode}</span>
                    </td>
                    <td className="py-3 px-4">
                      <span className="text-sm text-slate-300">{b.courtName}</span>
                    </td>
                    <td className="py-3 px-4">
                      <p className="text-sm text-slate-200 font-medium">{b.customerName ?? `User #${b.userId}`}</p>
                      {b.customerPhone && <p className="text-xs text-slate-500">{b.customerPhone}</p>}
                    </td>
                    <td className="py-3 px-4">
                      <div className="flex items-center gap-1.5 text-sm text-slate-400">
                        <CalendarDays className="w-3 h-3 text-slate-600 flex-shrink-0" />
                        {fmtDate(b.bookingDate)}
                      </div>
                    </td>
                    <td className="py-3 px-4">
                      <div className="flex items-center gap-1.5 text-sm text-slate-400">
                        <Clock className="w-3 h-3 text-slate-600 flex-shrink-0" />
                        {b.startTime} – {b.endTime}
                      </div>
                    </td>
                    <td className="py-3 px-4 text-right">
                      <span className="text-sm font-semibold text-primary-400">{fmtPrice(b.totalAmount)}</span>
                    </td>
                    <td className="py-3 px-4">
                      <span className="text-xs text-slate-400">{b.paymentMethod ?? '—'}</span>
                    </td>
                    <td className="py-3 pl-4 pr-5">
                      <BookingStatusBadge status={b.status} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            <div className="px-5 py-3 border-t border-surface-border/50 bg-slate-800/20 flex items-center justify-between">
              <p className="text-xs text-slate-500">
                Hiển thị <span className="text-slate-300 font-medium">{paged.length}</span> / {bookings.length} booking
              </p>
              {totalPages > 1 && (
                <div className="flex items-center gap-1.5">
                  <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}
                    className="p-1.5 rounded-lg border border-slate-700 text-slate-400 hover:text-white disabled:opacity-30 transition-colors">
                    <PrevIcon className="w-3.5 h-3.5" />
                  </button>
                  <span className="text-xs text-slate-400 px-2">{page} / {totalPages}</span>
                  <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages}
                    className="p-1.5 rounded-lg border border-slate-700 text-slate-400 hover:text-white disabled:opacity-30 transition-colors">
                    <NextIcon className="w-3.5 h-3.5" />
                  </button>
                </div>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}

// ─────────────────────────────────────────────
// Main: Chi tiết Tổ hợp sân
// ─────────────────────────────────────────────
export default function ComplexDetailPage() {
  const { complexId } = useParams<{ complexId: string }>();
  const navigate = useNavigate();
  const cid = Number(complexId);

  const [complex, setComplex]   = useState<CourtComplex | null>(null);
  const [manager, setManager]   = useState<ManagerUser | null>(null);
  const [courts, setCourts]     = useState<Court[]>([]);
  const [courtTypes, setCourtTypes] = useState<{ courtTypeId: number; typeName: string; isActive: boolean }[]>([]);
  const [loading, setLoading]   = useState(true);

  const [activeTab, setActiveTab] = useState<'courts' | 'history'>('courts');

  const [search, setSearch]           = useState('');
  const [filterStatus, setFilterStatus] = useState('');
  const [filterType, setFilterType]   = useState('');

  type ModalType =
    | { kind: 'create' }
    | { kind: 'edit'; court: Court }
    | { kind: 'delete'; court: Court }
    | null;
  const [modal, setModal] = useState<ModalType>(null);

  const [viewerImages, setViewerImages] = useState<string[] | null>(null);
  const [viewerIndex, setViewerIndex]   = useState(0);

  useEffect(() => {
    (async () => {
      setLoading(true);
      try {
        const [cx, co, ct] = await Promise.all([
          getComplexById(cid),
          getCourts({ complexId: cid }),
          getCourtTypes(),
        ]);
        setComplex(cx);
        setCourts(co);
        setCourtTypes(ct);

        // Fetch manager riêng nếu có managerId
        if (cx?.managerId) {
          const m = await getManagerById(cx.managerId);
          setManager(m);
        }
      } catch { toast.error('Không thể tải dữ liệu.'); }
      finally { setLoading(false); }
    })();
  }, [cid]);

  const filtered = useMemo(() => {
    const q = search.toLowerCase();
    return courts.filter((c) => {
      const mQ = !q || c.courtName.toLowerCase().includes(q) || c.courtCode.toLowerCase().includes(q) || c.location.toLowerCase().includes(q);
      const mS = !filterStatus || c.status === filterStatus;
      const mT = !filterType   || c.courtTypeId === Number(filterType);
      return mQ && mS && mT;
    });
  }, [courts, search, filterStatus, filterType]);

  const stats = useMemo(() => ({
    total:       courts.length,
    active:      courts.filter((c) => c.status === 'Available').length,
    maintenance: courts.filter((c) => c.status === 'Maintenance').length,
    inactive:    courts.filter((c) => c.status === 'Inactive').length,
  }), [courts]);

  const handleSaveCourt = useCallback(async (data: CourtFormData, courtId?: number) => {
    if (courtId) {
      await updateCourt(courtId, data);
      setCourts((prev) => prev.map((c) => c.courtId === courtId
        ? { ...c, ...data, courtType: courtTypes.find((t) => t.courtTypeId === data.courtTypeId) }
        : c
      ));
      toast.success('Cập nhật sân thành công!');
    } else {
      const created = await createCourt(data);
      setCourts((prev) => [...prev, {
        ...created, ...data,
        courtId: created.courtId || Date.now(),
        courtType: courtTypes.find((t) => t.courtTypeId === data.courtTypeId),
        rating: 0, reviewCount: 0, createdAt: new Date().toISOString(),
      }]);
      toast.success('Thêm sân mới thành công!');
    }
  }, [courtTypes]);

  const handleDeleteCourt = useCallback(async (court: Court) => {
    await deleteCourt(court.courtId);
    setCourts((prev) => prev.filter((c) => c.courtId !== court.courtId));
    toast.success(`Đã xóa sân ${court.courtName}!`);
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full py-32">
        <div className="text-center space-y-3">
          <Loader2 className="w-8 h-8 text-primary-500 animate-spin mx-auto" />
          <p className="text-sm text-slate-500">Đang tải dữ liệu...</p>
        </div>
      </div>
    );
  }

  if (!complex) {
    return (
      <div className="flex flex-col items-center justify-center h-full py-32 text-center">
        <Building2 className="w-12 h-12 text-slate-600 mb-3" />
        <h2 className="text-base font-semibold text-slate-400 mb-2">Không tìm thấy tổ hợp sân</h2>
        <button onClick={() => navigate('/admin/courts')} className="btn-secondary mt-4">
          <ChevronLeft className="w-4 h-4" /> Quay về danh sách
        </button>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6 min-h-full">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-sm text-slate-500">
        <button onClick={() => navigate('/admin/courts')} className="flex items-center gap-1.5 hover:text-slate-300 transition-colors">
          <ChevronLeft className="w-4 h-4" /> Quản lý sân
        </button>
        <span>/</span>
        <span className="text-slate-300 font-medium">{complex.complexName}</span>
      </div>

      {/* Complex Info Card */}
      <div className="rounded-xl border border-surface-border bg-surface-card overflow-hidden">
        <div className="flex items-start gap-5 p-5">
          {/* Ảnh */}
          <div className="w-16 h-16 rounded-xl overflow-hidden flex-shrink-0 bg-indigo-500/10 border border-indigo-500/20 flex items-center justify-center">
            {complex.imageUrl
              ? <img src={complex.imageUrl} alt={complex.complexName} className="w-full h-full object-cover" />
              : <Building2 className="w-7 h-7 text-indigo-400" />}
          </div>

          {/* Info */}
          <div className="flex-1 min-w-0 space-y-2">
            <h1 className="text-lg font-bold text-white">{complex.complexName}</h1>

            <div className="flex flex-wrap gap-4 text-xs text-slate-400">
              <span className="flex items-center gap-1"><MapPin className="w-3.5 h-3.5" />{complex.address}</span>
              {complex.phone && <span className="flex items-center gap-1"><Phone className="w-3.5 h-3.5" />{complex.phone}</span>}
            </div>

            {complex.description && (
              <p className="text-xs text-slate-500 line-clamp-1">{complex.description}</p>
            )}

            {/* Manager card — dữ liệu từ API không phải hardcode */}
            {manager ? (
              <ManagerCard manager={manager} />
            ) : complex.managerId ? (
              <div className="flex items-center gap-2 text-xs text-slate-500">
                <Loader2 className="w-3.5 h-3.5 animate-spin" /> Đang tải thông tin quản lý #{complex.managerId}...
              </div>
            ) : (
              <div className="flex items-center gap-2 text-xs text-slate-600 italic">
                <UserCheck className="w-3.5 h-3.5" /> Chưa phân công quản lý
              </div>
            )}
          </div>

          {/* Mini stats */}
          <div className="flex items-center gap-3 flex-shrink-0">
            <div className="text-center">
              <p className="text-xl font-bold text-slate-100">{stats.total}</p>
              <p className="text-[10px] text-slate-500 uppercase tracking-wider">Tổng sân</p>
            </div>
            <div className="h-8 w-px bg-surface-border" />
            <div className="text-center">
              <p className="text-xl font-bold text-emerald-400">{stats.active}</p>
              <p className="text-[10px] text-slate-500 uppercase tracking-wider">Hoạt động</p>
            </div>
            <div className="h-8 w-px bg-surface-border" />
            <div className="text-center">
              <p className="text-xl font-bold text-amber-400">{stats.maintenance}</p>
              <p className="text-[10px] text-slate-500 uppercase tracking-wider">Bảo trì</p>
            </div>
          </div>
        </div>
      </div>

      {/* Tab navigation */}
      <div className="flex items-center justify-between">
        <div className="flex gap-1 p-1 bg-slate-800/60 rounded-xl border border-surface-border">
          <button
            onClick={() => setActiveTab('courts')}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              activeTab === 'courts' ? 'bg-primary-600/20 text-primary-400 border border-primary-600/30' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            <Building2 className="w-4 h-4" />
            Danh sách sân
            <span className="text-xs text-slate-500">({courts.length})</span>
          </button>
          <button
            onClick={() => setActiveTab('history')}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              activeTab === 'history' ? 'bg-primary-600/20 text-primary-400 border border-primary-600/30' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            <History className="w-4 h-4" />
            Lịch sử thuê
          </button>
        </div>

        {activeTab === 'courts' && (
          <button onClick={() => setModal({ kind: 'create' })} className="btn-primary">
            <Plus className="w-4 h-4" /> Thêm sân mới
          </button>
        )}
      </div>

      {/* Tab: Danh sách sân */}
      {activeTab === 'courts' && (
        <>
          <div className="flex flex-wrap items-center gap-3">
            <div className="flex-1 min-w-52 flex items-center gap-2 bg-slate-800/60 border border-surface-border rounded-xl px-4 py-2.5">
              <Search className="w-4 h-4 text-slate-500 flex-shrink-0" />
              <input type="text" placeholder="Tìm theo tên, mã sân, vị trí..."
                className="flex-1 bg-transparent text-sm text-slate-300 placeholder:text-slate-600 outline-none"
                value={search} onChange={(e) => setSearch(e.target.value)} />
              {search && <button onClick={() => setSearch('')} className="text-slate-500 hover:text-slate-300"><X className="w-3.5 h-3.5" /></button>}
            </div>
            <select className="input-field max-w-[160px] py-2.5" value={filterType} onChange={(e) => setFilterType(e.target.value)}>
              <option value="">Tất cả loại sân</option>
              {courtTypes.map((t) => <option key={t.courtTypeId} value={t.courtTypeId}>{t.typeName}</option>)}
            </select>
            <select className="input-field max-w-[160px] py-2.5" value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)}>
              <option value="">Tất cả trạng thái</option>
              <option value="Available">Hoạt động</option>
              <option value="Maintenance">Bảo trì</option>
              <option value="Inactive">Ngưng hoạt động</option>
              <option value="Booked">Đã đặt</option>
              <option value="InUse">Đang sử dụng</option>
            </select>
            {(search || filterType || filterStatus) && (
              <button onClick={() => { setSearch(''); setFilterType(''); setFilterStatus(''); }} className="btn-ghost text-xs">
                <Filter className="w-3.5 h-3.5" /> Xóa lọc
              </button>
            )}
          </div>

          {filtered.length === 0 ? (
            <div className="rounded-xl border border-dashed border-slate-700 bg-slate-800/20 py-14 text-center">
              <Building2 className="w-10 h-10 text-slate-600 mx-auto mb-3" />
              <h3 className="text-sm font-semibold text-slate-400 mb-1">
                {search || filterType || filterStatus ? 'Không tìm thấy sân phù hợp' : 'Chưa có sân nào trong tổ hợp này'}
              </h3>
              {!search && !filterType && !filterStatus && (
                <button onClick={() => setModal({ kind: 'create' })} className="btn-primary mt-4">
                  <Plus className="w-4 h-4" /> Thêm sân đầu tiên
                </button>
              )}
            </div>
          ) : (
            <div className="rounded-xl border border-surface-border bg-surface-card overflow-hidden">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-surface-border bg-slate-800/60 text-xs text-slate-500 font-semibold uppercase tracking-wider">
                    <th className="py-3 pl-6 pr-4 text-left">Sân</th>
                    <th className="py-3 px-4 text-left">Loại</th>
                    <th className="py-3 px-4 text-left">Kích thước</th>
                    <th className="py-3 px-4 text-left">Vị trí</th>
                    <th className="py-3 px-4 text-left">Giờ hoạt động</th>
                    <th className="py-3 px-4 text-left">Giá/giờ</th>
                    <th className="py-3 px-4 text-left">Trạng thái</th>
                    <th className="py-3 pl-4 pr-6 text-center">Thao tác</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-surface-border/60">
                  {filtered.map((court) => (
                    <tr key={court.courtId} className="hover:bg-slate-800/40 transition-colors group">
                      <td className="py-3.5 pl-6 pr-4">
                        <div className="flex items-center gap-3">
                          <div
                            className="w-10 h-10 rounded-lg overflow-hidden flex-shrink-0 bg-slate-700 border border-slate-600/40 cursor-pointer hover:opacity-80 transition-opacity"
                            onClick={() => {
                              const all = [court.imageUrl, ...(court.imageUrls ?? [])].filter(Boolean) as string[];
                              if (all.length > 0) { setViewerImages(all); setViewerIndex(0); }
                              else toast.error('Sân này chưa có hình ảnh.');
                            }}
                            title="Xem album ảnh"
                          >
                            {court.imageUrl
                              ? <img src={court.imageUrl} alt={court.courtName} className="w-full h-full object-cover" />
                              : <div className="w-full h-full flex items-center justify-center"><ImageIcon className="w-4 h-4 text-slate-500" /></div>}
                          </div>
                          <div>
                            <p className="text-sm font-semibold text-slate-100">{court.courtName}</p>
                            <p className="text-xs text-slate-500 font-mono">{court.courtCode}</p>
                          </div>
                        </div>
                      </td>
                      <td className="py-3.5 px-4">
                        <span className="text-xs font-medium text-slate-300 bg-slate-700/60 border border-slate-600/50 px-2.5 py-1 rounded-full">
                          {court.courtType?.typeName ?? courtTypes.find((t) => t.courtTypeId === court.courtTypeId)?.typeName ?? '—'}
                        </span>
                      </td>
                      <td className="py-3.5 px-4">
                        <span className="text-xs text-slate-300 font-medium bg-indigo-500/10 border border-indigo-500/20 px-2.5 py-1 rounded-full">
                          {court.courtSize ?? 'Tiêu chuẩn'}
                        </span>
                      </td>
                      <td className="py-3.5 px-4">
                        <div className="flex items-center gap-1.5 text-sm text-slate-400">
                          <MapPin className="w-3 h-3 text-slate-600 flex-shrink-0" />{court.location}
                        </div>
                      </td>
                      <td className="py-3.5 px-4">
                        <div className="flex items-center gap-1.5 text-sm text-slate-400">
                          <Clock className="w-3 h-3 text-slate-600 flex-shrink-0" />{court.openTime} – {court.closeTime}
                        </div>
                      </td>
                      <td className="py-3.5 px-4">
                        <span className="text-sm font-semibold text-primary-400">{fmtPrice(court.pricePerHour)}</span>
                        <span className="text-xs text-slate-600">/giờ</span>
                      </td>
                      <td className="py-3.5 px-4"><StatusBadge status={court.status} /></td>
                      <td className="py-3.5 pl-4 pr-6">
                        <div className="flex items-center justify-center gap-1.5 opacity-0 group-hover:opacity-100 transition-opacity">
                          <button onClick={() => setModal({ kind: 'edit', court })} className="p-2 rounded-lg text-slate-400 hover:text-white hover:bg-slate-700 transition-colors" title="Chỉnh sửa">
                            <Edit2 className="w-3.5 h-3.5" />
                          </button>
                          <button onClick={() => setModal({ kind: 'delete', court })} className="p-2 rounded-lg text-slate-400 hover:text-red-400 hover:bg-red-400/10 transition-colors" title="Xóa">
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              <div className="px-6 py-3 border-t border-surface-border/50 bg-slate-800/20 flex items-center justify-between">
                <p className="text-xs text-slate-500">
                  Hiển thị <span className="text-slate-300 font-medium">{filtered.length}</span> / {courts.length} sân
                </p>
              </div>
            </div>
          )}
        </>
      )}

      {/* Tab: Lịch sử thuê */}
      {activeTab === 'history' && (
        <BookingHistoryTab complexId={cid} courts={courts} />
      )}

      {/* Modals */}
      {modal?.kind === 'create' && (
        <CourtModal mode="create" complexId={cid} courtTypes={courtTypes} onClose={() => setModal(null)} onSave={handleSaveCourt} />
      )}
      {modal?.kind === 'edit' && (
        <CourtModal mode="edit" initial={modal.court} complexId={cid} courtTypes={courtTypes} onClose={() => setModal(null)} onSave={handleSaveCourt} />
      )}
      {modal?.kind === 'delete' && (
        <DeleteModal title="Xóa sân" message={`Bạn có chắc muốn xóa sân "${modal.court.courtName}" (${modal.court.courtCode})?`}
          onClose={() => setModal(null)} onConfirm={() => handleDeleteCourt(modal.court)} />
      )}

      {/* Image Viewer */}
      {viewerImages && viewerImages.length > 0 && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/90 backdrop-blur-sm">
          <button onClick={() => setViewerImages(null)} className="absolute top-4 right-4 p-2 rounded-full bg-slate-900/80 text-slate-400 hover:text-white border border-slate-800 transition-colors">
            <X className="w-5 h-5" />
          </button>
          <div className="relative max-w-4xl max-h-[80vh] flex flex-col items-center">
            <img src={viewerImages[viewerIndex]} alt={`Ảnh ${viewerIndex + 1}`} className="max-w-full max-h-[70vh] object-contain rounded-lg border border-slate-800" />
            {viewerImages.length > 1 && (
              <div className="flex items-center justify-between w-full mt-4 gap-4 flex-shrink-0">
                <button onClick={() => setViewerIndex((i) => (i === 0 ? viewerImages.length - 1 : i - 1))} className="btn-secondary py-1.5 px-3 text-xs">◀ Trước</button>
                <span className="text-sm text-slate-400">{viewerIndex + 1} / {viewerImages.length}</span>
                <button onClick={() => setViewerIndex((i) => (i === viewerImages.length - 1 ? 0 : i + 1))} className="btn-secondary py-1.5 px-3 text-xs">Sau ▶</button>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
