namespace FcmSender.Tests.Services;

using FcmSender.Core.Models;
using FcmSender.Core.Services;

public class FcmMessageFactoryTests
{
    private readonly FcmMessageFactory _sut = new();

    [Fact]
    public void Create_WithToken_SetsTokenTarget()
    {
        // Arrange
        var request = new FcmNotificationRequest
        {
            Token = "token-123",
            Title = "Hello",
            Body = "World",
            Data = new Dictionary<string, string>
            {
                ["key"] = "value"
            }
        };

        // Act
        var message = _sut.Create(request);

        // Assert
        Assert.Equal("token-123", message.Token);
        Assert.Null(message.Topic);
        Assert.NotNull(message.Notification);
        Assert.Equal("Hello", message.Notification.Title);
        Assert.Equal("value", message.Data["key"]);
    }

    [Fact]
    public void Create_WithoutTarget_UsesDefaultToken()
    {
        // Arrange
        var request = new FcmNotificationRequest
        {
            Title = "Fallback",
            Body = "Using default token"
        };

        // Act
        var message = _sut.Create(request, defaultDeviceToken: "default-token");

        // Assert
        Assert.Equal("default-token", message.Token);
    }

    [Fact]
    public void Create_WithoutAnyTarget_Throws()
    {
        // Arrange
        var request = new FcmNotificationRequest
        {
            Title = "Title"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.Create(request));
    }

    [Fact]
    public void Create_WithTopic_SetsTopicTarget()
    {
        // Arrange
        var request = new FcmNotificationRequest
        {
            Topic = "news",
            Body = "Latest updates"
        };

        // Act
        var message = _sut.Create(request);

        // Assert
        Assert.Equal("news", message.Topic);
        Assert.Null(message.Token);
    }
}
