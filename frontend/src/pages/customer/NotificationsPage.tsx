import { useState } from 'react';
import Navbar from '@/components/Navbar';
import { 
  Bell, 
  Calendar, 
  CreditCard, 
  Award, 
  Trash2, 
  CheckCheck, 
  Clock,
  Sparkles
} from 'lucide-react';
import toast from 'react-hot-toast';

interface NotificationItem {
  id: number;
  title: string;
  message: string;
  type: 'booking' | 'payment' | 'membership' | 'system';
  time: string;
  isRead: boolean;
}

const INITIAL_NOTIFICATIONS: NotificationItem[] = [
  {
    id: 1,
    title: 'Thanh toán thành công',
    message: 'Giao dịch thanh toán trực tuyến cho đặt sân #BK-20260515-0001 đã thành công qua cổng VNPay. Số tiền: 100.000 đ.',
    type: 'payment',
    time: '2 giờ trước',
    isRead: false,
  },
  {
    id: 2,
    title: 'Xác nhận đặt sân thành công',
    message: 'Đơn đặt sân chơi của bạn tại Sân Pickleball A1, Ca 17:00–18:00 ngày 2026-05-16 đã được hệ thống xác nhận và giữ chỗ.',
    type: 'booking',
    time: '2 giờ trước',
    isRead: false,
  },
  {
    id: 3,
    title: 'Thăng hạng thành viên mới',
    message: 'Chúc mừng bạn đã đạt mốc 1.250 điểm tích lũy và được thăng lên hạng thành viên Bạc (Silver). Bạn nhận được ưu đãi giảm 5% cho tất cả các ca đặt tiếp theo!',
    type: 'membership',
    time: '2 ngày trước',
    isRead: true,
  },
  {
    id: 4,
    title: 'Chào mừng thành viên mới',
    message: 'Chào mừng bạn đến với hệ thống quản lý đặt sân SportsCourt. Hãy bổ sung thông tin cá nhân trong mục Hồ sơ để nhận thêm điểm thưởng nhé!',
    type: 'system',
    time: '5 ngày trước',
    isRead: true,
  },
];

export default function NotificationsPage() {
  const [notifications, setNotifications] = useState<NotificationItem[]>(INITIAL_NOTIFICATIONS);

  const markAllAsRead = () => {
    setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
    toast.success('Đã đánh dấu đọc tất cả thông báo');
  };

  const toggleRead = (id: number) => {
    setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: !n.isRead } : n));
  };

  const deleteNotification = (id: number, e: React.MouseEvent) => {
    e.stopPropagation();
    setNotifications(prev => prev.filter(n => n.id !== id));
    toast.success('Đã xóa thông báo');
  };

  const getIcon = (type: string) => {
    switch (type) {
      case 'booking':
        return <Calendar className="w-5 h-5 text-green-400" />;
      case 'payment':
        return <CreditCard className="w-5 h-5 text-blue-400" />;
      case 'membership':
        return <Award className="w-5 h-5 text-yellow-400 animate-pulse" />;
      default:
        return <Bell className="w-5 h-5 text-slate-400" />;
    }
  };

  const getBgColor = (type: string) => {
    switch (type) {
      case 'booking':
        return 'bg-green-500/10 border-green-500/20';
      case 'payment':
        return 'bg-blue-500/10 border-blue-500/20';
      case 'membership':
        return 'bg-yellow-500/10 border-yellow-500/20';
      default:
        return 'bg-slate-800/50 border-slate-700/50';
    }
  };

  const unreadCount = notifications.filter(n => !n.isRead).length;

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col">
      <Navbar />

      <div className="flex-1 max-w-3xl w-full mx-auto px-4 py-8 space-y-6">
        
        {/* Header toolbar */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 border-b border-slate-850 pb-5">
          <div>
            <h1 className="text-2xl font-bold text-white flex items-center gap-2">
              <Bell className="w-6 h-6 text-green-400" />
              Thông báo của tôi
            </h1>
            <p className="text-xs text-slate-400 mt-1">
              {unreadCount > 0 ? `Bạn có ${unreadCount} thông báo chưa đọc` : 'Bạn không có thông báo mới nào'}
            </p>
          </div>

          {notifications.length > 0 && (
            <button
              onClick={markAllAsRead}
              className="flex items-center gap-1.5 text-xs font-semibold text-green-400 hover:text-green-300 transition-colors border border-green-500/20 hover:border-green-500/40 bg-green-500/5 px-3.5 py-2 rounded-xl"
            >
              <CheckCheck className="w-4 h-4" />
              Đánh dấu đã đọc tất cả
            </button>
          )}
        </div>

        {/* Notifications List */}
        {notifications.length === 0 ? (
          <div className="card bg-slate-900 border-slate-800 text-center p-12 space-y-4 rounded-2xl border">
            <Bell className="w-16 h-16 text-slate-700 mx-auto" />
            <p className="text-slate-400 text-base font-medium">Hộp thư thông báo của bạn đang trống.</p>
          </div>
        ) : (
          <div className="space-y-3">
            {notifications.map((n) => (
              <div
                key={n.id}
                onClick={() => toggleRead(n.id)}
                className={`card p-5 border rounded-2xl flex gap-4 transition-all cursor-pointer relative group ${
                  n.isRead 
                    ? 'bg-slate-900/40 border-slate-850 opacity-80' 
                    : 'bg-slate-900 border-slate-800 hover:border-slate-700 shadow-md shadow-green-500/5'
                }`}
              >
                {/* Unread indicator dot */}
                {!n.isRead && (
                  <div className="absolute top-5 right-5 w-2.5 h-2.5 rounded-full bg-green-500 shadow-lg shadow-green-500/50" />
                )}

                {/* Icon Circle */}
                <div className={`w-10 h-10 rounded-xl flex items-center justify-center shrink-0 border ${getBgColor(n.type)}`}>
                  {getIcon(n.type)}
                </div>

                {/* Content Details */}
                <div className="flex-1 space-y-1 pr-6">
                  <div className="flex items-center gap-2 flex-wrap">
                    <h3 className={`text-sm font-bold ${n.isRead ? 'text-slate-300' : 'text-white'}`}>
                      {n.title}
                    </h3>
                    {n.type === 'membership' && (
                      <span className="inline-flex px-2 py-0.25 rounded bg-yellow-500/10 text-yellow-500 border border-yellow-500/20 text-[9px] font-bold items-center gap-0.5">
                        <Sparkles className="w-2.5 h-2.5" />
                        VIP
                      </span>
                    )}
                  </div>
                  
                  <p className="text-xs text-slate-400 leading-relaxed">
                    {n.message}
                  </p>

                  <div className="flex items-center gap-1.5 text-[10px] text-slate-500 pt-1">
                    <Clock className="w-3.5 h-3.5" />
                    <span>{n.time}</span>
                    <span className="text-slate-700">•</span>
                    <span className="hover:underline text-slate-400">
                      {n.isRead ? 'Đánh dấu chưa đọc' : 'Đánh dấu đã đọc'}
                    </span>
                  </div>
                </div>

                {/* Delete button (shows on hover) */}
                <button
                  onClick={(e) => deleteNotification(n.id, e)}
                  className="absolute bottom-5 right-5 p-1.5 rounded-lg border border-slate-800 text-slate-500 hover:text-red-400 hover:bg-red-500/10 opacity-0 group-hover:opacity-100 transition-opacity"
                  title="Xóa thông báo"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            ))}
          </div>
        )}

      </div>
    </div>
  );
}
