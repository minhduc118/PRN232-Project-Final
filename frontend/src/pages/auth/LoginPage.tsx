import { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { login } from '@/api/authApi';
import { Lock, Mail, ArrowRight, Activity } from 'lucide-react';
import toast from 'react-hot-toast';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const setAuth = useAuthStore((s) => s.setAuth);
  const navigate = useNavigate();
  const location = useLocation();

  // Find redirect url or go to "/"
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
      setAuth(response.accessToken, response.refreshToken, response.user);
      toast.success(`Chào mừng ${response.user.fullName} quay trở lại!`);
      navigate(from, { replace: true });
    } catch (error: unknown) {
      const message = error instanceof Error ? error.message : 'Đăng nhập thất bại';
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  // Helper to auto-fill mock credentials
  const fillMockUser = (role: 'Customer' | 'Admin') => {
    if (role === 'Customer') {
      setEmail('customer@sportcourt.vn');
      setPassword('customer123');
    } else {
      setEmail('admin@sportcourt.vn');
      setPassword('admin123');
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center px-4 relative overflow-hidden">
      {/* Dynamic Background Glowing Blobs */}
      <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-green-500/10 rounded-full blur-3xl" />
      <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-emerald-500/10 rounded-full blur-3xl" />

      <div className="w-full max-w-md bg-slate-900/80 border border-slate-800 rounded-2xl p-8 backdrop-blur-md shadow-2xl relative z-10 animate-slide-up">
        {/* Logo */}
        <div className="text-center mb-8">
          <div className="inline-flex w-12 h-12 rounded-xl bg-gradient-to-tr from-green-500 to-emerald-400 items-center justify-center shadow-lg shadow-green-500/20 mb-4">
            <Activity className="w-6 h-6 text-slate-950" />
          </div>
          <h2 className="text-2xl font-bold text-white tracking-tight">Đăng nhập tài khoản</h2>
          <p className="text-slate-400 text-sm mt-1">Hệ thống quản lý đặt sân SportsCourt</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          {/* Email input */}
          <div>
            <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
              Email đăng nhập
            </label>
            <div className="relative">
              <Mail className="absolute left-3.5 top-3.5 w-4 h-4 text-slate-500" />
              <input
                type="email"
                placeholder="customer@sportcourt.vn"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="input-field pl-10"
                required
              />
            </div>
          </div>

          {/* Password input */}
          <div>
            <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
              Mật khẩu
            </label>
            <div className="relative">
              <Lock className="absolute left-3.5 top-3.5 w-4 h-4 text-slate-500" />
              <input
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="input-field pl-10"
                required
              />
            </div>
          </div>

          {/* Submit Button */}
          <button
            type="submit"
            disabled={loading}
            className="w-full btn-primary flex items-center justify-center gap-2 py-3 rounded-lg text-base font-bold shadow-lg shadow-green-500/20"
          >
            {loading ? (
              <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
            ) : (
              <>
                Đăng nhập <ArrowRight className="w-4 h-4" />
              </>
            )}
          </button>
        </form>

        {/* Mock Accounts Quick Fill (Only in mock mode) */}
        <div className="mt-8 pt-6 border-t border-slate-800">
          <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider text-center mb-3">
            Tài khoản thử nghiệm nhanh (Mock)
          </p>
          <div className="flex gap-2">
            <button
              onClick={() => fillMockUser('Customer')}
              className="flex-1 text-xs py-2 px-3 bg-slate-800 hover:bg-slate-700 text-slate-200 border border-slate-700 rounded-lg transition-colors font-medium"
            >
              Khách hàng (Customer)
            </button>
            <button
              onClick={() => fillMockUser('Admin')}
              className="flex-1 text-xs py-2 px-3 bg-slate-800 hover:bg-slate-700 text-slate-200 border border-slate-700 rounded-lg transition-colors font-medium"
            >
              Quản trị viên (Admin)
            </button>
          </div>
        </div>

        <div className="mt-6 text-center text-sm text-slate-400">
          Chưa có tài khoản?{' '}
          <Link to="/register" className="text-green-400 hover:text-green-300 font-semibold underline">
            Đăng ký ngay
          </Link>
        </div>
      </div>
    </div>
  );
}
