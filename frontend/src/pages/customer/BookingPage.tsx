import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getCourtById } from '@/api/courtApi';
import { createBooking } from '@/api/bookingApi';
import { promotionApi } from '@/api/promotionApi';
import type { Court } from '@/types/court.types';
import type { CreateBookingRequest } from '@/types/booking.types';
import Navbar from '@/components/Navbar';
import { 
  Calendar as CalendarIcon, 
  Clock, 
  MapPin, 
  Tag, 
  Plus, 
  Minus, 
  Loader2, 
  AlertCircle,
  Repeat
} from 'lucide-react';
import toast from 'react-hot-toast';

interface TimeSlot {
  slotId: number;
  slotName: string;
  startTime: string;
  endTime: string;
  dayType: string;
}

interface AdditionalService {
  id: number;
  name: string;
  price: number;
  category: string;
  quantity: number;
}

export default function BookingPage() {
  const { courtId } = useParams<{ courtId: string }>();
  const navigate = useNavigate();
  
  const [court, setCourt] = useState<Court | null>(null);
  const [loadingCourt, setLoadingCourt] = useState(true);
  const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([]);
  
  // Selection states
  const [selectedDate, setSelectedDate] = useState<string>(
    new Date().toISOString().split('T')[0]
  );
  const [selectedSlotId, setSelectedSlotId] = useState<number | null>(null);
  const [note, setNote] = useState('');
  
  // Recurring states
  const [isRecurring, setIsRecurring] = useState(false);
  const [selectedDays, setSelectedDays] = useState<number[]>([]); // 0: CN, 1: T2, etc.
  const [recurringEndDate, setRecurringEndDate] = useState('');

  // Additional Services
  const [services, setServices] = useState<AdditionalService[]>([
    { id: 1, name: 'Nước Pocari Sweat (Chai)', price: 15000, category: 'Drink', quantity: 0 },
    { id: 2, name: 'Nước khoáng Aquafina (Chai)', price: 10000, category: 'Drink', quantity: 0 },
    { id: 3, name: 'Thuê vợt cao cấp (Cặp)', price: 30000, category: 'Rent', quantity: 0 },
    { id: 4, name: 'Thuê bóng thi đấu (Quả)', price: 10000, category: 'Rent', quantity: 0 },
  ]);

  // Promotions
  const [promoCode, setPromoCode] = useState('');
  const [appliedPromo, setAppliedPromo] = useState<{
    code: string;
    discountType: 'Percentage' | 'FixedAmount';
    value: number;
    name: string;
  } | null>(null);
  const [validatingPromo, setValidatingPromo] = useState(false);

  // Load court details and time slots
  useEffect(() => {
    async function loadData() {
      if (!courtId) return;
      try {
        setLoadingCourt(true);
        const data = await getCourtById(Number(courtId));
        setCourt(data);

        // Load slots
        const { default: slots } = await import('@/mocks/time-slots.json');
        setTimeSlots(slots as TimeSlot[]);
      } catch {
        toast.error('Không thể tải thông tin sân.');
      } finally {
        setLoadingCourt(false);
      }
    }
    loadData();
  }, [courtId]);

  // Handle quantity modification
  const updateServiceQty = (id: number, increment: boolean) => {
    setServices((prev) =>
      prev.map((s) => {
        if (s.id === id) {
          const nextQty = increment ? s.quantity + 1 : Math.max(0, s.quantity - 1);
          return { ...s, quantity: nextQty };
        }
        return s;
      })
    );
  };

  // Promo code validation
  const handleApplyPromo = async () => {
    if (!promoCode.trim()) {
      toast.error('Vui lòng nhập mã giảm giá.');
      return;
    }
    setValidatingPromo(true);
    try {
      if (import.meta.env.VITE_USE_MOCK === 'true') {
        await new Promise((r) => setTimeout(r, 400));
        const { default: promotions } = await import('@/mocks/promotions.json');
        const found = promotions.find(
          (p) => p.promoCode.toUpperCase() === promoCode.trim().toUpperCase() && p.isActive
        );

        if (found) {
          setAppliedPromo({
            code: found.promoCode,
            discountType: found.discountType as 'Percentage' | 'FixedAmount',
            value: found.discountValue,
            name: found.promoName,
          });
          toast.success(`Áp dụng mã thành công: ${found.promoName}`);
        } else {
          toast.error('Mã giảm giá không hợp lệ hoặc đã hết hạn.');
        }
      } else {
        const currentSubtotal = (court?.pricePerHour || 100000) + services.reduce((acc, curr) => acc + curr.price * curr.quantity, 0);
        const res = await promotionApi.validateCoupon(promoCode.trim(), currentSubtotal);
        if (res && res.valid) {
          setAppliedPromo({
            code: res.promoCode,
            discountType: res.discountType === 'Percent' ? 'Percentage' : 'FixedAmount',
            value: res.discountValue,
            name: res.promoName,
          });
          toast.success(`Áp dụng mã thành công: ${res.promoName}`);
        } else {
          toast.error(res?.message || 'Mã giảm giá không hợp lệ.');
        }
      }
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Lỗi khi kiểm tra mã giảm giá.');
    } finally {
      setValidatingPromo(false);
    }
  };

  // Check if slot is booked (seeded random simulation for mock mode)
  const isSlotBooked = (slotId: number) => {
    if (!selectedDate) return false;
    // Simple deterministic hash based on date and slotId to make booking stable for each date
    let hash = 0;
    const dateStr = selectedDate + slotId.toString();
    for (let i = 0; i < dateStr.length; i++) {
      hash = dateStr.charCodeAt(i) + ((hash << 5) - hash);
    }
    // Around 25% of slots will show as booked, except Slot 11 (17:00-18:00) which has specific mock booking
    if (courtId === '1' && selectedDate === '2026-05-16' && slotId === 11) {
      return true;
    }
    return Math.abs(hash) % 4 === 0;
  };

  // Pricing math
  const courtPrice = court?.pricePerHour || 100000;
  const servicePriceTotal = services.reduce((acc, curr) => acc + curr.price * curr.quantity, 0);
  const subtotal = courtPrice + servicePriceTotal;
  
  let discountAmount = 0;
  if (appliedPromo) {
    if (appliedPromo.discountType === 'Percentage') {
      discountAmount = (subtotal * appliedPromo.value) / 100;
    } else {
      discountAmount = appliedPromo.value;
    }
  }
  const totalAmount = Math.max(0, subtotal - discountAmount);

  // Submit Booking
  const handleBookingSubmit = async () => {
    if (!selectedSlotId) {
      toast.error('Vui lòng chọn khung giờ muốn đặt.');
      return;
    }

    if (isRecurring) {
      if (selectedDays.length === 0) {
        toast.error('Vui lòng chọn ít nhất một thứ trong tuần.');
        return;
      }
      if (!recurringEndDate) {
        toast.error('Vui lòng chọn ngày kết thúc đặt lịch định kỳ.');
        return;
      }
    }

    try {
      const payload: CreateBookingRequest = {
        courtId: Number(courtId),
        slotId: selectedSlotId,
        bookingDate: selectedDate,
        note: note.trim(),
        serviceIds: services
          .filter((s) => s.quantity > 0)
          .map((s) => ({ serviceId: s.id, quantity: s.quantity })),
        promotionCode: appliedPromo?.code,
        isRecurring,
        recurringDays: isRecurring ? selectedDays : undefined,
        recurringEndDate: isRecurring ? recurringEndDate : undefined,
      };

      toast.loading('Đang xử lý đơn đặt sân...', { id: 'booking-action' });
      const result = await createBooking(payload);
      toast.success('Đặt sân thành công! Đang chuyển đến trang thanh toán...', { id: 'booking-action' });
      
      // Redirect to Payment Screen
      navigate(`/payment/${result.bookingId}`);
    } catch {
      toast.error('Đã có lỗi xảy ra khi tạo đơn đặt sân.', { id: 'booking-action' });
    }
  };

  const toggleDaySelection = (day: number) => {
    setSelectedDays((prev) =>
      prev.includes(day) ? prev.filter((d) => d !== day) : [...prev, day]
    );
  };

  if (loadingCourt) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col">
        <Navbar />
        <div className="flex-1 flex flex-col items-center justify-center text-slate-400">
          <Loader2 className="w-12 h-12 text-green-500 animate-spin mb-4" />
          <p className="text-lg">Đang tải thông tin sân & lịch trình...</p>
        </div>
      </div>
    );
  }

  if (!court) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col">
        <Navbar />
        <div className="flex-1 flex flex-col items-center justify-center text-slate-400 p-4">
          <AlertCircle className="w-16 h-16 text-red-500 mb-4" />
          <p className="text-xl font-semibold text-white">Không tìm thấy sân thể thao</p>
          <button onClick={() => navigate('/courts')} className="btn-primary mt-4">
            Quay lại Danh sách
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-950 flex flex-col">
      <Navbar />
      
      <div className="flex-1 max-w-7xl w-full mx-auto px-4 py-8 grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        {/* Left column: Court Info + Booking Options */}
        <div className="lg:col-span-2 space-y-6">
          {/* Court Detail Card */}
          <div className="bg-slate-900 border border-slate-800 rounded-2xl overflow-hidden shadow-xl">
            <div className="h-64 md:h-80 w-full relative">
              <img 
                src={court.imageUrl} 
                alt={court.courtName} 
                className="w-full h-full object-cover"
              />
              <div className="absolute inset-0 bg-gradient-to-t from-slate-900 via-transparent to-transparent" />
              <div className="absolute bottom-6 left-6 right-6">
                <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-green-500/20 text-green-400 border border-green-500/30">
                  {court.status === 'Available' ? 'Sẵn sàng đặt' : 'Bảo trì'}
                </span>
                <h1 className="text-3xl font-extrabold text-white mt-2">{court.courtName}</h1>
                <div className="flex items-center gap-4 text-slate-300 text-sm mt-2">
                  <span className="flex items-center gap-1"><MapPin className="w-4 h-4 text-green-400" /> {court.location}</span>
                  <span className="flex items-center gap-1"><Clock className="w-4 h-4 text-green-400" /> {court.openTime} - {court.closeTime}</span>
                </div>
              </div>
            </div>
            <div className="p-6">
              <p className="text-slate-400 text-sm leading-relaxed">{court.description}</p>
            </div>
          </div>

          {/* Date and Time Slot Picker */}
          <div className="card space-y-6">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-800 pb-4">
              <div>
                <h2 className="text-xl font-bold text-white">1. Chọn thời gian chơi</h2>
                <p className="text-slate-400 text-xs mt-0.5">Đặt lịch theo ngày hoặc định kỳ hàng tuần</p>
              </div>
              
              {/* Date Input */}
              <div className="relative">
                <CalendarIcon className="absolute left-3.5 top-3.5 w-4 h-4 text-green-400" />
                <input
                  type="date"
                  min={new Date().toISOString().split('T')[0]}
                  value={selectedDate}
                  onChange={(e) => {
                    setSelectedDate(e.target.value);
                    setSelectedSlotId(null); // Clear selected slot when date changes
                  }}
                  className="input-field pl-10 max-w-[200px]"
                />
              </div>
            </div>

            {/* Time slot grid */}
            <div>
              <h3 className="text-sm font-semibold text-slate-400 mb-3">Khung giờ hoạt động</h3>
              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
                {timeSlots.map((slot) => {
                  const booked = isSlotBooked(slot.slotId);
                  const isSelected = selectedSlotId === slot.slotId;
                  
                  let slotClass = "slot-available";
                  if (booked) slotClass = "slot-booked";
                  else if (isSelected) slotClass = "slot-selecting";

                  return (
                    <button
                      key={slot.slotId}
                      disabled={booked}
                      onClick={() => setSelectedSlotId(slot.slotId)}
                      className={`p-3.5 rounded-xl flex flex-col items-center justify-center text-center transition-all ${slotClass}`}
                    >
                      <span className="font-bold text-sm">{slot.slotName.replace('Ca ', '')}</span>
                      <span className="text-[10px] mt-0.5 opacity-80 uppercase tracking-wide">
                        {booked ? 'Đã đặt' : 'Chọn'}
                      </span>
                    </button>
                  );
                })}
              </div>
            </div>

            {/* Recurring Booking Option */}
            <div className="border-t border-slate-850 pt-6 space-y-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Repeat className="w-5 h-5 text-green-400" />
                  <div>
                    <span className="font-bold text-white text-sm block">Đặt lịch định kỳ</span>
                    <span className="text-slate-400 text-xs">Lặp lại đơn đặt sân hàng tuần</span>
                  </div>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    checked={isRecurring}
                    onChange={(e) => setIsRecurring(e.target.checked)}
                    className="sr-only peer"
                  />
                  <div className="w-11 h-6 bg-slate-800 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-slate-400 after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-green-500" />
                </label>
              </div>

              {isRecurring && (
                <div className="p-4 bg-slate-950 border border-slate-850 rounded-xl space-y-4 animate-fade-in">
                  <div>
                    <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                      Chọn các thứ lặp lại trong tuần
                    </label>
                    <div className="flex flex-wrap gap-2">
                      {['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'].map((dayName, idx) => {
                        const dayValue = idx === 6 ? 0 : idx + 1; // 0 is Sun, 1 is Mon
                        const active = selectedDays.includes(dayValue);
                        return (
                          <button
                            key={dayName}
                            onClick={() => toggleDaySelection(dayValue)}
                            className={`w-10 h-10 rounded-lg text-xs font-bold transition-all border ${
                              active 
                                ? 'bg-green-500 text-slate-950 border-green-400 shadow-md shadow-green-500/25' 
                                : 'bg-slate-900 text-slate-300 border-slate-800 hover:border-slate-700'
                            }`}
                          >
                            {dayName}
                          </button>
                        );
                      })}
                    </div>
                  </div>

                  <div>
                    <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                      Ngày kết thúc chu kỳ
                    </label>
                    <input
                      type="date"
                      min={new Date().toISOString().split('T')[0]}
                      value={recurringEndDate}
                      onChange={(e) => setRecurringEndDate(e.target.value)}
                      className="input-field max-w-xs"
                    />
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* Add-on Services */}
          <div className="card space-y-4">
            <div>
              <h2 className="text-xl font-bold text-white">2. Dịch vụ đi kèm</h2>
              <p className="text-slate-400 text-xs mt-0.5">Nước giải khát và thiết bị chơi bổ trợ</p>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {services.map((item) => (
                <div 
                  key={item.id} 
                  className="flex items-center justify-between p-4 bg-slate-950 border border-slate-850 rounded-xl"
                >
                  <div>
                    <h4 className="text-sm font-semibold text-white">{item.name}</h4>
                    <span className="text-xs text-green-400 font-medium">
                      {item.price.toLocaleString('vi-VN')} đ
                    </span>
                  </div>
                  <div className="flex items-center gap-3">
                    <button
                      disabled={item.quantity === 0}
                      onClick={() => updateServiceQty(item.id, false)}
                      className="p-1 rounded-lg bg-slate-900 border border-slate-800 text-slate-400 hover:text-white disabled:opacity-50"
                    >
                      <Minus className="w-4 h-4" />
                    </button>
                    <span className="w-6 text-center text-sm font-bold text-white">
                      {item.quantity}
                    </span>
                    <button
                      onClick={() => updateServiceQty(item.id, true)}
                      className="p-1 rounded-lg bg-slate-900 border border-slate-800 text-slate-400 hover:text-white"
                    >
                      <Plus className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Right column: Order Summary & Coupon box */}
        <div className="space-y-6">
          {/* Coupon Code Panel */}
          <div className="card space-y-4">
            <h3 className="text-base font-bold text-white flex items-center gap-2">
              <Tag className="w-4 h-4 text-green-400" />
              Mã khuyến mãi
            </h3>
            <div className="flex gap-2">
              <input
                type="text"
                placeholder="Ví dụ: WELCOME10, SUMMER25"
                value={promoCode}
                onChange={(e) => setPromoCode(e.target.value)}
                className="input-field uppercase"
              />
              <button
                onClick={handleApplyPromo}
                disabled={validatingPromo}
                className="btn-secondary"
              >
                {validatingPromo ? '...' : 'Áp dụng'}
              </button>
            </div>
            {appliedPromo && (
              <div className="flex items-center justify-between p-2.5 bg-green-500/10 border border-green-500/20 rounded-lg text-xs text-green-400">
                <span className="font-semibold">{appliedPromo.name}</span>
                <span className="font-bold">
                  -{appliedPromo.discountType === 'Percentage' ? `${appliedPromo.value}%` : `${appliedPromo.value.toLocaleString()}đ`}
                </span>
              </div>
            )}
          </div>

          {/* Pricing Breakdown & Booking Button */}
          <div className="card space-y-6">
            <h3 className="text-lg font-bold text-white border-b border-slate-800 pb-3">
              Tóm tắt đơn đặt sân
            </h3>

            <div className="space-y-3 text-sm">
              <div className="flex justify-between text-slate-400">
                <span>Tiền sân ({court.courtName})</span>
                <span className="font-semibold text-white">
                  {courtPrice.toLocaleString('vi-VN')} đ
                </span>
              </div>
              <div className="flex justify-between text-slate-400">
                <span>Dịch vụ đi kèm</span>
                <span className="font-semibold text-white">
                  {servicePriceTotal.toLocaleString('vi-VN')} đ
                </span>
              </div>
              
              {appliedPromo && (
                <div className="flex justify-between text-green-400">
                  <span>Khuyến mãi ({appliedPromo.code})</span>
                  <span className="font-semibold">
                    -{discountAmount.toLocaleString('vi-VN')} đ
                  </span>
                </div>
              )}
              
              <div className="border-t border-slate-800 pt-3 flex justify-between items-end">
                <span className="font-bold text-slate-300">Tổng thanh toán</span>
                <span className="text-2xl font-extrabold text-green-400 leading-none">
                  {totalAmount.toLocaleString('vi-VN')} đ
                </span>
              </div>
            </div>

            {/* Additional note */}
            <div>
              <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
                Ghi chú thêm
              </label>
              <textarea
                rows={3}
                placeholder="Thời gian chuẩn bị đặc biệt, yêu cầu huấn luyện viên..."
                value={note}
                onChange={(e) => setNote(e.target.value)}
                className="input-field text-xs"
              />
            </div>

            <button
              onClick={handleBookingSubmit}
              className="w-full btn-primary py-3.5 text-base font-bold shadow-lg shadow-green-500/20"
            >
              Tiến hành thanh toán
            </button>
          </div>
        </div>

      </div>
    </div>
  );
}
