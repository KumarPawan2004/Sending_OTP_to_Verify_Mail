using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Collections.Concurrent;

namespace EmailVerificationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        // Store OTPs temporarily (In real project, use database)
        private static ConcurrentDictionary<string, string> otpStore = new ConcurrentDictionary<string, string>();

        [HttpPost("send-otp")]
        public IActionResult SendOtp([FromBody] EmailRequest request)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            otpStore[request.Email] = otp;

            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("your-email@gmail.com");
                mail.To.Add(request.Email);
                mail.Subject = "Your OTP Verification Code";
                mail.Body = $"Your OTP is {otp}. It is valid for 5 minutes.";

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("xyz@gmail.com", "ffbrs svswvsr sqwspss sasst");
                smtp.EnableSsl = true;

                smtp.Send(mail);
                return Ok(new { message = "OTP sent successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyRequest request)
        {
            if (otpStore.TryGetValue(request.Email, out var storedOtp))
            {
                if (storedOtp == request.Otp)
                {
                    otpStore.TryRemove(request.Email, out _);
                    return Ok(new { message = "Email verified successfully" });
                }
                return BadRequest(new { error = "Invalid OTP" });
            }
            return BadRequest(new { error = "No OTP found for this email" });
        }
    }

    public class EmailRequest
    {
        public string Email { get; set; }
    }

    public class VerifyRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
