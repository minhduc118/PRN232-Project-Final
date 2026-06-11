import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
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
  FileText,
  Printer,
  X,
  CreditCard,
  CheckCircle,
  Receipt,
  ArrowRight
} from 'lucide-react';
import toast from 'react-hot-toast';

export default function HistoryPage() {
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<BookingStatus | 'All'>('All');
  
  // Modal State for Invoice Detail
  const [selectedBooking, setSelectedBooking] = useState<Booking | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const loadMyBookings = async (showLoading = false) => {
    try {
      if (showLoading) setLoading(true);
      const data = await getMyBookings();
      // Sort bookings by newest created first
      data.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
      setBookings(data);
    } catch {
      toast.error('Không thể tải lịch sử đặt sân.');
    } finally {
      if (showLoading) setLoading(false);
    }
  };

  useEffect(() => {
    let active = true;
    const initialLoad = async () => {
      try {
        const data = await getMyBookings();
        if (!active) return;
        data.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        setBookings(data);
      } catch {
        toast.error('Không thể tải lịch sử đặt sân.');
      } finally {
        if (active) setLoading(false);
      }
    };
    initialLoad();
    return () => {
      active = false;
    };
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
      
      // If the cancelled booking is currently open in modal, close it
      if (selectedBooking && selectedBooking.bookingId === bookingId) {
        setIsModalOpen(false);
        setSelectedBooking(null);
      }
    } catch {
      toast.error('Có lỗi xảy ra khi hủy đặt sân.', { id: 'cancel-action' });
    }
  };

  const openInvoiceModal = (booking: Booking) => {
    setSelectedBooking(booking);
    setIsModalOpen(true);
  };

  const closeInvoiceModal = () => {
    setIsModalOpen(false);
    setSelectedBooking(null);
  };

  const handlePrint = () => {
    window.print();
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
    <div className="min-h-screen bg-slate-950 flex flex-col print:bg-white print:text-black">
      {/* Hide Navbar during print */}
      <div className="print:hidden">
        <Navbar />
      </div>

      <div className="flex-1 max-w-5xl w-full mx-auto px-4 py-8 space-y-6 print:p-0 print:max-w-none">
        
        {/* Header section (hidden on print) */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 print:hidden">
          <div>
            <h1 className="text-2xl font-bold text-white flex items-center gap-2">
              <CalendarDays className="w-6 h-6 text-green-400" />
              Lịch sử đặt sân của tôi
            </h1>
            <p className="text-slate-400 text-xs mt-1">Xem, thanh toán và quản lý các lịch đặt chơi của bạn</p>
          </div>

          {/* Refund policy quick notice */}
          <div className="bg-slate-900 border border-slate-800 rounded-xl p-3.5 flex gap-2.5 max-w-sm text-xs text-slate-350">
            <AlertCircle className="w-5 h-5 text-green-400 shrink-0 mt-0.5" />
            <div>
              <span className="font-semibold text-white block">Quy tắc hủy đặt sân:</span>
              <span className="block opacity-90 mt-0.5">Hoàn tiền 100% trước 24h, 50% trước 12h-24h. Dưới 12h không hoàn tiền.</span>
            </div>
          </div>
        </div>

        {/* Filter Toolbar (hidden on print) */}
        <div className="flex flex-wrap gap-2 border-b border-slate-850 pb-4 print:hidden">
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
                    : 'bg-slate-900 text-slate-300 border-slate-800 hover:border-slate-700 hover:text-white'
                }`}
              >
                {statusText}
              </button>
            );
          })}
        </div>

        {/* Bookings List (hidden on print) */}
        <div className="print:hidden">
          {filteredBookings.length === 0 ? (
            <div className="card bg-slate-900 border-slate-800 text-center p-12 space-y-4 rounded-2xl">
              <HelpCircle className="w-16 h-16 text-slate-655 mx-auto text-slate-600" />
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
                    className="card bg-slate-900 border-slate-800 p-6 flex flex-col md:flex-row md:items-center justify-between gap-6 hover:border-slate-700 transition-all rounded-2xl border"
                  >
                    <div className="space-y-3">
                      <div className="flex items-center gap-2.5">
                        <span className="text-xs font-mono font-semibold text-slate-400 uppercase tracking-wider">
                          #{b.bookingCode}
                        </span>
                        <span className={`inline-flex px-2.5 py-0.5 rounded-full text-[10px] font-bold ${
                          confirmed 
                            ? 'bg-green-500/10 text-green-400 border border-green-500/20' 
                            : unpaid 
                            ? 'bg-yellow-500/10 text-yellow-400 border border-yellow-500/20'
                            : cancelled
                            ? 'bg-red-500/10 text-red-400 border border-red-500/20'
                            : 'bg-blue-500/10 text-blue-400 border border-blue-500/20'
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
                          <span className="font-semibold text-slate-400 font-mono">Ghi chú:</span> {b.note}
                        </p>
                      )}
                    </div>

                    {/* Actions column */}
                    <div className="flex flex-wrap items-center gap-3 self-end md:self-center">
                      {unpaid && (
                        <>
                          <button
                            onClick={() => handleCancelBooking(b.bookingId)}
                            className="p-2.5 rounded-xl border border-slate-800 text-red-450 text-red-400 hover:bg-red-500/10 transition-colors"
                            title="Hủy đơn"
                          >
                            <Trash2 className="w-4.5 h-4.5" />
                          </button>
                          <button
                            onClick={() => navigate(`/payment/${b.bookingId}`)}
                            className="btn-primary py-2.5 px-4 text-xs font-bold shadow-md shadow-green-500/10 rounded-xl"
                          >
                            Thanh toán ngay
                          </button>
                        </>
                      )}

                      {confirmed && (
                        <button
                          onClick={() => handleCancelBooking(b.bookingId)}
                          className="btn-secondary text-red-400 border-slate-800 hover:bg-red-500/10 py-2.5 px-4 text-xs font-bold rounded-xl"
                        >
                          Hủy đặt sân
                        </button>
                      )}

                      <button
                        onClick={() => openInvoiceModal(b)}
                        className="p-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 hover:text-white transition-colors flex items-center gap-1.5 text-xs font-semibold"
                        title="Xem hóa đơn biên lai"
                      >
                        <FileText className="w-4.5 h-4.5" />
                        <span>Hóa đơn</span>
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        {/* INVOICE MODAL (Fills screen on print) */}
        {isModalOpen && selectedBooking && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm print:absolute print:inset-0 print:p-0 print:bg-white print:backdrop-blur-none">
            <div className="relative w-full max-w-2xl bg-slate-900 border border-slate-800 rounded-2xl shadow-2xl overflow-hidden animate-fade-in max-h-[90vh] flex flex-col print:border-none print:shadow-none print:bg-white print:max-h-none print:overflow-visible">
              
              {/* Modal Header (hidden on print) */}
              <div className="flex items-center justify-between px-6 py-4 border-b border-slate-800 print:hidden">
                <span className="font-bold text-white flex items-center gap-1.5">
                  <Receipt className="w-5 h-5 text-green-400" />
                  Chi tiết hóa đơn
                </span>
                <button 
                  onClick={closeInvoiceModal}
                  className="p-1 rounded-lg text-slate-400 hover:text-white hover:bg-slate-850 transition-colors"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>

              {/* Printable Content Container */}
              <div className="flex-1 overflow-y-auto p-6 space-y-6 print:overflow-visible print:p-0">
                {/* Brand / Logo */}
                <div className="flex justify-between items-center pb-4 border-b border-slate-800/80 print:border-slate-300">
                  <div className="flex items-center gap-2">
                    <div className="w-8 h-8 rounded-lg bg-green-500 flex items-center justify-center print:border print:border-green-600 print:bg-transparent">
                      <span className="text-slate-950 font-black text-sm print:text-green-600">S</span>
                    </div>
                    <div>
                      <span className="font-extrabold text-sm text-white print:text-black">SportsCourt</span>
                      <span className="block text-[8px] text-slate-455 text-slate-500 uppercase tracking-wider">Management</span>
                    </div>
                  </div>
                  <div className="text-right text-xs text-slate-400 print:text-black">
                    <span className="block font-bold">SportsCourt Hub</span>
                    <span className="block">Liên hệ: 1900 6868</span>
                    <span className="block">Website: sportscourt.vn</span>
                  </div>
                </div>

                {/* Invoice Meta */}
                <div className="flex flex-col sm:flex-row justify-between gap-4 py-2 text-xs">
                  <div>
                    <span className="text-slate-455 text-slate-500 uppercase tracking-wider block font-semibold">Mã hóa đơn</span>
                    <span className="font-mono font-bold text-sm text-white mt-0.5 block print:text-black">#{selectedBooking.bookingCode}</span>
                    <span className="text-slate-455 text-slate-500 block mt-1">Ngày lập: {new Date(selectedBooking.createdAt).toLocaleString('vi-VN')}</span>
                  </div>
                  <div className="sm:text-right">
                    <span className="text-slate-455 text-slate-500 uppercase tracking-wider block font-semibold">Trạng thái đặt sân</span>
                    <span className={`inline-flex px-2.5 py-0.5 rounded-full text-[10px] font-bold mt-1 ${
                      selectedBooking.status === 'Confirmed' 
                        ? 'bg-green-500/10 text-green-400 border border-green-500/20 print:border-green-600 print:text-green-600' 
                        : selectedBooking.status === 'Pending' 
                        ? 'bg-yellow-500/10 text-yellow-400 border border-yellow-500/20 print:border-yellow-600 print:text-yellow-600'
                        : selectedBooking.status === 'Cancelled'
                        ? 'bg-red-500/10 text-red-400 border border-red-500/20 print:border-red-650 print:text-red-650'
                        : 'bg-blue-500/10 text-blue-400 border border-blue-500/20 print:border-blue-600 print:text-blue-600'
                    }`}>
                      {selectedBooking.status === 'Confirmed' ? 'ĐÃ XÁC NHẬN' : selectedBooking.status === 'Pending' ? 'CHỜ THANH TOÁN' : selectedBooking.status === 'Cancelled' ? 'ĐÃ HỦY' : 'HOÀN THÀNH'}
                    </span>
                  </div>
                </div>

                {/* Details Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 bg-slate-950/40 border border-slate-800/80 rounded-xl p-4 text-xs print:bg-transparent print:border-slate-300">
                  <div className="space-y-1">
                    <span className="text-slate-455 text-slate-550 block font-semibold text-slate-500">Khách hàng:</span>
                    <span className="font-bold text-white print:text-black">{user?.fullName || 'Khách hàng'}</span>
                    <span className="block text-slate-400 print:text-black">{user?.email}</span>
                    {user?.phone && <span className="block text-slate-400 print:text-black">{user?.phone}</span>}
                  </div>
                  <div className="space-y-1">
                    <span className="text-slate-455 text-slate-550 block font-semibold text-slate-500">Chi tiết dịch vụ:</span>
                    <span className="font-bold text-white print:text-black">{selectedBooking.courtName}</span>
                    <span className="block text-slate-400 print:text-black">Ngày chơi: {selectedBooking.bookingDate}</span>
                    <span className="block text-slate-400 print:text-black">Giờ: {selectedBooking.slotName} ({selectedBooking.startTime} - {selectedBooking.endTime})</span>
                  </div>
                </div>

                {/* Payment Detail Section if available */}
                <div className="space-y-3">
                  <span className="text-xs font-bold text-white uppercase tracking-wider block print:text-black">Thông tin giao dịch</span>
                  {selectedBooking.payment ? (
                    <div className="border border-slate-800 rounded-xl p-4 space-y-2 text-xs print:border-slate-350">
                      <div className="flex justify-between">
                        <span className="text-slate-400 print:text-black">Phương thức thanh toán:</span>
                        <span className="font-bold text-white print:text-black flex items-center gap-1">
                          <CreditCard className="w-3.5 h-3.5 text-green-400" />
                          {selectedBooking.payment.paymentMethod}
                        </span>
                      </div>
                      {selectedBooking.payment.transactionId && (
                        <div className="flex justify-between">
                          <span className="text-slate-400 print:text-black">Mã giao dịch (Cổng):</span>
                          <span className="font-mono font-semibold text-white print:text-black">{selectedBooking.payment.transactionId}</span>
                        </div>
                      )}
                      <div className="flex justify-between">
                        <span className="text-slate-400 print:text-black">Trạng thái thanh toán:</span>
                        <span className="font-bold text-green-400 print:text-green-600 flex items-center gap-1">
                          <CheckCircle className="w-3.5 h-3.5" />
                          {selectedBooking.payment.status === 'Success' ? 'Thành công' : selectedBooking.payment.status}
                        </span>
                      </div>
                      {selectedBooking.payment.paidAt && (
                        <div className="flex justify-between">
                          <span className="text-slate-400 print:text-black">Thời gian thanh toán:</span>
                          <span className="text-white print:text-black">{new Date(selectedBooking.payment.paidAt).toLocaleString('vi-VN')}</span>
                        </div>
                      )}
                    </div>
                  ) : (
                    <div className="border border-slate-800 border-dashed rounded-xl p-4 text-center text-xs text-slate-500 print:border-slate-300">
                      Chưa có thông tin giao dịch thanh toán trực tuyến.
                    </div>
                  )}
                </div>

                {/* Price Summary */}
                <div className="border-t border-slate-800/80 pt-4 space-y-2 text-xs print:border-slate-300">
                  <div className="flex justify-between text-slate-400 print:text-black">
                    <span>Tiền thuê sân</span>
                    <span>{selectedBooking.subTotal.toLocaleString('vi-VN')} đ</span>
                  </div>
                  {selectedBooking.discountAmount > 0 && (
                    <div className="flex justify-between text-green-400 print:text-green-600">
                      <span>Mã giảm giá áp dụng</span>
                      <span>-{selectedBooking.discountAmount.toLocaleString('vi-VN')} đ</span>
                    </div>
                  )}
                  <div className="border-t border-slate-800/60 pt-2 flex justify-between items-end print:border-slate-200">
                    <span className="font-bold text-white print:text-black text-sm">Tổng thực tế</span>
                    <span className="text-lg font-black text-green-400 print:text-black">
                      {selectedBooking.totalAmount.toLocaleString('vi-VN')} đ
                    </span>
                  </div>
                </div>

                {/* Note / Refund Footer */}
                <div className="bg-slate-950/20 border border-slate-855 border-slate-800/50 rounded-xl p-3.5 text-[10px] text-slate-500 space-y-1 print:border-slate-300 print:text-black">
                  <span className="font-bold block text-slate-400 print:text-black">Chính sách hủy sân:</span>
                  <p>Hủy sân trước giờ chơi trên 24 tiếng sẽ được hoàn tiền 100%. Hủy trước từ 12 - 24 tiếng được hoàn tiền 50%. Mọi yêu cầu hủy dưới 12 tiếng sẽ không được hỗ trợ hoàn phí.</p>
                </div>
              </div>

              {/* Modal Footer (hidden on print) */}
              <div className="flex justify-between items-center px-6 py-4 bg-slate-850/50 border-t border-slate-800 print:hidden">
                <button
                  onClick={handlePrint}
                  className="flex items-center gap-1.5 text-xs font-semibold text-slate-300 hover:text-white transition-colors"
                >
                  <Printer className="w-4 h-4 text-green-400" />
                  In biên lai
                </button>

                <div className="flex gap-2.5">
                  {selectedBooking.status === 'Pending' && (
                    <button
                      onClick={() => {
                        closeInvoiceModal();
                        navigate(`/payment/${selectedBooking.bookingId}`);
                      }}
                      className="btn-primary py-2 px-3.5 text-xs font-bold rounded-lg shadow-md shadow-green-500/10 flex items-center gap-1"
                    >
                      Thanh toán <ArrowRight className="w-3.5 h-3.5" />
                    </button>
                  )}
                  <button
                    onClick={closeInvoiceModal}
                    className="btn-secondary py-2 px-3.5 text-xs font-bold rounded-lg"
                  >
                    Đóng
                  </button>
                </div>
              </div>

            </div>
          </div>
        )}

      </div>
    </div>
  );
}
