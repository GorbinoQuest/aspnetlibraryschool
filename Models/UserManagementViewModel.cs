using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Library.Data;

namespace Library.Models;

public class UserManagementViewModel
{
    [Display(Name = "Vartotojas")]
    public ApplicationUser User {get;set;}
    [Display(Name = "Rolė")]
    public string RoleValue {get;set;}
}
