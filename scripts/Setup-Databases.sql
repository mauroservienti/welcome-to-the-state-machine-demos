USE [master]
GO

:setvar DatabaseName "Ticketing"
IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = '$(DatabaseName)')
CREATE DATABASE [$(DatabaseName)] ON ( NAME = '$(DatabaseName)', FILENAME = '$(UserPath)\$(DatabaseName).mdf' )
GO

:setvar DatabaseName "Finance"
IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = '$(DatabaseName)')
CREATE DATABASE [$(DatabaseName)] ON ( NAME = '$(DatabaseName)', FILENAME = '$(UserPath)\$(DatabaseName).mdf' )
GO

:setvar DatabaseName "Finance.Service"
IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = '$(DatabaseName)')
CREATE DATABASE [$(DatabaseName)] ON ( NAME = '$(DatabaseName)', FILENAME = '$(UserPath)\$(DatabaseName).mdf' )
GO

:setvar DatabaseName "Reservations"
IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = '$(DatabaseName)')
CREATE DATABASE [$(DatabaseName)] ON ( NAME = '$(DatabaseName)', FILENAME = '$(UserPath)\$(DatabaseName).mdf' )
GO

:setvar DatabaseName "Reservations.Service"
IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = '$(DatabaseName)')
CREATE DATABASE [$(DatabaseName)] ON ( NAME = '$(DatabaseName)', FILENAME = '$(UserPath)\$(DatabaseName).mdf' )
GO

:setvar DatabaseName "Shipping.Service"
IF  NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = '$(DatabaseName)')
CREATE DATABASE [$(DatabaseName)] ON ( NAME = '$(DatabaseName)', FILENAME = '$(UserPath)\$(DatabaseName).mdf' )
GO