using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunicatieAppBackend.Migrations
{
    public partial class mig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Handleidingen",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Details = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Document = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Handleidingen", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Locaties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locaties", x => x.Id);
                })
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
                    Datum = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LocatieId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meldingen", x => x.MeldingId);
                    table.ForeignKey(
                        name: "FK_meldingen_Locaties_LocatieId",
                        column: x => x.LocatieId,
                        principalTable: "Locaties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    Image = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Datum = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LocatieId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nieuwsberichten", x => x.NieuwsberichtId);
                    table.ForeignKey(
                        name: "FK_nieuwsberichten_Locaties_LocatieId",
                        column: x => x.LocatieId,
                        principalTable: "Locaties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Locaties",
                columns: new[] { "Id", "name" },
                values: new object[] { 1, "Rijnland" });

            migrationBuilder.InsertData(
                table: "Locaties",
                columns: new[] { "Id", "name" },
                values: new object[] { 2, "Schieland" });

            migrationBuilder.InsertData(
                table: "meldingen",
                columns: new[] { "MeldingId", "Datum", "Inhoud", "LocatieId", "Titel" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 3, 8, 11, 33, 44, 121, DateTimeKind.Local).AddTicks(5401), "In navolging van het besluit dat het Centraal Stembureau 3 februari jl. heeft genomen is er geen bezwaar noch beroep hiertegen aangetekend. Dit houdt in dat de Kandidatenlijsten definitief zijn.Op vrijdag 3 februari heeft het Centraal Stembureau het besluit genomen over de geldigheid en nummering van de kandidatenlijsten voor de waterschapsverkiezingen op 15 maart 2023.", 1, "Besluit Centraal Stembureau kandidaatlijsten" },
                    { 2, new DateTime(2023, 3, 8, 11, 33, 44, 121, DateTimeKind.Local).AddTicks(5440), "In navolging van het besluit dat het Centraal Stembureau 3 februari jl. heeft genomen is er geen bezwaar noch beroep hiertegen aangetekend. Dit houdt in dat de Kandidatenlijsten definitief zijn.Op vrijdag 3 februari heeft het Centraal Stembureau het besluit genomen over de geldigheid en nummering van de kandidatenlijsten voor de waterschapsverkiezingen op 15 maart 2023.", 1, "Besluit Centraal Stembureau kandidaatlijsten" }
                });

            migrationBuilder.InsertData(
                table: "nieuwsberichten",
                columns: new[] { "NieuwsberichtId", "Datum", "Image", "Inhoud", "LocatieId", "Titel" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 3, 8, 11, 33, 44, 121, DateTimeKind.Local).AddTicks(5461), "plaatje.jpg", "Het is bijna weer zover, één keer in de vier jaar vieren we het feest van de democratie voor het waterschap via verkiezingen. Daar gaat een uitgekiende campagne bij helpen. Met de campagne maken we de inwoners van Rijnland nog meer bewust van het belangrijke werk dat wij doen. En vooral de bijzondere rol die zijzelf hebben, namelijk stemmen. En ja helaas, dat is in deze tijd nog steeds een bijzonder en groot goed dat we met elkaar moeten koesteren!!", 1, "Verkiezingen, wat kun jij doen…..?!" },
                    { 2, new DateTime(2023, 3, 8, 11, 33, 44, 121, DateTimeKind.Local).AddTicks(5464), "plaatje.jpg", "Het is bijna weer zover, één keer in de vier jaar vieren we het feest van de democratie voor het waterschap via verkiezingen. Daar gaat een uitgekiende campagne bij helpen. Met de campagne maken we de inwoners van Rijnland nog meer bewust van het belangrijke werk dat wij doen. En vooral de bijzondere rol die zijzelf hebben, namelijk stemmen. En ja helaas, dat is in deze tijd nog steeds een bijzonder en groot goed dat we met elkaar moeten koesteren!!", 1, "Verkiezingen, wat kun jij doen…..?!" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_meldingen_LocatieId",
                table: "meldingen",
                column: "LocatieId");

            migrationBuilder.CreateIndex(
                name: "IX_nieuwsberichten_LocatieId",
                table: "nieuwsberichten",
                column: "LocatieId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Handleidingen");

            migrationBuilder.DropTable(
                name: "meldingen");

            migrationBuilder.DropTable(
                name: "nieuwsberichten");

            migrationBuilder.DropTable(
                name: "Locaties");
        }
    }
}
