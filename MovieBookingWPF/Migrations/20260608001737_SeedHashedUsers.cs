using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MovieBookingWPF.Migrations
{
    /// <inheritdoc />
    public partial class SeedHashedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CHANGED: Using UpdateData instead of InsertData to overwrite existing records 1 and 2 safely
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "Password", "Role" },
                values: new object[] { "admin@movie.com", "$2a$11$dI/tar9dFZ1xy1gFJIpQoeYM4gikhfxjMGoeHGGH6TYc00XYOovWe", "Admin" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Email", "Password", "Role" },
                values: new object[] { "customer@movie.com", "$2a$11$4fYPgktg9TT0Z8DyFMGP1uyzBZaAQzhQEYMyvFKnN1ghHfdHk/J6K", "Customer" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Keeps consistency if rolling back migrations
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}