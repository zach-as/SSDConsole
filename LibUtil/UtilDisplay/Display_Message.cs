using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.UtilDisplay
{

    public static partial class CDisplay
    {
        internal class Message
        {
            private string message;
            private MessageSource source;
            private MessageSeverity severity;
            internal Message(string message, MessageSource source, MessageSeverity severity = MessageSeverity.Info)
            {
                this.message = message;
                this.source = source;
                this.severity = severity;
            }
            internal string Text() => message;
            internal MessageSource Source() => source;
            internal MessageSeverity Severity() => severity;
            internal ConsoleColor Color() => severity.Color();
        }

        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
        private class SeverityAttribute : System.Attribute
        {
            private ConsoleColor color;

            internal SeverityAttribute(ConsoleColor color)
            {
                this.color = color;
            }

            internal ConsoleColor Color()
                => color;
        }

        public enum MessageSeverity
        {
            [Severity(ConsoleColor.White)]
            Info,
            [Severity(ConsoleColor.Yellow)]
            Warning,
            [Severity(ConsoleColor.Red)]
            Error,
            [Severity(ConsoleColor.Green)]
            Success,
            [Severity(ConsoleColor.Cyan)]
            Debug,
        }

        internal static ConsoleColor Color(this MessageSeverity severity)
        {
            var fieldInfo = severity.GetType().GetField(severity.ToString());
            var attributes = fieldInfo?.GetCustomAttributes(typeof(SeverityAttribute), false) as SeverityAttribute[];
            return attributes is not null && attributes.Length > 0 ? attributes[0].Color() : ConsoleColor.White; // Default to white if no attribute is found
        }
    }
}
