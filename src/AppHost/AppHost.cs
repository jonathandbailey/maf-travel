
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);



builder.AddProject<Projects.Travel_Experience_Api>("travel-experience-api");



builder.Build().Run();
