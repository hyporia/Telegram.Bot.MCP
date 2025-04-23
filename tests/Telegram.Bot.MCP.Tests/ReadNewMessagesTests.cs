using AutoFixture.Xunit3;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;
using Telegram.Bot.MCP.Services.Abstract;
using Telegram.Bot.MCP.Tools;
using TelegramBotMCP.Tests._seedWork;

namespace TelegramBotMCP.Tests;

public class ReadNewMessagesTests(ITestOutputHelper output) : IAsyncLifetime
{
    private readonly TestFixture _testFixture = new(output);

    public ValueTask InitializeAsync() => _testFixture.InitializeAsync();

    public ValueTask DisposeAsync() => _testFixture.DisposeAsync();

    [Theory, AutoData]
    public async Task ReadNewMessages_WithMessages_ReturnsFormattedJson(UpdateDTO[] updates)
    {
        // Arrange
        _testFixture.BotMock.Setup(m => m.GetUpdates(It.IsAny<int>())).ReturnsAsync(updates);

        // Act
        var result = await _testFixture.TelegramBotTools.ReadNewMessages();

        // Assert
        var messagesSentAsResponse = JsonSerializer.Deserialize<List<TelegramBotTools.NewMessageDto>>(result);
        Assert.NotNull(messagesSentAsResponse);
        Assert.Equal(updates.Length, messagesSentAsResponse.Count);
        Assert.All(messagesSentAsResponse, message =>
        {
            var incomingMessage = updates.FirstOrDefault(u => u.Message!.Text == message.Message);
            Assert.NotNull(incomingMessage);
            Assert.Equal(incomingMessage.Message!.From!.Id, message.UserId);
            Assert.Equal(incomingMessage.Message.From.Username, message.From);
        });

        // Verify persistence
        var savedMessages = await _testFixture.DbContext.Messages.Include(m => m.User).ToListAsync(CancellationToken.None);
        Assert.Equal(updates.Length, savedMessages.Count);
        Assert.All(savedMessages, savedMessage =>
        {
            var incomingMessage = updates.FirstOrDefault(u => u.Message!.Text == savedMessage.Text);
            Assert.NotNull(incomingMessage);
            Assert.Equal(incomingMessage.Message!.Date.ToUniversalTime(), savedMessage.Timestamp);
            Assert.Equal(incomingMessage.Message.Text, savedMessage.Text);
            Assert.Equal(incomingMessage.Message.From!.Username, savedMessage.User.Username);
            Assert.Equal(incomingMessage.Message.From.FirstName, savedMessage.User.FirstName);
            Assert.Equal(incomingMessage.Message.From.LastName, savedMessage.User.LastName);
        });
    }

    [Fact]
    public async Task ReadNewMessages_NoMessages_ReturnsEmptyArray()
    {
        // Arrange
        _testFixture.BotMock.Setup(m => m.GetUpdates(It.IsAny<int>())).ReturnsAsync([]);

        // Act
        var result = await _testFixture.TelegramBotTools.ReadNewMessages();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("[]", result);
    }

    [Fact]
    public async Task ReadNewMessages_ExceptionThrown_ReturnsErrorMessage()
    {
        // Arrange
        _testFixture.BotMock.Setup(m => m.GetUpdates(It.IsAny<int>())).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _testFixture.TelegramBotTools.ReadNewMessages();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("Failed to get unread messages", result);
        Assert.Contains("Test exception", result);
    }
}
