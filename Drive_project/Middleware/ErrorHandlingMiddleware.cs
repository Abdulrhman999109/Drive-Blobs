using System.Text.Json;

namespace Drive_project.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                var status = 500;
                if (ex.Data.Contains("StatusCode") && ex.Data["StatusCode"] is int s) status = s;

                var title = status switch
                {
                    400 => "Validation Failed",
                    401 => "Unauthorized",
                    403 => "Forbidden",
                    404 => "Not Found",
                    _ => "Server Error"
                };

                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = status;

                var payload = new
                {
                    success = false,
                    title,
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                };

                await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        }
    }

    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseJsonErrorHandler(this IApplicationBuilder app)
            => app.UseMiddleware<Drive_project.Middleware.ErrorHandlingMiddleware>();
    }
}
