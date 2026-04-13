using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenTelemetry.Resources;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Configure the chat client (Ollama endpoint) ──────────
// Configuration - Ollama endpoint and model selection
var endpoint = "http://localhost:11434";  // Default Ollama endpoint
var modelName = "llama3.2";              // Model to use for AI responses
using OllamaApiClient chatClient = new(new Uri(endpoint), modelName);
builder.Services.AddChatClient(chatClient)
    .UseOpenTelemetry(sourceName: "Chat Client", configure: cfg =>
{
    // Only enable sensitive data locally — exposes prompts/responses in traces
    cfg.EnableSensitiveData = builder.Environment.IsDevelopment();
});

// ── 2. Register agents ──────────────────────────────────────────────
// Define tool functions
static string GetWeather(string city) => $"Weather in {city}: 18°C, partly cloudy."; // Replace with real API call
static string GetForecast(string city, int days) => $"{days}-day forecast for {city}: mostly sunny with occasional showers.";

// Create tool descriptors
var weatherTools = new[]
{
    AIFunctionFactory.Create(GetWeather, "get_weather", "Get the current weather for a city"),
    AIFunctionFactory.Create(GetForecast, "get_forecast", "Get a multi-day weather forecast"),
};


// Register an agent with tools
builder
    .AddAIAgent(
        "WeatherBot",
        "You are a friendly weather assistant. Always ask the user for their city if they haven't provided it."
    )
    .WithAITools(weatherTools);

// ── 3. Register OpenAI-compatible Responses + Conversations APIs ────

// These are required by DevUI to intercept and display traffic.
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("DevUI.Example"))
    .WithTracing(tracing => tracing
        .AddSource("Chat Client")
        .AddSource("Microsoft.Agents.AI")   // Agent Framework's own spans
    );

var app = builder.Build();

// ── 4. Map the OpenAI API endpoints ─────────────────────────────────
app.MapOpenAIResponses();
app.MapOpenAIConversations();

// ── 5. Map DevUI (development only) ──────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapDevUI();
}

app.Run();
