using System.Text.Json;

namespace Drive_project.Middleware
{
    public class NotFoundMiddleware
    {
        private readonly RequestDelegate _next;
        public NotFoundMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx)
        {
            await _next(ctx);

            if (!ctx.Response.HasStarted && ctx.Response.StatusCode == 404)
            {
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    success = false,
                    title = "Route not found",
                    message = "not Found",
                    stackTrace = (string?)null
                }));
            }
        }
    }

    public static class NotFoundMiddlewareExtensions
    {
        public static IApplicationBuilder UseJsonNotFound(this IApplicationBuilder app)
            => app.UseMiddleware<Drive_project.Middleware.NotFoundMiddleware>();
    }
}

