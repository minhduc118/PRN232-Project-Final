import { Link, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { Calendar, User, LogOut, ChevronDown, Bell } from 'lucide-react';
import { useState } from 'react';
import toast from 'react-hot-toast';

export default function Navbar() {
  const { user, logout, isAuthenticated } = useAuthStore();
  const navigate = useNavigate();
  const [dropdownOpen, setDropdownOpen] = useState(false);

  const handleLogout = () => {
    logout();
    toast.success('Đăng xuất thành công');
    navigate('/login');
  };

  return (
    <nav className="bg-slate-900 border-b border-slate-800 sticky top-0 z-50 px-6 py-4">
      <div className="max-w-7xl mx-auto flex items-center justify-between">
        {/* Brand Logo */}
        <Link to="/" className="flex items-center gap-2 group">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-green-500 to-emerald-400 flex items-center justify-center shadow-lg shadow-green-500/20 group-hover:scale-105 transition-transform duration-200">
            <span className="text-xl font-bold text-slate-950">S</span>
          </div>
          <div>
            <span className="text-lg font-bold text-white tracking-tight group-hover:text-green-400 transition-colors">
              SportsCourt
            </span>
            <span className="block text-[10px] text-slate-400 -mt-1 font-medium tracking-wider uppercase">
              Management
            </span>
          </div>
        </Link>

        {/* Navigation Links */}
        <div className="hidden md:flex items-center gap-6">
          <Link to="/" className="text-sm font-medium text-slate-300 hover:text-white hover:underline transition-all">
            Trang chủ
          </Link>
          <Link to="/courts" className="text-sm font-medium text-slate-300 hover:text-white hover:underline transition-all">
            Danh sách sân
          </Link>
          {isAuthenticated && (
            <Link to="/my-bookings" className="text-sm font-medium text-slate-300 hover:text-white hover:underline transition-all flex items-center gap-1.5">
              <Calendar className="w-4 h-4 text-green-400" />
              Đặt sân của tôi
            </Link>
          )}
        </div>

        {/* Action Panel */}
        <div className="flex items-center gap-4">
          {isAuthenticated ? (
            <>
              {/* Notification icon */}
              <button 
                onClick={() => navigate('/notifications')}
                className="relative p-2 rounded-lg bg-slate-800 text-slate-300 hover:text-white hover:bg-slate-700 transition-colors"
              >
                <Bell className="w-5 h-5" />
                <span className="absolute top-1.5 right-1.5 w-2 h-2 rounded-full bg-green-500" />
              </button>

              {/* User Dropdown */}
              <div className="relative">
                <button
                  onClick={() => setDropdownOpen(!dropdownOpen)}
                  className="flex items-center gap-2 p-1.5 rounded-lg hover:bg-slate-800 transition-colors"
                >
                  <img
                    src={user?.avatarUrl || 'https://api.dicebear.com/8.x/avataaars/svg?seed=default'}
                    alt="avatar"
                    className="w-8 h-8 rounded-full border border-slate-700"
                  />
                  <div className="hidden sm:block text-left">
                    <span className="block text-xs font-semibold text-white leading-tight">
                      {user?.fullName}
                    </span>
                    <span className="block text-[10px] text-green-400 leading-none">
                      {user?.membershipTierName || 'Thành viên'}
                    </span>
                  </div>
                  <ChevronDown className="w-4 h-4 text-slate-400" />
                </button>

                {dropdownOpen && (
                  <div className="absolute right-0 mt-2 w-48 rounded-xl bg-slate-850 border border-slate-700 p-2 shadow-2xl animate-fade-in">
                    <button
                      onClick={() => {
                        setDropdownOpen(false);
                        navigate('/profile');
                      }}
                      className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-sm text-slate-300 hover:bg-slate-800 hover:text-white transition-colors"
                    >
                      <User className="w-4 h-4" />
                      Thông tin cá nhân
                    </button>
                    <div className="my-1 border-t border-slate-800" />
                    <button
                      onClick={() => {
                        setDropdownOpen(false);
                        handleLogout();
                      }}
                      className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-sm text-red-400 hover:bg-red-500/10 transition-colors"
                    >
                      <LogOut className="w-4 h-4" />
                      Đăng xuất
                    </button>
                  </div>
                )}
              </div>
            </>
          ) : (
            <Link to="/login" className="btn-primary">
              Đăng nhập
            </Link>
          )}
        </div>
      </div>
    </nav>
  );
}
