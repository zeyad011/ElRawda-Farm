using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElRawda.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class Magzer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CowsId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MachId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "slaughteredCows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CowsId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeightAtSlaughter = table.Column<double>(type: "float", nullable: false),
                    Waste = table.Column<double>(type: "float", nullable: true),
                    DateOfSlaughter = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MachId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_slaughteredCows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cowsPieces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachId = table.Column<int>(type: "int", nullable: false),
                    PieceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PieceWeight = table.Column<double>(type: "float", nullable: true),
                    PieceTybe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateOfSupply = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dateofExpiere = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CowId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cowsPieces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cowsPieces_slaughteredCows_CowId",
                        column: x => x.CowId,
                        principalTable: "slaughteredCows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cowsPieces_CowId",
                table: "cowsPieces",
                column: "CowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cows");

            migrationBuilder.DropTable(
                name: "cowsPieces");

            migrationBuilder.DropTable(
                name: "slaughteredCows");
        }
    }
}
