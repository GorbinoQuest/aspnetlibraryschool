using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Library.Data;

namespace Library.Models;

//Model responsible for keeping track of borrowed books
public class BorrowingEntryModel
{
    public int Id {get;set;}

    [Required]
    [Display(Name = "Knyga")]
    public BookModel Book {get;set;}
    
    [Required]
    [Display(Name = "Vartotojas")]
    public ApplicationUser User {get;set;}
    
    [Required]
    [Display(Name = "Kada Paimta")]
    public DateTime WhenBorrowed {get;set;} = DateTime.Now;
    
    [Display(Name = "Kada Gražinta")]
    public DateTime? WhenReturned {get;set;} = null;
   
    //optional comment on the state of the book
    [Display(Name = "Komentaras")]
    public string? Comment {get;set;}

}
