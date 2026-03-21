using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Data.EntityConfigurations
{
    public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
    {
        public void Configure(EntityTypeBuilder<Shipment> builder)
        {
            builder.ToTable("Shipments");

            // Primary Key
            builder.HasKey(s => s.Id);

            // Properties
            builder.Property(s => s.AirlineId)
                .IsRequired();

            builder.Property(s => s.AirwayBillNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.Status)
                .IsRequired();

            builder.Property(s => s.ShipmentDate)
                .IsRequired();

            // BaseEntity Properties
            builder.Property(s => s.CreatedDate)
                .IsRequired();

            builder.Property(s => s.UpdatedDate)
                .IsRequired();

            // Unique Index on AirwayBillNumber and AirlineId
           
            builder.HasIndex(s => new { s.AirlineId, s.AirwayBillNumber })
                .IsUnique();

            // Relationships

            builder.HasMany(s => s.ShipmentNotes)
                .WithOne()
                .HasForeignKey("ShipmentId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.ShipmentDocuments)
                .WithOne()
                .HasForeignKey("ShipmentId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
