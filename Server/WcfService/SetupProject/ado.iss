[Code]
// NOTE, the sql scrips understand /* */  but not -- as comments after text
//ok to start with -- but not as comments after column names for example
const
  adCmdUnspecified = $FFFFFFFF;
  adCmdUnknown = $00000008;
  adCmdText = $00000001;
  adCmdTable = $00000002;
  adCmdStoredProc = $00000004;
  adCmdFile = $00000100;
  adCmdTableDirect = $00000200;
  adOptionUnspecified = $FFFFFFFF;
  adAsyncExecute = $00000010;
  adAsyncFetch = $00000020;
  adAsyncFetchNonBlocking = $00000040;
  adExecuteNoRecords = $00000080;
  adExecuteStream = $00000400;
  adExecuteRecord = $00000800;

var
  ADOGlobalCommand: Variant;
  ADOGlobalConnection: Variant;
  ADOGlobalDbName: string;

function ADOGetConnectionString(dbServer: string; dbName: string; userName: string; pwd: string; windowsAuth: Boolean): Variant;
var  
  ADOConnectionString: string;  
begin
  dbServer := Trim(dbServer);
  dbName := Trim(dbName);
  userName := Trim(userName);
  pwd := Trim(pwd);
  ADOGlobalDbName   := dbName;  //save for later
  
  // build a connection string; 
  ADOConnectionString := 
    'Provider=SQLOLEDB;' +               // provider
    'Data Source=' + dbServer + ';' +   // server name
    'Application Name= Inno Setup Execute SQL;' ; 
  
  if (Length(dbName) > 0)  then 
  begin
    ADOConnectionString := ADOConnectionString +
    'Initial Catalog=' + dbName + ';' // db name
  end;
     
  if windowsAuth then
    ADOConnectionString := ADOConnectionString + 'Integrated Security=SSPI;' // Windows Auth
  else
    ADOConnectionString := ADOConnectionString + 'User Id=' + userName + ';' + 'Password=' + pwd + ';';

  // open the connection by the assigned ConnectionString
  Result := ADOConnectionString;
end; 

procedure ADOInit(dbServer: string; dbName: string; userName: string; pwd: string; windowsAuth: Boolean);
begin
  // JIJ dont try to understand this, but best to create object 1x and keep open.
  // got access exception when I tried to have this local in functions !
  // SO keep it open and never close a connection to sql server!
  Log('Init ADOGlobalConnection called');
  ADOGlobalConnection := CreateOleObject('ADODB.Connection') ;
  ADOGlobalConnection.ConnectionString := ADOGetConnectionString(dbServer,dbName,userName,pwd,windowsAuth); 
  ADOGlobalConnection.Open;

  Log('Init ADOGlobalCommand called');
  ADOGlobalCommand := CreateOleObject('ADODB.Command') ;
  ADOGlobalCommand.ActiveConnection := ADOGlobalConnection; 
end;

// Check if full text search is enabled
Function ADOCheckFullTextSearch() : Boolean ;
var     
  sqlStr : string;
begin  
  Result := True;
  Log('ADOCheckFullTextSearch called'); 

  try
    sqlStr := 'if (SERVERPROPERTY(''IsFullTextInstalled'') = 0)' +
       ' Begin;  '  +
       ' RAISERROR (''Full-Text search is not installed. Sql Server must have Full-Text Search feature installed to continue...'',  '  +
       '  16, 1 ); End ';

    //switch out the database name for permission scripts assign text of a command to be issued against a provider
    ADOGlobalCommand.CommandText := sqlStr;
    ADOGlobalCommand.CommandTimeout := 30; //10 sec
    ADOGlobalCommand.CommandType := adCmdText;

    // this will execute the command and return dataset
    Log('ADOCheckFullTextSearch Execute ' + sqlStr);
    ADOGlobalCommand.Execute(NULL, NULL, adCmdText or adExecuteNoRecords);
  except
    Log('ADOCheckFullTextSearch failed. ' + GetExceptionMessage); 
    Result := False;
  finally
    //reset so next adoconnection string is correct
    //ADOGlobalConnection := Unassigned;
    //ADOGlobalCommand := Unassigned;
  end;
end;

// Check if sql server mixed authentication is enabled
Function ADOCheckIsMixedAuthentication() : Boolean ;
var     
  sqlStr : string;
begin  
  Result := True;
  Log('ADOCheckIsMixedAuthentication called'); 

  try
    // SELECT CASE SERVERPROPERTY('IsIntegratedSecurityOnly')   
    // WHEN 1 THEN 'Windows Authentication'   
    // WHEN 0 THEN 'Windows and SQL Server Authentication'   
    // END as [Authentication Mode]  

    sqlStr := 'if (SERVERPROPERTY(''IsIntegratedSecurityOnly'') = 1)' +
       ' Begin;  '  +
       ' RAISERROR (''Windows and SQL Server Authentication is not enabled. Sql Server must have it enabled to continue...'',  '  +
       '  16, 1 ); End ';

    //switch out the database name for permission scripts 
    // assign text of a command to be issued against a provider
    ADOGlobalCommand.CommandText := sqlStr;
    ADOGlobalCommand.CommandTimeout := 30; //10 sec
    ADOGlobalCommand.CommandType := adCmdText;
    
	// this will execute the command and return dataset
    Log('ADOCheckIsMixedAuthentication Execute ' + sqlStr);
    ADOGlobalCommand.Execute(NULL, NULL, adCmdText or adExecuteNoRecords);
  except
    Log('ADOCheckIsMixedAuthentication failed. ' + GetExceptionMessage); 
    Result := False;
  finally
    //reset so next adoconnection string is correct
    //ADOGlobalConnection := Unassigned;
    //ADOGlobalCommand := Unassigned;
  end;
end;

// Check if trying to create tables in the NAV database
Function ADOCheckIsNavDB() : Boolean ;
var     
  sqlStr : string;
begin  
  Result := False;
  Log('ADOCheckIsNavDB called'); 
  
  try
    sqlStr := 'IF EXISTS(SELECT * FROM sysobjects WHERE name = N''User Metadata'' and xtype=''U'')' +
       ' Begin;  '  +
       ' RAISERROR (''You are trying to create the LS Omni SQL Server objects in the LS Central database!!'',  '  +
       '  16, 1 ); End ';

    //switch out the database name for permission scripts 
    // assign text of a command to be issued against a provider
    ADOGlobalCommand.CommandText := sqlStr;
    ADOGlobalCommand.CommandTimeout := 30; //10 sec
    ADOGlobalCommand.CommandType := adCmdText;
 
    // this will execute the command and return dataset
    Log('ADOCheckIsNavDB Execute ' + sqlStr);
    ADOGlobalCommand.Execute(NULL, NULL, adCmdText or adExecuteNoRecords);
  except
    Log('ADOCheckIsNavDB failed. ' + GetExceptionMessage); 
    Result := True;
  finally
    //reset so next adoconnection string is correct
    //ADOGlobalConnection := Unassigned;
    //ADOGlobalCommand := Unassigned;
  end;
end;
 
// Execute sql script
procedure ADOExecuteSQL(sqlScript: string);
begin
  Log('ADOExecuteSQL called'); 
  if Length(sqlScript) > 0  then
  begin 
    //switch out the database name for permission scripts
    StringChangeEx(sqlScript, 'THEDATABASE', ADOGlobalDbName, True);  
    // assign text of a command to be issued against a provider
    ADOGlobalCommand.CommandText := sqlScript;
    ADOGlobalCommand.CommandTimeout := 30; //30 sec
    // this property setting means, that you're going to execute the 
    // CommandText text command; it does the same, like if you would
    // use only adCmdText flag in the Execute statement
    ADOGlobalCommand.CommandType := adCmdText;
    // this will execute the command and return dataset
    Log('ADOExecuteSQL Execute ' + sqlScript);
    ADOGlobalCommand.Execute(NULL, NULL, adCmdText or adExecuteNoRecords);
  end;   
end;

// Execute sql script
procedure ADOUpdateAppSettings(keyName: string; newValue: string);
begin
  Log('ADOUpdateAppSettings called'); 

  ADOGlobalCommand.CommandText := 'IF NOT EXISTS (SELECT [LSKey],[Key] FROM [TenantConfig] WHERE [LSKey]='''' AND [Key]=''' + keyname + ''') ' +
                                  'INSERT INTO [TenantConfig] ([LSKey],[Key],[Value],[DataType]) VALUES ('''',''' + keyname + ''',''' + newValue + ''','''') ' + 
								  'ELSE UPDATE [TenantConfig] SET [Value]=''' + newValue + ''' WHERE [LSKey]='''' AND [Key]=''' + keyname + '''';

  ADOGlobalCommand.CommandTimeout := 30;
  ADOGlobalCommand.CommandType := adCmdText;
  Log('ADOExecuteSQL Execute ' + ADOGlobalCommand.CommandText);
  ADOGlobalCommand.Execute(NULL, NULL, adCmdText or adExecuteNoRecords);
end;


function ADOCreateDb(dbServer: string; dbName: string; userName: string; pwd: string; windowsAuth: Boolean): Boolean;
var  
  sql: string;  
  dbLog : string;
begin
  Log('ADOCreateDb called> Srv:' + dbServer + ' Usr:' + userName + ' Pwd:' + pwd);
  Result := True;
  //dont pass in the dbname since it is not to be used
  ADOInit(dbServer,'', userName, pwd, windowsAuth);

  //create the database if needed
  dbName := Trim(dbName);
  dbLog := dbName + '_log';
  sql := 'USE [master]; IF NOT EXISTS (SELECT name FROM master.sys.databases WHERE name = ''' + dbName + ''')' + 
         ' CREATE DATABASE ['+ dbName + '] COLLATE Latin1_General_CI_AS; ' +
         ' ALTER DATABASE [' + dbName + '] MODIFY FILE  ( NAME = N''' + dbName + ''', FILEGROWTH = 10% ); ' +
	     ' ALTER DATABASE [' + dbName + '] MODIFY FILE ( NAME = N''' + dbLog + ''' , FILEGROWTH = 10%); ' +
	     ' USE [master]; '

  //Log('ADOCreateDb ' + sql);
  ADOExecuteSQL(sql);

  //reset so next adoconnection string is correct
  ADOGlobalConnection := Unassigned;
  ADOGlobalCommand := Unassigned;
end; { SqlCreateDb }

//read the sqlscript file into a list, breaking up at the GO line
function ADOLoadScriptFromFile(const FileName: string;  out CommandList: TStringList): Integer;
var
  I: Integer;
  SQLCommand: string;
  ScriptFile: TStringList;
begin
  Result := 0;
  ScriptFile := TStringList.Create;
  
  try
    SQLCommand := '';
    Log('ADOLoadScriptFromFile called: ' + FileName);
    ExtractTemporaryFile(FileName) ;

    ScriptFile.LoadFromFile(ExpandConstant('{tmp}\' + FileName));
    Log('ADOLoadScriptFromFile  ScriptFile.Count: '+ IntToStr(ScriptFile.Count) );
    
	for I := 0 to ScriptFile.Count - 1 do
    begin
      if Pos('go', LowerCase(Trim(ScriptFile[I]))) = 1 then
      begin
        Result := Result + 1;
        CommandList.Add(SQLCommand);
        SQLCommand := '';
      end
      else
        SQLCommand := SQLCommand + ' ' + ScriptFile[I] + ' '#10#13;
    end;

    //EDIT: If there is no GO syntax present int the file AND
    //To add the script after the last GO - Govs
    CommandList.Add(SQLCommand);
    Result:= Result + 1;
  finally
    ScriptFile.Clear;
  end;
end; { ADOLoadScriptFromFile }

procedure ADORunScript(const FileName: string);
var
  I: Integer;
  CommandList: TStringList;
begin
  // here is already established connection
  CommandList := TStringList.Create;
  try
    if ADOLoadScriptFromFile(FileName, CommandList) > 0 then
      Log('ADORunScript CommandList.Count  ' + IntToStr(CommandList.Count) );

    for I := 0 to CommandList.Count - 1 do
    begin
      // execute each command
      ADOExecuteSQL(CommandList[I]);
    end;
  except
    //reset so next adoconnection string is correct
    if (not VarIsEmpty(ADOGlobalConnection))  then
    begin
      ADOGlobalConnection := Unassigned;
    end;
    if (not VarIsEmpty(ADOGlobalCommand))  then
    begin
      ADOGlobalCommand := Unassigned;
    end;
    RaiseException(GetExceptionMessage);
  finally
    CommandList.Clear;
  end;
end; { ADORunScript }
 