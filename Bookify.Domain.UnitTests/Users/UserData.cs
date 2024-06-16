using Bookify.Domain.Users;

namespace Bookify.Domain.UnitTests.Users;

internal static class UserData
{
    public static readonly FirstName FirstName = new("Kent");
    public static readonly LastName LastName = new("Zurich");
    public static readonly Email Email = new("test@test.com");
}