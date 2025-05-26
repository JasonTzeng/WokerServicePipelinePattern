using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace WorkerServicePipeline.Logging
{
    public class CustomJsonConsoleFormatter : ConsoleFormatter, IDisposable
    {
        private readonly JsonWriterOptions _jsonWriterOptions = new() { Indented = true };

        public CustomJsonConsoleFormatter() : base("customJson") { }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
        {
            using var memoryStream = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(memoryStream, _jsonWriterOptions);

            jsonWriter.WriteStartObject();

            jsonWriter.WriteString("Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            jsonWriter.WriteNumber("EventId", logEntry.EventId.Id);
            jsonWriter.WriteString("LogLevel", logEntry.LogLevel.ToString());
            jsonWriter.WriteString("Category", logEntry.Category);

            // Message
            var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
            if (!string.IsNullOrEmpty(message))
                jsonWriter.WriteString("Message", message);

            // Trace/Span/ParentId
            var activity = Activity.Current;
            if (activity != null)
            {
                jsonWriter.WriteString("TraceId", activity.TraceId.ToString());
                jsonWriter.WriteString("SpanId", activity.SpanId.ToString());
                jsonWriter.WriteString("ParentId", activity.ParentSpanId.ToString());
            }

            // Exception
            if (logEntry.Exception != null)
                jsonWriter.WriteString("Exception", logEntry.Exception.ToString());

            jsonWriter.WriteEndObject();
            jsonWriter.Flush();
            // Write the JSON to the textWriter
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream, Encoding.UTF8);
            textWriter.WriteLine(reader.ReadToEnd());
        }

        public void Dispose() { }
    }
}
