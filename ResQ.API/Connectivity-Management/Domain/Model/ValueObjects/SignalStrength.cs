namespace ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;

/// <summary>
/// Quality of the wireless connection of a sensor, in dBm.
/// Valid values are between -100 dBm (worst) and 0 dBm (best).
/// </summary>
public record SignalStrength
{
    public const int MinDbm = -100;
    public const int MaxDbm = 0;

    public int Dbm { get; init; }

    private SignalStrength(int dbm)
    {
        Dbm = dbm;
    }

    public static SignalStrength Create(int dbm)
    {
        if (dbm < MinDbm || dbm > MaxDbm)
            throw new ArgumentException($"Signal strength must be between {MinDbm} and {MaxDbm} dBm.");

        return new SignalStrength(dbm);
    }
}
