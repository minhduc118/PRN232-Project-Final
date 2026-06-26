import { useState, useEffect } from 'react';
import { 
  getEquipment, addEquipment, updateEquipment, 
  deleteEquipment, getServices 
} from '@/api/staffApi';
import { 
  Package, Search, Plus, Edit2, Trash2, 
  RefreshCw, Calendar, X 
} from 'lucide-react';
import toast from 'react-hot-toast';

export default function StaffEquipmentPage() {
  const [equipmentList, setEquipmentList] = useState<any[]>([]);
  const [servicesList, setServicesList] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [conditionFilter, setConditionFilter] = useState('All');

  // Modal form states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  
  const [itemCode, setItemCode] = useState('');
  const [serviceId, setServiceId] = useState('');
  const [condition, setCondition] = useState('Good');
  const [purchaseDate, setPurchaseDate] = useState('');
  const [purchasePrice, setPurchasePrice] = useState('');
  const [isAvailable, setIsAvailable] = useState(true);
  const [modalSubmitting, setModalSubmitting] = useState(false);

  // Fetch initial data
  const fetchData = async () => {
    try {
      setLoading(true);
      const [eqData, svData] = await Promise.all([getEquipment(), getServices()]);
      setEquipmentList(eqData);
      setServicesList(svData);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Có lỗi xảy ra khi tải dữ liệu.';
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  // Format currency
  const formatPrice = (value: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  };

  // Format Date string
  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Chưa rõ';
    try {
      const d = new Date(dateString);
      return `${d.getDate()}/${d.getMonth() + 1}/${d.getFullYear()}`;
    } catch {
      return dateString;
    }
  };

  // Handle delete
  const handleDelete = async (id: number) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa dụng cụ này?')) return;
    try {
      await deleteEquipment(id);
      toast.success('Xóa dụng cụ thành công.');
      setEquipmentList(prev => prev.filter(x => x.inventoryId !== id));
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Xóa dụng cụ thất bại.';
      toast.error(message);
    }
  };

  // Open modal for Create
  const handleOpenAddModal = () => {
    setEditingId(null);
    setItemCode('');
    setServiceId(servicesList[0]?.serviceId?.toString() || '');
    setCondition('Good');
    setPurchaseDate(new Date().toISOString().substring(0, 10));
    setPurchasePrice('');
    setIsAvailable(true);
    setIsModalOpen(true);
  };

  // Open modal for Edit
  const handleOpenEditModal = (item: any) => {
    setEditingId(item.inventoryId);
    setItemCode(item.itemCode);
    setServiceId(item.serviceId.toString());
    setCondition(item.condition);
    setPurchaseDate(item.purchaseDate.substring(0, 10));
    setPurchasePrice(item.purchasePrice.toString());
    setIsAvailable(item.isAvailable);
    setIsModalOpen(true);
  };

  // Handle Form Submit (Add or Edit)
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!itemCode.trim()) {
      toast.error('Vui lòng nhập mã dụng cụ.');
      return;
    }
    if (!serviceId) {
      toast.error('Vui lòng chọn dịch vụ cho thuê tương ứng.');
      return;
    }
    if (!purchasePrice || Number(purchasePrice) < 0) {
      toast.error('Vui lòng nhập giá mua hợp lệ.');
      return;
    }

    setModalSubmitting(true);
    try {
      const payload = {
        serviceId: Number(serviceId),
        itemCode: itemCode.trim().toUpperCase(),
        condition,
        purchaseDate: new Date(purchaseDate).toISOString(),
        purchasePrice: Number(purchasePrice),
        isAvailable
      };

      if (editingId) {
        const updated = await updateEquipment(editingId, payload);
        toast.success('Cập nhật dụng cụ thành công.');
        setEquipmentList(prev => prev.map(x => x.inventoryId === editingId ? updated : x));
      } else {
        const created = await addEquipment(payload);
        toast.success('Thêm dụng cụ mới thành công.');
        setEquipmentList(prev => [...prev, created]);
      }
      setIsModalOpen(false);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Lưu dữ liệu thất bại.';
      toast.error(message);
    } finally {
      setModalSubmitting(false);
    }
  };

  // Toggle IsAvailable fast from grid
  const handleToggleAvailable = async (item: any) => {
    try {
      const payload = {
        serviceId: item.serviceId,
        itemCode: item.itemCode,
        condition: item.condition,
        purchaseDate: item.purchaseDate,
        purchasePrice: item.purchasePrice,
        isAvailable: !item.isAvailable
      };
      const updated = await updateEquipment(item.inventoryId, payload);
      setEquipmentList(prev => prev.map(x => x.inventoryId === item.inventoryId ? updated : x));
      toast.success(`Đã chuyển trạng thái sang: ${updated.isAvailable ? 'Sẵn sàng' : 'Không sẵn sàng'}`);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Không thể cập nhật trạng thái.';
      toast.error(message);
    }
  };

  // Filter list
  const filteredList = equipmentList.filter(item => {
    const matchesSearch = item.itemCode.toLowerCase().includes(search.toLowerCase()) || 
                          item.serviceName.toLowerCase().includes(search.toLowerCase());
    const matchesCondition = conditionFilter === 'All' || item.condition === conditionFilter;
    return matchesSearch && matchesCondition;
  });

  // Get condition badge style
  const getConditionStyle = (cond: string) => {
    switch (cond.toLowerCase()) {
      case 'good':
        return 'bg-green-500/10 text-green-400 border border-green-500/20';
      case 'damaged':
        return 'bg-yellow-500/10 text-yellow-400 border border-yellow-500/20';
      case 'retired':
        return 'bg-red-500/10 text-red-400 border border-red-500/20';
      default:
        return 'bg-slate-800 text-slate-400 border border-slate-700';
    }
  };

  const getConditionText = (cond: string) => {
    switch (cond.toLowerCase()) {
      case 'good': return 'Hoạt động tốt';
      case 'damaged': return 'Bị hỏng/Sửa';
      case 'retired': return 'Thanh lý';
      default: return cond;
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 py-10 px-4 md:px-8 relative overflow-hidden">
      {/* Background Blurs */}
      <div className="absolute top-1/4 left-1/4 w-[500px] h-[500px] bg-green-500/5 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-1/4 right-1/4 w-[500px] h-[500px] bg-emerald-500/5 rounded-full blur-3xl pointer-events-none" />

      <div className="max-w-7xl mx-auto relative z-10">
        
        {/* Header section */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-8 gap-4 animate-fade-in">
          <div>
            <h1 className="text-3xl font-extrabold text-white tracking-tight bg-gradient-to-r from-white via-slate-200 to-slate-400 bg-clip-text text-transparent">
              Quản lý kho dụng cụ (Equipment)
            </h1>
            <p className="text-slate-400 text-sm mt-1">
              Quản lý danh sách, số lượng, ngày nhập mua và tình trạng bảo trì của dụng cụ thuê trong tổ hợp.
            </p>
          </div>
          <button
            onClick={handleOpenAddModal}
            className="btn-primary flex items-center gap-2 self-start sm:self-center py-2.5 px-5 shadow-lg shadow-green-500/20 font-bold"
          >
            <Plus className="w-4 h-4" /> Thêm dụng cụ
          </button>
        </div>

        {/* Search and Filters Bar */}
        <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 mb-6 backdrop-blur-md flex flex-col md:flex-row md:items-center justify-between gap-4 animate-slide-up">
          
          {/* Search Input */}
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-3 w-4.5 h-4.5 text-slate-500" />
            <input
              type="text"
              placeholder="Tìm theo mã dụng cụ hoặc loại vợt, bóng..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="input-field pl-10 py-2.5 text-sm"
            />
          </div>

          {/* Condition Filter */}
          <div className="flex items-center gap-2">
            <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">Tình trạng:</span>
            <div className="flex bg-slate-800/80 rounded-lg p-0.5 border border-slate-700">
              {['All', 'Good', 'Damaged', 'Retired'].map((cond) => (
                <button
                  key={cond}
                  onClick={() => setConditionFilter(cond)}
                  className={`px-3 py-1.5 text-xs font-bold rounded-md transition-colors ${conditionFilter === cond ? 'bg-green-500 text-slate-950' : 'text-slate-400 hover:text-white'}`}
                >
                  {cond === 'All' ? 'Tất cả' : getConditionText(cond)}
                </button>
              ))}
            </div>
          </div>

        </div>

        {/* Data Grid Card */}
        <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 backdrop-blur-md shadow-xl animate-slide-up [animation-delay:100ms]">
          {loading ? (
            <div className="flex justify-center py-12">
              <RefreshCw className="w-8 h-8 animate-spin text-green-500" />
            </div>
          ) : filteredList.length === 0 ? (
            <div className="text-center py-16 text-slate-550 flex flex-col items-center justify-center">
              <Package className="w-12 h-12 text-slate-700 mb-3" />
              <p className="text-sm">Không tìm thấy thiết bị dụng cụ nào khớp với bộ lọc tìm kiếm.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm text-left text-slate-350">
                <thead className="text-xs text-slate-400 uppercase bg-slate-900/50 border-b border-slate-850">
                  <tr>
                    <th scope="col" className="px-4 py-3">Mã Dụng Cụ</th>
                    <th scope="col" className="px-4 py-3">Loại Dịch Vụ</th>
                    <th scope="col" className="px-4 py-3 text-center">Tình Trạng</th>
                    <th scope="col" className="px-4 py-3">Ngày Mua</th>
                    <th scope="col" className="px-4 py-3">Giá Mua</th>
                    <th scope="col" className="px-4 py-3 text-center">Cho Thuê</th>
                    <th scope="col" className="px-4 py-3 text-right">Thao Tác</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredList.map((item) => (
                    <tr key={item.inventoryId} className="border-b border-slate-850 hover:bg-slate-900/20 transition-colors">
                      <td className="px-4 py-4 font-bold text-white tracking-wide">{item.itemCode}</td>
                      <td className="px-4 py-4 font-medium text-slate-200">{item.serviceName}</td>
                      <td className="px-4 py-4 text-center">
                        <span className={`badge px-2.5 py-0.5 text-[10px] uppercase font-bold rounded-full ${getConditionStyle(item.condition)}`}>
                          {getConditionText(item.condition)}
                        </span>
                      </td>
                      <td className="px-4 py-4 text-slate-400">{formatDate(item.purchaseDate)}</td>
                      <td className="px-4 py-4 font-medium text-slate-300">{formatPrice(item.purchasePrice)}</td>
                      <td className="px-4 py-4 text-center">
                        <button
                          onClick={() => handleToggleAvailable(item)}
                          className={`w-10 h-6 inline-flex items-center rounded-full transition-colors focus:outline-none p-1 ${item.isAvailable ? 'bg-green-500 justify-end' : 'bg-slate-800 justify-start border border-slate-700'}`}
                        >
                          <span className={`w-4 h-4 rounded-full shadow-md transition-transform ${item.isAvailable ? 'bg-slate-950' : 'bg-slate-500'}`} />
                        </button>
                      </td>
                      <td className="px-4 py-4 text-right">
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={() => handleOpenEditModal(item)}
                            className="p-1.5 rounded-lg bg-slate-800 text-slate-300 hover:text-white hover:bg-slate-700 transition-colors"
                            title="Sửa thông tin"
                          >
                            <Edit2 className="w-4 h-4" />
                          </button>
                          <button
                            onClick={() => handleDelete(item.inventoryId)}
                            className="p-1.5 rounded-lg bg-red-500/10 text-red-400 hover:text-white hover:bg-red-650 transition-colors"
                            title="Xóa dụng cụ"
                          >
                            <Trash2 className="w-4 h-4" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

      </div>

      {/* ─────── CREATE & EDIT MODAL ─────── */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm animate-fade-in">
          
          <div className="relative w-full max-w-lg bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-2xl animate-slide-up">
            
            {/* Modal Header */}
            <div className="flex items-center justify-between pb-3 border-b border-slate-800/80 mb-5">
              <div className="flex items-center gap-2">
                <Package className="w-5 h-5 text-green-400" />
                <h3 className="font-bold text-white text-base">
                  {editingId ? 'Sửa thông tin dụng cụ' : 'Thêm dụng cụ mới'}
                </h3>
              </div>
              <button 
                onClick={() => setIsModalOpen(false)}
                className="p-1 rounded-lg hover:bg-slate-800 text-slate-400 hover:text-white transition-colors"
              >
                <X className="w-4.5 h-4.5" />
              </button>
            </div>

            {/* Modal Form */}
            <form onSubmit={handleSubmit} className="space-y-4">
              
              <div className="grid grid-cols-2 gap-4">
                
                {/* Item Code */}
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                    Mã dụng cụ <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    value={itemCode}
                    onChange={(e) => setItemCode(e.target.value)}
                    placeholder="VD: CL-V01, TN-B20"
                    className="input-field"
                    required
                  />
                </div>

                {/* Service mapping ID */}
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                    Nhóm dịch vụ liên kết <span className="text-red-500">*</span>
                  </label>
                  <select
                    value={serviceId}
                    onChange={(e) => setServiceId(e.target.value)}
                    className="input-field py-2.5 [color-scheme:dark]"
                    required
                  >
                    <option value="" disabled>-- Chọn dịch vụ --</option>
                    {servicesList.map(s => (
                      <option key={s.serviceId} value={s.serviceId}>{s.serviceName} ({s.category})</option>
                    ))}
                  </select>
                </div>

                {/* Condition */}
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                    Tình trạng
                  </label>
                  <select
                    value={condition}
                    onChange={(e) => setCondition(e.target.value)}
                    className="input-field py-2.5 [color-scheme:dark]"
                  >
                    <option value="Good">Hoạt động tốt (Good)</option>
                    <option value="Damaged">Bảo trì/Hỏng (Damaged)</option>
                    <option value="Retired">Thanh lý (Retired)</option>
                  </select>
                </div>

                {/* Purchase Date */}
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                    Ngày mua nhập kho
                  </label>
                  <div className="relative">
                    <Calendar className="absolute left-3 top-3 w-4.5 h-4.5 text-slate-500" />
                    <input
                      type="date"
                      value={purchaseDate}
                      onChange={(e) => setPurchaseDate(e.target.value)}
                      className="input-field pl-10 [color-scheme:dark]"
                      required
                    />
                  </div>
                </div>

                {/* Purchase Price */}
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                    Giá nhập mua (VNĐ) <span className="text-red-500">*</span>
                  </label>
                  <div className="relative">
                    <span className="absolute left-3 top-3 text-slate-500 text-sm font-semibold">đ</span>
                    <input
                      type="number"
                      value={purchasePrice}
                      onChange={(e) => setPurchasePrice(e.target.value)}
                      placeholder="VD: 450000"
                      className="input-field pl-7"
                      min="0"
                      required
                    />
                  </div>
                </div>

                {/* Is Available Toggle */}
                <div className="col-span-2 sm:col-span-1 flex items-center justify-between p-3 bg-slate-800/40 border border-slate-850 rounded-xl mt-3">
                  <div>
                    <span className="block text-xs font-bold text-white">Sẵn sàng cho thuê</span>
                    <span className="block text-[10px] text-slate-500">Khách có thể đặt khi đặt sân</span>
                  </div>
                  <button
                    type="button"
                    onClick={() => setIsAvailable(!isAvailable)}
                    className={`w-10 h-6 inline-flex items-center rounded-full transition-colors focus:outline-none p-1 ${isAvailable ? 'bg-green-500 justify-end' : 'bg-slate-800 justify-start border border-slate-700'}`}
                  >
                    <span className="w-4 h-4 rounded-full bg-slate-950 shadow-md" />
                  </button>
                </div>

              </div>

              {/* Action Buttons */}
              <div className="flex gap-3 pt-3 border-t border-slate-800/80 mt-6">
                <button
                  type="button"
                  onClick={() => setIsModalOpen(false)}
                  className="flex-1 btn-secondary"
                  disabled={modalSubmitting}
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={modalSubmitting}
                  className="flex-1 btn-primary flex items-center justify-center gap-2"
                >
                  {modalSubmitting ? (
                    <RefreshCw className="w-4 h-4 animate-spin" />
                  ) : (
                    'Lưu thông tin'
                  )}
                </button>
              </div>

            </form>

          </div>

        </div>
      )}

    </div>
  );
}
