namespace WorkflowEngine.Services
{
    using WorkflowEngine.Models;

    public class WorkflowService : IWorkflowService
    {
        private readonly IPersistenceService _persistenceService;

        public WorkflowService(IPersistenceService persistenceService)
        {
            _persistenceService = persistenceService;
        }

        public async Task<WorkflowDefinition> CreateWorkflowDefinitionAsync(CreateWorkflowRequest request)
        {
            ValidateWorkflowDefinition(request);

            var definition = new WorkflowDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                States = request.States,
                Actions = request.Actions
            };

            await _persistenceService.SaveWorkflowDefinitionAsync(definition);
            return definition;
        }

        public async Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(string id)
        {
            return await _persistenceService.LoadWorkflowDefinitionAsync(id);
        }

        public async Task<IEnumerable<WorkflowDefinition>> GetAllWorkflowDefinitionsAsync()
        {
            return await _persistenceService.LoadAllWorkflowDefinitionsAsync();
        }

        public async Task<WorkflowInstance> StartWorkflowInstanceAsync(string definitionId)
        {
            var definition = await GetWorkflowDefinitionAsync(definitionId);
            if (definition == null)
            {
                throw new ArgumentException($"Workflow definition '{definitionId}' not found");
            }

            var initialState = definition.States.FirstOrDefault(s => s.IsInitial);
            if (initialState == null)
            {
                throw new ArgumentException($"No initial state found in definition '{definitionId}'");
            }

            var instance = new WorkflowInstance
            {
                Id = Guid.NewGuid().ToString(),
                DefinitionId = definitionId,
                CurrentStateId = initialState.Id
            };

            await _persistenceService.SaveWorkflowInstanceAsync(instance);
            return instance;
        }

        public async Task<WorkflowInstance?> GetWorkflowInstanceAsync(string id)
        {
            return await _persistenceService.LoadWorkflowInstanceAsync(id);
        }

        public async Task<IEnumerable<WorkflowInstance>> GetAllWorkflowInstancesAsync()
        {
            return await _persistenceService.LoadAllWorkflowInstancesAsync();
        }

        public async Task<WorkflowInstance> ExecuteActionAsync(string instanceId, string actionId)
        {
            var instance = await GetWorkflowInstanceAsync(instanceId);
            if (instance == null)
            {
                throw new ArgumentException($"Workflow instance '{instanceId}' not found");
            }

            var definition = await GetWorkflowDefinitionAsync(instance.DefinitionId);
            if (definition == null)
            {
                throw new ArgumentException($"Workflow definition '{instance.DefinitionId}' not found");
            }

            var action = definition.Actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null)
            {
                throw new ArgumentException($"Action '{actionId}' not found in workflow definition");
            }

            var currentState = definition.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);
            if (currentState == null)
            {
                throw new ArgumentException($"Current state '{instance.CurrentStateId}' not found");
            }

            // Validation rules
            if (currentState.IsFinal)
            {
                throw new ArgumentException("Cannot execute actions on instances in final state");
            }

            if (!action.Enabled)
            {
                throw new ArgumentException($"Action '{actionId}' is disabled");
            }

            if (!action.FromStates.Contains(instance.CurrentStateId))
            {
                throw new ArgumentException($"Action '{actionId}' cannot be executed from current state '{instance.CurrentStateId}'");
            }

            var targetState = definition.States.FirstOrDefault(s => s.Id == action.ToState);
            if (targetState == null)
            {
                throw new ArgumentException($"Target state '{action.ToState}' not found");
            }

            // Execute the action
            var historyEntry = new HistoryEntry
            {
                ActionId = action.Id,
                ActionName = action.Name,
                FromStateId = instance.CurrentStateId,
                ToStateId = action.ToState
            };

            instance.History.Add(historyEntry);
            instance.CurrentStateId = action.ToState;
            instance.LastUpdated = DateTime.UtcNow;

            await _persistenceService.SaveWorkflowInstanceAsync(instance);
            return instance;
        }

        private static void ValidateWorkflowDefinition(CreateWorkflowRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Workflow name is required");
            }

            if (!request.States.Any())
            {
                throw new ArgumentException("At least one state is required");
            }

            // Check for duplicate state IDs
            var stateIds = request.States.Select(s => s.Id).ToList();
            if (stateIds.Count != stateIds.Distinct().Count())
            {
                throw new ArgumentException("Duplicate state IDs found");
            }

            // Check for duplicate action IDs
            var actionIds = request.Actions.Select(a => a.Id).ToList();
            if (actionIds.Count != actionIds.Distinct().Count())
            {
                throw new ArgumentException("Duplicate action IDs found");
            }

            // Validate exactly one initial state
            var initialStates = request.States.Where(s => s.IsInitial).ToList();
            if (initialStates.Count != 1)
            {
                throw new ArgumentException("Exactly one initial state is required");
            }

            // Validate action references
            var validStateIds = new HashSet<string>(stateIds);
            foreach (var action in request.Actions)
            {
                if (string.IsNullOrWhiteSpace(action.ToState))
                {
                    throw new ArgumentException($"Action '{action.Id}' must have a target state");
                }

                if (!validStateIds.Contains(action.ToState))
                {
                    throw new ArgumentException($"Action '{action.Id}' references unknown target state '{action.ToState}'");
                }

                if (!action.FromStates.Any())
                {
                    throw new ArgumentException($"Action '{action.Id}' must have at least one source state");
                }

                foreach (var fromState in action.FromStates)
                {
                    if (!validStateIds.Contains(fromState))
                    {
                        throw new ArgumentException($"Action '{action.Id}' references unknown source state '{fromState}'");
                    }
                }
            }
        }
    }
}