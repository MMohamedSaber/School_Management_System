namespace SchoolManagementSystem.Core.DTOs.User
{
    public class UserFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; }
        public int? Role { get; set; } // 1=Admin, 2=Teacher, 3=Student
        public bool? IsActive { get; set; }
    }
}
