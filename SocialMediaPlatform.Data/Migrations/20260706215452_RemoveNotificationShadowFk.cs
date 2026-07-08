using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNotificationShadowFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Comments_CommentId1",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CommentId1",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CommentId1",
                table: "Notifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommentId1",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CommentId1",
                table: "Notifications",
                column: "CommentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Comments_CommentId1",
                table: "Notifications",
                column: "CommentId1",
                principalTable: "Comments",
                principalColumn: "Id");
        }
    }
}
