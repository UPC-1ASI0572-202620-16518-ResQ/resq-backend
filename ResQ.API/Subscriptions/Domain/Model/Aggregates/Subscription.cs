using ResQ.API.Subscriptions.Domain.Model.ValueObjects;

namespace ResQ.API.Subscriptions.Domain.Model.Aggregates;

public class Subscription
{
    public SubscriptionId Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public ESubscriptionStatus Status { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    protected Subscription()
    {
    }

    private Subscription(SubscriptionId id, Guid organizationId, DateTime startDate, DateTime endDate)
    {
        Id = id;
        OrganizationId = organizationId;
        Status = ESubscriptionStatus.Active;
        StartDate = startDate;
        EndDate = endDate;
    }

    public static Subscription Create(Guid organizationId, DateTime startDate, DateTime endDate)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));

        if (endDate <= startDate)
            throw new ArgumentException("End date must be greater than start date.", nameof(endDate));

        if (endDate <= DateTime.UtcNow)
            throw new ArgumentException("End date must be in the future.", nameof(endDate));

        return new Subscription(
            SubscriptionId.New(),
            organizationId,
            startDate,
            endDate);
    }

    public bool IsActive()
    {
        var now = DateTime.UtcNow;

        return Status == ESubscriptionStatus.Active &&
               now >= StartDate &&
               now < EndDate;
    }

    public void Cancel()
    {
        if (Status != ESubscriptionStatus.Active)
            throw new InvalidOperationException("Only an active subscription can be cancelled.");

        Status = ESubscriptionStatus.Cancelled;
    }

    public void Expire()
    {
        if (Status != ESubscriptionStatus.Active)
            throw new InvalidOperationException("Only an active subscription can expire.");

        if (DateTime.UtcNow < EndDate)
            throw new InvalidOperationException("The subscription has not reached its expiration date.");

        Status = ESubscriptionStatus.Expired;
    }

    public void Renew(DateTime newEndDate)
    {
        if (Status != ESubscriptionStatus.Expired)
            throw new InvalidOperationException("Only an expired subscription can be renewed.");

        if (newEndDate <= DateTime.UtcNow)
            throw new ArgumentException("The new end date must be in the future.", nameof(newEndDate));

        StartDate = DateTime.UtcNow;
        EndDate = newEndDate;
        Status = ESubscriptionStatus.Active;
    }
}