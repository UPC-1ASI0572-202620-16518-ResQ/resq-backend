namespace ResQ.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     Roles available in the platform.
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
    Volunteer,

    /// <summary>
    ///     Administrator of an organization: manages buildings, devices, rules, subscriptions and members.
    ///     Assigned to the user who creates the organization at sign-up.
    /// </summary>
    Admin
}
