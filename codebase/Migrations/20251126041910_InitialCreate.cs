using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace codebase.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartingPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AuctionDuration = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Auctions",
                columns: table => new
                {
                    AuctionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    HighestBidId = table.Column<int>(type: "integer", nullable: true),
                    ExtensionCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auctions", x => x.AuctionId);
                    table.ForeignKey(
                        name: "FK_Auctions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bids",
                columns: table => new
                {
                    BidId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuctionId = table.Column<int>(type: "integer", nullable: false),
                    BidderId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bids", x => x.BidId);
                    table.ForeignKey(
                        name: "FK_Bids_Auctions_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auctions",
                        principalColumn: "AuctionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bids_Users_BidderId",
                        column: x => x.BidderId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAttempts",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuctionId = table.Column<int>(type: "integer", nullable: false),
                    BidderId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    AttemptTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    WindowExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAttempts", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_PaymentAttempts_Auctions_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auctions",
                        principalColumn: "AuctionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentAttempts_Users_BidderId",
                        column: x => x.BidderId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_HighestBidId",
                table: "Auctions",
                column: "HighestBidId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_ProductId",
                table: "Auctions",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bids_AuctionId_Timestamp",
                table: "Bids",
                columns: new[] { "AuctionId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Bids_BidderId",
                table: "Bids",
                column: "BidderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_AuctionId_AttemptNumber",
                table: "PaymentAttempts",
                columns: new[] { "AuctionId", "AttemptNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAttempts_BidderId",
                table: "PaymentAttempts",
                column: "BidderId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_OwnerId",
                table: "Products",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_Bids_HighestBidId",
                table: "Auctions",
                column: "HighestBidId",
                principalTable: "Bids",
                principalColumn: "BidId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_Bids_HighestBidId",
                table: "Auctions");

            migrationBuilder.DropTable(
                name: "PaymentAttempts");

            migrationBuilder.DropTable(
                name: "Bids");

            migrationBuilder.DropTable(
                name: "Auctions");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
