[Code]
// NOTE, the sql scrips understand /* */  but not -- as comments after text
//ok to start with -- but not as comments after column names for example
[Code]
const
  adNavCmdUnspecified = $FFFFFFFF;
  adNavCmdUnknown = $00000008;
  adNavCmdText = $00000001;
  adNavCmdTable = $00000002;
  adNavCmdStoredProc = $00000004;
  adNavCmdFile = $00000100;
  adNavCmdTableDirect = $00000200;
  adNavOptionUnspecified = $FFFFFFFF;
  adNavAsyncExecute = $00000010;
  adNavAsyncFetch = $00000020;
  adNavAsyncFetchNonBlocking = $00000040;
  adNavExecuteNoRecords = $00000080;
  adNavExecuteStream = $00000400;
  adNavExecuteRecord = $00000800;

  var
    ADONavGlobalCommand: Variant;
    ADONavGlobalConnection: Variant;
    ADONavGlobalDbName: string;
    ADONavGlobalCompanyName: string;

function ADONavGetConnectionString(dbServer: string; dbName: string; companyName: string;
      userName: string; pwd: string; windowsAuth: Boolean): Variant;
var  
  ADOConnectionString: string;  
begin

      dbServer := Trim(dbServer);
      dbName := Trim(dbName);
      companyName  := Trim(companyName);
      userName := Trim(userName);
      pwd := Trim(pwd);
      ADONavGlobalDbName := dbName ;
      ADONavGlobalCompanyName := companyName ;
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
        ADOConnectionString := ADOConnectionString +
        'Integrated Security=SSPI;'         // Windows Auth
      else
        ADOConnectionString := ADOConnectionString +
        'User Id=' + userName + ';' +              // user name
        'Password=' + pwd + ';';                   // password

      ADOConnectionString := ADOConnectionString + 'NAVCompanyName=' + companyName + ';'; 
	  
	  //ADOConnectionString := ADOConnectionString + ' Network Library=DBMSSOCN; ';

    // open the connection by the assigned ConnectionString
    Result := ADOConnectionString;
end; 

Procedure ADONavInit(dbServer: string; dbName: string; companyName: string; 
     userName: string; pwd: string; windowsAuth: Boolean);
begin
    //JIJ dont try to understand this, but best to create object 1x and keep open.
    //got access exception when I tried to have this local in functions !
    // SO keep it open and never close a connection to sql server!
    // if (VarIsEmpty(ADONavGlobalConnection))  then
     begin
        Log('Init ADONavGlobalConnection called');
        ADONavGlobalConnection := CreateOleObject('ADODB.Connection') ;
        ADONavGlobalConnection.ConnectionString := ADONavGetConnectionString(dbServer,dbName,companyName,userName,pwd,windowsAuth); 
        ADONavGlobalConnection.Open;
     end; 
   //  if (VarIsEmpty(ADONavGlobalCommand))  then
     begin
        Log('Init ADONavGlobalCommand called');
        ADONavGlobalCommand := CreateOleObject('ADODB.Command') ;
        // assign the currently opened connection to ADO command object
        ADONavGlobalCommand.ActiveConnection := ADONavGlobalConnection; 
     end; 
     //never do ADONavGlobalConnection.Close;
end;
 
// Execute files specified in [files] section (hardcoded) against the user defined server.database
procedure ADONavExecuteSQL(sqlScript: string);
var     
   navCompName : string;
begin
    Log('ADONavExecuteSQL called ' + ADONavGlobalCompanyName);
    //build the nav company name, add $ and get rid of periods . 
    navCompName := ADONavGlobalCompanyName + '$'; 
    StringChangeEx(navCompName, '.', '_', True);  //% [ ]  ' " are also converted
    Log('ADONavExecuteSQL navCompName: ' + navCompName);
    if Length(sqlScript) > 0  then
    begin 
      //switch out the database name for permission scripts
      StringChangeEx(sqlScript, 'THEDATABASE', ADONavGlobalDbName, True);
      StringChangeEx(sqlScript, '[dbo].[THECOMPANYNAME$', '[dbo].['+navCompName, True);
      // mpos
      //StringChangeEx(sqlScript, 'PK_THECOMPANYNAME$MobileAction', 'PK_'+ navCompName + 'MobileAction', True);
      //StringChangeEx(sqlScript, 'IX_THECOMPANYNAME$MobileAction_PK1', 'IX_'+ navCompName + 'MobileAction_PK1', True);
      StringChangeEx(sqlScript, '_THECOMPANYNAME', '_'+ navCompName, True);

      // assign text of a command to be issued against a provider
      ADONavGlobalCommand.CommandText := sqlScript;
      ADONavGlobalCommand.CommandTimeout := 600; //10 min 30 sec
      // this property setting means, that you're going to execute the 
      // CommandText text command; it does the same, like if you would
      // use only adCmdText flag in the Execute statement
      ADONavGlobalCommand.CommandType := adNavCmdText;
      // this will execute the command and return dataset
      //Log('ADONavExecuteSQL Execute ' + sqlScript);
      ADONavGlobalCommand.Execute(NULL, NULL, adNavCmdText or adNavExecuteNoRecords);
    end;
end;



//read the sqlscript file into a list, breaking up at the GO line
function ADONavLoadScriptFromFile(const FileName: string;  out CommandList: TStringList): Integer;
var
  I: Integer;
  SQLCommand: string;
  ScriptFile: TStringList;
begin
  Result := 0;
   ScriptFile := TStringList.Create;
  try
    SQLCommand := '';
    Log('ADONavLoadScriptFromFile called: ' + FileName);
    ExtractTemporaryFile(FileName) ;

    ScriptFile.LoadFromFile(ExpandConstant('{tmp}\' + FileName));
    Log('ADONavLoadScriptFromFile  ScriptFile.Count: '+ IntToStr(ScriptFile.Count) );
    for I := 0 to ScriptFile.Count - 1 do
    begin
      //skip lines that have --  in it
     //if Pos('--', LowerCase(Trim(ScriptFile[I]))) = 0 then
     //begin
        if Pos('go', LowerCase(Trim(ScriptFile[I]))) = 1 then
        begin
          Result := Result + 1;
          CommandList.Add(SQLCommand);
          SQLCommand := '';
        end
        else
          SQLCommand := SQLCommand + ' ' + ScriptFile[I] + ' '#10#13;
      end;
   //end;
    //EDIT: If there is no GO syntax present int the file AND
    //To add the script after the last GO - Govs
    CommandList.Add(SQLCommand);
    Result:= Result + 1;
  finally
    ScriptFile.Clear;
  end;
end; { ADONavLoadScriptFromFile }

procedure ADONavRunScript(const FileName: string);
var
  I: Integer;
  CommandList: TStringList;
begin
   
  // here is already established connection
  CommandList := TStringList.Create;
  try
    if ADONavLoadScriptFromFile(FileName, CommandList) > 0 then
      Log('ADONavRunScript CommandList.Count  ' + IntToStr(CommandList.Count) );
      //ADOGlobalConnection.Open;
      for I := 0 to CommandList.Count - 1 do
      begin
        // execute each command
        //Log('ADONavRunScript: I= ' + IntToStr(I) );   
        ADONavExecuteSQL(CommandList[I]);
      end;
  except
     //reset so next adoconnection string is correct
     if (not VarIsEmpty(ADONavGlobalConnection))  then
     begin
       ADONavGlobalConnection := Unassigned;
     end;
     if (not VarIsEmpty(ADONavGlobalCommand))  then
     begin
       ADONavGlobalCommand := Unassigned;
     end; 
     RaiseException(GetExceptionMessage);
  finally
    CommandList.Clear;
  end;
  
end; { ADONavRunScript }

 