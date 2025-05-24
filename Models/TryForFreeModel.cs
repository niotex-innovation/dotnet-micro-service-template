namespace PROJECT_TEMPLATE.Models;
using Swashbuckle.AspNetCore.Annotations;

public class TryForFreeModel
{
    [SwaggerSchema("User's first name")]
    public string FirstName { get; set; }

    [SwaggerSchema("User's last name")]
    public string LastName { get; set; }

    [SwaggerSchema("User's phone number")]
    public string PhoneNumber { get; set; }

    [SwaggerSchema("User's email address")]
    public string Email { get; set; }

    [SwaggerSchema("Size of the user's firm")]
    public string FirmSize { get; set; }

    [SwaggerSchema("Referral code if available")]
    public string ReferralCode { get; set; }

    [SwaggerSchema("Promotional code if available")]
    public string PromotionalCode { get; set; }
}