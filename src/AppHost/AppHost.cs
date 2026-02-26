
using AppHost.Extensions;
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Travel_Experience_Api>("travel-experience-api");

var ui = builder.AddUiServices(api);

builder.Build().Run();
