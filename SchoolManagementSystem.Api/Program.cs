using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SchoolManagementSystem.Infrastructure;

namespace SchoolManagementSystem.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Configure DbContext with SQL Server
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("SchoolManagementSystem.Infrastructure")
                )
            );
            // ===== Swagger =====
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "School Management System API",
                    Version = "v1"
                });
            });

            var app = builder.Build();

            // ===== Enable Swagger UI immediately on run =====
            app.UseSwagger();
            app.UseSwaggerUI();



            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
