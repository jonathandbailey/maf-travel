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
  InMemoryTravelPlanRepository.cs ← for tests (no storage dependency)
Documents/
  TravelPlanDocument.cs           ← internal persistence model (not exposed outside this project)
```

## Rules
- Accept domain models in public method signatures; map to/from Documents internally
- Documents are plain serializable types (records or classes) — no domain logic
- Serialize Documents as JSON via System.Text.Json
- InMemory implementations must implement the same interface and live in this project
- DI registration in `InfrastructureServiceCollectionExtensions.cs`
- References: Travel.Domain, Travel.Application, shared `Infrastructure/` project
