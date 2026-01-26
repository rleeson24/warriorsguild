USE [Warriorsguild]
GO

-- Declare the variable to be used.
DECLARE @UserId uniqueidentifier;

-- Initialize the variable.
SET @UserId = N'6d906802-6875-4372-a3f4-44067017da8a';

delete from [dbo].[CrossAnswers] where UserId = @UserId;
delete from [dbo].[CrossDayAnswers] where UserId = @UserId;
delete from [dbo].[CrossApprovals] where UserId = @UserId;
delete from [dbo].[CrossDayStatuses] where UserId = @UserId;
delete from [dbo].[PinnedRings] where UserId = @UserId;
delete from [dbo].[ProofOfCompletionAttachments] where UserId = @UserId;
delete from [dbo].[RankApprovals] where UserId = @UserId;
delete from [dbo].[RankStatusCrosses] where UserId = @UserId;
delete from [dbo].[RankstatusRings] where UserId = @UserId;
delete from [dbo].[RankStatuses] where UserId = @UserId;
delete from [dbo].[RingApprovals] where UserId = @UserId;
delete from [dbo].[ringstatuses] where UserId = @UserId;