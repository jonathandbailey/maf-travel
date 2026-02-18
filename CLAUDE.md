# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**MAF-Travel** is an educational project exploring the **Microsoft Agent Framework (MAF)** and agentic patterns. It implements a vacation planning assistant using a DAG-based workflow system with AI agents. This is a work-in-progress, not production-ready — APIs and structure change frequently.

## Commands

### Build & Run
```bash
# Build the entire solution
dotnet build MafTravel.sln

# Run the Aspire host (orchestrates all services)
dotnet run --project src/AppHost/AppHost.csproj

# Run the test dashboard service
dotnet run --project src/Travel.Tests.Dashboard/Travel.Tests.Dashboard.csproj
```

### Tests
```bash
# Run all tests
dotnet test MafTravel.sln

# Run tests in a specific project
dotnet test src/Travel.Tests/Travel.Tests.csproj

# Run a single test by name
dotnet test src/Travel.Tests/Travel.Tests.csproj --filter "FullyQualifiedName~PlanningWorkflow_ShouldUpdatePlanAndRequestionInformation"
```

## Required Configuration

Before running, configure `appsettings.Development.json` in `src/Travel.Tests/` and `src/Travel.Tests.Dashboard/` with Azure OpenAI credentials:

```json
{
  "LanguageModelSettings": {
    "DeploymentName": "<your-gpt-4o-mini-deployment-name>",
    "Endpoint": "<your-azure-openai-endpoint>"
  }
}
```

Authentication uses `DefaultAzureCredential` — no API key needed if you're logged into Azure CLI. Azure Storage is emulated locally via Azurite (launched by Aspire in a container).

## Architecture

### Workflow DAG

The core is a **directed acyclic graph workflow** with checkpoint/resume support. The graph is built in [WorkflowFactory.cs](src/Travel.Workflows/Services/WorkflowFactory.cs):

```
StartNode → ExtractionNode → UpdateNode → PlannerNode → ExecutionNode
                ↑                                              |
                |                                    (FunctionCallContent where
                |                                     Name == "request_information")
         InformationResponseNode ← [RequestPort] ← InformationRequestNode
                                                              |
                                                        EndNode (when "planning_complete")
```

**Node responsibilities:**
- `StartNode` — initializes `TravelPlanDto` from the incoming request
- `ExtractionNode` — calls the `extracting_agent` (LLM) to parse user input and emit `TravelPlanUpdateCommand` via tool call
- `UpdateNode` — applies `TravelPlanUpdateCommand` to the workflow context state
- `PlannerNode` — calls the `planning_agent` (LLM) with serialized `TravelPlanSummary` to determine what's missing
- `ExecutionNode` — routes the planner's tool call: `request_information` → loops back to user; `planning_complete` → ends
- `InformationRequestNode` / `InformationResponseNode` — suspend/resume the workflow for user input via `RequestPort`
- `EndNode` — emits `TravelPlanningCompleteEvent` with the final plan

### ReAct Loop Pattern

The workflow implements a **ReAct (Reason & Act)** loop:
1. Extract information from user message (ExtractionNode)
2. Update the travel plan state (UpdateNode)
3. Reason about what's still missing (PlannerNode)
4. Act: request more info or complete (ExecutionNode)
5. If requesting info, suspend workflow and wait for user response, then loop back to step 1

### AI Agents

Agents are defined via YAML templates in [src/Travel.Agents/Templates/](src/Travel.Agents/Templates/):
- `planning.yaml` — reasoning agent; audits `TravelPlanSummary` for null fields, calls `request_information` or `planning_complete`
- `extracting.yaml` — parsing agent; reads user natural language, calls `update_travel_plan` tool with structured `TravelPlanDto`
- `conversation.yaml` — conversation routing agent

Tools are defined as declaration-only `AIFunction` stubs in [PlanningTools.cs](src/Travel.Agents/Services/PlanningTools.cs) — the LLM sees the tool schema but execution is handled by the workflow nodes, not the tool functions themselves.

### Checkpoint/Resume

`TravelPlanningWorkflow` wraps a `Workflow` with state machine logic (`Created → Executing → Suspended → Completed`). Checkpoints are persisted via `CheckpointManager` (backed by `ICheckpointRepository`) so workflows can resume across process restarts. Passing `CheckpointInfo` in `TravelWorkflowRequest` resumes from where the workflow was suspended.

### State Sharing Between Nodes

Nodes communicate via `IWorkflowContext` using extension methods in [Extensions/](src/Travel.Workflows/Extensions/):
- `context.GetTravelPlan()` / `context.SetTravelPlan()` — reads/writes the current `TravelPlanDto` in shared context
- `context.SendMessageAsync(new TravelPlanUpdateCommand(...))` — passes typed messages to the next node

## Project Layout

```
src/
├── Agents/              # Shared agent infrastructure (AgentFactory, CustomPromptAgentFactory)
├── Infrastructure/      # Repository abstractions (Azure Blob, file system, in-memory)
├── Travel.Agents/       # Domain agents: PlanningTools, ExtractingTools, AgentProvider, YAML templates
├── Travel.Workflows/    # Workflow: nodes, WorkflowFactory, TravelPlanningWorkflow, TravelWorkflowService
├── Travel.Tests/        # xUnit integration tests with mock agents and scenario-driven test data
├── Travel.Tests.Dashboard/ # ASP.NET Core host for running tests via HTTP
├── ServiceDefaults/     # Shared Aspire service configuration (OpenTelemetry, service discovery)
└── AppHost/             # Aspire distributed application host
```

## Key Patterns

- **Central package management** — all NuGet versions in [Directory.Packages.props](Directory.Packages.props); add `<PackageVersion>` there, then reference without version in `.csproj`
- **Scenario-driven tests** — test cases are JSON files in `src/Travel.Tests/TestData/` (e.g., `PlanningWorkflowScenarios.json`). Each scenario defines agent mock responses and expected `TravelPlanDto`. Add new test cases there, not in C# code.
- **Mock agents in tests** — `WorkflowMockTestHarness2` sets up `Mock<IAgentProvider>` and `AgentFactoryHelper.CreateMockChatClient(meta)` to inject pre-scripted LLM responses without real Azure OpenAI calls
- **OpenTelemetry tracing** — every node starts an `Activity` via `TravelWorkflowTelemetry.InvokeNode(...)`. The Aspire dashboard (or external OTLP collector) receives traces. Tests can also send traces to the Aspire dashboard for visualization.
- **`AsDeclarationOnly()`** — tools passed to agents are declaration-only; the actual `AIFunction` implementations in `PlanningTools` are stubs. The workflow nodes inspect `FunctionCallContent` from agent responses and route accordingly.
