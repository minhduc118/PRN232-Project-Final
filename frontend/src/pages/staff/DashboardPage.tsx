import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { getStaffStats } from "@/api/staffApi";
import { ROUTES } from "@/constants/routes";
import Navbar from "@/components/Navbar";
import {
  Users,
  Package,
  Calendar,
  Activity,
  RefreshCw,
  TrendingUp,
  CheckCircle,
  Clock,
} from "lucide-react";
import toast from "react-hot-toast";

export default function StaffDashboardPage() {
  const [stats, setStats] = useState<any>(null);
  const [recentBookings, setRecentBookings] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchStats = async () => {
    try {
      setLoading(true);
      const data = await getStaffStats();
      setStats(data.stats);
      setRecentBookings(data.recentBookings);
    } catch (err: unknown) {
      const message =
        err instanceof Error
          ? err.message
          : "Không thể tải thống kê bảng điều khiển.";
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchStats();
  }, []);

  // Format currency
  const formatPrice = (value: number) => {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(value);
  };

  // Helper for booking status badge style
  const getStatusBadgeClass = (status: string) => {
    switch (status.toLowerCase()) {
      case "confirmed":
      case "success":
        return "bg-green-500/10 text-green-400 border border-green-500/20";
      case "pending":
        return "bg-yellow-500/10 text-yellow-400 border border-yellow-500/20";
      case "cancelled":
      case "failed":
        return "bg-red-500/10 text-red-400 border border-red-500/20";
      default:
        return "bg-slate-800 text-slate-400 border border-slate-700";
    }
  };

  const getStatusText = (status: string) => {
    switch (status.toLowerCase()) {
      case "confirmed":
      case "success":
        return "Đã xác nhận";
      case "pending":
        return "Chờ duyệt";
      case "cancelled":
        return "Đã hủy";
      default:
        return status;
    }
  };

  if (loading && !stats) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-950 text-slate-200">
        <RefreshCw className="w-8 h-8 animate-spin text-green-500" />
      </div>
    );
  }

  return (
    <>
      <Navbar />
      <div className="min-h-screen bg-slate-950 text-slate-100 py-10 px-4 md:px-8 relative overflow-hidden">
      {/* Background Glowing Blobs */}
      <div className="absolute top-1/4 left-1/4 w-[500px] h-[500px] bg-green-500/5 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-1/4 right-1/4 w-[500px] h-[500px] bg-emerald-500/5 rounded-full blur-3xl pointer-events-none" />

      <div className="max-w-7xl mx-auto relative z-10">
        {/* Header Dashboard */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-8 gap-4 animate-fade-in">
          <div>
            <h1 className="text-3xl font-extrabold text-white tracking-tight bg-gradient-to-r from-white via-slate-200 to-slate-400 bg-clip-text text-transparent">
              Trang quản trị nhân viên
            </h1>
            <p className="text-slate-400 text-sm mt-1">
              Theo dõi trạng thái đặt sân, quản lý dụng cụ và thông tin tài
              khoản khách hàng thời gian thực.
            </p>
          </div>
          <button
            onClick={fetchStats}
            disabled={loading}
            className="btn-secondary self-start sm:self-center flex items-center gap-2 border border-slate-700 px-4 py-2 text-xs"
          >
            <RefreshCw
              className={`w-3.5 h-3.5 ${loading ? "animate-spin" : ""}`}
            />
            Làm mới dữ liệu
          </button>
        </div>

        {/* Core Stats Grid */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8 animate-slide-up">
          {/* Card 1: Total Customers */}
          <Link
            to={ROUTES.STAFF_CUSTOMERS}
            className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 backdrop-blur-md shadow-xl flex items-center gap-5 hover:scale-[1.02] hover:border-green-500/30 transition-all duration-300 cursor-pointer text-slate-100"
          >
            <div className="w-12 h-12 rounded-xl bg-green-500/10 border border-green-500/25 flex items-center justify-center text-green-400">
              <Users className="w-6 h-6" />
            </div>
            <div>
              <span className="text-xs text-slate-400 font-medium">
                Khách hàng thành viên
              </span>
              <strong className="block text-2xl font-black text-white mt-1 leading-none">
                {stats?.totalCustomers || 0}
              </strong>
            </div>
          </Link>

          {/* Card 2: Total Equipment */}
          <Link
            to={ROUTES.STAFF_EQUIPMENT}
            className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 backdrop-blur-md shadow-xl flex items-center gap-5 hover:scale-[1.02] hover:border-emerald-500/30 transition-all duration-300 cursor-pointer text-slate-100"
          >
            <div className="w-12 h-12 rounded-xl bg-emerald-500/10 border border-emerald-500/25 flex items-center justify-center text-emerald-400">
              <Package className="w-6 h-6" />
            </div>
            <div>
              <span className="text-xs text-slate-400 font-medium">
                Tổng thiết bị dụng cụ
              </span>
              <strong className="block text-2xl font-black text-white mt-1 leading-none">
                {stats?.totalEquipments || 0}
              </strong>
            </div>
          </Link>

          {/* Card 3: Today's Bookings */}
          <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 backdrop-blur-md shadow-xl flex items-center gap-5 hover:scale-[1.02] transition-transform duration-300">
            <div className="w-12 h-12 rounded-xl bg-blue-500/10 border border-blue-500/25 flex items-center justify-center text-blue-400">
              <Calendar className="w-6 h-6" />
            </div>
            <div>
              <span className="text-xs text-slate-400 font-medium">
                Lượt đặt sân hôm nay
              </span>
              <strong className="block text-2xl font-black text-white mt-1 leading-none">
                {stats?.todayBookings || 0}
              </strong>
            </div>
          </div>

          {/* Card 4: Available Courts */}
          <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 backdrop-blur-md shadow-xl flex items-center gap-5 hover:scale-[1.02] transition-transform duration-300">
            <div className="w-12 h-12 rounded-xl bg-teal-500/10 border border-teal-500/25 flex items-center justify-center text-teal-400">
              <CheckCircle className="w-6 h-6" />
            </div>
            <div>
              <span className="text-xs text-slate-400 font-medium">
                Số sân hiện đang trống
              </span>
              <strong className="block text-2xl font-black text-white mt-1 leading-none">
                {stats?.availableCourts || 0}
              </strong>
            </div>
          </div>
        </div>

        {/* Dashboard Panels Grid */}
        <div className="grid grid-cols-12 gap-8 animate-slide-up [animation-delay:150ms]">
          {/* Left panel: Today's bookings */}
          <div className="col-span-12 lg:col-span-8 bg-slate-900/60 border border-slate-800 rounded-2xl p-6 backdrop-blur-md shadow-xl">
            <h3 className="font-bold text-white text-base mb-6 flex items-center gap-2">
              <Activity className="w-5 h-5 text-green-400" />
              Lịch đặt sân hôm nay
            </h3>

            {recentBookings.length === 0 ? (
              <div className="py-12 text-center text-slate-500">
                <Clock className="w-8 h-8 text-slate-650 mx-auto mb-2" />
                <span className="text-sm">
                  Hôm nay chưa có lượt đặt sân nào được đăng ký.
                </span>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm text-left text-slate-350">
                  <thead className="text-xs text-slate-400 uppercase bg-slate-900/50 border-b border-slate-850">
                    <tr>
                      <th scope="col" className="px-4 py-3">
                        Mã Booking
                      </th>
                      <th scope="col" className="px-4 py-3">
                        Khách hàng
                      </th>
                      <th scope="col" className="px-4 py-3">
                        Sân / Khung giờ
                      </th>
                      <th scope="col" className="px-4 py-3">
                        Tổng thanh toán
                      </th>
                      <th scope="col" className="px-4 py-3">
                        Trạng thái
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {recentBookings.map((bk) => (
                      <tr
                        key={bk.bookingId}
                        className="border-b border-slate-850 hover:bg-slate-900/20 transition-colors"
                      >
                        <td className="px-4 py-4 font-bold text-white">
                          {bk.bookingCode}
                        </td>
                        <td className="px-4 py-4 text-slate-200">
                          {bk.customerName}
                        </td>
                        <td className="px-4 py-4">
                          <span className="block text-slate-300 font-medium">
                            {bk.courtName}
                          </span>
                          <span className="block text-xs text-slate-500">
                            {bk.slotName}
                          </span>
                        </td>
                        <td className="px-4 py-4 font-semibold text-green-400">
                          {formatPrice(bk.totalAmount)}
                        </td>
                        <td className="px-4 py-4">
                          <span
                            className={`badge px-2.5 py-0.5 text-[10px] uppercase font-bold rounded-full ${getStatusBadgeClass(bk.status)}`}
                          >
                            {getStatusText(bk.status)}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          {/* Right panel: Staff Instructions & Shortcuts */}
          <div className="col-span-12 lg:col-span-4 space-y-6">
            {/* Shifts & Tasks summary box */}
            <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 backdrop-blur-md shadow-xl">
              <h3 className="font-bold text-white text-base mb-4 flex items-center gap-2">
                <CheckCircle className="w-5 h-5 text-amber-500" />
                Hướng dẫn nhiệm vụ Staff
              </h3>
              <div className="space-y-4 text-xs text-slate-400">
                <Link 
                  to={ROUTES.STAFF_EQUIPMENT} 
                  className="block p-3 bg-slate-800/40 rounded-xl border border-slate-850 hover:bg-slate-800/80 hover:border-emerald-500/20 transition-all duration-300 group text-slate-400"
                >
                  <strong className="block text-slate-200 mb-1 group-hover:text-emerald-400 transition-colors">
                    1. Quản lý kho dụng cụ &rarr;
                  </strong>
                  <span>
                    Theo dõi tình trạng hỏng hóc, sửa đổi thiết bị và cập nhật
                    số lượng tồn kho liên kết với dịch vụ cho thuê tương ứng.
                  </span>
                </Link>

                <Link 
                  to={ROUTES.STAFF_CUSTOMERS} 
                  className="block p-3 bg-slate-800/40 rounded-xl border border-slate-850 hover:bg-slate-800/80 hover:border-green-500/20 transition-all duration-300 group text-slate-400"
                >
                  <strong className="block text-slate-200 mb-1 group-hover:text-green-400 transition-colors">
                    2. Quản lý thông tin khách hàng &rarr;
                  </strong>
                  <span>
                    Hỗ trợ tạo mới tài khoản cho khách đặt sân trực tiếp, điều
                    chỉnh tích lũy loyalty points và cập nhật phân hạng thành
                    viên.
                  </span>
                </Link>

                <div className="p-3 bg-slate-800/40 rounded-xl border border-slate-850 text-slate-400">
                  <strong className="block text-slate-200 mb-1">
                    3. Kiểm tra check-in sân
                  </strong>
                  <span>
                    Đối chiếu mã đặt sân (Booking Code) khi khách đến nhận sân
                    đúng khung giờ hiển thị bên bảng điều khiển.
                  </span>
                </div>
              </div>
            </div>

            {/* Quick Stats Progress */}
            <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-6 backdrop-blur-md shadow-xl">
              <div className="flex items-center justify-between mb-4">
                <h4 className="font-bold text-white text-xs uppercase tracking-wider text-slate-400">
                  Hiệu năng hệ thống
                </h4>
                <TrendingUp className="w-4 h-4 text-green-400" />
              </div>
              <div className="space-y-4">
                <div>
                  <div className="flex justify-between text-xs text-slate-400 mb-1.5">
                    <span>Dụng cụ sẵn sàng phục vụ</span>
                    <span className="text-green-400 font-semibold">80%</span>
                  </div>
                  <div className="h-1.5 w-full bg-slate-850 rounded-full overflow-hidden">
                    <div
                      className="h-full bg-green-500 rounded-full"
                      style={{ width: "80%" }}
                    />
                  </div>
                </div>

                <div>
                  <div className="flex justify-between text-xs text-slate-400 mb-1.5">
                    <span>Tỷ lệ lấp đầy sân hôm nay</span>
                    <span className="text-blue-400 font-semibold">65%</span>
                  </div>
                  <div className="h-1.5 w-full bg-slate-850 rounded-full overflow-hidden">
                    <div
                      className="h-full bg-blue-500 rounded-full"
                      style={{ width: "65%" }}
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    </>
  );
}
