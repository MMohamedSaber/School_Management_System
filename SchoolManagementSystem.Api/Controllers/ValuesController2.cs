using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Core.Interfaces;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailTestController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        /// Test email configuration
        /// </summary>
        [HttpPost("send-test")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string toEmail)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Email address is required"
                });
            }

            try
            {
                await _emailService.SendEmailAsync(
                    toEmail,
                    "Test Email - School Management System",
                    "<h1>Email Configuration Test</h1><p>If you receive this email, your SMTP configuration is working correctly!</p>");

                return Ok(new
                {
                    success = true,
                    message = $"Test email sent to {toEmail}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send email",
                    error = ex.Message
                });
            }
        }
    }
}
