using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Data;
using GHI_ASSET_CARGO.Domain.Constants;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq;


namespace GHI_ASSET_CARGO.Infrastructure
{
    public static class DataSeeder
    {

        public static async Task SeedAsync(AppDbContext context, IUnitOfWork unitOfWork, RoleManager<IdentityRole<Guid>> roleManager, UserManager<AppUser> userManager, ILogger logger)
        {
            await SeedAirlines(context, unitOfWork, logger);

            await SeedRoles(roleManager, logger);

            await SeedUsers(userManager, roleManager, logger);
        }

        private static async Task SeedAirlines(AppDbContext context, IUnitOfWork unitOfWork, ILogger logger)
        {
            var airlinesToSeed = new List<Airline>
            {
                new Airline { AirlineName = "Turkish Airline" },
                new Airline { AirlineName = "United Cargo" },
                new Airline { AirlineName = "RwandAir" },
                new Airline { AirlineName = "South African Airways" },
                new Airline { AirlineName = "Air Cote D'Voire" }
            };

            var existingAirlines = await context.Airlines
                .Select(a => a.AirlineName)
                .ToListAsync();

            var newAirlines = airlinesToSeed
                .Where(a => !existingAirlines.Contains(a.AirlineName))
                .ToList();

            if (newAirlines.Any())
            {
                await context.Airlines.AddRangeAsync(newAirlines);
                await unitOfWork.SaveChangesAsync();
                logger.LogInformation("Seeded airlines: {Airlines}", string.Join(", ", newAirlines.Select(a => a.AirlineName)));
            }
            else
            {
                logger.LogInformation("No new airlines to seed");
            }
        }

        private static async Task SeedRoles(RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
        {
            var roles = new List<string>
            {
                RolesConstant.User,
                RolesConstant.Admin,
                RolesConstant.Executive
            };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
                    if (!result.Succeeded)
                    {
                        logger.LogError("Failed to create role {Role}: {Errors}", roleName, string.Join(';', result.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        logger.LogInformation("Created role {Role}", roleName);
                    }
                }
                else
                {
                    logger.LogInformation("Role {Role} already exists", roleName);
                }
            }
        }

        private static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
        {
            // Admin
            await EnsureUser(userManager, "admin@ghi.com", "Admin@1234", RolesConstant.Admin, "System", "Administrator", logger);

            // Executive
            await EnsureUser(userManager, "executive@ghi.com", "Executive@1234", RolesConstant.Executive, "System", "Executive", logger);

            logger.LogInformation("User seeding completed");
        }

        private static async Task EnsureUser(UserManager<AppUser> userManager, string email, string password, string role, string firstName, string lastName, ILogger logger)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                logger.LogInformation("User {Email} already exists", email);
                return;
            }

            var user = new AppUser
            {
                FirstName = firstName,
                LastName = lastName,
                MiddleName = string.Empty,
                Email = email,
                UserName = email,
                PhoneNumber = string.Empty,
                CreatedDate = DateTimeOffset.UtcNow,
                UpdatedDate = DateTimeOffset.UtcNow,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (createResult.Succeeded)
            {
                var addRoleResult = await userManager.AddToRoleAsync(user, role);
                if (!addRoleResult.Succeeded)
                {
                    logger.LogError("Failed to add user {Email} to role {Role}: {Errors}", email, role, string.Join(';', addRoleResult.Errors.Select(e => e.Description)));
                }
                else
                {
                    logger.LogInformation("Created user {Email} and added to role {Role}", email, role);
                }
            }
            else
            {
                logger.LogError("Failed to create user {Email}: {Errors}", email, string.Join(';', createResult.Errors.Select(e => e.Description)));
            }
        }
    }
    }

   
