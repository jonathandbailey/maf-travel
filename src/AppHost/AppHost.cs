
using AppHost.Extensions;
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorageServices();

var blobs = builder.AddAzureBlobsServices(storage);

var api = builder.AddProject<Projects.Travel_Experience_Api>("travel-experience-api").WithReference(blobs)
    .WaitFor(blobs);

var ui = builder.AddUiServices(api);

builder.Build().Run();
