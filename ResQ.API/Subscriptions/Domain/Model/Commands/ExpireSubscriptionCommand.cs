namespace ResQ.API.Subscriptions.Domain.Model.Commands;

public record ExpireSubscriptionCommand(Guid OrganizationId, Guid SubscriptionId);