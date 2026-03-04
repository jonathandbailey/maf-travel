
using AppHost.Extensions;
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorageServices();

var blobs = builder.AddAzureBlobsServices(storage);

var storageInit = builder.AddProject<Projects.Travel_Storage_Init>("storage-init")
    .WithReference(blobs)
    .WaitFor(blobs)
    .WithEnvironment("AzureStorageSettings__AgentThreadContainerName", "agent-threads")
    .WithEnvironment("AzureStorageSettings__CheckpointContainerName", "checkpoints");

var api = builder.AddProject<Projects.Travel_Experience_Api>("travel-experience-api")
    .WithReference(blobs)
    .WaitFor(blobs)
    .WaitForCompletion(storageInit);

var ui = builder.AddUiServices(api);

builder.Build().Run();
