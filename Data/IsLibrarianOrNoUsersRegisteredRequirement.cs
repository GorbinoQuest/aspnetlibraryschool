using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security;
using System;

namespace Library.Data;

public class IsLibrarianOrNoUsersRegisteredRequirement : IAuthorizationRequirement { }

public class IsLibrarianOrNoUsersRegisteredAuthorizationHandler : AuthorizationHandler<IsLibrarianOrNoUsersRegisteredRequirement>
{
    private readonly ApplicationDbContext _context;

    public IsLibrarianOrNoUsersRegisteredAuthorizationHandler(ApplicationDbContext dbcontext)
    {
        _context = dbcontext;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsLibrarianOrNoUsersRegisteredRequirement requirement)
    {
        var userCount = _context.Users.Count();

        if (userCount == 0)
        {
            Console.WriteLine("0 users succeeded");
            context.Succeed(requirement);
        }

        if (context.User.HasClaim("Role", "Librarian"))
        {
            Console.WriteLine("HasClaim succeeded");
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
