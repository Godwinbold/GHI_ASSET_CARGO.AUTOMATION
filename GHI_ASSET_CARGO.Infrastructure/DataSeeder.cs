using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Data;
using GHI_ASSET_CARGO.Domain.Constants;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace GHI_ASSET_CARGO.Infrastructure
{
    public static class DataSeeder
    {

        public static async Task SeedAsync(AppDbContext context, IUnitOfWork unitOfWork, RoleManager<IdentityRole<Guid>> roleManager)
        {
            await SeedAirlines(context, unitOfWork);
            await SeedRoles(roleManager);
        }

        private static async Task SeedAirlines(AppDbContext context, IUnitOfWork unitOfWork)
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
            }
        }

        private static async Task SeedRoles(RoleManager<IdentityRole<Guid>> roleManager)
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
                            await roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
                        }
                    }
        }

    }
    }

   
