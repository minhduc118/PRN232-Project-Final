import { useState, useEffect } from 'react';
import { useAuthStore } from '@/store/authStore';
import { updateProfile, changePassword, getMembershipTiers } from '@/api/authApi';
import type { MembershipTier, UpdateProfileRequest } from '@/types/auth.types';
import { 
  User, Lock, Mail, Phone, Calendar, Award, 
  Save, CheckCircle2, Shield, TrendingUp, Info, HelpCircle, 
  RefreshCw, Check, AlertCircle, X
} from 'lucide-react';
import toast from 'react-hot-toast';

export default function ProfilePage() {
  const { user, setUser } = useAuthStore();
  
  // Profile forms state
  const [fullName, setFullName] = useState('');
  const [phone, setPhone] = useState('');
  const [dateOfBirth, setDateOfBirth] = useState('');
  const [gender, setGender] = useState<'Male' | 'Female' | 'Other' | ''>('');
  const [avatarUrl, setAvatarUrl] = useState('');
  
  const [saveLoading, setSaveLoading] = useState(false);
  const [tiers, setTiers] = useState<MembershipTier[]>([]);
  const [tiersLoading, setTiersLoading] = useState(true);
  
  // Password modal state
  const [isPasswordModalOpen, setIsPasswordModalOpen] = useState(false);
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');
  const [pwdLoading, setPwdLoading] = useState(false);

  // Avatar preset options for quick selection
  const avatarPresets = [
    'https://api.dicebear.com/8.x/avataaars/svg?seed=Felix',
    'https://api.dicebear.com/8.x/avataaars/svg?seed=Aria',
    'https://api.dicebear.com/8.x/avataaars/svg?seed=George',
    'https://api.dicebear.com/8.x/avataaars/svg?seed=Jack',
    'https://api.dicebear.com/8.x/avataaars/svg?seed=Milo',
    'https://api.dicebear.com/8.x/avataaars/svg?seed=Zoe',
  ];

  // Load user data into form
  useEffect(() => {
    if (user) {
      setFullName(user.fullName || '');
      setPhone(user.phone || '');
      if (user.dateOfBirth) {
        setDateOfBirth(user.dateOfBirth.substring(0, 10));
      } else {
        setDateOfBirth('');
      }
      setGender(user.gender || '');
      setAvatarUrl(user.avatarUrl || '');
    }
  }, [user]);

  // Load membership tiers metadata
  useEffect(() => {
    async function fetchTiers() {
      try {
        const data = await getMembershipTiers();
        setTiers(data);
      } catch (err) {
        console.error('Lỗi khi tải thông tin hạng thành viên:', err);
      } finally {
        setTiersLoading(false);
      }
    }
    fetchTiers();
  }, []);

  if (!user) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-slate-950 text-slate-200">
        <AlertCircle className="w-12 h-12 text-red-500 mb-4 animate-bounce" />
        <h2 className="text-xl font-bold">Bạn chưa đăng nhập</h2>
        <p className="text-slate-400 mt-1 text-sm">Vui lòng đăng nhập để xem thông tin cá nhân.</p>
        <a href="/login" className="mt-4 btn-primary">Đăng nhập ngay</a>
      </div>
    );
  }

  // Calculate tier points requirements and progression
  const currentPoints = user.loyaltyPoints || 0;
  
  // Find current and next tier specs
  const getProgressStats = () => {
    const activeTiers = tiers.length > 0 ? tiers : [
      { tierId: 1, tierName: 'Bronze', minPoints: 0, discountPercent: 0 },
      { tierId: 2, tierName: 'Silver', minPoints: 500, discountPercent: 5 },
      { tierId: 3, tierName: 'Gold', minPoints: 2000, discountPercent: 10 },
      { tierId: 4, tierName: 'Platinum', minPoints: 5000, discountPercent: 15 }
    ];

    let currentTierIndex = 0;
    for (let i = activeTiers.length - 1; i >= 0; i--) {
      if (currentPoints >= activeTiers[i].minPoints) {
        currentTierIndex = i;
        break;
      }
    }

    const currentTier = activeTiers[currentTierIndex];
    const isMaxTier = currentTierIndex === activeTiers.length - 1;
    const nextTier = isMaxTier ? null : activeTiers[currentTierIndex + 1];

    let percent = 100;
    let pointsNeeded = 0;

    if (nextTier) {
      const tierRange = nextTier.minPoints - currentTier.minPoints;
      const currentProgress = currentPoints - currentTier.minPoints;
      percent = Math.min(100, Math.max(0, Math.floor((currentProgress / tierRange) * 100)));
      pointsNeeded = nextTier.minPoints - currentPoints;
    }

    return {
      currentTierName: currentTier.tierName,
      discountPercent: currentTier.discountPercent,
      nextTierName: nextTier?.tierName || '',
      pointsNeeded,
      percent,
      isMaxTier
    };
  };

  const progressStats = getProgressStats();

  // Handle Profile Save
  const handleSaveProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!fullName.trim()) {
      toast.error('Họ và tên không được để trống.');
      return;
    }
    
    setSaveLoading(true);
    try {
      const payload: UpdateProfileRequest = {
        fullName: fullName.trim(),
        phone: phone.trim() || undefined,
        dateOfBirth: dateOfBirth || undefined,
        gender: gender || undefined,
        avatarUrl: avatarUrl.trim() || undefined
      };
      
      const updatedUser = await updateProfile(payload);
      setUser(updatedUser);
      toast.success('Cập nhật thông tin cá nhân thành công!');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Có lỗi xảy ra khi lưu thông tin.';
      toast.error(message);
    } finally {
      setSaveLoading(false);
    }
  };

  // Handle Change Password
  const handleChangePasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!oldPassword || !newPassword || !confirmNewPassword) {
      toast.error('Vui lòng điền đầy đủ các thông tin mật khẩu.');
      return;
    }
    if (newPassword.length < 6) {
      toast.error('Mật khẩu mới phải dài tối thiểu 6 ký tự.');
      return;
    }
    if (newPassword !== confirmNewPassword) {
      toast.error('Xác nhận mật khẩu mới không khớp.');
      return;
    }

    setPwdLoading(true);
    try {
      await changePassword({
        oldPassword,
        newPassword,
        confirmNewPassword
      });
      toast.success('Đổi mật khẩu thành công!');
      setOldPassword('');
      setNewPassword('');
      setConfirmNewPassword('');
      setIsPasswordModalOpen(false);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Đổi mật khẩu thất bại.';
      toast.error(message);
    } finally {
      setPwdLoading(false);
    }
  };

  const formatDateJoined = (dateString?: string) => {
    if (!dateString) return 'Chưa rõ';
    try {
      const d = new Date(dateString);
      return `${d.getDate()}/${d.getMonth() + 1}/${d.getFullYear()}`;
    } catch {
      return dateString;
    }
  };

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
      {/* Background Glowing Blobs */}
      <div className="absolute top-1/4 left-1/4 w-[500px] h-[500px] bg-green-500/5 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-1/4 right-1/4 w-[500px] h-[500px] bg-emerald-500/5 rounded-full blur-3xl pointer-events-none" />

      <div className="max-w-7xl mx-auto relative z-10">
        
        {/* Header Breadcrumbs */}
        <div className="mb-8 animate-fade-in">
          <h1 className="text-3xl font-extrabold text-white tracking-tight bg-gradient-to-r from-white via-slate-200 to-slate-400 bg-clip-text text-transparent">
            Thông tin tài khoản & Hạng thành viên
          </h1>
          <p className="text-slate-400 text-sm mt-1">
            Quản lý thông tin hồ sơ cá nhân và theo dõi điểm thưởng, ưu đãi đặt sân của bạn.
          </p>
        </div>

        {/* Dashboard Grid */}
        <div className="grid grid-cols-12 gap-8">
          
          {/* ─────── LEFT COLUMN (Overview & Progress) ─────── */}
          <div className="col-span-12 lg:col-span-4 space-y-6 animate-slide-up">
            
            {/* User Profile Card */}
            <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 backdrop-blur-md shadow-xl text-center relative overflow-hidden group">
              <div className="absolute top-0 left-0 right-0 h-1.5 bg-gradient-to-r from-green-500 to-emerald-400" />
              
              {/* Profile Avatar Frame */}
              <div className="relative inline-block mt-4 mb-4">
                <img
                  src={avatarUrl || 'https://api.dicebear.com/8.x/avataaars/svg?seed=default'}
                  alt="Avatar"
                  className="w-28 h-28 rounded-full mx-auto border-4 border-slate-800 bg-slate-800 object-cover shadow-lg group-hover:border-green-500/30 transition-all duration-300"
                />
                <div className="absolute -bottom-1.5 -right-1.5 bg-green-500 text-slate-950 p-1.5 rounded-full shadow-lg">
                  <Shield className="w-4 h-4" />
                </div>
              </div>

              {/* Basic Details */}
              <h2 className="text-xl font-bold text-white tracking-tight">{user.fullName}</h2>
              <div className="flex items-center justify-center gap-2 mt-1.5">
                <span className="text-xs font-semibold px-2 py-0.5 rounded bg-slate-800 text-slate-300 border border-slate-700">
                  {user.role === 'Customer' ? 'Khách hàng' : user.role}
                </span>
                <span className={`badge px-2.5 py-0.5 text-[10px] uppercase font-bold tracking-wider rounded-full shadow-sm ${getTierBadgeClass(user.membershipTier)}`}>
                  {user.membershipTier || 'Bronze'}
                </span>
              </div>
              <p className="text-xs text-slate-500 mt-3 flex items-center justify-center gap-1">
                <Calendar className="w-3.5 h-3.5 text-slate-500" /> Ngày tham gia: {formatDateJoined(user.createdAt)}
              </p>

              {/* Quick Preset Avatars Picker */}
              <div className="mt-6 pt-5 border-t border-slate-800/80">
                <span className="block text-xs font-semibold text-slate-400 text-left mb-2 flex items-center gap-1.5">
                  <Info className="w-3.5 h-3.5 text-slate-400" /> Chọn ảnh đại diện nhanh:
                </span>
                <div className="flex justify-center gap-2 flex-wrap">
                  {avatarPresets.map((preset, idx) => (
                    <button
                      key={idx}
                      type="button"
                      onClick={() => setAvatarUrl(preset)}
                      className={`w-10 h-10 rounded-full border-2 overflow-hidden hover:scale-110 transition-transform ${avatarUrl === preset ? 'border-green-500 scale-105' : 'border-slate-800'}`}
                    >
                      <img src={preset} alt={`avatar-${idx}`} className="w-full h-full bg-slate-700" />
                    </button>
                  ))}
                </div>
                {/* Custom input for avatar url */}
                <div className="mt-3">
                  <input
                    type="text"
                    placeholder="Hoặc nhập URL ảnh đại diện tùy chọn..."
                    value={avatarUrl}
                    onChange={(e) => setAvatarUrl(e.target.value)}
                    className="input-field py-1.5 text-xs text-slate-300"
                  />
                </div>
              </div>
            </div>

            {/* Membership Progress Card */}
            <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 backdrop-blur-md shadow-xl">
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-2">
                  <Award className="w-5 h-5 text-green-400" />
                  <h3 className="font-bold text-white text-base">Tiến trình thăng hạng</h3>
                </div>
                <div className="text-right">
                  <span className="text-xs text-slate-400">Tích lũy</span>
                  <span className="block text-lg font-extrabold text-green-400 leading-none">{currentPoints} pts</span>
                </div>
              </div>

              {/* Progress Bar Area */}
              <div className="space-y-2 mt-4">
                <div className="h-2.5 w-full bg-slate-800 rounded-full overflow-hidden border border-slate-700/50">
                  <div 
                    className="h-full bg-gradient-to-r from-green-500 to-emerald-400 rounded-full shadow-lg shadow-green-500/20 transition-all duration-700"
                    style={{ width: `${progressStats.percent}%` }}
                  />
                </div>
                
                <div className="flex justify-between items-center text-xs text-slate-400">
                  <span>{progressStats.currentTierName}</span>
                  {!progressStats.isMaxTier && (
                    <span className="font-semibold text-green-400">{progressStats.percent}% tiến trình</span>
                  )}
                  <span>{progressStats.isMaxTier ? 'Tối đa' : progressStats.nextTierName}</span>
                </div>
              </div>

              {/* Next Tier helper text */}
              {!progressStats.isMaxTier ? (
                <div className="mt-4 p-3 bg-green-500/5 rounded-xl border border-green-500/10 flex items-start gap-2">
                  <TrendingUp className="w-4 h-4 text-green-400 mt-0.5 flex-shrink-0" />
                  <p className="text-xs text-slate-300 leading-relaxed">
                    Bạn cần tích lũy thêm <strong className="text-green-400 font-bold">{progressStats.pointsNeeded} điểm</strong> nữa để nâng cấp lên hạng thành viên <strong className="text-white">{progressStats.nextTierName}</strong>. 
                  </p>
                </div>
              ) : (
                <div className="mt-4 p-3 bg-gradient-to-r from-teal-500/10 to-indigo-500/10 rounded-xl border border-teal-500/20 flex items-start gap-2">
                  <CheckCircle2 className="w-4 h-4 text-teal-400 mt-0.5 flex-shrink-0" />
                  <p className="text-xs text-slate-300 leading-relaxed">
                    Chúc mừng! Bạn đã đạt mức hạng thành viên cao nhất (<strong className="text-teal-400 font-bold">{progressStats.currentTierName}</strong>). Bạn đang được hưởng trọn bộ ưu đãi VIP của chúng tôi.
                  </p>
                </div>
              )}

              {/* Benefits list for current tier */}
              <div className="mt-6 pt-5 border-t border-slate-800/80">
                <h4 className="text-xs font-bold uppercase tracking-wider text-slate-400 mb-3">
                  Quyền lợi hạng {progressStats.currentTierName}:
                </h4>
                <ul className="space-y-2 text-xs">
                  <li className="flex items-center gap-2 text-slate-300">
                    <Check className="w-4 h-4 text-green-400 flex-shrink-0" />
                    <span>Giảm trực tiếp <strong className="text-green-400 font-semibold">{progressStats.discountPercent}%</strong> cho mỗi lượt đặt sân.</span>
                  </li>
                  {progressStats.currentTierName === 'Bronze' && (
                    <li className="flex items-center gap-2 text-slate-400">
                      <HelpCircle className="w-4 h-4 text-slate-500 flex-shrink-0" />
                      <span>Hạng cơ bản để tích lũy điểm đặt sân.</span>
                    </li>
                  )}
                  {progressStats.currentTierName === 'Silver' && (
                    <li className="flex items-center gap-2 text-slate-300">
                      <Check className="w-4 h-4 text-green-400 flex-shrink-0" />
                      <span>Nhận các khuyến mãi mùa vụ sớm.</span>
                    </li>
                  )}
                  {progressStats.currentTierName === 'Gold' && (
                    <>
                      <li className="flex items-center gap-2 text-slate-300">
                        <Check className="w-4 h-4 text-green-400 flex-shrink-0" />
                        <span>Ưu tiên hàng đợi danh sách đặt sân giờ cao điểm.</span>
                      </li>
                      <li className="flex items-center gap-2 text-slate-300">
                        <Check className="w-4 h-4 text-green-400 flex-shrink-0" />
                        <span>Giờ vàng mở rộng ưu đãi.</span>
                      </li>
                    </>
                  )}
                  {progressStats.currentTierName === 'Platinum' && (
                    <>
                      <li className="flex items-center gap-2 text-slate-300">
                        <Check className="w-4 h-4 text-green-400 flex-shrink-0" />
                        <span>Ưu tiên đặt trước sân đến 14 ngày.</span>
                      </li>
                      <li className="flex items-center gap-2 text-slate-300">
                        <Check className="w-4 h-4 text-green-400 flex-shrink-0" />
                        <span>Dịch vụ hỗ trợ trực tuyến VIP 24/7.</span>
                      </li>
                      <li className="flex items-center gap-2 text-slate-300">
                        <Check className="w-4 h-4 text-green-400 flex-shrink-0" />
                        <span>Miễn phí 1 chai nước khoáng cho mỗi booking.</span>
                      </li>
                    </>
                  )}
                </ul>
              </div>

            </div>

          </div>

          {/* ─────── RIGHT COLUMN (Form editor) ─────── */}
          <div className="col-span-12 lg:col-span-8 space-y-6 animate-slide-up [animation-delay:150ms]">
            
            {/* Edit Profile Form */}
            <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 md:p-8 backdrop-blur-md shadow-xl">
              <div className="flex items-center justify-between pb-4 border-b border-slate-800/80 mb-6">
                <div className="flex items-center gap-2">
                  <User className="w-5 h-5 text-green-400" />
                  <h3 className="font-bold text-white text-lg">Thông tin cá nhân</h3>
                </div>
                <span className="text-xs text-slate-500 font-medium">Bảo mật thông tin của bạn</span>
              </div>

              <form onSubmit={handleSaveProfile} className="space-y-6">
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                  
                  {/* Full Name */}
                  <div className="col-span-2 md:col-span-1">
                    <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                      Họ và tên <span className="text-red-500">*</span>
                    </label>
                    <div className="relative">
                      <User className="absolute left-3.5 top-3.5 w-4 h-4 text-slate-500" />
                      <input
                        type="text"
                        value={fullName}
                        onChange={(e) => setFullName(e.target.value)}
                        placeholder="Nguyễn Văn A"
                        className="input-field pl-10"
                        required
                      />
                    </div>
                  </div>

                  {/* Email (Disabled) */}
                  <div className="col-span-2 md:col-span-1">
                    <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2 flex items-center justify-between">
                      <span>Email đăng nhập</span>
                      <span className="text-[10px] text-slate-500 normal-case font-normal">(Không thể thay đổi)</span>
                    </label>
                    <div className="relative">
                      <Mail className="absolute left-3.5 top-3.5 w-4 h-4 text-slate-650" />
                      <input
                        type="email"
                        value={user.email}
                        className="input-field pl-10 bg-slate-900/80 border-slate-850 text-slate-500 cursor-not-allowed"
                        disabled
                        readOnly
                      />
                    </div>
                  </div>

                  {/* Phone */}
                  <div className="col-span-2 md:col-span-1">
                    <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                      Số điện thoại
                    </label>
                    <div className="relative">
                      <Phone className="absolute left-3.5 top-3.5 w-4 h-4 text-slate-500" />
                      <input
                        type="tel"
                        value={phone}
                        onChange={(e) => setPhone(e.target.value)}
                        placeholder="0912345678"
                        className="input-field pl-10"
                      />
                    </div>
                  </div>

                  {/* Date of Birth */}
                  <div className="col-span-2 md:col-span-1">
                    <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                      Ngày sinh
                    </label>
                    <div className="relative">
                      <Calendar className="absolute left-3.5 top-3.5 w-4 h-4 text-slate-500" />
                      <input
                        type="date"
                        value={dateOfBirth}
                        onChange={(e) => setDateOfBirth(e.target.value)}
                        className="input-field pl-10 [color-scheme:dark]"
                      />
                    </div>
                  </div>

                  {/* Gender Select Cards */}
                  <div className="col-span-2">
                    <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-3">
                      Giới tính
                    </label>
                    <div className="grid grid-cols-3 gap-3">
                      {[
                        { key: 'Male', label: 'Nam' },
                        { key: 'Female', label: 'Nữ' },
                        { key: 'Other', label: 'Khác' }
                      ].map((item) => (
                        <button
                          key={item.key}
                          type="button"
                          onClick={() => setGender(item.key as any)}
                          className={`py-3 rounded-xl border-2 text-sm font-semibold transition-all flex items-center justify-center gap-2 ${gender === item.key ? 'border-green-500 bg-green-500/10 text-green-400 shadow-md' : 'border-slate-800 bg-slate-800/40 text-slate-400 hover:bg-slate-800'}`}
                        >
                          <span className={`w-2 h-2 rounded-full ${gender === item.key ? 'bg-green-400 scale-125' : 'bg-slate-650'} transition-transform`} />
                          {item.label}
                        </button>
                      ))}
                    </div>
                  </div>

                </div>

                {/* Save button */}
                <div className="flex justify-end pt-2">
                  <button
                    type="submit"
                    disabled={saveLoading}
                    className="btn-primary px-6 py-3 shadow-lg shadow-green-500/10 text-sm font-bold flex items-center gap-2"
                  >
                    {saveLoading ? (
                      <RefreshCw className="w-4 h-4 animate-spin" />
                    ) : (
                      <Save className="w-4 h-4" />
                    )}
                    {saveLoading ? 'Đang lưu...' : 'Lưu thay đổi'}
                  </button>
                </div>

              </form>
            </div>

            {/* Account Security Settings Card */}
            <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 md:p-8 backdrop-blur-md shadow-xl flex items-center justify-between">
              <div className="flex items-start gap-4">
                <div className="p-3 bg-red-500/10 border border-red-500/20 text-red-400 rounded-2xl">
                  <Lock className="w-6 h-6" />
                </div>
                <div>
                  <h4 className="font-bold text-white text-base">Đổi mật khẩu tài khoản</h4>
                  <p className="text-slate-400 text-xs mt-1 max-w-md">
                    Nên thay đổi mật khẩu định kỳ hoặc sử dụng mật khẩu mạnh để bảo vệ tài khoản khỏi truy cập trái phép.
                  </p>
                </div>
              </div>
              <button
                type="button"
                onClick={() => setIsPasswordModalOpen(true)}
                className="btn-secondary whitespace-nowrap text-xs font-semibold px-4 py-2.5 rounded-lg border border-slate-700 text-slate-300 hover:bg-slate-800"
              >
                Cập nhật mật khẩu
              </button>
            </div>

            {/* Membership Tiers Map / Timeline */}
            <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 md:p-8 backdrop-blur-md shadow-xl">
              <h3 className="font-bold text-white text-base mb-6 flex items-center gap-2">
                <Award className="w-5 h-5 text-amber-500" />
                Bản đồ cấp độ thành viên (Membership Map)
              </h3>
              
              {tiersLoading ? (
                <div className="flex justify-center py-6">
                  <RefreshCw className="w-6 h-6 animate-spin text-green-500" />
                </div>
              ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                  {tiers.map((tier) => {
                    const isUserCurrent = user.membershipTier?.toLowerCase() === tier.tierName.toLowerCase() || 
                                          (!user.membershipTier && tier.tierName === 'Bronze');
                    const hasPassed = currentPoints >= tier.minPoints;
                    
                    return (
                      <div 
                        key={tier.tierId} 
                        className={`relative rounded-xl p-4 border-2 transition-all flex flex-col justify-between h-40 ${isUserCurrent ? 'border-green-500/50 bg-green-500/5 shadow-md shadow-green-500/5' : 'border-slate-800 bg-slate-900/30'}`}
                      >
                        <div>
                          <div className="flex items-center justify-between mb-2">
                            <span className={`text-[10px] font-extrabold uppercase px-2 py-0.5 rounded tracking-wide ${getTierBadgeClass(tier.tierName)}`}>
                              {tier.tierName}
                            </span>
                            {isUserCurrent ? (
                              <span className="text-[10px] bg-green-500 text-slate-950 font-bold px-1.5 py-0.5 rounded animate-pulse">
                                Hiện tại
                              </span>
                            ) : hasPassed ? (
                              <CheckCircle2 className="w-4 h-4 text-green-500" />
                            ) : (
                              <Lock className="w-3.5 h-3.5 text-slate-650" />
                            )}
                          </div>
                          
                          <span className="text-[10px] font-semibold text-slate-500 uppercase tracking-wider">
                            Yêu cầu tích lũy
                          </span>
                          <span className="block text-sm font-bold text-white">
                            {tier.minPoints} điểm (pts)
                          </span>
                          
                          <p className="text-[11px] text-slate-400 mt-2 line-clamp-2 leading-relaxed">
                            {tier.tierName === 'Bronze' ? 'Thành viên cơ bản tích lũy điểm đặt sân.' : 
                             tier.tierName === 'Silver' ? 'Ưu đãi chiết khấu giảm 5% cho mỗi đặt sân.' :
                             tier.tierName === 'Gold' ? 'Ưu đãi chiết khấu giảm 10% + ưu tiên hàng đợi.' :
                             'Ưu đãi chiết khấu giảm 15% + dịch vụ VIP.'}
                          </p>
                        </div>

                        <div className="text-right pt-2 border-t border-slate-800/60 flex items-center justify-between text-xs">
                          <span className="text-[10px] text-slate-555">Giảm giá</span>
                          <strong className="text-sm text-green-400 font-extrabold">{tier.discountPercent}%</strong>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>

          </div>

        </div>

      </div>

      {/* ─────── CHANGE PASSWORD FLOATING MODAL ─────── */}
      {isPasswordModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm animate-fade-in">
          
          <div className="relative w-full max-w-md bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-2xl animate-slide-up">
            
            {/* Modal Header */}
            <div className="flex items-center justify-between pb-3 border-b border-slate-800/80 mb-5">
              <div className="flex items-center gap-2">
                <Lock className="w-4 h-4 text-red-500" />
                <h3 className="font-bold text-white text-base">Đổi mật khẩu tài khoản</h3>
              </div>
              <button 
                onClick={() => setIsPasswordModalOpen(false)}
                className="p-1 rounded-lg hover:bg-slate-800 text-slate-400 hover:text-white transition-colors"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            {/* Modal Form */}
            <form onSubmit={handleChangePasswordSubmit} className="space-y-4">
              
              {/* Current Password */}
              <div>
                <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                  Mật khẩu hiện tại
                </label>
                <div className="relative">
                  <Lock className="absolute left-3.5 top-3.5 w-4 h-4 text-slate-500" />
                  <input
                    type="password"
                    value={oldPassword}
                    onChange={(e) => setOldPassword(e.target.value)}
                    placeholder="••••••••"
                    className="input-field pl-10"
                    required
                  />
                </div>
              </div>

              {/* New Password */}
              <div>
                <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                  Mật khẩu mới
                </label>
                <div className="relative">
                  <Lock className="absolute left-3.5 top-3.5 w-4 h-4 text-slate-500" />
                  <input
                    type="password"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    placeholder="Tối thiểu 6 ký tự"
                    className="input-field pl-10"
                    required
                  />
                </div>
              </div>

              {/* Confirm New Password */}
              <div>
                <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                  Xác nhận mật khẩu mới
                </label>
                <div className="relative">
                  <Lock className="absolute left-3.5 top-3.5 w-4 h-4 text-slate-500" />
                  <input
                    type="password"
                    value={confirmNewPassword}
                    onChange={(e) => setConfirmNewPassword(e.target.value)}
                    placeholder="Nhập lại mật khẩu mới"
                    className="input-field pl-10"
                    required
                  />
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex gap-3 pt-3 border-t border-slate-800/80 mt-6">
                <button
                  type="button"
                  onClick={() => setIsPasswordModalOpen(false)}
                  className="flex-1 btn-secondary"
                  disabled={pwdLoading}
                >
                  Hủy bỏ
                </button>
                <button
                  type="submit"
                  disabled={pwdLoading}
                  className="flex-1 btn-danger flex items-center justify-center gap-2"
                >
                  {pwdLoading ? (
                    <RefreshCw className="w-4 h-4 animate-spin" />
                  ) : (
                    'Cập nhật'
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
