using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestTask.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clients", x => x.id);
                    table.CheckConstraint("CK_Client_Balance_Positive", "balance >= 0");
                });

            migrationBuilder.CreateTable(
                name: "finance_transaction",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    transaction_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_finance_transaction", x => x.id);
                    table.ForeignKey(
                        name: "fk_finance_transaction_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transaction_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    finance_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    modification_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    old_client_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    new_client_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction_history", x => x.id);
                    table.CheckConstraint("CK_TransactionHistory_ClientBalance_Positive", "old_client_balance >= 0");
                    table.CheckConstraint("CK_TransactionHistory_NewClientBalance_Positive", "new_client_balance >= 0");
                    table.ForeignKey(
                        name: "fk_transaction_history_finance_transaction_finance_transaction",
                        column: x => x.finance_transaction_id,
                        principalTable: "finance_transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinanceTransaction_ClientId",
                table: "finance_transaction",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistory_FinanceTransactionId",
                table: "transaction_history",
                columns: new[] { "finance_transaction_id", "status" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transaction_history");

            migrationBuilder.DropTable(
                name: "finance_transaction");

            migrationBuilder.DropTable(
                name: "clients");
        }
    }
}
