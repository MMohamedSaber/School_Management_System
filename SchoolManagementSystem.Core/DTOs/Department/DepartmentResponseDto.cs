namespace SchoolManagementSystem.Core.DTOs.Department
{
    public class DepartmentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? HeadOfDepartmentId { get; set; }
        public string HeadOfDepartmentName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int CoursesCount { get; set; }
    }

}
