# TelegramBotMCP

TelegramBotMCP is a .NET 9.0 console application that implements a Telegram bot integrated with a Model Context Protocol (MCP) server. It allows your local agent to communicate with Telegram users via a bot account.

## Features

-   Send messages to a user
-   Send messages to an admin user
-   Get new messages

## Getting Started

### Prerequisites

-   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Configuration

-   Set your Telegram bot token in the environment variable `TELEGRAM_BOT_TOKEN`.
-   Send a message to your bot.
-   Ask the agent to fetch new messages. It will also store the user who wrote to the bot in its SQLite database. The agent will provide the user's information along with the messages.
-   Ask the agent to send a message to the user.
-   You can also assign admin status to some users and send messages to all admins.

### How I use it

I [configured the `msp.json` file for my VSCode](https://code.visualstudio.com/docs/copilot/chat/mcp-servers) in the following way:

```Json
{
    "inputs": [
        {
            "type": "promptString",
            "id": "telegram_bot_token",
            "description": "Telegram Bot Token",
            "password": true
        }
    ],
    "servers": {
        "telegram-bot": {
            "command": "docker", // or podman
            "args": [
                "run",
                "-i",
                "--rm",
                "-e",
                "TELEGRAM_BOT_TOKEN",
                "telegrambotmcp:latest"
            ],
            "env": {
                "TELEGRAM_BOT_TOKEN": "${input:telegram_bot_token}"
            }
        }
    }
}
```

After I did that, Copilot in agent mode was able to see TelegramBotMCP tools and use them.

### Build and Run

```sh
dotnet build
dotnet run
```

### Docker

A `Dockerfile` is provided for containerized deployment.

## License

TBD

## Author

hyporia
