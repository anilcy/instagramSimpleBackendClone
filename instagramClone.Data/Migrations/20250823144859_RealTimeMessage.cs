using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace instagramClone.Data.Migrations
{
    /// <inheritdoc />
    public partial class RealTimeMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "Messages",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Messages");
        }
    }
}
