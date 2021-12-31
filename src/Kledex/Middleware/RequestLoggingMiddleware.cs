using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;

namespace Kledex.Middleware
{
    /// <summary>
    /// Logs request/response information.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public RequestLoggingMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestInfo = new StringBuilder();
            var shouldReadRequestBody = context.Request.ContentLength < 20_000_000;
            var requestIP = string.Empty;
            try
            {
                try
                {
                    if (shouldReadRequestBody)
                    {
                        if (context.Request.Query?.Any() ?? false)
                        {
                            var query = context.Request.Query.Select(p => $"{p.Key}: {p.Value}").ToList();
                            requestInfo.Append($"Query {JsonConvert.SerializeObject(query)}");
                        }

                        if (context.Request.ContentType?.Contains("json") ?? false)
                        {
                            context.Request.EnableBuffering();
                            if (context.Request.Body.CanSeek)
                            {
                                var bufferSize = context.Request.ContentLength > 5_000
                                    ? 5_000
                                    : (int)context.Request.ContentLength;

                                using var reader = new StreamReader(
                                           context.Request.Body,
                                           encoding: Encoding.UTF8,
                                           detectEncodingFromByteOrderMarks: false,
                                           bufferSize: 1024,
                                           leaveOpen: true);
                                context.Request.Body.Position = 0;
                                var buffer = new char[bufferSize];
                                await reader.ReadBlockAsync(buffer, 0, bufferSize);
                                context.Request.Body.Position = 0;
                                var content = new string(buffer);
                                requestInfo.Append(content);
                            }
                        }
                    }
                    requestIP = context.Connection.RemoteIpAddress.ToString();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "");
                }

                await _next(context);
            }
            finally
            {
                var user = context.User?.FindFirst("name") ??
                           context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
                _logger.Information(
                    "[RequestLog]: IP: {requestIP} User: {user}, method: {method}, path: {path}, status: {stauts}.{msg}, requestInfo {requestInfo}",
                    requestIP,
                    user?.Value,
                    context.Request?.Method,
                    context.Request?.Path,
                    context.Response?.StatusCode.ToString(),
                    context.Response?.StatusCode != 200 ? " See error details above." : string.Empty,
                    requestInfo
                );
            }
        }
    }
}