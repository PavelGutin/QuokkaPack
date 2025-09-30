-- Production Database Initialization Script
-- This script sets up the production database with optimized settings

USE master;
GO

-- Create the database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'QuokkaPackDb')
BEGIN
    CREATE DATABASE QuokkaPackDb
    ON (
        NAME = 'QuokkaPackDb_Data',
        FILENAME = '/var/opt/mssql/data/QuokkaPackDb.mdf',
        SIZE = 100MB,
        MAXSIZE = 1GB,
        FILEGROWTH = 10MB
    )
    LOG ON (
        NAME = 'QuokkaPackDb_Log',
        FILENAME = '/var/opt/mssql/log/QuokkaPackDb.ldf',
        SIZE = 10MB,
        MAXSIZE = 100MB,
        FILEGROWTH = 5MB
    );
END
GO

-- Set database options for production
ALTER DATABASE QuokkaPackDb SET RECOVERY FULL;
ALTER DATABASE QuokkaPackDb SET AUTO_CLOSE OFF;
ALTER DATABASE QuokkaPackDb SET AUTO_SHRINK OFF;
ALTER DATABASE QuokkaPackDb SET AUTO_CREATE_STATISTICS ON;
ALTER DATABASE QuokkaPackDb SET AUTO_UPDATE_STATISTICS ON;
GO

-- Create backup device for automated backups
IF NOT EXISTS (SELECT name FROM sys.backup_devices WHERE name = 'QuokkaPackDb_Backup')
BEGIN
    EXEC sp_addumpdevice 'disk', 'QuokkaPackDb_Backup', '/var/opt/mssql/backup/QuokkaPackDb.bak';
END
GO

PRINT 'Production database initialization completed successfully.';
GO