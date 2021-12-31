using Kledex.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Kledex.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IKledexAppBuilder UseKledex(this IApplicationBuilder app)
        {
            return new KledexAppBuilder(app);
        }

        public static IApplicationBuilder UseKledexMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}