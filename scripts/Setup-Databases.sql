USE [master]
GO

:setvar DatabaseName "Ticketing"
IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'$(DatabaseName)')
CREATE DATABASE [N'$(DatabaseName)'] ON ( NAME = N'$(DatabaseName)', FILENAME = N'$(UserPath)\$(DatabaseName).mdf' )
GO

:setvar DatabaseName "Finance"
IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'$(DatabaseName)')
CREATE DATABASE [N'$(DatabaseName)'] ON ( NAME = N'$(DatabaseName)', FILENAME = N'$(UserPath)\$(DatabaseName).mdf' )
GO

:setvar DatabaseName "Reservations"
IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'$(DatabaseName)')
CREATE DATABASE [N'$(DatabaseName)'] ON ( NAME = N'$(DatabaseName)', FILENAME = N'$(UserPath)\$(DatabaseName).mdf' )
GO