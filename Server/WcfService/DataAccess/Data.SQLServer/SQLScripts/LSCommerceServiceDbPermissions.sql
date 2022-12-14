-----------------------------
 -----  create login and user
-----------------------------
USE master
GO

BEGIN TRY
    IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'LSCommerceUser')
    begin
        CREATE LOGIN LSCommerceUser WITH PASSWORD=N'LSCommerceUser',  DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;
    end
END TRY 
BEGIN CATCH
     Begin;
      RAISERROR ('Could not create login LSCommerceUser . Make sure you have SysAdmin permission on this SQL server.', -- Message text.
                   16, -- Severity.
                   1 -- State.
                   );
     End 
END CATCH; 
GO

USE [THEDATABASE]
GO 

BEGIN TRY
    -- drop the user to the internal id match the login
    IF EXISTS (SELECT * FROM sys.database_principals WHERE name = N'LSCommerceUser')
    begin	
        DROP USER LSCommerceUser;
    end
    CREATE USER LSCommerceUser FOR LOGIN LSCommerceUser WITH DEFAULT_SCHEMA=[dbo];

    EXEC sp_addrolemember N'db_datawriter', N'LSCommerceUser';
    EXEC sp_addrolemember N'db_datareader', N'LSCommerceUser';
    GRANT EXECUTE TO  LSCommerceUser;
END TRY 
BEGIN CATCH
     Begin;
      RAISERROR ('Could not create user LSCommerceUser  user. Make sure you have SysAdmin permission on this SQL server.', -- Message text.
                   16, -- Severity.
                   1 -- State.
                   );
     End 
END CATCH; 
GO 
