using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballQuizAPI.Migrations
{
    /// <inheritdoc />
    public partial class mig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "Choices");

            migrationBuilder.RenameColumn(
                name: "Answer",
                table: "Choices",
                newName: "Text");

            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "23ae9a14-084a-44eb-8dfe-6c510bbd6357", "AQAAAAIAAYagAAAAEAiixl3eqjNY8qLC8gCzQzkoU5VjPrSykrR5TQiJUVBt2nSTERXLm8g7KYv/8hDd7Q==", "bc2d81ec-a05a-4c9d-bcd9-5dd0b312b73f" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answer",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Choices",
                newName: "Answer");

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "Choices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b2a4225a-4986-4e8c-b37a-ff4e9b3cba1f", "AQAAAAIAAYagAAAAENe1NokD7C0APEgE4K7pvw9XtVQAbrNcJB2E/Cg/In6J0UTF91oyEhZvXnUU2RycfQ==", "513253b0-f8e3-444d-a900-95f82b36c032" });
        }
    }
}
