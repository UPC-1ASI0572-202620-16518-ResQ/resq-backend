using ResQ.API.Subscriptions.Domain.Model.ValueObjects;

namespace ResQ.API.Subscriptions.Interfaces.REST.Resources;

public record SubscriptionResource(Guid Id, Guid OrganizationId, ESubscriptionStatus Status, DateTime StartDate, DateTime EndDate);