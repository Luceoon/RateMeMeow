using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RMM.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the column
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Reviews",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow);
            
            // 2. Update existing data: Use SubmittedAt if available, else use current UTC time
            // Note: The SQL syntax might vary slightly depending on your provider (SQL Server, SQLite, PostgreSql)
            // For SQL Server:
            migrationBuilder.Sql(
                "UPDATE \"Reviews\" SET \"CreatedAt\" = COALESCE(\"SubmittedAt\", now())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Reviews");
        }
    }
}
