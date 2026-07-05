import { useState, useEffect } from 'react';
import { Plus, Search, Edit2, Trash2, Gift, Filter, X, Save, Loader2, RefreshCw, AlertTriangle } from 'lucide-react';
import toast from 'react-hot-toast';
import { promotionApi } from '@/api/promotionApi';
import type { Promotion, CreatePromotionRequest, UpdatePromotionRequest, DiscountType } from '@/types/promotion.types';

export default function ManagePromotionsPage() {
  const [promotions, setPromotions] = useState<Promotion[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [filterStatus, setFilterStatus] = useState<string>('all');

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<'create' | 'edit'>('create');
  const [selectedPromo, setSelectedPromo] = useState<Promotion | null>(null);
  const [saving, setSaving] = useState(false);

  // Delete confirm modal
  const [deleteTarget, setDeleteTarget] = useState<Promotion | null>(null);
  const [deleting, setDeleting] = useState(false);

  // Form State
  const [promoCode, setPromoCode] = useState('');
  const [promoName, setPromoName] = useState('');
  const [discountType, setDiscountType] = useState<DiscountType>('Percent');
  const [discountValue, setDiscountValue] = useState<number>(10);
  const [minOrderAmount, setMinOrderAmount] = useState<number>(0);
  const [maxDiscount, setMaxDiscount] = useState<number | ''>('');
  const [usageLimit, setUsageLimit] = useState<number | ''>('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [isActive, setIsActive] = useState(true);

  useEffect(() => {
    loadPromotions();
  }, []);

  const loadPromotions = async () => {
    try {
      setLoading(true);
      const data = await promotionApi.getAllPromotions();
      setPromotions(data);
    } catch (error) {
      toast.error('Lỗi khi tải danh sách khuyến mãi');
    } finally {
      setLoading(false);
    }
  };

  const openCreateModal = () => {
    setModalMode('create');
    setSelectedPromo(null);
    setPromoCode('');
    setPromoName('');
    setDiscountType('Percent');
    setDiscountValue(10);
    setMinOrderAmount(0);
    setMaxDiscount('');
    setUsageLimit('');
    
    // Default dates (today and +30 days)
    const today = new Date();
    const nextMonth = new Date();
    nextMonth.setDate(today.getDate() + 30);
    
    setStartDate(today.toISOString().split('T')[0]);
    setEndDate(nextMonth.toISOString().split('T')[0]);
    setIsActive(true);
    setIsModalOpen(true);
  };

  const openEditModal = (promo: Promotion) => {
    setModalMode('edit');
    setSelectedPromo(promo);
    setPromoCode(promo.promoCode);
    setPromoName(promo.promoName);
    setDiscountType(promo.discountType);
    setDiscountValue(promo.discountValue);
    setMinOrderAmount(promo.minOrderAmount);
    setMaxDiscount(promo.maxDiscount ?? '');
    setUsageLimit(promo.usageLimit ?? '');
    setStartDate(promo.startDate.split('T')[0]);
    setEndDate(promo.endDate.split('T')[0]);
    setIsActive(promo.isActive);
    setIsModalOpen(true);
  };

  const handleSave = async () => {
    // Validation Form
    if (!promoCode || !promoName || !startDate || !endDate) {
      toast.error('Vui lòng điền đầy đủ các trường bắt buộc');
      return;
    }
    if (new Date(endDate) < new Date(startDate)) {
      toast.error('Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu');
      return;
    }
    if (discountType === 'Percent' && (discountValue < 1 || discountValue > 100)) {
      toast.error('Phần trăm giảm phải từ 1 đến 100');
      return;
    }
    if (discountType === 'Percent' && maxDiscount === '') {
      toast.error('Bắt buộc nhập giới hạn giảm tối đa khi chọn giảm theo %');
      return;
    }
    if (discountType === 'FixedAmount' && discountValue <= 0) {
      toast.error('Số tiền giảm phải lớn hơn 0');
      return;
    }

    setSaving(true);
    try {
      if (modalMode === 'create') {
        const payload: CreatePromotionRequest = {
          promoCode: promoCode.toUpperCase(),
          promoName,
          discountType,
          discountValue: Number(discountValue),
          minOrderAmount: Number(minOrderAmount),
          maxDiscount: discountType === 'Percent' && maxDiscount !== '' ? Number(maxDiscount) : undefined,
          usageLimit: usageLimit !== '' ? Number(usageLimit) : undefined,
          startDate: new Date(startDate).toISOString(),
          endDate: new Date(endDate).toISOString(),
          isActive
        };
        await promotionApi.createPromotion(payload);
        toast.success('Tạo mã khuyến mãi thành công');
      } else {
        if (!selectedPromo) return;
        const payload: UpdatePromotionRequest = {
          promoName,
          discountType,
          discountValue: Number(discountValue),
          minOrderAmount: Number(minOrderAmount),
          maxDiscount: discountType === 'Percent' && maxDiscount !== '' ? Number(maxDiscount) : undefined,
          usageLimit: usageLimit !== '' ? Number(usageLimit) : undefined,
          startDate: new Date(startDate).toISOString(),
          endDate: new Date(endDate).toISOString(),
          isActive
        };
        await promotionApi.updatePromotion(selectedPromo.promotionId, payload);
        toast.success('Cập nhật mã khuyến mãi thành công');
      }
      setIsModalOpen(false);
      loadPromotions();
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Có lỗi xảy ra khi lưu');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await promotionApi.deletePromotion(deleteTarget.promotionId);
      toast.success(`Đã xóa mã khuyến mãi ${deleteTarget.promoCode}`);
      setDeleteTarget(null);
      loadPromotions();
    } catch (error: any) {
      toast.error(error?.response?.data?.message || 'Không thể xóa mã khuyến mãi này');
    } finally {
      setDeleting(false);
    }
  };

  const filteredPromotions = promotions.filter(p => {
    const matchSearch = p.promoCode.toLowerCase().includes(search.toLowerCase()) || p.promoName.toLowerCase().includes(search.toLowerCase());
    let matchStatus = true;
    const now = new Date();
    const end = new Date(p.endDate);
    
    if (filterStatus === 'active') {
      matchStatus = p.isActive && end >= now;
    } else if (filterStatus === 'expired') {
      matchStatus = end < now;
    } else if (filterStatus === 'inactive') {
      matchStatus = !p.isActive;
    }
    
    return matchSearch && matchStatus;
  });

  const formatVND = (amount: number) => {
    return amount.toLocaleString('vi-VN') + 'đ';
  };

  return (
    <div className="p-6 space-y-6 min-h-full">
      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-xl font-bold text-white">Quản lý Khuyến Mãi</h1>
          <p className="text-sm text-slate-500 mt-0.5">
            Tạo và quản lý các chiến dịch mã giảm giá (Promotions).
          </p>
        </div>
        <button onClick={openCreateModal} className="btn-primary flex-shrink-0">
          <Plus className="w-4 h-4" /> Tạo Mã Khuyến Mãi
        </button>
      </div>

      {/* Filter Bar */}
      <div className="flex flex-wrap items-center gap-3">
        <div className="flex-1 min-w-[200px] max-w-sm flex items-center gap-2 bg-slate-800/60 border border-surface-border rounded-xl px-4 py-2.5">
          <Search className="w-4 h-4 text-slate-500 flex-shrink-0" />
          <input
            type="text"
            placeholder="Tìm mã code, tên khuyến mãi..."
            className="flex-1 bg-transparent text-sm text-slate-300 placeholder:text-slate-600 outline-none"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          <Filter className="w-4 h-4 text-slate-500 flex-shrink-0" />
          <select 
            className="bg-slate-800/60 border border-surface-border text-slate-300 text-sm rounded-xl px-3 py-2 outline-none focus:border-primary-500"
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
          >
            <option value="all">Tất cả trạng thái</option>
            <option value="active">Đang chạy</option>
            <option value="expired">Đã hết hạn</option>
            <option value="inactive">Đã khóa</option>
          </select>
          <button onClick={loadPromotions} className="btn-secondary px-3 py-2 text-sm ml-auto">
            <RefreshCw className="w-4 h-4" /> Làm mới
          </button>
        </div>
      </div>

      {/* Table */}
      {loading ? (
        <div className="flex justify-center py-20">
          <Loader2 className="w-8 h-8 text-primary-500 animate-spin" />
        </div>
      ) : filteredPromotions.length === 0 ? (
        <div className="rounded-xl border border-dashed border-slate-700 bg-slate-800/20 py-16 text-center">
          <Gift className="w-12 h-12 text-slate-600 mx-auto mb-3" />
          <h3 className="text-base font-semibold text-slate-400 mb-1">Không tìm thấy mã khuyến mãi</h3>
          <p className="text-sm text-slate-600">Thử thay đổi từ khóa hoặc bộ lọc của bạn</p>
        </div>
      ) : (
        <div className="rounded-xl border border-surface-border bg-surface-card overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-surface-border bg-slate-800/60 text-xs text-slate-500 font-semibold uppercase tracking-wider">
                <th className="py-3 pl-6 pr-4 text-left">Mã Code</th>
                <th className="py-3 px-4 text-left">Mức giảm</th>
                <th className="py-3 px-4 text-left">Giới hạn & Đã dùng</th>
                <th className="py-3 px-4 text-left">Thời hạn</th>
                <th className="py-3 px-4 text-left">Trạng thái</th>
                <th className="py-3 pl-4 pr-6 text-center">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-surface-border/60">
              {filteredPromotions.map((promo) => {
                const now = new Date();
                const end = new Date(promo.endDate);
                const isExpired = end < now;
                const isActive = promo.isActive && !isExpired;

                return (
                  <tr key={promo.promotionId} className="hover:bg-slate-800/40 transition-colors">
                    <td className="py-4 pl-6 pr-4">
                      <div className="flex flex-col">
                        <span className="font-bold text-white uppercase text-base">{promo.promoCode}</span>
                        <span className="text-xs text-slate-400">{promo.promoName}</span>
                      </div>
                    </td>
                    <td className="py-4 px-4 text-sm text-slate-300">
                      {promo.discountType === 'Percent' ? (
                        <div>
                          <span className="font-semibold text-emerald-400">{promo.discountValue}%</span>
                          {promo.maxDiscount && <div className="text-xs text-slate-500">Tối đa {formatVND(promo.maxDiscount)}</div>}
                        </div>
                      ) : (
                        <span className="font-semibold text-emerald-400">{formatVND(promo.discountValue)}</span>
                      )}
                      {promo.minOrderAmount > 0 && <div className="text-xs text-slate-500">ĐH từ {formatVND(promo.minOrderAmount)}</div>}
                    </td>
                    <td className="py-4 px-4 text-sm text-slate-300">
                      <div>Đã dùng: <span className="font-medium">{promo.usedCount}</span></div>
                      <div className="text-xs text-slate-500">Giới hạn: {promo.usageLimit ?? 'Không giới hạn'}</div>
                    </td>
                    <td className="py-4 px-4">
                      <div className="text-sm text-slate-300">{new Date(promo.startDate).toLocaleDateString('vi-VN')}</div>
                      <div className="text-sm text-slate-300">{new Date(promo.endDate).toLocaleDateString('vi-VN')}</div>
                    </td>
                    <td className="py-4 px-4">
                      {isActive ? (
                        <span className="inline-flex px-2 py-1 rounded-full bg-emerald-500/10 text-emerald-400 text-xs font-medium border border-emerald-500/20">Đang chạy</span>
                      ) : isExpired ? (
                        <span className="inline-flex px-2 py-1 rounded-full bg-red-500/10 text-red-400 text-xs font-medium border border-red-500/20">Đã hết hạn</span>
                      ) : (
                        <span className="inline-flex px-2 py-1 rounded-full bg-slate-500/10 text-slate-400 text-xs font-medium border border-slate-500/20">Đã khóa</span>
                      )}
                    </td>
                    <td className="py-4 pl-4 pr-6 text-center">
                      <div className="flex items-center justify-center gap-1.5">
                        <button onClick={() => openEditModal(promo)} className="p-2 rounded-lg text-slate-400 hover:text-white hover:bg-slate-700 transition-colors" title="Chỉnh sửa">
                          <Edit2 className="w-3.5 h-3.5" />
                        </button>
                        <button onClick={() => setDeleteTarget(promo)} className="p-2 rounded-lg text-slate-400 hover:text-red-400 hover:bg-red-400/10 transition-colors" title="Xóa">
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

      {/* Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
          <div className="w-full max-w-lg bg-surface-card border border-surface-border rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]">
            <div className="px-6 py-4 border-b border-surface-border flex items-center justify-between bg-slate-800/60">
              <h2 className="text-base font-bold text-white">
                {modalMode === 'create' ? 'Tạo Mã Khuyến Mãi Mới' : 'Cập Nhật Mã Khuyến Mãi'}
              </h2>
              <button onClick={() => setIsModalOpen(false)} className="text-slate-400 hover:text-white p-1 rounded-lg hover:bg-slate-700 transition-colors">
                <X className="w-4 h-4" />
              </button>
            </div>

            <div className="p-6 space-y-4 overflow-y-auto">
              <div>
                <label className="block text-xs font-medium text-slate-400 mb-1.5">Mã Code (Chữ in hoa và số) *</label>
                <input 
                  type="text" 
                  className="input-field w-full uppercase" 
                  placeholder="VD: SUMMER2026"
                  value={promoCode}
                  onChange={e => setPromoCode(e.target.value)}
                  disabled={modalMode === 'edit'}
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-slate-400 mb-1.5">Tên hiển thị *</label>
                <input 
                  type="text" 
                  className="input-field w-full" 
                  placeholder="VD: Khuyến mãi chào hè"
                  value={promoName}
                  onChange={e => setPromoName(e.target.value)}
                />
              </div>

              <div className="flex gap-4">
                <div className="flex-1">
                  <label className="block text-xs font-medium text-slate-400 mb-1.5">Kiểu giảm *</label>
                  <select 
                    className="input-field w-full"
                    value={discountType}
                    onChange={e => setDiscountType(e.target.value as DiscountType)}
                  >
                    <option value="Percent">% (Phần trăm)</option>
                    <option value="FixedAmount">VNĐ (Tiền mặt)</option>
                  </select>
                </div>
                <div className="flex-1">
                  <label className="block text-xs font-medium text-slate-400 mb-1.5">Mức giảm *</label>
                  <input 
                    type="number" 
                    className="input-field w-full" 
                    value={discountValue}
                    onChange={e => setDiscountValue(Number(e.target.value))}
                    min={1}
                  />
                </div>
              </div>

              <div className="flex gap-4">
                <div className="flex-1">
                  <label className="block text-xs font-medium text-slate-400 mb-1.5">Giảm tối đa (VNĐ)</label>
                  <input 
                    type="number" 
                    className="input-field w-full" 
                    placeholder="Bỏ trống nếu không giới hạn"
                    value={maxDiscount}
                    onChange={e => setMaxDiscount(e.target.value ? Number(e.target.value) : '')}
                    disabled={discountType === 'FixedAmount'}
                  />
                </div>
                <div className="flex-1">
                  <label className="block text-xs font-medium text-slate-400 mb-1.5">Đơn tối thiểu (VNĐ)</label>
                  <input 
                    type="number" 
                    className="input-field w-full" 
                    value={minOrderAmount}
                    onChange={e => setMinOrderAmount(Number(e.target.value))}
                  />
                </div>
              </div>

              <div className="flex gap-4">
                <div className="flex-1">
                  <label className="block text-xs font-medium text-slate-400 mb-1.5">Ngày bắt đầu *</label>
                  <input 
                    type="date" 
                    className="input-field w-full" 
                    value={startDate}
                    onChange={e => setStartDate(e.target.value)}
                  />
                </div>
                <div className="flex-1">
                  <label className="block text-xs font-medium text-slate-400 mb-1.5">Ngày kết thúc *</label>
                  <input 
                    type="date" 
                    className="input-field w-full" 
                    value={endDate}
                    onChange={e => setEndDate(e.target.value)}
                  />
                </div>
              </div>

              <div className="flex gap-4">
                <div className="flex-1">
                  <label className="block text-xs font-medium text-slate-400 mb-1.5">Số lần sử dụng tối đa</label>
                  <input 
                    type="number" 
                    className="input-field w-full" 
                    placeholder="Bỏ trống nếu không giới hạn"
                    value={usageLimit}
                    onChange={e => setUsageLimit(e.target.value ? Number(e.target.value) : '')}
                  />
                </div>
                <div className="flex-1">
                  <label className="block text-xs font-medium text-slate-400 mb-1.5">Trạng thái</label>
                  <select 
                    className="input-field w-full"
                    value={isActive ? 'true' : 'false'}
                    onChange={e => setIsActive(e.target.value === 'true')}
                  >
                    <option value="true">Đang kích hoạt</option>
                    <option value="false">Khóa</option>
                  </select>
                </div>
              </div>

            </div>

            <div className="px-6 py-4 border-t border-surface-border flex justify-end gap-3 bg-slate-800/40 mt-auto">
              <button onClick={() => setIsModalOpen(false)} disabled={saving} className="btn-secondary">Hủy</button>
              <button onClick={handleSave} disabled={saving} className="btn-primary">
                {saving ? <Loader2 className="w-4 h-4 animate-spin" /> : <Save className="w-4 h-4" />}
                {modalMode === 'create' ? 'Thêm mới' : 'Lưu thay đổi'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirm Modal */}
      {deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={() => setDeleteTarget(null)} />
          <div className="relative bg-surface-card border border-surface-border rounded-2xl w-full max-w-sm shadow-2xl p-6 animate-fade-in">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-10 h-10 rounded-full bg-red-500/10 border border-red-500/30 flex items-center justify-center flex-shrink-0">
                <AlertTriangle className="w-5 h-5 text-red-400" />
              </div>
              <h3 className="text-base font-semibold text-white">Xóa mã khuyến mãi</h3>
            </div>
            <p className="text-sm text-slate-400 mb-2">
              Bạn có chắc muốn xóa mã khuyến mãi:
            </p>
            <p className="text-sm font-bold text-white mb-1">{deleteTarget.promoCode}</p>
            <p className="text-xs text-slate-500 mb-6">{deleteTarget.promoName}</p>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setDeleteTarget(null)}
                disabled={deleting}
                className="btn-secondary"
              >
                Hủy
              </button>
              <button
                onClick={handleDelete}
                disabled={deleting}
                className="flex items-center gap-2 px-4 py-2 rounded-xl bg-red-600 hover:bg-red-500 text-white text-sm font-semibold transition-colors disabled:opacity-50"
              >
                {deleting ? <Loader2 className="w-4 h-4 animate-spin" /> : <Trash2 className="w-4 h-4" />}
                Xóa
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
