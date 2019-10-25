;THIS IS THE MAIN setup script !

#define VersionFile '..\Service\bin\LSOmni.Service.dll'
#define ApplicationVersion GetFileVersion(VersionFile)

#define Major
#define Minor
#define Rev
#define Build
#expr ParseVersion(VersionFile, Major, Minor, Rev, Build)
#define VersionNo Str(Major)+(Minor > 9 ? "." : ".")+Str(Minor)+(Rev > 9 ? "." : ".")+Str(Rev)

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{07C464D2-8863-4A44-B30C-A4EBDAE8D246}
AppName="LS Omni Server version {#VersionNo}"
AppVersion={#ApplicationVersion}
AppVerName="LS Omni Server {#ApplicationVersion}"
AppPublisher=LS Retail, Inc.
AppPublisherURL=http://www.lsretail.com/
AppSupportURL=http://www.lsretail.com/
AppUpdatesURL=http://www.lsretail.com/
AppReadmeFile=http://www.tr.im/lsomni
AppComments=LS Omni WCF Service
AppCopyright=Copyright (C) 2019 LS Retail
;file version
VersionInfoVersion={#ApplicationVersion}
;when changing the DefaultDirName I had to change the AppId !!
DefaultDirName=C:\LS Retail\LSOmni
DefaultGroupName=notused
DisableProgramGroupPage=yes
DisableDirPage=no
DirExistsWarning=yes
OutputBaseFilename="LSOmni.Service.Central.Setup.{#VersionNo}"
SetupIconFile=LSIcon.ico
UninstallDisplayIcon={app}\{code:WcfDir}\bin\LSIcon.ico
Compression=lzma             
SolidCompression=yes
 
;SetupLogging file location C:\Users\<user>\AppData\Local\Temp\Setup Log 2015-05-29 #003.txt
SetupLogging = yes 
DisableWelcomePage=False   
DisableReadyPage=False   
DisableFinishedPage=False   
Uninstallable=yes
;location of unins000.exe and unins000.dat is saved
UninstallFilesDir={win}\Installer  

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; sql files
; DestDir: {app}; Source: Files\*; Excludes: "*.m,.svn,private"; Flags: recursesubdirs
Source: "..\DataAccess\Data.SQLServer\SQLScripts\LSOmniServiceDbInitData.sql"; DestDir: "{app}\Sql\SqlScripts\"; Flags: ignoreversion
Source: "..\DataAccess\Data.SQLServer\SQLScripts\LSOmniServiceDbObjects.sql"; DestDir: "{app}\Sql\SqlScripts\"; Flags: ignoreversion
Source: "..\DataAccess\Data.SQLServer\SQLScripts\LSOmniServiceDbPermissions.sql"; DestDir: "{app}\Sql\SqlScripts\"; Flags: ignoreversion

;all files should be in other include files
#include "FilesInclude.iss"

[Icons]
Name: "{group}\{cm:ProgramOnTheWeb,LSOmniService}"; Filename: "http://www.lsretail.com/"

[Code]
//tried to have code in other files
#include "Library.iss"
#include "ado.iss"
#include "adonav.iss"
#include "IISFunctions.iss"
#include "SqlPage.iss"
#include "NavSqlPage.iss"
#include "CheckPage.iss"
#include "IISPage.iss"
 
var
  //the custome pages used, defined in other iss files
  SqlCustomPage : TWizardPage;
  NavSqlCustomPage : TWizardPage;
  IISCustomPage : TWizardPage;
  CheckCustomPage : TWizardPage;

  SQLFileList: TStringList;
  
  UpdateAppSettings : Boolean;
  CopyAppSettingsFile : Boolean;
  CmdMode : Boolean;

procedure InitializeWizard;
begin
  SQLFileList := TStringList.Create;

  { Create the pages }
  //from CheckPage.iss
  CheckCustomPage := CheckCustomForm_CreatePage(wpWelcome);  
  //from SqlPage.iss
  SqlCustomPage := SQLCustomForm_CreatePage(CheckCustomPage.ID);  
  //from NavSqlPage.iss
  NavSqlCustomPage := NavSQLCustomForm_CreatePage(SqlCustomPage.ID);  
  //from IISPage.iss
  IISCustomPage := IISCustomForm_CreatePage(NavSqlCustomPage.ID); 

  //should only set any texts here..
  CmdMode := GetCommandLineParamBoolean('-Cmd', false);
  CheckPage_NavSQLCheckBox.Checked := GetCommandLineParamBoolean('-NavX', true);
  NavSQLPage_txtServer.Text := GetCommandLineParamString('-NavSrv', 'localhost');
  NavSQLPage_txtDBname.Text := GetCommandLineParamString('-NavDb', '');
  NavSQLPage_txtNavCompany.Text := GetCommandLineParamString('-NavComp', '');
  NavSQLPage_txtUsername.Text := GetCommandLineParamString('-NavUsr', '');
  NavSQLPage_txtPassword.Text := GetCommandLineParamString('-NavPwd', '');
  NavSQLPage_chkWindowsAuth.Checked := GetCommandLineParamBoolean('-NavWaun', true);
  NavSQLPage_chkSQLAuth.Checked := GetCommandLineParamBoolean('-NavSau', false);
  CheckPage_SQLCheckBox.Checked := GetCommandLineParamBoolean('-SqlX', true);
  CheckPage_MultiCheckBox.Checked := GetCommandLineParamBoolean('-MultiX', false);
  CheckPage_WSCheckBox.Checked := GetCommandLineParamBoolean('-WSX', false);
  SQLPage_txtDBname.Text := GetCommandLineParamString('-SqlDb', 'LSOmni');
  SQLPage_txtServer.Text := GetCommandLineParamString('-SqlSrv', 'localhost');
  SQLPage_txtUsername.Text := GetCommandLineParamString('-SqlUsr', '');
  SQLPage_txtPassword.Text := GetCommandLineParamString('-SqlPwd', '');
  SQLPage_chkWindowsAuth.Checked := GetCommandLineParamBoolean('-SqlWau', true);
  SQLPage_chkSQLAuth.Checked := GetCommandLineParamBoolean('-SqlSau', false);
  CheckPage_IISCheckBox.Checked := GetCommandLineParamBoolean('-IisX', true);
  IISPage_txtWcfSiteName.Text := GetCommandLineParamString('-IisSite', 'Default Web Site');
  IISPage_txtWcfServiceName.Text := GetCommandLineParamString('-IisSrv', 'LSOmniService');
  IISPage_txtNavUrl.Text := GetCommandLineParamString('-IisUrl', 'http://localhost:7047/BC140/WS/CRONUS LS 1401 W1 Demo/Codeunit/RetailWebServices');
  IISPage_txtNavUser.Text := GetCommandLineParamString('-IisUsr', '');
  IISPage_txtNavPwd.Text := GetCommandLineParamString('-IisPwd', '');

  //should only set the file here..
  //will be executed in this order
  SQLFileList.Add('LSOmniServiceDbObjects.sql');
  SQLFileList.Add('LSOmniServiceDbPermissions.sql');
  SQLFileList.Add('LSOmniServiceDbInitData.sql');
end;

function WcfDir(Param: String): String;
begin
  Result := IISPage_txtWcfServiceName.Text;
end;

function UpdAppSettings: Boolean;
begin
  if not CheckPage_MultiCheckBox.Checked then
	Result := CopyAppSettingsFile
  else
    Result := False;
end;

function UpdAppMultiSettings: Boolean;
begin
  if CopyAppSettingsFile then
	Result := CheckPage_MultiCheckBox.Checked
  else
    Result := False;
end;

function SingleMode: Boolean;
begin
  Result := CheckPage_MultiCheckBox.Checked = False;
end;

function WebMode: Boolean;
begin
  Result := CheckPage_WSCheckBox.Checked = False;
end;

function SqlCreateDb(): Boolean;
var
  msg : string;
begin
  Log('SqlCreateDb called');
  Result := True;
  try
    ADOCreateDb(SQLPage_txtServer.Text, SQLPage_txtDBname.Text, SQLPage_txtUsername.Text, SQLPage_txtPassword.Text, SQLPage_chkWindowsAuth.Checked);

    //check full text search after db and connection has been checked
    ADOInit(SQLPage_txtServer.Text, SQLPage_txtDBname.Text,SQLPage_txtUsername.Text, SQLPage_txtPassword.Text, SQLPage_chkWindowsAuth.Checked);

    // Check if sql server mixed authentication is enabled
    if (not ADOCheckIsMixedAuthentication()) then
    begin
      msg := 'SQL Server mixed authentication mode is not enabled on ' + SQLPage_txtServer.Text + ' '#13
      msg := msg + 'You need this to continue the easy way. You can use Windows Authentication only but '#13
      msg := msg + 'then you need to change the config files manually.'#13
      msg := msg + 'See LS Omni Server Installation doc'#13
      msg := msg + 'Quit the setup?'
      if (not CmdMode) and (MsgBox(msg, mbConfirmation, MB_YESNO) = idYes ) then
      begin
        Result := False;
      end;
    end ;

    // Check if trying to create tables in the NAV database
    if (Result and ADOCheckIsNavDB()) then
    begin
      msg := 'You are trying to create the LS Omni SQL Server objects in db: ' + SQLPage_txtDBname.Text + '  on ' + SQLPage_txtServer.Text + ' '#13
      msg := msg + 'This database is a LS Nav/Central database and you are not allowed to create the LSOmni database objects'#13
      msg := msg + 'in the LS Nav/Central Database'#13
      msg := msg + 'See LS Omni Server Installation doc'#13
      if (not CmdMode) then
        MsgBox(msg, mbConfirmation, MB_OK);
      Result := False;
    end;                                              
 
  except
    Log('SqlCreateDb error: ' + GetExceptionMessage );
    MsgBox(GetExceptionMessage, mbError, MB_OK);
    Result := False;
  end;      
end; { SqlCreateDb }

function SqlRunScripts(): Boolean;
var  
  I : Integer;
begin
  Log('SqlRunScripts called');
  Result := True;
  try
    ADOInit(SQLPage_txtServer.Text, SQLPage_txtDBname.Text,SQLPage_txtUsername.Text, SQLPage_txtPassword.Text, SQLPage_chkWindowsAuth.Checked);

    for I := 0 to SQLFileList.Count - 1 do
    begin
      // execute each command
      Log('SQLFileList: I= ' + IntToStr(I) + ' - '+ SQLFileList[I] );
      //ADORunScript('HospLoyObjects2.sql');   
      ADORunScript(SQLFileList[I]);
    end;     
  except
    Log('SqlRunScripts error: ' + GetExceptionMessage );
    MsgBox(GetExceptionMessage, mbError, MB_OK);
    Result := False;
  end;   
end; { SqlRunScripts }
                             
function AppSettingsChangeScript(): Boolean;     
var  
  omniStr: string; 
  navStr: string; 
  navCompany : string;
  user: string;
  pwd: string;
begin
  Log('AppSettingsChangeScript called');  //called after install in CurStepChanged

  //check if we can even parse the xml
  if not ValidationXMLDomExists then
  begin
	  Log('AppSettingsChangeScript error: Failed to update AppSettings.config file');
    MsgBox('Failed to update AppSettings.config file'#13'AppSettings.Config file will not get updated'#13'with values entered', mbError, MB_OK);
  end;                   

  //standardize on the LSOmniUser
  navCompany := Trim(NavSQLPage_txtNavCompany.Text); //no dont add + '$'; 
  StringChangeEx(navCompany, '.', '_', True);
  Result := True;
  try
    // NAV Web Service
    if CheckPage_IISCheckBox.Checked then
    begin
      if CheckPage_MultiCheckBox.Checked then
	  begin
        Log('Update DB IIS Settings');
	    ADOUpdateAppSettings('BOUrl', Trim(IISPage_txtNavUrl.Text));
        ADOUpdateAppSettings('BOUser', Trim(IISPage_txtNavUser.Text));
        ADOUpdateAppSettings('BOPassword', Trim(IISPage_txtNavPwd.Text));
	  end
	  else
	  begin
        Log('Update File IIS Settings');
        UpdateAppSettingsConfig('BOConnection.Nav.Url', Trim(IISPage_txtNavUrl.Text), ExpandConstant('{app}\{code:WcfDir}'));
        UpdateAppSettingsConfig('BOConnection.Nav.UserName', Trim(IISPage_txtNavUser.Text), ExpandConstant('{app}\{code:WcfDir}'));
        UpdateAppSettingsConfig('BOConnection.Nav.Password', Trim(IISPage_txtNavPwd.Text), ExpandConstant('{app}\{code:WcfDir}'));
	  end;
    end;

	if (Length(NavSQLPage_txtUsername.Text) > 0) then
	  user := NavSQLPage_txtUsername.Text
	else
	  user := 'LSOmniUser';

	if (Length(NavSQLPage_txtPassword.Text) > 0) then
	  pwd := NavSQLPage_txtPassword.Text
	else
	  pwd := 'LSOmniUser';

    // NAV connection string
    if CheckPage_NavSQLCheckBox.Checked then
    begin
      Log('Update NAV SQL Settings');
      navStr := 'Data Source=' + Trim(NavSQLPage_txtServer.Text);
      navStr := navStr + ';Initial Catalog=' + Trim(NavSQLPage_txtDBname.Text);
      navStr := navStr + ';User ID=' + Trim(user);
      navStr := navStr + ';Password=' + Trim(pwd);
      navStr := navStr + ';NAVCompanyName=' + navCompany;
      navStr := navStr + ';Persist Security Info=True;MultipleActiveResultSets=True;Connection Timeout=10;';

      if CheckPage_MultiCheckBox.Checked then
	    ADOUpdateAppSettings('BOSql', navStr)
      else
        UpdateAppSettingsConfig('SqlConnectionString.Nav', navStr, ExpandConstant('{app}\{code:WcfDir}'));
    end;

	if CheckPage_WSCheckBox.Checked then
	  UpdateAppSettingsConfig('BOConnection.AssemblyName','LSOmni.DataAccess.BOConnection.NavWS.dll', ExpandConstant('{app}\{code:WcfDir}'))
	else
	  UpdateAppSettingsConfig('BOConnection.AssemblyName','LSOmni.DataAccess.BOConnection.NavSQL.dll', ExpandConstant('{app}\{code:WcfDir}'));

	if (Length(SQLPage_txtUsername.Text) > 0) then
	  user := SQLPage_txtUsername.Text
	else
	  user := 'LSOmniUser';

	if (Length(SQLPage_txtPassword.Text) > 0) then
	  pwd := SQLPage_txtPassword.Text
	else
	  pwd := 'LSOmniUser';

    // LSOmni database string
    if CheckPage_SQLCheckBox.Checked then
    begin
      Log('Update LS Omni SQL Settings');
      omniStr := 'Data Source=' + Trim(SQLPage_txtServer.Text);
      omniStr := omniStr + ';Initial Catalog=' + Trim(SQLPage_txtDBname.Text);
      omniStr := omniStr + ';User ID=' + Trim(user);
      omniStr := omniStr + ';Password=' + Trim(pwd);
      omniStr := omniStr + ';Persist Security Info=True;MultipleActiveResultSets=True;Connection Timeout=10;';
      UpdateAppSettingsConfig('SQLConnectionString.LSOmni', omniStr, ExpandConstant('{app}\{code:WcfDir}'));
    end; 
  except
	Log('AppSettingsChangeScript error: ' + GetExceptionMessage );
    MsgBox(GetExceptionMessage, mbError, MB_OK);
    MsgBox('Check the values in the AppSettings.Config file', mbError, MB_OK);
    Result := False;
  end;      
end; { NavSqlRunScripts }

//Hook into setting up the sql and navsql installation     
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
   doContinue : Boolean;
begin
  Log('PrepareToInstall() called');
  doContinue := True;

  if (doContinue and CheckPage_SQLCheckBox.Checked) then
  begin
    WizardForm.PreparingLabel.Visible := True;
    WizardForm.PreparingLabel.Show();
    WizardForm.PreparingLabel.Caption := 'Please wait while running SQL scripts...';
    if (SqlCreateDb() and SqlRunScripts()) then
    begin
      doContinue := True;
      Result := '';
    end
    else
    begin
      doContinue := False;
      Result := 'Something went wrong in the sql setup...' ;
    end;
  end;

  if (doContinue and CheckPage_IISCheckBox.Checked) then
  begin
    WizardForm.PreparingLabel.Visible := True;
    WizardForm.PreparingLabel.Show();
    WizardForm.PreparingLabel.Caption := 'Please wait while creating web application under IIS...';
    
    //  WizardForm.DirEdit.Text has the c:\LS Retail\andWhatHasChangeToo
    if (not IISCreateWebApplication(IISPage_txtWcfSiteName.Text,IISPage_txtWcfServiceName.Text, WizardForm.DirEdit.Text)) then
    begin
      doContinue := False;
      Result := 'Something went wrong in the IIS WCF setup...';
    end
    else
    begin
      doContinue := True;
      Result := '';
    end;
  end;
  if (Length(Result) > 0) then
  begin
      Result := Result + ''#13  + 'Logfile: ' + expandconstant('{log}')
  end;
end; { PrepareToInstall }
 
procedure CurStepChanged(CurStep: TSetupStep);
// You can this event function to perform your own pre-install and post-install tasks.
// Called with CurStep=ssInstall just before the actual installation starts,
// with CurStep=ssPostInstall just after the actual installation finishes,
// and with CurStep=ssDone just before Setup terminates after a successful install.
begin
  case CurStep of
    ssPostInstall :
      begin
        //Alert(' ssPostInstall after install');
        if UpdateAppSettings then
        begin
          //must change the appsettings here because the files have been extracted to app folder
          AppSettingsChangeScript();
        end;
      end;
  end;
end;
 
function NextButtonClick(CurPageID: Integer): Boolean;
var
  I: Integer;
begin
  I := SqlCustomPage.ID;
  Result := True
  UpdateAppSettings := true;
  if CurPageID = wpSelectDir then
  begin
    CopyAppSettingsFile := true;
    if FileExists(ExpandConstant('{app}\{code:WcfDir}\AppSettings.config')) then
    begin
      CopyAppSettingsFile := false;
      if CheckPage_IISCheckBox.Checked or CheckPage_NavSQLCheckBox.Checked or CheckPage_SQLCheckBox.Checked then
      begin
        if not CmdMode and (MsgBox('AppSettings.config exists in this folder.  Do you want to update it with new values?', mbConfirmation, MB_YESNO) = idNo) then
        begin
          UpdateAppSettings := false;
        end;
      end
      else
        UpdateAppSettings := false;
    end;
  end; 
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  Log('CurPageChanged(' + IntToStr(CurPageID) + ') called');

  case CurPageID of
    SQLPage.ID:
    begin
      with SQLPage do
      begin
        OnActivate := @SQLCustomForm_Activate;
      end;
    end;
    
    NavSQLPage.ID:
    begin
      with NavSQLPage do
      begin
        OnActivate := @NavSQLCustomForm_Activate;
      end;
    end;
    
    IISPage.ID:
    begin
      with IISPage do
      begin
	    IISPage_lblComment1.Visible := SingleMode;
		IISPage_lblNavUrl.Visible := SingleMode;
		IISPage_txtNavUrl.Visible := SingleMode;
	    IISPage_lblNavAuthentication.Visible := SingleMode;
		IISPage_lblNavUser.Visible := SingleMode;
		IISPage_txtNavUser.Visible := SingleMode;
		IISPage_lblNavPwd.Visible := SingleMode;
		IISPage_txtNavPwd.Visible := SingleMode;
        OnActivate := @IISCustomForm_Activate;
      end;
    end;
  end;
end; { CurPageChanged }

function InitializeSetup(): Boolean;
var 
  Version: TWindowsVersion;
begin
  //windows version information
  //5.0.2195 Windows 2000 
  //5.1.2600 Windows XP or Windows XP 64-Bit Edition Version 2002 (Itanium) 
  //5.2.3790 Windows Server 2003 or Windows XP x64 Edition (AMD64/EM64T) or Windows XP 64-Bit Edition Version 2003 (Itanium) 
  //6.0.6000 Windows Vista 
  //6.1.7600 Windows 7 or Windows Server 2008 R2  
  //6.2.9200 Windows 8 or Windows Server 2012
  //6.3  windows 8.1 
  
  GetWindowsVersionEx(Version); 
  result := True;
     
  if (Version.Major < 6) then
  begin
    MsgBox('This setup requires Windows 7 and above.', mbError, MB_OK);
    result := False;
  end
  else if not IsDotNetDetected('v4.7', 0) then 
  begin
    MsgBox('This setup requires Microsoft .NET Framework 4.7'#13#13
           'Please use Windows Update to install this version,'#13
           'and then re-run this setup program.', mbError, MB_OK);
    result := False;
  end
  else if  not (IsIIS7AboveInstalled) then 
  begin
    MsgBox('This setup requires IIS 7 and above.'#13#13
           'You can continue the installation of SQL Server scripts.'#13
           'but will fail on the IIS setup part.', mbError, MB_OK);
    result := True;
  end;
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  // Skip pages that shouldn't be shown 
  if not CheckPage_IISCheckBox.Checked and (PageID = wpInstalling) then
    Result := True
  else if not CheckPage_IISCheckBox.Checked and (PageID = wpSelectProgramGroup) then
    Result := True
  else if not CheckPage_IISCheckBox.Checked and (PageID = wpReady) then
    Result := True
  else if not CheckPage_SQLCheckBox.Checked and (PageID = SqlCustomPage.ID) then
    Result := True
  else if not CheckPage_NavSQLCheckBox.Checked and (PageID = NavSqlCustomPage.ID)  then
    Result := True
  else if not CheckPage_IISCheckBox.Checked and (PageID = IISCustomPage.ID)   then
    Result := True
  else if not CheckPage_IISCheckBox.Checked and not CheckPage_NavSQLCheckBox.Checked and not CheckPage_SQLCheckBox.Checked  then
    Result := False
//  else if not CheckPage_IISCheckBox.Checked and (PageID = wpSelectDir)  then
//    Result := True
  else
    Result := False;
end;

function InitializeUninstall(): Boolean;
begin
  Result := True;
  MsgBox('Only files will be removed.' #13#13 'No SQL object or data removed' #13#13 'IIS Web application must be removed with the IIS Manager', mbInformation, MB_OK) 

  //Result := MsgBox('Only files are removed.' #13#13 'SQL scripts', mbConfirmation, MB_YESNO) = idYes;
  //if Result = False then
  //  MsgBox('InitializeUninstall:' #13#13 'Ok, bye bye.', mbInformation, MB_OK);
end;

// CurPageID values for predefined wizard pages
//----------------------------------------------
// wpWelcome, wpLicense, wpPassword, wpInfoBefore, wpUserInfo, wpSelectDir, wpSelectComponents,
// wpSelectProgramGroup, wpSelectTasks, wpReady, wpPreparing, wpInstalling, wpInfoAfter, wpFinished
 

 
