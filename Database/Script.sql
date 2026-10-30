-- ============================================================================
-- SMART TICKETING SYSTEM - TICKET VENDOR MACHINE (TVM)
-- Database Engine: Microsoft SQL Server (MSSQL Server)
-- Schema Definition and Seed Data Script
-- Synchronization: Figma UI/UX "MetroLink - Riverside Line" (USD Currency)
-- Author: Nguyen Nhat Huy (Student ID: 524H0052) - Member 3
-- Group: Group 14 | Software Engineering Lab
-- ============================================================================

USE master;
GO

-- 1. Create Database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SmartTicketingDB')
BEGIN
    CREATE DATABASE SmartTicketingDB;
    PRINT 'Database SmartTicketingDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database SmartTicketingDB already exists.';
END
GO

USE SmartTicketingDB;
GO

-- 2. Drop existing foreign keys and tables if recreating
IF OBJECT_ID('dbo.Tickets', 'U') IS NOT NULL DROP TABLE dbo.Tickets;
IF OBJECT_ID('dbo.PaymentTransactions', 'U') IS NOT NULL DROP TABLE dbo.PaymentTransactions;
IF OBJECT_ID('dbo.Stations', 'U') IS NOT NULL DROP TABLE dbo.Stations;
IF OBJECT_ID('dbo.TVMMachines', 'U') IS NOT NULL DROP TABLE dbo.TVMMachines;
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
GO

-- ============================================================================
-- 3. Table: Stations
-- Matches Domain Model & Figma UI: Riverside Line [01]
-- ============================================================================
CREATE TABLE dbo.Stations (
    StationId INT IDENTITY(1,1) PRIMARY KEY,
    StationName NVARCHAR(100) NOT NULL UNIQUE,
    LineName NVARCHAR(50) NOT NULL DEFAULT 'Riverside Line [01]',
    BaseFare DECIMAL(10,2) NOT NULL DEFAULT 3.00,
    StopOrder INT NOT NULL,
    EstimatedMinutes INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsPopular BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ============================================================================
-- 4. Table: TVMMachines
-- Represents the hardware kiosk unit
-- ============================================================================
CREATE TABLE dbo.TVMMachines (
    MachineId NVARCHAR(50) PRIMARY KEY,
    Location NVARCHAR(100) NOT NULL,
    PrinterModel NVARCHAR(100) NOT NULL DEFAULT 'Citizen CT-S310II Thermal Barcode Printer',
    DevicePort NVARCHAR(20) NOT NULL DEFAULT 'COM3',
    RemainingPaperUnits INT NOT NULL DEFAULT 500,
    IsInService BIT NOT NULL DEFAULT 1,
    LastHeartbeat DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ============================================================================
-- 5. Table: PaymentTransactions
-- Stores payment audit records (Credit Card & Digital Wallet QR)
-- ============================================================================
CREATE TABLE dbo.PaymentTransactions (
    TransactionId NVARCHAR(64) PRIMARY KEY,
    Amount DECIMAL(10,2) NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    PaymentMethod NVARCHAR(30) NOT NULL,     -- 'CreditCard' or 'DigitalWallet'
    Status NVARCHAR(20) NOT NULL,            -- 'SUCCESS', 'DECLINED', 'TIMEOUT', 'REFUNDED'
    MaskedCardNumber NVARCHAR(20) NULL,      -- e.g. '•••• •••• •••• 4242'
    CardHolderName NVARCHAR(100) NULL,       -- e.g. 'JOHN DOE'
    BankApprovalCode NVARCHAR(50) NULL,      -- e.g. 'AUTH-892401'
    WalletProvider NVARCHAR(50) NULL,        -- 'Momo', 'VNPay', 'ApplePay'
    QRCodeData NVARCHAR(500) NULL,           -- Raw QR Payload
    GatewayReferenceId NVARCHAR(100) NULL,   -- e.g. 'GW-TXN-991204'
    MachineId NVARCHAR(50) NOT NULL FOREIGN KEY REFERENCES dbo.TVMMachines(MachineId)
);
GO

-- ============================================================================
-- 6. Table: Tickets
-- Stores generated physical barcode tickets
-- ============================================================================
CREATE TABLE dbo.Tickets (
    TicketId NVARCHAR(64) PRIMARY KEY,
    Barcode NVARCHAR(100) NOT NULL UNIQUE,   -- e.g. 'TKT-2026-0942'
    OriginStationId INT NOT NULL FOREIGN KEY REFERENCES dbo.Stations(StationId),
    DestinationStationId INT NOT NULL FOREIGN KEY REFERENCES dbo.Stations(StationId),
    FareCategory NVARCHAR(20) NOT NULL,      -- 'Adult' ($3.00), 'Child' ($1.50), 'Senior' ($1.50)
    Price DECIMAL(10,2) NOT NULL,
    IssueTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'VALID', -- 'VALID', 'USED', 'REFUNDED'
    TransactionId NVARCHAR(64) NOT NULL FOREIGN KEY REFERENCES dbo.PaymentTransactions(TransactionId)
);
GO

-- ============================================================================
-- 7. Table: AuditLogs
-- Records hardware status, errors, and system events
-- ============================================================================
CREATE TABLE dbo.AuditLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    MachineId NVARCHAR(50) NOT NULL,
    EventType NVARCHAR(50) NOT NULL,         -- 'TRANSACTION_SUCCESS', 'PAYMENT_DECLINED', 'QR_TIMEOUT', 'PRINTER_JAM'
    Severity NVARCHAR(20) NOT NULL,          -- 'INFO', 'WARNING', 'ERROR'
    Message NVARCHAR(MAX) NOT NULL
);
GO

-- ============================================================================
-- 8. Seed Data (Riverside Line Stations matching Figma UI)
-- ============================================================================
INSERT INTO dbo.TVMMachines (MachineId, Location, PrinterModel, DevicePort, RemainingPaperUnits, IsInService)
VALUES ('TVM-01', 'Central Station - Main Concourse', 'Citizen CT-S310II Thermal', 'COM3', 450, 1);

INSERT INTO dbo.Stations (StationName, LineName, BaseFare, StopOrder, EstimatedMinutes, IsActive, IsPopular)
VALUES 
    ('Central',       'Riverside Line [01]', 0.00, 0, 0,  1, 0), -- Origin
    ('Market Street', 'Riverside Line [01]', 3.00, 1, 3,  1, 1), -- Popular
    ('Civic Square',  'Riverside Line [01]', 3.00, 2, 6,  1, 0),
    ('City Park',     'Riverside Line [01]', 3.00, 3, 9,  1, 0),
    ('Museum',        'Riverside Line [01]', 3.00, 4, 11, 1, 0),
    ('Riverside',     'Riverside Line [01]', 3.00, 5, 14, 1, 1), -- Popular / Primary Target
    ('Harbour',       'Riverside Line [01]', 3.00, 6, 18, 1, 1); -- Popular

-- Insert Sample Audit Log
INSERT INTO dbo.AuditLogs (MachineId, EventType, Severity, Message)
VALUES ('TVM-01', 'SYSTEM_STARTUP', 'INFO', 'TVM application initialized on Central Station - Main Concourse kiosk.');
GO

-- ============================================================================
-- 9. Stored Procedures for System Operations
-- ============================================================================

-- Stored Procedure: Get Active Route Stations
CREATE OR ALTER PROCEDURE dbo.sp_GetStations
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StationId, StationName, LineName, BaseFare, StopOrder, EstimatedMinutes, IsActive, IsPopular
    FROM dbo.Stations
    WHERE IsActive = 1
    ORDER BY StopOrder ASC;
END;
GO

-- Stored Procedure: Record Completed Purchase
CREATE OR ALTER PROCEDURE dbo.sp_RecordPurchaseTransaction
    @TransactionId NVARCHAR(64),
    @Amount DECIMAL(10,2),
    @PaymentMethod NVARCHAR(30),
    @Status NVARCHAR(20),
    @MaskedCardNumber NVARCHAR(20) = NULL,
    @CardHolderName NVARCHAR(100) = NULL,
    @BankApprovalCode NVARCHAR(50) = NULL,
    @WalletProvider NVARCHAR(50) = NULL,
    @GatewayReferenceId NVARCHAR(100) = NULL,
    @MachineId NVARCHAR(50) = 'TVM-01'
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO dbo.PaymentTransactions (
            TransactionId, Amount, PaymentMethod, Status, 
            MaskedCardNumber, CardHolderName, BankApprovalCode, 
            WalletProvider, GatewayReferenceId, MachineId
        )
        VALUES (
            @TransactionId, @Amount, @PaymentMethod, @Status,
            @MaskedCardNumber, @CardHolderName, @BankApprovalCode,
            @WalletProvider, @GatewayReferenceId, @MachineId
        );

        INSERT INTO dbo.AuditLogs (MachineId, EventType, Severity, Message)
        VALUES (@MachineId, 'TRANSACTION_' + @Status, 
            CASE WHEN @Status = 'SUCCESS' THEN 'INFO' ELSE 'WARNING' END,
            CONCAT('Payment transaction ', @TransactionId, ' completed with status: ', @Status, ', Amount: $', @Amount));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

PRINT 'MSSQL Database Script initialization completed successfully.';
GO
