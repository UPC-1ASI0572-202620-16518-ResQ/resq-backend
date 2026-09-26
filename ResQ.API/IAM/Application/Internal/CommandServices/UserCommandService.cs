using ResQ.API.IAM.Application.Internal.OutboundServices;
using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Domain.Model.Commands;
using ResQ.API.IAM.Domain.Repositories;
using ResQ.API.IAM.Domain.Services;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.IAM.Application.Internal.CommandServices;

/// <summary>
///     Handles user-related commands such as sign-in and sign-up.
/// </summary>
public class UserCommandService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IHashingService hashingService,
    IUnitOfWork unitOfWork)
    : IUserCommandService
{
    /// <summary>
    ///     Authenticate a user using the provided credentials.
    /// </summary>
    /// <param name="command">The sign-in command containing username and password.</param>
    /// <returns>A tuple with the authenticated <see cref="User" /> and the generated JWT token.</returns>
    /// <exception cref="Exception">Thrown when credentials are invalid.</exception>
    public async Task<(User user, string token)> Handle(SignInCommand command)
    {
        var user = await userRepository.FindByUsernameAsync(command.Username);

        if (user == null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            throw new Exception("Invalid username or password");

        var token = tokenService.GenerateToken(user);

        return (user, token);
    }

    /// <summary>
    ///     Create a new user account.
    /// </summary>
    /// <param name="command">The sign-up command with username, password, and role.</param>
    /// <returns>A completed <see cref="Task" /> when the operation succeeds.</returns>
    /// <exception cref="Exception">Thrown when the username is already taken or creation fails.</exception>
    public async Task<int> Handle(SignUpCommand command)
    {
        if (userRepository.ExistsByUsername(command.Username))
            throw new Exception($"Username {command.Username} is already taken");

        var hashedPassword = hashingService.HashPassword(command.Password);

        var user = new User(command.Username, hashedPassword, command.Role);
        try
        {
            await userRepository.AddAsync(user);
            await unitOfWork.CompleteAsync();
            return user.Id;
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while creating user: {e.Message}");
        }
    }
}
