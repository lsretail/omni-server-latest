-----------------------------
 -----  create login and user
-----------------------------
USE master
GO

BEGIN TRY
    IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'CommerceUser')
    begin
        CREATE LOGIN CommerceUser WITH PASSWORD=N'CommerceUser',  DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;
    end
END TRY 
BEGIN CATCH
     Begin;
      RAISERROR ('Could not create login CommerceUser . Make sure you have SysAdmin permission on this SQL server.', -- Message text.
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
    IF EXISTS (SELECT * FROM sys.database_principals WHERE name = N'CommerceUser')
    begin	
        DROP USER CommerceUser;
    end
    CREATE USER CommerceUser FOR LOGIN CommerceUser WITH DEFAULT_SCHEMA=[dbo];

    EXEC sp_addrolemember N'db_datawriter', N'CommerceUser';
    EXEC sp_addrolemember N'db_datareader', N'CommerceUser';
    GRANT EXECUTE TO  CommerceUser;
END TRY 
BEGIN CATCH
     Begin;
      RAISERROR ('Could not create user CommerceUser  user. Make sure you have SysAdmin permission on this SQL server.', -- Message text.
                   16, -- Severity.
                   1 -- State.
                   );
     End 
END CATCH; 
GO 
