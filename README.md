# Telegram.Bot.MCP

## Overview

Telegram.Bot.MCP serves as a bridge between AI agents (like GitHub Copilot) and the Telegram messaging platform. The application allows AI assistants to interact with Telegram users via a bot, maintain user information in a SQLite database, and can be configured and run via Docker.

## Features

### Message Management

-   Read new messages from Telegram users
-   Send messages to specific users
-   Send messages to the current user marked as "Me"

### User Management

-   Store and retrieve user data (ID, username, first name, last name)
-   Set a specific user as "Me" to enable direct interactions
-   View all registered users
-   View conversation history with users

### Group Management

-   Create user groups (not Telegram groups, but logical groups for managing users)
-   Add/remove users to/from groups
-   List all groups and their members
-   Get groups a user belongs to
-   Broadcast messages to all members of a group

## Quickstart Guide

### Step 1: Set up a Telegram Bot

1. Create a new bot through [@BotFather](https://t.me/botfather) in Telegram
2. Copy your bot token (looks like `123456789:ABCdefGhIjKlmNoPQRsTUVwxYZ`)

### Step 2: Run using Docker

```bash
# Pull and run the container
docker run -d --name telegram-bot-mcp \
  -e TELEGRAM_BOT_TOKEN=your_token_here \
  telegrambotmcp:latest
```

### Step 3: Configure VSCode for MCP

Create an `mcp.json` file in your `.vscode` folder with the following configuration:

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

### Step 4: Use the Bot with Copilot

1. Start Copilot Chat in VS Code and prompt: "Connect to telegram-bot server"
2. When prompted, enter your Telegram bot token
3. Send a message to your bot in Telegram
4. In Copilot Chat, ask: "Read all messages using available tool"
5. Try other commands like:
    - "Send a message to user 123456789"
    - "Create group named Admins"
    - "Add user 123456789 to Admins group"

### Building from Source

A `Dockerfile` is provided in the repository for building your own container image:

```bash
git clone https://github.com/hyporia/Telegram.Bot.MCP.git
cd Telegram.Bot.MCP
docker build -t telegrambotmcp:latest .
```

## License

TBD

## Author

hyporia
