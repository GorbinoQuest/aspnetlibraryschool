using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Library.Models;

namespace Library.Data
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Display(Name = "Vardas")]
        public string FirstName {get;set;}
        [PersonalData]
        [Display(Name = "Pavardė")]
        public string LastName {get;set;}
        [Display(Name = "Grupė")]
        [PersonalData]
        public GroupModel? Group {get;set;}
    
        //Storing a randomly generated password here until self-service gets implemented in the future.
        public string? TempPassword{get;set;}

        public virtual ICollection<BorrowingEntryModel> BorrowedBooks { get; set; } = new List<BorrowingEntryModel>();
        
        [Display(Name = "Vardas")]
        public string FullName {
            get{
                return $"{FirstName} {LastName}";
            }
        }
        [Display(Name = "El. Paštas")]
        public string EmailAddress {
            get{
                return Email;
            }}
        
        [Display(Name = "Tel. Nr.")]
        public string PhoneNum {
            get{
                return PhoneNumber;
        }}
    }
}
