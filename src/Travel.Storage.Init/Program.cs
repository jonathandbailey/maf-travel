using Azure.Storage.Blobs;

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__blobs")
    ?? throw new InvalidOperationException("Missing required environment variable: ConnectionStrings__blobs");

var agentThreadContainer =
    Environment.GetEnvironmentVariable("AzureStorageSettings__AgentThreadContainerName")
    ?? "agent-threads";

var checkpointContainer =
    Environment.GetEnvironmentVariable("AzureStorageSettings__CheckpointContainerName")
    ?? "checkpoints";

var travelPlanContainer =
    Environment.GetEnvironmentVariable("TravelPlanStorageSettings__ContainerName")
    ?? "travel-plans";

Console.WriteLine("Ensuring blob containers exist...");

var client = new BlobServiceClient(connectionString);

await client.GetBlobContainerClient(agentThreadContainer).CreateIfNotExistsAsync();
Console.WriteLine($"  '{agentThreadContainer}' - ready");

await client.GetBlobContainerClient(checkpointContainer).CreateIfNotExistsAsync();
Console.WriteLine($"  '{checkpointContainer}' - ready");

await client.GetBlobContainerClient(travelPlanContainer).CreateIfNotExistsAsync();
Console.WriteLine($"  '{travelPlanContainer}' - ready");

Console.WriteLine("Storage initialization complete.");
