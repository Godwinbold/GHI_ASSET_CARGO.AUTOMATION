using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHI_ASSET_CARGO.Data.EntityConfigurations
{
    public class FinancialConfiguration : IEntityTypeConfiguration<Financial>
    {
        public void Configure(EntityTypeBuilder<Financial> builder)
        {
            builder.ToTable("Financials");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.MAWB)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(f => f.AgentsOrClients)
                   .HasMaxLength(200);

            builder.Property(f => f.Product)
                   .HasMaxLength(100);

            builder.Property(f => f.Routing)
                   .HasMaxLength(100);

            builder.Property(f => f.FlightNo)
                   .HasMaxLength(20);

            builder.Property(f => f.Pieces);

            builder.Property(f => f.ChargeableWeightKg)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.GrossWeightKg)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.SpotRate)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.PublishedRates)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.ROE)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.FreightAmountNGN)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.NCAACharges5Percent)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.TotalChargeNGN)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.ChargesCollect)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.FuelSurcharge)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.SECSurcharge)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.HandlingSurcharge)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.SurchargeDueAgent)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.AWBFee)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.GSACommissionNGN)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.VATOnCommission)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.AmtDueAirline)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.DueAPGInc)
                   .HasColumnType("decimal(18,2)");

            builder.Property(f => f.DueSLC)
                   .HasColumnType("decimal(18,2)");

            // Relationships
            builder.HasOne(f => f.Airline)
                   .WithMany(a => a.Financials)
                   .HasForeignKey(f => f.AirlineId)
                   .OnDelete(DeleteBehavior.Restrict);

            // BaseEntity fields
            builder.Property(f => f.CreatedDate).IsRequired();
            builder.Property(f => f.UpdatedDate).IsRequired();
        }
    }
}