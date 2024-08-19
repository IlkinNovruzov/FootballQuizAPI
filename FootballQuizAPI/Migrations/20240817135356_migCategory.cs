using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballQuizAPI.Migrations
{
    /// <inheritdoc />
    public partial class migCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnlockLevels",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{\"easy\":1, \"medium\":6, \"hard\":11, \"medium\":16}");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "80c86dde-0cf9-42de-95b5-ecc3b7b8b028", "AQAAAAIAAYagAAAAEDj2sKu42oIfHIE+Le3MVZzM2AJ2Cagc55YN7K6UU7i/ZBb1JLCMyACly5lx9Q66mw==", "4d5380f6-6ca7-4a9f-b954-919a38a9dc8e" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnlockLevels",
                table: "Categories");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "196e33e2-b555-4b4b-abb4-145aea103221", "AQAAAAIAAYagAAAAEMZthrwz0L2R2ukRB86cneDOVBichLOq4QgrPYFTh8kmQhBZtbL/YIKf+DPAKlJmhQ==", "8b3813c8-a8cd-4178-b78d-df1a5c8bd5ea" });
        }
    }
}
