import { useState, useEffect, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus, Search, Edit2, Trash2, Building2, MapPin,
  X, Save, Loader2, Phone, Image as ImageIcon,
  ChevronRight, CheckCircle, Wrench, Ban, Eye,
  ChevronLeft, Filter, UserCheck, AlertCircle,
} from 'lucide-react';
import toast from 'react-hot-toast';
import {
  getComplexes, getComplexStats, createComplex, updateComplex, deleteComplex,
  getCourtTypes, getManagerById, getManagersList,
} from '@/api/courtApi';
import type {
  CourtComplex, CourtComplexFormData, ComplexStats, ManagerUser,
} from '@/types/court.types';

// ─────────────────────────────────────────────
// Court type badge config
// ─────────────────────────────────────────────
const TYPE_BADGE: Record<number, { label: string; color: string }> = {
  1: { label: 'Cầu lông',   color: 'bg-sky-500/10 text-sky-400 border-sky-500/20' },
  2: { label: 'Bóng đá',   color: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20' },
  3: { label: 'Pickleball', color: 'bg-violet-500/10 text-violet-400 border-violet-500/20' },
  4: { label: 'Tennis',     color: 'bg-amber-500/10 text-amber-400 border-amber-500/20' },
  5: { label: 'Bóng rổ',   color: 'bg-orange-500/10 text-orange-400 border-orange-500/20' },
};

// ─────────────────────────────────────────────
// Field wrapper
// ─────────────────────────────────────────────
function Field({ label, icon, children, hint }: {
  label: string; icon?: React.ReactNode; children: React.ReactNode; hint?: string;
}) {
  return (
    <div>
      <label className="flex items-center gap-1.5 text-xs font-medium text-slate-400 mb-1.5">
        {icon} {label}
      </label>
      {children}
      {hint && <p className="mt-1 text-[11px] text-slate-600">{hint}</p>}
    </div>
  );
}

// ─────────────────────────────────────────────
// Manager Lookup — nhập mã → fetch → preview
// ─────────────────────────────────────────────
function ManagerLookup({
  managerId,
  resolvedManager,
  onResolve,
}: {
  managerId: string;
  resolvedManager: ManagerUser | null;
  onResolve: (id: string, manager: ManagerUser | null) => void;
}) {
  const [inputId, setInputId] = useState(managerId);
  const [looking, setLooking] = useState(false);
  const [error, setError] = useState('');
  const [allManagers, setAllManagers] = useState<ManagerUser[]>([]);
  const [showDropdown, setShowDropdown] = useState(false);

  useEffect(() => {
    getManagersList().then(setAllManagers).catch(() => {});
  }, []);

  const handleLookup = async () => {
    const id = inputId.trim();
    if (!id) { onResolve('', null); setError(''); return; }
    const numId = Number(id);
    if (isNaN(numId) || numId <= 0) { setError('Mã quản lý phải là số nguyên dương'); return; }
    setLooking(true); setError('');
    try {
      const m = await getManagerById(numId);
      if (m) { onResolve(id, m); setError(''); }
      else   { onResolve(id, null); setError(`Không tìm thấy quản lý với mã #${numId}`); }
    } catch { setError('Lỗi khi tìm quản lý'); }
    finally { setLooking(false); }
  };

  const pickFromList = (m: ManagerUser) => {
    setInputId(String(m.userId));
    onResolve(String(m.userId), m);
    setShowDropdown(false);
    setError('');
  };

  const clear = () => { setInputId(''); onResolve('', null); setError(''); };

  return (
    <div className="space-y-2">
      <div className="flex gap-2">
        <div className="relative flex-1">
          <input
            className="input-field w-full pr-8"
            placeholder="Nhập mã quản lý (VD: 10)"
            value={inputId}
            onChange={(e) => { setInputId(e.target.value); setError(''); }}
            onKeyDown={(e) => e.key === 'Enter' && handleLookup()}
          />
          {inputId && (
            <button
              onClick={clear}
              className="absolute right-2.5 top-1/2 -translate-y-1/2 text-slate-500 hover:text-slate-300"
            >
              <X className="w-3.5 h-3.5" />
            </button>
          )}
        </div>
        <button
          type="button"
          onClick={handleLookup}
          disabled={looking}
          className="btn-primary px-3 py-2 text-xs whitespace-nowrap"
        >
          {looking ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Search className="w-3.5 h-3.5" />}
          Tìm
        </button>
        <div className="relative">
          <button
            type="button"
            onClick={() => setShowDropdown((v) => !v)}
            className="btn-secondary px-3 py-2 text-xs"
            title="Chọn từ danh sách"
          >
            ▾
          </button>
          {showDropdown && allManagers.length > 0 && (
            <div className="absolute right-0 top-full mt-1 z-50 w-64 bg-slate-800 border border-surface-border rounded-xl shadow-2xl overflow-hidden">
              <p className="px-3 py-2 text-[10px] text-slate-500 uppercase tracking-wider font-semibold border-b border-surface-border">
                Chọn quản lý
              </p>
              {allManagers.map((m) => (
                <button
                  key={m.userId}
                  type="button"
                  onClick={() => pickFromList(m)}
                  className="w-full flex items-center gap-2.5 px-3 py-2.5 hover:bg-slate-700 text-left transition-colors"
                >
                  <div className="w-7 h-7 rounded-full bg-primary-500/20 flex items-center justify-center flex-shrink-0 text-xs font-bold text-primary-400">
                    {m.fullName.charAt(m.fullName.lastIndexOf(' ') + 1)}
                  </div>
                  <div className="min-w-0">
                    <p className="text-sm text-slate-200 font-medium truncate">{m.fullName}</p>
                    <p className="text-[10px] text-slate-500 truncate">#{m.userId} · {m.email}</p>
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>
      </div>

      {error && (
        <div className="flex items-center gap-1.5 text-xs text-red-400">
          <AlertCircle className="w-3.5 h-3.5 flex-shrink-0" /> {error}
        </div>
      )}

      {resolvedManager && (
        <div className="flex items-center gap-3 rounded-xl border border-emerald-500/25 bg-emerald-500/5 px-3 py-2.5">
          <img
            src={resolvedManager.avatarUrl ?? `https://api.dicebear.com/8.x/avataaars/svg?seed=${resolvedManager.userId}`}
            alt=""
            className="w-8 h-8 rounded-full bg-slate-700 flex-shrink-0"
          />
          <div className="flex-1 min-w-0">
            <p className="text-sm font-semibold text-emerald-300 flex items-center gap-1.5">
              <UserCheck className="w-3.5 h-3.5" /> {resolvedManager.fullName}
            </p>
            <p className="text-[11px] text-slate-400 truncate">
              #{resolvedManager.userId} · {resolvedManager.email} · {resolvedManager.phone ?? '—'}
            </p>
          </div>
          <button
            type="button"
            onClick={clear}
            className="text-slate-500 hover:text-red-400 transition-colors flex-shrink-0"
            title="Xóa chọn"
          >
            <X className="w-3.5 h-3.5" />
          </button>
        </div>
      )}
    </div>
  );
}

// ─────────────────────────────────────────────
// Modal: Thêm / Sửa Tổ hợp sân
// ─────────────────────────────────────────────
interface ComplexModalProps {
  mode: 'create' | 'edit';
  initial?: CourtComplex | null;
  courtTypeOptions: { courtTypeId: number; typeName: string }[];
  onClose: () => void;
  onSave: (data: CourtComplexFormData, complexId?: number) => Promise<void>;
}

function ComplexModal({ mode, initial, courtTypeOptions, onClose, onSave }: ComplexModalProps) {
  const [form, setForm] = useState<CourtComplexFormData>({
    complexName: initial?.complexName ?? '',
    address:     initial?.address ?? '',
    phone:       initial?.phone ?? '',
    managerId:   initial?.managerId,
    description: initial?.description ?? '',
    imageUrl:    initial?.imageUrl ?? '',
  });
  const [managerIdStr, setManagerIdStr] = useState(
    initial?.managerId ? String(initial.managerId) : ''
  );
  const [resolvedManager, setResolvedManager] = useState<ManagerUser | null>(null);
  const [saving, setSaving] = useState(false);

  // Nếu edit, tự fetch manager hiện tại để hiển thị preview
  useEffect(() => {
    if (initial?.managerId) {
      getManagerById(initial.managerId).then((m) => {
        if (m) setResolvedManager(m);
      });
    }
  }, [initial?.managerId]);

  const set = <K extends keyof CourtComplexFormData>(key: K, val: CourtComplexFormData[K]) =>
    setForm((f) => ({ ...f, [key]: val }));

  const handleManagerResolve = (id: string, manager: ManagerUser | null) => {
    setManagerIdStr(id);
    setResolvedManager(manager);
    set('managerId', manager ? manager.userId : undefined);
  };

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
        {/* Header */}
        <div className="px-6 py-4 border-b border-surface-border flex items-center justify-between bg-slate-800/60">
          <h2 className="text-base font-bold text-white">
            {mode === 'create' ? 'Thêm tổ hợp sân mới' : 'Chỉnh sửa tổ hợp sân'}
          </h2>
          <button onClick={onClose} className="text-slate-400 hover:text-white p-1 rounded-lg hover:bg-slate-700 transition-colors">
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Body */}
        <div className="p-6 space-y-4 max-h-[65vh] overflow-y-auto">
          <Field label="Tên tổ hợp *" icon={<Building2 className="w-3.5 h-3.5" />}>
            <input className="input-field w-full" placeholder="VD: SportZone Cầu Giấy"
              value={form.complexName} onChange={(e) => set('complexName', e.target.value)} />
          </Field>

          <Field label="Địa chỉ *" icon={<MapPin className="w-3.5 h-3.5" />}>
            <input className="input-field w-full" placeholder="VD: 123 Nguyễn Trãi, Hà Nội"
              value={form.address} onChange={(e) => set('address', e.target.value)} />
          </Field>

          <Field label="Số điện thoại" icon={<Phone className="w-3.5 h-3.5" />}>
            <input className="input-field w-full" placeholder="024 xxxx xxxx"
              value={form.phone} onChange={(e) => set('phone', e.target.value)} />
          </Field>

          {/* Manager lookup */}
          <Field
            label="Quản lý phụ trách"
            icon={<UserCheck className="w-3.5 h-3.5" />}
            hint="Nhập mã số quản lý hoặc chọn từ danh sách ▾ để tra cứu thông tin tự động"
          >
            <ManagerLookup
              managerId={managerIdStr}
              resolvedManager={resolvedManager}
              onResolve={handleManagerResolve}
            />
          </Field>

          {/* Loại sân */}
          {courtTypeOptions.length > 0 && (
            <Field label="Loại sân cung cấp">
              <div className="flex flex-wrap gap-2">
                {courtTypeOptions.map((t) => {
                  const active = (form as CourtComplexFormData & { courtTypeIds?: number[] }).courtTypeIds?.includes(t.courtTypeId) ?? false;
                  return (
                    <button
                      key={t.courtTypeId}
                      type="button"
                      onClick={() => {
                        const cur = (form as CourtComplexFormData & { courtTypeIds?: number[] }).courtTypeIds ?? [];
                        const next = active ? cur.filter((id) => id !== t.courtTypeId) : [...cur, t.courtTypeId];
                        setForm((f) => ({ ...f, courtTypeIds: next } as typeof f));
                      }}
                      className={`px-3 py-1.5 rounded-lg text-xs font-semibold border transition-colors ${
                        active
                          ? 'bg-primary-600/20 border-primary-500/40 text-primary-300'
                          : 'bg-slate-800/60 border-slate-700 text-slate-400 hover:text-slate-200'
                      }`}
                    >
                      {t.typeName}
                    </button>
                  );
                })}
              </div>
            </Field>
          )}

          <Field label="Mô tả">
            <textarea className="input-field w-full min-h-[72px] resize-none"
              placeholder="Mô tả ngắn về tổ hợp sân..."
              value={form.description} onChange={(e) => set('description', e.target.value)} />
          </Field>

          <Field label="URL ảnh đại diện" icon={<ImageIcon className="w-3.5 h-3.5" />}>
            <input className="input-field w-full" placeholder="https://..."
              value={form.imageUrl} onChange={(e) => set('imageUrl', e.target.value)} />
          </Field>
          {form.imageUrl && (
            <img src={form.imageUrl} alt="preview"
              className="w-full h-28 object-cover rounded-lg border border-surface-border"
              onError={(e) => { e.currentTarget.style.display = 'none'; }} />
          )}
        </div>

        {/* Footer */}
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
function DeleteModal({ title, message, onClose, onConfirm }: {
  title: string; message: string; onClose: () => void; onConfirm: () => Promise<void>;
}) {
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

const COURT_TYPE_FILTERS = [
  { id: undefined, label: 'Tất cả' },
  { id: 1, label: '🏸 Cầu lông' },
  { id: 2, label: '⚽ Bóng đá' },
  { id: 3, label: '🏓 Pickleball' },
  { id: 4, label: '🎾 Tennis' },
  { id: 5, label: '🏀 Bóng rổ' },
] as const;

const PAGE_SIZE = 8;

// ─────────────────────────────────────────────
// Main Page
// ─────────────────────────────────────────────
export default function ManageCourtsPage() {
  const navigate = useNavigate();

  const [complexes, setComplexes]   = useState<CourtComplex[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [page, setPage]             = useState(1);
  const [loading, setLoading]       = useState(true);

  const [search, setSearch]               = useState('');
  const [pendingSearch, setPendingSearch] = useState('');
  const [courtTypeId, setCourtTypeId]     = useState<number | undefined>(undefined);

  const [stats, setStats]           = useState<ComplexStats | null>(null);
  const [courtTypeOptions, setCourtTypeOptions] = useState<{ courtTypeId: number; typeName: string }[]>([]);

  // Manager cache: managerId → ManagerUser
  const [managerCache, setManagerCache] = useState<Map<number, ManagerUser>>(new Map());

  type ModalType =
    | { kind: 'create' }
    | { kind: 'edit'; complex: CourtComplex }
    | { kind: 'delete'; complex: CourtComplex }
    | null;
  const [modal, setModal]     = useState<ModalType>(null);
  const debounceTimer         = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    getComplexStats().then(setStats).catch(() => {});
    getCourtTypes().then(setCourtTypeOptions).catch(() => {});
  }, []);

  const loadComplexes = useCallback(async (p: number, q: string, typeId: number | undefined) => {
    setLoading(true);
    try {
      const result = await getComplexes({ search: q || undefined, courtTypeId: typeId, page: p, pageSize: PAGE_SIZE });
      setComplexes(result.items);
      setTotalCount(result.totalCount);
      setTotalPages(result.totalPages);

      // Preload managers vào cache
      const ids = [...new Set(result.items.map((c) => c.managerId).filter(Boolean) as number[])];
      const toFetch = ids.filter((id) => !managerCache.has(id));
      if (toFetch.length > 0) {
        const fetched = await Promise.all(toFetch.map((id) => getManagerById(id)));
        setManagerCache((prev) => {
          const next = new Map(prev);
          toFetch.forEach((id, i) => { if (fetched[i]) next.set(id, fetched[i]!); });
          return next;
        });
      }
    } catch {
      toast.error('Không thể tải dữ liệu. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  }, [managerCache]);

  useEffect(() => {
    loadComplexes(page, search, courtTypeId);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, search, courtTypeId]);

  const handleSearchInput = (val: string) => {
    setPendingSearch(val);
    if (debounceTimer.current) clearTimeout(debounceTimer.current);
    debounceTimer.current = setTimeout(() => { setSearch(val); setPage(1); }, 400);
  };

  const refreshAll = useCallback(() => {
    loadComplexes(page, search, courtTypeId);
    getComplexStats().then(setStats).catch(() => {});
  }, [page, search, courtTypeId, loadComplexes]);

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
      toast.error('Vui lòng xóa hoặc di chuyển hết sân trước khi xóa tổ hợp!');
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
            Quản lý các tổ hợp sân — nhấn <span className="text-primary-400 font-medium">Chi tiết</span> để xem sân và lịch sử thuê
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
            type="text"
            placeholder="Tìm tên, địa chỉ, quản lý..."
            className="flex-1 bg-transparent text-sm text-slate-300 placeholder:text-slate-600 outline-none"
            value={pendingSearch}
            onChange={(e) => handleSearchInput(e.target.value)}
          />
          {pendingSearch && (
            <button onClick={() => { setPendingSearch(''); setSearch(''); setPage(1); }} className="text-slate-500 hover:text-slate-300">
              <X className="w-3.5 h-3.5" />
            </button>
          )}
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          <Filter className="w-4 h-4 text-slate-500 flex-shrink-0" />
          {COURT_TYPE_FILTERS.map((f) => (
            <button
              key={f.label}
              onClick={() => { setCourtTypeId(f.id); setPage(1); }}
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
                <th className="py-3 px-4 text-left">Loại sân</th>
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
                const manager = cx.managerId ? managerCache.get(cx.managerId) : undefined;
                const counts = {
                  total: cx.totalCourts ?? 0,
                  active: cx.activeCourts ?? 0,
                  maintenance: cx.maintenanceCourts ?? 0,
                };
                return (
                  <tr key={cx.complexId} className="hover:bg-slate-800/40 transition-colors group">
                    {/* Tổ hợp */}
                    <td className="py-4 pl-6 pr-4">
                      <div className="flex items-center gap-3">
                        <div className="w-11 h-11 rounded-xl overflow-hidden flex-shrink-0 bg-indigo-500/10 border border-indigo-500/20 flex items-center justify-center">
                          {cx.imageUrl
                            ? <img src={cx.imageUrl} alt={cx.complexName} className="w-full h-full object-cover" />
                            : <Building2 className="w-5 h-5 text-indigo-400" />}
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

                    {/* Loại sân */}
                    <td className="py-4 px-4">
                      <div className="flex flex-wrap gap-1">
                        {(cx.courtTypeIds ?? []).length > 0
                          ? cx.courtTypeIds!.map((tid) => {
                              const cfg = TYPE_BADGE[tid];
                              if (!cfg) return null;
                              return (
                                <span key={tid} className={`inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold border ${cfg.color}`}>
                                  {cfg.label}
                                </span>
                              );
                            })
                          : <span className="text-xs text-slate-600 italic">—</span>
                        }
                      </div>
                    </td>

                    {/* Quản lý — fetch từ cache hoặc managerName từ API */}
                    <td className="py-4 px-4">
                      {manager ? (
                        <div className="flex items-center gap-2">
                          <img
                            src={manager.avatarUrl ?? `https://api.dicebear.com/8.x/avataaars/svg?seed=${manager.userId}`}
                            alt=""
                            className="w-6 h-6 rounded-full bg-slate-700 flex-shrink-0"
                          />
                          <div className="min-w-0">
                            <p className="text-sm text-slate-300 truncate font-medium">{manager.fullName}</p>
                            <p className="text-[10px] text-slate-600 truncate">#{manager.userId}</p>
                          </div>
                        </div>
                      ) : cx.managerName ? (
                        <div className="min-w-0">
                          <p className="text-sm text-slate-300 truncate font-medium">{cx.managerName}</p>
                          {cx.managerId && <p className="text-[10px] text-slate-600 truncate">#{cx.managerId}</p>}
                        </div>
                      ) : cx.managerId ? (
                        <span className="text-xs text-slate-600 italic">#{cx.managerId} (đang tải...)</span>
                      ) : (
                        <span className="text-xs text-slate-600 italic">— Chưa phân công</span>
                      )}
                    </td>

                    {/* Liên hệ */}
                    <td className="py-4 px-4">
                      <span className="text-sm text-slate-400">{cx.phone ?? '—'}</span>
                    </td>

                    {/* Counts */}
                    <td className="py-4 px-4 text-center">
                      <span className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-slate-700/60 border border-slate-600/40 text-sm font-bold text-slate-200">
                        {counts.total}
                      </span>
                    </td>
                    <td className="py-4 px-4 text-center">
                      {counts.active > 0
                        ? <span className="inline-flex items-center gap-1 rounded-full border border-emerald-400/30 bg-emerald-400/10 px-2.5 py-0.5 text-xs font-semibold text-emerald-400"><CheckCircle className="w-3 h-3" />{counts.active}</span>
                        : <span className="text-xs text-slate-600">—</span>}
                    </td>
                    <td className="py-4 px-4 text-center">
                      {counts.maintenance > 0
                        ? <span className="inline-flex items-center gap-1 rounded-full border border-amber-400/30 bg-amber-400/10 px-2.5 py-0.5 text-xs font-semibold text-amber-400"><Wrench className="w-3 h-3" />{counts.maintenance}</span>
                        : <span className="text-xs text-slate-600">—</span>}
                    </td>

                    {/* Thao tác */}
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
                          title="Chỉnh sửa"
                        >
                          <Edit2 className="w-3.5 h-3.5" />
                        </button>
                        <button
                          onClick={() => setModal({ kind: 'delete', complex: cx })}
                          className="p-2 rounded-lg text-slate-400 hover:text-red-400 hover:bg-red-400/10 transition-colors"
                          title="Xóa"
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
            <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}
              className="p-2 rounded-lg border border-slate-700 text-slate-400 hover:text-white hover:border-slate-500 disabled:opacity-30 disabled:cursor-not-allowed transition-colors">
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
                  <button key={item} onClick={() => setPage(item as number)}
                    className={`w-9 h-9 rounded-lg text-sm font-medium border transition-colors ${page === item ? 'bg-primary-600 border-primary-500 text-white' : 'border-slate-700 text-slate-400 hover:text-white hover:border-slate-500'}`}>
                    {item}
                  </button>
                )
              )}
            <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages}
              className="p-2 rounded-lg border border-slate-700 text-slate-400 hover:text-white hover:border-slate-500 disabled:opacity-30 disabled:cursor-not-allowed transition-colors">
              <ChevronRight className="w-4 h-4" />
            </button>
          </div>
        </div>
      )}

      {/* Modals */}
      {modal?.kind === 'create' && (
        <ComplexModal mode="create" courtTypeOptions={courtTypeOptions} onClose={() => setModal(null)} onSave={handleSave} />
      )}
      {modal?.kind === 'edit' && (
        <ComplexModal mode="edit" initial={modal.complex} courtTypeOptions={courtTypeOptions} onClose={() => setModal(null)} onSave={handleSave} />
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
