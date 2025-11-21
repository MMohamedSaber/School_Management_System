
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Core.DTOs.Attendance
{
    public class BulkMarkAttendanceDto
    {
        [Required(ErrorMessage = "Class ID is required")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Attendance records are required")]
        [MinLength(1, ErrorMessage = "At least one attendance record is required")]
        public List<StudentAttendanceDto> Attendances { get; set; }
    }
    public class StudentAttendanceDto
    {
        [Required(ErrorMessage = "Student ID is required")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Range(1, 3, ErrorMessage = "Status must be 1 (Present), 2 (Absent), or 3 (Late)")]
        public int Status { get; set; }
    }
}
