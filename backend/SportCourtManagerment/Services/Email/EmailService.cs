using System.Net;
using System.Net.Mail;

namespace SportCourtManagerment.Services.Email;

/// <summary>
/// Sends transactional emails via SMTP.
/// If SMTP credentials are not configured, the OTP is printed to the console
/// so developers can still test without a real mail server.
/// </summary>
public class EmailService : IEmailService
{
  private readonly IConfiguration _config;
  private readonly ILogger<EmailService> _logger;

  public EmailService(IConfiguration config, ILogger<EmailService> logger)
  {
    _config = config;
    _logger = logger;
  }

  /// <inheritdoc />
  public async Task SendOtpEmailAsync(string toEmail, string toName, string otp)
  {
    var smtpSection = _config.GetSection("Smtp");
    var userName    = smtpSection["UserName"];
    var password    = smtpSection["Password"];

    // ── Fallback: log to console when SMTP is not configured ──────────────────
    if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
    {
      _logger.LogWarning(
        "SMTP not configured — printing OTP to console for development use.");
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("═══════════════════════════════════════════════");
      Console.WriteLine($"  📧  OTP for {toEmail}: {otp}  (expires in 10 minutes)");
      Console.WriteLine("═══════════════════════════════════════════════");
      Console.ResetColor();
      return;
    }

    // ── Real SMTP send ─────────────────────────────────────────────────────────
    var host      = smtpSection["Host"]!;
    var port      = int.Parse(smtpSection["Port"]!);
    var enableSsl = bool.Parse(smtpSection["EnableSsl"]!);
    var fromEmail = smtpSection["FromEmail"]!;
    var fromName  = smtpSection["FromName"]!;

    var body = $"""
      <div style="font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:32px;
                  border:1px solid #e2e8f0;border-radius:12px;">
        <h2 style="color:#22c55e;margin-bottom:4px;">🏟️ SportsCourt Management</h2>
        <p style="color:#64748b;font-size:14px;">Xác thực tài khoản của bạn</p>
        <hr style="border:none;border-top:1px solid #e2e8f0;margin:20px 0;"/>
        <p>Xin chào <strong>{toName}</strong>,</p>
        <p>Mã OTP xác thực tài khoản của bạn là:</p>
        <div style="font-size:40px;font-weight:bold;letter-spacing:12px;
                    color:#1e293b;background:#f1f5f9;padding:20px 24px;
                    border-radius:8px;text-align:center;margin:20px 0;">
          {otp}
        </div>
        <p style="color:#ef4444;font-size:13px;">
          ⏰ Mã này có hiệu lực trong <strong>10 phút</strong>.<br/>
          Tuyệt đối không chia sẻ mã này với người khác.
        </p>
        <hr style="border:none;border-top:1px solid #e2e8f0;margin:20px 0;"/>
        <p style="color:#94a3b8;font-size:12px;">
          Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này.
        </p>
      </div>
      """;

    using var client = new SmtpClient(host, port)
    {
      Credentials = new NetworkCredential(userName, password),
      EnableSsl   = enableSsl,
    };

    using var mail = new MailMessage
    {
      From       = new MailAddress(fromEmail, fromName),
      Subject    = $"[SportsCourt] Mã OTP xác thực tài khoản: {otp}",
      Body       = body,
      IsBodyHtml = true,
    };
    mail.To.Add(new MailAddress(toEmail, toName));

    await client.SendMailAsync(mail);
    _logger.LogInformation("OTP email sent to {Email}", toEmail);
  }
}
