import { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { getBookingById } from '@/api/bookingApi';
import type { Booking } from '@/types/booking.types';
import Navbar from '@/components/Navbar';
import { 
  CheckCircle2, 
  XCircle, 
  Loader2, 
  Printer, 
  ArrowRight, 
  CalendarDays
} from 'lucide-react';
import toast from 'react-hot-toast';

export default function PaymentResultPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const bookingId = searchParams.get('bookingId');
  const responseCode = searchParams.get('vnp_ResponseCode');
  const customStatus = searchParams.get('status');

  const [booking, setBooking] = useState<Booking | null>(null);
  const [loading, setLoading] = useState(true);

  // Success is code '00' for VNPay or custom status 'Success'
  const isSuccess = responseCode === '00' || customStatus === 'Success';

  useEffect(() => {
    async function fetchReceipt() {
      if (!bookingId) return;
      try {
        setLoading(true);
        const data = await getBookingById(Number(bookingId));
        setBooking(data);
      } catch {
        toast.error('Không thể lấy thông tin chi tiết hóa đơn.');
      } finally {
        setLoading(false);
      }
    }
    fetchReceipt();
  }, [bookingId]);

  const handlePrint = () => {
    window.print();
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col">
        <Navbar />
        <div className="flex-1 flex flex-col items-center justify-center text-slate-400">
          <Loader2 className="w-12 h-12 text-green-500 animate-spin mb-4" />
          <p className="text-lg font-medium">Đang đối chiếu kết quả thanh toán...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-950 flex flex-col print:bg-white print:text-black">
      <div className="print:hidden">
        <Navbar />
      </div>

      <div className="flex-1 max-w-2xl w-full mx-auto px-4 py-12 space-y-8 animate-fade-in">
        
        {/* Status Indicator Card */}
        <div className="card text-center p-8 space-y-4 print:hidden border-slate-800">
          {isSuccess ? (
            <>
              <CheckCircle2 className="w-20 h-20 text-green-500 mx-auto animate-bounce" />
              <h1 className="text-3xl font-extrabold text-white">Thanh toán thành công!</h1>
              <p className="text-slate-400 text-sm max-w-md mx-auto">
                Cảm ơn bạn đã lựa chọn dịch vụ của SportsCourt. Đơn đặt sân đã được xác nhận và giữ chỗ.
              </p>
            </>
          ) : (
            <>
              <XCircle className="w-20 h-20 text-red-500 mx-auto animate-pulse" />
              <h1 className="text-3xl font-extrabold text-white">Thanh toán thất bại</h1>
              <p className="text-slate-400 text-sm max-w-md mx-auto">
                Giao dịch của bạn không thể hoàn tất hoặc đã bị hủy. Vui lòng thử lại.
              </p>
            </>
          )}
        </div>

        {/* Invoice details */}
        {booking && (
          <div className="card p-8 border-slate-800 bg-slate-900 shadow-2xl relative print:border-none print:shadow-none print:bg-white print:p-0">
            {/* Corner decoration */}
            <div className="absolute top-0 right-0 w-24 h-24 bg-gradient-to-bl from-green-500/10 to-transparent rounded-tr-xl print:hidden" />
            
            {/* Header info */}
            <div className="border-b border-slate-800 pb-6 flex flex-col sm:flex-row justify-between items-start gap-4">
              <div>
                <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider block">HÓA ĐƠN ĐẶT SÂN</span>
                <span className="text-lg font-bold text-white mt-1 block print:text-black">#{booking.bookingCode}</span>
                <span className="text-xs text-slate-500 block mt-1">Ngày lập: {new Date(booking.createdAt).toLocaleString('vi-VN')}</span>
              </div>
              <div className="text-left sm:text-right">
                <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider block">Trạng thái</span>
                <span className={`inline-flex px-3 py-1 rounded-full text-xs font-bold mt-1 ${
                  isSuccess 
                    ? 'bg-green-500/10 text-green-400 border border-green-500/20' 
                    : 'bg-red-500/10 text-red-400 border border-red-500/20'
                }`}>
                  {isSuccess ? 'ĐÃ THANH TOÁN' : 'CHƯA THANH TOÁN'}
                </span>
              </div>
            </div>

            {/* Receipt table details */}
            <div className="py-6 space-y-4 text-sm">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <span className="text-slate-400 block text-xs">Tên khách hàng:</span>
                  <span className="font-semibold text-white print:text-black">Nguyễn Văn Hùng</span>
                </div>
                <div>
                  <span className="text-slate-400 block text-xs">Sân thể thao:</span>
                  <span className="font-semibold text-white print:text-black">{booking.courtName}</span>
                </div>
                <div>
                  <span className="text-slate-400 block text-xs">Khung giờ đặt chơi:</span>
                  <span className="font-semibold text-white print:text-black">{booking.slotName} ({booking.startTime} - {booking.endTime})</span>
                </div>
                <div>
                  <span className="text-slate-400 block text-xs">Ngày chơi:</span>
                  <span className="font-semibold text-white print:text-black">{booking.bookingDate}</span>
                </div>
                {booking.payment && (
                  <>
                    <div>
                      <span className="text-slate-400 block text-xs">Phương thức:</span>
                      <span className="font-semibold text-white print:text-black">{booking.payment.paymentMethod}</span>
                    </div>
                    <div>
                      <span className="text-slate-400 block text-xs">Mã giao dịch cổng:</span>
                      <span className="font-semibold text-green-400">{booking.payment.transactionId || 'N/A'}</span>
                    </div>
                  </>
                )}
              </div>

              {/* Price Breakdown */}
              <div className="border-t border-slate-800 pt-6 mt-6 space-y-3">
                <div className="flex justify-between text-slate-400 text-xs">
                  <span>Tiền thuê sân</span>
                  <span className="text-white print:text-black font-semibold">{booking.subTotal.toLocaleString('vi-VN')} đ</span>
                </div>
                
                {booking.discountAmount > 0 && (
                  <div className="flex justify-between text-green-400 text-xs">
                    <span>Mã giảm giá đã áp dụng</span>
                    <span className="font-semibold">-{booking.discountAmount.toLocaleString('vi-VN')} đ</span>
                  </div>
                )}

                <div className="border-t border-slate-800 pt-3 flex justify-between items-end">
                  <span className="font-bold text-slate-300 print:text-black">Tổng thực tế</span>
                  <span className="text-xl font-extrabold text-green-400">
                    {booking.totalAmount.toLocaleString('vi-VN')} đ
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Action button bar */}
        <div className="flex flex-col sm:flex-row gap-4 justify-between items-center print:hidden">
          <button
            onClick={handlePrint}
            className="flex items-center gap-2 text-sm text-slate-400 hover:text-white transition-colors"
          >
            <Printer className="w-4 h-4 text-green-400" />
            In biên lai hóa đơn
          </button>

          <div className="flex gap-3">
            {isSuccess ? (
              <>
                <button
                  onClick={() => navigate('/my-bookings')}
                  className="btn-secondary flex items-center gap-2"
                >
                  <CalendarDays className="w-4 h-4 text-green-400" />
                  Quản lý đặt sân của tôi
                </button>
                <button
                  onClick={() => navigate('/')}
                  className="btn-primary flex items-center gap-2"
                >
                  Về trang chủ <ArrowRight className="w-4 h-4" />
                </button>
              </>
            ) : (
              <>
                <button
                  onClick={() => navigate(`/payment/${bookingId}`)}
                  className="btn-primary bg-orange-600 hover:bg-orange-700 flex items-center gap-2 border border-orange-500/25"
                >
                  Thử thanh toán lại
                </button>
                <button
                  onClick={() => navigate('/')}
                  className="btn-secondary"
                >
                  Trở lại Trang chủ
                </button>
              </>
            )}
          </div>
        </div>

      </div>
    </div>
  );
}
