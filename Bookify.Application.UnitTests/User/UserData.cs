using Bookify.Domain.Users;
using Users = Bookify.Domain.Users.User;

namespace Bookify.Application.UnitTests.User;

public class UserData
{
    public static Users Create() => Users.Create(FirstName, LastName, Email);

    public static readonly FirstName FirstName = new("Kent");
    public static readonly LastName LastName = new("Zurich");
    public static readonly Email Email = new("test@test.com");
}
