import { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus, Search, Edit2, Trash2, Building2, MapPin,
  X, Save, Loader2, Phone, Image as ImageIcon,
  ChevronRight, CheckCircle, Wrench, Ban, Eye,
  ChevronLeft, Filter,
} from 'lucide-react';
import toast from 'react-hot-toast';
import {
  getComplexes, getComplexStats, createComplex, updateComplex, deleteComplex,
} from '@/api/courtApi';
import type { CourtComplex, CourtComplexFormData, ComplexStats } from '@/types/court.types';

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
// Modal: Thêm / Sửa Tổ hợp sân
// ─────────────────────────────────────────────
interface ComplexModalProps {
  mode: 'create' | 'edit';
  initial?: CourtComplex | null;
  onClose: () => void;
  onSave: (data: CourtComplexFormData, complexId?: number) => Promise<void>;
}

function ComplexModal({ mode, initial, onClose, onSave }: ComplexModalProps) {
  const [form, setForm] = useState<CourtComplexFormData>({
    complexName: initial?.complexName ?? '',
    address: initial?.address ?? '',
    phone: initial?.phone ?? '',
    managerName: initial?.managerName ?? '',
    description: initial?.description ?? '',
    imageUrl: initial?.imageUrl ?? '',
  });
  const [saving, setSaving] = useState(false);

  const set = (key: keyof CourtComplexFormData, val: string) =>
    setForm((f) => ({ ...f, [key]: val }));

  const handleSubmit = async () => {
    if (!form.complexName.trim() || !form.address.trim()) {
      toast.error('Tên tổ hợp và địa chỉ không được để trống.');
      return;
    }
    setSaving(true);
    try {
      await onSave(form, initial?.complexId);
      onClose();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string } }; message?: string };
      toast.error(err?.response?.data?.message ?? err?.message ?? 'Lưu thất bại.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div className="w-full max-w-lg bg-surface-card border border-surface-border rounded-2xl shadow-2xl overflow-hidden">
        <div className="px-6 py-4 border-b border-surface-border flex items-center justify-between bg-slate-800/60">
          <h2 className="text-base font-bold text-white">
            {mode === 'create' ? 'Thêm tổ hợp sân mới' : 'Chỉnh sửa tổ hợp sân'}
          </h2>
          <button onClick={onClose} className="text-slate-400 hover:text-white transition-colors p-1 rounded-lg hover:bg-slate-700">
            <X className="w-4 h-4" />
          </button>
        </div>

        <div className="p-6 space-y-4 max-h-[65vh] overflow-y-auto">
          <Field label="Tên tổ hợp *" icon={<Building2 className="w-3.5 h-3.5" />}>
            <input className="input-field w-full" placeholder="VD: SportZone Cầu Giấy" value={form.complexName} onChange={(e) => set('complexName', e.target.value)} />
          </Field>
          <Field label="Địa chỉ *" icon={<MapPin className="w-3.5 h-3.5" />}>
            <input className="input-field w-full" placeholder="VD: 123 Nguyễn Trãi, Hà Nội" value={form.address} onChange={(e) => set('address', e.target.value)} />
          </Field>
          <div className="grid grid-cols-2 gap-4">
            <Field label="Số điện thoại" icon={<Phone className="w-3.5 h-3.5" />}>
              <input className="input-field w-full" placeholder="0912345678" value={form.phone} onChange={(e) => set('phone', e.target.value)} />
            </Field>
            <Field label="Tên quản lý" icon={<CheckCircle className="w-3.5 h-3.5" />}>
              <input className="input-field w-full" placeholder="VD: Nguyễn Văn A" value={form.managerName} onChange={(e) => set('managerName', e.target.value)} />
            </Field>
          </div>
          <Field label="Mô tả">
            <textarea className="input-field w-full min-h-[80px] resize-none" placeholder="Mô tả ngắn về tổ hợp sân..." value={form.description} onChange={(e) => set('description', e.target.value)} />
          </Field>
          <Field label="URL ảnh đại diện" icon={<ImageIcon className="w-3.5 h-3.5" />}>
            <input className="input-field w-full" placeholder="https://..." value={form.imageUrl} onChange={(e) => set('imageUrl', e.target.value)} />
          </Field>
          {form.imageUrl && (
            <img src={form.imageUrl} alt="preview" className="w-full h-32 object-cover rounded-lg border border-surface-border" onError={(e) => { e.currentTarget.style.display = 'none'; }} />
          )}
        </div>

        <div className="px-6 py-4 border-t border-surface-border flex justify-end gap-3 bg-slate-800/40">
          <button onClick={onClose} disabled={saving} className="btn-secondary">Hủy</button>
          <button onClick={handleSubmit} disabled={saving} className="btn-primary">
            {saving ? <Loader2 className="w-4 h-4 animate-spin" /> : <Save className="w-4 h-4" />}
            {mode === 'create' ? 'Thêm mới' : 'Lưu thay đổi'}
          </button>
        </div>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────
// Modal: Xác nhận xóa
// ─────────────────────────────────────────────
interface DeleteModalProps {
  title: string;
  message: string;
  onClose: () => void;
  onConfirm: () => Promise<void>;
}

function DeleteModal({ title, message, onClose, onConfirm }: DeleteModalProps) {
  const [busy, setBusy] = useState(false);
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div className="w-full max-w-sm bg-surface-card border border-surface-border rounded-2xl shadow-2xl p-6">
        <div className="flex items-center gap-3 mb-4">
          <div className="w-10 h-10 rounded-full bg-red-500/10 border border-red-500/30 flex items-center justify-center flex-shrink-0">
            <Trash2 className="w-5 h-5 text-red-400" />
          </div>
          <h2 className="text-base font-bold text-white">{title}</h2>
        </div>
        <p className="text-sm text-slate-400 mb-6">{message}</p>
        <div className="flex justify-end gap-3">
          <button onClick={onClose} disabled={busy} className="btn-secondary">Hủy</button>
          <button
            onClick={async () => { setBusy(true); try { await onConfirm(); onClose(); } finally { setBusy(false); } }}
            disabled={busy}
            className="px-4 py-2 rounded-xl bg-red-600 hover:bg-red-500 text-white font-semibold text-sm flex items-center gap-2 transition-colors disabled:opacity-50"
          >
            {busy ? <Loader2 className="w-4 h-4 animate-spin" /> : <Trash2 className="w-4 h-4" />}
            Xóa
          </button>
        </div>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────────
// Court type filter definitions
// ─────────────────────────────────────────────
const COURT_TYPE_FILTERS = [
  { id: undefined, label: 'Tất cả' },
  { id: 1,         label: '🏸 Cầu lông' },
  { id: 2,         label: '⚽ Bóng đá' },
  { id: 3,         label: '🏓 Pickleball' },
  { id: 4,         label: '🎾 Tennis' },
  { id: 5,         label: '🏀 Bóng rổ' },
] as const;

const PAGE_SIZE = 8;

// ─────────────────────────────────────────────
// Main Page
// ─────────────────────────────────────────────
export default function ManageCourtsPage() {
  const navigate = useNavigate();

  // List state
  const [complexes, setComplexes]   = useState<CourtComplex[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [page, setPage]             = useState(1);
  const [loading, setLoading]       = useState(true);

  // Filter state
  const [search, setSearch]               = useState('');
  const [pendingSearch, setPendingSearch] = useState('');
  const [courtTypeId, setCourtTypeId]     = useState<number | undefined>(undefined);

  // Stats
  const [stats, setStats] = useState<ComplexStats | null>(null);

  type ModalType =
    | { kind: 'create' }
    | { kind: 'edit'; complex: CourtComplex }
    | { kind: 'delete'; complex: CourtComplex }
    | null;

  const [modal, setModal]       = useState<ModalType>(null);
  const debounceTimer           = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Load stats once
  useEffect(() => {
    getComplexStats()
      .then(setStats)
      .catch(() => {/* stats not critical */});
  }, []);

  // Load list
  const loadComplexes = useCallback(async (
    p: number,
    q: string,
    typeId: number | undefined
  ) => {
    setLoading(true);
    try {
      const result = await getComplexes({
        search: q || undefined,
        courtTypeId: typeId,
        page: p,
        pageSize: PAGE_SIZE,
      });
      setComplexes(result.items);
      setTotalCount(result.totalCount);
      setTotalPages(result.totalPages);
    } catch {
      toast.error('Không thể tải dữ liệu. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadComplexes(page, search, courtTypeId);
  }, [page, search, courtTypeId, loadComplexes]);

  // Debounced search
  const handleSearchInput = (val: string) => {
    setPendingSearch(val);
    if (debounceTimer.current) clearTimeout(debounceTimer.current);
    debounceTimer.current = setTimeout(() => {
      setSearch(val);
      setPage(1);
    }, 400);
  };

  const handleTypeFilter = (typeId: number | undefined) => {
    setCourtTypeId(typeId);
    setPage(1);
  };

  const refreshAll = useCallback(() => {
    loadComplexes(page, search, courtTypeId);
    getComplexStats().then(setStats).catch(() => {});
  }, [page, search, courtTypeId, loadComplexes]);

  // CRUD
  const handleSave = useCallback(async (data: CourtComplexFormData, complexId?: number) => {
    if (complexId) {
      await updateComplex(complexId, data);
      toast.success('Cập nhật tổ hợp sân thành công!');
    } else {
      await createComplex(data);
      toast.success('Thêm tổ hợp sân thành công!');
    }
    refreshAll();
  }, [refreshAll]);

  const handleDelete = useCallback(async (complex: CourtComplex) => {
    if (complex.totalCourts && complex.totalCourts > 0) {
      toast.error('Vui lòng xóa hoặc di chuyển hết sân trong tổ hợp trước khi xóa!');
      return;
    }
    await deleteComplex(complex.complexId);
    toast.success('Xóa tổ hợp sân thành công!');
    refreshAll();
  }, [refreshAll]);

  return (
    <div className="p-6 space-y-6 min-h-full">
      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-xl font-bold text-white">Quản lý sân thể thao</h1>
          <p className="text-sm text-slate-500 mt-0.5">
            Quản lý các tổ hợp sân — nhấn <span className="text-primary-400 font-medium">Chi tiết</span> để xem và quản lý sân bên trong
          </p>
        </div>
        <button onClick={() => setModal({ kind: 'create' })} className="btn-primary flex-shrink-0">
          <Plus className="w-4 h-4" /> Thêm tổ hợp sân
        </button>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4">
        {[
          { label: 'Tổ hợp sân',     value: stats?.totalComplexes,    color: 'text-indigo-400',  bg: 'from-indigo-500/10 to-indigo-500/5 border-indigo-500/20',   icon: <Building2   className="w-5 h-5 text-indigo-400" /> },
          { label: 'Tổng số sân',     value: stats?.totalCourts,       color: 'text-slate-100',   bg: 'from-slate-700/30 to-slate-700/10 border-slate-600/30',     icon: <CheckCircle className="w-5 h-5 text-slate-400" /> },
          { label: 'Đang hoạt động',  value: stats?.activeCourts,      color: 'text-emerald-400', bg: 'from-emerald-500/10 to-emerald-500/5 border-emerald-500/20', icon: <CheckCircle className="w-5 h-5 text-emerald-400" /> },
          { label: 'Đang bảo trì',    value: stats?.maintenanceCourts, color: 'text-amber-400',   bg: 'from-amber-500/10 to-amber-500/5 border-amber-500/20',       icon: <Wrench      className="w-5 h-5 text-amber-400" /> },
          { label: 'Ngưng hoạt động', value: stats?.inactiveCourts,    color: 'text-slate-400',   bg: 'from-slate-700/20 to-slate-700/5 border-slate-600/20',      icon: <Ban         className="w-5 h-5 text-slate-500" /> },
        ].map((s) => (
          <div key={s.label} className={`rounded-xl border bg-gradient-to-br ${s.bg} p-4 flex items-center gap-3`}>
            <div className="flex-shrink-0">{s.icon}</div>
            <div>
              <p className="text-xs text-slate-500 leading-none mb-1">{s.label}</p>
              <p className={`text-2xl font-bold leading-none ${s.color}`}>
                {s.value !== undefined ? s.value : <span className="inline-block w-6 h-5 bg-slate-700 rounded animate-pulse" />}
              </p>
            </div>
          </div>
        ))}
      </div>

      {/* Search + Filter */}
      <div className="flex flex-wrap items-center gap-3">
        <div className="flex-1 min-w-[200px] max-w-sm flex items-center gap-2 bg-slate-800/60 border border-surface-border rounded-xl px-4 py-2.5">
          <Search className="w-4 h-4 text-slate-500 flex-shrink-0" />
          <input
            id="complex-search"
            type="text"
            placeholder="Tìm tên, địa chỉ, quản lý..."
            className="flex-1 bg-transparent text-sm text-slate-300 placeholder:text-slate-600 outline-none"
            value={pendingSearch}
            onChange={(e) => handleSearchInput(e.target.value)}
          />
          {pendingSearch && (
            <button onClick={() => { setPendingSearch(''); setSearch(''); setPage(1); }} className="text-slate-500 hover:text-slate-300 transition-colors">
              <X className="w-3.5 h-3.5" />
            </button>
          )}
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          <Filter className="w-4 h-4 text-slate-500 flex-shrink-0" />
          {COURT_TYPE_FILTERS.map((f) => (
            <button
              key={f.label}
              onClick={() => handleTypeFilter(f.id)}
              className={`px-3 py-1.5 rounded-lg text-xs font-medium border transition-colors whitespace-nowrap ${
                courtTypeId === f.id
                  ? 'bg-primary-600/20 border-primary-500/40 text-primary-300'
                  : 'bg-slate-800/40 border-slate-700/40 text-slate-400 hover:text-slate-200 hover:border-slate-600'
              }`}
            >
              {f.label}
            </button>
          ))}
        </div>

        <p className="text-sm text-slate-500 ml-auto whitespace-nowrap">
          <span className="text-slate-300 font-medium">{totalCount}</span> tổ hợp
        </p>
      </div>

      {/* Table */}
      {loading ? (
        <div className="flex items-center justify-center py-20">
          <div className="text-center space-y-3">
            <Loader2 className="w-8 h-8 text-primary-500 animate-spin mx-auto" />
            <p className="text-sm text-slate-500">Đang tải dữ liệu...</p>
          </div>
        </div>
      ) : complexes.length === 0 ? (
        <div className="rounded-xl border border-dashed border-slate-700 bg-slate-800/20 py-16 text-center">
          <Building2 className="w-12 h-12 text-slate-600 mx-auto mb-3" />
          <h3 className="text-base font-semibold text-slate-400 mb-1">
            {search || courtTypeId ? 'Không tìm thấy tổ hợp phù hợp' : 'Chưa có tổ hợp sân nào'}
          </h3>
          <p className="text-sm text-slate-600 mb-5">
            {search || courtTypeId ? 'Thử thay đổi từ khóa hoặc bộ lọc' : 'Thêm tổ hợp sân đầu tiên để bắt đầu quản lý'}
          </p>
          {!search && !courtTypeId && (
            <button onClick={() => setModal({ kind: 'create' })} className="btn-primary">
              <Plus className="w-4 h-4" /> Thêm tổ hợp sân
            </button>
          )}
        </div>
      ) : (
        <div className="rounded-xl border border-surface-border bg-surface-card overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-surface-border bg-slate-800/60 text-xs text-slate-500 font-semibold uppercase tracking-wider">
                <th className="py-3 pl-6 pr-4 text-left">Tổ hợp sân</th>
                <th className="py-3 px-4 text-left">Quản lý</th>
                <th className="py-3 px-4 text-left">Liên hệ</th>
                <th className="py-3 px-4 text-center">Tổng sân</th>
                <th className="py-3 px-4 text-center">Hoạt động</th>
                <th className="py-3 px-4 text-center">Bảo trì</th>
                <th className="py-3 pl-4 pr-6 text-center">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-surface-border/60">
              {complexes.map((cx) => {
                const counts = {
                  total: cx.totalCourts ?? 0,
                  active: cx.activeCourts ?? 0,
                  maintenance: cx.maintenanceCourts ?? 0,
                  inactive: cx.inactiveCourts ?? 0,
                };
                return (
                  <tr key={cx.complexId} className="hover:bg-slate-800/40 transition-colors group">
                    <td className="py-4 pl-6 pr-4">
                      <div className="flex items-center gap-3">
                        <div className="w-11 h-11 rounded-xl overflow-hidden flex-shrink-0 bg-indigo-500/10 border border-indigo-500/20 flex items-center justify-center">
                          {cx.imageUrl ? (
                            <img src={cx.imageUrl} alt={cx.complexName} className="w-full h-full object-cover" />
                          ) : (
                            <Building2 className="w-5 h-5 text-indigo-400" />
                          )}
                        </div>
                        <div>
                          <p className="text-sm font-semibold text-slate-100 group-hover:text-white transition-colors">{cx.complexName}</p>
                          <div className="flex items-center gap-1 mt-0.5">
                            <MapPin className="w-3 h-3 text-slate-600" />
                            <p className="text-xs text-slate-500 truncate max-w-[200px]">{cx.address}</p>
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="py-4 px-4">
                      {cx.managerName ? (
                        <div className="flex items-center gap-2">
                          <div className="w-6 h-6 rounded-full bg-gradient-to-br from-primary-500/30 to-indigo-500/30 flex items-center justify-center flex-shrink-0 text-[10px] font-bold text-primary-400">
                            {cx.managerName.charAt(cx.managerName.lastIndexOf(' ') + 1).toUpperCase()}
                          </div>
                          <span className="text-sm text-slate-300">{cx.managerName}</span>
                        </div>
                      ) : (
                        <span className="text-xs text-slate-600 italic">— Chưa phân công</span>
                      )}
                    </td>
                    <td className="py-4 px-4">
                      <span className="text-sm text-slate-400">{cx.phone ?? '—'}</span>
                    </td>
                    <td className="py-4 px-4 text-center">
                      <span className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-slate-700/60 border border-slate-600/40 text-sm font-bold text-slate-200">
                        {counts.total}
                      </span>
                    </td>
                    <td className="py-4 px-4 text-center">
                      {counts.active > 0 ? (
                        <span className="inline-flex items-center gap-1 rounded-full border border-emerald-400/30 bg-emerald-400/10 px-2.5 py-0.5 text-xs font-semibold text-emerald-400">
                          <CheckCircle className="w-3 h-3" /> {counts.active}
                        </span>
                      ) : (
                        <span className="text-xs text-slate-600">—</span>
                      )}
                    </td>
                    <td className="py-4 px-4 text-center">
                      {counts.maintenance > 0 ? (
                        <span className="inline-flex items-center gap-1 rounded-full border border-amber-400/30 bg-amber-400/10 px-2.5 py-0.5 text-xs font-semibold text-amber-400">
                          <Wrench className="w-3 h-3" /> {counts.maintenance}
                        </span>
                      ) : (
                        <span className="text-xs text-slate-600">—</span>
                      )}
                    </td>
                    <td className="py-4 pl-4 pr-6">
                      <div className="flex items-center justify-center gap-1.5">
                        <button
                          onClick={() => navigate(`/admin/courts/${cx.complexId}`)}
                          className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium bg-primary-600/10 text-primary-400 border border-primary-600/30 hover:bg-primary-600/20 transition-colors"
                        >
                          <Eye className="w-3.5 h-3.5" /> Chi tiết <ChevronRight className="w-3 h-3" />
                        </button>
                        <button
                          onClick={() => setModal({ kind: 'edit', complex: cx })}
                          className="p-2 rounded-lg text-slate-400 hover:text-white hover:bg-slate-700 transition-colors"
                        >
                          <Edit2 className="w-3.5 h-3.5" />
                        </button>
                        <button
                          onClick={() => setModal({ kind: 'delete', complex: cx })}
                          className="p-2 rounded-lg text-slate-400 hover:text-red-400 hover:bg-red-400/10 transition-colors"
                        >
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && !loading && (
        <div className="flex items-center justify-between pt-2">
          <p className="text-sm text-slate-500">
            Trang <span className="text-slate-300 font-medium">{page}</span> / {totalPages}
            {' '}· Tổng <span className="text-slate-300 font-medium">{totalCount}</span> tổ hợp
          </p>
          <div className="flex items-center gap-1.5">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
              className="p-2 rounded-lg border border-slate-700 text-slate-400 hover:text-white hover:border-slate-500 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
            >
              <ChevronLeft className="w-4 h-4" />
            </button>
            {Array.from({ length: totalPages }, (_, i) => i + 1)
              .filter((p) => p === 1 || p === totalPages || Math.abs(p - page) <= 1)
              .reduce<(number | '...')[]>((acc, p, idx, arr) => {
                if (idx > 0 && p - (arr[idx - 1] as number) > 1) acc.push('...');
                acc.push(p);
                return acc;
              }, [])
              .map((item, idx) =>
                item === '...' ? (
                  <span key={`dot-${idx}`} className="px-2 text-slate-600 text-sm">…</span>
                ) : (
                  <button
                    key={item}
                    onClick={() => setPage(item as number)}
                    className={`w-9 h-9 rounded-lg text-sm font-medium border transition-colors ${
                      page === item
                        ? 'bg-primary-600 border-primary-500 text-white'
                        : 'border-slate-700 text-slate-400 hover:text-white hover:border-slate-500'
                    }`}
                  >
                    {item}
                  </button>
                )
              )}
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
              className="p-2 rounded-lg border border-slate-700 text-slate-400 hover:text-white hover:border-slate-500 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
            >
              <ChevronRight className="w-4 h-4" />
            </button>
          </div>
        </div>
      )}

      {/* Modals */}
      {modal?.kind === 'create' && (
        <ComplexModal mode="create" onClose={() => setModal(null)} onSave={handleSave} />
      )}
      {modal?.kind === 'edit' && (
        <ComplexModal mode="edit" initial={modal.complex} onClose={() => setModal(null)} onSave={handleSave} />
      )}
      {modal?.kind === 'delete' && (
        <DeleteModal
          title="Xóa tổ hợp sân"
          message={`Bạn có chắc muốn xóa tổ hợp "${modal.complex.complexName}"? Hành động này không thể hoàn tác.`}
          onClose={() => setModal(null)}
          onConfirm={() => handleDelete(modal.complex)}
        />
      )}
    </div>
  );
}
