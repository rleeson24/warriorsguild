using WarriorsGuild.Models.Account;

namespace WarriorsGuild.Models
{
    public class EditProfileViewModel
    {
        public UserProfile User { get; set; } = default!;
        public EditProfileUrls? Urls { get; set; }
        public string? StripePublishableKey { get; set; }
    }

    public class EditProfileUrls
    {
        public string? GetProfile { get; internal set; } = "/api/Profile";

        public string? PaymentMethods { get; internal set; } = "/api/PaymentMethods";
        public string? AvatarUploadUrl { get; internal set; } = "/api/Profile/UploadAvatar";
        public string? GetAvatarUrl { get; internal set; }
    }
}