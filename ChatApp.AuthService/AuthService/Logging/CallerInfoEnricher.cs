using System.Diagnostics;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace AuthService.API.Logging
{
    public class CallerInfoEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var stackFrame = new StackFrame(2, true);  // Adjust the level for where the log is being called
            var method = stackFrame.GetMethod();
            var lineNumber = stackFrame.GetFileLineNumber();
            var fileName = stackFrame.GetFileName();
            var className = method.DeclaringType?.Name;

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("MethodName", method?.Name));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ClassName", className));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("LineNumber", lineNumber));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("FileName", fileName));
        }
    }

    public class LoggerHelper
    {
        private static Serilog.ILogger _logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        public static void Log(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            _logger
                .ForContext("MemberName", memberName)
                .ForContext("FilePath", filePath)
                .ForContext("LineNumber", lineNumber)
                .Information(message);
        }
    }
}
