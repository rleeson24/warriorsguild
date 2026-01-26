using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WarriorsGuild.Data.Models;


namespace WarriorsGuild.Models.Account
{
    // Models returned by MeController actions.
    public class MeViewModel
    {
        public UserProfile? Me { get; set; }
        public UserProfile[] ChildUsers { get; internal set; } = Array.Empty<UserProfile>();
    }


    public class UserProfile
    {
        public string Email { get; set; } = default!;
        [Display( Name = "First Name" )]
        public string FirstName { get; set; } = default!;
        [Display( Name = "Last Name" )]
        public string LastName { get; set; } = default!;
        [Display( Name = "Address" )]
        public string AddressLine1 { get; set; } = default!;
        [Display( Name = "Address" )]
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = default!;
        public string State { get; set; } = default!;
        [Display( Name = "Postal Code" )]
        public string PostalCode { get; set; }
        [Display( Name = "Phone Number" )]
        public string? PhoneNumber { get; set; }
        [Display( Name = "Shirt Size" )]
        public string ShirtSize { get; set; } = default!;

        [Display( Name = "Favorite Verse" )]
        public string? FavoriteVerse { get; set; }
        [Display( Name = "Hobbies" )]
        public string? Hobbies { get; set; }
        [Display( Name = "Interesting Fact" )]
        public string? InterestingFact { get; set; }
        [Display( Name = "Favorite Movie" )]
        public string? FavoriteMovie { get; set; }

        [Display( Name = "Confirm Email" )]
        public string? EmailConfirmed { get; set; }

        [Display( Name = "Enable Two-factor Sign-in" )]
        public bool TwoFactorEnabled { get; set; }
        public string? Avatar { get; set; }

        public string? AvatarContentType { get; set; }

        public string Id { get; set; } = default!;

        [Display( Name = "User Name" )]
        public string UserName { get; set; } = default!;

        public IEnumerable<SelectListItem> StateOptions
        {
            get
            {
                return ClientOptions.StateOptions;
            }
        }

        public IEnumerable<SelectListItem> ShirtSizeOptions
        {
            get
            {
                return ClientOptions.ShirtSizeOptions;
            }
        }

        public DateTime BirthDate { get; set; }
    }


}