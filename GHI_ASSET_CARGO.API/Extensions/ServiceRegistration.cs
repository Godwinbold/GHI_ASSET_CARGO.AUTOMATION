using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Services;
using GHI_ASSET_CARGO.Infrastructure;
using GHI_ASSET_CARGO.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace GHI_ASSET_CARGO.API.Extensions
{
    public static class ServiceRegistration
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter: Bearer {your JWT token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Scheme = "bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });


            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //}).AddJwtBearer(options =>
            //{
            //    var key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]!);

            //    options.MapInboundClaims = false;

            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuerSigningKey = true,
            //        IssuerSigningKey = new SymmetricSecurityKey(key),
            //        ValidateLifetime = true,
            //        ValidateAudience = false,
            //        ValidateIssuer = false,
            //        ClockSkew = TimeSpan.Zero
            //    };
            //});

            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //})
            //.AddJwtBearer(options =>
            //{
            //    var key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]!);

            //    options.Events = new JwtBearerEvents
            //    {
            //        OnAuthenticationFailed = context =>
            //        {
            //            Console.WriteLine("AUTH FAILED: " + context.Exception.Message);
            //            return Task.CompletedTask;
            //        },
            //        OnTokenValidated = context =>
            //        {
            //            Console.WriteLine("TOKEN VALIDATED");
            //            return Task.CompletedTask;
            //        },
            //        OnChallenge = context =>
            //        {
            //            Console.WriteLine("AUTH CHALLENGE");
            //            return Task.CompletedTask;
            //        }
            //    };

            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuerSigningKey = true,
            //        IssuerSigningKey = new SymmetricSecurityKey(key),
            //        ValidateIssuer = false,
            //        ValidateAudience = false,
            //        ValidateLifetime = true,
            //        ClockSkew = TimeSpan.Zero
            //    };
            //});

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IShipmentService, ShipmentService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IFinancialService, FinancialService>();
        }
    }
}
