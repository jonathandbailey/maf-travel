using AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorageServices();

var blobs = builder.AddAzureBlobsServices(storage);

 builder.AddProject<Projects.Travel_Gateway>("travel-gateway");


builder.AddProject<Projects.Travel_Workflows_A2A>("travel-workflows-a2a")
.WithReference(blobs)
.WaitFor(blobs);

builder.AddProject<Projects.Travel_Application_Api>("travel-application-api")
.WithReference(blobs)
.WaitFor(blobs);


var mcp = builder.AddProject<Projects.Travel_Application_Mcp>("travel-application-mcp")
.WithReference(blobs)
.WaitFor(blobs);


builder.AddProject<Projects.Travel_Agents_A2A>("travel-agents-a2a")
.WithReference(mcp)
.WithReference(blobs)
.WaitFor(blobs);



builder.Build().Run();
