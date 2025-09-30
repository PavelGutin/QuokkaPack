-- Development Database Initialization Script
-- This script sets up the development database with initial configuration

USE master;
GO

-- Create the QuokkaPackDb database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'QuokkaPackDb')
BEGIN
    CREATE DATABASE QuokkaPackDb;
    PRINT 'QuokkaPackDb database created successfully.';
END
ELSE
BEGIN
    PRINT 'QuokkaPackDb database already exists.';
END
GO

-- Switch to the QuokkaPackDb database
USE QuokkaPackDb;
GO

-- Enable development-friendly settings
ALTER DATABASE QuokkaPackDb SET READ_COMMITTED_SNAPSHOT ON;
ALTER DATABASE QuokkaPackDb SET ALLOW_SNAPSHOT_ISOLATION ON;
GO

PRINT 'Development database initialization completed.';
GO