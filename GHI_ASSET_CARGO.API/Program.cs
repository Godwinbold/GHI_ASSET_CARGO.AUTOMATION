using GHI_ASSET_CARGO.API.Extensions;
using GHI_ASSET_CARGO.API.Middlewares;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Data;
using GHI_ASSET_CARGO.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddServices(builder.Configuration);
builder.Services.AddDbServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();
    var unitOfWork = services.GetRequiredService<IUnitOfWork>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    await context.Database.MigrateAsync();

    await DataSeeder.SeedAsync(context, unitOfWork, roleManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
