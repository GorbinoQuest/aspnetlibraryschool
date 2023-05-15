using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Library.Models;

public class EmailPasswordPair
{
    [Display(Name = "El. Paštas")]
    public string Email {get;set;}
    [Display(Name = "Slaptažodis")]
    public string Password {get;set;}
}
