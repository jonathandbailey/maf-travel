using AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorageServices();

var blobs = builder.AddAzureBlobsServices(storage);

builder.AddProject<Projects.Travel_Application_Api>("travel-application-api")
.WithReference(blobs)
.WaitFor(blobs);


var mcp = builder.AddProject<Projects.Travel_Application_Mcp>("travel-application-mcp")
.WithReference(blobs)
.WaitFor(blobs);


builder.Build().Run();
