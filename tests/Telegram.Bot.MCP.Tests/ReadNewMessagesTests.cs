using AutoFixture.Xunit3;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;
using Telegram.Bot.MCP.Application.Interfaces;
using TelegramBotMCP.Tests._seedWork;
using static Telegram.Bot.MCP.Application.Tools.ReadNewMessagesTool;

namespace TelegramBotMCP.Tests;

public class ReadNewMessagesTests(ITestOutputHelper output) : IAsyncLifetime
{
    private readonly TestFixture _testFixture = new(output);

    public ValueTask InitializeAsync() => _testFixture.InitializeAsync();

    public ValueTask DisposeAsync() => _testFixture.DisposeAsync();

    [Theory, AutoData]
    public async Task ReadNewMessages_WithMessages_ReturnsFormattedJson(Message[] updates)
    {
        // Arrange
        _testFixture.BotMock.Setup(m => m.ReadNewMessages(It.IsAny<int>())).ReturnsAsync(updates);

        // Act
        var result = await _testFixture.SUT.Handle(new(100), CancellationToken.None);

        // Assert
        var messagesSentAsResponse = JsonSerializer.Deserialize<List<NewMessageDto>>(result);
        Assert.NotNull(messagesSentAsResponse);
        Assert.Equal(updates.Length, messagesSentAsResponse.Count);
        Assert.All(messagesSentAsResponse, message =>
        {
            var incomingMessage = updates.FirstOrDefault(u => u.Text == message.Message);
            Assert.NotNull(incomingMessage);
            Assert.Equal(incomingMessage.From.Id, message.UserId);
            Assert.Equal(incomingMessage.From.Username, message.From);
        });

        // Verify persistence
        var savedMessages = await _testFixture.DbContext.Messages.Include(m => m.User).ToListAsync(CancellationToken.None);
        Assert.Equal(updates.Length, savedMessages.Count);
        Assert.All(savedMessages, savedMessage =>
        {
            var incomingMessage = updates.FirstOrDefault(u => u.Text == savedMessage.Text);
            Assert.NotNull(incomingMessage);
            Assert.Equal(incomingMessage.Timestamp.ToUniversalTime(), savedMessage.Timestamp);
            Assert.Equal(incomingMessage.Text, savedMessage.Text);
            Assert.Equal(incomingMessage.From.Username, savedMessage.User.Username);
            Assert.Equal(incomingMessage.From.FirstName, savedMessage.User.FirstName);
            Assert.Equal(incomingMessage.From.LastName, savedMessage.User.LastName);
        });
    }

    [Fact]
    public async Task ReadNewMessages_NoMessages_ReturnsEmptyArray()
    {
        // Arrange
        _testFixture.BotMock.Setup(m => m.ReadNewMessages(It.IsAny<int>())).ReturnsAsync([]);

        // Act
        var result = await _testFixture.SUT.Handle(new(100), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("[]", result);
    }

    [Fact]
    public async Task ReadNewMessages_ExceptionThrown_ReturnsErrorMessage()
    {
        // Arrange
        _testFixture.BotMock.Setup(m => m.ReadNewMessages(It.IsAny<int>())).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _testFixture.SUT.Handle(new(100), CancellationToken.None);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("Failed to get unread messages", result);
        Assert.Contains("Test exception", result);
    }
}
