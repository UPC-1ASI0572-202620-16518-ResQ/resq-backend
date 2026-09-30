using ResQ.API.Profiles.Domain.Model.ValueObjects;

namespace ResQ.API.Profiles.Domain.Model.Aggregates;

/// <summary>
///     Aggregate Root that represents a User Profile.
///     The Id of this aggregate is tightly coupled with the IAM User Id.
/// </summary>
public class UserProfile
{
    /// <summary>
    ///     The unique identifier for the User Profile. It matches the IAM User Id.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    ///     The full name of the user.
    /// </summary>
    public FullName Name { get; private set; }

    /// <summary>
    ///     The contact information of the user.
    /// </summary>
    public ContactInformation ContactInfo { get; private set; }

    /// <summary>
    ///     The preferences of the user.
    /// </summary>
    public UserPreferences Preferences { get; private set; }

    protected UserProfile()
    {
        Name = new FullName();
        ContactInfo = new ContactInformation();
        Preferences = new UserPreferences();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserProfile" /> class.
    /// </summary>
    /// <param name="id">The Id matching the IAM User.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="email">The user's email address.</param>
    public UserProfile(int id, string firstName, string lastName, string email)
    {
        Id = id;
        Name = new FullName(firstName, lastName);
        ContactInfo = new ContactInformation(email, string.Empty);
        Preferences = new UserPreferences();
    }

    /// <summary>
    ///     Updates the contact information for the user profile.
    /// </summary>
    /// <param name="email">The new email address.</param>
    /// <param name="phoneNumber">The new phone number.</param>
    public void UpdateContactInfo(string? email, string? phoneNumber)
    {
        var newEmail = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(email) ? ContactInfo.Email : email!.Trim();
        var newPhone = ResQ.API.Shared.Application.Internal.PartialUpdateHelper.ShouldIgnore(phoneNumber) ? ContactInfo.PhoneNumber : phoneNumber!.Trim();
        ContactInfo = new ContactInformation(newEmail, newPhone);
    }
}
