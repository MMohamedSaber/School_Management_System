
namespace SchoolManagementSystem.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendAssignmentGradedEmailAsync(string studentEmail, string studentName, string assignmentTitle, decimal grade);
        Task SendClassEnrollmentEmailAsync(string studentEmail, string studentName, string className, string courseName, string teacherName);

    }
}
