import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getCourts } from '@/api/courtApi';
import type { Court } from '@/types/court.types';
import Navbar from '@/components/Navbar';
import { Star, MapPin, Clock, ArrowRight, Sparkles, Filter, Loader2 } from 'lucide-react';
import toast from 'react-hot-toast';

export default function CourtListPage() {
  const navigate = useNavigate();
  const [courts, setCourts] = useState<Court[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedType, setSelectedType] = useState<number | undefined>(undefined);

  useEffect(() => {
    async function fetchCourts() {
      try {
        setLoading(true);
        const data = await getCourts(selectedType);
        setCourts(data);
      } catch {
        toast.error('Lỗi khi tải danh sách sân.');
      } finally {
        setLoading(false);
      }
    }
    fetchCourts();
  }, [selectedType]);

  return (
    <div className="min-h-screen bg-slate-950 flex flex-col">
      <Navbar />

      <div className="flex-1 max-w-7xl w-full mx-auto px-4 py-8 space-y-8">
        
        {/* Banner Hero */}
        <div className="relative rounded-3xl overflow-hidden bg-gradient-to-r from-green-950 to-slate-900 border border-green-500/10 p-8 md:p-12 shadow-2xl flex flex-col justify-center">
          <div className="absolute top-0 right-0 w-80 h-80 bg-green-500/5 rounded-full blur-3xl" />
          <div className="relative z-10 space-y-4 max-w-2xl">
            <span className="inline-flex items-center gap-1 text-xs font-bold text-green-400 bg-green-500/10 border border-green-500/20 px-3 py-1 rounded-full uppercase tracking-wider">
              <Sparkles className="w-3.5 h-3.5" /> Trải nghiệm tốt nhất
            </span>
            <h1 className="text-3xl md:text-5xl font-extrabold text-white tracking-tight leading-tight">
              Hệ thống đặt sân thể thao hiện đại bậc nhất
            </h1>
            <p className="text-slate-300 text-sm md:text-base leading-relaxed">
              Khám phá và đặt chỗ các sân Pickleball, Cầu lông, và Bóng đá tiêu chuẩn quốc tế ngay hôm nay. Trải nghiệm hệ thống đặt lịch tự động tiện lợi, cập nhật trạng thái thời gian thực.
            </p>
          </div>
        </div>

        {/* Tab filters and list */}
        <div className="space-y-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-800 pb-4">
            <div>
              <h2 className="text-2xl font-bold text-white flex items-center gap-2">
                <Filter className="w-5 h-5 text-green-400" />
                Danh mục sân chơi
              </h2>
              <p className="text-slate-400 text-xs mt-0.5">Lọc danh sách theo loại bộ môn thi đấu</p>
            </div>

            {/* Filter Tabs */}
            <div className="flex flex-wrap gap-2">
              <button
                onClick={() => setSelectedType(undefined)}
                className={`px-4 py-2 rounded-xl text-xs font-bold border transition-all ${
                  selectedType === undefined 
                    ? 'bg-green-500 text-slate-950 border-green-400 shadow-md shadow-green-500/10' 
                    : 'bg-slate-900 text-slate-300 border-slate-800 hover:border-slate-700'
                }`}
              >
                Tất cả bộ môn
              </button>
              <button
                onClick={() => setSelectedType(1)}
                className={`px-4 py-2 rounded-xl text-xs font-bold border transition-all ${
                  selectedType === 1 
                    ? 'bg-green-500 text-slate-950 border-green-400 shadow-md shadow-green-500/10' 
                    : 'bg-slate-900 text-slate-300 border-slate-800 hover:border-slate-700'
                }`}
              >
                Pickleball
              </button>
              <button
                onClick={() => setSelectedType(2)}
                className={`px-4 py-2 rounded-xl text-xs font-bold border transition-all ${
                  selectedType === 2 
                    ? 'bg-green-500 text-slate-950 border-green-400 shadow-md shadow-green-500/10' 
                    : 'bg-slate-900 text-slate-300 border-slate-800 hover:border-slate-700'
                }`}
              >
                Cầu lông
              </button>
              <button
                onClick={() => setSelectedType(3)}
                className={`px-4 py-2 rounded-xl text-xs font-bold border transition-all ${
                  selectedType === 3 
                    ? 'bg-green-500 text-slate-950 border-green-400 shadow-md shadow-green-500/10' 
                    : 'bg-slate-900 text-slate-300 border-slate-800 hover:border-slate-700'
                }`}
              >
                Bóng đá mini
              </button>
            </div>
          </div>

          {/* Cards List Grid */}
          {loading ? (
            <div className="flex flex-col items-center justify-center py-20 text-slate-400">
              <Loader2 className="w-10 h-10 animate-spin text-green-500 mb-2" />
              <span>Đang tải danh sách sân chơi...</span>
            </div>
          ) : courts.length === 0 ? (
            <div className="card text-center py-16 text-slate-400">
              Không có sân nào phù hợp với bộ lọc này.
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {courts.map((court) => (
                <div 
                  key={court.courtId} 
                  className="card bg-slate-900 border-slate-800 overflow-hidden p-0 flex flex-col hover:border-slate-700 transition-all duration-300 hover:-translate-y-1 shadow-lg"
                >
                  {/* Card Thumbnail */}
                  <div className="h-48 w-full relative">
                    <img 
                      src={court.imageUrl} 
                      alt={court.courtName} 
                      className="w-full h-full object-cover"
                    />
                    <div className="absolute top-3 right-3 bg-slate-950/80 backdrop-blur-md px-2.5 py-1 rounded-lg border border-slate-800 text-[10px] font-bold text-green-400">
                      {court.pricePerHour.toLocaleString('vi-VN')} đ/h
                    </div>
                  </div>

                  {/* Card Info */}
                  <div className="p-5 flex-1 flex flex-col justify-between space-y-4">
                    <div className="space-y-2">
                      <div className="flex justify-between items-center">
                        <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">
                          {court.courtTypeId === 1 ? 'Pickleball' : court.courtTypeId === 2 ? 'Cầu lông' : 'Bóng đá'}
                        </span>
                        <div className="flex items-center gap-1 text-xs text-yellow-400">
                          <Star className="w-3.5 h-3.5 fill-yellow-400 text-yellow-400" />
                          <span className="font-bold">{court.rating}</span>
                          <span className="text-slate-500">({court.reviewCount})</span>
                        </div>
                      </div>

                      <h3 className="text-lg font-bold text-white hover:text-green-400 transition-colors line-clamp-1">
                        {court.courtName}
                      </h3>

                      <div className="space-y-1.5 text-xs text-slate-400">
                        <span className="flex items-center gap-1.5">
                          <MapPin className="w-4 h-4 text-green-500" /> {court.location}
                        </span>
                        <span className="flex items-center gap-1.5">
                          <Clock className="w-4 h-4 text-green-500" /> {court.openTime} - {court.closeTime}
                        </span>
                      </div>
                    </div>

                    <div className="pt-4 border-t border-slate-850 flex gap-2">
                      <button 
                        onClick={() => navigate(`/courts/${court.courtId}`)}
                        className="flex-1 btn-secondary text-xs py-2 px-3 hover:bg-slate-800 hover:text-white"
                      >
                        Chi tiết sân
                      </button>
                      <button 
                        onClick={() => navigate(`/booking/${court.courtId}`)}
                        className="flex-1 btn-primary text-xs py-2 px-3 flex items-center justify-center gap-1"
                      >
                        Đặt sân ngay <ArrowRight className="w-3.5 h-3.5" />
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

      </div>
    </div>
  );
}
