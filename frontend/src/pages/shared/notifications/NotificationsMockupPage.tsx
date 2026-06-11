import { useState } from "react";
import Navbar from "@/components/Navbar";
import {
  Mail,
  Smartphone,
  Copy,
  Send,
  RefreshCw,
  Check,
  Info,
  Eye,
} from "lucide-react";
import toast from "react-hot-toast";

type TemplateType =
  | "confirmation"
  | "receipt"
  | "cancellation"
  | "waitlist"
  | "membership";

export default function NotificationsMockupPage() {
  // Variables for dynamic templates
  const [customerName, setCustomerName] = useState("Nguyễn Văn Hùng");
  const [courtName, setCourtName] = useState("Sân Pickleball VIP");
  const [bookingCode, setBookingCode] = useState("BK-20260515-0002");
  const [bookingDate, setBookingDate] = useState("2026-06-15");
  const [slotTime, setSlotTime] = useState("18:00 - 19:00 (Ca 12)");
  const [amount, setAmount] = useState(135000);
  const [points, setPoints] = useState(1250);
  const [tier, setTier] = useState("Silver");

  // Preview format: email (default), sms/push
  const [previewChannel, setPreviewChannel] = useState<"email" | "mobile">(
    "email",
  );
  // Email preview mode: visual vs raw html code
  const [emailMode, setEmailMode] = useState<"visual" | "code">("visual");
  // Selected template
  const [selectedTemplate, setSelectedTemplate] =
    useState<TemplateType>("confirmation");
  const [copied, setCopied] = useState(false);
  const [sending, setSending] = useState(false);

  // Generate Email Templates HTML
  const getEmailSubject = () => {
    switch (selectedTemplate) {
      case "confirmation":
        return `[SportsCourt] Xác nhận đặt sân thành công - #${bookingCode}`;
      case "receipt":
        return `[SportsCourt] Hóa đơn điện tử thanh toán - #${bookingCode}`;
      case "cancellation":
        return `[SportsCourt] Xác nhận hủy đặt sân & Hoàn tiền - #${bookingCode}`;
      case "waitlist":
        return `[SportsCourt] CƠ HỘI ĐẶT SÂN: Khung giờ vàng ca ${slotTime} đang trống!`;
      case "membership":
        return `[SportsCourt] Chúc mừng bạn đã nâng hạng thành viên lên ${tier}!`;
    }
  };

  const generateEmailHtml = () => {
    const formattedAmount = amount.toLocaleString("vi-VN") + " đ";
    const refundAmount =
      Math.round(amount * 0.5).toLocaleString("vi-VN") + " đ"; // 50% refund example

    // Common Styles
    const containerStyle =
      "max-width: 600px; margin: 0 auto; font-family: 'Segoe UI', Arial, sans-serif; background-color: #ffffff; border-radius: 16px; overflow: hidden; border: 1px solid #e2e8f0; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);";
    const headerStyle =
      "background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 32px 24px; text-align: center; color: #ffffff;";
    const footerStyle =
      "background-color: #f8fafc; padding: 24px; text-align: center; font-size: 12px; color: #64748b; border-top: 1px solid #f1f5f9;";

    switch (selectedTemplate) {
      case "confirmation":
        return `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>Xác nhận đặt sân</title>
</head>
<body style="background-color: #f1f5f9; padding: 40px 10px; margin: 0;">
  <div style="${containerStyle}">
    <!-- Header -->
    <div style="${headerStyle}">
      <h1 style="margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.5px;">XÁC NHẬN ĐẶT SÂN</h1>
      <p style="margin: 8px 0 0 0; font-size: 14px; opacity: 0.9;">Mã đơn đặt: #${bookingCode}</p>
    </div>

    <!-- Content -->
    <div style="padding: 32px 24px; color: #1e293b; line-height: 1.6;">
      <p style="margin: 0 0 16px 0; font-size: 15px;">Chào <strong>${customerName}</strong>,</p>
      <p style="margin: 0 0 24px 0; font-size: 14px; color: #475569;">Đơn đặt sân thể thao của bạn đã được hệ thống ghi nhận thành công. Dưới đây là thông tin chi tiết lịch hẹn chơi của bạn:</p>
      
      <!-- Info Card -->
      <div style="background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px; margin-bottom: 24px;">
        <table style="width: 100%; border-collapse: collapse; font-size: 14px;">
          <tr>
            <td style="padding: 6px 0; color: #64748b; width: 120px;">Sân chơi:</td>
            <td style="padding: 6px 0; color: #1e293b; font-weight: bold;">${courtName}</td>
          </tr>
          <tr>
            <td style="padding: 6px 0; color: #64748b;">Ngày đặt:</td>
            <td style="padding: 6px 0; color: #1e293b;">${bookingDate}</td>
          </tr>
          <tr>
            <td style="padding: 6px 0; color: #64748b;">Thời gian:</td>
            <td style="padding: 6px 0; color: #1e293b; font-weight: bold; color: #059669;">${slotTime}</td>
          </tr>
          <tr>
            <td style="padding: 6px 0; color: #64748b;">Tổng cộng:</td>
            <td style="padding: 6px 0; color: #1e293b; font-weight: bold; font-size: 16px;">${formattedAmount}</td>
          </tr>
        </table>
      </div>

      <!-- Action Button -->
      <div style="text-align: center; margin: 32px 0 20px 0;">
        <a href="http://localhost:5173/my-bookings" style="background-color: #10b981; color: #ffffff; padding: 12px 28px; text-decoration: none; font-weight: bold; font-size: 14px; border-radius: 8px; display: inline-block; box-shadow: 0 4px 6px rgba(16, 185, 129, 0.2);">Xem Lịch Đặt Của Tôi</a>
      </div>

      <!-- Reminder Notice -->
      <div style="border-left: 4px solid #10b981; padding-left: 16px; margin: 28px 0 0 0; font-size: 13px; color: #64748b;">
        <p style="margin: 0 0 4px 0; font-weight: bold; color: #1e293b;">Lưu ý quan trọng:</p>
        <p style="margin: 0;">Vui lòng đến trước 10 phút để chuẩn bị. Xuất trình email này hoặc mã đặt sân tại quầy lễ tân để check-in nhận sân nhanh chóng.</p>
      </div>
    </div>

    <!-- Footer -->
    <div style="${footerStyle}">
      <p style="margin: 0 0 8px 0; font-weight: bold; color: #475569;">SportsCourt Hub</p>
      <p style="margin: 0 0 16px 0;">Địa chỉ: Khu Công Nghệ Cao, Quận 9, TP. Hồ Chí Minh</p>
      <p style="margin: 0; font-size: 10px; opacity: 0.8;">Đây là email tự động gửi từ hệ thống. Vui lòng không trả lời thư này.</p>
    </div>
  </div>
</body>
</html>`;

      case "receipt":
        return `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>Biên lai hóa đơn</title>
</head>
<body style="background-color: #f1f5f9; padding: 40px 10px; margin: 0;">
  <div style="${containerStyle}">
    <!-- Header -->
    <div style="background-color: #1e293b; padding: 32px 24px; color: #ffffff; border-bottom: 3px solid #10b981;">
      <table style="width: 100%; border-collapse: collapse;">
        <tr>
          <td>
            <h1 style="margin: 0; font-size: 22px; font-weight: 800; color: #ffffff;">BIÊN LAI ĐIỆN TỬ</h1>
            <p style="margin: 4px 0 0 0; font-size: 12px; color: #94a3b8;">Mã hóa đơn: #INV-${bookingCode}</p>
          </td>
          <td style="text-align: right; vertical-align: middle;">
            <span style="background-color: #10b981; color: #ffffff; padding: 6px 12px; font-size: 11px; font-weight: bold; border-radius: 20px;">ĐÃ THANH TOÁN</span>
          </td>
        </tr>
      </table>
    </div>

    <!-- Content -->
    <div style="padding: 32px 24px; color: #1e293b; line-height: 1.6;">
      <p style="margin: 0 0 20px 0; font-size: 14px;">Chào <strong>${customerName}</strong>,</p>
      <p style="margin: 0 0 24px 0; font-size: 14px; color: #475569;">Giao dịch thanh toán trực tuyến của bạn đã thành công. Chi tiết hóa đơn thanh toán của bạn như sau:</p>

      <table style="width: 100%; border-collapse: collapse; font-size: 13px; margin-bottom: 24px;">
        <thead>
          <tr style="border-bottom: 2px solid #e2e8f0; color: #64748b; font-weight: bold;">
            <td style="padding: 8px 0;">Mô tả dịch vụ</td>
            <td style="padding: 8px 0; text-align: center;">Ca chơi</td>
            <td style="padding: 8px 0; text-align: right;">Thành tiền</td>
          </tr>
        </thead>
        <tbody>
          <tr style="border-bottom: 1px solid #f1f5f9;">
            <td style="padding: 12px 0;">
              <span style="font-weight: bold; font-size: 14px; color: #1e293b;">Thuê ${courtName}</span><br>
              <span style="font-size: 11px; color: #64748b;">Ngày chơi: ${bookingDate}</span>
            </td>
            <td style="padding: 12px 0; text-align: center; color: #475569;">${slotTime}</td>
            <td style="padding: 12px 0; text-align: right; font-weight: bold; color: #1e293b;">${formattedAmount}</td>
          </tr>
          <tr>
            <td colspan="2" style="padding: 16px 0 8px 0; text-align: right; color: #64748b;">Tạm tính:</td>
            <td style="padding: 16px 0 8px 0; text-align: right; color: #1e293b;">${formattedAmount}</td>
          </tr>
          <tr>
            <td colspan="2" style="padding: 8px 0; text-align: right; color: #64748b;">Giảm giá:</td>
            <td style="padding: 8px 0; text-align: right; color: #10b981;">0 đ</td>
          </tr>
          <tr style="border-top: 1px solid #e2e8f0; font-weight: bold; font-size: 16px;">
            <td colspan="2" style="padding: 16px 0 8px 0; text-align: right; color: #1e293b;">Tổng thanh toán:</td>
            <td style="padding: 16px 0 8px 0; text-align: right; color: #059669;">${formattedAmount}</td>
          </tr>
        </tbody>
      </table>

      <!-- Payment Method detail -->
      <div style="background-color: #f8fafc; border-radius: 8px; padding: 16px; border: 1px solid #e2e8f0; font-size: 12px; color: #64748b;">
        <p style="margin: 0 0 6px 0; color: #1e293b; font-weight: bold;">Chi tiết giao dịch:</p>
        <table style="width: 100%; border-collapse: collapse;">
          <tr>
            <td style="padding: 2px 0;">Cổng thanh toán:</td>
            <td style="padding: 2px 0; text-align: right; color: #1e293b; font-weight: bold;">VNPay</td>
          </tr>
          <tr>
            <td style="padding: 2px 0;">Mã GD Cổng:</td>
            <td style="padding: 2px 0; text-align: right; color: #1e293b; font-family: monospace;">VNP${bookingCode}</td>
          </tr>
        </table>
      </div>
    </div>

    <!-- Footer -->
    <div style="${footerStyle}">
      <p style="margin: 0 0 8px 0; font-weight: bold; color: #475569;">SportsCourt Hub</p>
      <p style="margin: 0 0 16px 0;">Xin chân thành cảm ơn sự đồng hành của bạn!</p>
      <p style="margin: 0; font-size: 10px; opacity: 0.8;">Hóa đơn điện tử được khởi tạo tự động. Quý khách cần hóa đơn đỏ GTGT vui lòng liên hệ lễ tân trong vòng 24h kể từ khi kết thúc giao dịch.</p>
    </div>
  </div>
</body>
</html>`;

      case "cancellation":
        return `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>Xác nhận hủy đặt sân & hoàn phí</title>
</head>
<body style="background-color: #f1f5f9; padding: 40px 10px; margin: 0;">
  <div style="${containerStyle}">
    <!-- Header -->
    <div style="background: linear-gradient(135deg, #ef4444 0%, #b91c1c 100%); padding: 32px 24px; text-align: center; color: #ffffff;">
      <h1 style="margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.5px;">XÁC NHẬN HỦY SÂN</h1>
      <p style="margin: 8px 0 0 0; font-size: 14px; opacity: 0.9;">Đơn đặt đã hủy: #${bookingCode}</p>
    </div>

    <!-- Content -->
    <div style="padding: 32px 24px; color: #1e293b; line-height: 1.6;">
      <p style="margin: 0 0 16px 0; font-size: 15px;">Chào <strong>${customerName}</strong>,</p>
      <p style="margin: 0 0 24px 0; font-size: 14px; color: #475569;">Chúng tôi xác nhận yêu cầu hủy đặt sân chơi của bạn đã được xử lý thành công. Theo chính sách hoàn tiền, giao dịch của bạn đủ điều kiện hoàn phí cụ thể như sau:</p>
      
      <!-- Info Card -->
      <div style="background-color: #fff5f5; border: 1px solid #fee2e2; border-radius: 12px; padding: 20px; margin-bottom: 24px;">
        <table style="width: 100%; border-collapse: collapse; font-size: 13px;">
          <tr>
            <td style="padding: 6px 0; color: #7f1d1d; width: 130px; font-weight: bold;">Sân hủy:</td>
            <td style="padding: 6px 0; color: #1e293b;">${courtName}</td>
          </tr>
          <tr>
            <td style="padding: 6px 0; color: #7f1d1d; font-weight: bold;">Lịch hẹn gốc:</td>
            <td style="padding: 6px 0; color: #1e293b;">${bookingDate} (${slotTime})</td>
          </tr>
          <tr>
            <td style="padding: 6px 0; color: #7f1d1d; font-weight: bold;">Tổng tiền đã đóng:</td>
            <td style="padding: 6px 0; color: #1e293b; text-decoration: line-through;">${formattedAmount}</td>
          </tr>
          <tr style="border-top: 1px solid #fee2e2;">
            <td style="padding: 10px 0 6px 0; color: #b91c1c; font-weight: bold; font-size: 14px;">Số tiền hoàn lại:</td>
            <td style="padding: 10px 0 6px 0; color: #ef4444; font-weight: bold; font-size: 16px;">${refundAmount}</td>
          </tr>
          <tr>
            <td style="padding: 2px 0; color: #7f1d1d; font-size: 11px;">Mức hoàn phí:</td>
            <td style="padding: 2px 0; color: #7f1d1d; font-size: 11px; font-weight: bold;">Hoàn tiền 50% (Hủy từ 12h - 24h trước giờ chơi)</td>
          </tr>
        </table>
      </div>

      <!-- Refund notice -->
      <div style="background-color: #f8fafc; border-radius: 8px; padding: 16px; border: 1px solid #e2e8f0; font-size: 12px; color: #64748b;">
        <p style="margin: 0 0 6px 0; color: #1e293b; font-weight: bold;">Quy trình nhận tiền hoàn trả:</p>
        <ul style="margin: 0; padding-left: 18px;">
          <li style="margin-bottom: 4px;">Tiền hoàn trả sẽ được tự động hoàn lại qua kênh thanh toán gốc của bạn (Cổng VNPay).</li>
          <li>Thời gian nhận được tiền phụ thuộc vào ngân hàng phát hành thẻ của bạn (thường từ 2 - 5 ngày làm việc).</li>
        </ul>
      </div>
    </div>

    <!-- Footer -->
    <div style="${footerStyle}">
      <p style="margin: 0 0 8px 0; font-weight: bold; color: #475569;">SportsCourt Hub</p>
      <p style="margin: 0; font-size: 10px; opacity: 0.8;">Nếu bạn không thực hiện yêu cầu hủy này, vui lòng liên hệ ngay hotline 1900 6868 để bảo vệ tài khoản.</p>
    </div>
  </div>
</body>
</html>`;

      case "waitlist":
        return `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>Hàng chờ có sân trống</title>
</head>
<body style="background-color: #f1f5f9; padding: 40px 10px; margin: 0;">
  <div style="${containerStyle}">
    <!-- Header -->
    <div style="background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); padding: 32px 24px; text-align: center; color: #ffffff;">
      <h1 style="margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.5px;">CƠ HỘI ĐẶT SÂN TRỐNG!</h1>
      <p style="margin: 8px 0 0 0; font-size: 14px; opacity: 0.9;">Thông báo từ hàng chờ SportsCourt</p>
    </div>

    <!-- Content -->
    <div style="padding: 32px 24px; color: #1e293b; line-height: 1.6;">
      <p style="margin: 0 0 16px 0; font-size: 15px;">Chào <strong>${customerName}</strong>,</p>
      <p style="margin: 0 0 24px 0; font-size: 14px; color: #475569;">Tin vui! Một người chơi khác vừa hủy đặt sân ở khung giờ bạn đang đăng ký hàng chờ. Bạn đang đứng đầu danh sách chờ nên có quyền ưu tiên đặt sân ngay bây giờ:</p>
      
      <!-- Info Card -->
      <div style="background-color: #fffbeb; border: 1px solid #fef3c7; border-radius: 12px; padding: 20px; margin-bottom: 24px;">
        <table style="width: 100%; border-collapse: collapse; font-size: 13px;">
          <tr>
            <td style="padding: 6px 0; color: #92400e; width: 120px; font-weight: bold;">Sân trống:</td>
            <td style="padding: 6px 0; color: #1e293b; font-weight: bold;">${courtName}</td>
          </tr>
          <tr>
            <td style="padding: 6px 0; color: #92400e; font-weight: bold;">Ngày chơi:</td>
            <td style="padding: 6px 0; color: #1e293b;">${bookingDate}</td>
          </tr>
          <tr>
            <td style="padding: 6px 0; color: #92400e; font-weight: bold;">Thời gian ca:</td>
            <td style="padding: 6px 0; color: #d97706; font-weight: bold;">${slotTime}</td>
          </tr>
        </table>
      </div>

      <!-- Urgent alert warning banner -->
      <div style="background-color: #fffbeb; border-radius: 8px; padding: 14px; border: 1px solid #fde68a; font-size: 12px; color: #b45309; margin-bottom: 28px; text-align: center;">
        ⚠️ <strong>Lưu ý khẩn:</strong> Bạn chỉ có đúng <strong>15 phút</strong> để bấm giữ sân và hoàn tất thanh toán. Quá 15 phút, suất này sẽ tự động được chuyển nhượng cho khách hàng tiếp theo trong hàng chờ.
      </div>

      <!-- Action Button -->
      <div style="text-align: center; margin: 24px 0;">
        <a href="http://localhost:5173/courts" style="background-color: #d97706; color: #ffffff; padding: 12px 28px; text-decoration: none; font-weight: bold; font-size: 14px; border-radius: 8px; display: inline-block; box-shadow: 0 4px 6px rgba(217, 119, 6, 0.2);">Đặt Sân Ngay Bây Giờ</a>
      </div>
    </div>

    <!-- Footer -->
    <div style="${footerStyle}">
      <p style="margin: 0 0 8px 0; font-weight: bold; color: #475569;">SportsCourt Hub</p>
      <p style="margin: 0; font-size: 10px; opacity: 0.8;">Bạn nhận được tin này do đang đăng ký nhận thông báo hàng chờ ở khung giờ trên. Bạn có thể xóa thông báo hàng chờ bất cứ lúc nào trong ứng dụng.</p>
    </div>
  </div>
</body>
</html>`;

      case "membership":
        return `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
  <title>Chúc mừng nâng hạng thành viên</title>
</head>
<body style="background-color: #0f172a; padding: 40px 10px; margin: 0;">
  <div style="max-width: 600px; margin: 0 auto; font-family: 'Segoe UI', Arial, sans-serif; background-color: #1e293b; border-radius: 16px; overflow: hidden; border: 1px solid #334155; box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.3);">
    <!-- Header -->
    <div style="background: linear-gradient(135deg, #eab308 0%, #ca8a04 100%); padding: 48px 24px; text-align: center; color: #ffffff;">
      <span style="font-size: 32px;">👑</span>
      <h1 style="margin: 12px 0 0 0; font-size: 26px; font-weight: 900; letter-spacing: -0.5px; text-shadow: 0 2px 4px rgba(0,0,0,0.2);">THĂNG HẠNG THÀNH VIÊN</h1>
      <p style="margin: 6px 0 0 0; font-size: 14px; opacity: 0.9;">SportsCourt VIP Club</p>
    </div>

    <!-- Content -->
    <div style="padding: 36px 24px; color: #f1f5f9; line-height: 1.6;">
      <p style="margin: 0 0 16px 0; font-size: 16px; text-align: center;">Kính chào Thượng khách <strong>${customerName}</strong>,</p>
      <p style="margin: 0 0 24px 0; font-size: 14px; color: #cbd5e1; text-align: center;">Chúng tôi hân hạnh thông báo tổng số điểm tích lũy của bạn đã đạt mốc mới. Tài khoản của bạn đã được nâng cấp chính thức lên thứ hạng VIP:</p>
      
      <!-- Tier upgrade display card -->
      <div style="background-color: #0f172a; border: 2px solid #eab308; border-radius: 16px; padding: 24px; text-align: center; margin-bottom: 28px; box-shadow: inset 0 0 20px rgba(234, 179, 8, 0.1);">
        <span style="font-size: 12px; color: #94a3b8; uppercase tracking-widest block font-semibold">CẤP ĐỘ MỚI CỦA BẠN</span>
        <span style="font-size: 30px; font-weight: 955; color: #eab308; display: block; margin: 8px 0; font-weight: bold;">${tier} Member</span>
        <span style="font-size: 13px; color: #34d399; font-weight: bold; display: block; margin-top: 4px;">Điểm tích lũy: ${points.toLocaleString("vi-VN")} điểm</span>
      </div>

      <p style="margin: 0 0 12px 0; font-size: 14px; font-weight: bold; color: #ffffff;">Quyền lợi hạng ${tier} mới của bạn:</p>
      <ul style="margin: 0 0 28px 0; padding-left: 20px; color: #cbd5e1; font-size: 13.5px;">
        <li style="margin-bottom: 8px;"><strong>Giảm ngay 10%</strong> giá trị hóa đơn cho mỗi lần tự đặt sân trực tuyến trên hệ thống.</li>
        <li style="margin-bottom: 8px;">Ưu tiên chọn sân đẹp và đặt lịch giữ chỗ định kỳ trước 7 ngày.</li>
        <li>Miễn phí nước uống tăng lực cho các lượt chơi thuộc ca Vàng định kỳ hàng tuần.</li>
      </ul>

      <!-- Action Button -->
      <div style="text-align: center; margin: 24px 0 12px 0;">
        <a href="http://localhost:5173/profile" style="background-color: #eab308; color: #0f172a; padding: 12px 28px; text-decoration: none; font-weight: bold; font-size: 14px; border-radius: 8px; display: inline-block; box-shadow: 0 4px 6px rgba(234, 179, 8, 0.2);">Kiểm Tra Thẻ Thành Viên</a>
      </div>
    </div>

    <!-- Footer -->
    <div style="background-color: #0f172a; padding: 24px; text-align: center; font-size: 12px; color: #64748b; border-top: 1px solid #1e293b;">
      <p style="margin: 0 0 8px 0; font-weight: bold; color: #94a3b8;">Ban Quản Trị SportsCourt VIP Club</p>
      <p style="margin: 0;">Cảm ơn bạn đã lựa chọn chơi thể thao và đồng hành gắn bó cùng chúng tôi!</p>
    </div>
  </div>
</body>
</html>`;
    }
  };

  // Generate Mobile SMS/Push Notification text template
  const getMobileNotificationText = () => {
    switch (selectedTemplate) {
      case "confirmation":
        return `[SportsCourt] XÁC NHẬN: Dat san ${courtName} ngay ${bookingDate} ca ${slotTime} thanh cong! Ma dat: #${bookingCode}. Chi tiet: http://localhost:5173/my-bookings`;
      case "receipt":
        return `[SportsCourt] HOA DON: Giao dich dat san #${bookingCode} thanh cong. So tien: ${amount.toLocaleString("vi-VN")}d. Phg thuc: VNPay. Cam on Quy khach!`;
      case "cancellation":
        return `[SportsCourt] HUY SAN: Da huy dat san #${bookingCode} theo yeu cau. So tien hoan tra: ${Math.round(amount * 0.5).toLocaleString("vi-VN")}d (hoan 50%) se chuyen vao tk goc cua Quy khach trong 2-5 ngay.`;
      case "waitlist":
        return `[SportsCourt] HANG CHO: Ca ${slotTime} ngay ${bookingDate} tai ${courtName} dang trong! Quy khach co 15 phut de dat truoc khi chuyen luot. Click dat ngay: http://localhost:5173/courts`;
      case "membership":
        return `[SportsCourt] CHUC MUNG: Tai khoan cua Quy khach da duoc thich luy ${points} diem va thang hang thanh vien VIP [${tier}]! Quy khach duoc giam 10% khi dat lich tu ca tiep theo.`;
    }
  };

  const handleCopyHtml = () => {
    const htmlText = generateEmailHtml();
    navigator.clipboard.writeText(htmlText);
    setCopied(true);
    toast.success("Đã sao chép mã nguồn HTML vào Clipboard!");
    setTimeout(() => setCopied(false), 2000);
  };

  const handleSendTest = () => {
    setSending(true);
    toast.loading("Đang khởi tạo dịch vụ gửi thư mẫu...", { id: "send-test" });
    setTimeout(() => {
      setSending(false);
      toast.success(
        `Gửi thử nghiệm thành công! Hộp thư của khách hàng đã nhận được thông báo mẫu [${getEmailSubject()}]`,
        { id: "send-test", duration: 4500 },
      );
    }, 1200);
  };

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col">
      <Navbar />

      <div className="flex-1 max-w-7xl w-full mx-auto px-4 py-8 flex flex-col gap-6">
        {/* Header Title */}
        <div>
          <h1 className="text-2xl font-bold text-white flex items-center gap-2">
            <Mail className="w-6 h-6 text-green-400" />
            Bản mẫu Thông báo & Email mẫu
          </h1>
          <p className="text-xs text-slate-400 mt-1">
            Trang kiểm tra, xem trước và chỉnh sửa động các loại thông báo tự
            động gửi cho khách hàng trong hệ thống.
          </p>
        </div>

        {/* Outer Grid split columns */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
          {/* LEFT PANEL: VARIABLES AND SWITCHES (4 cols) */}
          <div className="lg:col-span-4 space-y-6">
            <div className="card bg-slate-900 border-slate-800 p-6 rounded-2xl border">
              <h3 className="text-sm font-bold text-white mb-4 flex items-center gap-2">
                <RefreshCw className="w-4 h-4 text-green-400" />
                Cấu hình Bản mẫu & Biến số
              </h3>

              {/* Template selector select box */}
              <div className="space-y-4">
                <div className="space-y-1.5">
                  <label className="block text-xs font-bold text-slate-400 uppercase tracking-wider">
                    Chọn Loại Bản Mẫu
                  </label>
                  <select
                    value={selectedTemplate}
                    onChange={(e) =>
                      setSelectedTemplate(e.target.value as TemplateType)
                    }
                    className="w-full bg-slate-950 border border-slate-800 rounded-xl py-2.5 px-3 text-sm text-slate-200 focus:outline-none focus:ring-1 focus:ring-green-400 focus:border-transparent"
                  >
                    <option value="confirmation">Thư Xác nhận đặt sân</option>
                    <option value="receipt">Biên lai / Hóa đơn điện tử</option>
                    <option value="cancellation">
                      Xác nhận Hủy sân & Hoàn phí
                    </option>
                    <option value="waitlist">Báo Hàng chờ có sân trống</option>
                    <option value="membership">Chúc mừng Thăng hạng VIP</option>
                  </select>
                </div>

                <div className="border-t border-slate-800/80 my-4 pt-4" />

                {/* Variable fields list */}
                <h4 className="text-xs font-bold text-slate-400 uppercase tracking-wider mb-2">
                  Chỉnh sửa giá trị động:
                </h4>
                <div className="space-y-3.5">
                  {/* Name */}
                  <div className="space-y-1">
                    <label className="block text-[10px] font-bold text-slate-455 text-slate-500 uppercase">
                      Tên khách hàng
                    </label>
                    <input
                      type="text"
                      value={customerName}
                      onChange={(e) => setCustomerName(e.target.value)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-lg py-1.5 px-3 text-xs focus:outline-none focus:ring-1 focus:ring-green-400"
                    />
                  </div>

                  {/* Booking Code */}
                  <div className="space-y-1">
                    <label className="block text-[10px] font-bold text-slate-455 text-slate-500 uppercase">
                      Mã đặt sân
                    </label>
                    <input
                      type="text"
                      value={bookingCode}
                      onChange={(e) => setBookingCode(e.target.value)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-lg py-1.5 px-3 text-xs focus:outline-none focus:ring-1 focus:ring-green-400"
                    />
                  </div>

                  {/* Court Name */}
                  <div className="space-y-1">
                    <label className="block text-[10px] font-bold text-slate-455 text-slate-500 uppercase">
                      Tên sân thể thao
                    </label>
                    <input
                      type="text"
                      value={courtName}
                      onChange={(e) => setCourtName(e.target.value)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-lg py-1.5 px-3 text-xs focus:outline-none focus:ring-1 focus:ring-green-400"
                    />
                  </div>

                  {/* Slot Details */}
                  <div className="space-y-1">
                    <label className="block text-[10px] font-bold text-slate-455 text-slate-500 uppercase">
                      Khung giờ ca chơi
                    </label>
                    <input
                      type="text"
                      value={slotTime}
                      onChange={(e) => setSlotTime(e.target.value)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-lg py-1.5 px-3 text-xs focus:outline-none focus:ring-1 focus:ring-green-400"
                    />
                  </div>

                  {/* Date */}
                  <div className="space-y-1">
                    <label className="block text-[10px] font-bold text-slate-455 text-slate-500 uppercase">
                      Ngày chơi
                    </label>
                    <input
                      type="date"
                      value={bookingDate}
                      onChange={(e) => setBookingDate(e.target.value)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-lg py-1.5 px-3 text-xs focus:outline-none focus:ring-1 focus:ring-green-400 text-slate-350"
                    />
                  </div>

                  {/* Amount */}
                  <div className="space-y-1">
                    <label className="block text-[10px] font-bold text-slate-455 text-slate-500 uppercase">
                      Số tiền thanh toán (đ)
                    </label>
                    <input
                      type="number"
                      value={amount}
                      onChange={(e) => setAmount(Number(e.target.value))}
                      className="w-full bg-slate-950 border border-slate-800 rounded-lg py-1.5 px-3 text-xs focus:outline-none focus:ring-1 focus:ring-green-400"
                    />
                  </div>

                  {/* For Membership template */}
                  {selectedTemplate === "membership" && (
                    <div className="grid grid-cols-2 gap-2 p-2.5 bg-slate-955 bg-slate-950 rounded-xl border border-slate-800">
                      <div className="space-y-1">
                        <label className="block text-[10px] font-bold text-slate-500 uppercase">
                          Hạng VIP mới
                        </label>
                        <select
                          value={tier}
                          onChange={(e) => setTier(e.target.value)}
                          className="w-full bg-slate-900 border border-slate-800 rounded-lg py-1 px-1.5 text-xs text-white"
                        >
                          <option value="Silver">Bạc (Silver)</option>
                          <option value="Gold">Vàng (Gold)</option>
                          <option value="Platinum">Kim cương (Platinum)</option>
                        </select>
                      </div>
                      <div className="space-y-1">
                        <label className="block text-[10px] font-bold text-slate-500 uppercase">
                          Điểm đạt mốc
                        </label>
                        <input
                          type="number"
                          value={points}
                          onChange={(e) => setPoints(Number(e.target.value))}
                          className="w-full bg-slate-900 border border-slate-800 rounded-lg py-1 px-2 text-xs focus:outline-none focus:ring-1 focus:ring-green-400 text-white"
                        />
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Quick Actions Panel */}
            <div className="card bg-slate-900 border-slate-800 p-4 rounded-xl border flex flex-col gap-2.5">
              <button
                onClick={handleSendTest}
                disabled={sending}
                className="w-full btn-primary py-2.5 rounded-xl font-bold flex items-center justify-center gap-2 shadow-lg shadow-green-500/10 text-xs"
              >
                <Send className="w-4 h-4 text-slate-950" />
                Gửi email thử nghiệm
              </button>
              <button
                onClick={handleCopyHtml}
                disabled={previewChannel !== "email"}
                className="w-full border border-slate-800 hover:border-slate-700 bg-slate-950 text-slate-200 hover:text-white transition-all py-2.5 rounded-xl font-bold text-xs flex items-center justify-center gap-2"
              >
                {copied ? (
                  <Check className="w-4 h-4 text-green-400" />
                ) : (
                  <Copy className="w-4 h-4 text-green-400" />
                )}
                Copy mã nguồn HTML Email
              </button>
            </div>
          </div>

          {/* RIGHT PANEL: EMAIL/SMS LIVE PREVIEWS (8 cols) */}
          <div className="lg:col-span-8 space-y-4">
            {/* Tab channels selector */}
            <div className="flex justify-between items-center bg-slate-900 border border-slate-800 p-1.5 rounded-xl">
              <div className="flex gap-1">
                <button
                  onClick={() => setPreviewChannel("email")}
                  className={`px-4 py-2 rounded-lg text-xs font-bold flex items-center gap-1.5 transition-all ${
                    previewChannel === "email"
                      ? "bg-slate-850 text-white border border-slate-700 shadow-sm"
                      : "text-slate-400 hover:text-white"
                  }`}
                >
                  <Mail className="w-4 h-4 text-green-400" />
                  Mẫu Email (HTML)
                </button>
                <button
                  onClick={() => setPreviewChannel("mobile")}
                  className={`px-4 py-2 rounded-lg text-xs font-bold flex items-center gap-1.5 transition-all ${
                    previewChannel === "mobile"
                      ? "bg-slate-850 text-white border border-slate-700 shadow-sm"
                      : "text-slate-400 hover:text-white"
                  }`}
                >
                  <Smartphone className="w-4 h-4 text-green-400" />
                  Tin nhắn SMS / Push
                </button>
              </div>

              {previewChannel === "email" && (
                <div className="flex border border-slate-800 rounded-lg p-0.5 bg-slate-950 shrink-0">
                  <button
                    onClick={() => setEmailMode("visual")}
                    className={`px-3 py-1 rounded text-[10px] font-bold flex items-center gap-1 transition-all ${
                      emailMode === "visual"
                        ? "bg-green-500 text-slate-950"
                        : "text-slate-400 hover:text-white"
                    }`}
                  >
                    <Eye className="w-3 h-3" />
                    Trực quan
                  </button>
                  <button
                    onClick={() => setEmailMode("code")}
                    className={`px-3 py-1 rounded text-[10px] font-bold transition-all ${
                      emailMode === "code"
                        ? "bg-green-500 text-slate-950"
                        : "text-slate-400 hover:text-white"
                    }`}
                  >
                    Mã HTML
                  </button>
                </div>
              )}
            </div>

            {/* CHANNEL 1: EMAIL PREVIEW CONTAINER */}
            {previewChannel === "email" && (
              <div className="card bg-slate-900 border border-slate-800 rounded-2xl overflow-hidden flex flex-col h-[680px]">
                {/* Simulated Email Client Header Chrome */}
                <div className="bg-slate-850 px-5 py-4 border-b border-slate-800 space-y-2 text-xs">
                  <div className="flex gap-2 items-center">
                    <span className="text-slate-400 w-16 block font-medium">
                      Người gửi:
                    </span>
                    <span className="text-white font-bold flex items-center gap-1">
                      noreply@sportscourt.vn
                      <span className="text-[10px] bg-green-500/10 text-green-400 border border-green-500/20 px-1.5 py-0.25 rounded font-bold">
                        Hệ thống
                      </span>
                    </span>
                  </div>
                  <div className="flex gap-2 items-center">
                    <span className="text-slate-400 w-16 block font-medium">
                      Người nhận:
                    </span>
                    <span className="text-white font-semibold">
                      {customerName} &lt;khachhang@sportscourt.vn&gt;
                    </span>
                  </div>
                  <div className="flex gap-2 items-center pt-1">
                    <span className="text-slate-400 w-16 block font-medium">
                      Tiêu đề:
                    </span>
                    <span className="text-green-400 font-bold">
                      {getEmailSubject()}
                    </span>
                  </div>
                </div>

                {/* Email Body Rendering */}
                <div className="flex-1 bg-slate-950 p-4 sm:p-8 overflow-y-auto">
                  {emailMode === "visual" ? (
                    <div
                      className="bg-white rounded-xl shadow-lg border border-slate-200/20 max-w-full overflow-hidden"
                      dangerouslySetInnerHTML={{ __html: generateEmailHtml() }}
                    />
                  ) : (
                    <div className="h-full flex flex-col">
                      <div className="flex justify-between items-center bg-slate-900 px-4 py-2 border-b border-slate-800 rounded-t-xl">
                        <span className="text-[10px] font-mono text-slate-400">
                          EMAIL_TEMPLATE_INDEX.HTML
                        </span>
                        <button
                          onClick={handleCopyHtml}
                          className="text-green-400 hover:text-white transition-colors text-[10px] font-bold flex items-center gap-1"
                        >
                          <Copy className="w-3.5 h-3.5" />
                          Sao chép mã
                        </button>
                      </div>
                      <pre className="flex-1 bg-slate-955 p-4 rounded-b-xl border border-slate-800 text-[11px] font-mono text-slate-350 overflow-auto max-h-[500px]">
                        <code>{generateEmailHtml()}</code>
                      </pre>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* CHANNEL 2: MOBILE SMS / PUSH PREVIEW */}
            {previewChannel === "mobile" && (
              <div className="flex items-center justify-center py-6 bg-slate-900 border border-slate-850 rounded-2xl h-[680px]">
                {/* Mock Phone Container */}
                <div className="w-[300px] h-[580px] bg-slate-950 border-4 border-slate-800 rounded-[45px] shadow-2xl relative p-3 flex flex-col justify-between overflow-hidden">
                  {/* Phone Speaker & Notch */}
                  <div className="absolute top-2 left-1/2 -translate-x-1/2 w-28 h-4 bg-slate-800 rounded-full z-20 flex justify-center items-center">
                    <div className="w-10 h-1 bg-slate-900 rounded-full" />
                  </div>

                  {/* Phone Top Status Bar */}
                  <div className="flex justify-between items-center px-6 pt-3 text-[10px] text-slate-300 font-bold z-10">
                    <span>9:41</span>
                    <div className="flex gap-1.5 items-center">
                      <span>LTE</span>
                      <div className="w-4 h-2 bg-slate-300 rounded-sm" />
                    </div>
                  </div>

                  {/* Simulated Mobile screen Content */}
                  <div className="flex-1 pt-6 px-2.5 flex flex-col gap-4 overflow-y-auto">
                    {/* SMS APP chrome */}
                    <div className="bg-slate-900 border border-slate-800 rounded-2xl p-3 shadow-md">
                      <div className="flex items-center gap-2 border-b border-slate-800 pb-2 mb-2">
                        <div className="w-6 h-6 rounded-full bg-green-500 flex items-center justify-center text-[10px] font-black text-slate-950">
                          S
                        </div>
                        <div>
                          <span className="text-[10px] font-bold text-white block leading-tight">
                            SportsCourt Hub
                          </span>
                          <span className="text-[8px] text-slate-400 block leading-none">
                            Tổng đài SMS
                          </span>
                        </div>
                      </div>

                      {/* SMS Chat Bubbles */}
                      <div className="space-y-3">
                        <div className="bg-slate-800 border border-slate-700/80 rounded-2xl p-2.5 text-[11px] text-slate-100 max-w-[85%] rounded-tl-none font-medium leading-relaxed">
                          {getMobileNotificationText()}
                          <span className="block text-[8px] text-slate-400 mt-1.5 text-right font-normal">
                            Hôm nay, 10:42
                          </span>
                        </div>
                      </div>
                    </div>

                    {/* PUSH NOTIFICATION chrome */}
                    <div className="bg-slate-900/95 border border-slate-750/70 rounded-2xl p-3.5 shadow-xl relative mt-4">
                      <div className="flex justify-between items-center mb-1.5">
                        <div className="flex items-center gap-1.5">
                          <div className="w-4 h-4 rounded bg-green-500 flex items-center justify-center text-[8px] font-black text-slate-950">
                            S
                          </div>
                          <span className="text-[9px] font-bold text-white">
                            SPORTSCOURT
                          </span>
                        </div>
                        <span className="text-[8px] text-slate-400 font-medium">
                          1 phút trước
                        </span>
                      </div>
                      <span className="text-[11px] font-bold text-white block mb-0.5">
                        Đặt sân thành công
                      </span>
                      <p className="text-[10px] text-slate-300 leading-normal line-clamp-2">
                        {customerName} ơi, bạn đã hoàn tất đặt sân {courtName}{" "}
                        ca {slotTime}. Chúc bạn chơi vui vẻ!
                      </p>
                    </div>
                  </div>

                  {/* Phone Bottom Home Bar Indicator */}
                  <div className="w-32 h-1 bg-slate-750 bg-slate-800 rounded-full mx-auto mb-1.5" />
                </div>
              </div>
            )}

            {/* Quick Template Tips */}
            <div className="bg-slate-900 border border-slate-800 rounded-xl p-4 flex gap-3 text-xs text-slate-400">
              <Info className="w-5 h-5 text-green-400 shrink-0 mt-0.5" />
              <div>
                <span className="font-semibold text-white block">
                  Tài nguyên lập trình:
                </span>
                <p className="block opacity-90 mt-0.5">
                  Các mã HTML Email ở trên sử dụng thiết kế Table cổ điển (HTML
                  Tables) đảm bảo tương thích 100% với các trình đọc email phổ
                  biến (Outlook, Gmail, Apple Mail) mà không lo bị vỡ khung.
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
