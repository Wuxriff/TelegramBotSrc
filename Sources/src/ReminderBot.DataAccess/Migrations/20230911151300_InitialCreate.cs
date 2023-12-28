using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReminderBot.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelegramUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChatId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TelegramUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Base64Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReminderDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OriginalReminderDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReminderDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OriginalReminderDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateSentUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OriginalDateSentUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    MessageId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_TelegramUsers_TelegramUserId",
                        column: x => x.TelegramUserId,
                        principalTable: "TelegramUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelegramUserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TelegramUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPaused = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAutoDeleteMessages = table.Column<bool>(type: "INTEGER", nullable: false),
                    PostponeMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    TimeZoneId = table.Column<string>(type: "TEXT", nullable: true),
                    CountryName = table.Column<string>(type: "TEXT", nullable: true),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeFormatType = table.Column<string>(type: "TEXT", nullable: false),
                    DateTimeFormat = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUserSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelegramUserSettings_TelegramUsers_TelegramUserId",
                        column: x => x.TelegramUserId,
                        principalTable: "TelegramUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_TelegramUserId",
                table: "Reminders",
                column: "TelegramUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramUserSettings_TelegramUserId",
                table: "TelegramUserSettings",
                column: "TelegramUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "TelegramUserSettings");

            migrationBuilder.DropTable(
                name: "TelegramUsers");
        }
    }
}
