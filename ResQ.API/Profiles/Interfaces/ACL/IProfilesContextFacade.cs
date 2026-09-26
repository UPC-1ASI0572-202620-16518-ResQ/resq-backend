namespace ResQ.API.Profiles.Interfaces.ACL;

public interface IProfilesContextFacade
{
    Task<int> CreateUserProfile(int userId, string firstName, string lastName, string email);
}
