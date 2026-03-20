# Travel.Infrastructure

Infrastructure layer — implements repository interfaces using `IAzureStorageRepository` from the shared `Infrastructure/` project.

## Core pattern: Domain → Document mapping
Domain models are accepted in repository method signatures but **never persisted directly**.
Each repository maps to an internal Document type before storage:

```
TravelPlan (domain)  ──map──►  TravelPlanDocument (internal)  ──JSON──►  blob
```

This decouples domain from persistence. To swap to a different store (Table Storage, Cosmos, Mongo),
only the repository implementation changes — domain and application layers are unaffected.

## IAzureStorageRepository (from shared `Infrastructure/`)
Inject this interface into every repository. Key methods:
- `UploadTextBlobAsync(blobName, containerName, content, "application/json")`
- `DownloadTextBlobAsync(blobName, containerName)`
- `ListBlobsAsync(containerName, prefix)`
- `DeleteBlobAsync(blobName, containerName)`
- `BlobExists(blobName, containerName)` / `ContainerExists(containerName)`

## Folder structure
```
Repositories/
  TravelPlanRepository.cs    ← writes: AddAsync, UpdateAsync, DeleteAsync
Queries/
  TravelPlanQuery.cs         ← reads: GetAsync, ListAsync
Documents/
  TravelPlanDocument.cs      ← internal persistence model (not exposed outside this project)
Common/
  Json.cs                    ← shared JsonSerializerOptions (Json.JsonOptions)
```

## Rules
- Accept domain models in public method signatures; map to/from Documents internally
- Documents are plain serializable types (records or classes) — no domain logic
- Serialize Documents as JSON via System.Text.Json
- DI registration in `InfrastructureServiceCollectionExtensions.cs`
- References: Travel.Domain, Travel.Application, shared `Infrastructure/` project

## Not-found and error handling
- `GetAsync` throws `NotFoundException` (from `Travel.Application.Exceptions`) when the blob does not exist — never returns null; `ITravelPlanRepository.GetAsync` return type is `Task<TravelPlan>` (non-nullable)
- `UpdateAsync` must verify the plan exists before uploading — throw `NotFoundException` if the blob is not found; do not silently upsert
- `DeleteAsync` throws `NotFoundException` if the blob does not exist — delete is not idempotent
- `EnsureContainerAsync` is called only in `AddAsync` by design: a plan cannot exist to update or delete if it was never added, so container existence is guaranteed transitively
- `ListAsync` logs `LogWarning` for each blob that fails to deserialize, then skips it — never swallow deserialization failures silently

## Coding Rules

These rules apply to all repositories and query classes in this project. The canonical reference for all rules is `TravelPlanRepository.GetAsync` (lines 23–44).

### 1 — Always use braces `{}`, even for single-line blocks
No brace-free one-liners on `if`, `else`, `foreach`, etc.

```csharp
// ✓ correct
if (!await storageRepository.BlobExists(blobName, ContainerName))
{
    logger.LogError("TravelPlan {Id} not found", id);
    throw new NotFoundException($"TravelPlan {id} not found");
}

// ✗ wrong
if (!await storageRepository.BlobExists(blobName, ContainerName))
    throw new NotFoundException($"TravelPlan {id} not found");
```

### 2 — Always log before throwing exceptions
Every `throw` must be preceded by a log call at the appropriate level (`LogError` for hard failures, `LogWarning` for soft/skippable failures). Repositories and query classes that throw must inject `ILogger<T>`.

```csharp
// ✓ correct
logger.LogError("TravelPlan {Id} not found in container {Container}", id, ContainerName);
throw new NotFoundException($"TravelPlan {id} not found in container {ContainerName}");
```

### 3 — Use shared `Json.JsonOptions` — never define local `JsonSerializerOptions`
Use `Travel.Infrastructure.Common.Json.JsonOptions` in every repository and query class. Do not define a local `JsonSerializerOptions` field.

```csharp
// ✓ correct
var json = JsonSerializer.Serialize(document, Json.JsonOptions);

// ✗ wrong — duplicated definition
private static readonly JsonSerializerOptions JsonOptions = new() { ... };
```

### 4 — Pragmatic CQRS: query handlers use `Queries/`, command handlers use `Repositories/`
Repositories retain `GetAsync`/`ListAsync` for use by **command handlers only** — commands need to load aggregates before mutating them (e.g. verify existence, apply domain logic, then save). Dedicated query classes under `Queries/` are used by **query handlers only**. The separation is enforced by caller convention, not by removing methods from interfaces:

- Command handlers → inject the repository interface (`ITravelPlanRepository`, `ISessionRepository`, etc.)
- Query handlers → inject the query interface (`ITravelPlanQuery`, `ISessionQuery`, etc.)

### 5 — Use blank lines to separate logical blocks within methods
Each phase (guard check, load, deserialize, map, return) gets its own visual block separated by a blank line.

```csharp
// ✓ correct
var blobName = BlobName(id);

if (!await storageRepository.BlobExists(blobName, ContainerName))
{
    logger.LogError("TravelPlan {Id} not found", id);
    throw new NotFoundException($"TravelPlan {id} not found");
}

var json = await storageRepository.DownloadTextBlobAsync(blobName, ContainerName);
var document = JsonSerializer.Deserialize<TravelPlanDocument>(json, Json.JsonOptions);

if (document is null)
{
    logger.LogError("TravelPlan {Id} failed to deserialize", id);
    throw new JsonException($"TravelPlan {id} failed to deserialize");
}

return ToDomain(document);
```
