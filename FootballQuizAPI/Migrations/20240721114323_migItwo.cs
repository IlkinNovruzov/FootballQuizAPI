using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballQuizAPI.Migrations
{
    /// <inheritdoc />
    public partial class migItwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte>(
                name: "Chest",
                table: "AspNetUsers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "ExtraHeart",
                table: "AspNetUsers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Heart",
                table: "AspNetUsers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)5);

            migrationBuilder.AddColumn<byte>(
                name: "Hint",
                table: "AspNetUsers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)10);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Chest", "ConcurrencyStamp", "ExtraHeart", "Heart", "Hint", "PasswordHash", "SecurityStamp" },
                values: new object[] { (byte)0, "b2a4225a-4986-4e8c-b37a-ff4e9b3cba1f", (byte)25, (byte)5, (byte)10, "AQAAAAIAAYagAAAAENe1NokD7C0APEgE4K7pvw9XtVQAbrNcJB2E/Cg/In6J0UTF91oyEhZvXnUU2RycfQ==", "513253b0-f8e3-444d-a900-95f82b36c032" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Chest",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ExtraHeart",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Heart",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Hint",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "58bf5bf5-f59e-46ba-b92d-abf91d71f6b3", "AQAAAAIAAYagAAAAEKwf+lkWR1j9q2FNkhWD6+yrMvG7hlf2/mY1fH0ETSw+RINGj18yiqSG0Bhzf8bmfw==", "37e0e72d-a878-4285-8c68-ad0a29d44f9f" });
        }
    }
}
