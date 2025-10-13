using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project.Migrations
{
    /// <inheritdoc />
    public partial class addcollum_email_vv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Order",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Order",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TourId",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_TourId",
                table: "Order",
                column: "TourId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Tour_TourId",
                table: "Order",
                column: "TourId",
                principalTable: "Tour",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Tour_TourId",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_TourId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "TourId",
                table: "Order");
        }
    }
}
