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
    public BookModel Book {get;set;}
    
    [Required]
    public ApplicationUser User {get;set;}
    
    [Required]
    public DateTime WhenBorrowed {get;set;} = DateTime.Now;
    
    public DateTime? WhenReturned {get;set;} = null;
   
    //optional comment on the state of the book
    public string? Comment {get;set;}

}
