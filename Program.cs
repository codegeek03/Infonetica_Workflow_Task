using WorkflowEngine.Models;
using WorkflowEngine.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configuration for persistence type
var persistenceType = builder.Configuration.GetValue<string>("PersistenceType") ?? "file";
var dataDirectory = builder.Configuration.GetValue<string>("DataDirectory") ?? "data";

// Add services based on configuration
if (persistenceType.ToLower() == "memory")
{
    builder.Services.AddSingleton<IPersistenceService, InMemoryPersistenceService>();
}
else
{
    builder.Services.AddSingleton<IPersistenceService>(provider => 
        new FilePersistenceService(dataDirectory));
}

builder.Services.AddSingleton<IWorkflowService, WorkflowService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Load existing data on startup (for file persistence)
var persistenceService = app.Services.GetRequiredService<IPersistenceService>();
if (persistenceService is FilePersistenceService)
{
    // This will initialize the data directory if it doesn't exist
    await persistenceService.LoadAllWorkflowDefinitionsAsync();
    await persistenceService.LoadAllWorkflowInstancesAsync();
}

// Workflow Definition endpoints
app.MapPost("/api/workflows", async (CreateWorkflowRequest request, IWorkflowService service) =>
{
    try
    {
        var definition = await service.CreateWorkflowDefinitionAsync(request);
        return Results.Created($"/api/workflows/{definition.Id}", definition);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/workflows/{id}", async (string id, IWorkflowService service) =>
{
    var definition = await service.GetWorkflowDefinitionAsync(id);
    return definition != null ? Results.Ok(definition) : Results.NotFound();
});

app.MapGet("/api/workflows", async (IWorkflowService service) =>
{
    var definitions = await service.GetAllWorkflowDefinitionsAsync();
    return Results.Ok(definitions);
});

// Workflow Instance endpoints
app.MapPost("/api/workflows/{definitionId}/instances", async (string definitionId, IWorkflowService service) =>
{
    try
    {
        var instance = await service.StartWorkflowInstanceAsync(definitionId);
        return Results.Created($"/api/instances/{instance.Id}", instance);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/instances/{id}", async (string id, IWorkflowService service) =>
{
    var instance = await service.GetWorkflowInstanceAsync(id);
    return instance != null ? Results.Ok(instance) : Results.NotFound();
});

app.MapPost("/api/instances/{id}/actions/{actionId}", async (string id, string actionId, IWorkflowService service) =>
{
    try
    {
        var instance = await service.ExecuteActionAsync(id, actionId);
        return Results.Ok(instance);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/api/instances", async (IWorkflowService service) =>
{
    var instances = await service.GetAllWorkflowInstancesAsync();
    return Results.Ok(instances);
});

app.Run();