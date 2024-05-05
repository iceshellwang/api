using System.ComponentModel.DataAnnotations;

namespace AuthenticationApi.Dtos;

public class RegisterRequest
{
    [Required]
    public string? Username { get; set; }
    [Required]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }
    [Required]
    public string? CardNumber { get; set; }
    [Required]
    public string? Expiry { get; set; }
    [Required]
    public int? CVC { get; set; }
    [Required]
    public string? Country { get; set; }
}