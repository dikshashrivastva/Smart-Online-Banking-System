using Microsoft.EntityFrameworkCore;
using SmartBank.Data.Context;

namespace SmartBank.API.Helpers;

public static class TransactionSchemaInitializer
{
    public static async Task EnsureTransactionSchemaAsync(SmartOnlineBankingDbContext db)
    {
        if (db.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) != true)
            return;

        const string sql = """
IF COL_LENGTH('Accounts', 'RowVersion') IS NULL
    ALTER TABLE Accounts ADD RowVersion rowversion NOT NULL;

IF COL_LENGTH('Transactions', 'BalanceBefore') IS NULL
    ALTER TABLE Transactions ADD BalanceBefore decimal(18,2) NOT NULL CONSTRAINT DF_Transactions_BalanceBefore DEFAULT 0;

IF COL_LENGTH('Transactions', 'IPAddress') IS NULL
    ALTER TABLE Transactions ADD IPAddress nvarchar(50) NULL;

IF COL_LENGTH('Transactions', 'DeviceInfo') IS NULL
    ALTER TABLE Transactions ADD DeviceInfo nvarchar(500) NULL;

IF COL_LENGTH('Transfers', 'Fee') IS NULL
    ALTER TABLE Transfers ADD Fee decimal(18,2) NOT NULL CONSTRAINT DF_Transfers_Fee DEFAULT 0;

IF COL_LENGTH('Transfers', 'IdempotencyKey') IS NULL
    ALTER TABLE Transfers ADD IdempotencyKey nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Transfers_IdempotencyKey' AND object_id = OBJECT_ID('Transfers'))
    EXEC('CREATE UNIQUE INDEX IX_Transfers_IdempotencyKey ON Transfers(IdempotencyKey) WHERE IdempotencyKey IS NOT NULL');
""";

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
