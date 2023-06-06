using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Library.Models;

//THIS MODEL IS USED FOR IMPORTING USERS INTO GROUP, NOT FOR REGISTERING.
public class UserModel
{
    [Required]
    [Display(Name = "Vardas")]
    public string FirstName {get;set;}

    [Required]
    [Display(Name = "Pavardė")]
    public string LastName {get;set;}

    [Display(Name = "El. Paštas")]
    public string? Email {get;set;}

    [Display(Name = "Tel. Nr.")]
    public string? PhoneNumber {get;set;}

}
