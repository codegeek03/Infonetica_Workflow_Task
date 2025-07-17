namespace WorkflowEngine.Models
{
    public class WorkflowInstance
    {
        public string Id { get; set; } = string.Empty;
        public string DefinitionId { get; set; } = string.Empty;
        public string CurrentStateId { get; set; } = string.Empty;
        public List<HistoryEntry> History { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class HistoryEntry
    {
        public string ActionId { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string FromStateId { get; set; } = string.Empty;
        public string ToStateId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}