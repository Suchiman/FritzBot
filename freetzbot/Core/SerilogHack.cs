using Serilog;
using Serilog.Events;
using System;

namespace FritzBot.Core
{
    static class SerilogHack
    {
        public static LogEvent CreateLogEvent(DateTimeOffset timestamp, LogEventLevel level, Exception? exception, string messageTemplate, params object[] messageTemplateParameters)
        {
#pragma warning disable Serilog004 // Constant MessageTemplate verifier
            Log.BindMessageTemplate(messageTemplate, messageTemplateParameters, out var parsedTemplate, out var boundProperties);
#pragma warning restore Serilog004 // Constant MessageTemplate verifier

            return new LogEvent(timestamp, level, exception, parsedTemplate, boundProperties);
        }
    }
}
