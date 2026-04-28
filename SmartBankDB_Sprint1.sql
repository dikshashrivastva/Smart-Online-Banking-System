Create Database SmartOnlineBankingDb;
use SmartOnlineBankingDb;

-- =============================================
-- ROLES
-- =============================================
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- =============================================
-- USERS
-- =============================================
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    RoleId INT NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(512) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20) UNIQUE,
    NationalId NVARCHAR(30) UNIQUE,
    DateOfBirth DATE,
    Gender NVARCHAR(10),
    Address NVARCHAR(500),
    City NVARCHAR(100),
    Country NVARCHAR(100) DEFAULT 'India',
    KycStatus NVARCHAR(20) DEFAULT 'Pending',
    IsEmailVerified BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    IsFrozen BIT DEFAULT 0,
    FailedLoginAttempts INT DEFAULT 0,
    LastLoginAt DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,

    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId)
        REFERENCES Roles(RoleId)
);

-- =============================================
-- KYC DOCUMENTS
-- =============================================
CREATE TABLE KycDocuments (
    KycDocumentId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    DocumentType NVARCHAR(50),
    DocumentNumber NVARCHAR(100) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    UploadedAt DATETIME2 DEFAULT GETDATE(),
    VerifiedAt DATETIME2,
    VerifiedByUserId INT,
    Status NVARCHAR(20),
    RejectionReason NVARCHAR(500),

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (VerifiedByUserId) REFERENCES Users(UserId)
);

-- =============================================
-- ACCOUNTS
-- =============================================
CREATE TABLE Accounts (
    AccountId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    AccountNumber NVARCHAR(20) NOT NULL UNIQUE,
    AccountType NVARCHAR(20) NOT NULL,
    Balance DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency NVARCHAR(5) DEFAULT 'INR',
    Status NVARCHAR(20) DEFAULT 'Active',
    MinimumBalance DECIMAL(18,2) DEFAULT 500,
    InterestRate DECIMAL(5,2),
    BranchCode NVARCHAR(20),
    IFSCCode NVARCHAR(15),
    OpenedAt DATETIME2 DEFAULT GETDATE(),
    ClosedAt DATETIME2,
    UpdatedAt DATETIME2,

    CONSTRAINT FK_Accounts_Users FOREIGN KEY (UserId)
        REFERENCES Users(UserId),

    CONSTRAINT CK_Account_Balance CHECK (Balance >= 0)
);

-- =============================================
-- TRANSFERS
-- =============================================
CREATE TABLE Transfers (
    TransferId INT IDENTITY(1,1) PRIMARY KEY,
    FromAccountId INT NOT NULL,
    ToAccountId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Remarks NVARCHAR(500),
    ReferenceNumber NVARCHAR(50) UNIQUE,
    Status NVARCHAR(20),
    InitiatedByUserId INT NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CompletedAt DATETIME2,

    FOREIGN KEY (FromAccountId) REFERENCES Accounts(AccountId),
    FOREIGN KEY (ToAccountId) REFERENCES Accounts(AccountId),
    FOREIGN KEY (InitiatedByUserId) REFERENCES Users(UserId),

    CONSTRAINT CK_Transfer_Amount CHECK (Amount > 0),
    CONSTRAINT CK_Transfer_Diff CHECK (FromAccountId <> ToAccountId)
);

-- =============================================
-- TRANSACTIONS
-- =============================================
CREATE TABLE Transactions (
    TransactionId INT IDENTITY(1,1) PRIMARY KEY,
    AccountId INT NOT NULL,
    TransferId INT,
    PerformedByUserId INT,
    TransactionType NVARCHAR(20),
    Amount DECIMAL(18,2) NOT NULL,
    BalanceAfter DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500),
    ReferenceNumber NVARCHAR(50) UNIQUE,
    Channel NVARCHAR(20),
    Status NVARCHAR(20),
    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
    FOREIGN KEY (TransferId) REFERENCES Transfers(TransferId),
    FOREIGN KEY (PerformedByUserId) REFERENCES Users(UserId),

    CONSTRAINT CK_Transaction_Amount CHECK (Amount > 0)
);

-- =============================================
-- LOANS
-- =============================================
CREATE TABLE Loans (
    LoanId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    AccountId INT,
    ReviewedByUserId INT,
    LoanType NVARCHAR(50),
    RequestedAmount DECIMAL(18,2) NOT NULL,
    ApprovedAmount DECIMAL(18,2),
    InterestRate DECIMAL(5,2),
    TenureMonths INT NOT NULL,
    EMIAmount DECIMAL(18,2),
    Purpose NVARCHAR(500),
    Status NVARCHAR(20),
    ReviewedAt DATETIME2,
    RejectionReason NVARCHAR(500),
    DisbursedAt DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
    FOREIGN KEY (ReviewedByUserId) REFERENCES Users(UserId),

    CONSTRAINT CK_Loan_Amount CHECK (RequestedAmount > 0),
    CONSTRAINT CK_Loan_Tenure CHECK (TenureMonths > 0)
);

-- =============================================
-- LOAN DOCUMENTS
-- =============================================
CREATE TABLE LoanDocuments (
    LoanDocumentId INT IDENTITY(1,1) PRIMARY KEY,
    LoanId INT NOT NULL,
    DocumentType NVARCHAR(100),
    FilePath NVARCHAR(500) NOT NULL,
    UploadedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (LoanId) REFERENCES Loans(LoanId)
);

-- =============================================
-- SUPPORT TICKETS
-- =============================================
CREATE TABLE SupportTickets (
    TicketId INT IDENTITY(1,1) PRIMARY KEY,
    CreatedByUserId INT NOT NULL,
    AssignedToUserId INT,
    Subject NVARCHAR(200) NOT NULL,
    Description NVARCHAR(2000) NOT NULL,
    Category NVARCHAR(50),
    Priority NVARCHAR(20),
    Status NVARCHAR(20),
    Resolution NVARCHAR(2000),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    ResolvedAt DATETIME2,

    FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId),
    FOREIGN KEY (AssignedToUserId) REFERENCES Users(UserId)
);

-- =============================================
-- NOTIFICATIONS
-- =============================================
CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(2000) NOT NULL,
    Type NVARCHAR(50),
    IsRead BIT DEFAULT 0,
    ReadAt DATETIME2,
    RelatedEntityId INT,
    RelatedEntityType NVARCHAR(50),
    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- =============================================
-- AUDIT LOGS
-- =============================================
CREATE TABLE AuditLogs (
    AuditLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId INT,
    Action NVARCHAR(100) NOT NULL,
    EntityType NVARCHAR(50),
    EntityId INT,
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    IPAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- =============================================
-- INDEXES
-- =============================================
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Accounts_UserId ON Accounts(UserId);
CREATE INDEX IX_Transactions_AccountId ON Transactions(AccountId);
CREATE INDEX IX_Transfers_FromAccountId ON Transfers(FromAccountId);
CREATE INDEX IX_Transfers_ToAccountId ON Transfers(ToAccountId);
CREATE INDEX IX_Loans_UserId ON Loans(UserId);
CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);

--  SECTION 2: NON-CLUSTERED INDEXES
-- ============================================================
 
-- Users
CREATE NONCLUSTERED INDEX IX_Users_PhoneNumber    ON Users(PhoneNumber) WHERE PhoneNumber IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Users_NationalId     ON Users(NationalId)  WHERE NationalId  IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Users_RoleId         ON Users(RoleId);
 
-- Accounts
CREATE NONCLUSTERED INDEX IX_Accounts_AccountNumber ON Accounts(AccountNumber);
CREATE NONCLUSTERED INDEX IX_Accounts_Status        ON Accounts(Status);
 
-- Transactions
CREATE NONCLUSTERED INDEX IX_Transactions_AccountId_CreatedAt
    ON Transactions(AccountId, CreatedAt DESC)
    INCLUDE (TransactionType, Amount, BalanceAfter, Status);
 
CREATE NONCLUSTERED INDEX IX_Transactions_ReferenceNumber
    ON Transactions(ReferenceNumber);
 
CREATE NONCLUSTERED INDEX IX_Transactions_TransferId
    ON Transactions(TransferId) WHERE TransferId IS NOT NULL;
 
-- Transfers

CREATE NONCLUSTERED INDEX IX_Transfers_ReferenceNumber ON Transfers(ReferenceNumber);
 
-- Loans
CREATE NONCLUSTERED INDEX IX_Loans_UserId_Status ON Loans(UserId, Status);
CREATE NONCLUSTERED INDEX IX_Loans_AccountId     ON Loans(AccountId) WHERE AccountId IS NOT NULL;
 
-- KYC Documents
CREATE NONCLUSTERED INDEX IX_KycDocs_UserId_Status ON KycDocuments(UserId, Status);
 
-- Support Tickets
CREATE NONCLUSTERED INDEX IX_Tickets_CreatedByUserId_Status
    ON SupportTickets(CreatedByUserId, Status);
 
CREATE NONCLUSTERED INDEX IX_Tickets_AssignedToUserId
    ON SupportTickets(AssignedToUserId) WHERE AssignedToUserId IS NOT NULL;
 
-- Notifications
CREATE NONCLUSTERED INDEX IX_Notif_UserId_IsRead
    ON Notifications(UserId, IsRead)
    INCLUDE (Title, Type, CreatedAt);
 
-- Audit Logs
CREATE NONCLUSTERED INDEX IX_AuditLogs_UserId_CreatedAt
    ON AuditLogs(UserId, CreatedAt DESC) WHERE UserId IS NOT NULL;
 
CREATE NONCLUSTERED INDEX IX_AuditLogs_EntityType_EntityId
    ON AuditLogs(EntityType, EntityId) WHERE EntityType IS NOT NULL;
 
GO
 
 
-- ============================================================
--  SECTION 3: STORED PROCEDURES
-- ============================================================
 
-- ── SP 1: Register User ───────────────────────────────────────
CREATE OR ALTER PROCEDURE sp_RegisterUser
    @RoleId         INT,
    @Email          NVARCHAR(150),
    @PasswordHash   NVARCHAR(512),
    @FirstName      NVARCHAR(100),
    @LastName       NVARCHAR(100),
    @PhoneNumber    NVARCHAR(20)  = NULL,
    @NationalId     NVARCHAR(30)  = NULL,
    @NewUserId      INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
 
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        RAISERROR(N'Email already registered.', 16, 1);
        RETURN;
    END
 
    INSERT INTO Users (RoleId, Email, PasswordHash, FirstName, LastName, PhoneNumber, NationalId)
    VALUES (@RoleId, @Email, @PasswordHash, @FirstName, @LastName, @PhoneNumber, @NationalId);
 
    SET @NewUserId = SCOPE_IDENTITY();
 
    INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId)
    VALUES (@NewUserId, N'UserRegistered', N'User', @NewUserId);
END;
GO
 
-- ── SP 2: Open Account ────────────────────────────────────────
CREATE OR ALTER PROCEDURE sp_OpenAccount
    @UserId         INT,
    @AccountType    NVARCHAR(20),
    @InitialDeposit DECIMAL(18,2) = 0.00,
    @NewAccountId   INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
 
    DECLARE @AccountNumber NVARCHAR(20);
    SET @AccountNumber = N'SB' + FORMAT(YEAR(GETDATE()), '0000')
                        + RIGHT('000000' + CAST(NEXT VALUE FOR AccountNumberSeq AS NVARCHAR), 6);
 
    INSERT INTO Accounts (UserId, AccountNumber, AccountType, Balance)
    VALUES (@UserId, @AccountNumber, @AccountType, @InitialDeposit);
 
    SET @NewAccountId = SCOPE_IDENTITY();
 
    IF @InitialDeposit > 0
    BEGIN
        DECLARE @TxnRef NVARCHAR(50) = N'TXN' + CONVERT(NVARCHAR, NEWID());
        INSERT INTO Transactions (AccountId, TransactionType, Amount, BalanceAfter, ReferenceNumber, Description)
        VALUES (@NewAccountId, N'Deposit', @InitialDeposit, @InitialDeposit, @TxnRef, N'Initial deposit on account opening');
    END
 
    INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId)
    VALUES (@UserId, N'AccountOpened', N'Account', @NewAccountId);
END;
GO
 
-- ── SP 3: Deposit ─────────────────────────────────────────────
CREATE OR ALTER PROCEDURE sp_Deposit
    @AccountId          INT,
    @Amount             DECIMAL(18,2),
    @PerformedByUserId  INT = NULL,
    @Description        NVARCHAR(500) = NULL,
    @Channel            NVARCHAR(20)  = N'Online'
AS
BEGIN
    SET NOCOUNT ON;
 
    IF @Amount <= 0
    BEGIN
        RAISERROR(N'Deposit amount must be greater than zero.', 16, 1);
        RETURN;
    END
 
    IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountId = @AccountId AND Status = N'Active')
    BEGIN
        RAISERROR(N'Account is not active.', 16, 1);
        RETURN;
    END
 
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE Accounts SET Balance = Balance + @Amount, UpdatedAt = GETDATE()
        WHERE AccountId = @AccountId;
 
        DECLARE @NewBalance DECIMAL(18,2);
        SELECT @NewBalance = Balance FROM Accounts WHERE AccountId = @AccountId;
 
        DECLARE @Ref NVARCHAR(50) = N'DEP' + REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', '');
 
        INSERT INTO Transactions
            (AccountId, PerformedByUserId, TransactionType, Amount, BalanceAfter, ReferenceNumber, Description, Channel)
        VALUES
            (@AccountId, @PerformedByUserId, N'Deposit', @Amount, @NewBalance, @Ref, @Description, @Channel);
 
        INSERT INTO Notifications (UserId, Title, Message, Type, RelatedEntityId, RelatedEntityType)
        SELECT UserId,
               N'Amount Deposited',
               N'INR ' + CAST(@Amount AS NVARCHAR) + N' deposited. New balance: INR ' + CAST(@NewBalance AS NVARCHAR),
               N'Transaction',
               SCOPE_IDENTITY(),
               N'Transaction'
        FROM Accounts WHERE AccountId = @AccountId;
 
        INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId, NewValue)
        SELECT @PerformedByUserId, N'Deposit', N'Account', @AccountId,
               N'{"amount":' + CAST(@Amount AS NVARCHAR) + N',"balanceAfter":' + CAST(@NewBalance AS NVARCHAR) + N'}'
        FROM Accounts WHERE AccountId = @AccountId;
 
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
 
-- ── SP 4: Withdraw ────────────────────────────────────────────
CREATE OR ALTER PROCEDURE sp_Withdraw
    @AccountId          INT,
    @Amount             DECIMAL(18,2),
    @PerformedByUserId  INT = NULL,
    @Description        NVARCHAR(500) = NULL,
    @Channel            NVARCHAR(20)  = N'Online'
AS
BEGIN
    SET NOCOUNT ON;
 
    IF @Amount <= 0
    BEGIN
        RAISERROR(N'Withdrawal amount must be greater than zero.', 16, 1);
        RETURN;
    END
 
    DECLARE @CurrentBalance DECIMAL(18,2), @MinBal DECIMAL(18,2), @AccStatus NVARCHAR(20);
    SELECT @CurrentBalance = Balance, @MinBal = MinimumBalance, @AccStatus = Status
    FROM Accounts WHERE AccountId = @AccountId;
 
    IF @AccStatus <> N'Active'
    BEGIN
        RAISERROR(N'Account is not active.', 16, 1);
        RETURN;
    END
 
    IF (@CurrentBalance - @Amount) < @MinBal
    BEGIN
        RAISERROR(N'Insufficient balance. Minimum balance requirement must be maintained.', 16, 1);
        RETURN;
    END
 
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE Accounts SET Balance = Balance - @Amount, UpdatedAt = GETDATE()
        WHERE AccountId = @AccountId;
 
        DECLARE @NewBalance DECIMAL(18,2);
        SELECT @NewBalance = Balance FROM Accounts WHERE AccountId = @AccountId;
 
        DECLARE @Ref NVARCHAR(50) = N'WDR' + REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', '');
 
        INSERT INTO Transactions
            (AccountId, PerformedByUserId, TransactionType, Amount, BalanceAfter, ReferenceNumber, Description, Channel)
        VALUES
            (@AccountId, @PerformedByUserId, N'Withdrawal', @Amount, @NewBalance, @Ref, @Description, @Channel);
 
        INSERT INTO Notifications (UserId, Title, Message, Type, RelatedEntityId, RelatedEntityType)
        SELECT UserId,
               N'Amount Withdrawn',
               N'INR ' + CAST(@Amount AS NVARCHAR) + N' withdrawn. New balance: INR ' + CAST(@NewBalance AS NVARCHAR),
               N'Transaction',
               SCOPE_IDENTITY(),
               N'Transaction'
        FROM Accounts WHERE AccountId = @AccountId;
 
        INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId, OldValue, NewValue)
        VALUES (@PerformedByUserId, N'Withdrawal', N'Account', @AccountId,
                N'{"balanceBefore":' + CAST(@CurrentBalance AS NVARCHAR) + N'}',
                N'{"balanceAfter":'  + CAST(@NewBalance    AS NVARCHAR) + N'}');
 
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
 
-- ── SP 5: Transfer Funds (Atomic) ─────────────────────────────
-- Creates one Transfers row + two Transactions rows atomically.
CREATE OR ALTER PROCEDURE sp_TransferFunds
    @FromAccountId      INT,
    @ToAccountId        INT,
    @Amount             DECIMAL(18,2),
    @Remarks            NVARCHAR(500) = NULL,
    @InitiatedByUserId  INT,
    @NewTransferId      INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
 
    IF @Amount <= 0
    BEGIN
        RAISERROR(N'Transfer amount must be greater than zero.', 16, 1);
        RETURN;
    END
 
    IF @FromAccountId = @ToAccountId
    BEGIN
        RAISERROR(N'Cannot transfer to the same account.', 16, 1);
        RETURN;
    END
 
    DECLARE @FromBal DECIMAL(18,2), @FromMin DECIMAL(18,2), @FromStatus NVARCHAR(20);
    DECLARE @ToStatus NVARCHAR(20);
 
    SELECT @FromBal = Balance, @FromMin = MinimumBalance, @FromStatus = Status
    FROM Accounts WHERE AccountId = @FromAccountId;
 
    SELECT @ToStatus = Status FROM Accounts WHERE AccountId = @ToAccountId;
 
    IF @FromStatus <> N'Active'
    BEGIN
        RAISERROR(N'Source account is not active.', 16, 1);
        RETURN;
    END
 
    IF @ToStatus <> N'Active'
    BEGIN
        RAISERROR(N'Destination account is not active.', 16, 1);
        RETURN;
    END
 
    IF (@FromBal - @Amount) < @FromMin
    BEGIN
        RAISERROR(N'Insufficient balance in source account.', 16, 1);
        RETURN;
    END
 
    DECLARE @TrfRef NVARCHAR(50) = N'TRF' + REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', '');
 
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Create transfer header
        INSERT INTO Transfers (FromAccountId, ToAccountId, Amount, Remarks, ReferenceNumber, InitiatedByUserId)
        VALUES (@FromAccountId, @ToAccountId, @Amount, @Remarks, @TrfRef, @InitiatedByUserId);
        SET @NewTransferId = SCOPE_IDENTITY();
 
        -- Debit source
        UPDATE Accounts SET Balance = Balance - @Amount, UpdatedAt = GETDATE()
        WHERE AccountId = @FromAccountId;
 
        DECLARE @FromBalAfter DECIMAL(18,2);
        SELECT @FromBalAfter = Balance FROM Accounts WHERE AccountId = @FromAccountId;
 
        INSERT INTO Transactions
            (AccountId, TransferId, PerformedByUserId, TransactionType, Amount, BalanceAfter, ReferenceNumber, Description)
        VALUES
            (@FromAccountId, @NewTransferId, @InitiatedByUserId, N'TransferDebit',
             @Amount, @FromBalAfter, N'DBT-' + @TrfRef, N'Transfer debit - Ref: ' + @TrfRef);
 
        -- Credit destination
        UPDATE Accounts SET Balance = Balance + @Amount, UpdatedAt = GETDATE()
        WHERE AccountId = @ToAccountId;
 
        DECLARE @ToBalAfter DECIMAL(18,2);
        SELECT @ToBalAfter = Balance FROM Accounts WHERE AccountId = @ToAccountId;
 
        INSERT INTO Transactions
            (AccountId, TransferId, PerformedByUserId, TransactionType, Amount, BalanceAfter, ReferenceNumber, Description)
        VALUES
            (@ToAccountId, @NewTransferId, @InitiatedByUserId, N'TransferCredit',
             @Amount, @ToBalAfter, N'CRD-' + @TrfRef, N'Transfer credit - Ref: ' + @TrfRef);
 
        -- Mark transfer completed
        UPDATE Transfers SET Status = N'Completed', CompletedAt = GETDATE()
        WHERE TransferId = @NewTransferId;
 
        -- Notify both parties
        INSERT INTO Notifications (UserId, Title, Message, Type, RelatedEntityId, RelatedEntityType)
        SELECT UserId,
               N'Funds Transferred Out',
               N'INR ' + CAST(@Amount AS NVARCHAR) + N' transferred. Ref: ' + @TrfRef,
               N'Transaction', @NewTransferId, N'Transfer'
        FROM Accounts WHERE AccountId = @FromAccountId;
 
        INSERT INTO Notifications (UserId, Title, Message, Type, RelatedEntityId, RelatedEntityType)
        SELECT UserId,
               N'Funds Received',
               N'INR ' + CAST(@Amount AS NVARCHAR) + N' received. Ref: ' + @TrfRef,
               N'Transaction', @NewTransferId, N'Transfer'
        FROM Accounts WHERE AccountId = @ToAccountId;
 
        INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId, NewValue)
        VALUES (@InitiatedByUserId, N'Transfer', N'Transfer', @NewTransferId,
                N'{"from":' + CAST(@FromAccountId AS NVARCHAR) +
                N',"to":'   + CAST(@ToAccountId   AS NVARCHAR) +
                N',"amount":' + CAST(@Amount AS NVARCHAR) + N'}');
 
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        UPDATE Transfers SET Status = N'Failed' WHERE TransferId = @NewTransferId;
        THROW;
    END CATCH;
END;
GO
 
-- ── SP 6: Apply for Loan ──────────────────────────────────────
CREATE OR ALTER PROCEDURE sp_ApplyLoan
    @UserId          INT,
    @AccountId       INT = NULL,
    @LoanType        NVARCHAR(50),
    @RequestedAmount DECIMAL(18,2),
    @TenureMonths    INT,
    @Purpose         NVARCHAR(500) = NULL,
    @NewLoanId       INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
 
    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId AND IsActive = 1 AND IsFrozen = 0)
    BEGIN
        RAISERROR(N'User account is inactive or frozen.', 16, 1);
        RETURN;
    END
 
    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId AND KycStatus = N'Verified')
    BEGIN
        RAISERROR(N'KYC verification required before applying for a loan.', 16, 1);
        RETURN;
    END
 
    INSERT INTO Loans (UserId, AccountId, LoanType, RequestedAmount, TenureMonths, Purpose)
    VALUES (@UserId, @AccountId, @LoanType, @RequestedAmount, @TenureMonths, @Purpose);
 
    SET @NewLoanId = SCOPE_IDENTITY();
 
    INSERT INTO Notifications (UserId, Title, Message, Type, RelatedEntityId, RelatedEntityType)
    VALUES (@UserId, N'Loan Application Submitted',
            N'Your ' + @LoanType + N' loan application for INR ' + CAST(@RequestedAmount AS NVARCHAR) + N' has been received.',
            N'LoanUpdate', @NewLoanId, N'Loan');
 
    INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId)
    VALUES (@UserId, N'LoanApplied', N'Loan', @NewLoanId);
END;
GO
 
-- ── SP 7: Approve / Reject Loan ───────────────────────────────
CREATE OR ALTER PROCEDURE sp_ReviewLoan
    @LoanId           INT,
    @ReviewedByUserId INT,
    @Decision         NVARCHAR(20),   -- 'Approved' or 'Rejected'
    @ApprovedAmount   DECIMAL(18,2) = NULL,
    @InterestRate     DECIMAL(5,2)  = NULL,
    @RejectionReason  NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
 
    IF @Decision NOT IN (N'Approved', N'Rejected')
    BEGIN
        RAISERROR(N'Decision must be Approved or Rejected.', 16, 1);
        RETURN;
    END
 
    DECLARE @CurrentStatus NVARCHAR(20), @LoanUserId INT, @LoanType NVARCHAR(50);
    SELECT @CurrentStatus = Status, @LoanUserId = UserId, @LoanType = LoanType
    FROM Loans WHERE LoanId = @LoanId;
 
    IF @CurrentStatus NOT IN (N'Pending', N'UnderReview')
    BEGIN
        RAISERROR(N'Loan is not in a reviewable state.', 16, 1);
        RETURN;
    END
 
    UPDATE Loans
    SET Status           = @Decision,
        ReviewedByUserId = @ReviewedByUserId,
        ReviewedAt       = GETDATE(),
        ApprovedAmount   = CASE WHEN @Decision = N'Approved' THEN @ApprovedAmount ELSE NULL END,
        InterestRate     = CASE WHEN @Decision = N'Approved' THEN @InterestRate   ELSE NULL END,
        RejectionReason  = CASE WHEN @Decision = N'Rejected' THEN @RejectionReason ELSE NULL END,
        UpdatedAt        = GETDATE()
    WHERE LoanId = @LoanId;
 
    DECLARE @NotifTitle   NVARCHAR(200);
    DECLARE @NotifMessage NVARCHAR(2000);
 
    IF @Decision = N'Approved'
    BEGIN
        SET @NotifTitle   = N'Loan Approved';
        SET @NotifMessage = N'Your ' + @LoanType + N' loan of INR ' + CAST(@ApprovedAmount AS NVARCHAR) + N' has been approved.';
    END
    ELSE
    BEGIN
        SET @NotifTitle   = N'Loan Rejected';
        SET @NotifMessage = N'Your ' + @LoanType + N' loan application has been rejected. Reason: ' + ISNULL(@RejectionReason, N'N/A');
    END
 
    INSERT INTO Notifications (UserId, Title, Message, Type, RelatedEntityId, RelatedEntityType)
    VALUES (@LoanUserId, @NotifTitle, @NotifMessage, N'LoanUpdate', @LoanId, N'Loan');
 
    INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId, NewValue)
    VALUES (@ReviewedByUserId, N'Loan' + @Decision, N'Loan', @LoanId,
            N'{"decision":"' + @Decision + N'"}');
END;
GO
 
-- ── SP 8: Raise Support Ticket ────────────────────────────────
CREATE OR ALTER PROCEDURE sp_RaiseTicket
    @CreatedByUserId INT,
    @Subject         NVARCHAR(200),
    @Description     NVARCHAR(2000),
    @Category        NVARCHAR(50),
    @Priority        NVARCHAR(20) = N'Medium',
    @NewTicketId     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
 
    INSERT INTO SupportTickets (CreatedByUserId, Subject, Description, Category, Priority)
    VALUES (@CreatedByUserId, @Subject, @Description, @Category, @Priority);
 
    SET @NewTicketId = SCOPE_IDENTITY();
 
    INSERT INTO Notifications (UserId, Title, Message, Type, RelatedEntityId, RelatedEntityType)
    VALUES (@CreatedByUserId,
            N'Support Ticket Raised',
            N'Ticket #' + CAST(@NewTicketId AS NVARCHAR) + N' created: ' + @Subject,
            N'TicketUpdate', @NewTicketId, N'Ticket');
 
    INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId)
    VALUES (@CreatedByUserId, N'TicketRaised', N'SupportTicket', @NewTicketId);
END;
GO
 
-- ── SP 9: Freeze / Unfreeze Account ───────────────────────────
CREATE OR ALTER PROCEDURE sp_SetAccountFreeze
    @AccountId     INT,
    @AdminUserId   INT,
    @Freeze        BIT   -- 1 = Freeze, 0 = Unfreeze
AS
BEGIN
    SET NOCOUNT ON;
 
    IF NOT EXISTS (
        SELECT 1 FROM Users u
        JOIN Roles r ON u.RoleId = r.RoleId
        WHERE u.UserId = @AdminUserId AND r.RoleName IN (N'Admin', N'Manager')
    )
    BEGIN
        RAISERROR(N'Only Admin or Manager can freeze/unfreeze accounts.', 16, 1);
        RETURN;
    END
 
    DECLARE @NewStatus NVARCHAR(20) = CASE WHEN @Freeze = 1 THEN N'Frozen' ELSE N'Active' END;
    DECLARE @Action    NVARCHAR(100) = CASE WHEN @Freeze = 1 THEN N'AccountFrozen' ELSE N'AccountUnfrozen' END;
 
    UPDATE Accounts SET Status = @NewStatus, UpdatedAt = GETDATE()
    WHERE AccountId = @AccountId;
 
    INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId)
    VALUES (@AdminUserId, @Action, N'Account', @AccountId);
END;
GO
 
-- ── SP 10: Get Transaction History ────────────────────────────
CREATE OR ALTER PROCEDURE sp_GetTransactionHistory
    @AccountId  INT,
    @PageNumber INT = 1,
    @PageSize   INT = 20
AS
BEGIN
    SET NOCOUNT ON;
 
    SELECT
        t.TransactionId,
        t.TransactionType,
        t.Amount,
        t.BalanceAfter,
        t.ReferenceNumber,
        t.Description,
        t.Channel,
        t.Status,
        t.CreatedAt,
        tf.ReferenceNumber AS TransferReference
    FROM Transactions t
    LEFT JOIN Transfers tf ON t.TransferId = tf.TransferId
    WHERE t.AccountId = @AccountId
    ORDER BY t.CreatedAt DESC
    OFFSET  (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO
 
 
-- ============================================================
--  SECTION 4: VIEWS
-- ============================================================
 
-- Active customers with account summary
CREATE OR ALTER VIEW vw_CustomerAccountSummary AS
SELECT
    u.UserId,
    u.FirstName + N' ' + u.LastName   AS FullName,
    u.Email,
    u.PhoneNumber,
    u.KycStatus,
    u.IsActive,
    u.IsFrozen,
    COUNT(a.AccountId)                 AS TotalAccounts,
    SUM(a.Balance)                     AS TotalBalance,
    SUM(CASE WHEN a.AccountType = N'Savings' THEN 1 ELSE 0 END) AS SavingsAccounts,
    SUM(CASE WHEN a.AccountType = N'Current' THEN 1 ELSE 0 END) AS CurrentAccounts
FROM Users u
LEFT JOIN Accounts a ON u.UserId = a.UserId AND a.Status = N'Active'
JOIN Roles r ON u.RoleId = r.RoleId AND r.RoleName = N'Customer'
GROUP BY u.UserId, u.FirstName, u.LastName, u.Email, u.PhoneNumber,
         u.KycStatus, u.IsActive, u.IsFrozen;
GO
 
-- Loan pipeline for admin/manager dashboard
CREATE OR ALTER VIEW vw_LoanPipeline AS
SELECT
    l.LoanId,
    u.FirstName + N' ' + u.LastName AS CustomerName,
    u.Email                          AS CustomerEmail,
    l.LoanType,
    l.RequestedAmount,
    l.ApprovedAmount,
    l.InterestRate,
    l.TenureMonths,
    l.EMIAmount,
    l.Status,
    l.Purpose,
    l.CreatedAt                      AS AppliedOn,
    l.ReviewedAt,
    rv.FirstName + N' ' + rv.LastName AS ReviewedBy
FROM Loans l
JOIN Users u  ON l.UserId           = u.UserId
LEFT JOIN Users rv ON l.ReviewedByUserId = rv.UserId;
GO
 
-- Daily transaction summary report
CREATE OR ALTER VIEW vw_DailyTransactionSummary AS
SELECT
    CAST(CreatedAt AS DATE)  AS TransactionDate,
    TransactionType,
    COUNT(*)                  AS TotalCount,
    SUM(Amount)               AS TotalAmount,
    AVG(Amount)               AS AverageAmount,
    MIN(Amount)               AS MinAmount,
    MAX(Amount)               AS MaxAmount
FROM Transactions
WHERE Status = N'Success'
GROUP BY CAST(CreatedAt AS DATE), TransactionType;
GO
 
-- Accounts with low balance warning
CREATE OR ALTER VIEW vw_LowBalanceAccounts AS
SELECT
    a.AccountId,
    a.AccountNumber,
    a.AccountType,
    a.Balance,
    a.MinimumBalance,
    a.Balance - a.MinimumBalance AS BufferAmount,
    u.FirstName + N' ' + u.LastName AS CustomerName,
    u.Email,
    u.PhoneNumber
FROM Accounts a
JOIN Users u ON a.UserId = u.UserId
WHERE a.Status = N'Active'
  AND a.Balance <= a.MinimumBalance * 1.1;
GO
 
-- Unread notifications per user
CREATE OR ALTER VIEW vw_UnreadNotifications AS
SELECT
    UserId,
    COUNT(*)          AS UnreadCount,
    MIN(CreatedAt)    AS OldestUnread,
    MAX(CreatedAt)    AS LatestUnread
FROM Notifications
WHERE IsRead = 0
GROUP BY UserId;
GO
 
 
-- ============================================================
--  SECTION 5: SEQUENCE (Account Number Generator)
-- ============================================================
 
CREATE SEQUENCE AccountNumberSeq
    AS INT
    START WITH 1
    INCREMENT BY 1
    NO CYCLE
    CACHE 50;
GO
 
 
-- ============================================================
--  SECTION 6: SEED DATA
-- ============================================================
 
-- Roles
INSERT INTO Roles (RoleName, Description) VALUES
    (N'Customer', N'Bank customer — can use banking services'),
    (N'Teller',   N'Bank teller — supports cash operations'),
    (N'Manager',  N'Branch manager — approves loans and reviews accounts'),
    (N'Admin',    N'System administrator — full control'),
    (N'Auditor',  N'Read-only access to reports and audit logs');
GO
 
-- Admin user (password: Admin@123 — hash must be updated in production)
INSERT INTO Users
    (RoleId, Email, PasswordHash, FirstName, LastName, PhoneNumber, KycStatus, IsEmailVerified)
VALUES
    (4, N'admin@smartbank.com',
     N'$2a$12$PlaceholderHashForAdminUser_ReplaceInProduction',
     N'System', N'Admin', N'9000000000', N'Verified', 1);
GO
 
-- Demo customer
INSERT INTO Users
    (RoleId, Email, PasswordHash, FirstName, LastName, PhoneNumber, NationalId, KycStatus, IsEmailVerified)
VALUES
    (1, N'demo.customer@smartbank.com',
     N'$2a$12$PlaceholderHashForDemoUser_ReplaceInProduction',
     N'Demo', N'Customer', N'9000000001', N'DEMO1234567890', N'Pending', 0);
GO
 
 
-- ============================================================
--  SECTION 7: PERMISSIONS (Application Role)
-- ============================================================
 
-- Create a login for the application service account
-- (Replace password in production; use Azure Key Vault or Secrets Manager)
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'SmartBankAppLogin')
BEGIN
    CREATE LOGIN SmartBankAppLogin WITH PASSWORD = N'Change_This_In_Production!123';
END
GO
 
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'SmartBankApp')
BEGIN
    CREATE USER SmartBankApp FOR LOGIN SmartBankAppLogin;
END
GO
 
-- Grant execute on stored procedures only — never direct table access
GRANT EXECUTE ON sp_RegisterUser          TO SmartBankApp;
GRANT EXECUTE ON sp_OpenAccount           TO SmartBankApp;
GRANT EXECUTE ON sp_Deposit               TO SmartBankApp;
GRANT EXECUTE ON sp_Withdraw              TO SmartBankApp;
GRANT EXECUTE ON sp_TransferFunds         TO SmartBankApp;
GRANT EXECUTE ON sp_ApplyLoan             TO SmartBankApp;
GRANT EXECUTE ON sp_ReviewLoan            TO SmartBankApp;
GRANT EXECUTE ON sp_RaiseTicket           TO SmartBankApp;
GRANT EXECUTE ON sp_SetAccountFreeze      TO SmartBankApp;
GRANT EXECUTE ON sp_GetTransactionHistory TO SmartBankApp;
 
-- Grant SELECT on views
GRANT SELECT ON vw_CustomerAccountSummary  TO SmartBankApp;
GRANT SELECT ON vw_LoanPipeline            TO SmartBankApp;
GRANT SELECT ON vw_DailyTransactionSummary TO SmartBankApp;
GRANT SELECT ON vw_LowBalanceAccounts      TO SmartBankApp;
GRANT SELECT ON vw_UnreadNotifications     TO SmartBankApp;
GO
 
 
-- ============================================================
--  END OF SCRIPT
--  SmartBankDB is ready for Entity Framework Core migrations.
--  Run: dotnet ef migrations add InitialCreate
--       dotnet ef database update
-- ============================================================
PRINT N'SmartBankDB created successfully.';
GO