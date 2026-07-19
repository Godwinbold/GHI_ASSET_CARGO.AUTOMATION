using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GHI_ASSET_CARGO.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Airline> Airlines { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentNote> ShipmentNotes { get; set; }
        public DbSet<Financial> Financials { get; set; }
        public DbSet<ShipmentDocument> ShipmentDocuments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<DeletedEntity> DeletedEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
