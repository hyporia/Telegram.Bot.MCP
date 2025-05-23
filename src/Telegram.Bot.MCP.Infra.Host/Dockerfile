FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Telegram.Bot.MCP.Infra.Host/Telegram.Bot.MCP.Infra.Host.csproj
RUN dotnet publish src/Telegram.Bot.MCP.Infra.Host/Telegram.Bot.MCP.Infra.Host.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS final

RUN addgroup -g 1001 -S appgroup && \
    adduser -u 1001 -S appuser -G appgroup

WORKDIR /app
COPY --from=build /app/publish .

RUN mkdir -p /var/lib/telegram-bot-mcp && \
    chown -R appuser:appgroup /var/lib/telegram-bot-mcp && \
    chmod 755 /var/lib/telegram-bot-mcp
fv
# Set connection string to use standard Alpine data directory
ENV ConnectionStrings__DefaultConnection="Data Source=/var/lib/telegram-bot-mcp/telegram.db"

# Create volume for persistent data storage using standard location
VOLUME ["/var/lib/telegram-bot-mcp"]

# Switch to non-root user
USER appuser

EXPOSE 80
ENTRYPOINT ["dotnet", "Telegram.Bot.MCP.dll"]