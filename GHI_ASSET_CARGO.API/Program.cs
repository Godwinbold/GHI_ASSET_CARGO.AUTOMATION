using GHI_ASSET_CARGO.API.Extensions;
using GHI_ASSET_CARGO.API.Middlewares;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Data;
using GHI_ASSET_CARGO.Infrastructure;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddServices(builder.Configuration); 
builder.Services.AddDbServices(builder.Configuration);

var key = Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    //options.Events = new JwtBearerEvents
    //{
    //    OnAuthenticationFailed = context =>
    //    {
    //        Console.WriteLine("AUTH FAILED: " + context.Exception?.Message);
    //        return Task.CompletedTask;
    //    },
    //    OnTokenValidated = context =>
    //    {
    //        Console.WriteLine("TOKEN VALIDATED");
    //        return Task.CompletedTask;
    //    },
    //    OnChallenge = context =>
    //    {
    //        Console.WriteLine("AUTH CHALLENGE");
    //        return Task.CompletedTask;
    //    }
    //};

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

 builder.Services
            .AddHttpClient()
            .AddHttpContextAccessor()
            .AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();
    var unitOfWork = services.GetRequiredService<IUnitOfWork>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var userSeederLogger = services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger("UserSeeder");

    // Apply migrations
    context.Database.Migrate();

    // Seed data
    await DataSeeder.SeedAsync(context, unitOfWork, roleManager, userManager, userSeederLogger);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();

app.Run();