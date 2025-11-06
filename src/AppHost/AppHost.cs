var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Api>("api")
    .WithEndpoint("http", endpoint =>
    {
        endpoint.Port = 5000;
    })
    .WithEndpoint("https", endpoint =>
    {
        endpoint.Port = 5001;
    });

builder.Build().Run();
