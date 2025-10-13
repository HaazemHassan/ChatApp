using Microsoft.AspNetCore.Http;
using System.Net;

namespace ChatApi.Core.Helpers {
    /// <summary>
    /// Helper class for HttpContext operations
    /// </summary>
    public static class HttpContextHelper {


        /// <summary>
        /// Gets the real client IP address, considering proxy/load balancer headers
        /// </summary>
        /// <returns>The client IP address as string</returns>
        public static string GetClientIpAddress(HttpContext context) {
            // Check X-Forwarded-For header (set by proxies/load balancers)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor)) {
                // X-Forwarded-For can contain multiple IPs (client, proxy1, proxy2, ...)
                // Take the first one (original client IP)
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0) {
                    var clientIp = ips[0].Trim();
                    if (IPAddress.TryParse(clientIp, out _)) {
                        return clientIp;
                    }
                }
            }

            // Check X-Real-IP header (alternative header used by some proxies)
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp) && IPAddress.TryParse(realIp, out _)) {
                return realIp;
            }

            // Fallback to RemoteIpAddress
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }


    }
}
