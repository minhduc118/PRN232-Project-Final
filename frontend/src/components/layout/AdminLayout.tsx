import { useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import toast from 'react-hot-toast';
import {
  LayoutDashboard,
  CalendarCheck,
  Users,
  TrendingUp,
  Settings,
  Shield,
  Gift,
  Wrench,
  Bell,
  LogOut,
  Menu,
  X,
  ChevronDown,
  Search,
  ClipboardList,
  FileText,
} from 'lucide-react';

interface NavItem {
  label: string;
  to: string;
  icon: React.ReactNode;
  badge?: number;
  exact?: boolean;
}

interface NavSection {
  label: string;
  items: NavItem[];
}

// Court icon inline
function CourtIcon({ className }: { className?: string }) {
  return (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
      stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"
      className={className}>
      <ellipse cx="12" cy="12" rx="9" ry="6" />
      <line x1="12" y1="6" x2="12" y2="18" />
      <line x1="3" y1="12" x2="21" y2="12" />
    </svg>
  );
}

const NAV_SECTIONS: NavSection[] = [
  {
    label: 'Tổng quan',
    items: [
      { label: 'Dashboard',     to: '/admin',          icon: <LayoutDashboard className="w-4 h-4" />, exact: true },
      { label: 'Đặt sân',       to: '/admin/bookings', icon: <CalendarCheck   className="w-4 h-4" />, badge: 12 },
      { label: 'Quản lý sân',   to: '/admin/courts',   icon: <CourtIcon       className="w-4 h-4" /> },
    ],
  },
  {
    label: 'Quản lý',
    items: [
      { label: 'Khách hàng',    to: '/admin/users',       icon: <Users     className="w-4 h-4" /> },
      { label: 'Khuyến mãi',    to: '/admin/promotions',  icon: <Gift      className="w-4 h-4" /> },
      { label: 'Dịch vụ & Kho', to: '/admin/services',    icon: <Wrench    className="w-4 h-4" /> },
    ],
  },
  {
    label: 'Báo cáo',
    items: [
      { label: 'Doanh thu',         to: '/admin/reports',  icon: <TrendingUp    className="w-4 h-4" /> },
      { label: 'Hóa đơn',           to: '/admin/payments', icon: <FileText      className="w-4 h-4" /> },
      { label: 'Nhật ký hệ thống',  to: '#',               icon: <ClipboardList className="w-4 h-4" /> },
    ],
  },
  {
    label: 'Hệ thống',
    items: [
      { label: 'Cài đặt',    to: '/admin/settings', icon: <Settings className="w-4 h-4" /> },
      { label: 'Phân quyền', to: '#',               icon: <Shield   className="w-4 h-4" /> },
    ],
  },
];

export default function AdminLayout() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [profileOpen, setProfileOpen] = useState(false);

  const handleLogout = () => {
    logout();
    toast.success('Đăng xuất thành công!');
    navigate('/login');
  };

  const avatarInitial = (user?.fullName ?? 'A').charAt(0).toUpperCase();

  return (
    <div className="flex h-screen bg-surface overflow-hidden">
      {/* ─── Sidebar ─── */}
      <aside
        className={`
          ${sidebarOpen ? 'w-64' : 'w-0 md:w-[60px]'}
          flex-shrink-0 flex flex-col
          bg-surface-card border-r border-surface-border
          transition-all duration-300 ease-in-out overflow-hidden z-40
        `}
      >
        {/* Logo */}
        <div className="flex items-center gap-3 px-4 py-5 border-b border-surface-border min-h-[68px]">
          <div className="w-9 h-9 rounded-xl bg-gradient-to-tr from-primary-600 to-primary-400 flex items-center justify-center flex-shrink-0 shadow-lg shadow-primary-600/25">
            <span className="text-base font-black text-white">S</span>
          </div>
          {sidebarOpen && (
            <div className="min-w-0">
              <h2 className="text-sm font-bold text-white leading-tight">SportsPlex</h2>
              <p className="text-[10px] text-slate-400 leading-none tracking-widest uppercase">Management</p>
            </div>
          )}
        </div>

        {/* Nav */}
        <nav className="flex-1 overflow-y-auto py-3 px-2 space-y-5 scrollbar-thin">
          {NAV_SECTIONS.map((section) => (
            <div key={section.label}>
              {sidebarOpen && (
                <p className="px-3 mb-1.5 text-[10px] font-bold tracking-widest text-slate-600 uppercase">
                  {section.label}
                </p>
              )}
              <div className="space-y-0.5">
                {section.items.map((item) => (
                  <NavLink
                    key={item.to + item.label}
                    to={item.to}
                    end={item.exact}
                    className={({ isActive }) =>
                      `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all duration-150 group
                      ${isActive
                        ? 'bg-primary-600/15 text-primary-400 border border-primary-600/25'
                        : 'text-slate-400 hover:bg-slate-800/80 hover:text-slate-100 border border-transparent'
                      }`
                    }
                  >
                    <span className="flex-shrink-0">{item.icon}</span>
                    {sidebarOpen && (
                      <>
                        <span className="flex-1 truncate">{item.label}</span>
                        {item.badge && (
                          <span className="bg-primary-600 text-white text-[10px] font-bold px-1.5 py-0.5 rounded-full">
                            {item.badge}
                          </span>
                        )}
                      </>
                    )}
                  </NavLink>
                ))}
              </div>
            </div>
          ))}
        </nav>

        {/* Footer */}
        <div className="px-3 py-4 border-t border-surface-border">
          <div className={`flex items-center gap-3 ${!sidebarOpen ? 'justify-center' : ''}`}>
            <div className="w-8 h-8 rounded-full bg-gradient-to-br from-primary-500 to-indigo-500 flex items-center justify-center flex-shrink-0 text-sm font-bold text-white shadow-md">
              {avatarInitial}
            </div>
            {sidebarOpen && (
              <>
                <div className="min-w-0 flex-1">
                  <p className="text-xs font-semibold text-slate-100 truncate">{user?.fullName ?? 'Super Admin'}</p>
                  <p className="text-[10px] text-slate-500 truncate">{user?.role ?? 'Admin'}</p>
                </div>
                <button
                  onClick={handleLogout}
                  className="p-1.5 rounded-lg text-slate-500 hover:text-red-400 hover:bg-red-400/10 transition-colors flex-shrink-0"
                  title="Đăng xuất"
                >
                  <LogOut className="w-4 h-4" />
                </button>
              </>
            )}
          </div>
        </div>
      </aside>

      {/* ─── Main ─── */}
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">
        {/* Header */}
        <header className="flex items-center gap-4 px-6 py-3 bg-surface-card border-b border-surface-border flex-shrink-0 z-30 min-h-[68px]">
          <button
            onClick={() => setSidebarOpen(!sidebarOpen)}
            className="p-2 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors flex-shrink-0"
          >
            {sidebarOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
          </button>

          {/* Search */}
          <div className="flex-1 flex items-center gap-3 bg-slate-800/50 rounded-xl px-4 py-2 border border-surface-border max-w-lg">
            <Search className="w-4 h-4 text-slate-600 flex-shrink-0" />
            <input
              type="text"
              placeholder="Tìm kiếm booking, sân, khách hàng..."
              className="flex-1 bg-transparent text-sm text-slate-300 placeholder:text-slate-600 outline-none"
            />
          </div>

          <div className="flex items-center gap-2 ml-auto">
            {/* Bell */}
            <button className="relative p-2 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors">
              <Bell className="w-5 h-5" />
              <span className="absolute top-2 right-2 w-1.5 h-1.5 rounded-full bg-primary-500 ring-2 ring-surface-card" />
            </button>

            {/* User dropdown */}
            <div className="relative">
              <button
                onClick={() => setProfileOpen(!profileOpen)}
                className="flex items-center gap-2 px-2.5 py-1.5 rounded-xl hover:bg-slate-800 transition-colors"
              >
                <div className="w-7 h-7 rounded-full bg-gradient-to-br from-primary-500 to-indigo-500 flex items-center justify-center text-xs font-bold text-white flex-shrink-0">
                  {avatarInitial}
                </div>
                <span className="text-sm font-medium text-slate-300 hidden sm:block">{user?.fullName ?? 'Super Admin'}</span>
                <ChevronDown className={`w-4 h-4 text-slate-500 transition-transform ${profileOpen ? 'rotate-180' : ''}`} />
              </button>

              {profileOpen && (
                <>
                  <div className="fixed inset-0 z-40" onClick={() => setProfileOpen(false)} />
                  <div className="absolute right-0 mt-2 w-48 rounded-xl bg-surface-card border border-surface-border shadow-2xl p-1.5 z-50 animate-fade-in">
                    <button
                      onClick={() => { setProfileOpen(false); navigate('/profile'); }}
                      className="w-full flex items-center gap-2 px-3 py-2 text-sm text-slate-300 hover:bg-slate-800 hover:text-white rounded-lg transition-colors"
                    >
                      <Users className="w-4 h-4" /> Tài khoản
                    </button>
                    <div className="my-1 border-t border-surface-border" />
                    <button
                      onClick={handleLogout}
                      className="w-full flex items-center gap-2 px-3 py-2 text-sm text-red-400 hover:bg-red-500/10 rounded-lg transition-colors"
                    >
                      <LogOut className="w-4 h-4" /> Đăng xuất
                    </button>
                  </div>
                </>
              )}
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="flex-1 overflow-y-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
