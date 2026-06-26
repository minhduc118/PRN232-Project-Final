import { useState, useEffect } from 'react';
import { 
  getCustomers, addCustomer, updateCustomer, 
  toggleCustomerStatus 
} from '@/api/staffApi';
import { getMembershipTiers } from '@/api/authApi';
import type { MembershipTier } from '@/types/auth.types';
import { 
  Users, Search, Plus, Edit2, 
  RefreshCw, Mail, Phone, X 
} from 'lucide-react';
import toast from 'react-hot-toast';

export default function StaffCustomersPage() {
  const [customers, setCustomers] = useState<any[]>([]);
  const [tiers, setTiers] = useState<MembershipTier[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  // Modal Form States
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [gender, setGender] = useState('Other');
  const [skillLevel, setSkillLevel] = useState('Beginner');
  const [loyaltyPoints, setLoyaltyPoints] = useState('0');
  const [membershipTierId, setMembershipTierId] = useState('1');
  const [isActive, setIsActive] = useState(true);
  const [modalSubmitting, setModalSubmitting] = useState(false);

  const fetchData = async (query?: string) => {
    try {
      setLoading(true);
      const [cData, tData] = await Promise.all([
        getCustomers(query),
        getMembershipTiers()
      ]);
      setCustomers(cData);
      setTiers(tData);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Lỗi khi tải danh sách khách hàng.';
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    fetchData(search);
  };

  // Open modal for Create Customer
  const handleOpenAddModal = () => {
    setEditingId(null);
    setFullName('');
    setEmail('');
    setPhone('');
    setPassword('');
    setGender('Other');
    setSkillLevel('Beginner');
    setLoyaltyPoints('0');
    setMembershipTierId('1');
    setIsActive(true);
    setIsModalOpen(true);
  };

  // Open modal for Edit Customer
  const handleOpenEditModal = (cust: any) => {
    setEditingId(cust.userId);
    setFullName(cust.fullName);
    setEmail(cust.email);
    setPhone(cust.phone || '');
    setPassword('');
    setGender(cust.gender || 'Other');
    setSkillLevel(cust.skillLevel || 'Beginner');
    setLoyaltyPoints(cust.loyaltyPoints.toString());
    setMembershipTierId(cust.membershipTierId?.toString() || '1');
    setIsActive(cust.isActive);
    setIsModalOpen(true);
  };

  // Submit Add or Edit Form
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!fullName.trim()) {
      toast.error('Vui lòng nhập họ tên khách hàng.');
      return;
    }
    if (!editingId && !email.trim()) {
      toast.error('Vui lòng nhập email đăng nhập.');
      return;
    }
    if (Number(loyaltyPoints) < 0) {
      toast.error('Điểm tích lũy không hợp lệ.');
      return;
    }

    setModalSubmitting(true);
    try {
      const payload = {
        fullName: fullName.trim(),
        email: email.trim(),
        phone: phone.trim() || undefined,
        password: password.trim() || undefined,
        gender,
        skillLevel,
        loyaltyPoints: Number(loyaltyPoints),
        membershipTierId: Number(membershipTierId),
        isActive
      };

      if (editingId) {
        const updated = await updateCustomer(editingId, payload);
        toast.success('Cập nhật thông tin khách hàng thành công.');
        setCustomers(prev => prev.map(x => x.userId === editingId ? updated : x));
      } else {
        const created = await addCustomer(payload);
        toast.success('Thêm khách hàng thành công.');
        setCustomers(prev => [...prev, created]);
      }
      setIsModalOpen(false);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Lưu dữ liệu khách hàng thất bại.';
      toast.error(message);
    } finally {
      setModalSubmitting(false);
    }
  };

  // Toggle active/inactive account status
  const handleToggleStatus = async (item: any) => {
    try {
      const newStatus = await toggleCustomerStatus(item.userId);
      setCustomers(prev => prev.map(x => x.userId === item.userId ? { ...x, isActive: newStatus } : x));
      toast.success(`Đã ${newStatus ? 'kích hoạt' : 'khóa'} tài khoản khách hàng thành công.`);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Không thể thay đổi trạng thái tài khoản.';
      toast.error(message);
    }
  };

  // Helper to get Tier Badge Color Classes
  const getTierBadgeClass = (tierName?: string) => {
    switch (tierName?.toLowerCase()) {
      case 'platinum':
        return 'bg-gradient-to-r from-teal-400 to-indigo-500 text-white shadow-teal-500/20';
      case 'gold':
        return 'bg-gradient-to-r from-amber-400 to-yellow-600 text-slate-950 shadow-amber-500/20 font-bold';
      case 'silver':
        return 'bg-gradient-to-r from-slate-300 to-slate-500 text-slate-900 shadow-slate-400/20';
      case 'bronze':
      default:
        return 'bg-gradient-to-r from-amber-700 to-orange-950 text-orange-100 shadow-orange-900/20';
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
              Quản lý tài khoản khách hàng
            </h1>
            <p className="text-slate-400 text-sm mt-1">
              Tra cứu thông tin khách hàng, cấp hạng ưu đãi, khóa/mở tài khoản đặt sân.
            </p>
          </div>
          <button
            onClick={handleOpenAddModal}
            className="btn-primary flex items-center gap-2 self-start sm:self-center py-2.5 px-5 shadow-lg shadow-green-500/20 font-bold"
          >
            <Plus className="w-4 h-4" /> Thêm khách hàng
          </button>
        </div>

        {/* Search and Filters Bar */}
        <form onSubmit={handleSearchSubmit} className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4 mb-6 backdrop-blur-md flex gap-4 animate-slide-up">
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-3 w-4.5 h-4.5 text-slate-500" />
            <input
              type="text"
              placeholder="Tìm theo tên khách hàng, email hoặc số điện thoại..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="input-field pl-10 py-2.5 text-sm"
            />
          </div>
          <button type="submit" className="btn-secondary px-5 py-2.5 text-xs border border-slate-700">
            Tìm kiếm
          </button>
        </form>

        {/* Data Grid Card */}
        <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 backdrop-blur-md shadow-xl animate-slide-up [animation-delay:100ms]">
          {loading ? (
            <div className="flex justify-center py-12">
              <RefreshCw className="w-8 h-8 animate-spin text-green-500" />
            </div>
          ) : customers.length === 0 ? (
            <div className="text-center py-16 text-slate-550 flex flex-col items-center justify-center">
              <Users className="w-12 h-12 text-slate-700 mb-3" />
              <p className="text-sm">Không tìm thấy tài khoản khách hàng nào khớp với tìm kiếm.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm text-left text-slate-350">
                <thead className="text-xs text-slate-400 uppercase bg-slate-900/50 border-b border-slate-850">
                  <tr>
                    <th scope="col" className="px-4 py-3">Khách Hàng</th>
                    <th scope="col" className="px-4 py-3">Liên Hệ</th>
                    <th scope="col" className="px-4 py-3 text-center">Giới Tính</th>
                    <th scope="col" className="px-4 py-3 text-center">Trình Độ</th>
                    <th scope="col" className="px-4 py-3 text-center">Hạng Thành Viên</th>
                    <th scope="col" className="px-4 py-3 text-center">Điểm Tích Lũy</th>
                    <th scope="col" className="px-4 py-3 text-center">Trạng Thái</th>
                    <th scope="col" className="px-4 py-3 text-right">Thao Tác</th>
                  </tr>
                </thead>
                <tbody>
                  {customers.map((cust) => (
                    <tr key={cust.userId} className="border-b border-slate-850 hover:bg-slate-900/20 transition-colors">
                      
                      {/* Customer Info (Avatar + Name) */}
                      <td className="px-4 py-4">
                        <div className="flex items-center gap-3">
                          <img
                            src={cust.avatarUrl || 'https://api.dicebear.com/8.x/avataaars/svg?seed=default'}
                            alt="Avatar"
                            className="w-9 h-9 rounded-full border border-slate-700 bg-slate-800 object-cover"
                          />
                          <div>
                            <span className="block font-bold text-white leading-tight">{cust.fullName}</span>
                            <span className="block text-[10px] text-slate-500">ID: {cust.userId}</span>
                          </div>
                        </div>
                      </td>

                      {/* Contact details */}
                      <td className="px-4 py-4">
                        <div className="space-y-0.5 text-xs">
                          <span className="flex items-center gap-1 text-slate-300">
                            <Mail className="w-3 h-3 text-slate-550" /> {cust.email}
                          </span>
                          {cust.phone && (
                            <span className="flex items-center gap-1 text-slate-400">
                              <Phone className="w-3 h-3 text-slate-550" /> {cust.phone}
                            </span>
                          )}
                        </div>
                      </td>

                      {/* Gender */}
                      <td className="px-4 py-4 text-center text-slate-300 font-medium">
                        {cust.gender === 'Male' ? 'Nam' : cust.gender === 'Female' ? 'Nữ' : 'Khác'}
                      </td>

                      {/* Skill level */}
                      <td className="px-4 py-4 text-center">
                        <span className="text-xs px-2 py-0.5 rounded bg-slate-800 border border-slate-750 text-slate-300 font-semibold">
                          {cust.skillLevel === 'Beginner' ? 'Mới chơi' : cust.skillLevel === 'Intermediate' ? 'Khá' : 'Chuyên nghiệp'}
                        </span>
                      </td>

                      {/* Membership tier badge */}
                      <td className="px-4 py-4 text-center">
                        <span className={`badge px-2.5 py-0.5 text-[9px] uppercase font-bold tracking-wider rounded-full shadow-sm ${getTierBadgeClass(cust.membershipTierName)}`}>
                          {cust.membershipTierName || 'Bronze'}
                        </span>
                      </td>

                      {/* Loyalty points */}
                      <td className="px-4 py-4 text-center font-bold text-white tracking-wide">
                        {cust.loyaltyPoints} pts
                      </td>

                      {/* Status Toggle switch */}
                      <td className="px-4 py-4 text-center">
                        <button
                          onClick={() => handleToggleStatus(cust)}
                          className={`w-10 h-6 inline-flex items-center rounded-full transition-colors focus:outline-none p-1 ${cust.isActive ? 'bg-green-500 justify-end' : 'bg-slate-800 justify-start border border-slate-700'}`}
                        >
                          <span className={`w-4 h-4 rounded-full shadow-md transition-transform ${cust.isActive ? 'bg-slate-950' : 'bg-slate-500'}`} />
                        </button>
                      </td>

                      {/* Action buttons */}
                      <td className="px-4 py-4 text-right">
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={() => handleOpenEditModal(cust)}
                            className="p-1.5 rounded-lg bg-slate-800 text-slate-300 hover:text-white hover:bg-slate-700 transition-colors"
                            title="Sửa thông tin khách hàng"
                          >
                            <Edit2 className="w-4 h-4" />
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

      {/* ─────── CREATE & EDIT CUSTOMER MODAL ─────── */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm animate-fade-in">
          
          <div className="relative w-full max-w-lg bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-2xl animate-slide-up">
            
            {/* Modal Header */}
            <div className="flex items-center justify-between pb-3 border-b border-slate-800/80 mb-5">
              <div className="flex items-center gap-2">
                <Users className="w-5 h-5 text-green-400" />
                <h3 className="font-bold text-white text-base">
                  {editingId ? 'Sửa thông tin khách hàng' : 'Thêm tài khoản khách hàng mới'}
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
                
                {/* Full Name */}
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                    Họ và tên <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                    placeholder="VD: Nguyễn Văn Hùng"
                    className="input-field"
                    required
                  />
                </div>

                {/* Email (Disabled when editing) */}
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                    Email đăng nhập <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="VD: customer@gmail.com"
                    className={`input-field ${editingId ? 'bg-slate-900 border-slate-850 text-slate-500 cursor-not-allowed' : ''}`}
                    disabled={!!editingId}
                    required
                  />
                </div>

                {/* Phone */}
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                    Số điện thoại
                  </label>
                  <input
                    type="tel"
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                    placeholder="VD: 0912345678"
                    className="input-field"
                  />
                </div>

                {/* Password (Only show on Create) */}
                {!editingId && (
                  <div className="col-span-2 sm:col-span-1">
                    <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2 flex justify-between">
                      <span>Mật khẩu</span>
                      <span className="text-[10px] text-slate-500 normal-case font-normal">(Mặc định: Customer@123)</span>
                    </label>
                    <input
                      type="password"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      placeholder="Nhập mật khẩu"
                      className="input-field"
                    />
                  </div>
                )}

                {/* Gender */}
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                    Giới tính
                  </label>
                  <select
                    value={gender}
                    onChange={(e) => setGender(e.target.value)}
                    className="input-field py-2.5 [color-scheme:dark]"
                  >
                    <option value="Male">Nam (Male)</option>
                    <option value="Female">Nữ (Female)</option>
                    <option value="Other">Khác (Other)</option>
                  </select>
                </div>

                {/* Skill Level */}
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                    Trình độ chơi
                  </label>
                  <select
                    value={skillLevel}
                    onChange={(e) => setSkillLevel(e.target.value)}
                    className="input-field py-2.5 [color-scheme:dark]"
                  >
                    <option value="Beginner">Mới chơi (Beginner)</option>
                    <option value="Intermediate">Khá (Intermediate)</option>
                    <option value="Advanced">Chuyên nghiệp (Advanced)</option>
                  </select>
                </div>

                {/* Loyalty points */}
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2 flex justify-between">
                    <span>Điểm tích lũy (loyalty points)</span>
                    {editingId && <span className="text-[10px] text-green-400 font-semibold leading-none mt-0.5">Tự động tính phân hạng</span>}
                  </label>
                  <input
                    type="number"
                    value={loyaltyPoints}
                    onChange={(e) => setLoyaltyPoints(e.target.value)}
                    placeholder="VD: 1250"
                    className="input-field"
                    min="0"
                    required
                  />
                </div>

                {/* Explicit Membership Tier Choice (only when adding or manual override) */}
                <div className="col-span-2 sm:col-span-1">
                  <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2 flex justify-between">
                    <span>Hạng thành viên</span>
                    <span className="text-[10px] text-slate-500 font-normal normal-case">(Tùy chỉnh ghi đè)</span>
                  </label>
                  <select
                    value={membershipTierId}
                    onChange={(e) => setMembershipTierId(e.target.value)}
                    className="input-field py-2.5 [color-scheme:dark]"
                  >
                    {tiers.map(t => (
                      <option key={t.tierId} value={t.tierId}>{t.tierName} (Giảm {t.discountPercent}%)</option>
                    ))}
                  </select>
                </div>

                {/* Account Status active toggle */}
                <div className="col-span-2 flex items-center justify-between p-3 bg-slate-800/40 border border-slate-850 rounded-xl mt-3">
                  <div>
                    <span className="block text-xs font-bold text-white">Tài khoản hoạt động</span>
                    <span className="block text-[10px] text-slate-500">Khách hàng có thể đăng nhập & đặt sân</span>
                  </div>
                  <button
                    type="button"
                    onClick={() => setIsActive(!isActive)}
                    className={`w-10 h-6 inline-flex items-center rounded-full transition-colors focus:outline-none p-1 ${isActive ? 'bg-green-500 justify-end' : 'bg-slate-800 justify-start border border-slate-700'}`}
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
