using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballQuizAPI.Migrations
{
    /// <inheritdoc />
    public partial class mig2222 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "QuizResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "196e33e2-b555-4b4b-abb4-145aea103221", "AQAAAAIAAYagAAAAEMZthrwz0L2R2ukRB86cneDOVBichLOq4QgrPYFTh8kmQhBZtbL/YIKf+DPAKlJmhQ==", "8b3813c8-a8cd-4178-b78d-df1a5c8bd5ea" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "QuizResults");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "23ae9a14-084a-44eb-8dfe-6c510bbd6357", "AQAAAAIAAYagAAAAEAiixl3eqjNY8qLC8gCzQzkoU5VjPrSykrR5TQiJUVBt2nSTERXLm8g7KYv/8hDd7Q==", "bc2d81ec-a05a-4c9d-bcd9-5dd0b312b73f" });
        }
    }
}
