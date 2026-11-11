namespace ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;

/// <summary>
/// Network status of a sensor. Exposed through the API as ONLINE, OFFLINE or TIMEOUT.
/// </summary>
public enum EConnectionStatus
{
    Online,
    Offline,
    Timeout
}
