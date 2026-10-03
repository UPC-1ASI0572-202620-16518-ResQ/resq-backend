namespace ResQ.API.Subscriptions.Domain.Model.Queries;

public record GetSubscriptionByIdQuery(Guid OrganizationId, Guid SubscriptionId);