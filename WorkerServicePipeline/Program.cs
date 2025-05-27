using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Console;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using WorkerServicePipeline.Abstractions;
using WorkerServicePipeline.Apis.Clients;
using WorkerServicePipeline.Apis.Interfaces;
using WorkerServicePipeline.Logging;
using WorkerServicePipeline.Messaging;
using WorkerServicePipeline.Models;
using WorkerServicePipeline.Pipelines;
using WorkerServicePipeline.Pipelines.Factories;
using WorkerServicePipeline.Pipelines.Steps;
using WorkerServicePipeline.Services;

var builder = WebApplication.CreateBuilder(args);

// Host Services Register
builder.Services.AddHostedService<Worker1>();
//builder.Services.AddHostedService<Worker2>();
// Pipeline Register
builder.Services.AddScoped<Pipeline1>();
builder.Services.AddScoped<Pipeline2>();
// Factory Register
builder.Services.AddSingleton<IStepFactory, SetupFactory>();
// Step Register
builder.Services.AddScoped<Enrich1>();
builder.Services.AddScoped<Enrich2>();
builder.Services.AddScoped<Enrich3>();
builder.Services.AddScoped<ConsumeFromKafka>();
builder.Services.AddScoped<PublishToKafka>();
// Model Register
builder.Services.AddScoped<CaseContext>();
// Messaging Register
builder.Services.AddSingleton<IEventPublisher, KafkaPublisher>();
builder.Services.AddSingleton<IEventConsumer, KafkaConsumer>();
// HttpClient Register
builder.Services.AddHttpClient<IFakeApiClient, FakeApiClient>(client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
});
// Logging Register
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = "customJson";
});
builder.Logging.AddConsoleFormatter<CustomJsonConsoleFormatter, ConsoleFormatterOptions>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WorkerServicePipeline"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("WorkerServicePipeline")
            .AddOtlpExporter(otlp =>
            {
                otlp.Endpoint = new Uri("http://localhost:4317");
            });
    });
// Healcheck Register
builder.Services.AddHealthChecks();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(50001);
});

var app = builder.Build();

app.UseRouting();
app.MapMetrics(); //Prometheus metrics endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.Run();
