namespace ResQ.API.Incident_Management.Domain.Model.ValueObjects;

public record IncidentId
{
    public Guid Value { get; }

    public IncidentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Incident id cannot be empty.", nameof(value));

        Value = value;
    }

    public static IncidentId New()
    {
        return new IncidentId(Guid.NewGuid());
    }
}