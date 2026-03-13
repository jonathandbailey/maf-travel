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
  TravelPlanRepository.cs         ← implements ITravelPlanRepository; injects IAzureStorageRepository
Documents/
  TravelPlanDocument.cs           ← internal persistence model (not exposed outside this project)
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
