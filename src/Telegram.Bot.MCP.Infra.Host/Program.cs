using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Telegram.Bot;
using Telegram.Bot.MCP.Application.Interfaces;
using Telegram.Bot.MCP.Application.Tools;
using Telegram.Bot.MCP.Infra.Persistance;
using Telegram.Bot.MCP.Infra.Persistance.Repositories;
using Telegram.Bot.MCP.Services;

var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
{
    EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environments.Production,
});

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

builder.Configuration.AddConfiguration(configuration);

builder.Logging.ClearProviders()
    .AddConsole(x => x.LogToStandardErrorThreshold = LogLevel.Trace)
    .AddOpenTelemetry(x =>
    {
        x.IncludeScopes = true;
        x.IncludeFormattedMessage = true;
    });

var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "telegram.db");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddTransient<ITelegramRepository, TelegramRepository>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<TelegramBotTools>()
    ;

var token = builder.Configuration["TELEGRAM_BOT_TOKEN"];
if (string.IsNullOrEmpty(token))
{
    throw new ArgumentException("Bot token is not set. Please set the BotToken in the configuration.");
}
builder.Services
    .AddTransient<ITelegramBot, TelegramBot>()
    .AddHttpClient<ITelegramBotClient, TelegramBotClient>(httpClient => new(token, httpClient));

builder.Services.AddOpenTelemetry()
    .WithTracing(builder =>
    {
        builder
            .AddEntityFrameworkCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    })
    .WithMetrics(builder =>
    {
        builder
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    })
    .WithLogging(x => x.AddOtlpExporter());

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

    if (!Directory.Exists(Path.GetDirectoryName(dbPath)!))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        logger.LogInformation("Directory for SQLite database created successfully.");
    }

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
}

await app.RunAsync();