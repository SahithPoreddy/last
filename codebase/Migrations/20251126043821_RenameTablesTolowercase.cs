using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace codebase.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesTolowercase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_Bids_HighestBidId",
                table: "Auctions");

            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_Products_ProductId",
                table: "Auctions");

            migrationBuilder.DropForeignKey(
                name: "FK_Bids_Auctions_AuctionId",
                table: "Bids");

            migrationBuilder.DropForeignKey(
                name: "FK_Bids_Users_BidderId",
                table: "Bids");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentAttempts_Auctions_AuctionId",
                table: "PaymentAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentAttempts_Users_BidderId",
                table: "PaymentAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_OwnerId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bids",
                table: "Bids");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Auctions",
                table: "Auctions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentAttempts",
                table: "PaymentAttempts");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "products");

            migrationBuilder.RenameTable(
                name: "Bids",
                newName: "bids");

            migrationBuilder.RenameTable(
                name: "Auctions",
                newName: "auctions");

            migrationBuilder.RenameTable(
                name: "PaymentAttempts",
                newName: "payment_attempts");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "users",
                newName: "IX_users_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Products_OwnerId",
                table: "products",
                newName: "IX_products_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Bids_BidderId",
                table: "bids",
                newName: "IX_bids_BidderId");

            migrationBuilder.RenameIndex(
                name: "IX_Bids_AuctionId_Timestamp",
                table: "bids",
                newName: "IX_bids_AuctionId_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_Auctions_ProductId",
                table: "auctions",
                newName: "IX_auctions_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Auctions_HighestBidId",
                table: "auctions",
                newName: "IX_auctions_HighestBidId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentAttempts_BidderId",
                table: "payment_attempts",
                newName: "IX_payment_attempts_BidderId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentAttempts_AuctionId_AttemptNumber",
                table: "payment_attempts",
                newName: "IX_payment_attempts_AuctionId_AttemptNumber");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_products",
                table: "products",
                column: "ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bids",
                table: "bids",
                column: "BidId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_auctions",
                table: "auctions",
                column: "AuctionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payment_attempts",
                table: "payment_attempts",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_auctions_bids_HighestBidId",
                table: "auctions",
                column: "HighestBidId",
                principalTable: "bids",
                principalColumn: "BidId");

            migrationBuilder.AddForeignKey(
                name: "FK_auctions_products_ProductId",
                table: "auctions",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_bids_auctions_AuctionId",
                table: "bids",
                column: "AuctionId",
                principalTable: "auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_bids_users_BidderId",
                table: "bids",
                column: "BidderId",
                principalTable: "users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_attempts_auctions_AuctionId",
                table: "payment_attempts",
                column: "AuctionId",
                principalTable: "auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_payment_attempts_users_BidderId",
                table: "payment_attempts",
                column: "BidderId",
                principalTable: "users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_products_users_OwnerId",
                table: "products",
                column: "OwnerId",
                principalTable: "users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_auctions_bids_HighestBidId",
                table: "auctions");

            migrationBuilder.DropForeignKey(
                name: "FK_auctions_products_ProductId",
                table: "auctions");

            migrationBuilder.DropForeignKey(
                name: "FK_bids_auctions_AuctionId",
                table: "bids");

            migrationBuilder.DropForeignKey(
                name: "FK_bids_users_BidderId",
                table: "bids");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_attempts_auctions_AuctionId",
                table: "payment_attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_payment_attempts_users_BidderId",
                table: "payment_attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_products_users_OwnerId",
                table: "products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_products",
                table: "products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bids",
                table: "bids");

            migrationBuilder.DropPrimaryKey(
                name: "PK_auctions",
                table: "auctions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payment_attempts",
                table: "payment_attempts");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "products",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "bids",
                newName: "Bids");

            migrationBuilder.RenameTable(
                name: "auctions",
                newName: "Auctions");

            migrationBuilder.RenameTable(
                name: "payment_attempts",
                newName: "PaymentAttempts");

            migrationBuilder.RenameIndex(
                name: "IX_users_Email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameIndex(
                name: "IX_products_OwnerId",
                table: "Products",
                newName: "IX_Products_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_bids_BidderId",
                table: "Bids",
                newName: "IX_Bids_BidderId");

            migrationBuilder.RenameIndex(
                name: "IX_bids_AuctionId_Timestamp",
                table: "Bids",
                newName: "IX_Bids_AuctionId_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_auctions_ProductId",
                table: "Auctions",
                newName: "IX_Auctions_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_auctions_HighestBidId",
                table: "Auctions",
                newName: "IX_Auctions_HighestBidId");

            migrationBuilder.RenameIndex(
                name: "IX_payment_attempts_BidderId",
                table: "PaymentAttempts",
                newName: "IX_PaymentAttempts_BidderId");

            migrationBuilder.RenameIndex(
                name: "IX_payment_attempts_AuctionId_AttemptNumber",
                table: "PaymentAttempts",
                newName: "IX_PaymentAttempts_AuctionId_AttemptNumber");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bids",
                table: "Bids",
                column: "BidId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Auctions",
                table: "Auctions",
                column: "AuctionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentAttempts",
                table: "PaymentAttempts",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_Bids_HighestBidId",
                table: "Auctions",
                column: "HighestBidId",
                principalTable: "Bids",
                principalColumn: "BidId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_Products_ProductId",
                table: "Auctions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_Auctions_AuctionId",
                table: "Bids",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_Users_BidderId",
                table: "Bids",
                column: "BidderId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentAttempts_Auctions_AuctionId",
                table: "PaymentAttempts",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentAttempts_Users_BidderId",
                table: "PaymentAttempts",
                column: "BidderId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_OwnerId",
                table: "Products",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
