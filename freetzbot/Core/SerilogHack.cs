using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace FritzBot.Core
{
    static class SerilogHack
    {
        delegate void ProcessDelegate(string messageTemplate, object[] messageTemplateParameters, out MessageTemplate parsedTemplate, out IEnumerable<LogEventProperty> properties);

        private static ProcessDelegate ProcessMethod;

        static SerilogHack()
        {
            var _messageTemplateProcessor = Log.Logger.GetType().GetField("_messageTemplateProcessor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            ProcessMethod = (ProcessDelegate)_messageTemplateProcessor.FieldType.GetMethod("Process").CreateDelegate(typeof(ProcessDelegate), _messageTemplateProcessor.GetValue(Log.Logger));
        }

        public static LogEvent CreateLogEvent(DateTimeOffset timestamp, LogEventLevel level, Exception exception, string messageTemplate, params object[] messageTemplateParameters)
        {
            MessageTemplate parsedTemplate;
            IEnumerable<LogEventProperty> properties;
            ProcessMethod(messageTemplate, messageTemplateParameters, out parsedTemplate, out properties);

            return new LogEvent(timestamp, level, exception, parsedTemplate, properties);
        }
    }
}
