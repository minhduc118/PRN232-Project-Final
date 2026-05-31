import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getCourtById } from '@/api/courtApi';
import type { Court } from '@/types/court.types';
import Navbar from '@/components/Navbar';
import { Star, MapPin, Clock, ArrowRight, Loader2, ArrowLeft, Award } from 'lucide-react';
import toast from 'react-hot-toast';

interface ReviewItem {
  id: number;
  userName: string;
  rating: number;
  date: string;
  comment: string;
}

export default function CourtDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [court, setCourt] = useState<Court | null>(null);
  const [loading, setLoading] = useState(true);

  // Mock reviews
  const reviews: ReviewItem[] = [
    { id: 1, userName: 'Trần Minh Quân', rating: 5, date: '28/05/2026', comment: 'Sân Pickleball mới, sạch sẽ, lưới căng chuẩn, chủ sân rất thân thiện nhiệt tình. Sẽ ủng hộ lâu dài!' },
    { id: 2, userName: 'Lê Hoàng Hải', rating: 4, date: '20/05/2026', comment: 'Mái che tốt, ánh sáng đèn LED ban đêm sáng rõ không bị chói mắt. Rất hài lòng.' },
    { id: 3, userName: 'Nguyễn Bích Ngọc', rating: 5, date: '15/05/2026', comment: 'Vị trí dễ tìm, dịch vụ nước Pocari phục vụ nhanh chóng. Có chỗ gửi xe ô tô rộng rãi.' }
  ];

  useEffect(() => {
    async function loadCourt() {
      if (!id) return;
      try {
        setLoading(true);
        const data = await getCourtById(Number(id));
        setCourt(data);
      } catch {
        toast.error('Lỗi khi tải thông tin chi tiết sân.');
      } finally {
        setLoading(false);
      }
    }
    loadCourt();
  }, [id]);

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col">
        <Navbar />
        <div className="flex-1 flex flex-col items-center justify-center text-slate-400">
          <Loader2 className="w-12 h-12 text-green-500 animate-spin mb-4" />
          <p className="text-lg">Đang tải chi tiết sân chơi...</p>
        </div>
      </div>
    );
  }

  if (!court) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col">
        <Navbar />
        <div className="flex-1 flex flex-col items-center justify-center text-slate-400 p-4">
          <ArrowLeft className="w-12 h-12 text-red-500 mb-4" />
          <p className="text-xl font-semibold text-white">Không tìm thấy sân chơi này</p>
          <button onClick={() => navigate('/courts')} className="btn-primary mt-4">
            Quay lại danh sách
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-950 flex flex-col">
      <Navbar />

      <div className="flex-1 max-w-5xl w-full mx-auto px-4 py-8 space-y-6">
        
        {/* Back navigation */}
        <button 
          onClick={() => navigate('/courts')} 
          className="flex items-center gap-2 text-sm text-slate-400 hover:text-white transition-colors"
        >
          <ArrowLeft className="w-4 h-4" />
          Quay lại danh sách tất cả sân
        </button>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Main Info */}
          <div className="lg:col-span-2 space-y-6">
            {/* Image & Title Header */}
            <div className="bg-slate-900 border border-slate-800 rounded-3xl overflow-hidden shadow-xl">
              <div className="h-72 md:h-[400px] w-full relative">
                <img 
                  src={court.imageUrl} 
                  alt={court.courtName} 
                  className="w-full h-full object-cover"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-slate-900 via-transparent to-transparent" />
              </div>
              <div className="p-6 md:p-8 space-y-4">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-green-500/10 text-green-400 border border-green-500/20">
                    <Award className="w-3.5 h-3.5" /> Tiêu chuẩn quốc tế
                  </span>
                  
                  <div className="flex items-center gap-1.5 text-sm text-yellow-400">
                    <Star className="w-4 h-4 fill-yellow-400 text-yellow-400" />
                    <span className="font-bold">{court.rating} / 5.0</span>
                    <span className="text-slate-500">({court.reviewCount} đánh giá)</span>
                  </div>
                </div>

                <h1 className="text-3xl font-extrabold text-white">{court.courtName}</h1>

                <p className="text-slate-350 text-sm leading-relaxed">{court.description}</p>
              </div>
            </div>

            {/* Reviews list */}
            <div className="card space-y-6">
              <h2 className="text-xl font-bold text-white border-b border-slate-800 pb-3">
                Đánh giá từ khách hàng ({reviews.length})
              </h2>

              <div className="space-y-4">
                {reviews.map((r) => (
                  <div key={r.id} className="p-4 bg-slate-950 border border-slate-850 rounded-2xl space-y-2">
                    <div className="flex justify-between items-center text-xs">
                      <span className="font-bold text-white">{r.userName}</span>
                      <span className="text-slate-500">{r.date}</span>
                    </div>
                    <div className="flex items-center gap-0.5">
                      {[...Array(5)].map((_, i) => (
                        <Star 
                          key={i} 
                          className={`w-3.5 h-3.5 ${
                            i < r.rating ? 'fill-yellow-400 text-yellow-400' : 'text-slate-700'
                          }`} 
                        />
                      ))}
                    </div>
                    <p className="text-slate-300 text-xs leading-relaxed">{r.comment}</p>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Booking call-out panel */}
          <div className="space-y-6">
            <div className="card space-y-6 sticky top-24">
              <h2 className="text-xl font-bold text-white border-b border-slate-800 pb-3">
                Thông tin thuê sân
              </h2>

              <div className="space-y-3.5 text-sm">
                <div className="flex justify-between items-center text-slate-400">
                  <span className="flex items-center gap-1.5"><MapPin className="w-4 h-4 text-green-500" /> Vị trí</span>
                  <span className="font-semibold text-white">{court.location}</span>
                </div>
                <div className="flex justify-between items-center text-slate-400">
                  <span className="flex items-center gap-1.5"><Clock className="w-4 h-4 text-green-500" /> Giờ mở cửa</span>
                  <span className="font-semibold text-white">{court.openTime} - {court.closeTime}</span>
                </div>
                <div className="border-t border-slate-800 pt-3 flex justify-between items-end">
                  <span className="font-bold text-slate-300">Đơn giá</span>
                  <span className="text-xl font-extrabold text-green-400 leading-none">
                    {court.pricePerHour.toLocaleString('vi-VN')} đ / giờ
                  </span>
                </div>
              </div>

              <button
                onClick={() => navigate(`/booking/${court.courtId}`)}
                className="w-full btn-primary py-3.5 text-base font-bold shadow-lg shadow-green-500/20 flex items-center justify-center gap-2"
              >
                Đặt sân ngay <ArrowRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

      </div>
    </div>
  );
}
