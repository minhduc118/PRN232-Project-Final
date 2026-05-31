import { useNavigate } from 'react-router-dom';
import Navbar from '@/components/Navbar';
import { ArrowRight, Trophy, Shield, Calendar, Users, Activity } from 'lucide-react';

export default function HomePage() {
  const navigate = useNavigate();

  const features = [
    {
      icon: <Calendar className="w-6 h-6 text-green-400" />,
      title: 'Đặt chỗ nhanh chóng',
      description: 'Lựa chọn sân chơi và khung giờ mong muốn chỉ trong vòng 30 giây.'
    },
    {
      icon: <Shield className="w-6 h-6 text-green-400" />,
      title: 'Thanh toán bảo mật',
      description: 'Hỗ trợ thanh toán an toàn, linh hoạt qua VNPay, chuyển khoản ngân hàng.'
    },
    {
      icon: <Trophy className="w-6 h-6 text-green-400" />,
      title: 'Sân chơi chuẩn quốc tế',
      description: 'Tất cả các sân đấu đều đạt tiêu chuẩn quốc tế BWF, polymer cao cấp, có mái che.'
    },
    {
      icon: <Users className="w-6 h-6 text-green-400" />,
      title: 'Ghép cặp tìm đối thủ',
      description: 'Tính năng Matching tìm bạn cùng chơi, cùng trình độ dễ dàng.'
    }
  ];

  return (
    <div className="min-h-screen bg-slate-950 flex flex-col text-slate-150">
      <Navbar />

      {/* Hero Section */}
      <section className="relative flex-1 flex items-center py-20 px-6 overflow-hidden">
        {/* Glow Effects */}
        <div className="absolute top-10 left-10 w-96 h-96 bg-green-500/10 rounded-full blur-3xl" />
        <div className="absolute bottom-10 right-10 w-96 h-96 bg-emerald-500/10 rounded-full blur-3xl" />

        <div className="max-w-7xl mx-auto w-full grid grid-cols-1 lg:grid-cols-2 gap-12 items-center relative z-10">
          <div className="space-y-6 text-left">
            <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-green-500/10 text-green-400 border border-green-500/20 uppercase tracking-widest">
              <Activity className="w-3.5 h-3.5" /> Thể thao đỉnh cao
            </span>
            <h1 className="text-4xl md:text-6xl font-extrabold text-white leading-tight tracking-tight">
              Đặt sân chơi <br />
              <span className="text-transparent bg-clip-text bg-gradient-to-r from-green-400 to-emerald-300">
                Nhận lịch tức thì
              </span>
            </h1>
            <p className="text-slate-300 text-sm md:text-base leading-relaxed max-w-lg">
              Hệ thống đặt sân thể thao tối ưu hàng đầu. Phù hợp cho những trận giao hữu cầu lông kịch tính, các set đấu Pickleball thời thượng hay trận bóng đá mini nảy lửa.
            </p>
            
            <div className="pt-4 flex flex-wrap gap-4">
              <button
                onClick={() => navigate('/courts')}
                className="btn-primary px-8 py-4 text-base font-bold shadow-lg shadow-green-500/20 flex items-center gap-2 group"
              >
                Khám phá sân ngay 
                <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
              </button>
              <button
                onClick={() => navigate('/login')}
                className="btn-secondary px-8 py-4 text-base font-bold"
              >
                Đăng nhập tài khoản
              </button>
            </div>
          </div>

          {/* Banner Graphic Showcase */}
          <div className="relative rounded-3xl overflow-hidden shadow-2xl border border-slate-800 bg-slate-900 group">
            <img 
              src="https://images.unsplash.com/photo-1554068865-24cecd4e34b8?w=800" 
              alt="Sports Court Banner" 
              className="w-full h-80 md:h-[450px] object-cover group-hover:scale-105 transition-transform duration-700"
            />
            <div className="absolute inset-0 bg-gradient-to-t from-slate-950 via-slate-950/20 to-transparent" />
            <div className="absolute bottom-8 left-8 right-8 text-white space-y-2">
              <span className="text-xs font-bold text-green-400 uppercase tracking-wider">Hạ tầng cao cấp</span>
              <h3 className="text-2xl font-bold">Tổ hợp sân Pickleball & Badminton</h3>
              <p className="text-slate-300 text-xs leading-relaxed">
                Được trang bị thảm sàn polymer chống trơn trượt chuẩn thi đấu Olympic cùng dàn đèn LED cao áp chuẩn truyền hình trực tiếp.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Feature section */}
      <section className="bg-slate-900/50 border-t border-slate-900 py-16 px-6 relative z-10">
        <div className="max-w-7xl mx-auto space-y-12">
          <div className="text-center space-y-2">
            <h2 className="text-3xl font-extrabold text-white">Tại sao chọn chúng tôi?</h2>
            <p className="text-slate-400 text-sm">Trải nghiệm dịch vụ thể thao chất lượng và tiện ích tối đa</p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {features.map((f, idx) => (
              <div 
                key={idx} 
                className="p-6 bg-slate-900 border border-slate-800 rounded-2xl space-y-4 hover:border-green-500/35 transition-colors"
              >
                <div className="w-12 h-12 rounded-xl bg-slate-950 flex items-center justify-center border border-slate-850">
                  {f.icon}
                </div>
                <h3 className="text-lg font-bold text-white">{f.title}</h3>
                <p className="text-slate-400 text-xs leading-relaxed">{f.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="border-t border-slate-900 bg-slate-950 py-8 px-6 text-center text-xs text-slate-500">
        <div className="max-w-7xl mx-auto flex flex-col sm:flex-row justify-between items-center gap-4">
          <span>&copy; {new Date().getFullYear()} SportsCourt Center. Mọi quyền được bảo lưu.</span>
          <div className="flex gap-4">
            <a href="#about" className="hover:text-slate-350 transition-colors">Về chúng tôi</a>
            <a href="#policy" className="hover:text-slate-350 transition-colors">Điều khoản dịch vụ</a>
            <a href="#support" className="hover:text-slate-350 transition-colors">Hỗ trợ khách hàng</a>
          </div>
        </div>
      </footer>
    </div>
  );
}
