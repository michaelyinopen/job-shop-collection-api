using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace job_shop_collection_api.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobSet",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: false),
                    JobColors = table.Column<string>(nullable: true),
                    IsAutoTimeOptions = table.Column<bool>(nullable: false),
                    TimeOptions = table.Column<string>(nullable: true),
                    IsLocked = table.Column<bool>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSet", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobSet");
        }
    }
}
