using Serilog;
using Serilog.Events;
using System;

namespace FritzBot.Core
{
    static class SerilogHack
    {
        public static LogEvent CreateLogEvent(DateTimeOffset timestamp, LogEventLevel level, Exception exception, string messageTemplate, params object[] messageTemplateParameters)
        {
            Log.BindMessageTemplate(messageTemplate, messageTemplateParameters, out var parsedTemplate, out var boundProperties);

            return new LogEvent(timestamp, level, exception, parsedTemplate, boundProperties);
        }
    }
}
