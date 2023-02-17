using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CommunicatieAppBackend.Migrations
{
    /// <inheritdoc />
    public partial class newmig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "meldingen",
                columns: table => new
                {
                    MeldingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Titel = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Inhoud = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Datum = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meldingen", x => x.MeldingId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "nieuwsberichten",
                columns: table => new
                {
                    NieuwsberichtId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Titel = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Inhoud = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Image = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Datum = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nieuwsberichten", x => x.NieuwsberichtId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "meldingen",
                columns: new[] { "MeldingId", "Datum", "Inhoud", "Titel" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 2, 17, 10, 16, 32, 589, DateTimeKind.Local).AddTicks(8546), "hoi", "test" },
                    { 2, new DateTime(2023, 2, 17, 10, 16, 32, 589, DateTimeKind.Local).AddTicks(8612), "hoi2", "test2" }
                });

            migrationBuilder.InsertData(
                table: "nieuwsberichten",
                columns: new[] { "NieuwsberichtId", "Datum", "Image", "Inhoud", "Titel" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 2, 17, 10, 16, 32, 589, DateTimeKind.Local).AddTicks(8827), "noimage", "hoi", "test" },
                    { 2, new DateTime(2023, 2, 17, 10, 16, 32, 589, DateTimeKind.Local).AddTicks(8837), "noimage", "hoi2", "test2" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meldingen");

            migrationBuilder.DropTable(
                name: "nieuwsberichten");
        }
    }
}
