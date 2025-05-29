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
├── Abstractions/         # 定義核心介面，如 IPipeline、IStep、IStepFactory
├── Apis/                 # API 客戶端與介面
│   ├── Interfaces/       # API 介面定義
│   └── Clients/          # API 實作
├── Helpers/              # 共用註冊與輔助類別
├── Instrumentation/      # 監控與追蹤相關程式
├── Logging/              # 日誌格式化與相關設定
├── Messaging/         # 訊息佇列（如 Kafka、Solace）相關元件
├── Models/               # 資料模型（如 CaseContext）
├── Pipelines/            # Pipeline 與步驟實作
│   ├── Steps/            # 各種步驟（Step）實作
│   └── Factories/        # 步驟工廠
├── Services/             # Worker 服務本體
├── appsettings.json      # 設定檔
├── Program.cs            # 進入點與 DI 註冊
├── WorkerServicePipeline.csproj # 專案檔
└── README.md             # 專案說明文件

---



