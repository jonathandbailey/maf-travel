[![Build & Unit Test](https://github.com/jonathandbailey/maf-travel/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jonathandbailey/maf-travel/actions/workflows/dotnet.yml)

# Microsoft Agent Framework Travel

An educational agentic travel application built on the Microsoft Agent Framework (MAF), using .NET, C#, and React. Users plan a vacation through a chat interface — the assistant gathers required details across multiple turns and produces a structured travel plan.

This project explores the patterns and trade-offs that arise when moving beyond demos toward more realistic agentic applications. It is a work-in-progress and not production-ready.

The travel application showcases key principles of UX and Agentic Architecture:

**Stateless Workflows**
Workflow state is checkpointed and persisted to Azure Blob Storage at every suspension point, then restored on resume. This enables human-in-the-loop feedback and seamless recovery across process restarts.

**Tool Registry**
Agents discover and invoke tools consistently via a central Tool Registry. Tools are registered as declaration-only stubs — the LLM sees the schema, but execution is handled by workflow nodes, not the tool functions themselves.

**Real-Time Streaming**
Intermediate status updates are streamed from workflows to the UI via the AG-UI protocol, giving the user real-time visibility into what the assistant is doing.

**Interruptability**
The user can cancel the request at any time via the UI. Because workflow state is checkpointed, cancellation is non-destructive — the workflow can be resumed from where it left off.

**Capability Discovery**
The user can query the application's capabilities through the chat interface to understand what actions are available to them.

**Traceability**
Application, agent, and workflow traces are emitted as OpenTelemetry spans and displayed in the Aspire Dashboard. Metrics include per-run token usage.

**Structured Agent ReAct Workflows**
The travel plan collection workflow uses a ReAct (Reason and Act) loop: the planning agent audits what information is still missing, then either requests more from the user or signals completion. This loop drives a DAG-based workflow with suspend/resume support.


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
