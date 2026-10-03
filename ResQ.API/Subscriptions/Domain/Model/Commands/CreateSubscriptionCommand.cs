namespace ResQ.API.Subscriptions.Domain.Model.Commands;

public record CreateSubscriptionCommand(Guid OrganizationId, DateTime StartDate, DateTime EndDate);