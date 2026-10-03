namespace ResQ.API.Subscriptions.Interfaces.ACL;

public interface ISubscriptionsContextFacade
{
    Task<bool> HasActiveSubscription(Guid organizationId);
}