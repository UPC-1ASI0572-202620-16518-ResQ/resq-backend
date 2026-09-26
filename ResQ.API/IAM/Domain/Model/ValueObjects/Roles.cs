namespace ResQ.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     Defines the possible roles for an application user.
/// </summary>
public enum Roles
{
    /// <summary>
    ///     Default role for regular users who report emergencies and request help.
    /// </summary>
    Citizen,

    /// <summary>
    ///     Role for users who provide rescue and emergency response services.
    /// </summary>
    Volunteer
}
