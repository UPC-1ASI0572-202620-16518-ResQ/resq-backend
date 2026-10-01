namespace ResQ.API.Incident_Management.Domain.Model.ValueObjects;

public record AttendantId
{
    public Guid Value { get; }

    public AttendantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Attendant id cannot be empty.", nameof(value));

        Value = value;
    }
}