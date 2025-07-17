namespace WorkflowEngine.Models
{
    public class CreateWorkflowRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<State> States { get; set; } = new();
        public List<WorkflowAction> Actions { get; set; } = new();
    }
}