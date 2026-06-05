namespace SportCourtManagerment.Services.Email;

/// <summary>Contract for sending transactional emails.</summary>
public interface IEmailService
{
  /// <summary>Sends an OTP verification code to the user's email after registration.</summary>
  Task SendOtpEmailAsync(string toEmail, string toName, string otp);
}
