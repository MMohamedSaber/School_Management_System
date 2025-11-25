
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchoolManagementSystem.Core.Interfaces;
using SchoolManagementSystem.Core.Settings;
using System.Net;
using System.Net.Mail;

namespace SchoolManagementSystem.Infrastructure.Services
{

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                    message.To.Add(new MailAddress(toEmail));
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                    {
                        smtpClient.Credentials = new NetworkCredential(
                            _emailSettings.Username,
                            _emailSettings.Password);
                        smtpClient.EnableSsl = _emailSettings.EnableSsl;

                        await smtpClient.SendMailAsync(message);
                        _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                // Don't throw - email failure shouldn't break the application
            }
        }

        public async Task SendAssignmentGradedEmailAsync(
            string studentEmail,
            string studentName,
            string assignmentTitle,
            decimal grade)
        {
            var subject = $"Assignment Graded: {assignmentTitle}";
            var body = GetAssignmentGradedEmailTemplate(studentName, assignmentTitle, grade);

            await SendEmailAsync(studentEmail, subject, body);
        }

        public async Task SendClassEnrollmentEmailAsync(
            string studentEmail,
            string studentName,
            string className,
            string courseName,
            string teacherName)
        {
            var subject = $"Enrolled in {className}";
            var body = GetClassEnrollmentEmailTemplate(studentName, className, courseName, teacherName);

            await SendEmailAsync(studentEmail, subject, body);
        }

        private string GetAssignmentGradedEmailTemplate(string studentName, string assignmentTitle, decimal grade)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin: 20px 0; }}
        .grade {{ font-size: 36px; font-weight: bold; color: #4CAF50; text-align: center; margin: 20px 0; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Assignment Graded!</h1>
        </div>
        <div class='content'>
            <p>Dear {studentName},</p>
            <p>Your assignment has been graded by your teacher.</p>
            <p><strong>Assignment:</strong> {assignmentTitle}</p>
            <div class='grade'>{grade}%</div>
            <p>You can view detailed feedback and remarks by logging into the system.</p>
            <p>Keep up the good work!</p>
        </div>
        <div class='footer'>
            <p>School Management System</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetClassEnrollmentEmailTemplate(
            string studentName,
            string className,
            string courseName,
            string teacherName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 20px; margin: 20px 0; }}
        .info-box {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #2196F3; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to {className}!</h1>
        </div>
        <div class='content'>
            <p>Dear {studentName},</p>
            <p>You have been successfully enrolled in a new class.</p>
            <div class='info-box'>
                <p><strong>Class:</strong> {className}</p>
                <p><strong>Course:</strong> {courseName}</p>
                <p><strong>Teacher:</strong> {teacherName}</p>
            </div>
            <p>You can now:</p>
            <ul>
                <li>View class materials and assignments</li>
                <li>Track your attendance</li>
                <li>Submit assignments</li>
                <li>Check your grades</li>
            </ul>
            <p>Login to the system to get started!</p>
        </div>
        <div class='footer'>
            <p>School Management System</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}

