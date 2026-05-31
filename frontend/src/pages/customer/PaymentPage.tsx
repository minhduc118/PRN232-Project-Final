import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getBookingById, updateBookingPayment } from '@/api/bookingApi';
import type { Booking } from '@/types/booking.types';
import Navbar from '@/components/Navbar';
import { 
  CreditCard, 
  ArrowLeft, 
  Loader2, 
  AlertCircle, 
  CheckCircle,
  HelpCircle
} from 'lucide-react';
import toast from 'react-hot-toast';

export default function PaymentPage() {
  const { bookingId } = useParams<{ bookingId: string }>();
  const navigate = useNavigate();

  const [booking, setBooking] = useState<Booking | null>(null);
  const [loading, setLoading] = useState(true);
  const [paymentMethod, setPaymentMethod] = useState<'VNPay' | 'MoMo' | 'BankTransfer' | 'Cash'>('VNPay');
  const [paying, setPaying] = useState(false);
  const [showSimulator, setShowSimulator] = useState(false);

  useEffect(() => {
    async function loadBooking() {
      if (!bookingId) return;
      try {
        setLoading(true);
        const data = await getBookingById(Number(bookingId));
        setBooking(data);
      } catch {
        toast.error('Không thể tải thông tin đặt sân.');
      } finally {
        setLoading(false);
      }
    }
    loadBooking();
  }, [bookingId]);

  const handleProcessPayment = async () => {
    if (!booking) return;

    if (paymentMethod === 'Cash' || paymentMethod === 'BankTransfer') {
      // Simulate direct cash booking confirmation
      setPaying(true);
      try {
        await updateBookingPayment(booking.bookingId, 'Success', paymentMethod);
        toast.success('Xác nhận đặt sân thành công!');
        navigate(`/payment/result?bookingId=${booking.bookingId}&status=Success&method=${paymentMethod}`);
      } catch {
        toast.error('Lỗi khi ghi nhận thanh toán.');
      } finally {
        setPaying(false);
      }
      return;
    }

    if (paymentMethod === 'VNPay') {
      setShowSimulator(true);
    } else {
      toast.error('Phương thức MoMo hiện tại đang được bảo trì.');
    }
  };

  const handleSimulatedResponse = async (status: 'Success' | 'Failed') => {
    if (!booking) return;
    setShowSimulator(false);
    setPaying(true);
    try {
      const txId = `VNP${Date.now()}`;
      await updateBookingPayment(booking.bookingId, status, 'VNPay', txId);
      if (status === 'Success') {
        toast.success('Thanh toán VNPay thành công!');
        navigate(`/payment/result?bookingId=${booking.bookingId}&vnp_ResponseCode=00&vnp_TxnRef=${txId}`);
      } else {
        toast.error('Thanh toán VNPay thất bại hoặc bị hủy.');
        navigate(`/payment/result?bookingId=${booking.bookingId}&vnp_ResponseCode=24`);
      }
    } catch {
      toast.error('Lỗi hệ thống khi cập nhật thanh toán.');
    } finally {
      setPaying(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col">
        <Navbar />
        <div className="flex-1 flex flex-col items-center justify-center text-slate-400">
          <Loader2 className="w-12 h-12 text-green-500 animate-spin mb-4" />
          <p className="text-lg">Đang chuẩn bị thông tin hóa đơn...</p>
        </div>
      </div>
    );
  }

  if (!booking) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col">
        <Navbar />
        <div className="flex-1 flex flex-col items-center justify-center text-slate-400 p-4">
          <AlertCircle className="w-16 h-16 text-red-500 mb-4" />
          <p className="text-xl font-semibold text-white">Không tìm thấy mã đặt sân hợp lệ</p>
          <button onClick={() => navigate('/courts')} className="btn-primary mt-4">
            Về trang chủ
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-950 flex flex-col">
      <Navbar />

      <div className="flex-1 max-w-4xl w-full mx-auto px-4 py-8 space-y-6">
        {/* Back Button */}
        <button 
          onClick={() => navigate(`/booking/${booking.courtId}`)} 
          className="flex items-center gap-2 text-sm text-slate-400 hover:text-white transition-colors"
        >
          <ArrowLeft className="w-4 h-4" />
          Quay lại chỉnh sửa đặt sân
        </button>

        <div className="grid grid-cols-1 md:grid-cols-5 gap-6">
          {/* Billing details */}
          <div className="md:col-span-3 space-y-6">
            {/* Order info summary */}
            <div className="card space-y-4">
              <h2 className="text-lg font-bold text-white border-b border-slate-800 pb-3">
                Chi tiết đặt sân #{booking.bookingCode}
              </h2>
              
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-slate-400 block text-xs">Tên sân</span>
                  <span className="font-semibold text-white">{booking.courtName}</span>
                </div>
                <div>
                  <span className="text-slate-400 block text-xs">Ngày đặt</span>
                  <span className="font-semibold text-white">{booking.bookingDate}</span>
                </div>
                <div>
                  <span className="text-slate-400 block text-xs">Khung giờ</span>
                  <span className="font-semibold text-white">{booking.slotName} ({booking.startTime} - {booking.endTime})</span>
                </div>
                <div>
                  <span className="text-slate-400 block text-xs">Trạng thái đặt</span>
                  <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-semibold mt-1 ${
                    booking.status === 'Confirmed' 
                      ? 'bg-green-500/10 text-green-400 border border-green-500/20' 
                      : 'bg-yellow-500/10 text-yellow-400 border border-yellow-500/20'
                  }`}>
                    {booking.status === 'Confirmed' ? 'Đã xác nhận' : 'Chờ thanh toán'}
                  </span>
                </div>
              </div>

              {booking.note && (
                <div className="bg-slate-950 p-3 rounded-lg border border-slate-850 text-xs text-slate-400">
                  <span className="font-semibold text-slate-300 block mb-1">Ghi chú:</span>
                  {booking.note}
                </div>
              )}
            </div>

            {/* Payment methods */}
            <div className="card space-y-4">
              <h2 className="text-lg font-bold text-white border-b border-slate-800 pb-3">
                Phương thức thanh toán
              </h2>

              <div className="space-y-3">
                {/* VNPay */}
                <label className={`flex items-center justify-between p-4 rounded-xl border-2 cursor-pointer transition-all ${
                  paymentMethod === 'VNPay' 
                    ? 'bg-slate-900 border-green-500 shadow-md shadow-green-500/5' 
                    : 'bg-slate-950 border-slate-800 hover:border-slate-700'
                }`}>
                  <div className="flex items-center gap-3">
                    <input
                      type="radio"
                      name="payment_method"
                      checked={paymentMethod === 'VNPay'}
                      onChange={() => setPaymentMethod('VNPay')}
                      className="sr-only"
                    />
                    <div className="w-12 h-8 bg-white rounded flex items-center justify-center p-1 border border-slate-200">
                      <img src="https://sandbox.vnpayment.vn/paymentv2/Images/brands/logo.svg" alt="VNPay Logo" className="max-h-full" />
                    </div>
                    <div>
                      <span className="block text-sm font-semibold text-white">Cổng thanh toán VNPay</span>
                      <span className="text-xs text-slate-400">Thẻ ATM nội địa, Mobile Banking, quét mã QR</span>
                    </div>
                  </div>
                  <div className={`w-4 h-4 rounded-full border-2 flex items-center justify-center ${
                    paymentMethod === 'VNPay' ? 'border-green-500 bg-green-500' : 'border-slate-650'
                  }`}>
                    {paymentMethod === 'VNPay' && <div className="w-1.5 h-1.5 rounded-full bg-slate-950" />}
                  </div>
                </label>

                {/* Momo */}
                <label className={`flex items-center justify-between p-4 rounded-xl border-2 cursor-pointer transition-all opacity-60 ${
                  paymentMethod === 'MoMo' 
                    ? 'bg-slate-900 border-green-500' 
                    : 'bg-slate-950 border-slate-800 hover:border-slate-700'
                }`}>
                  <div className="flex items-center gap-3">
                    <input
                      type="radio"
                      name="payment_method"
                      checked={paymentMethod === 'MoMo'}
                      onChange={() => setPaymentMethod('MoMo')}
                      className="sr-only"
                    />
                    <div className="w-12 h-8 bg-[#a50064] rounded flex items-center justify-center p-1 font-bold text-white text-xs">
                      MoMo
                    </div>
                    <div>
                      <span className="block text-sm font-semibold text-white">Ví MoMo (Đang bảo trì)</span>
                      <span className="text-xs text-slate-400">Ví điện tử siêu ứng dụng MoMo</span>
                    </div>
                  </div>
                  <div className="w-4 h-4 rounded-full border-2 border-slate-650" />
                </label>

                {/* Direct Bank Transfer */}
                <label className={`flex items-center justify-between p-4 rounded-xl border-2 cursor-pointer transition-all ${
                  paymentMethod === 'BankTransfer' 
                    ? 'bg-slate-900 border-green-500 shadow-md shadow-green-500/5' 
                    : 'bg-slate-950 border-slate-800 hover:border-slate-700'
                }`}>
                  <div className="flex items-center gap-3">
                    <input
                      type="radio"
                      name="payment_method"
                      checked={paymentMethod === 'BankTransfer'}
                      onChange={() => setPaymentMethod('BankTransfer')}
                      className="sr-only"
                    />
                    <div className="w-12 h-8 bg-slate-850 rounded flex items-center justify-center p-1 border border-slate-750">
                      <CreditCard className="w-5 h-5 text-green-400" />
                    </div>
                    <div>
                      <span className="block text-sm font-semibold text-white">Chuyển khoản trực tiếp</span>
                      <span className="text-xs text-slate-400">Xem thông tin và quét mã QR chuyển khoản của trung tâm</span>
                    </div>
                  </div>
                  <div className={`w-4 h-4 rounded-full border-2 flex items-center justify-center ${
                    paymentMethod === 'BankTransfer' ? 'border-green-500 bg-green-500' : 'border-slate-650'
                  }`}>
                    {paymentMethod === 'BankTransfer' && <div className="w-1.5 h-1.5 rounded-full bg-slate-950" />}
                  </div>
                </label>
              </div>
            </div>
          </div>

          {/* Right column: Invoice Payment summary */}
          <div className="md:col-span-2 space-y-6">
            <div className="card space-y-6 sticky top-24">
              <h2 className="text-lg font-bold text-white border-b border-slate-800 pb-3">
                Hóa đơn thanh toán
              </h2>

              <div className="space-y-3 text-sm">
                <div className="flex justify-between text-slate-400">
                  <span>Tiền thuê sân</span>
                  <span className="font-semibold text-white">
                    {booking.subTotal.toLocaleString('vi-VN')} đ
                  </span>
                </div>
                
                {booking.discountAmount > 0 && (
                  <div className="flex justify-between text-green-400">
                    <span>Mã giảm giá</span>
                    <span className="font-semibold">
                      -{booking.discountAmount.toLocaleString('vi-VN')} đ
                    </span>
                  </div>
                )}
                
                <div className="border-t border-slate-800 pt-3 flex justify-between items-end">
                  <span className="font-bold text-slate-300">Tổng tiền</span>
                  <span className="text-2xl font-extrabold text-green-400 leading-none">
                    {booking.totalAmount.toLocaleString('vi-VN')} đ
                  </span>
                </div>
              </div>

              <button
                disabled={paying}
                onClick={handleProcessPayment}
                className="w-full btn-primary py-3.5 text-base font-bold shadow-lg shadow-green-500/20 flex items-center justify-center gap-2"
              >
                {paying ? (
                  <>
                    <Loader2 className="w-5 h-5 animate-spin" /> Đang xử lý...
                  </>
                ) : (
                  'Thanh toán ngay'
                )}
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* VNPay Simulator Overlay Modal */}
      {showSimulator && (
        <div className="fixed inset-0 z-[100] bg-slate-950/90 backdrop-blur-md flex items-center justify-center p-4">
          <div className="bg-slate-900 border border-slate-800 rounded-3xl w-full max-w-lg overflow-hidden shadow-2xl animate-slide-up">
            {/* Header */}
            <div className="bg-gradient-to-r from-red-600 to-orange-500 p-6 text-white text-center">
              <img src="https://sandbox.vnpayment.vn/paymentv2/Images/brands/logo.svg" alt="VNPay Logo" className="h-8 mx-auto filter invert brightness-0" />
              <h3 className="text-lg font-extrabold tracking-wide mt-2">CỔNG THANH TOÁN GIẢ LẬP VNPAY</h3>
              <p className="text-[11px] opacity-80 mt-0.5">Môi trường kiểm thử Sandbox</p>
            </div>

            {/* Content */}
            <div className="p-8 space-y-6">
              <div className="bg-slate-950 rounded-xl p-5 border border-slate-850 space-y-3">
                <div className="flex justify-between text-sm">
                  <span className="text-slate-400">Đơn vị thụ hưởng:</span>
                  <span className="font-semibold text-white">SportsCourt Center</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-slate-400">Mã giao dịch đặt sân:</span>
                  <span className="font-semibold text-green-400">{booking.bookingCode}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-slate-400">Số tiền giao dịch:</span>
                  <span className="text-lg font-bold text-orange-400">{booking.totalAmount.toLocaleString('vi-VN')} đ</span>
                </div>
              </div>

              <div className="bg-yellow-500/10 border border-yellow-500/25 rounded-xl p-4 flex gap-3 text-xs text-yellow-300">
                <HelpCircle className="w-8 h-8 shrink-0 text-yellow-400" />
                <p className="leading-relaxed">
                  Bạn đang ở chế độ **USE_MOCK=true**. Cổng thanh toán này hoàn toàn giả lập để phục vụ kiểm thử luồng nghiệp vụ. Hãy chọn kết quả thanh toán bạn mong muốn dưới đây.
                </p>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <button
                  onClick={() => handleSimulatedResponse('Success')}
                  className="w-full btn-primary bg-emerald-600 hover:bg-emerald-700 py-3.5 text-sm font-extrabold flex items-center justify-center gap-2 border border-emerald-500/30"
                >
                  <CheckCircle className="w-4 h-4 text-white" />
                  Thanh toán THÀNH CÔNG
                </button>
                <button
                  onClick={() => handleSimulatedResponse('Failed')}
                  className="w-full btn-danger bg-red-950 text-red-400 hover:bg-red-900 py-3.5 text-sm font-extrabold flex items-center justify-center gap-2 border border-red-800/30"
                >
                  HỦY / Giao dịch thất bại
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
