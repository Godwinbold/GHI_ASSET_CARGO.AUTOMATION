using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Services;
using GHI_ASSET_CARGO.Domain.Options;
using GHI_ASSET_CARGO.Infrastructure;
using GHI_ASSET_CARGO.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
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

            services.Configure<PostMarkOptions>(configuration.GetSection(PostMarkOptions.SectionName))
            .AddSingleton(x => x.GetRequiredService<IOptions<PostMarkOptions>>().Value);

            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IShipmentService, ShipmentService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IFinancialService, FinancialService>();
            services.AddScoped<IExecutiveService, ExecutiveService>();
            services.AddScoped<IMailSenderService, MailSenderService>();
        }
    }
}
