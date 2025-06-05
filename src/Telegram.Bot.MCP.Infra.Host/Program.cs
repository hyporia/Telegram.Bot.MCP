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
using Telegram.Bot.MCP.Application.Resources;
using Telegram.Bot.MCP.Application.Tools;
using Telegram.Bot.MCP.Infra.Host.Services;
using Telegram.Bot.MCP.Infra.Persistance;
using Telegram.Bot.MCP.Infra.Persistance.Repositories;

var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
{
    EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environments.Production,
});

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

builder.Configuration.AddConfiguration(configuration);

builder.Logging.ClearProviders()
    .AddConfiguration(builder.Configuration.GetSection("Logging"))
    .AddConsole(x => x.LogToStandardErrorThreshold = LogLevel.Trace);

// Add OpenTelemetry only if configured
var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (!string.IsNullOrEmpty(otlpEndpoint))
{
    builder.Logging.AddOpenTelemetry(x =>
    {
        x.IncludeScopes = true;
        x.IncludeFormattedMessage = true;
    });
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")!, sqliteOptions =>
    {
        sqliteOptions.CommandTimeout(30);
    });

    if (builder.Environment.IsProduction())
    {
        options.EnableSensitiveDataLogging(false);
        options.EnableDetailedErrors(false);
    }

    options.EnableServiceProviderCaching();
});
builder.Services.AddTransient<ITelegramRepository, TelegramRepository>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(GetAllUsersTool).Assembly)
    .WithResourcesFromAssembly(typeof(ConversationResource).Assembly)
    ;

var token = builder.Configuration["TELEGRAM_BOT_TOKEN"];
if (string.IsNullOrEmpty(token))
{
    Console.Error.WriteLine("Bot token is not set. Please provide bot token in 'TELEGRAM_BOT_TOKEN' environment variable.");
    return 1;
}
builder.Services
    .AddTransient<ITelegramBot, TelegramBot>()
    .AddHttpClient<ITelegramBotClient, TelegramBotClient>(httpClient => new(token, httpClient));

if (!string.IsNullOrEmpty(otlpEndpoint))
{
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
}

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
}

await app.RunAsync();
return 0;