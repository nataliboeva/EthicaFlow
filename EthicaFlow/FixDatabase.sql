-- Fix for ReviewDecisions table creation
-- This script manually creates the ReviewDecisions table with NO ACTION on delete
-- to avoid multiple cascade paths error

-- First, check if the table already exists and drop it if needed
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReviewDecisions]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[ReviewDecisions];
END

-- Create the ReviewDecisions table with correct foreign key constraints
CREATE TABLE [ReviewDecisions] (
    [DecisionId] int NOT NULL IDENTITY,
    [SubmissionId] int NOT NULL,
    [ReviewerId] int NOT NULL,
    [Decision] nvarchar(max) NOT NULL,
    [Comments] nvarchar(max) NOT NULL,
    [DecisionDate] datetime2 NOT NULL,
    CONSTRAINT [PK_ReviewDecisions] PRIMARY KEY ([DecisionId]),
    CONSTRAINT [FK_ReviewDecisions_EthicsSubmissions_SubmissionId] FOREIGN KEY ([SubmissionId]) REFERENCES [EthicsSubmissions] ([SubmissionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReviewDecisions_Users_ReviewerId] FOREIGN KEY ([ReviewerId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

-- Create indexes
CREATE INDEX [IX_ReviewDecisions_ReviewerId] ON [ReviewDecisions] ([ReviewerId]);
CREATE INDEX [IX_ReviewDecisions_SubmissionId] ON [ReviewDecisions] ([SubmissionId]);

