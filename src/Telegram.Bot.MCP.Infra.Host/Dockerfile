FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY . .
RUN dotnet restore Telegram.Bot.MCP.Infra.Host/Telegram.Bot.MCP.Infra.Host.csproj
RUN dotnet publish Telegram.Bot.MCP.Infra.Host/Telegram.Bot.MCP.Infra.Host.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Telegram.Bot.MCP.dll"]