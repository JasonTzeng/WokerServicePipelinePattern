# WorkerServicePipeline

A .NET 8 worker service for executing configurable pipelines and steps using dependency injection and configuration-driven design.

---

## Features

- **Configurable Pipelines**: Define and select pipelines to run via `appsettings.json`.
- **Step Factory**: Dynamically create and execute steps for each pipeline.
- **Dependency Injection**: Uses `Microsoft.Extensions.DependencyInjection` for service management.
- **Logging**: Integrated with `Microsoft.Extensions.Logging` and supports custom JSON console formatting.
- **API Integration**: Built-in HTTP client with Polly-based resilience (retry, circuit breaker).
- **Messaging**: Supports consuming from Kafka and Solace.
- **OpenTelemetry**: Tracing support for pipeline and step execution.
- **Hot Reload**: Supports .NET Hot Reload for rapid development and testing.
- **Extensible**: Easily add new pipelines, steps, and integrations.

---

## How It Works

- Each Worker reads the pipeline type from configuration and resolves it using DI.
- Each Pipeline reads its step list from configuration and uses `IStepFactory` to instantiate each step.
- Each step implements the `IStep` interface and is executed in order.
- Steps can interact with external systems (e.g., HTTP APIs, Kafka, Solace) and update a shared `CaseContext`.
- Logging and tracing are available for all steps and pipelines.

---

## Extending

- **Add a new pipeline**: Implement `IPipeline` and register it in DI.
- **Add a new step**: Implement `IStep` and update `IStepFactory` to support it.
- **Add new API clients**: Implement the interface in `Apis/Interfaces/` and the client in `Apis/Clients/`, then register via `Helpers/ServiceRegistrationHelpers.cs`.
- **Configure steps**: Update `appsettings.json` to include new steps in the desired pipeline.

---

## Dependency Injection Registration

All pipelines, steps, factories, API clients, and message consumers are registered in `Program.cs` and `Helpers/ServiceRegistrationHelpers.cs`.

---

## Running

1. Update `appsettings.json` with your pipeline and steps configuration.
2. Build and Run the service:

---

## Future Works

- Dead letter queue support for failed messages
- TBD

---

## Folder Structure

WorkerServicePipeline/
├── Abstractions/         # Defines core interfaces, such as IPipeline, IStep, IStepFactory
├── Apis/                 # API clients and interfaces
│   ├── Interfaces/       # API interface definitions
│   └── Clients/          # API implementations
├── Helpers/              # Shared registration and helper classes
├── Instrumentation/      # Monitoring and tracing related code
├── Logging/              # Log formatting and related settings
├── Messaging/            # Messaging components (e.g., Kafka, Solace)
├── Models/               # Data models (e.g., CaseContext)
├── Pipelines/            # Pipeline and step implementations
│   ├── Steps/            # Step implementations
│   └── Factories/        # Step factories
├── Services/             # Worker service implementations
├── Docker/               # Dockerfiles and containerization resources
├── appsettings.json      # Configuration file
├── Program.cs            # Entry point and DI registration
├── WorkerServicePipeline.csproj # Project file
└── README.md             # Project documentation

---



