using System.Text;
using Calabonga.OperationResults;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Catalog.Logging.Middleware
{
    public class LogService
    {
        private readonly ILogger _logger;
        public LogService(ILogger logger)
        {
            _logger = logger;
        }
        public async Task<Operation<bool, string>> LogAsync(HttpContext httpContext, String body)
        {
            if (httpContext.Request.Path.StartsWithSegments("/swagger"))
                return new();

            var formattedRequestBody = string.IsNullOrEmpty(body)
            ? "N/A"
            : body;

            StringBuilder stringBuilder = new();

            stringBuilder.Append(" Объект: ");
            stringBuilder.Append(httpContext.Response.HasStarted ? "RESPONSE;" : "REQUEST;");
            stringBuilder.Append(" Тип запроса: ");
            stringBuilder.Append(httpContext.Request.Method);
            stringBuilder.Append(';');
            stringBuilder.AppendLine($" Тело запроса: ");
            stringBuilder.Append($"{formattedRequestBody};");

            try
            {
                _logger.Error(stringBuilder.ToString());
            }
            catch (Exception exception)
            {
                return Operation.Error($"Ошибка в LogService: {exception.Message}");
            }
            return true;
        }

        public async Task<Operation<bool, string>> LogErrorAsync(HttpContext httpContext, Exception exception)
        {
            StringBuilder stringBuilder = new();

            stringBuilder.Append("ERROR: ");
            stringBuilder.AppendLine(
                string.IsNullOrEmpty(exception.Message) ? "" : exception.Message);

            try
            {
                _logger.Error(stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                return Operation.Error($"Ошибка в LogService: {exception.Message}");
            };

            return true;
        }
    }
}
