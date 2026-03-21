using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHI_ASSET_CARGO.Data.EntityConfigurations
{
    public class AirlineConfiguration : IEntityTypeConfiguration<Airline>
    {
        public void Configure(EntityTypeBuilder<Airline> builder)
        {
            builder.ToTable("Airlines"); // match existing table

            builder.HasKey(a => a.Id);

            builder.Property(a => a.AirlineName)
                   .IsRequired()
                   .HasMaxLength(100);

            // Relationships
            builder.HasMany(a => a.Shipments)
                   .WithOne(s => s.Airline)
                   .HasForeignKey(s => s.AirlineId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(a => a.Financials)
                   .WithOne(f => f.Airline)
                   .HasForeignKey(f => f.AirlineId)
                   .OnDelete(DeleteBehavior.Cascade);

            // BaseEntity fields
            builder.Property(a => a.CreatedDate).IsRequired();
            builder.Property(a => a.UpdatedDate).IsRequired();
        }
    }
}
