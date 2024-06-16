using Bookify.Api.Controllers.Users;

namespace Bookify.Api.FunctionalTests.User;

internal static class UserData
{
    public static RegisterUserRequest RegisterUserRequest = new("test@test.com", "test", "test", "12345");
}
