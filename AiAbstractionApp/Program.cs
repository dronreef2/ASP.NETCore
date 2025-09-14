using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

// --- Dependency Injection Setup ---

// 1. Add support for a distributed in-memory cache.
builder.Services.AddDistributedMemoryCache();

// 2. Register the base Ollama client. This will be used by the non-cached endpoints
//    and as the underlying client for the cached version.
builder.Services.AddSingleton(new OllamaApiClient(new Uri("http://localhost:11434"), "llama3:latest"));

// 3. Register the cached client as a "keyed" service.
// This allows us to have multiple IChatClient implementations and choose the one we want at the endpoint.
builder.Services.AddKeyedSingleton<IChatClient>("cached", (sp, key) =>
{
    // Resolve the dependencies needed for the builder from the service provider (sp).
    var ollamaClient = sp.GetRequiredService<OllamaApiClient>();
    var cache = sp.GetRequiredService<IDistributedCache>();

    // Use ChatClientBuilder to compose the client with a distributed cache layer.
    return new ChatClientBuilder(ollamaClient)
        .UseDistributedCache(cache)
        .Build();
});

// --- Application Setup ---

var app = builder.Build();

// A simple record to structure the incoming request body.
public record Question(string Prompt);

// --- Endpoints ---

// Endpoint 1: Basic prompt-to-response using the direct client.
app.MapPost("/", async (Question question, OllamaApiClient client) =>
{
    var result = await client.GetResponseAsync(question.Prompt);
    return Results.Ok(result.Text);
});

// Endpoint 2: Advanced interaction with a system prompt to guide the AI.
app.MapPost("/v2", async (Question question, OllamaApiClient client) =>
{
    var messages = new ChatMessage[]
    {
        new(ChatRole.System, "You are a weather expert, answer me in just one sentence within 50 words."),
        new(ChatRole.User, question.Prompt)
    };

    var result = await client.GetResponseAsync(messages);
    return Results.Ok(result.Text);
});

// Endpoint 3: Caching demonstration.
// This uses the keyed "cached" client.
app.MapPost("/cached", async (Question question, [FromKeyedServices("cached")] IChatClient client) =>
{
    // The first call for a specific prompt will go to the AI.
    // Subsequent identical prompts will be served instantly from the cache.
    var result = await client.GetResponseAsync(question.Prompt);

    // We return a custom object to show whether the response was a cache hit.
    return Results.Ok(new { Response = result.Text, IsFromCache = result.IsFromCache });
});

app.Run();
