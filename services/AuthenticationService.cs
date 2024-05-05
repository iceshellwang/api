using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using AuthenticationApi.Dtos;
using AuthenticationApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationApi.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public AuthenticationService (UserManager<User> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<string> Register(RegisterRequest request)
    {
        var userByEmail = await _userManager.FindByEmailAsync(request.Email);
        var userByUsername = await _userManager.FindByNameAsync(request.Username);
        if (userByEmail is not null || userByUsername is not null)
        {
            throw new ArgumentException($"User with email {request.Email} or username {request.Username} already exists.");
        }

        User user = new()
        {
            Email = request.Email,
            UserName = request.Username,
            CardNumber = request.CardNumber,
            Expiry = request.Expiry,
            CVC = request.CVC,
            Country = request.Country,
            SecurityStamp = Guid.NewGuid().ToString()
        };
    
        var result = await _userManager.CreateAsync(user, request.Password);

        if(!result.Succeeded)
        {
            throw new ArgumentException($"Unable to register user {request.Username} errors: {GetErrorsText(result.Errors)}");
        }
        // send welcome email
        // var smtpClient = new SmtpClient("smtp.gmail.com")
        // {
        //     Port = 587,
        //     Credentials = new NetworkCredential("username", "password"),
        //     EnableSsl = true,
        // };
    
        // // smtpClient.Send("email", "recipient", "subject", "body");
        // var mailMessage = new MailMessage
        // {
        //     From = new MailAddress("email"),
        //     Subject = "subject",
        //     Body = "<h1>Hello</h1>",
        //     IsBodyHtml = true,
        // };
        // mailMessage.To.Add("recipient");
        // smtpClient.Send(mailMessage);

        // end of sending email
        return await Login(new LoginRequest { Email = request.Email, Password = request.Password });
    }

    public async Task<string> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new ArgumentException($"Unable to authenticate user {request.Email}");
        }

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = GetToken(authClaims);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private JwtSecurityToken GetToken(IEnumerable<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

        return token;
    }

    private string GetErrorsText(IEnumerable<IdentityError> errors)
    {
        return string.Join(", ", errors.Select(error => error.Description).ToArray());
    }
}