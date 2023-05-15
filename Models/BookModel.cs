using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Library.Models;


public class BookModel
{
    //unique ID of the book
    public int Id {get;set;}
    
    [Required]
    [Display(Name = "Inventoriaus ID")]
    public string InventoryID {get;set;}

    //book's metadata
    [Required]
    [Display(Name = "Pavadinimas")]
    public string Title {get;set;}

    [Display(Name = "Autorius")]
    public string BookAuthor {get;set;}

    [Display(Name = "Leidimo Metai")]
    public string? ReleaseDate {get;set;}

    [Required]
    [Display(Name = "Skyrius")]
    public string Skyrius {get;set;}

    [Display(Name = "Kaina")]
    public string? Price {get;set;}
    
    //is the book in the library or being borrowed. Defaults to true.
    [Display(Name = "Ar yra bibliotekoje?")]
    public bool IsAvailable {get;set;} = true;

    public virtual ICollection<BorrowingEntryModel> Borrowings {get;set;} = new List<BorrowingEntryModel>();
    [Display(Name = "Būsena")]
    public string Status {
        get{
            if(IsAvailable)
            {
                return $"Laisva";
            }
            else
            {
                return $"Paimta";
            }
        }
    }

}
