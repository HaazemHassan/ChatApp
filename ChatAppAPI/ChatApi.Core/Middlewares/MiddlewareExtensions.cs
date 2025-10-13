using Microsoft.AspNetCore.Builder;

namespace ChatApi.Core.Middlewares {

    public static class MiddlewareExtensions {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder) {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }

        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder) {
            return builder.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
