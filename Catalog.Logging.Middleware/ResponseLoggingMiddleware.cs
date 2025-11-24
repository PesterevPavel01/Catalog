using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.Logging.Middleware
{
    public class ResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next = null!;
        private readonly LogService _logService = null!;
        private readonly ITelegramService _telegramService;

        public ResponseLoggingMiddleware(RequestDelegate next, ITelegramService telegramService, ILogger logger, IOptions<TelegramBotConfiguration> telegramBotConfiguration)
        {
            _next = next;
            _logService = new LogService(logger);
            _telegramService = telegramService;
            _telegramService.Initialize(token: telegramBotConfiguration.Value.Token, chatId: telegramBotConfiguration.Value.ChatId);
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var originalBodyStream = httpContext.Response.Body;

            try
            {
                using var memoryStream = new MemoryStream();

                httpContext.Response.Body = memoryStream;

                await _next(httpContext);

                memoryStream.Seek(0, SeekOrigin.Begin);

                using var streamReader = new StreamReader(memoryStream);

                var responseBodyText = await streamReader.ReadToEndAsync();

                memoryStream.Seek(0, SeekOrigin.Begin);

                await memoryStream.CopyToAsync(originalBodyStream);

                var logResult = await _logService.LogAsync(httpContext, responseBodyText);

            }
            catch (Exception exception)
            {
                await _telegramService.SendMessageAsync(exception.Message);
            }
            finally
            {
                httpContext.Response.Body = originalBodyStream;
            }
        }
    }
}
