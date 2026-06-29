using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportCourtManagent_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddComplexCourtTypeServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('Services', 'CreatedAt') IS NULL
                    ALTER TABLE [Services] ADD [CreatedAt] datetime2 NOT NULL CONSTRAINT DF_Services_CreatedAt DEFAULT (GETUTCDATE());

                IF COL_LENGTH('Services', 'Description') IS NULL
                    ALTER TABLE [Services] ADD [Description] nvarchar(300) NULL;

                IF COL_LENGTH('Services', 'IsActive') IS NULL
                    ALTER TABLE [Services] ADD [IsActive] bit NOT NULL CONSTRAINT DF_Services_IsActive DEFAULT (1);

                IF COL_LENGTH('Services', 'Unit') IS NULL
                    ALTER TABLE [Services] ADD [Unit] nvarchar(30) NOT NULL CONSTRAINT DF_Services_Unit DEFAULT (N'cái');
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[ComplexCourtTypeServices]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [ComplexCourtTypeServices] (
                        [OfferingId] int NOT NULL IDENTITY,
                        [ComplexId] int NOT NULL,
                        [CourtTypeId] int NOT NULL,
                        [ServiceId] int NOT NULL,
                        [Price] decimal(18,2) NOT NULL,
                        [StockQty] int NOT NULL,
                        [ServiceMode] int NOT NULL,
                        [IsActive] bit NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_ComplexCourtTypeServices] PRIMARY KEY ([OfferingId]),
                        CONSTRAINT [FK_ComplexCourtTypeServices_CourtComplexes_ComplexId] FOREIGN KEY ([ComplexId]) REFERENCES [CourtComplexes] ([ComplexId]) ON DELETE CASCADE,
                        CONSTRAINT [FK_ComplexCourtTypeServices_CourtTypes_CourtTypeId] FOREIGN KEY ([CourtTypeId]) REFERENCES [CourtTypes] ([CourtTypeId]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_ComplexCourtTypeServices_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE NO ACTION
                    );

                    CREATE UNIQUE INDEX [IX_ComplexCourtTypeServices_ComplexId_CourtTypeId_ServiceId]
                        ON [ComplexCourtTypeServices] ([ComplexId], [CourtTypeId], [ServiceId]);

                    CREATE INDEX [IX_ComplexCourtTypeServices_CourtTypeId]
                        ON [ComplexCourtTypeServices] ([CourtTypeId]);

                    CREATE INDEX [IX_ComplexCourtTypeServices_ServiceId]
                        ON [ComplexCourtTypeServices] ([ServiceId]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplexCourtTypeServices");

            migrationBuilder.Sql("""
                IF COL_LENGTH('Services', 'CreatedAt') IS NOT NULL
                    ALTER TABLE [Services] DROP COLUMN [CreatedAt];
                IF COL_LENGTH('Services', 'Description') IS NOT NULL
                    ALTER TABLE [Services] DROP COLUMN [Description];
                IF COL_LENGTH('Services', 'IsActive') IS NOT NULL
                    ALTER TABLE [Services] DROP COLUMN [IsActive];
                IF COL_LENGTH('Services', 'Unit') IS NOT NULL
                    ALTER TABLE [Services] DROP COLUMN [Unit];
                """);
        }
    }
}
