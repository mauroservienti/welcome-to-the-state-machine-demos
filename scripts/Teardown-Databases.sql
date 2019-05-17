USE [master]
GO

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'Ticketing')
DROP DATABASE [Ticketing]
GO

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'Finance')
DROP DATABASE [Finance]
GO