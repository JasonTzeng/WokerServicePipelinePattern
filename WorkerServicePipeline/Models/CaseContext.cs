namespace WorkerServicePipeline.Models
{
    public class CaseContext
    {
        public string? RawJson { get; set; }
        public Dictionary<string, object> EnrichedData { get; set; } = new();
    }
}
