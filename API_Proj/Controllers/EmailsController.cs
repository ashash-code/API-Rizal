using API_Proj.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace API_Proj.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/emails")]
    [ApiController]
    public class EmailsController: ControllerBase
    {
        private readonly IEmailService emailService;

        public EmailsController(IEmailService emailService)
        {
            this.emailService = emailService;
        }
        public static class VerificationStore
        {
            public static Dictionary<string, (string Code, DateTime CreatedAt)> Codes = new();
        }


        [HttpPost]
        public async Task<IActionResult> SendEmail([FromQuery] string receptor)
        {
            // Generate a fixed 6-digit code
            var code = new Random().Next(100000, 999999).ToString();

            // Subject and body
            var subject = "Your Verification Code";
            var body = $"Your verification code is: {code}";

            await emailService.SendEmail(receptor, subject, body);

            // Store code with timestamp
            VerificationStore.Codes[receptor] = (code, DateTime.UtcNow);
            return Ok(new { code });
        }
        [HttpPost("verify")]
        public IActionResult VerifyCode([FromQuery] string receptor, [FromQuery] string inputCode)
        {
            if (!VerificationStore.Codes.TryGetValue(receptor, out var entry))
                return BadRequest("No code found for this email.");

            var elapsed = DateTime.UtcNow - entry.CreatedAt;
            if (elapsed.TotalMinutes > 10)
                return BadRequest("Verification code has expired.");

            if (inputCode != entry.Code)
                return BadRequest("Incorrect verification code.");

            return Ok("Verification successful.");
        }
    }
}
