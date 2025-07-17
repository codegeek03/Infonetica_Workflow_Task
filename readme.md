# Configurable Workflow Engine

A minimal backend service that provides a state machine API for defining and executing configurable workflows.

## Quick Start

### Prerequisites
- .NET 8.0 SDK

### Running the Application
```bash
dotnet run
```

The API will be available at `http://localhost:5000` (or the port shown in console output).

### Project Structure
```
WorkflowEngine/
├── Models/
│   ├── State.cs              # State definition
│   ├── WorkflowAction.cs     # Action/transition definition
│   ├── WorkflowDefinition.cs # Workflow template
│   ├── WorkflowInstance.cs   # Running workflow instance
│   └── Requests.cs           # API request models
├── Services/
│   ├── IWorkflowService.cs   # Service interface
│   └── WorkflowService.cs    # Core business logic
├── Program.cs                # API endpoints and startup
└── WorkflowEngine.csproj     # Project file
```

## API Endpoints

### Workflow Definitions
- **POST** `/api/workflows` - Create a new workflow definition
- **GET** `/api/workflows/{id}` - Get a specific workflow definition
- **GET** `/api/workflows` - List all workflow definitions

### Workflow Instances
- **POST** `/api/workflows/{definitionId}/instances` - Start a new workflow instance
- **GET** `/api/instances/{id}` - Get a specific workflow instance
- **GET** `/api/instances` - List all workflow instances
- **POST** `/api/instances/{id}/actions/{actionId}` - Execute an action on an instance

## Example Usage

### 1. Create a Workflow Definition
```bash
curl -X POST http://localhost:5000/api/workflows \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Document Review",
    "description": "Simple document review workflow",
    "states": [
      {
        "id": "draft",
        "name": "Draft",
        "isInitial": true,
        "isFinal": false,
        "enabled": true
      },
      {
        "id": "review",
        "name": "Under Review",
        "isInitial": false,
        "isFinal": false,
        "enabled": true
      },
      {
        "id": "approved",
        "name": "Approved",
        "isInitial": false,
        "isFinal": true,
        "enabled": true
      }
    ],
    "actions": [
      {
        "id": "submit",
        "name": "Submit for Review",
        "enabled": true,
        "fromStates": ["draft"],
        "toState": "review"
      },
      {
        "id": "approve",
        "name": "Approve",
        "enabled": true,
        "fromStates": ["review"],
        "toState": "approved"
      },
      {
        "id": "reject",
        "name": "Reject",
        "enabled": true,
        "fromStates": ["review"],
        "toState": "draft"
      }
    ]
  }'
```

### 2. Start a Workflow Instance
```bash
curl -X POST http://localhost:5000/api/workflows/{definition-id}/instances
```

### 3. Execute an Action
```bash
curl -X POST http://localhost:5000/api/instances/{instance-id}/actions/submit
```

## Design Decisions & Assumptions

### Architecture
- **Minimal API**: Used ASP.NET Core minimal APIs for simplicity and reduced boilerplate
- **Single Responsibility**: Clear separation between models, services, and API endpoints
- **Dependency Injection**: Service layer is injected for testability and extensibility

### Data Storage
- **In-Memory**: Using `ConcurrentDictionary` for thread-safe in-memory storage
- **Assumption**: This is acceptable for the prototype; production would use persistent storage

### Validation Rules
- Workflow definitions must have exactly one initial state
- No duplicate state or action IDs within a definition
- Actions can only execute from valid source states
- Cannot execute actions on instances in final states
- All state references in actions must exist

### Error Handling
- Validation errors return 400 Bad Request with descriptive messages
- Missing resources return 404 Not Found
- Business rule violations return 400 Bad Request

### State Machine Rules
- Instances start at the initial state of their definition
- Actions can have multiple source states but only one target state
- Full history tracking with timestamps
- Enabled/disabled flags for both states and actions