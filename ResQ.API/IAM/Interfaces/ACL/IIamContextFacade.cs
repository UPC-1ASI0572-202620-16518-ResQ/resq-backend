using ResQ.API.IAM.Domain.Model.ValueObjects;

namespace ResQ.API.IAM.Interfaces.ACL;

/// <summary>
///     Public facade of the IAM bounded context for other contexts.
/// </summary>
public interface IIamContextFacade
{
    /// <summary>Creates a citizen or volunteer inside an existing organization. Returns the new user id.</summary>
    Task<int> CreateUser(Guid organizationId, string firstName, string lastName, string email, string username,
        string password, Roles role);

    Task<int> FetchUserIdByUsername(string username);

    Task<string> FetchUsernameByUserId(int userId);

    /// <summary>Returns the organization of the user, or null when the user does not exist.</summary>
    Task<Guid?> FetchOrganizationIdByUserId(int userId);
}
