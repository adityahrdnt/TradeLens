CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE instruments (
    "Id" uuid NOT NULL,
    "Symbol" character varying(20) NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Currency" character varying(3) NOT NULL,
    CONSTRAINT "PK_instruments" PRIMARY KEY ("Id")
);

CREATE TABLE positions (
    "Id" uuid NOT NULL,
    "PortfolioId" uuid NOT NULL,
    "InstrumentId" uuid NOT NULL,
    "Quantity" bigint NOT NULL,
    "CostBasis" numeric(18,4) NOT NULL,
    "AveragePrice" numeric(18,4) NOT NULL,
    "Version" bigint NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_positions" PRIMARY KEY ("Id")
);

CREATE TABLE transactions (
    "Id" uuid NOT NULL,
    "PortfolioId" uuid NOT NULL,
    "BrokerAccountId" uuid NOT NULL,
    "InstrumentId" uuid NOT NULL,
    "Type" integer NOT NULL,
    "Quantity" bigint NOT NULL,
    "Price" numeric(18,4) NOT NULL,
    "Fee" numeric(18,4) NOT NULL,
    "TransactionDate" date NOT NULL,
    "Sequence" bigint NOT NULL,
    "Status" integer NOT NULL,
    "SupersedesTransactionId" uuid,
    "CorrectionReason" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid NOT NULL,
    "SupersededAt" timestamp with time zone,
    "SupersededBy" uuid,
    "VoidReason" character varying(500),
    CONSTRAINT "PK_transactions" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_instruments_Symbol" ON instruments ("Symbol");

CREATE UNIQUE INDEX "IX_positions_PortfolioId_InstrumentId" ON positions ("PortfolioId", "InstrumentId");

CREATE INDEX "IX_transactions_PortfolioId_InstrumentId_Status" ON transactions ("PortfolioId", "InstrumentId", "Status");

CREATE INDEX "IX_transactions_PortfolioId_InstrumentId_TransactionDate_Seque~" ON transactions ("PortfolioId", "InstrumentId", "TransactionDate", "Sequence");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260911064952_InitialCreate', '8.0.31');

COMMIT;

START TRANSACTION;

CREATE TABLE portfolios (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    CONSTRAINT "PK_portfolios" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_portfolios_UserId" ON portfolios ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260916085659_AddPortfolio', '8.0.31');

COMMIT;

START TRANSACTION;

ALTER TABLE positions ALTER COLUMN "CostBasis" TYPE numeric(18,8);

ALTER TABLE positions ALTER COLUMN "AveragePrice" TYPE numeric(18,8);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260917044928_IncreasePositionDecimalPrecision', '8.0.31');

COMMIT;

START TRANSACTION;

CREATE TABLE idempotency_records (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "IdempotencyKey" character varying(200) NOT NULL,
    "RequestHash" character varying(64) NOT NULL,
    "TransactionId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_idempotency_records" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_idempotency_records_UserId_IdempotencyKey" ON idempotency_records ("UserId", "IdempotencyKey");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260917052249_AddIdempotencyRecord', '8.0.31');

COMMIT;

START TRANSACTION;

ALTER TABLE idempotency_records ADD "PositionAveragePrice" numeric(18,8) NOT NULL DEFAULT 0.0;

ALTER TABLE idempotency_records ADD "PositionCostBasis" numeric(18,8) NOT NULL DEFAULT 0.0;

ALTER TABLE idempotency_records ADD "PositionId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE idempotency_records ADD "PositionQuantity" bigint NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260917053129_AddIdempotencyResponseSnapshot', '8.0.31');

COMMIT;

START TRANSACTION;

CREATE TABLE market_prices (
    "Id" uuid NOT NULL,
    "InstrumentId" uuid NOT NULL,
    "Price" numeric(18,8) NOT NULL,
    "PriceTimestamp" timestamp with time zone NOT NULL,
    "Source" character varying(50) NOT NULL,
    CONSTRAINT "PK_market_prices" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_market_prices_instruments_InstrumentId" FOREIGN KEY ("InstrumentId") REFERENCES instruments ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_market_prices_InstrumentId_PriceTimestamp" ON market_prices ("InstrumentId", "PriceTimestamp");

CREATE INDEX "IX_market_prices_InstrumentId_Source_PriceTimestamp" ON market_prices ("InstrumentId", "Source", "PriceTimestamp");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260921044438_AddMarketPrice', '8.0.31');

COMMIT;

START TRANSACTION;

CREATE TABLE corporate_actions (
    "Id" uuid NOT NULL,
    "InstrumentId" uuid NOT NULL,
    "Type" integer NOT NULL,
    "Numerator" integer NOT NULL,
    "Denominator" integer NOT NULL,
    "RecordDate" date NOT NULL,
    "ExDate" date NOT NULL,
    "EffectiveDate" date NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" uuid NOT NULL,
    "AppliedAt" timestamp with time zone,
    "AppliedBy" uuid,
    "CancelledAt" timestamp with time zone,
    "CancelledBy" uuid,
    "CancellationReason" character varying(500),
    CONSTRAINT "PK_corporate_actions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_corporate_actions_instruments_InstrumentId" FOREIGN KEY ("InstrumentId") REFERENCES instruments ("Id") ON DELETE RESTRICT
);

CREATE TABLE corporate_action_applications (
    "Id" uuid NOT NULL,
    "CorporateActionId" uuid NOT NULL,
    "PortfolioId" uuid NOT NULL,
    "InstrumentId" uuid NOT NULL,
    "EligibleQuantity" bigint NOT NULL,
    "ResultingQuantity" bigint NOT NULL,
    "AppliedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_corporate_action_applications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_corporate_action_applications_corporate_actions_CorporateAc~" FOREIGN KEY ("CorporateActionId") REFERENCES corporate_actions ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_corporate_action_applications_instruments_InstrumentId" FOREIGN KEY ("InstrumentId") REFERENCES instruments ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_corporate_action_applications_portfolios_PortfolioId" FOREIGN KEY ("PortfolioId") REFERENCES portfolios ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_corporate_action_applications_CorporateActionId_PortfolioId" ON corporate_action_applications ("CorporateActionId", "PortfolioId");

CREATE INDEX "IX_corporate_action_applications_InstrumentId" ON corporate_action_applications ("InstrumentId");

CREATE INDEX "IX_corporate_action_applications_PortfolioId_InstrumentId_Appl~" ON corporate_action_applications ("PortfolioId", "InstrumentId", "AppliedAt");

CREATE INDEX "IX_corporate_actions_InstrumentId_EffectiveDate_Status" ON corporate_actions ("InstrumentId", "EffectiveDate", "Status");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260930103655_AddCorporateActions', '8.0.31');

COMMIT;

