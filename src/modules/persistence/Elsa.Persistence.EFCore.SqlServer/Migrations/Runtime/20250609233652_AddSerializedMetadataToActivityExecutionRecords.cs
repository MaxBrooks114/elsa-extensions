using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elsa.Persistence.EFCore.SqlServer.Migrations.Runtime
{
    /// <inheritdoc />
    public partial class AddSerializedMetadataToActivityExecutionRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SerializedMetadata",
                schema: "Elsa",
                table: "ActivityExecutionRecords",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerializedMetadata",
                schema: "Elsa",
                table: "ActivityExecutionRecords");
        }
    }
}
