namespace ResQ.API.Subscriptions.Domain.Model.ValueObjects;

public record SubscriptionId
{
    public Guid Value { get; }

    public SubscriptionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Subscription id cannot be empty.", nameof(value));

        Value = value;
    }

    public static SubscriptionId New()
    {
        return new SubscriptionId(Guid.NewGuid());
    }
}