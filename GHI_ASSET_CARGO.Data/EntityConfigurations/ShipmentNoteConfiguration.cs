using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHI_ASSET_CARGO.Data.EntityConfigurations
{
    public class ShipmentNoteConfiguration : IEntityTypeConfiguration<ShipmentNote>
    {
        public void Configure(EntityTypeBuilder<ShipmentNote> builder)
        {
            builder.ToTable("ShipmentNotes");

            // Primary Key
            builder.HasKey(sn => sn.Id);

            // Properties
            builder.Property(sn => sn.Content)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(sn => sn.ShipmentId)
                   .IsRequired();

            // BaseEntity Properties
            builder.Property(sn => sn.CreatedDate)
                   .IsRequired();

            builder.Property(sn => sn.UpdatedDate)
                   .IsRequired();

            // Relationship
            builder.HasOne(sn => sn.Shipment)
                   .WithMany(s => s.ShipmentNotes)
                   .HasForeignKey(sn => sn.ShipmentId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
