
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Core.Entities;

namespace SchoolManagementSystem.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }


        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // العلاقة بين Attendance.MarkedByTeacher و User.MarkedAttendances
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.MarkedByTeacher)
                .WithMany(u => u.MarkedAttendances)
                .HasForeignKey(a => a.MarkedByTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // العلاقة بين Attendance.Student و User.StudentAttendances
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(u => u.StudentAttendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة Submission.GradedByTeacher مع User.GradedSubmissions
            modelBuilder.Entity<Submission>()
                .HasOne(s => s.GradedByTeacher)
                .WithMany(u => u.GradedSubmissions)
                .HasForeignKey(s => s.GradedByTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة Assignment.CreatedByTeacher مع User.CreatedAssignments
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.CreatedByTeacher)
                .WithMany(u => u.CreatedAssignments)
                .HasForeignKey(a => a.CreatedByTeacherId)
                .OnDelete(DeleteBehavior.Restrict); // يمنع multiple cascade paths

            modelBuilder.Entity<StudentClass>()
                .HasOne(sc => sc.Student)
                .WithMany(u => u.StudentClasses)
                .HasForeignKey(sc => sc.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // يمنع multiple cascade paths

            modelBuilder.Entity<StudentClass>()
    .HasOne(sc => sc.Class)
    .WithMany(c => c.StudentClasses)
    .HasForeignKey(sc => sc.ClassId)
    .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Submission>()
    .HasOne(s => s.Student)
    .WithMany(u => u.Submissions)
    .HasForeignKey(s => s.StudentId)
    .OnDelete(DeleteBehavior.Restrict);


        }

    }

}
