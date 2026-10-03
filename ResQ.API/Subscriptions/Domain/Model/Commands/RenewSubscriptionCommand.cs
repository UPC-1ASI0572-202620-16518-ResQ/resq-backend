namespace ResQ.API.Subscriptions.Domain.Model.Commands;

public record RenewSubscriptionCommand(Guid OrganizationId, Guid SubscriptionId, DateTime NewEndDate);