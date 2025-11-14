
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }


    }

}
