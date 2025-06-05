using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot.MCP.Application.Interfaces;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.MCP.Infra.Host.Services;

/// <summary>
/// Background service that continuously listens for new Telegram messages
/// and automatically processes them as they arrive
/// </summary>
public class TelegramMessageListener(
    ITelegramBotClient botClient,
    ITelegramRepository repository,
    ILogger<TelegramMessageListener> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting Telegram message listener...");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message }, // Only process messages
            DropPendingUpdates = true, // Ignore old messages when starting
        };

        try
        {
            // Start receiving messages using the event-driven approach
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken
            );

            logger.LogInformation("Telegram message listener started successfully");

            // Keep the service running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Telegram message listener stopped");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error in Telegram message listener");
            throw;
        }
    }

    /// <summary>
    /// Handles incoming Telegram updates (messages)
    /// </summary>
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Type != UpdateType.Message ||
                update.Message?.Text == null ||
                update.Message.From?.Username == null)
            {
                logger.LogDebug("Skipping non-text message or message without username from user {UserId}",
                    update.Message?.From?.Id);
                return;
            }

            var message = update.Message;
            var telegramUser = message.From;

            logger.LogInformation("Received message from {Username} ({UserId}): {MessageText}",
                telegramUser.Username, telegramUser.Id, message.Text);

            var user = await repository.CreateOrUpdateUserAsync(
                new Domain.User(
                    telegramUser.Id,
                    telegramUser.Username,
                    telegramUser.FirstName,
                    telegramUser.LastName,
                    false));

            var domainMessage = new Domain.Message(
                user,
                message.Text,
                message.Date.ToUniversalTime(),
                true); // Mark as incoming message

            await repository.SaveMessageAsync(domainMessage);

            logger.LogDebug("Message saved to database for user {Username}", telegramUser.Username);

            await ProcessIncomingMessage(user, message.Text, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing update {UpdateId}", update.Id);
        }
    }

    private async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Polling error occurred");

        // Implement retry logic or error recovery here if needed
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
    }

    protected virtual async Task ProcessIncomingMessage(Domain.User user, string messageText, CancellationToken cancellationToken)
    {
        if (messageText.StartsWith("/help", StringComparison.OrdinalIgnoreCase))
        {
            await botClient.SendMessage(
                chatId: user.Id,
                text: "Hello! I'm an MCP Telegram bot. I can help you manage users and groups.",
                cancellationToken: cancellationToken);
        }

        // Add more custom processing logic here as needed
        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Telegram message listener...");
        await base.StopAsync(cancellationToken);
    }
}
