using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHI_ASSET_CARGO.Data.EntityConfigurations
{
    public class ShipmentDocumentConfiguration : IEntityTypeConfiguration<ShipmentDocument>
    {
        public void Configure(EntityTypeBuilder<ShipmentDocument> builder)
        {
            builder.ToTable("ShipmentDocuments");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.AirwayBillNumber)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(d => d.ShipmentDate)
                   .IsRequired();

            builder.Property(d => d.FileName)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(d => d.ContentType)
                   .HasMaxLength(100);

            builder.Property(d => d.FileSizeBytes);

            builder.Property(d => d.StoragePath)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(d => d.UploadedAt)
                   .IsRequired();

            builder.Property(d => d.UploadedByUserId)
                   .IsRequired();

            builder.HasOne(d => d.Shipment)
                   .WithMany(s => s.ShipmentDocuments)
                   .HasForeignKey(d => d.ShipmentId)
                   .OnDelete(DeleteBehavior.Cascade);

            // BaseEntity fields
            builder.Property(d => d.CreatedDate).IsRequired();
            builder.Property(d => d.UpdatedDate).IsRequired();
        }
    }
}