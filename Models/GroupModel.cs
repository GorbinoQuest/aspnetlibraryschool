using Library.Data;

namespace Library.Models;


public class GroupModel
{
    public int Id {get;set;}
    public string Name {get;set;}
    public string EndDate {get;set;}
    public string GroupTeacherName {get;set;}

    public ICollection<ApplicationUser> Members {get;set;} = new List<ApplicationUser>();
}
