FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM base AS final
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "TelegramBotMCP.dll"]