using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Library.Models;


public class BookModel
{
    //unique ID of the book
    public int Id {get;set;}
    //book's metadata
    [Required]
    public string Title {get;set;}
    [Required]
    public string BookAuthor {get;set;}
    public string? ReleaseDate {get;set;}
    [Required]
    public string Genre {get;set;}
    
    //is the book in the library or being borrowed. Defaults to true.
    public bool IsAvailable {get;set;} = true;

    public virtual ICollection<BorrowingEntryModel> Borrowings {get;set;} = new List<BorrowingEntryModel>();


}
