# Database Migration Fix Instructions

The migration partially applied (Notifications table was created, but ReviewDecisions failed due to cascade path conflict).

## Option 1: Rollback and Reapply (Recommended)

1. **Rollback the migration:**
   ```
   dotnet ef database update 20251130191903_InitialCreate
   ```
   This will remove the Notifications table.

2. **Reapply the fixed migration:**
   ```
   dotnet ef database update
   ```

## Option 2: Manual Fix (If Option 1 doesn't work)

1. **Run the SQL script** in SQL Server Management Studio or via command line:
   ```sql
   -- Connect to your database and run:
   USE EthicaFlowDb;
   GO
   
   -- Drop ReviewDecisions if it exists
   IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReviewDecisions]') AND type in (N'U'))
   BEGIN
       DROP TABLE [dbo].[ReviewDecisions];
   END
   GO
   
   -- Create ReviewDecisions table with NO ACTION
   CREATE TABLE [ReviewDecisions] (
       [DecisionId] int NOT NULL IDENTITY,
       [SubmissionId] int NOT NULL,
       [ReviewerId] int NOT NULL,
       [Decision] nvarchar(max) NOT NULL,
       [Comments] nvarchar(max) NOT NULL,
       [DecisionDate] datetime2 NOT NULL,
       CONSTRAINT [PK_ReviewDecisions] PRIMARY KEY ([DecisionId]),
       CONSTRAINT [FK_ReviewDecisions_EthicsSubmissions_SubmissionId] 
           FOREIGN KEY ([SubmissionId]) REFERENCES [EthicsSubmissions] ([SubmissionId]) 
           ON DELETE CASCADE,
       CONSTRAINT [FK_ReviewDecisions_Users_ReviewerId] 
           FOREIGN KEY ([ReviewerId]) REFERENCES [Users] ([UserId]) 
           ON DELETE NO ACTION
   );
   GO
   
   -- Create indexes
   CREATE INDEX [IX_ReviewDecisions_ReviewerId] ON [ReviewDecisions] ([ReviewerId]);
   CREATE INDEX [IX_ReviewDecisions_SubmissionId] ON [ReviewDecisions] ([SubmissionId]);
   GO
   ```

2. **Mark the migration as applied:**
   ```
   dotnet ef migrations script --idempotent
   ```
   Or manually insert into __EFMigrationsHistory:
   ```sql
   INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
   VALUES ('20251130195805_AddReviewDecisionAndNotification', '8.0.0');
   ```

## What Was Fixed

The issue was that SQL Server doesn't allow multiple cascade delete paths. The ReviewDecisions table had:
- CASCADE delete on ReviewerId → Users
- CASCADE delete on SubmissionId → EthicsSubmissions → Users (via ResearcherId)

This created a conflict. The fix changes the ReviewerId foreign key to use NO ACTION instead of CASCADE, which prevents the cascade path conflict while still maintaining referential integrity.

