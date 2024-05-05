using System.Text;
using AuthenticationApi.Db;
using AuthenticationApi.Entities;
using AuthenticationApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;

StripeConfiguration.ApiKey = "sk_test_51PD97RLN58NtFWNdTmyErOxpDuF2LZu5BQYOqrF9lxIEvLaBd6BocDHZbXBR3L62FOuEQu34EpjTCrZxAc4l8r9I00T7tVxThD";


// Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY", EnvironmentVariableTarget.Machine);


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
builder.Services.Configure<StripeOptions>(options =>
{
    options.PublishableKey = "pk_test_51PD97RLN58NtFWNdKMuYpZtFfeC5XO5TbiqFzra1aVEaJwGvyqC5uB48eLREnirnppIiRwNHgrk2yDQrJ4835bdB00CqX7JenA";
    
    // Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY", EnvironmentVariableTarget.Machine);
    options.SecretKey = "sk_test_51PD97RLN58NtFWNdTmyErOxpDuF2LZu5BQYOqrF9lxIEvLaBd6BocDHZbXBR3L62FOuEQu34EpjTCrZxAc4l8r9I00T7tVxThD";
    // Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY", EnvironmentVariableTarget.Machine);
    options.WebhookSecret = "whsec_7c7f70bf90d077e44d6d59d6fd89761d4a8ccc07ebdba9f1b9175144e6a09663";
    // Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET", EnvironmentVariableTarget.Machine);
});

// 1. DbContext
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("db")));

// 2. Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 3. Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

// 4. Adding Jwt Bearer
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = configuration["JWT:ValidAudience"],
            ValidIssuer = configuration["JWT:ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
        };
    });
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// 5. Swagger authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Wedding Planner API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,

            },
            new List<string>()
        }
    });
});

// 6. Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCorsPolicy",
        b =>
        {
            b
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseStaticFiles(new StaticFileOptions()
// {
//     FileProvider = new PhysicalFileProvider(
//         Path.Combine(Directory.GetCurrentDirectory(),
//         "127.0.0.1:3000")
//     ),
//     RequestPath = new PathString("")
// });


//7. Use CORS
app.UseCors("ApiCorsPolicy");
app.UseHttpsRedirection();

// 8. Authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();