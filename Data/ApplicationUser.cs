using Microsoft.AspNetCore.Identity;
using Library.Models;

namespace Library.Data
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FirstName {get;set;}
        [PersonalData]
        public string LastName {get;set;}
        [PersonalData]
        public GroupModel? Group {get;set;}
    
        //Storing a randomly generated password here until self-service gets implemented in the future.
        public string TempPassword{get;set;}

        public virtual ICollection<BorrowingEntryModel> BorrowedBooks { get; set; } = new List<BorrowingEntryModel>();

        public string FullName {
            get{
                return $"{FirstName} {LastName}";
            }
        }
    }
}
