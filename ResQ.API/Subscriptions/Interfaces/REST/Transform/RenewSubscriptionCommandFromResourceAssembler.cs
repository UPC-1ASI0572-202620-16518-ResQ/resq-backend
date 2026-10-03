using ResQ.API.Subscriptions.Domain.Model.Commands;
using ResQ.API.Subscriptions.Interfaces.REST.Resources;

namespace ResQ.API.Subscriptions.Interfaces.REST.Transform;

public static class RenewSubscriptionCommandFromResourceAssembler
{
    public static RenewSubscriptionCommand ToCommandFromResource(Guid subscriptionId, Guid organizationId, RenewSubscriptionResource resource)
    {
        return new RenewSubscriptionCommand(subscriptionId, organizationId, resource.NewEndDate);
    }
}