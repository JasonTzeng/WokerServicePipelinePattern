using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Console;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using WorkerServicePipeline.Abstractions;
using WorkerServicePipeline.Helpers;
using WorkerServicePipeline.Logging;
using WorkerServicePipeline.Messaging;
using WorkerServicePipeline.Models;
using WorkerServicePipeline.Pipelines;
using WorkerServicePipeline.Pipelines.Factories;
using WorkerServicePipeline.Services;

var builder = WebApplication.CreateBuilder(args);

// Host Services Register
builder.Services.AddHostedService<Worker1>();
builder.Services.AddHostedService<Worker2>();
// Pipeline Register
builder.Services.AddScoped<Pipeline1>();
builder.Services.AddScoped<Pipeline2>();
// Factory Register
builder.Services.AddSingleton<IStepFactory, SetupFactory>();
// Step Register
ServiceRegistrationHelpers.RegisterAllSteps(builder.Services);
// Model Register
builder.Services.AddScoped<CaseContext>();
// Messaging Register
builder.Services.AddKeyedSingleton<IEventConsumer, KafkaConsumer>("kafka");
builder.Services.AddKeyedSingleton<IEventConsumer, SolaceConsumer>("solace");
builder.Services.AddKeyedSingleton<IEventPublisher, KafkaPublisher>("kafka");
builder.Services.AddKeyedSingleton<IEventPublisher, SolacePublisher>("solace");
// HttpClient Register
ServiceRegistrationHelpers.RegisterHttpClients(builder.Services);
// Logging Register
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = "customJson";
});
builder.Logging.AddConsoleFormatter<CustomJsonConsoleFormatter, ConsoleFormatterOptions>();
// OpenTelemetry Register
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

// Middleware & Endpoint Register
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
