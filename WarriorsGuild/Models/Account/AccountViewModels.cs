using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WarriorsGuild.Data.Models;


namespace WarriorsGuild.Models.Account
{
    public class BaseRegistrationViewModel
    {
        [EmailAddress]
        [Display( Name = "Email" )]
        public string Email { get; set; } = default!;

        [Required]
        [Display( Name = "Username" )]
        public string UserName { get; set; } = default!;

        [Required]
        [Display( Name = "First Name" )]
        public string FirstName { get; set; } = default!;

        [Required]
        [Display( Name = "Last Name" )]
        public string LastName { get; set; } = default!;

        [Required]
        [Display( Name = "Line 1" )]
        public string AddressLine1 { get; set; } = default!;

        [Display( Name = "Line 2" )]
        public string? AddressLine2 { get; set; }

        [Required]
        [Display( Name = "City" )]
        public string City { get; set; } = default!;

        [Required]
        [Display( Name = "State" )]
        public string State { get; set; } = default!;

        [Required]
        [Display( Name = "Postal Code" )]
        public string PostalCode { get; set; } = default!;

        [Required]
        [Display( Name = "Phone Number" )]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display( Name = "Shirt Size" )]
        public string ShirtSize { get; set; }

        [Required]
        [Display( Name = "Birth Date" )]
        [DataType( DataType.Date )]
        [DisplayFormat( DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true )]
        public DateTime BirthDate { get; set; }

        [Display( Name = "Favorite Verse" )]
        public string? FavoriteVerse { get; set; }
        [Display( Name = "Hobbies" )]
        public string? Hobbies { get; set; }
        [Display( Name = "Interesting Fact" )]
        public string? InterestingFact { get; set; }
        [Display( Name = "Favorite Movie" )]
        public string? FavoriteMovie { get; set; }

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

        public virtual string? Password { get; }
        public virtual string? ConfirmPassword { get; }
    }

    // Models returned by AccountController actions.
    public class ExternalLoginConfirmationViewModel : BaseRegistrationViewModel
    {
    }

    public class ExternalLoginFailureModel
    {
        public string Message { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string? ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<SelectListItem> Providers { get; set; }
        public string? ReturnUrl { get; set; }
        public bool? RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; } = default!;

        [Required]
        [Display( Name = "Code" )]
        public string Code { get; set; } = default!;
        public string? ReturnUrl { get; set; }

        [Display( Name = "Remember this browser?" )]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display( Name = "Email" )]
        public string Email { get; set; } = default!;
    }

    //public class LoginViewModel
    //{
    //    [Required]
    //    [Display( Name = "Username" )]
    //    public string? UserName { get; set; }

    //    [Required]
    //    [DataType( DataType.Password )]
    //    [Display( Name = "Password" )]
    //    public string? Password { get; set; }

    //    [Display( Name = "Remember me?" )]
    //    public bool RememberMe { get; set; }
    //}
    //public class LoginInputModel
    //{
    //    [Required]
    //    public string? Username { get; set; }
    //    [Required]
    //    public string? Password { get; set; }
    //    public bool RememberLogin { get; set; }
    //    public string? ReturnUrl { get; set; }
    //}

    //public class LogoutInputModel
    //{
    //    public string? LogoutId { get; set; }
    //}

    //public class LogoutViewModel : LogoutInputModel
    //{
    //    public bool ShowLogoutPrompt { get; set; } = true;
    //}

    public class RegisterViewModel : BaseRegistrationViewModel
    {
        [Required]
        [StringLength( 100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6 )]
        [DataType( DataType.Password )]
        [Display( Name = "Password" )]
        public string Password { get; set; } = default!;

        [DataType( DataType.Password )]
        [Display( Name = "Confirm password" )]
        [Compare( "Password", ErrorMessage = "The password and confirmation password do not match." )]
        public string ConfirmPassword { get; set; } = default!;
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display( Name = "Email" )]
        public string Email { get; set; } = default!;

        [Required]
        [StringLength( 100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6 )]
        [DataType( DataType.Password )]
        [Display( Name = "Password" )]
        public string Password { get; set; } = default!;

        [DataType( DataType.Password )]
        [Display( Name = "Confirm password" )]
        [Compare( "Password", ErrorMessage = "The password and confirmation password do not match." )]
        public string ConfirmPassword { get; set; } = default!;

        public string Token { get; set; } = default!;
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display( Name = "Email" )]
        public string Email { get; set; } = default!;
    }
}
