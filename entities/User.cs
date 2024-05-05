using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationApi.Entities;

public class User : IdentityUser
{

    [Required]
    public string? CardNumber { get; set; }
    [Required]
    public string? Expiry { get; set; }
    [Required]
    public int? CVC { get; set; }
    [Required]
    public string? Country { get; set; }
}