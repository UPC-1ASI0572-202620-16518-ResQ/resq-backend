using ResQ.API.Subscriptions.Domain.Model.ValueObjects;

namespace ResQ.Tests.Subscriptions.UnitTests;

public class SubscriptionIdTests
{
    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    [Fact]
    public void Constructor_ShouldCreateSubscriptionIdWithValidGuid()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var subscriptionId = new SubscriptionId(value);

        // Assert
        Assert.Equal(value, subscriptionId.Value);
    }


    [Fact]
    public void Constructor_ShouldRejectEmptyGuid()
    {
        // Arrange
        var value = Guid.Empty;

        // Act
        var action = () => new SubscriptionId(value);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }


    // ============================================================
    // NEW
    // ============================================================

    [Fact]
    public void New_ShouldGenerateNonEmptyGuid()
    {
        // Act
        var subscriptionId = SubscriptionId.New();

        // Assert
        Assert.NotEqual(Guid.Empty, subscriptionId.Value);
    }


    [Fact]
    public void New_ShouldGenerateDifferentIds()
    {
        // Act
        var firstId = SubscriptionId.New();
        var secondId = SubscriptionId.New();

        // Assert
        Assert.NotEqual(firstId.Value, secondId.Value);
    }


    // ============================================================
    // EQUALITY
    // ============================================================

    [Fact]
    public void SubscriptionId_ShouldBeEqualWhenValuesAreTheSame()
    {
        // Arrange
        var value = Guid.NewGuid();

        var firstId = new SubscriptionId(value);
        var secondId = new SubscriptionId(value);

        // Assert
        Assert.Equal(firstId, secondId);
    }


    [Fact]
    public void SubscriptionId_ShouldNotBeEqualWhenValuesAreDifferent()
    {
        // Arrange
        var firstId = new SubscriptionId(Guid.NewGuid());
        var secondId = new SubscriptionId(Guid.NewGuid());

        // Assert
        Assert.NotEqual(firstId, secondId);
    }
}