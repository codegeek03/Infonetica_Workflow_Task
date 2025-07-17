namespace WorkflowEngine.Services
{
    using WorkflowEngine.Models;
    using System.Text.Json;
    using System.Collections.Concurrent;

    public interface IPersistenceService
    {
        Task SaveWorkflowDefinitionAsync(WorkflowDefinition definition);
        Task<WorkflowDefinition?> LoadWorkflowDefinitionAsync(string id);
        Task<IEnumerable<WorkflowDefinition>> LoadAllWorkflowDefinitionsAsync();
        Task DeleteWorkflowDefinitionAsync(string id);

        Task SaveWorkflowInstanceAsync(WorkflowInstance instance);
        Task<WorkflowInstance?> LoadWorkflowInstanceAsync(string id);
        Task<IEnumerable<WorkflowInstance>> LoadAllWorkflowInstancesAsync();
        Task DeleteWorkflowInstanceAsync(string id);
    }

    public class FilePersistenceService : IPersistenceService
    {
        private readonly string _dataDirectory;
        private readonly string _definitionsPath;
        private readonly string _instancesPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public FilePersistenceService(string dataDirectory = "data")
        {
            _dataDirectory = dataDirectory;
            _definitionsPath = Path.Combine(_dataDirectory, "definitions.json");
            _instancesPath = Path.Combine(_dataDirectory, "instances.json");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            EnsureDataDirectoryExists();
        }

        private void EnsureDataDirectoryExists()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        public async Task SaveWorkflowDefinitionAsync(WorkflowDefinition definition)
        {
            var definitions = (await LoadAllWorkflowDefinitionsAsync()).ToList();
            var existingIndex = definitions.FindIndex(d => d.Id == definition.Id);

            if (existingIndex >= 0)
            {
                definitions[existingIndex] = definition;
            }
            else
            {
                definitions.Add(definition);
            }

            await SaveDefinitionsToFileAsync(definitions);
        }

        public async Task<WorkflowDefinition?> LoadWorkflowDefinitionAsync(string id)
        {
            var definitions = await LoadAllWorkflowDefinitionsAsync();
            return definitions.FirstOrDefault(d => d.Id == id);
        }

        public async Task<IEnumerable<WorkflowDefinition>> LoadAllWorkflowDefinitionsAsync()
        {
            if (!File.Exists(_definitionsPath))
            {
                return new List<WorkflowDefinition>();
            }

            try
            {
                var json = await File.ReadAllTextAsync(_definitionsPath);
                var definitions = JsonSerializer.Deserialize<List<WorkflowDefinition>>(json, _jsonOptions);
                return definitions ?? new List<WorkflowDefinition>();
            }
            catch (Exception)
            {
                // If file is corrupted, return empty list
                return new List<WorkflowDefinition>();
            }
        }

        public async Task DeleteWorkflowDefinitionAsync(string id)
        {
            var definitions = (await LoadAllWorkflowDefinitionsAsync()).ToList();
            definitions.RemoveAll(d => d.Id == id);
            await SaveDefinitionsToFileAsync(definitions);
        }

        public async Task SaveWorkflowInstanceAsync(WorkflowInstance instance)
        {
            var instances = (await LoadAllWorkflowInstancesAsync()).ToList();
            var existingIndex = instances.FindIndex(i => i.Id == instance.Id);

            if (existingIndex >= 0)
            {
                instances[existingIndex] = instance;
            }
            else
            {
                instances.Add(instance);
            }

            await SaveInstancesToFileAsync(instances);
        }

        public async Task<WorkflowInstance?> LoadWorkflowInstanceAsync(string id)
        {
            var instances = await LoadAllWorkflowInstancesAsync();
            return instances.FirstOrDefault(i => i.Id == id);
        }

        public async Task<IEnumerable<WorkflowInstance>> LoadAllWorkflowInstancesAsync()
        {
            if (!File.Exists(_instancesPath))
            {
                return new List<WorkflowInstance>();
            }

            try
            {
                var json = await File.ReadAllTextAsync(_instancesPath);
                var instances = JsonSerializer.Deserialize<List<WorkflowInstance>>(json, _jsonOptions);
                return instances ?? new List<WorkflowInstance>();
            }
            catch (Exception)
            {
                // If file is corrupted, return empty list
                return new List<WorkflowInstance>();
            }
        }

        public async Task DeleteWorkflowInstanceAsync(string id)
        {
            var instances = (await LoadAllWorkflowInstancesAsync()).ToList();
            instances.RemoveAll(i => i.Id == id);
            await SaveInstancesToFileAsync(instances);
        }

        private async Task SaveDefinitionsToFileAsync(IEnumerable<WorkflowDefinition> definitions)
        {
            var json = JsonSerializer.Serialize(definitions, _jsonOptions);
            await File.WriteAllTextAsync(_definitionsPath, json);
        }

        private async Task SaveInstancesToFileAsync(IEnumerable<WorkflowInstance> instances)
        {
            var json = JsonSerializer.Serialize(instances, _jsonOptions);
            await File.WriteAllTextAsync(_instancesPath, json);
        }
    }

    public class InMemoryPersistenceService : IPersistenceService
    {
        private readonly ConcurrentDictionary<string, WorkflowDefinition> _definitions = new();
        private readonly ConcurrentDictionary<string, WorkflowInstance> _instances = new();

        public async Task SaveWorkflowDefinitionAsync(WorkflowDefinition definition)
        {
            _definitions[definition.Id] = definition;
            await Task.CompletedTask;
        }

        public async Task<WorkflowDefinition?> LoadWorkflowDefinitionAsync(string id)
        {
            _definitions.TryGetValue(id, out var definition);
            return await Task.FromResult(definition);
        }

        public async Task<IEnumerable<WorkflowDefinition>> LoadAllWorkflowDefinitionsAsync()
        {
            return await Task.FromResult(_definitions.Values);
        }

        public async Task DeleteWorkflowDefinitionAsync(string id)
        {
            _definitions.TryRemove(id, out _);
            await Task.CompletedTask;
        }

        public async Task SaveWorkflowInstanceAsync(WorkflowInstance instance)
        {
            _instances[instance.Id] = instance;
            await Task.CompletedTask;
        }

        public async Task<WorkflowInstance?> LoadWorkflowInstanceAsync(string id)
        {
            _instances.TryGetValue(id, out var instance);
            return await Task.FromResult(instance);
        }

        public async Task<IEnumerable<WorkflowInstance>> LoadAllWorkflowInstancesAsync()
        {
            return await Task.FromResult(_instances.Values);
        }

        public async Task DeleteWorkflowInstanceAsync(string id)
        {
            _instances.TryRemove(id, out _);
            await Task.CompletedTask;
        }
    }

    public interface IWorkflowService
    {
        Task<WorkflowDefinition> CreateWorkflowDefinitionAsync(CreateWorkflowRequest request);
        Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(string id);
        Task<IEnumerable<WorkflowDefinition>> GetAllWorkflowDefinitionsAsync();
        
        Task<WorkflowInstance> StartWorkflowInstanceAsync(string definitionId);
        Task<WorkflowInstance?> GetWorkflowInstanceAsync(string id);
        Task<IEnumerable<WorkflowInstance>> GetAllWorkflowInstancesAsync();
        Task<WorkflowInstance> ExecuteActionAsync(string instanceId, string actionId);
    }
}