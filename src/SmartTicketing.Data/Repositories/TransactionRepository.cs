using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SmartTicketing.Core.Models;

namespace SmartTicketing.Data.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private static readonly List<PaymentTransaction> _inMemoryTransactions = new();
        private static readonly List<Ticket> _inMemoryTickets = new();
        private static readonly List<string> _inMemoryAuditLogs = new();

        public async Task<bool> SaveTransactionAsync(PaymentTransaction transaction, List<Ticket> tickets)
        {
            // Always store in-memory for immediate UI tracking
            _inMemoryTransactions.Add(transaction);
            _inMemoryTickets.AddRange(tickets);

            if (DatabaseHelper.CanConnectToSqlServer())
            {
                try
                {
                    using var conn = DatabaseHelper.CreateConnection();
                    await conn.OpenAsync();
                    using var sqlTxn = conn.BeginTransaction();

                    // Insert PaymentTransaction
                    string txnSql = @"
                        INSERT INTO dbo.PaymentTransactions 
                        (TransactionId, Amount, Timestamp, PaymentMethod, Status, MaskedCardNumber, CardHolderName, BankApprovalCode, WalletProvider, QRCodeData, GatewayReferenceId, MachineId)
                        VALUES 
                        (@TxnId, @Amt, @Time, @Method, @Status, @MaskCard, @Holder, @AuthCode, @Wallet, @QR, @RefId, @MachineId)";

                    using (var cmd = new SqlCommand(txnSql, conn, sqlTxn))
                    {
                        cmd.Parameters.AddWithValue("@TxnId", transaction.TransactionId);
                        cmd.Parameters.AddWithValue("@Amt", transaction.Amount);
                        cmd.Parameters.AddWithValue("@Time", transaction.Timestamp);
                        cmd.Parameters.AddWithValue("@Method", transaction.MethodType.ToString());
                        cmd.Parameters.AddWithValue("@Status", transaction.Status.ToString().ToUpperInvariant());

                        if (transaction is CreditCardTransaction cc)
                        {
                            cmd.Parameters.AddWithValue("@MaskCard", (object?)cc.MaskedCardNumber ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Holder", (object?)cc.CardHolderName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@AuthCode", (object?)cc.BankApprovalCode ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Wallet", DBNull.Value);
                            cmd.Parameters.AddWithValue("@QR", DBNull.Value);
                            cmd.Parameters.AddWithValue("@RefId", DBNull.Value);
                        }
                        else if (transaction is DigitalWalletTransaction dw)
                        {
                            cmd.Parameters.AddWithValue("@MaskCard", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Holder", DBNull.Value);
                            cmd.Parameters.AddWithValue("@AuthCode", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Wallet", (object?)dw.WalletProvider ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@QR", (object?)dw.QRCodeData ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@RefId", (object?)dw.GatewayReferenceId ?? DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@MaskCard", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Holder", DBNull.Value);
                            cmd.Parameters.AddWithValue("@AuthCode", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Wallet", DBNull.Value);
                            cmd.Parameters.AddWithValue("@QR", DBNull.Value);
                            cmd.Parameters.AddWithValue("@RefId", DBNull.Value);
                        }

                        cmd.Parameters.AddWithValue("@MachineId", transaction.MachineId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Insert Tickets
                    string ticketSql = @"
                        INSERT INTO dbo.Tickets 
                        (TicketId, Barcode, OriginStationId, DestinationStationId, FareCategory, Price, IssueTime, Status, TransactionId)
                        VALUES 
                        (@TktId, @Barcode, @OrigId, @DestId, @Category, @Price, @Time, @Status, @TxnId)";

                    foreach (var tkt in tickets)
                    {
                        using var cmdTkt = new SqlCommand(ticketSql, conn, sqlTxn);
                        cmdTkt.Parameters.AddWithValue("@TktId", tkt.TicketId);
                        cmdTkt.Parameters.AddWithValue("@Barcode", tkt.Barcode);
                        cmdTkt.Parameters.AddWithValue("@OrigId", tkt.OriginStationId);
                        cmdTkt.Parameters.AddWithValue("@DestId", tkt.DestinationStationId);
                        cmdTkt.Parameters.AddWithValue("@Category", tkt.Category.ToString());
                        cmdTkt.Parameters.AddWithValue("@Price", tkt.Price);
                        cmdTkt.Parameters.AddWithValue("@Time", tkt.IssueTime);
                        cmdTkt.Parameters.AddWithValue("@Status", tkt.Status.ToString().ToUpperInvariant());
                        cmdTkt.Parameters.AddWithValue("@TxnId", transaction.TransactionId);
                        await cmdTkt.ExecuteNonQueryAsync();
                    }

                    sqlTxn.Commit();
                    return true;
                }
                catch
                {
                    // Fall back cleanly if MSSQL commit fails
                }
            }

            return await Task.FromResult(true);
        }

        public async Task<List<PaymentTransaction>> GetAllTransactionsAsync()
        {
            return await Task.FromResult(new List<PaymentTransaction>(_inMemoryTransactions));
        }

        public async Task<List<Ticket>> GetAllIssuedTicketsAsync()
        {
            return await Task.FromResult(new List<Ticket>(_inMemoryTickets));
        }

        public async Task<bool> LogAuditEventAsync(string machineId, string eventType, string severity, string message)
        {
            string entry = $"[{DateTime.UtcNow:O}] [{machineId}] [{severity}] [{eventType}]: {message}";
            _inMemoryAuditLogs.Add(entry);

            if (DatabaseHelper.CanConnectToSqlServer())
            {
                try
                {
                    using var conn = DatabaseHelper.CreateConnection();
                    await conn.OpenAsync();
                    using var cmd = new SqlCommand("INSERT INTO dbo.AuditLogs (MachineId, EventType, Severity, Message) VALUES (@MachineId, @Type, @Sev, @Msg)", conn);
                    cmd.Parameters.AddWithValue("@MachineId", machineId);
                    cmd.Parameters.AddWithValue("@Type", eventType);
                    cmd.Parameters.AddWithValue("@Sev", severity);
                    cmd.Parameters.AddWithValue("@Msg", message);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch { }
            }

            return await Task.FromResult(true);
        }
    }
}
