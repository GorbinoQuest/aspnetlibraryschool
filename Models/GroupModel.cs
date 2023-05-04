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
    public string Name {get;set;}
    [DataType(DataType.Date)]
    public DateTime? EndDate {get;set;}
    public string? GroupTeacherName {get;set;}

    public ICollection<ApplicationUser> Members {get;set;} = new List<ApplicationUser>();
}
