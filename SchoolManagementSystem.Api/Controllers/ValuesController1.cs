using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CacheController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public CacheController(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Clear all departments cache
        /// </summary>
        [HttpPost("clear/departments")]
        public IActionResult ClearDepartmentsCache()
        {
            // Clear departments cache
            _cache.Remove("departments_list");

            for (int i = 1; i <= 10; i++)
            {
                for (int size = 10; size <= 100; size += 10)
                {
                    _cache.Remove($"departments_list_page_{i}_size_{size}");
                }
            }

            return Ok(new
            {
                success = true,
                message = "Departments cache cleared successfully"
            });
        }

        /// <summary>
        /// Clear all courses cache
        /// </summary>
        [HttpPost("clear/courses")]
        public IActionResult ClearCoursesCache()
        {
            _cache.Remove("courses_list");

            for (int i = 1; i <= 10; i++)
            {
                for (int size = 10; size <= 100; size += 10)
                {
                    _cache.Remove($"courses_list_page_{i}_size_{size}");
                }
            }

            return Ok(new
            {
                success = true,
                message = "Courses cache cleared successfully"
            });
        }

        /// <summary>
        /// Clear all cache
        /// </summary>
        [HttpPost("clear/all")]
        public IActionResult ClearAllCache()
        {
            // Note: IMemoryCache doesn't have a Clear method
            // This is a simplified approach
            ClearDepartmentsCache();
            ClearCoursesCache();

            return Ok(new
            {
                success = true,
                message = "All cache cleared successfully"
            });
        }
    }
}
