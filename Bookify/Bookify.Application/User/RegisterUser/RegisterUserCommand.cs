using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.User.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password) : ICommand<Guid>;
