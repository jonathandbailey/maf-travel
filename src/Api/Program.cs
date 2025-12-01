using Api;
using Api.Hub;
using Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

builder.Services.AddSignalR();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapApi();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.MapHub<UserHub>("hub");



app.Run();
