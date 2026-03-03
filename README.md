[![Build & Unit Test](https://github.com/jonathandbailey/maf-travel/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jonathandbailey/maf-travel/actions/workflows/dotnet.yml)

# MAF Travel — Alpha Release

An AI-powered vacation planning assistant built on the **Microsoft Agent Framework (MAF)**. Chat naturally with the assistant to describe your trip — it will ask the right questions, build up a structured travel plan, and keep track of everything across turns.

This project is an exploration of practical agentic patterns in .NET. The travel domain is a vehicle for demonstrating those patterns clearly — the techniques here are designed to be lifted and applied to other problems.

## User Experience

The UI is designed to demonstrate what a well-behaved agentic application feels like from the user's perspective — not just what it produces, but how it communicates while it works.

**Real-time feedback from agents and workflows**
The interface shows a live "Thought process" timeline as the workflow runs — each step, agent action, and tool call surfaces as it happens, not as a batch at the end. A structured travel plan panel updates in the sidebar as details are confirmed, giving the user a continuous view of what the assistant has gathered so far.

**Interruptability**
The submit button becomes a stop button the moment a response starts streaming. The user can cancel at any point mid-workflow — the assistant stops immediately without waiting for the current operation to complete. This reflects a core principle of agentic UX: the user should always be in control.

**Pre-loaded prompts and application capabilities**
A suggestions menu in the chat input offers quick-start prompts, including one that asks the assistant to describe what it can do. This lowers the barrier for new users and demonstrates how an application can surface its own capabilities as a first-class feature rather than leaving them to documentation.

---

## Key Features

**Streaming real-time workflow events via AG-UI**
Workflow progress — agent thoughts, tool calls, state snapshots — streams to the client in real time using the AG-UI protocol. The UI updates as the workflow runs, not after it finishes.

**Stateless suspend and resume**
Workflows checkpoint themselves to Azure Blob Storage at every suspension point. There is no in-memory session state: a workflow can be resumed by any process instance, across restarts, simply by passing the checkpoint reference on the next request.

**Tool Repository Pattern**
Tools are registered in a central `IToolRegistry` and resolved by name at runtime. Agent prompts declare tool schemas, but execution is handled by the registry — keeping AI tool definitions cleanly separated from the code that acts on them.

**Declarative agent templates**
Agents are defined as YAML prompt files, not C# classes. Swapping out an agent's behaviour means editing a template, not recompiling. Tool schemas are attached as declaration-only stubs so the LLM sees a consistent interface regardless of how execution is wired up.

**Unit and integration test coverage**
Integration tests run full end-to-end workflow scenarios with pre-scripted mock agent responses — no real Azure OpenAI calls required. Unit tests cover individual components in isolation. Scenario definitions live in JSON files, making it straightforward to add new test cases without touching C# code.

**Built-in observability**
Every workflow node and tool call emits an OpenTelemetry trace span. The full execution path — from user message to agent response to tool result — is visible as a distributed trace in the Aspire dashboard.

---

## What It Does

You describe a vacation in plain language. The assistant uses a **ReAct (Reason & Act)** loop to:

1. Extract structured travel details from what you say
2. Reason about what information is still missing
3. Ask targeted follow-up questions until the plan is complete
4. Emit a finalised `TravelPlanDto` when all required fields are satisfied

The assistant communicates over the **AG-UI protocol**, so any AG-UI-compatible frontend can connect to it.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core (.NET 10) |
| Agent Framework | Microsoft Agent Framework (MAF) |
| AI Model | Azure OpenAI — `gpt-4o-mini` |
| Workflow Engine | DAG workflow with checkpoint/resume |
| State Storage | Azure Blob Storage (Azurite locally) |
| Orchestration | .NET Aspire |
| Frontend | React + TypeScript + Vite |
| Protocol | AG-UI |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (for the UI)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Aspire uses it to run Azurite)
- An **Azure OpenAI** resource with a `gpt-4o-mini` deployment
- Azure CLI logged in (`az login`) — authentication uses `DefaultAzureCredential`, no API key required

---

## Configuration

Create `appsettings.Development.json` in `src/Travel.Experience.Api/` (or use User Secrets):

```json
{
  "LanguageModelSettings": {
    "DeploymentName": "<your-gpt-4o-mini-deployment-name>",
    "Endpoint": "<your-azure-openai-endpoint>"
  }
}
```

---

## Running the Application

```bash
# Build the solution
dotnet build MafTravel.sln

# Start everything via Aspire (API + UI + Azurite storage)
dotnet run --project src/AppHost/AppHost.csproj
```

Aspire will launch:
- **Travel Experience API** — the backend with the AG-UI endpoint at `/ag-ui`
- **UI** — the React frontend at `http://localhost:5173`
- **Azurite** — local Azure Storage emulator (workflow checkpoints are persisted here)

Open the Aspire dashboard (printed in the console output) to see traces, logs, and service health.

---

## Architecture Overview

### Workflow DAG

The core of the assistant is a **directed acyclic graph** workflow with suspend/resume support:

```
StartNode → ExtractionNode → UpdateNode → PlannerNode → ExecutionNode
                ↑                                              |
                |                              ┌──────────────┴──────────────┐
         InformationResponseNode          request_information         planning_complete
                ↑                               |                            |
         InformationRequestNode ←───────────────┘                        EndNode
          (workflow suspends, waits for user)
```

| Node | Responsibility |
|---|---|
| `StartNode` | Initialises `TravelPlanDto` from the incoming request |
| `ExtractionNode` | Calls the extracting agent (LLM) to parse user input |
| `UpdateNode` | Applies the extracted update to shared workflow state |
| `PlannerNode` | Calls the planning agent to audit what's still missing |
| `ExecutionNode` | Routes the planner's decision: ask more / complete |
| `InformationRequestNode` | Suspends the workflow and signals the user |
| `InformationResponseNode` | Resumes the workflow with the user's reply |
| `EndNode` | Emits `TravelPlanningCompleteEvent` with the finalised plan |

### Checkpoint / Resume

Workflows are persisted to Azure Blob Storage at every suspension point. Passing a `CheckpointInfo` in the next request resumes exactly where execution left off — even across process restarts.

### AI Agents

Agents are defined as YAML prompt templates in [`src/Travel.Agents/Templates/`](src/Travel.Agents/Templates/):

- `planning.yaml` — reasons about `TravelPlanSummary`, calls `request_information` or `planning_complete`
- `extracting.yaml` — parses natural language input, calls `update_travel_plan` with structured data
- `conversation.yaml` — routes the user's message into the workflow

Tool schemas are registered with the LLM as declaration-only stubs — the workflow nodes handle execution, not the tool functions themselves.

---

## Running the Tests

```bash
# All tests
dotnet test MafTravel.sln

# Integration tests only
dotnet test src/Travel.Tests.Integration/Travel.Tests.Integration.csproj

# Unit tests only
dotnet test src/Travel.Tests.Unit/Travel.Tests.Unit.csproj

# Run a specific test by name
dotnet test src/Travel.Tests.Integration/Travel.Tests.Integration.csproj \
  --filter "FullyQualifiedName~PlanningWorkflow_ShouldUpdatePlanAndRequestionInformation"
```

Tests use pre-scripted mock agent responses — no real Azure OpenAI calls are made. Scenario definitions live in [`src/Travel.Tests/TestData/`](src/Travel.Tests/TestData/) as JSON files.

---

## Project Structure

```
src/
├── AppHost/                     # Aspire host — orchestrates all services
├── Travel.Experience.Api/       # ASP.NET Core API, AG-UI endpoint
├── Travel.Experience.Application/ # ConversationAgent, tool handlers
├── Travel.Workflows/            # DAG workflow, nodes, WorkflowFactory
├── Travel.Agents/               # Agent definitions, YAML templates, tools
├── Agents/                      # Shared MAF agent infrastructure
├── Infrastructure/              # Checkpoint & session repositories (Blob, in-memory)
├── ServiceDefaults/             # Shared Aspire config (OpenTelemetry, service discovery)
├── Travel.Tests.Integration/    # Scenario-driven integration tests
├── Travel.Tests.Unit/           # Isolated unit tests
└── Travel.Tests.Shared/         # Shared test helpers and harnesses
ui/                              # React + TypeScript + Vite frontend
```

---

## Observability

Every workflow node emits an **OpenTelemetry** trace span via `TravelWorkflowTelemetry`. The Aspire dashboard displays distributed traces end-to-end — from the user's chat message through agent calls, tool executions, and workflow node transitions.
