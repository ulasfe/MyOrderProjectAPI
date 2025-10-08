using System.Net;
using System.Text.Json;

namespace MyOrderProjectAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // İstek başarılıysa bir sonraki middleware'e geç
                await _next(context);
            }
            catch (Exception ex)
            {
                // Hata yakalandı!
                _logger.LogError(ex, "Global Middleware tarafından yakalanan hata: {Message}", ex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500 Internal Server Error

                // Geliştirme (Development) ortamında hata detaylarını göster, 
                // Üretim (Production) ortamında gizle
                var response = _env.IsDevelopment()
                    ? new ApiExceptionDetails(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    : new ApiExceptionDetails(context.Response.StatusCode, "Sunucuda beklenmeyen bir hata oluştu.");

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }

    // Hata detaylarını taşıyan yardımcı sınıf
    public class ApiExceptionDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string? Details { get; set; }

        public ApiExceptionDetails(int statusCode, string message, string? details = null)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }
    }
}
