using System.Text.RegularExpressions;

namespace ResQ.API.Device_Management.Domain.Model.ValueObjects;

public record ExternalDeviceReference
{
    private static readonly Regex SourceSystemPattern =
        new("^[A-Za-z0-9_-]+$", RegexOptions.Compiled);

    public string SourceSystem { get; }
    public string ExternalDeviceId { get; }

    private ExternalDeviceReference()
    {
        SourceSystem = null!;
        ExternalDeviceId = null!;
    }

    public ExternalDeviceReference(string sourceSystem, string externalDeviceId)
    {
        if (string.IsNullOrWhiteSpace(sourceSystem))
            throw new ArgumentException(
                "Source system is required.",
                nameof(sourceSystem));

        if (string.IsNullOrWhiteSpace(externalDeviceId))
            throw new ArgumentException(
                "External device id is required.",
                nameof(externalDeviceId));

        var normalizedSourceSystem =
            sourceSystem.Trim().ToLowerInvariant();

        var normalizedExternalDeviceId =
            externalDeviceId.Trim();

        if (normalizedSourceSystem.Length > 80)
            throw new ArgumentException(
                "Source system cannot exceed 80 characters.",
                nameof(sourceSystem));

        if (!SourceSystemPattern.IsMatch(normalizedSourceSystem))
            throw new ArgumentException(
                "Source system can only contain ASCII letters, numbers, hyphens and underscores.",
                nameof(sourceSystem));

        if (normalizedExternalDeviceId.Length > 120)
            throw new ArgumentException(
                "External device id cannot exceed 120 characters.",
                nameof(externalDeviceId));

        SourceSystem = normalizedSourceSystem;
        ExternalDeviceId = normalizedExternalDeviceId;
    }
}