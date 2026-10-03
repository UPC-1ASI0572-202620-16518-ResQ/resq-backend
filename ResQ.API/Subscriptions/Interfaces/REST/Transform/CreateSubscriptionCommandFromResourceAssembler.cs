using ResQ.API.Subscriptions.Domain.Model.Commands;
using ResQ.API.Subscriptions.Interfaces.REST.Resources;

namespace ResQ.API.Subscriptions.Interfaces.REST.Transform;

public static class CreateSubscriptionCommandFromResourceAssembler
{
    public static CreateSubscriptionCommand ToCommandFromResource(Guid organizationId, CreateSubscriptionResource resource)
    {
        return new CreateSubscriptionCommand(organizationId, resource.StartDate, resource.EndDate);
    }
}