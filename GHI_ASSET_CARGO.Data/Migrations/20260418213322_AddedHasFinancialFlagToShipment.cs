using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHI_ASSET_CARGO.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedHasFinancialFlagToShipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Financials_ShipmentId",
                table: "Financials");

            migrationBuilder.AddColumn<bool>(
                name: "HasFinancial",
                table: "Shipments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Financials_ShipmentId",
                table: "Financials",
                column: "ShipmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Financials_ShipmentId",
                table: "Financials");

            migrationBuilder.DropColumn(
                name: "HasFinancial",
                table: "Shipments");

            migrationBuilder.CreateIndex(
                name: "IX_Financials_ShipmentId",
                table: "Financials",
                column: "ShipmentId");
        }
    }
}
