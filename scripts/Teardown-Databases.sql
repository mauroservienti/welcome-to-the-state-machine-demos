USE [master]
GO

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'Tickets')
DROP DATABASE [Sales]
GO