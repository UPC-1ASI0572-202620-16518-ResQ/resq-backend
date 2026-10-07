using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using ResQ.API.Subscriptions.Domain.Model.Aggregates;
using ResQ.API.Subscriptions.Domain.Model.ValueObjects;
using ResQ.API.Subscriptions.Infrastructure.Persistence.EFC.Repositories;

namespace ResQ.Tests.Subscriptions.IntegrationTests;

public class SubscriptionPersistenceIntegrationTests
{
    private static async Task<(SqliteConnection Connection, AppDbContext Context)> CreateContextAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        await context.Database.EnsureCreatedAsync();

        return (connection, context);
    }

    private static Subscription CreateSubscription(
        Guid organizationId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var effectiveStartDate = startDate ?? DateTime.UtcNow;
        var effectiveEndDate = endDate ?? effectiveStartDate.AddYears(1);

        return Subscription.Create(
            organizationId,
            effectiveStartDate,
            effectiveEndDate);
    }


    // ============================================================
    // PERSISTENCE
    // ============================================================

    [Fact]
    public async Task ShouldPersistSubscriptionAndRetrieveItFromDatabase()
    {
        // Arrange
        var organizationId = Guid.NewGuid();

        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var subscription = CreateSubscription(organizationId);

        // Act
        context.Set<Subscription>().Add(subscription);

        await context.SaveChangesAsync();

        await using var verificationContext = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options);

        var persistedSubscription = await verificationContext
            .Set<Subscription>()
            .SingleAsync(s => s.Id == subscription.Id);

        // Assert
        Assert.NotNull(persistedSubscription);

        Assert.Equal(
            subscription.Id,
            persistedSubscription.Id);

        Assert.Equal(
            organizationId,
            persistedSubscription.OrganizationId);

        Assert.Equal(
            subscription.StartDate,
            persistedSubscription.StartDate);

        Assert.Equal(
            subscription.EndDate,
            persistedSubscription.EndDate);

        Assert.Equal(
            subscription.Status,
            persistedSubscription.Status);
    }


    [Fact]
    public async Task ShouldPersistActiveSubscription()
    {
        // Arrange
        var organizationId = Guid.NewGuid();

        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var subscription = CreateSubscription(organizationId);

        // Act
        context.Set<Subscription>().Add(subscription);

        await context.SaveChangesAsync();

        // Assert
        var persistedSubscription = await context
            .Set<Subscription>()
            .SingleAsync(s => s.Id == subscription.Id);

        Assert.Equal(
            ESubscriptionStatus.Active,
            persistedSubscription.Status);

        Assert.Equal(
            organizationId,
            persistedSubscription.OrganizationId);
    }


    // ============================================================
    // FIND BY ID
    // ============================================================

    [Fact]
    public async Task FindByIdAsync_ShouldReturnSubscriptionForMatchingOrganization()
    {
        // Arrange
        var organizationId = Guid.NewGuid();

        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var subscription = CreateSubscription(organizationId);

        context.Set<Subscription>().Add(subscription);

        await context.SaveChangesAsync();

        var repository = new SubscriptionRepository(context);

        // Act
        var result = await repository.FindByIdAsync(
            subscription.Id.Value,
            organizationId);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            subscription.Id,
            result.Id);

        Assert.Equal(
            organizationId,
            result.OrganizationId);
    }


    [Fact]
    public async Task FindByIdAsync_ShouldReturnNullForDifferentOrganization()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var differentOrganizationId = Guid.NewGuid();

        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var subscription = CreateSubscription(organizationId);

        context.Set<Subscription>().Add(subscription);

        await context.SaveChangesAsync();

        var repository = new SubscriptionRepository(context);

        // Act
        var result = await repository.FindByIdAsync(
            subscription.Id.Value,
            differentOrganizationId);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task FindByIdAsync_ShouldReturnNullForUnknownSubscription()
    {
        // Arrange
        var organizationId = Guid.NewGuid();

        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var repository = new SubscriptionRepository(context);

        // Act
        var result = await repository.FindByIdAsync(
            Guid.NewGuid(),
            organizationId);

        // Assert
        Assert.Null(result);
    }


    // ============================================================
    // FIND BY ORGANIZATION
    // ============================================================

    [Fact]
    public async Task FindByOrganizationIdAsync_ShouldReturnSubscription()
    {
        // Arrange
        var organizationId = Guid.NewGuid();

        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var subscription = CreateSubscription(organizationId);

        context.Set<Subscription>().Add(subscription);

        await context.SaveChangesAsync();

        var repository = new SubscriptionRepository(context);

        // Act
        var result = await repository.FindByOrganizationIdAsync(
            organizationId);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            subscription.Id,
            result.Id);

        Assert.Equal(
            organizationId,
            result.OrganizationId);
    }


    [Fact]
    public async Task FindByOrganizationIdAsync_ShouldReturnNullWhenOrganizationHasNoSubscription()
    {
        // Arrange
        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var repository = new SubscriptionRepository(context);

        // Act
        var result = await repository.FindByOrganizationIdAsync(
            Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }


    // ============================================================
    // EXISTS BY ORGANIZATION
    // ============================================================

    [Fact]
    public async Task ExistsByOrganizationIdAsync_ShouldReturnTrueForActiveSubscription()
    {
        // Arrange
        var organizationId = Guid.NewGuid();

        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var subscription = CreateSubscription(organizationId);

        context.Set<Subscription>().Add(subscription);

        await context.SaveChangesAsync();

        var repository = new SubscriptionRepository(context);

        // Act
        var result = await repository.ExistsByOrganizationIdAsync(
            organizationId);

        // Assert
        Assert.True(result);
    }


    [Fact]
    public async Task ExistsByOrganizationIdAsync_ShouldReturnFalseWhenOrganizationHasNoActiveSubscription()
    {
        // Arrange
        var database = await CreateContextAsync();

        await using var connection = database.Connection;
        await using var context = database.Context;

        var repository = new SubscriptionRepository(context);

        // Act
        var result = await repository.ExistsByOrganizationIdAsync(
            Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}