using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace PM_Case_Managemnt_Implementation.Helpers.Logger
{
    public class LoggerManagerService : ILoggerManagerService
    {
        private readonly ILogger<LoggerManagerService> _logger;

        public LoggerManagerService(ILogger<LoggerManagerService> logger)
        {
            _logger = logger;
        }

        public void LogCreate(string module, string createdById, string message)
        {
            LogWithProperties(LogLevel.Information, "Create", module, createdById, message);
        }

        public void LogException(string module, string createdById, string message)
        {
            LogWithProperties(LogLevel.Error, "Exception", module, createdById, message);
        }

        public void LogUpdate(string module, string createdById, string message)
        {
            LogWithProperties(LogLevel.Warning, "Update", module, createdById, message);
        }

        private void LogWithProperties(LogLevel logLevel, string operationType, string module, string userId, string message)
        {
            using (LogContext.PushProperty("OperationType", operationType))
            using (LogContext.PushProperty("ChangedOn", DateTime.UtcNow))
            using (LogContext.PushProperty("Module", module))
            using (LogContext.PushProperty("UserId", userId))
            {
                _logger.Log(logLevel, $"{{OperationType}}: {message}");
            }
        }
    }

}
