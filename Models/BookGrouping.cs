using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Library.Models;

public class BookGrouping
{
    [Display(Name = "Pavadinimas")]
    public string Title {get;set;}
    [Display(Name = "Autorius")]
    public string BookAuthor {get;set;}
    [Display(Name = "Leidimo Metai")]
    public string ReleaseDate {get;set;}

    public List<BookModel> Models {get;set;}
}
