namespace SchoolManagementSystem.Core.DTOs.Student
{
    public class StudentClassDto
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credits { get; set; }
        public string TeacherName { get; set; }
        public string Semester { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime EnrollmentDate { get; set; }



    }
}
