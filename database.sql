USE master
GO

IF NOT EXISTS (
  SELECT name
    FROM sys.databases
    WHERE name = N'Machinery'
)
CREATE DATABASE Machinery
GO

USE Machinery;
GO

IF OBJECT_ID('InferenceLog', 'U') IS NOT NULL
DROP TABLE InferenceLog
GO
CREATE TABLE InferenceLog (
    Id INT PRIMARY KEY IDENTITY (1, 1),
    ClassName NVARCHAR(100),
    Score FLOAT,
    Machine NVARCHAR(100),
    Timestamp DATETIME
);
GO
