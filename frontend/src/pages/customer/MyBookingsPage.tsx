import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyBookings, cancelBooking } from '@/api/bookingApi';
import type { Booking, BookingStatus } from '@/types/booking.types';
import Navbar from '@/components/Navbar';
import { 
  CalendarDays, 
  Clock, 
  AlertCircle,
  HelpCircle,
  Loader2,
  Trash2,
  FileText
} from 'lucide-react';
import toast from 'react-hot-toast';

export default function MyBookingsPage() {
  const navigate = useNavigate();
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<BookingStatus | 'All'>('All');

  const loadMyBookings = async () => {
    try {
      setLoading(true);
      const data = await getMyBookings();
      // Sort bookings by newest created first
      data.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
      setBookings(data);
    } catch {
      toast.error('Không thể tải lịch sử đặt sân.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadMyBookings();
  }, []);

  const handleCancelBooking = async (bookingId: number) => {
    const confirm = window.confirm(
      'Bạn có chắc chắn muốn hủy đơn đặt sân này không?\n\nChính sách hoàn tiền:\n- Trước 24h: Hoàn tiền 100%\n- 12h - 24h: Hoàn tiền 50%\n- Dưới 12h: Không hoàn tiền'
    );
    if (!confirm) return;

    try {
      toast.loading('Đang xử lý hủy đơn đặt...', { id: 'cancel-action' });
      await cancelBooking(bookingId);
      toast.success('Hủy đặt sân thành công!', { id: 'cancel-action' });
      loadMyBookings(); // reload
    } catch {
      toast.error('Có lỗi xảy ra khi hủy đặt sân.', { id: 'cancel-action' });
    }
  };

  const filteredBookings = filter === 'All' 
    ? bookings 
    : bookings.filter((b) => b.status === filter);

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col">
        <Navbar />
        <div className="flex-1 flex flex-col items-center justify-center text-slate-400">
          <Loader2 className="w-12 h-12 text-green-500 animate-spin mb-4" />
          <p className="text-lg">Đang tải lịch sử đặt sân của bạn...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-950 flex flex-col">
      <Navbar />

      <div className="flex-1 max-w-5xl w-full mx-auto px-4 py-8 space-y-6">
        {/* Header section */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-white flex items-center gap-2">
              <CalendarDays className="w-6 h-6 text-green-400" />
              Lịch sử đặt sân của tôi
            </h1>
            <p className="text-slate-400 text-xs mt-1">Xem, thanh toán và quản lý các lịch đặt chơi của bạn</p>
          </div>

          {/* Refund policy quick notice */}
          <div className="bg-slate-900 border border-slate-800 rounded-xl p-3.5 flex gap-2.5 max-w-sm text-xs text-slate-300">
            <AlertCircle className="w-4 h-4 text-green-400 shrink-0 mt-0.5" />
            <div>
              <span className="font-semibold text-white block">Quy tắc hủy đặt sân:</span>
              <span className="block opacity-80">Hoàn tiền 100% trước 24h, 50% trước 12h. Trễ hơn không áp dụng hoàn tiền.</span>
            </div>
          </div>
        </div>

        {/* Filter Toolbar */}
        <div className="flex flex-wrap gap-2 border-b border-slate-800 pb-4">
          {(['All', 'Pending', 'Confirmed', 'Cancelled', 'Completed'] as const).map((status) => {
            const active = filter === status;
            let statusText = 'Tất cả';
            if (status === 'Pending') statusText = 'Chờ thanh toán';
            else if (status === 'Confirmed') statusText = 'Đã xác nhận';
            else if (status === 'Cancelled') statusText = 'Đã hủy';
            else if (status === 'Completed') statusText = 'Hoàn thành';

            return (
              <button
                key={status}
                onClick={() => setFilter(status)}
                className={`px-4 py-2 rounded-xl text-xs font-semibold border transition-all ${
                  active 
                    ? 'bg-green-500 text-slate-950 border-green-400 shadow-md shadow-green-500/10' 
                    : 'bg-slate-900 text-slate-300 border-slate-800 hover:border-slate-700'
                }`}
              >
                {statusText}
              </button>
            );
          })}
        </div>

        {/* Bookings List */}
        {filteredBookings.length === 0 ? (
          <div className="card text-center p-12 space-y-4">
            <HelpCircle className="w-16 h-16 text-slate-600 mx-auto" />
            <p className="text-slate-400 text-base font-medium">Bạn chưa có đặt lịch chơi nào trong trạng thái này.</p>
            <button onClick={() => navigate('/courts')} className="btn-primary mt-2">
              Khám phá và Đặt sân ngay
            </button>
          </div>
        ) : (
          <div className="space-y-4">
            {filteredBookings.map((b) => {
              const unpaid = b.status === 'Pending';
              const cancelled = b.status === 'Cancelled';
              const confirmed = b.status === 'Confirmed';

              return (
                <div 
                  key={b.bookingId} 
                  className="card bg-slate-900 border-slate-800 p-6 flex flex-col md:flex-row md:items-center justify-between gap-6 hover:border-slate-700 transition-colors"
                >
                  <div className="space-y-3">
                    <div className="flex items-center gap-2.5">
                      <span className="text-xs font-mono font-semibold text-slate-400 uppercase tracking-wider">
                        #{b.bookingCode}
                      </span>
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-[10px] font-bold ${
                        confirmed 
                          ? 'bg-green-500/10 text-green-400 border border-green-500/20' 
                          : unpaid 
                          ? 'bg-yellow-500/10 text-yellow-400 border border-yellow-500/20'
                          : cancelled
                          ? 'bg-red-500/10 text-red-400 border border-red-500/20'
                          : 'bg-slate-800 text-slate-400'
                      }`}>
                        {b.status === 'Confirmed' ? 'Đã xác nhận' : b.status === 'Pending' ? 'Chờ thanh toán' : b.status === 'Cancelled' ? 'Đã hủy' : 'Hoàn thành'}
                      </span>
                    </div>

                    <h3 className="text-lg font-bold text-white">{b.courtName}</h3>

                    <div className="flex flex-wrap items-center gap-x-6 gap-y-1.5 text-xs text-slate-400">
                      <span className="flex items-center gap-1">
                        <CalendarDays className="w-4 h-4 text-green-500" />
                        {b.bookingDate}
                      </span>
                      <span className="flex items-center gap-1">
                        <Clock className="w-4 h-4 text-green-500" />
                        {b.slotName} ({b.startTime} - {b.endTime})
                      </span>
                      <span className="font-semibold text-green-400">
                        {b.totalAmount.toLocaleString('vi-VN')} đ
                      </span>
                    </div>

                    {b.note && (
                      <p className="text-xs text-slate-500 leading-tight">
                        <span className="font-semibold text-slate-400">Ghi chú:</span> {b.note}
                      </p>
                    )}
                  </div>

                  {/* Actions column */}
                  <div className="flex flex-wrap items-center gap-3 self-end md:self-center">
                    {unpaid && (
                      <>
                        <button
                          onClick={() => handleCancelBooking(b.bookingId)}
                          className="p-2.5 rounded-lg border border-slate-800 text-red-400 hover:bg-red-500/10 transition-colors"
                          title="Hủy đơn"
                        >
                          <Trash2 className="w-4.5 h-4.5" />
                        </button>
                        <button
                          onClick={() => navigate(`/payment/${b.bookingId}`)}
                          className="btn-primary py-2.5 text-xs font-bold shadow-md shadow-green-500/10"
                        >
                          Thanh toán ngay
                        </button>
                      </>
                    )}

                    {confirmed && (
                      <button
                        onClick={() => handleCancelBooking(b.bookingId)}
                        className="btn-secondary text-red-400 border-slate-850 hover:bg-red-500/10 py-2.5 text-xs font-bold"
                      >
                        Hủy đặt sân
                      </button>
                    )}

                    <button
                      onClick={() => navigate(`/payment/result?bookingId=${b.bookingId}&status=${b.status === 'Confirmed' ? 'Success' : 'Failed'}`)}
                      className="p-2.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300 hover:text-white transition-colors"
                      title="Xem hóa đơn biên lai"
                    >
                      <FileText className="w-4.5 h-4.5" />
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
