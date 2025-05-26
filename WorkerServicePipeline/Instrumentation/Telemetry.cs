using System.Diagnostics;

namespace WorkerServicePipeline.Instrumentation
{
    public static class Telemetry
    {
        public static readonly ActivitySource ActivitySource = new("WorkerServicePipeline");
    }
}
