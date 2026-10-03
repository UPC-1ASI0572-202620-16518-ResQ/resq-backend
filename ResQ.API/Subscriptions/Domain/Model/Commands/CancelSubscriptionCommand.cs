namespace ResQ.API.Subscriptions.Domain.Model.Commands;

public record CancelSubscriptionCommand(Guid OrganizationId, Guid SubscriptionId);