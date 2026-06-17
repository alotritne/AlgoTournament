using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace algotournament.Migrations
{
    /// <inheritdoc />
    public partial class AddDuelSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DuelMatchId",
                table: "Submissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DuelRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoomCode = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    HostUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuelRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuelRooms_AspNetUsers_HostUserId",
                        column: x => x.HostUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DuelRooms_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DuelMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DuelRoomId = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WinnerUserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FinalScorePlayer1 = table.Column<int>(type: "int", nullable: false),
                    FinalScorePlayer2 = table.Column<int>(type: "int", nullable: false),
                    Player1UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Player2UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuelMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuelMatches_AspNetUsers_WinnerUserId",
                        column: x => x.WinnerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DuelMatches_DuelRooms_DuelRoomId",
                        column: x => x.DuelRoomId,
                        principalTable: "DuelRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DuelParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DuelRoomId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsReady = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SlotIndex = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuelParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuelParticipants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuelParticipants_DuelRooms_DuelRoomId",
                        column: x => x.DuelRoomId,
                        principalTable: "DuelRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_DuelMatchId",
                table: "Submissions",
                column: "DuelMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelMatches_DuelRoomId",
                table: "DuelMatches",
                column: "DuelRoomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DuelMatches_Player1UserId",
                table: "DuelMatches",
                column: "Player1UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelMatches_Player2UserId",
                table: "DuelMatches",
                column: "Player2UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelMatches_StartedAt",
                table: "DuelMatches",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DuelMatches_Status",
                table: "DuelMatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DuelMatches_WinnerUserId",
                table: "DuelMatches",
                column: "WinnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelParticipants_DuelRoomId_UserId",
                table: "DuelParticipants",
                columns: new[] { "DuelRoomId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DuelParticipants_UserId",
                table: "DuelParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelRooms_ExpiresAt",
                table: "DuelRooms",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_DuelRooms_HostUserId",
                table: "DuelRooms",
                column: "HostUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelRooms_ProblemId",
                table: "DuelRooms",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_DuelRooms_RoomCode",
                table: "DuelRooms",
                column: "RoomCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DuelRooms_Status",
                table: "DuelRooms",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_DuelMatches_DuelMatchId",
                table: "Submissions",
                column: "DuelMatchId",
                principalTable: "DuelMatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_DuelMatches_DuelMatchId",
                table: "Submissions");

            migrationBuilder.DropTable(
                name: "DuelMatches");

            migrationBuilder.DropTable(
                name: "DuelParticipants");

            migrationBuilder.DropTable(
                name: "DuelRooms");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_DuelMatchId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "DuelMatchId",
                table: "Submissions");
        }
    }
}
