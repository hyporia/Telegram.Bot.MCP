# TelegramBotMCP

TelegramBotMCP is a .NET 9.0 console application that implements a Telegram bot with Model Context Protocol (MCP) server integration. It uses Entity Framework Core with SQLite for data storage, Serilog for logging, and OpenTelemetry for observability.

## Features

-   Telegram bot integration using [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot)
-   MCP server tool integration
-   Entity Framework Core with SQLite
-   Serilog logging (file and console)
-   OpenTelemetry tracing and metrics

## Getting Started

### Prerequisites

-   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
-   SQLite (optional, database file is created automatically)

### Configuration

-   Set your Telegram bot token in user secrets or environment variables as `TELEGRAM_BOT_TOKEN`.
-   Adjust `appsettings.json` and `appsettings.Development.json` as needed.

### Build and Run

```sh
dotnet build
dotnet run
```

### Docker

A `Dockerfile` is provided for containerized deployment.

## Project Structure

-   `Program.cs`: Application entry point and configuration
-   `Data/`: Entity Framework Core context and repository
-   `Models/`: Data models
-   `Tools/`: MCP server tools

## License

Specify your license here (e.g., MIT, Apache-2.0).

## Author

Piligrimm
