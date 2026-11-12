namespace ResQ.API.Monitoring.Infrastructure.ThingSpeak;

/// <summary>
/// Converts ThingSpeak fields into backend measurement contracts without
/// silently interpreting MQ-2 raw ADC as calibrated PPM.
/// </summary>
public static class ThingSpeakMeasurementMapper
{
    private static readonly (string VariableType, string Unit)[] FieldDefinitions =
    [
        ("GAS_ADC", "ADC"),
        ("TEMPERATURE_C", "C"),
        ("WIFI_RSSI", "DBM"),
        ("ALERT_FLAG", "BOOLEAN"),
        ("MOTION", "BOOLEAN"),
        ("HUMIDITY", "PERCENT")
    ];

    public static IReadOnlyList<ThingSpeakMeasurement> Map(
        ThingSpeakFeedEntry entry)
    {
        var result = new List<ThingSpeakMeasurement>();
        for (var index = 0; index < Math.Min(entry.Fields.Count, FieldDefinitions.Length); index++)
        {
            if (!entry.Fields[index].HasValue)
                continue;

            var (variableType, unit) = FieldDefinitions[index];
            result.Add(new ThingSpeakMeasurement(
                entry.EntryId, index + 1, variableType,
                entry.Fields[index]!.Value, unit, entry.CreatedAt));
        }

        return result;
    }
}

public sealed record ThingSpeakMeasurement(
    long EntryId,
    int FieldNumber,
    string VariableType,
    decimal Value,
    string Unit,
    DateTimeOffset MeasuredAt);
