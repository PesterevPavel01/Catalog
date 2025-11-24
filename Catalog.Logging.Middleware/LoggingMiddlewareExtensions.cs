using Microsoft.AspNetCore.Builder;

namespace Catalog.Logging.Middleware
{
    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }

        public static IApplicationBuilder UseResponseLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ResponseLoggingMiddleware>();
        }

        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
        {
            return app.UseRequestLogging()
                      .UseResponseLogging();
        }
    }
}
