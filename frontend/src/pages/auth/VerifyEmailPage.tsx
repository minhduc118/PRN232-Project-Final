import { useState, useRef, useEffect } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { verifyEmail } from '@/api/authApi';
import { Activity, Mail, ArrowRight, RefreshCw } from 'lucide-react';
import toast from 'react-hot-toast';

const OTP_LENGTH = 6;

export default function VerifyEmailPage() {
  const navigate  = useNavigate();
  const location  = useLocation();

  // Email is passed from RegisterPage via router state
  const emailFromState = (location.state as { email?: string })?.email ?? '';
  const [email]        = useState(emailFromState);

  // Each OTP digit lives in its own cell
  const [digits, setDigits] = useState<string[]>(Array(OTP_LENGTH).fill(''));
  const inputRefs           = useRef<(HTMLInputElement | null)[]>([]);
  const [loading, setLoading] = useState(false);

  // Focus first cell on mount
  useEffect(() => {
    inputRefs.current[0]?.focus();
  }, []);

  // If no email in state, redirect to register
  useEffect(() => {
    if (!emailFromState) navigate('/register', { replace: true });
  }, [emailFromState, navigate]);

  /** Handle digit input in a cell */
  const handleChange = (index: number, value: string) => {
    if (!/^\d*$/.test(value)) return; // digits only
    const newDigits = [...digits];
    newDigits[index] = value.slice(-1); // keep last char if user pastes
    setDigits(newDigits);

    // Auto-advance to next cell
    if (value && index < OTP_LENGTH - 1) {
      inputRefs.current[index + 1]?.focus();
    }
  };

  /** Handle paste — distribute digits across cells */
  const handlePaste = (e: React.ClipboardEvent) => {
    e.preventDefault();
    const pasted = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, OTP_LENGTH);
    const newDigits = [...digits];
    pasted.split('').forEach((char, i) => { newDigits[i] = char; });
    setDigits(newDigits);
    // Focus the cell after the last pasted digit
    const nextIndex = Math.min(pasted.length, OTP_LENGTH - 1);
    inputRefs.current[nextIndex]?.focus();
  };

  /** Handle backspace — move to previous cell */
  const handleKeyDown = (index: number, e: React.KeyboardEvent) => {
    if (e.key === 'Backspace' && !digits[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const otp = digits.join('');
    if (otp.length < OTP_LENGTH) {
      toast.error('Vui lòng nhập đủ 6 chữ số OTP.');
      return;
    }

    setLoading(true);
    try {
      await verifyEmail({ email, otp });
      toast.success('Xác thực tài khoản thành công! Bạn có thể đăng nhập.');
      navigate('/login', { replace: true });
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Mã OTP không hợp lệ hoặc đã hết hạn.';
      toast.error(message);
      // Clear all cells on error so user can re-enter
      setDigits(Array(OTP_LENGTH).fill(''));
      inputRefs.current[0]?.focus();
    } finally {
      setLoading(false);
    }
  };

  const maskedEmail = email
    ? email.replace(/(.{2}).+(@.+)/, '$1***$2')
    : '';

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center px-4 relative overflow-hidden">
      {/* Background blobs */}
      <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-green-500/10 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute bottom-1/4 right-1/4 w-80 h-80 bg-emerald-400/10 rounded-full blur-3xl pointer-events-none" />

      <div className="w-full max-w-md bg-slate-900/80 border border-slate-800 rounded-2xl p-8 backdrop-blur-md shadow-2xl relative z-10 animate-slide-up">

        {/* Header */}
        <div className="text-center mb-8">
          <div className="inline-flex w-14 h-14 rounded-2xl bg-gradient-to-tr from-green-500 to-emerald-400 items-center justify-center shadow-lg shadow-green-500/30 mb-4">
            <Activity className="w-7 h-7 text-slate-950" />
          </div>
          <h1 className="text-2xl font-bold text-white tracking-tight">Xác thực Email</h1>
          <p className="text-slate-400 text-sm mt-2">
            Chúng tôi đã gửi mã OTP gồm{' '}
            <span className="text-white font-semibold">6 chữ số</span> đến
          </p>
          <div className="flex items-center justify-center gap-2 mt-2">
            <Mail className="w-4 h-4 text-green-400 shrink-0" />
            <span className="text-green-400 font-semibold text-sm">{maskedEmail}</span>
          </div>
          <p className="text-slate-500 text-xs mt-1.5">
            Mã có hiệu lực trong <span className="text-yellow-400 font-semibold">10 phút</span>
          </p>
        </div>

        {/* OTP Form */}
        <form onSubmit={handleSubmit} className="space-y-6">

          {/* 6-digit cells */}
          <div className="flex gap-3 justify-center" onPaste={handlePaste}>
            {digits.map((digit, i) => (
              <input
                key={i}
                id={`otp-digit-${i}`}
                ref={(el) => { inputRefs.current[i] = el; }}
                type="text"
                inputMode="numeric"
                maxLength={1}
                value={digit}
                onChange={(e) => handleChange(i, e.target.value)}
                onKeyDown={(e) => handleKeyDown(i, e)}
                className={`
                  w-12 h-14 text-center text-2xl font-bold rounded-xl border-2 outline-none
                  bg-slate-800 text-white transition-all duration-200
                  ${digit
                    ? 'border-green-500 shadow-[0_0_12px_rgba(34,197,94,0.25)]'
                    : 'border-slate-700 hover:border-slate-600'}
                  focus:border-green-500 focus:shadow-[0_0_16px_rgba(34,197,94,0.35)]
                  caret-green-400 selection:bg-green-500/30
                `}
                aria-label={`OTP chữ số ${i + 1}`}
              />
            ))}
          </div>

          {/* Progress indicator */}
          <div className="flex justify-center gap-1.5">
            {digits.map((d, i) => (
              <div
                key={i}
                className={`h-1 rounded-full transition-all duration-300 ${
                  d ? 'w-8 bg-green-500' : 'w-4 bg-slate-700'
                }`}
              />
            ))}
          </div>

          {/* Submit button */}
          <button
            id="verify-otp-submit"
            type="submit"
            disabled={loading || digits.join('').length < OTP_LENGTH}
            className="w-full btn-primary flex items-center justify-center gap-2 py-3.5 rounded-xl text-base font-bold shadow-lg shadow-green-500/20 disabled:opacity-50 disabled:cursor-not-allowed transition-opacity"
          >
            {loading ? (
              <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
            ) : (
              <>Xác thực tài khoản <ArrowRight className="w-4 h-4" /></>
            )}
          </button>
        </form>

        {/* Helpers */}
        <div className="mt-6 pt-6 border-t border-slate-800 space-y-3 text-center text-sm text-slate-400">
          <div className="flex items-center justify-center gap-2">
            <RefreshCw className="w-3.5 h-3.5" />
            <span>
              Không nhận được mã?{' '}
              <Link
                to="/register"
                className="text-green-400 hover:text-green-300 font-semibold underline underline-offset-2"
              >
                Đăng ký lại
              </Link>{' '}
              để nhận mã mới.
            </span>
          </div>
          <div>
            <Link
              to="/login"
              className="text-slate-500 hover:text-slate-300 text-xs transition-colors"
            >
              ← Quay lại trang đăng nhập
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
