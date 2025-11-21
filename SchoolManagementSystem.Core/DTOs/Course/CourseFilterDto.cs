namespace SchoolManagementSystem.Core.DTOs.Course
{
    public class CourseFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public int? DepartmentId { get; set; }
        public int? MinCredits { get; set; }
        public int? MaxCredits { get; set; }
    }
}
