import { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { login } from '@/api/authApi';
import { Lock, Mail, ArrowRight, Activity, Eye, EyeOff } from 'lucide-react';
import toast from 'react-hot-toast';

export default function LoginPage() {
  const [email, setEmail]               = useState('');
  const [password, setPassword]         = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading]           = useState(false);

  const setAuth  = useAuthStore((s) => s.setAuth);
  const navigate = useNavigate();
  const location = useLocation();

  // Redirect back to originally requested page after login
  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || !password) {
      toast.error('Vui lòng điền đầy đủ email và mật khẩu.');
      return;
    }
    setLoading(true);
    try {
      const response = await login({ email, password });

      // Persist tokens to localStorage AND Zustand store
      localStorage.setItem('accessToken',  response.accessToken);
      localStorage.setItem('refreshToken', response.refreshToken);
      setAuth(response.accessToken, response.refreshToken, response.user);

      toast.success(`Chào mừng ${response.user.fullName} quay trở lại! 🎉`);

      // Role-based redirect
      const role = response.user.role;
      if (role === 'Admin' || role === 'Staff' || role === 'Coach') {
        navigate('/admin', { replace: true });
      } else {
        navigate(from, { replace: true });
      }
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Đăng nhập thất bại';
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  /** Quick-fill helpers for development / demo */
  const fillQuickLogin = (role: 'Customer' | 'Admin' | 'Staff') => {
    const accounts = {
      Customer: { email: 'customer@gmail.com',      password: 'Customer@123' },
      Admin:    { email: 'admin@sportscourtms.vn',  password: 'Admin@123'    },
      Staff:    { email: 'staff@sportscourtms.vn',  password: 'Staff@123'    },
    };
    setEmail(accounts[role].email);
    setPassword(accounts[role].password);
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center px-4 relative overflow-hidden">
      {/* Background blobs */}
      <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-green-500/10 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-emerald-500/10 rounded-full blur-3xl pointer-events-none" />

      <div className="w-full max-w-md bg-slate-900/80 border border-slate-800 rounded-2xl p-8 backdrop-blur-md shadow-2xl relative z-10 animate-slide-up">

        {/* Logo */}
        <div className="text-center mb-8">
          <div className="inline-flex w-12 h-12 rounded-xl bg-gradient-to-tr from-green-500 to-emerald-400 items-center justify-center shadow-lg shadow-green-500/20 mb-4">
            <Activity className="w-6 h-6 text-slate-950" />
          </div>
          <h1 className="text-2xl font-bold text-white tracking-tight">Đăng nhập tài khoản</h1>
          <p className="text-slate-400 text-sm mt-1">Hệ thống quản lý đặt sân SportsCourt</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">

          {/* Email */}
          <div>
            <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
              Email đăng nhập
            </label>
            <div className="relative">
              <Mail className="absolute left-3.5 top-3.5 w-4 h-4 text-slate-500" />
              <input
                id="login-email"
                type="email"
                placeholder="example@gmail.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="input-field pl-10"
                required
                autoComplete="email"
              />
            </div>
          </div>

          {/* Password */}
          <div>
            <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
              Mật khẩu
            </label>
            <div className="relative">
              <Lock className="absolute left-3.5 top-3.5 w-4 h-4 text-slate-500" />
              <input
                id="login-password"
                type={showPassword ? 'text' : 'password'}
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="input-field pl-10 pr-11"
                required
                autoComplete="current-password"
              />
              <button
                type="button"
                onClick={() => setShowPassword((v) => !v)}
                className="absolute right-3.5 top-3.5 text-slate-500 hover:text-slate-300 transition-colors"
                aria-label="Toggle password visibility"
              >
                {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
              </button>
            </div>
          </div>

          {/* Submit */}
          <button
            id="login-submit"
            type="submit"
            disabled={loading}
            className="w-full btn-primary flex items-center justify-center gap-2 py-3 rounded-lg text-base font-bold shadow-lg shadow-green-500/20"
          >
            {loading ? (
              <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
            ) : (
              <>Đăng nhập <ArrowRight className="w-4 h-4" /></>
            )}
          </button>
        </form>

        {/* Quick-fill Demo Accounts */}
        <div className="mt-7 pt-6 border-t border-slate-800">
          <p className="text-xs font-semibold text-slate-500 uppercase tracking-wider text-center mb-3">
            Tài khoản thử nghiệm nhanh
          </p>
          <div className="grid grid-cols-3 gap-2">
            {(['Customer', 'Admin', 'Staff'] as const).map((role) => (
              <button
                key={role}
                type="button"
                onClick={() => fillQuickLogin(role)}
                className="text-xs py-2 px-2 bg-slate-800 hover:bg-slate-700 text-slate-300 border border-slate-700 rounded-lg transition-colors font-medium"
              >
                {role === 'Customer' ? '👤 Khách hàng' : role === 'Admin' ? '🛡️ Admin' : '👷 Nhân viên'}
              </button>
            ))}
          </div>
        </div>

        {/* Footer links */}
        <div className="mt-5 text-center text-sm text-slate-400 space-y-2">
          <div>
            Chưa có tài khoản?{' '}
            <Link to="/register" className="text-green-400 hover:text-green-300 font-semibold underline underline-offset-2">
              Đăng ký ngay
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
