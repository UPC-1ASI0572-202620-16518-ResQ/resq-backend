namespace ResQ.API.Incident_Management.Domain.Model.ValueObjects;

public record AttendantId
{
    public int Value { get; }

    public AttendantId(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Attendant id must be greater than zero.", nameof(value));

        Value = value;
    }
}