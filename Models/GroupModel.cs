using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Library.Data;

namespace Library.Models;


public class GroupModel
{
    public int Id {get;set;}
    [Required]
    [Display(Name = "Pavadinimas")]
    public string Name {get;set;}
    [DataType(DataType.Date)]
    [Display(Name = "Baigimo data")]
    public DateTime? EndDate {get;set;}
    [Display(Name = "Grupės Mokytojas")]
    public string? GroupTeacherName {get;set;}

    public ICollection<ApplicationUser> Members {get;set;} = new List<ApplicationUser>();
}
