using Microsoft.AspNetCore.Identity;
using Library.Data;

namespace Library.Models;

public class UserManagementViewModel
{
    public ApplicationUser User {get;set;}
    public string RoleValue {get;set;}
}
