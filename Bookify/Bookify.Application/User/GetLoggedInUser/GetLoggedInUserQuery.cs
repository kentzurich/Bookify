using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.User.GetLoggedInUser;

public sealed record GetLoggedInUserQuery : IQuery<UserResponse>;