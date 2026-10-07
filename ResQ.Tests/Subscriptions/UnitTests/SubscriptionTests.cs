using ResQ.API.Subscriptions.Domain.Model.Aggregates;
using ResQ.API.Subscriptions.Domain.Model.ValueObjects;

namespace ResQ.Tests.Subscriptions.UnitTests;

public class SubscriptionTests
{
    private static readonly Guid OrganizationId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static Subscription CreateSubscription(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        return Subscription.Create(
            OrganizationId,
            startDate ?? DateTime.UtcNow.AddDays(-1),
            endDate ?? DateTime.UtcNow.AddDays(30));
    }

    private static Subscription CreateExpiredSubscription()
    {
        var subscription = Subscription.Create(
            OrganizationId,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30));

        typeof(Subscription)
            .GetProperty(nameof(Subscription.EndDate))!
            .SetValue(subscription, DateTime.UtcNow.AddMinutes(-1));

        return subscription;
    }


    // ============================================================
    // CREATE
    // ============================================================

    [Fact]
    public void Create_ShouldCreateActiveSubscription()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(30);

        // Act
        var subscription = Subscription.Create(
            OrganizationId,
            startDate,
            endDate);

        // Assert
        Assert.NotEqual(Guid.Empty, subscription.Id.Value);
        Assert.Equal(OrganizationId, subscription.OrganizationId);
        Assert.Equal(ESubscriptionStatus.Active, subscription.Status);
        Assert.Equal(startDate, subscription.StartDate);
        Assert.Equal(endDate, subscription.EndDate);
    }


    [Fact]
    public void Create_ShouldRejectEmptyOrganizationId()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(30);

        // Act
        var action = () => Subscription.Create(
            Guid.Empty,
            startDate,
            endDate);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }


    [Fact]
    public void Create_ShouldRejectEndDateBeforeStartDate()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(5);

        // Act
        var action = () => Subscription.Create(
            OrganizationId,
            startDate,
            endDate);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }


    [Fact]
    public void Create_ShouldRejectEndDateEqualToStartDate()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = startDate;

        // Act
        var action = () => Subscription.Create(
            OrganizationId,
            startDate,
            endDate);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }


    [Fact]
    public void Create_ShouldRejectEndDateInThePast()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var action = () => Subscription.Create(
            OrganizationId,
            startDate,
            endDate);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }


    // ============================================================
    // IS ACTIVE
    // ============================================================

    [Fact]
    public void IsActive_ShouldReturnTrueForActiveSubscription()
    {
        // Arrange
        var subscription = CreateSubscription(
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(30));

        // Act
        var result = subscription.IsActive();

        // Assert
        Assert.True(result);
    }


    [Fact]
    public void IsActive_ShouldReturnFalseBeforeStartDate()
    {
        // Arrange
        var subscription = CreateSubscription(
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30));

        // Act
        var result = subscription.IsActive();

        // Assert
        Assert.False(result);
    }


    [Fact]
    public void IsActive_ShouldReturnFalseAfterEndDate()
    {
        // Arrange
        var subscription = CreateSubscription();

        typeof(Subscription)
            .GetProperty(nameof(Subscription.EndDate))!
            .SetValue(subscription, DateTime.UtcNow.AddMinutes(-1));

        // Act
        var result = subscription.IsActive();

        // Assert
        Assert.False(result);
    }


    [Fact]
    public void IsActive_ShouldReturnFalseAfterCancellation()
    {
        // Arrange
        var subscription = CreateSubscription();

        subscription.Cancel();

        // Act
        var result = subscription.IsActive();

        // Assert
        Assert.False(result);
    }


    [Fact]
    public void IsActive_ShouldReturnFalseAfterExpiration()
    {
        // Arrange
        var subscription = CreateExpiredSubscription();

        subscription.Expire();

        // Act
        var result = subscription.IsActive();

        // Assert
        Assert.False(result);
    }


    // ============================================================
    // CANCEL
    // ============================================================

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var subscription = CreateSubscription();

        // Act
        subscription.Cancel();

        // Assert
        Assert.Equal(
            ESubscriptionStatus.Cancelled,
            subscription.Status);
    }


    [Fact]
    public void Cancel_ShouldRejectAlreadyCancelledSubscription()
    {
        // Arrange
        var subscription = CreateSubscription();

        subscription.Cancel();

        // Act
        var action = () => subscription.Cancel();

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(
            "Only an active subscription can be cancelled.",
            exception.Message);
    }


    [Fact]
    public void Cancel_ShouldRejectExpiredSubscription()
    {
        // Arrange
        var subscription = CreateExpiredSubscription();

        subscription.Expire();

        // Act
        var action = () => subscription.Cancel();

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(
            "Only an active subscription can be cancelled.",
            exception.Message);
    }


    // ============================================================
    // EXPIRE
    // ============================================================

    [Fact]
    public void Expire_ShouldRejectSubscriptionBeforeEndDate()
    {
        // Arrange
        var subscription = CreateSubscription();

        // Act
        var action = () => subscription.Expire();

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(
            "The subscription has not reached its expiration date.",
            exception.Message);
    }


    [Fact]
    public void Expire_ShouldChangeStatusToExpired()
    {
        // Arrange
        var subscription = CreateExpiredSubscription();

        // Act
        subscription.Expire();

        // Assert
        Assert.Equal(
            ESubscriptionStatus.Expired,
            subscription.Status);
    }


    [Fact]
    public void Expire_ShouldRejectCancelledSubscription()
    {
        // Arrange
        var subscription = CreateSubscription();

        subscription.Cancel();

        // Act
        var action = () => subscription.Expire();

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(
            "Only an active subscription can expire.",
            exception.Message);
    }


    [Fact]
    public void Expire_ShouldRejectAlreadyExpiredSubscription()
    {
        // Arrange
        var subscription = CreateExpiredSubscription();

        subscription.Expire();

        // Act
        var action = () => subscription.Expire();

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(
            "Only an active subscription can expire.",
            exception.Message);
    }


    // ============================================================
    // RENEW
    // ============================================================

    [Fact]
    public void Renew_ShouldReactivateExpiredSubscription()
    {
        // Arrange
        var subscription = CreateExpiredSubscription();

        subscription.Expire();

        var newEndDate = DateTime.UtcNow.AddDays(365);
        var beforeRenewal = DateTime.UtcNow;

        // Act
        subscription.Renew(newEndDate);

        var afterRenewal = DateTime.UtcNow;

        // Assert
        Assert.Equal(
            ESubscriptionStatus.Active,
            subscription.Status);

        Assert.Equal(
            newEndDate,
            subscription.EndDate);

        Assert.InRange(
            subscription.StartDate,
            beforeRenewal,
            afterRenewal);
    }


    [Fact]
    public void Renew_ShouldRejectActiveSubscription()
    {
        // Arrange
        var subscription = CreateSubscription();

        var newEndDate = DateTime.UtcNow.AddDays(365);

        // Act
        var action = () => subscription.Renew(newEndDate);

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(
            "Only an expired subscription can be renewed.",
            exception.Message);
    }


    [Fact]
    public void Renew_ShouldRejectCancelledSubscription()
    {
        // Arrange
        var subscription = CreateSubscription();

        subscription.Cancel();

        var newEndDate = DateTime.UtcNow.AddDays(365);

        // Act
        var action = () => subscription.Renew(newEndDate);

        // Assert
        var exception =
            Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(
            "Only an expired subscription can be renewed.",
            exception.Message);
    }


    [Fact]
    public void Renew_ShouldRejectEndDateInThePast()
    {
        // Arrange
        var subscription = CreateExpiredSubscription();

        subscription.Expire();

        var newEndDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var action = () => subscription.Renew(newEndDate);

        // Assert
        var exception =
            Assert.Throws<ArgumentException>(action);

        Assert.Equal(
            "The new end date must be in the future. (Parameter 'newEndDate')",
            exception.Message);
    }
}