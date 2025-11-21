// API/Middleware/GlobalExceptionHandlerMiddleware.cs
using System.Net;
using System.Text.Json;

namespace SchoolManagementSystem.API.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Success = false,
                TraceId = context.TraceIdentifier
            };

            switch (exception)
            {
                case KeyNotFoundException keyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = keyNotFoundException.Message;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case UnauthorizedAccessException unauthorizedException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = unauthorizedException.Message;
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case InvalidOperationException invalidOperationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = invalidOperationException.Message;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case ArgumentNullException argumentNullException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = $"Required parameter is missing: {argumentNullException.ParamName}";
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case ArgumentException argumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = argumentException.Message;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = _env.IsDevelopment()
                        ? exception.Message
                        : "An internal server error occurred. Please try again later.";
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    // في Development mode، نضيف تفاصيل أكتر
                    if (_env.IsDevelopment())
                    {
                        response.Details = exception.StackTrace;
                    }
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string TraceId { get; set; }
        public string Details { get; set; } // Only in Development
    }

    // Extension method for easy registration
    public static class GlobalExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}