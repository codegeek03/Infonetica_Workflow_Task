# Configurable Workflow Engine

> A blazing-fast, lightweight backend service that transforms complex business processes into elegant state machines. Build, deploy, and scale your workflows in minutes, not months.

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](LICENSE)
[![Status](https://img.shields.io/badge/status-ready-brightgreen?style=flat-square)](https://github.com)

## ✨ Features

- **🎯 Zero-Configuration Setup** - Get running in under 30 seconds
- **🔄 Dynamic State Machines** - Create complex workflows with simple JSON
- **⚡ Lightning Fast** - Built on .NET 8 with minimal overhead
- **🛡️ Bulletproof Validation** - Comprehensive error handling and business rules
- **📊 Full History Tracking** - Every state transition is logged with timestamps
- **🔒 Thread-Safe** - Production-ready concurrent operations
- **🎨 RESTful API** - Clean, intuitive endpoints that just make sense

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) OR [Docker](https://www.docker.com/get-started)

### Option 1: Run with Docker (Recommended) 🐳

1. **Clone & Navigate**
   ```bash
   git clone <repository-url>
   cd WorkflowEngine
   ```

2. **Launch with Docker Compose**
   ```bash
   docker-compose up --build
   ```

3. **Verify**
   ```bash
   curl http://localhost:5191/api/workflows
   ```

### Option 2: Run with .NET

1. **Clone & Navigate**
   ```bash
   git clone <repository-url>
   cd WorkflowEngine
   ```

2. **Launch**
   ```bash
   dotnet run
   ```

3. **Verify**
   ```bash
   curl http://localhost:5191/api/workflows
   ```

🎉 **That's it!** Your workflow engine is live at `http://localhost:5191`

## 🏗️ Architecture

```
WorkflowEngine/
├── 📁 Models/
│   ├── State.cs              # 🏛️ State definition & validation
│   ├── WorkflowAction.cs     # ⚡ Action/transition logic
│   ├── WorkflowDefinition.cs # 📋 Workflow blueprints
│   ├── WorkflowInstance.cs   # 🏃 Live workflow execution
│   └── Requests.cs           # 📡 API contracts
├── 📁 Services/
│   ├── IWorkflowService.cs   # 🔌 Clean abstraction layer
│   └── WorkflowService.cs    # 🧠 Core business engine
├── Program.cs                # 🚪 API gateway & DI setup
├── WorkflowEngine.csproj     # ⚙️ Project configuration
├── Dockerfile                # 🐳 Container configuration
├── docker-compose.yml        # 🚢 Orchestration setup
└── .dockerignore             # 📦 Build optimization
```

## 🎯 Workflow State Machine

Here's how our Document Review workflow operates as a state machine:

## 📚 API Reference

### 🏗️ Workflow Definitions
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/workflows` | 🎨 Create workflow template |
| `GET` | `/api/workflows` | 📋 List all templates |
| `GET` | `/api/workflows/{id}` | 🔍 Get specific template |

### 🏃 Workflow Instances
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/workflows/{definitionId}/instances` | ▶️ Start new workflow |
| `GET` | `/api/instances` | 📊 List all instances |
| `GET` | `/api/instances/{id}` | 🔍 Get instance details |
| `POST` | `/api/instances/{id}/actions/{actionId}` | ⚡ Execute action |

## 🎯 Real-World Example

Let's build a **Document Review Workflow** from scratch:

### Step 1: Create the Workflow Definition

```bash
curl -X POST http://localhost:5191/api/workflows \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Document Review Process",
    "description": "Enterprise document approval workflow",
    "states": [
      {
        "id": "draft",
        "name": "📝 Draft",
        "isInitial": true,
        "isFinal": false,
        "enabled": true
      },
      {
        "id": "review",
        "name": "👀 Under Review",
        "isInitial": false,
        "isFinal": false,
        "enabled": true
      },
      {
        "id": "approved",
        "name": "✅ Approved",
        "isInitial": false,
        "isFinal": true,
        "enabled": true
      },
      {
        "id": "rejected",
        "name": "❌ Rejected",
        "isInitial": false,
        "isFinal": true,
        "enabled": true
      }
    ],
    "actions": [
      {
        "id": "submit",
        "name": "📤 Submit for Review",
        "enabled": true,
        "fromStates": ["draft"],
        "toState": "review"
      },
      {
        "id": "approve",
        "name": "👍 Approve Document",
        "enabled": true,
        "fromStates": ["review"],
        "toState": "approved"
      },
      {
        "id": "reject",
        "name": "👎 Reject Document",
        "enabled": true,
        "fromStates": ["review"],
        "toState": "rejected"
      },
      {
        "id": "revise",
        "name": "✏️ Send Back for Revision",
        "enabled": true,
        "fromStates": ["review"],
        "toState": "draft"
      }
    ]
  }'
```

**Response:**
```json
{
  "id": "abc123-def456-ghi789",
  "name": "Document Review Process",
  "description": "Enterprise document approval workflow",
  "createdAt": "2025-07-18T10:30:00Z",
  "states": [...],
  "actions": [...]
}
```

### Step 2: Start a Workflow Instance

```bash
# First, get your workflow definition ID
curl http://localhost:5191/api/workflows

# Then start an instance using the ID from above
curl -X POST http://localhost:5191/api/workflows/abc123-def456-ghi789/instances
```

**Response:**
```json
{
  "id": "instance-xyz789",
  "definitionId": "abc123-def456-ghi789",
  "currentState": "draft",
  "createdAt": "2025-07-18T10:35:00Z",
  "history": [
    {
      "fromState": null,
      "toState": "draft",
      "action": null,
      "timestamp": "2025-07-18T10:35:00Z"
    }
  ]
}
```

### Step 3: Execute Actions

```bash
# Submit document for review
curl -X POST http://localhost:5191/api/instances/instance-xyz789/actions/submit

# Approve the document
curl -X POST http://localhost:5191/api/instances/instance-xyz789/actions/approve
```

### Step 4: Track Progress

```bash
# Check current status
curl http://localhost:5191/api/instances/instance-xyz789
```

**Response:**
```json
{
  "id": "instance-xyz789",
  "definitionId": "abc123-def456-ghi789",
  "currentState": "approved",
  "createdAt": "2025-07-18T10:35:00Z",
  "history": [
    {
      "fromState": null,
      "toState": "draft",
      "action": null,
      "timestamp": "2025-07-18T10:35:00Z"
    },
    {
      "fromState": "draft",
      "toState": "review",
      "action": "submit",
      "timestamp": "2025-07-18T10:40:00Z"
    },
    {
      "fromState": "review",
      "toState": "approved",
      "action": "approve",
      "timestamp": "2025-07-18T10:45:00Z"
    }
  ]
}
```

## 🛡️ Built-in Safeguards

### ✅ Validation Rules
- **Single Initial State**: Every workflow must have exactly one starting point
- **Unique Identifiers**: No duplicate state or action IDs allowed
- **Valid Transitions**: Actions can only execute from authorized source states
- **Final State Protection**: Cannot execute actions on completed workflows
- **Reference Integrity**: All state references must exist in the definition

### 🚨 Error Handling
- **400 Bad Request**: Validation errors with detailed messages
- **404 Not Found**: Missing workflows or instances
- **Business Rules**: Automatic enforcement of state machine constraints

## 🎨 Design Philosophy

### **Simplicity First**
Built with ASP.NET Core minimal APIs - no unnecessary complexity, just pure performance.

### **Production Ready**
Thread-safe concurrent operations using `ConcurrentDictionary` ensure your workflows scale seamlessly.

### **Extensible Architecture**
Clean separation of concerns with dependency injection makes customization effortless.

### **Developer Experience**
Intuitive REST API design that follows conventions you already know and love.

## 🐳 Docker Deployment

### Quick Start with Docker

```bash
# Build and start with Docker Compose
docker-compose up --build

# Run in detached mode (background)
docker-compose up -d --build

# View logs
docker-compose logs -f workflow-engine

# Stop the service
docker-compose down
```

### Manual Docker Commands

```bash
# Build the image
docker build -t workflow-engine:latest .

# Run the container
docker run -d \
  --name workflow-engine \
  -p 5191:5191 \
  --restart unless-stopped \
  workflow-engine:latest

# View logs
docker logs -f workflow-engine

# Stop and remove
docker stop workflow-engine && docker rm workflow-engine
```

### Docker Features

- **🔒 Security**: Runs as non-root user
- **📦 Optimized**: Multi-stage build for minimal image size
- **🔄 Health Checks**: Automatic endpoint monitoring
- **🚀 Production Ready**: Restart policies and proper configuration

## 🔧 Configuration

### Memory Storage
Currently uses in-memory storage for lightning-fast prototyping. For production deployments, simply swap in your preferred database provider through the service layer.

### Port Configuration
Default port is `5191`. Customize in `Program.cs` or via environment variables.

## 🤝 Contributing

We'd love your help making this even better! Feel free to:
- 🐛 Report bugs
- 💡 Suggest features
- 🔧 Submit pull requests
- 📖 Improve documentation

## 📄 License

MIT License - feel free to use this in your projects!

---

<div align="center">
  <strong>Built with ❤️ using .NET 8</strong>
  <br>
  <em>Making workflows work for you</em>
</div>
