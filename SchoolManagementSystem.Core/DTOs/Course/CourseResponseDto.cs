namespace SchoolManagementSystem.Core.DTOs.Course
{
    public class CourseResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int Credits { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int ClassesCount { get; set; }
    }
}
