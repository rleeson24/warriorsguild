using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WarriorsGuild.Data.Models.Account;

namespace WarriorsGuild.Data.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Display( Name = "First Name" )]
        public string FirstName { get; set; }

        [Display( Name = "Last Name" )]
        public string LastName { get; set; }

        [Display( Name = "Line 1" )]
        public string AddressLine1 { get; set; }

        [Display( Name = "Line 2" )]
        public string AddressLine2 { get; set; }

        [Display( Name = "City" )]
        public string City { get; set; }

        [Display( Name = "State" )]
        public string State { get; set; }

        [Display( Name = "Postal Code" )]
        public string PostalCode { get; set; }

        [Display( Name = "Phone Number" )]
        public override string PhoneNumber { get; set; }

        [Display( Name = "Shirt Size" )]
        public string ShirtSize { get; set; }

        [Display( Name = "Favorite Verse" )]
        public string FavoriteVerse { get; set; }
        [Display( Name = "Hobbies" )]
        public string Hobbies { get; set; }
        [Display( Name = "Interesting Fact" )]
        public string InterestingFact { get; set; }
        [Display( Name = "Favorite Movie" )]
        public string FavoriteMovie { get; set; }
        [Display( Name = "Birth Date" )]
        public DateTime BirthDate { get; set; }

        public virtual ICollection<ApplicationUser> ChildUsers { get; set; } = new List<ApplicationUser>();

        public string StripeCustomerId { get; set; }
        public AvatarDetail Avatar { get; set; }

        //public async Task<ClaimsIdentity> GenerateUserIdentityAsync( UserManager<ApplicationUser> manager )
        //{
        //    // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        //    var userIdentity = await manager.CreateIdentityAsync( this, DefaultAuthenticationTypes.ApplicationCookie );
        //    // Add custom user claims here
        //    return userIdentity;
        //}
    }
}