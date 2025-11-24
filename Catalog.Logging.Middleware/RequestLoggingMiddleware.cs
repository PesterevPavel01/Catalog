using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.Logging.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next = null!;
        private readonly LogService _logService = null!;
        private readonly ITelegramService _telegramService;

        public RequestLoggingMiddleware(RequestDelegate next, ITelegramService telegramService, ILogger logger, IOptions<TelegramBotConfiguration> telegramBotConfiguration)
        {
            _next = next;
            _logService = new LogService(logger);
            _telegramService = telegramService;
            _telegramService.Initialize(token: telegramBotConfiguration.Value.Token, chatId: telegramBotConfiguration.Value.ChatId);
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {

            httpContext.Request.EnableBuffering(); // Позволяет читать тело запроса несколько раз

            using StreamReader streamReader = new(httpContext.Request.Body);

            var requestBody = await streamReader.ReadToEndAsync();

            httpContext.Request.Body.Position = 0;

            try
            {
                var logResult = await _logService.LogAsync(httpContext, requestBody);
            }
            catch (Exception exception)
            {
                await _telegramService.SendMessageAsync(exception.Message);
            }

            await _next(httpContext);
        }
    }
}
