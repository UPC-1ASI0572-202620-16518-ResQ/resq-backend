using System.Text.RegularExpressions;
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
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid.</exception>
    public async Task<(User user, string token)> Handle(SignInCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrEmpty(command.Password))
            throw new UnauthorizedAccessException("Invalid username or password");

        var user = await userRepository.FindByUsernameAsync(command.Username.Trim());

        if (user == null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password");

        var token = tokenService.GenerateToken(user);

        return (user, token);
    }

    /// <summary>
    ///     Create a new user account.
    /// </summary>
    /// <param name="command">The sign-up command with username, password, and role.</param>
    /// <returns>A completed <see cref="Task" /> when the operation succeeds.</returns>
    /// <exception cref="ArgumentException">Thrown when the sign-up data is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the username is already taken.</exception>
    /// <exception cref="Exception">Thrown when creation fails.</exception>
    public async Task<int> Handle(SignUpCommand command)
    {
        // Validated before saving so a failed profile creation never leaves a user without profile
        ValidateSignUp(command);

        var username = command.Username.Trim();

        if (userRepository.ExistsByUsername(username))
            throw new InvalidOperationException($"Username {username} is already taken");

        var hashedPassword = hashingService.HashPassword(command.Password);

        var user = new User(username, hashedPassword, command.Role);
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

    /// <summary>
    ///     Validates the sign-up data, including the profile data that is created right after the user.
    /// </summary>
    /// <param name="command">The sign-up command to validate.</param>
    /// <exception cref="ArgumentException">Thrown when a field is missing or invalid.</exception>
    private static void ValidateSignUp(SignUpCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Username))
            throw new ArgumentException("Username is required.");

        if (command.Username.Trim().Length > 50)
            throw new ArgumentException("Username cannot exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(command.Password))
            throw new ArgumentException("Password is required.");

        if (!Enum.IsDefined(command.Role))
            throw new ArgumentException("Invalid role. Allowed values: citizen, volunteer.");

        if (string.IsNullOrWhiteSpace(command.FirstName) || command.FirstName.Trim().Length > 50)
            throw new ArgumentException("First name is required and cannot exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(command.LastName) || command.LastName.Trim().Length > 50)
            throw new ArgumentException("Last name is required and cannot exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(command.Email) || command.Email.Trim().Length > 150 ||
            !EmailPattern.IsMatch(command.Email.Trim()))
            throw new ArgumentException("Email must be a valid email address of up to 150 characters.");
    }

    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
}
