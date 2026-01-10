using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashRegister.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    AuthorizedByUserId = table.Column<int>(type: "int", nullable: true),
                    AuthorizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashEntries_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashEntries_Users_AuthorizedByUserId",
                        column: x => x.AuthorizedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashEntries_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashEntryRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashEntryId = table.Column<int>(type: "int", nullable: false),
                    RowType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsOutflow = table.Column<bool>(type: "bit", nullable: false),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    Amount1000 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount500 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount200 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount100 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount50 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount20 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount10 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount5 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount2 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount1 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoinAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashEntryRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashEntryRows_CashEntries_CashEntryId",
                        column: x => x.CashEntryId,
                        principalTable: "CashEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_BranchCode",
                table: "Branches",
                column: "BranchCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashEntries_AuthorizedByUserId",
                table: "CashEntries",
                column: "AuthorizedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CashEntries_BranchId_EntryDate",
                table: "CashEntries",
                columns: new[] { "BranchId", "EntryDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashEntries_CreatedByUserId",
                table: "CashEntries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CashEntryRows_CashEntryId_SequenceOrder",
                table: "CashEntryRows",
                columns: new[] { "CashEntryId", "SequenceOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_BranchId",
                table: "Users",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashEntryRows");

            migrationBuilder.DropTable(
                name: "CashEntries");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Branches");
        }
    }
}
