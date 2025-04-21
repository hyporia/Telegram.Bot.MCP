# TelegramBotMCP

TelegramBotMCP is a .NET 9.0 console application that implements a Telegram bot integrated with a Model Context Protocol (MCP) server. It allows your local agent to communicate with Telegram users via a bot account.
## Features

-   Telegram bot integration using [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot)
-   MCP server tool integration
-   Entity Framework Core with SQLite
-   OpenTelemetry tracing, metrics and logging

## Getting Started

### Prerequisites

-   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Configuration

-   Set your Telegram bot token in environment variables as `TELEGRAM_BOT_TOKEN`.

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

TBD

## Author

hyporia
