import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useAuthStore } from '@/store/authStore';
import Navbar from '@/components/Navbar';
import { 
  User as UserIcon, 
  Phone, 
  Mail, 
  Lock, 
  ShieldCheck, 
  Award, 
  Loader2, 
  Camera, 
  Check, 
  Sparkles,
  Info
} from 'lucide-react';
import toast from 'react-hot-toast';

// Preset avatar seeds for user selection
const AVATAR_PRESETS = [
  'Sophia', 'Jackson', 'Emma', 'Aiden', 'Mia', 'Lucas', 'Olivia', 'Ethan', 'Bella', 'Liam'
];

// Form Validation Schemas
const profileSchema = z.object({
  fullName: z.string().min(2, 'Họ và tên phải có ít nhất 2 ký tự').max(100, 'Họ và tên quá dài'),
  phone: z.string().regex(/^(0[3|5|7|8|9])+([0-9]{8})$/, 'Số điện thoại không đúng định dạng Việt Nam (10 chữ số)'),
  avatarUrl: z.string().url('Đường dẫn ảnh đại diện không hợp lệ').or(z.string().length(0)),
});

const passwordSchema = z.object({
  currentPassword: z.string().min(1, 'Vui lòng nhập mật khẩu hiện tại'),
  newPassword: z.string().min(6, 'Mật khẩu mới phải từ 6 ký tự trở lên'),
  confirmPassword: z.string().min(1, 'Vui lòng xác nhận mật khẩu mới'),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: 'Mật khẩu xác nhận không khớp',
  path: ['confirmPassword'],
});

type ProfileFormValues = z.infer<typeof profileSchema>;
type PasswordFormValues = z.infer<typeof passwordSchema>;

export default function ProfilePage() {
  const { user, setUser } = useAuthStore();
  const [activeTab, setActiveTab] = useState<'profile' | 'security'>('profile');
  const [profileSaving, setProfileSaving] = useState(false);
  const [passwordSaving, setPasswordSaving] = useState(false);

  // User details
  const loyaltyPoints = user?.loyaltyPoints ?? 1250; // default to 1250 if not set (to show progress)
  const currentTier = user?.membershipTierName ?? 'Silver';

  // Membership calculation
  let nextTier: string;
  let pointsNeeded: number;
  let progress: number;
  let tierColor: string;

  if (loyaltyPoints < 1000) {
    nextTier = 'Silver';
    pointsNeeded = 1000 - loyaltyPoints;
    progress = (loyaltyPoints / 1000) * 100;
    tierColor = 'from-slate-700 to-slate-800 border-amber-700/20';
  } else if (loyaltyPoints < 3000) {
    nextTier = 'Gold';
    pointsNeeded = 3000 - loyaltyPoints;
    progress = ((loyaltyPoints - 1000) / (3000 - 1000)) * 100;
    tierColor = 'from-slate-300 to-slate-400 border-slate-300/30';
  } else if (loyaltyPoints < 5000) {
    nextTier = 'Platinum';
    pointsNeeded = 5000 - loyaltyPoints;
    progress = ((loyaltyPoints - 3000) / (5000 - 3000)) * 100;
    tierColor = 'from-yellow-400 to-amber-500 border-yellow-400/30';
  } else {
    nextTier = 'Max';
    pointsNeeded = 0;
    progress = 100;
    tierColor = 'from-cyan-400 to-blue-500 border-cyan-400/30';
  }

  // React Hook Form for profile
  const {
    register: registerProfile,
    handleSubmit: handleProfileSubmit,
    formState: { errors: profileErrors },
    setValue: setProfileValue,
    watch: watchProfile,
  } = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      fullName: user?.fullName || '',
      phone: user?.phone || '',
      avatarUrl: user?.avatarUrl || '',
    },
  });

  // React Hook Form for password
  const {
    register: registerPassword,
    handleSubmit: handlePasswordSubmit,
    formState: { errors: passwordErrors },
    reset: resetPasswordForm,
  } = useForm<PasswordFormValues>({
    resolver: zodResolver(passwordSchema),
  });

  const watchedAvatarUrl = watchProfile('avatarUrl');

  // Submit Profile handler
  const onProfileSubmit = async (values: ProfileFormValues) => {
    if (!user) return;
    setProfileSaving(true);
    try {
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 800));

      const updatedUser = {
        ...user,
        fullName: values.fullName,
        phone: values.phone,
        avatarUrl: values.avatarUrl,
      };

      // Update Zustand store
      setUser(updatedUser);

      // Mock save to users local database if needed
      const storedUsers = localStorage.getItem('mock_users');
      if (storedUsers) {
        const users = JSON.parse(storedUsers);
        const index = users.findIndex((u: { userId: number }) => u.userId === user.userId);
        if (index !== -1) {
          users[index] = { ...users[index], ...values };
          localStorage.setItem('mock_users', JSON.stringify(users));
        }
      }

      toast.success('Cập nhật thông tin cá nhân thành công!');
    } catch {
      toast.error('Có lỗi xảy ra, vui lòng thử lại.');
    } finally {
      setProfileSaving(false);
    }
  };

  // Submit Password handler
  const onPasswordSubmit = async (values: PasswordFormValues) => {
    setPasswordSaving(true);
    try {
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 1000));
      
      // Verification logic mockup
      if (values.currentPassword !== 'customer123') {
        toast.error('Mật khẩu hiện tại không chính xác.');
        return;
      }

      toast.success('Đổi mật khẩu thành công!');
      resetPasswordForm();
    } catch {
      toast.error('Có lỗi xảy ra khi đổi mật khẩu.');
    } finally {
      setPasswordSaving(false);
    }
  };

  const handleSelectPreset = (seed: string) => {
    const url = `https://api.dicebear.com/8.x/avataaars/svg?seed=${seed}`;
    setProfileValue('avatarUrl', url, { shouldValidate: true });
  };

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col">
      <Navbar />

      <div className="flex-1 max-w-7xl w-full mx-auto px-4 py-8 grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        {/* LEFT COLUMN: PROFILE FORM AND OPTIONS */}
        <div className="lg:col-span-2 space-y-6">
          <div className="card bg-slate-900 border-slate-800 p-6 sm:p-8">
            <h1 className="text-2xl font-bold text-white mb-2">Hồ sơ cá nhân</h1>
            <p className="text-xs text-slate-400 mb-6">Quản lý thông tin tài khoản của bạn, cập nhật ảnh đại diện và bảo mật tài khoản.</p>

            {/* Navigation Tabs */}
            <div className="flex border-b border-slate-800 mb-6">
              <button
                onClick={() => setActiveTab('profile')}
                className={`pb-3.5 px-4 text-sm font-semibold transition-all relative ${
                  activeTab === 'profile' 
                    ? 'text-green-400' 
                    : 'text-slate-400 hover:text-slate-200'
                }`}
              >
                <span className="flex items-center gap-2">
                  <UserIcon className="w-4.5 h-4.5" />
                  Thông tin cơ bản
                </span>
                {activeTab === 'profile' && (
                  <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-green-500 rounded-full" />
                )}
              </button>
              <button
                onClick={() => setActiveTab('security')}
                className={`pb-3.5 px-4 text-sm font-semibold transition-all relative ${
                  activeTab === 'security' 
                    ? 'text-green-400' 
                    : 'text-slate-400 hover:text-slate-200'
                }`}
              >
                <span className="flex items-center gap-2">
                  <Lock className="w-4.5 h-4.5" />
                  Mật khẩu & Bảo mật
                </span>
                {activeTab === 'security' && (
                  <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-green-500 rounded-full" />
                )}
              </button>
            </div>

            {/* PROFILE TAB */}
            {activeTab === 'profile' && (
              <form onSubmit={handleProfileSubmit(onProfileSubmit)} className="space-y-6">
                {/* Avatar upload section */}
                <div className="flex flex-col sm:flex-row items-center gap-6 pb-6 border-b border-slate-800/50">
                  <div className="relative group">
                    <img
                      src={watchedAvatarUrl || 'https://api.dicebear.com/8.x/avataaars/svg?seed=default'}
                      alt="Avatar preview"
                      className="w-24 h-24 rounded-full border-2 border-green-500/50 object-cover bg-slate-800"
                    />
                    <div className="absolute inset-0 bg-black/60 rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer">
                      <Camera className="w-6 h-6 text-white" />
                    </div>
                  </div>

                  <div className="flex-1 space-y-3 w-full">
                    <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider">
                      Chọn ảnh đại diện mẫu:
                    </label>
                    <div className="flex flex-wrap gap-2">
                      {AVATAR_PRESETS.map((seed) => {
                        const targetUrl = `https://api.dicebear.com/8.x/avataaars/svg?seed=${seed}`;
                        const isSelected = watchedAvatarUrl === targetUrl;
                        return (
                          <button
                            key={seed}
                            type="button"
                            onClick={() => handleSelectPreset(seed)}
                            className={`px-3 py-1.5 rounded-lg text-xs font-medium border flex items-center gap-1 transition-all ${
                              isSelected
                                ? 'bg-green-500/10 text-green-400 border-green-500/40'
                                : 'bg-slate-800 text-slate-350 border-slate-700 hover:border-slate-600 hover:text-white'
                            }`}
                          >
                            <img
                              src={targetUrl}
                              alt={seed}
                              className="w-4 h-4 rounded-full bg-slate-900"
                            />
                            {seed}
                            {isSelected && <Check className="w-3.5 h-3.5 text-green-400 ml-0.5" />}
                          </button>
                        );
                      })}
                    </div>
                  </div>
                </div>

                {/* Form Fields */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                  {/* Full Name */}
                  <div className="space-y-1.5">
                    <label className="block text-xs font-bold text-slate-400 uppercase tracking-wider">
                      Họ và Tên
                    </label>
                    <div className="relative">
                      <UserIcon className="absolute left-3.5 top-3 w-5 h-5 text-slate-500" />
                      <input
                        type="text"
                        {...registerProfile('fullName')}
                        className={`w-full bg-slate-950 border rounded-xl py-2.5 pl-11 pr-4 text-sm focus:outline-none focus:ring-1 focus:ring-green-400 transition-all ${
                          profileErrors.fullName ? 'border-red-500/60 focus:ring-red-400' : 'border-slate-800 focus:border-slate-700'
                        }`}
                        placeholder="Nguyễn Văn A"
                      />
                    </div>
                    {profileErrors.fullName && (
                      <p className="text-red-400 text-xs mt-1 font-medium">{profileErrors.fullName.message}</p>
                    )}
                  </div>

                  {/* Phone */}
                  <div className="space-y-1.5">
                    <label className="block text-xs font-bold text-slate-400 uppercase tracking-wider">
                      Số điện thoại
                    </label>
                    <div className="relative">
                      <Phone className="absolute left-3.5 top-3 w-5 h-5 text-slate-500" />
                      <input
                        type="text"
                        {...registerProfile('phone')}
                        className={`w-full bg-slate-950 border rounded-xl py-2.5 pl-11 pr-4 text-sm focus:outline-none focus:ring-1 focus:ring-green-400 transition-all ${
                          profileErrors.phone ? 'border-red-500/60 focus:ring-red-400' : 'border-slate-800 focus:border-slate-700'
                        }`}
                        placeholder="09XXXXXXXX"
                      />
                    </div>
                    {profileErrors.phone && (
                      <p className="text-red-400 text-xs mt-1 font-medium">{profileErrors.phone.message}</p>
                    )}
                  </div>

                  {/* Email (Disabled) */}
                  <div className="space-y-1.5">
                    <label className="block text-xs font-bold text-slate-400 uppercase tracking-wider">
                      Địa chỉ Email
                    </label>
                    <div className="relative opacity-65">
                      <Mail className="absolute left-3.5 top-3 w-5 h-5 text-slate-500" />
                      <input
                        type="email"
                        value={user?.email || ''}
                        disabled
                        className="w-full bg-slate-950 border border-slate-800 rounded-xl py-2.5 pl-11 pr-4 text-sm cursor-not-allowed"
                      />
                    </div>
                    <p className="text-slate-500 text-[10px]">Email là định danh tài khoản, không thể thay đổi.</p>
                  </div>

                  {/* Avatar Custom URL */}
                  <div className="space-y-1.5 col-span-1 sm:col-span-2">
                    <label className="block text-xs font-bold text-slate-400 uppercase tracking-wider">
                      Đường dẫn ảnh đại diện tùy chỉnh (URL)
                    </label>
                    <input
                      type="text"
                      {...registerProfile('avatarUrl')}
                      className={`w-full bg-slate-950 border rounded-xl py-2.5 px-4 text-sm focus:outline-none focus:ring-1 focus:ring-green-400 transition-all ${
                        profileErrors.avatarUrl ? 'border-red-500/60 focus:ring-red-400' : 'border-slate-800 focus:border-slate-700'
                      }`}
                      placeholder="https://example.com/avatar.jpg"
                    />
                    {profileErrors.avatarUrl && (
                      <p className="text-red-400 text-xs mt-1 font-medium">{profileErrors.avatarUrl.message}</p>
                    )}
                  </div>
                </div>

                <div className="flex justify-end pt-4 border-t border-slate-800/50">
                  <button
                    type="submit"
                    disabled={profileSaving}
                    className="btn-primary px-6 py-2.5 rounded-xl font-bold flex items-center justify-center gap-2 disabled:opacity-60 shadow-lg shadow-green-500/10"
                  >
                    {profileSaving ? (
                      <>
                        <Loader2 className="w-4 h-4 animate-spin text-slate-950" />
                        Đang lưu...
                      </>
                    ) : (
                      'Lưu thay đổi'
                    )}
                  </button>
                </div>
              </form>
            )}

            {/* SECURITY TAB */}
            {activeTab === 'security' && (
              <form onSubmit={handlePasswordSubmit(onPasswordSubmit)} className="space-y-6">
                <div className="bg-slate-950 border border-slate-800/80 rounded-xl p-4 flex gap-3 text-xs text-slate-400">
                  <ShieldCheck className="w-5 h-5 text-green-400 shrink-0 mt-0.5" />
                  <div>
                    <span className="font-semibold text-white block">Gợi ý tài khoản mock:</span>
                    <span className="block opacity-90 mt-0.5">
                      Mật khẩu mặc định trong hệ thống giả lập là <code className="text-green-400 font-mono px-1 bg-slate-900 rounded border border-slate-800">customer123</code>. Vui lòng sử dụng mật khẩu này để kiểm tra tính năng đổi mật khẩu.
                    </span>
                  </div>
                </div>

                <div className="space-y-5">
                  {/* Current Password */}
                  <div className="space-y-1.5">
                    <label className="block text-xs font-bold text-slate-400 uppercase tracking-wider">
                      Mật khẩu hiện tại
                    </label>
                    <div className="relative">
                      <Lock className="absolute left-3.5 top-3 w-5 h-5 text-slate-500" />
                      <input
                        type="password"
                        {...registerPassword('currentPassword')}
                        className={`w-full bg-slate-950 border rounded-xl py-2.5 pl-11 pr-4 text-sm focus:outline-none focus:ring-1 focus:ring-green-400 transition-all ${
                          passwordErrors.currentPassword ? 'border-red-500/60 focus:ring-red-400' : 'border-slate-800 focus:border-slate-700'
                        }`}
                        placeholder="••••••••"
                      />
                    </div>
                    {passwordErrors.currentPassword && (
                      <p className="text-red-400 text-xs mt-1 font-medium">{passwordErrors.currentPassword.message}</p>
                    )}
                  </div>

                  {/* New Password */}
                  <div className="space-y-1.5">
                    <label className="block text-xs font-bold text-slate-400 uppercase tracking-wider">
                      Mật khẩu mới
                    </label>
                    <div className="relative">
                      <Lock className="absolute left-3.5 top-3 w-5 h-5 text-slate-500" />
                      <input
                        type="password"
                        {...registerPassword('newPassword')}
                        className={`w-full bg-slate-950 border rounded-xl py-2.5 pl-11 pr-4 text-sm focus:outline-none focus:ring-1 focus:ring-green-400 transition-all ${
                          passwordErrors.newPassword ? 'border-red-500/60 focus:ring-red-400' : 'border-slate-800 focus:border-slate-700'
                        }`}
                        placeholder="Tối thiểu 6 ký tự"
                      />
                    </div>
                    {passwordErrors.newPassword && (
                      <p className="text-red-400 text-xs mt-1 font-medium">{passwordErrors.newPassword.message}</p>
                    )}
                  </div>

                  {/* Confirm Password */}
                  <div className="space-y-1.5">
                    <label className="block text-xs font-bold text-slate-400 uppercase tracking-wider">
                      Xác nhận mật khẩu mới
                    </label>
                    <div className="relative">
                      <Lock className="absolute left-3.5 top-3 w-5 h-5 text-slate-500" />
                      <input
                        type="password"
                        {...registerPassword('confirmPassword')}
                        className={`w-full bg-slate-950 border rounded-xl py-2.5 pl-11 pr-4 text-sm focus:outline-none focus:ring-1 focus:ring-green-400 transition-all ${
                          passwordErrors.confirmPassword ? 'border-red-500/60 focus:ring-red-400' : 'border-slate-800 focus:border-slate-700'
                        }`}
                        placeholder="Nhập lại mật khẩu mới"
                      />
                    </div>
                    {passwordErrors.confirmPassword && (
                      <p className="text-red-400 text-xs mt-1 font-medium">{passwordErrors.confirmPassword.message}</p>
                    )}
                  </div>
                </div>

                <div className="flex justify-end pt-4 border-t border-slate-800/50">
                  <button
                    type="submit"
                    disabled={passwordSaving}
                    className="btn-primary px-6 py-2.5 rounded-xl font-bold flex items-center justify-center gap-2 disabled:opacity-60 shadow-lg shadow-green-500/10"
                  >
                    {passwordSaving ? (
                      <>
                        <Loader2 className="w-4 h-4 animate-spin text-slate-950" />
                        Đang cập nhật...
                      </>
                    ) : (
                      'Thay đổi mật khẩu'
                    )}
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>

        {/* RIGHT COLUMN: MEMBERSHIP TIERS DASHBOARD */}
        <div className="space-y-6">
          {/* Main Tier Card */}
          <div className={`card p-6 bg-gradient-to-br ${tierColor} border border-slate-800 shadow-xl relative overflow-hidden flex flex-col justify-between min-h-[260px]`}>
            {/* Corner decoration glow */}
            <div className="absolute top-0 right-0 w-32 h-32 bg-white/5 rounded-full blur-2xl pointer-events-none" />

            <div>
              <div className="flex justify-between items-start">
                <div>
                  <span className="text-white/60 font-semibold text-xs tracking-wider uppercase">Hạng thành viên</span>
                  <h2 className="text-3xl font-black text-white mt-1 flex items-center gap-2">
                    <Award className="w-8 h-8 text-green-400 drop-shadow" />
                    {currentTier}
                  </h2>
                </div>
                <div className="px-2.5 py-1 rounded-full bg-slate-950/40 border border-white/10 text-xs font-bold text-white flex items-center gap-1">
                  <Sparkles className="w-3.5 h-3.5 text-green-400" />
                  Active
                </div>
              </div>

              <div className="mt-8 space-y-1">
                <span className="text-xs text-white/70 font-semibold block">Tích lũy điểm chơi:</span>
                <span className="text-2xl font-black text-white">{loyaltyPoints.toLocaleString('vi-VN')} <span className="text-sm font-normal text-white/60">điểm</span></span>
              </div>
            </div>

            <div className="space-y-2 mt-4">
              {pointsNeeded > 0 ? (
                <>
                  <div className="flex justify-between text-xs text-white/85">
                    <span>Hạng tiếp theo: <span className="font-bold text-white">{nextTier}</span></span>
                    <span>Cần thêm {pointsNeeded} điểm</span>
                  </div>
                  <div className="w-full bg-slate-950/50 rounded-full h-2.5 p-0.5 border border-white/5">
                    <div 
                      className="bg-gradient-to-r from-green-400 to-emerald-500 h-1.5 rounded-full shadow-lg shadow-green-400/20 transition-all duration-500" 
                      style={{ width: `${progress}%` }}
                    />
                  </div>
                </>
              ) : (
                <div className="bg-slate-950/30 border border-white/10 rounded-lg p-2.5 text-center text-xs text-white/90 font-medium">
                  🎉 Bạn đã đạt cấp độ thành viên cao nhất (Platinum)!
                </div>
              )}
            </div>
          </div>

          {/* Detailed Tiers list */}
          <div className="card bg-slate-900 border-slate-800 p-6 space-y-4">
            <h3 className="text-sm font-bold text-white flex items-center gap-1.5">
              <Info className="w-4.5 h-4.5 text-green-400" />
              Chi tiết các hạng & Ưu đãi
            </h3>
            
            <div className="space-y-3">
              {/* Bronze */}
              <div className={`p-3 rounded-xl border flex items-center justify-between gap-4 transition-all ${
                currentTier === 'Bronze' 
                  ? 'bg-amber-900/10 border-amber-500/30 ring-1 ring-amber-500/20' 
                  : 'bg-slate-950/30 border-slate-800/80 opacity-70'
              }`}>
                <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 rounded-lg bg-amber-700/20 flex items-center justify-center font-bold text-amber-500 border border-amber-600/30">B</div>
                  <div>
                    <span className="font-bold text-sm block text-white">Đồng (Bronze)</span>
                    <span className="text-[10px] text-slate-400 block">Dưới 1,000 điểm</span>
                  </div>
                </div>
                <div className="text-right">
                  <span className="text-xs font-semibold text-slate-350 block">Ưu đãi</span>
                  <span className="font-bold text-sm text-green-400">Giảm 0%</span>
                </div>
              </div>

              {/* Silver */}
              <div className={`p-3 rounded-xl border flex items-center justify-between gap-4 transition-all ${
                currentTier === 'Silver' 
                  ? 'bg-slate-800/20 border-slate-400/30 ring-1 ring-slate-400/20 shadow-lg shadow-slate-500/5' 
                  : 'bg-slate-950/30 border-slate-800/80 opacity-70'
              }`}>
                <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 rounded-lg bg-slate-500/20 flex items-center justify-center font-bold text-slate-400 border border-slate-500/30">S</div>
                  <div>
                    <span className="font-bold text-sm block text-white">Bạc (Silver)</span>
                    <span className="text-[10px] text-slate-400 block">1,000 - 2,999 điểm</span>
                  </div>
                </div>
                <div className="text-right">
                  <span className="text-xs font-semibold text-slate-350 block">Ưu đãi</span>
                  <span className="font-bold text-sm text-green-400">Giảm 5%</span>
                </div>
              </div>

              {/* Gold */}
              <div className={`p-3 rounded-xl border flex items-center justify-between gap-4 transition-all ${
                currentTier === 'Gold' 
                  ? 'bg-yellow-500/10 border-yellow-500/30 ring-1 ring-yellow-500/20' 
                  : 'bg-slate-950/30 border-slate-800/80 opacity-70'
              }`}>
                <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 rounded-lg bg-yellow-400/25 flex items-center justify-center font-bold text-yellow-400 border border-yellow-400/30">G</div>
                  <div>
                    <span className="font-bold text-sm block text-white">Vàng (Gold)</span>
                    <span className="text-[10px] text-slate-400 block">3,000 - 4,999 điểm</span>
                  </div>
                </div>
                <div className="text-right">
                  <span className="text-xs font-semibold text-slate-350 block">Ưu đãi</span>
                  <span className="font-bold text-sm text-green-400">Giảm 10%</span>
                </div>
              </div>

              {/* Platinum */}
              <div className={`p-3 rounded-xl border flex items-center justify-between gap-4 transition-all ${
                currentTier === 'Platinum' 
                  ? 'bg-cyan-500/10 border-cyan-400/35 ring-1 ring-cyan-400/25' 
                  : 'bg-slate-950/30 border-slate-800/80 opacity-70'
              }`}>
                <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 rounded-lg bg-cyan-400/20 flex items-center justify-center font-bold text-cyan-400 border border-cyan-400/30">P</div>
                  <div>
                    <span className="font-bold text-sm block text-white">Kim cương (Platinum)</span>
                    <span className="text-[10px] text-slate-400 block">Trên 5,000 điểm</span>
                  </div>
                </div>
                <div className="text-right">
                  <span className="text-xs font-semibold text-slate-350 block">Ưu đãi</span>
                  <span className="font-bold text-sm text-green-400">Giảm 15%</span>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>
  );
}
