using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphProcessorAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedImagecolumninGraphentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image",
                table: "graph",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image",
                table: "graph");
        }
    }
}
