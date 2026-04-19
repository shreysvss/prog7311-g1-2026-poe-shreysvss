IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Clients] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [ContactEmail] nvarchar(max) NOT NULL,
    [ContactPhone] nvarchar(max) NOT NULL,
    [Region] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Contracts] (
    [Id] int NOT NULL IDENTITY,
    [ClientId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [ServiceLevel] int NOT NULL,
    [SignedAgreementPath] nvarchar(max) NULL,
    [SignedAgreementFileName] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Contracts_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ServiceRequests] (
    [Id] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [CostUSD] decimal(18,2) NOT NULL,
    [CostZAR] decimal(18,2) NOT NULL,
    [ExchangeRateUsed] decimal(18,4) NOT NULL,
    [Status] int NOT NULL,
    [RequestedOn] datetime2 NOT NULL,
    CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ServiceRequests_Contracts_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [Contracts] ([Id]) ON DELETE CASCADE
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ContactEmail', N'ContactPhone', N'Name', N'Region') AND [object_id] = OBJECT_ID(N'[Clients]'))
    SET IDENTITY_INSERT [Clients] ON;
INSERT INTO [Clients] ([Id], [ContactEmail], [ContactPhone], [Name], [Region])
VALUES (1, N'freight@globalco.com', N'+27112345678', N'Global Freight Co', N'Africa'),
(2, N'ops@eulogistics.eu', N'+441234567890', N'EU Logistics Ltd', N'Europe');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ContactEmail', N'ContactPhone', N'Name', N'Region') AND [object_id] = OBJECT_ID(N'[Clients]'))
    SET IDENTITY_INSERT [Clients] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ClientId', N'CreatedAt', N'EndDate', N'ServiceLevel', N'SignedAgreementFileName', N'SignedAgreementPath', N'StartDate', N'Status', N'Title') AND [object_id] = OBJECT_ID(N'[Contracts]'))
    SET IDENTITY_INSERT [Contracts] ON;
INSERT INTO [Contracts] ([Id], [ClientId], [CreatedAt], [EndDate], [ServiceLevel], [SignedAgreementFileName], [SignedAgreementPath], [StartDate], [Status], [Title])
VALUES (1, 1, '2024-01-01T00:00:00.0000000', '2026-12-31T00:00:00.0000000', 2, NULL, NULL, '2024-01-01T00:00:00.0000000', 1, N'SA Distribution Contract'),
(2, 2, '2023-06-01T00:00:00.0000000', '2024-05-31T00:00:00.0000000', 1, NULL, NULL, '2023-06-01T00:00:00.0000000', 2, N'EU Express Freight');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ClientId', N'CreatedAt', N'EndDate', N'ServiceLevel', N'SignedAgreementFileName', N'SignedAgreementPath', N'StartDate', N'Status', N'Title') AND [object_id] = OBJECT_ID(N'[Contracts]'))
    SET IDENTITY_INSERT [Contracts] OFF;
GO

CREATE INDEX [IX_Contracts_ClientId] ON [Contracts] ([ClientId]);
GO

CREATE INDEX [IX_ServiceRequests_ContractId] ON [ServiceRequests] ([ContractId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260409201227_InitialCreate', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ServiceRequests] ADD [SourceCurrency] nvarchar(3) NOT NULL DEFAULT N'';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260418171924_AddSourceCurrencyToServiceRequest', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

EXEC sp_rename N'[ServiceRequests].[CostUSD]', N'Cost', N'COLUMN';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260418174159_RenameCostUSDtoCost', N'8.0.0');
GO

COMMIT;
GO

