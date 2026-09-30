namespace ResQ.API.Incident_Management.Domain.Model.ValueObjects;

public record ZoneId
{
    public Guid Value { get; }

    public ZoneId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Zone id cannot be empty.", nameof(value));

        Value = value;
    }
}