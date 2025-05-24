# Telegram.Bot.MCP

[![Docker Image Version](https://img.shields.io/docker/v/hyporia123/telegram-bot-mcp?sort=semver&label=docker)](https://hub.docker.com/r/hyporia123/telegram-bot-mcp)
[![Docker Pulls](https://img.shields.io/docker/pulls/hyporia123/telegram-bot-mcp)](https://hub.docker.com/r/hyporia123/telegram-bot-mcp)
[![Docker Image Size](https://img.shields.io/docker/image-size/hyporia123/telegram-bot-mcp)](https://hub.docker.com/r/hyporia123/telegram-bot-mcp)

## Overview

Telegram.Bot.MCP serves as a bridge between AI agents (like GitHub Copilot) and the Telegram messaging platform. The application allows AI assistants to interact with Telegram users via a bot, maintain user information in a SQLite database, and can be configured and run via Docker.

## Features

### Message Management

- Read new messages from Telegram users
- Send messages to specific users
- Send messages to the current user marked as "Me"

### User Management

- Store and retrieve user data (ID, username, first name, last name)
- Set a specific user as "Me" to enable direct interactions
- View all registered users
- View conversation history with users

### Group Management

- Create user groups (not Telegram groups, but logical groups for managing users)
- Add/remove users to/from groups
- List all groups and their members
- Get groups a user belongs to
- Broadcast messages to all members of a group

## Quickstart Guide

### Start with Docker

#### Step 1: Set up a Telegram Bot

1. Create a new bot through [@BotFather](https://t.me/botfather) in Telegram
2. Copy your bot token (looks like `123456789:ABCdefGhIjKlmNoPQRsTUVwxYZ`)

#### Step 2: Run using Docker

```bash
# Pull and run the container
docker run -d --name telegram-bot-mcp \
    -e TELEGRAM_BOT_TOKEN=your_token_here \
    hyporia123/telegram-bot-mcp:latest
```

> **Docker Image**: [`hyporia123/telegram-bot-mcp:latest`](https://hub.docker.com/r/hyporia123/telegram-bot-mcp)  
> **Docker Hub**: [https://hub.docker.com/r/hyporia123/telegram-bot-mcp](https://hub.docker.com/r/hyporia123/telegram-bot-mcp)

### Integrate with GitHub Copilot in VSCode

#### Step 1: Configure VSCode

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
                "hyporia123/telegram-bot-mcp:latest"
            ],
            "env": {
                "TELEGRAM_BOT_TOKEN": "${input:telegram_bot_token}"
            }
        }
    }
}
```

#### Step 2: Use the Bot with Copilot

1. When prompted, enter your Telegram bot token
2. Send a message to your bot in Telegram
3. In Copilot Chat, ask: "Read all messages using available tool"
4. Try other commands like:
   - "Send a message to user 123456789"
   - "Create group named Admins"
   - "Add user 123456789 to Admins group"

#### PS: you can also add this MCP server manually in the Copilot Chat settings in VSCode.

1. Open Copilot Chat
2. Switch to agent mode
3. Click on 'Select tools' icon
4. Scroll down to the bottom and click on 'Add more tools'
5. Follow the instructions to add telegram-bot-mcp as a new tool

## Docker Image

The Docker image for this project is available on Docker Hub:

- **Repository**: [hyporia123/telegram-bot-mcp](https://hub.docker.com/r/hyporia123/telegram-bot-mcp)
- **Tags**:
  - `latest`: Most recent stable build
  - `x.y.z`: Version specific tags

### Image Information

- Based on .NET 9.0 Alpine
- Runs as a non-root user for improved security
- Data persisted in `/var/lib/telegram-bot-mcp/telegram.db`

### Usage

```bash
docker pull hyporia123/telegram-bot-mcp:latest
```

For custom configuration:

```bash
docker run -d \
  --name telegram-bot-mcp \
  -e TELEGRAM_BOT_TOKEN=your_token_here \
  -v telegram_data:/var/lib/telegram-bot-mcp \
  hyporia123/telegram-bot-mcp:latest
```

### Building from Source

A `Dockerfile` is provided in the repository for building your own container image:

```bash
git clone https://github.com/hyporia/Telegram.Bot.MCP.git
cd Telegram.Bot.MCP
docker build -t telegram-bot-mcp:latest .
```

## License

TBD

## Author

hyporia
