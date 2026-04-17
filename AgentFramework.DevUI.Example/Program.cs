using AgentFramework.DevUI.Example;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenTelemetry.Resources;
using System.ComponentModel;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Configure the chat client (Ollama endpoint) ──────────
// Configuration - Ollama endpoint and model selection
var endpoint = "http://localhost:11434";  // Default Ollama endpoint
var modelName = "llama3.2";              // Model to use for AI responses
using OllamaApiClient chatClient = new(new Uri(endpoint), modelName);
builder.Services.AddChatClient(chatClient);

// ── 2. Create agents for the workflow ───────────────────────────
var reviewerAgentName = "Reviewer";
var reviewerInstructions = "You are a content editor. Give 2-3 bullet points of actionable feedback on the draft you receive. Be specific and concise.";
var writerAgentName = "Writer";
var writerInstructions = "You are a content writer. Write a clear, engaging paragraph on the topic provided. Be concise — 3-5 sentences.";

var writerAgent = new ChatClientAgent(chatClient, name: writerAgentName,
    instructions:writerInstructions );

var reviewerAgent = new ChatClientAgent(chatClient,name: reviewerAgentName,
instructions: reviewerInstructions );

var reviewerAgentBuilder = builder.AddAIAgent(reviewerAgentName, reviewerInstructions);
var writerAgentBuilder = builder.AddAIAgent(writerAgentName, writerInstructions);


// ── 3. Build the workflow graph ──────────────────────────────────

var writer = new WriterExecutor(writerAgent);
var reviewer = new ReviewerExecutor(reviewerAgent);
var formatter = new FormatterExecutor();

var workflow = new WorkflowBuilder(writer)// writer is the entry point
    .WithName("ContentReviewWorkflow")
    .AddEdge(writer, reviewer)
    .AddEdge(reviewer, formatter)
    .Build();

// ── 4. Register the workflow with the hosting layer ──────────────

// The workflow name shows up in the DevUI sidebar
builder.AddWorkflow("ContentReviewWorkflow", (sp, name) => {
    var agents = new List<IHostedAgentBuilder>() { reviewerAgentBuilder, writerAgentBuilder }.Select(ab => sp.GetRequiredKeyedService<AIAgent>(ab.Name));
    return AgentWorkflowBuilder.BuildSequential(workflowName: name, agents: agents);

    //Uncomment this line to register our mixed workflow
    //return workflow;
}, ServiceLifetime.Transient).AddAsAIAgent(); //AddAsAIAgent makes the workflow available as an agent in the DevUI, allowing you to run it step-by-step and inspect inputs/outputs at each stage.


// ── 3. Register OpenAI-compatible Responses + Conversations APIs ────

// These are required by DevUI to intercept and display traffic.
builder.Services.AddOpenAIResponses();
builder.Services.AddOpenAIConversations();

var app = builder.Build();

// ── 4. Map the OpenAI API endpoints ─────────────────────────────────
app.MapOpenAIResponses();
app.MapOpenAIConversations();

// ── 5. Map DevUI (development only) ──────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapDevUI();
}

// ── 6. Add minimal API endpoint to run the workflow ──────────────────
app.MapPost("/api/workflow/run", async (WorkflowRequest request, [FromKeyedServices("ContentReviewWorkflow")] Workflow workflow) =>
{
    Run run = await InProcessExecution.RunAsync(workflow, request.Input);
    WorkflowEvent result = null;
    foreach (WorkflowEvent evt in run.OutgoingEvents)
    {
        if (evt is WorkflowOutputEvent response)
        {
            result = evt;
        }
    }
    return Results.Ok(new { result });
})
.WithName("RunWorkflow");

await app.RunAsync();

record WorkflowRequest(string Input);
