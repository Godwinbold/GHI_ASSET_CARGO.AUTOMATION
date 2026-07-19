using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHI_ASSET_CARGO.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteFiltersAndShipmentNoteIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ShipmentNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ShipmentNotes");
        }
    }
}
