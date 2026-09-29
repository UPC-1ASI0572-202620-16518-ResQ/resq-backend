namespace ResQ.API.Device_Management.Domain.Model.ValueObjects;

public record DeviceSpecifications
{
    public string? Manufacturer { get; }
    public string? Model { get; }
    public string? SerialNumber { get; }

    private DeviceSpecifications()
    {
    }

    public DeviceSpecifications(
        string? manufacturer,
        string? model,
        string? serialNumber)
    {
        Manufacturer = Normalize(manufacturer, nameof(manufacturer));
        Model = Normalize(model, nameof(model));
        SerialNumber = Normalize(serialNumber, nameof(serialNumber));
    }

    private static string? Normalize(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > 100)
            throw new ArgumentException(
                $"{parameterName} cannot exceed 100 characters.",
                parameterName);

        return normalized;
    }
}