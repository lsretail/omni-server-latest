;THIS IS THE MAIN setup script !

#define VersionFile '..\Service\bin\LSOmni.Service.dll'
#define ApplicationVersion GetVersionNumbersString(VersionFile)

#define Major
#define Minor
#define Rev
#define Build
#expr GetVersionComponents(VersionFile, Major, Minor, Rev, Build)
#define VersionNo Str(Major)+(Minor > 9 ? "." : ".")+Str(Minor)+(Rev > 9 ? "." : ".")+Str(Rev)

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{07C464D2-8863-4A44-B30C-A4EBDAE8D246}
AppName="Commerce Service for LS Central Version {#VersionNo}"
AppVersion={#ApplicationVersion}
AppVerName="Commerce Service for LS Central {#ApplicationVersion}"
AppPublisher=LS Retail Inc.
AppPublisherURL=http://www.lsretail.com/
AppSupportURL=http://www.lsretail.com/
AppUpdatesURL=http://www.lsretail.com/
AppComments=Commerce Service for LS Central
AppCopyright=Copyright (C) 2021 LS Retail
;file version
VersionInfoVersion={#ApplicationVersion}
;when changing the DefaultDirName I had to change the AppId !!
DefaultDirName=C:\LS Retail\Commerce
DefaultGroupName=notused
DisableProgramGroupPage=yes
DisableDirPage=no
DirExistsWarning=yes
OutputBaseFilename="CommerceServiceForLSCentral.Setup.{#VersionNo}"
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
Source: "..\DataAccess\Data.SQLServer\SQLScripts\LSCommerceServiceDbInitData.sql"; DestDir: "{app}\Sql\SqlScripts\"; Flags: ignoreversion
Source: "..\DataAccess\Data.SQLServer\SQLScripts\LSCommerceServiceDbObjects.sql"; DestDir: "{app}\Sql\SqlScripts\"; Flags: ignoreversion
Source: "..\DataAccess\Data.SQLServer\SQLScripts\LSCommerceServiceDbPermissions.sql"; DestDir: "{app}\Sql\SqlScripts\"; Flags: ignoreversion

;all files should be in other include files
#include "FilesInclude.iss"

[Icons]
Name: "{group}\{cm:ProgramOnTheWeb,CommerceService}"; Filename: "http://www.lsretail.com/"

[Registry]
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Services\EventLog\Application\Commerce"; ValueType: string; ValueName: "EventMessageFile"; ValueData: "C:\Windows\Microsoft.NET\Framework\v2.0.50727\EventLogMessages.dll"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\LS Retail\Omni"; ValueType: string; ValueName: "Path"; ValueData: "{app}\{code:WcfDir}";


[Code]
//tried to have code in other files
#include "Library.iss"
#include "ado.iss"
#include "IISFunctions.iss"
#include "SqlPage.iss"
#include "NavSqlPage.iss"
#include "CheckPage.iss"
#include "IISPage.iss"
 
var
  //the custom pages used, defined in other iss files
  SqlCustomPage : TWizardPage;
  NavSqlCustomPage : TWizardPage;
  IISCustomPage : TWizardPage;
  CheckCustomPage : TWizardPage;

  UpdateAppSettings : Boolean;
  CopyAppSettingsFile : Boolean;
  CmdMode : Boolean;

procedure InitializeWizard;
begin
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
  CmdMode := GetCommandLineParamBoolean('-Cmd', False);

  CheckPage_NavSQLCheckBox.Checked := GetCommandLineParamBoolean('-NavX', True);
  NavSQLPage_txtServer.Text := GetCommandLineParamString('-NavSrv', 'localhost');
  NavSQLPage_txtDBname.Text := GetCommandLineParamString('-NavDb', 'LSCentral');
  NavSQLPage_txtNavCompany.Text := GetCommandLineParamString('-NavComp', 'CRONUS - LS Central');
  NavSQLPage_txtUsername.Text := GetCommandLineParamString('-NavUsr', 'CommerceUser');
  NavSQLPage_txtPassword.Text := GetCommandLineParamString('-NavPwd', 'CommerceUser');
  NavSQLPage_chkWindowsAuth.Checked := GetCommandLineParamBoolean('-NavWaun', False);
  NavSQLPage_VerCombBox.ItemIndex := GetCommandLineParamInteger('-NavVer', 2);

  CheckPage_SQLCheckBox.Checked := GetCommandLineParamBoolean('-SqlX', True);
  CheckPage_WSCheckBox.Checked := GetCommandLineParamBoolean('-WSX', False);
  SQLPage_txtDBname.Text := GetCommandLineParamString('-SqlDb', 'Commerce');
  SQLPage_txtServer.Text := GetCommandLineParamString('-SqlSrv', 'localhost');
  SQLPage_txtUsername.Text := GetCommandLineParamString('-SqlUsr', 'CommerceUser');
  SQLPage_txtPassword.Text := GetCommandLineParamString('-SqlPwd', 'CommerceUser');
  SQLPage_chkWindowsAuth.Checked := GetCommandLineParamBoolean('-SqlWau', True);
  SQLPage_xCreateUser.Checked := GetCommandLineParamBoolean('-SqlCrUsr', True);

  CheckPage_IISCheckBox.Checked := GetCommandLineParamBoolean('-IisX', True);
  IISPage_txtWcfSiteName.Text := GetCommandLineParamString('-IisSite', 'Default Web Site');
  IISPage_txtWcfServiceName.Text := GetCommandLineParamString('-IisSrv', 'CommerceService');
  IISPage_txtNavUrl.Text := GetCommandLineParamString('-IisUrl', 'http://localhost:7047/BC210/WS/CRONUS - LS Central/Codeunit/RetailWebServices');
  IISPage_txtODataUrl.Text := GetCommandLineParamString('-IisOData', 'http://localhost:7048/BC210/ODataV4');
  IISPage_txtNavUser.Text := GetCommandLineParamString('-IisUsr', '');
  IISPage_txtNavPwd.Text := GetCommandLineParamString('-IisPwd', '');
end;

function WcfDir(Param: String): String;
begin
  Result := IISPage_txtWcfServiceName.Text;
end;

function UpdAppSettings: Boolean;
begin
  Result := CopyAppSettingsFile
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
    ADOInit(SQLPage_txtServer.Text, SQLPage_txtDBname.Text, SQLPage_txtUsername.Text, SQLPage_txtPassword.Text, SQLPage_chkWindowsAuth.Checked);

    // Check if sql server mixed authentication is enabled
    if (not ADOCheckIsMixedAuthentication()) then
    begin
      msg := 'SQL Server mixed authentication mode is not enabled on ' + SQLPage_txtServer.Text + ' '#13
      msg := msg + 'You need this to continue the easy way. You can use Windows Authentication only but '#13
      msg := msg + 'then you need to change the config files manually.'#13
      msg := msg + 'See Commerce Service for LS Central Installation doc'#13
      msg := msg + 'Quit the setup?'
      if (not CmdMode) and (MsgBox(msg, mbConfirmation, MB_YESNO) = idYes ) then
      begin
        Result := False;
      end;
    end ;

    // Check if trying to create tables in the Central database
    if (Result and ADOCheckIsNavDB()) then
    begin
      msg := 'You are trying to create the Commerce Service for LS Central SQL Server objects in db: ' + SQLPage_txtDBname.Text + '  on ' + SQLPage_txtServer.Text + ' '#13
      msg := msg + 'This database is a LS Central database and you are not allowed to create the Commerce Service for LS Central Database objects'#13
      msg := msg + 'in the LS Central Database'#13
      msg := msg + 'See Commerce Service for LS Central Installation doc'#13
      if (not CmdMode) then
        MsgBox(msg, mbConfirmation, MB_OK);
      
      Result := False;
    end;

    Log('SqlCreateDb - Create Tables');
    ADORunScript('LSCommerceServiceDbObjects.sql');
    Log('SqlCreateDb - Default Data');
    ADORunScript('LSCommerceServiceDbInitData.sql');

    if SQLPage_xCreateUser.Checked then
    begin
      Log('SqlCreateDb - Create User');
      ADORunScript('LSCommerceServiceDbPermissions.sql');
    end;
  except
    ErrorMsg('SqlCreateDb', CmdMode);
    Result := False;
  end;      
end; { SqlCreateDb }

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
    ErrorWarningMsg('Failed to update AppSettings.config file'#13'AppSettings.Config file will not get updated'#13'with values entered', CmdMode);
  end;

  //standardize on the CommerceUser
  navCompany := Trim(NavSQLPage_txtNavCompany.Text); //no dont add + '$'; 
  StringChangeEx(navCompany, '.', '_', True);
  Result := True;
  try
    // Central Web Service
    if CheckPage_IISCheckBox.Checked then
    begin
        Log('Update File IIS Settings');
        UpdateAppSettingsConfig('BOConnection.Nav.Url', Trim(IISPage_txtNavUrl.Text), ExpandConstant('{app}\{code:WcfDir}'));
        UpdateAppSettingsConfig('BOConnection.Nav.ODataUrl', Trim(IISPage_txtODataUrl.Text), ExpandConstant('{app}\{code:WcfDir}'));
        UpdateAppSettingsConfig('BOConnection.Nav.UserName', Trim(IISPage_txtNavUser.Text), ExpandConstant('{app}\{code:WcfDir}'));
        UpdateAppSettingsConfig('BOConnection.Nav.Password', Trim(IISPage_txtNavPwd.Text), ExpandConstant('{app}\{code:WcfDir}'));
        UpdateAppSettingsConfig('BOConnection.Nav.Tenant', Trim(IISPage_txtNavTen.Text), ExpandConstant('{app}\{code:WcfDir}'));
        if IISPage_xS2S.Checked then
          UpdateAppSettingsConfig('BOConnection.Nav.Protocol', 'S2S', ExpandConstant('{app}\{code:WcfDir}'));
    end;

    // Central connection string
    if CheckPage_NavSQLCheckBox.Checked then
    begin
      Log('Update Central SQL Settings');

      navStr := 'Data Source=' + Trim(NavSQLPage_txtServer.Text);
      navStr := navStr + ';Initial Catalog=' + Trim(NavSQLPage_txtDBname.Text);

      if NavSQLPage_chkWindowsAuth.Checked then
      begin
        navStr := navStr + ';Integrated Security=True';
      end
      else
      begin
        if (Length(NavSQLPage_txtUsername.Text) > 0) then
          user := NavSQLPage_txtUsername.Text
        else
          user := 'CommerceUser';
  
        if (Length(NavSQLPage_txtPassword.Text) > 0) then
          pwd := NavSQLPage_txtPassword.Text
        else
          pwd := 'CommerceUser';

        navStr := navStr + ';User ID=' + Trim(user);
        navStr := navStr + ';Password=' + Trim(pwd);
      end;

      navStr := navStr + ';NAVCompanyName=' + navCompany;
      navStr := navStr + ';Persist Security Info=True;MultipleActiveResultSets=True;Connection Timeout=10;';

      UpdateAppSettingsConfig('SqlConnectionString.Nav', navStr, ExpandConstant('{app}\{code:WcfDir}'));
    end;

    if CheckPage_WSCheckBox.Checked then
    begin
      UpdateAppSettingsConfig('BOConnection.Nav.Protocol','Tls12', ExpandConstant('{app}\{code:WcfDir}'));
      if IISPage_xS2S.Checked then
        UpdateAppSettingsConfig('BOConnection.Nav.Protocol', 'S2S', ExpandConstant('{app}\{code:WcfDir}'));
      UpdateAppSettingsConfig('BOConnection.AssemblyName','LSOmni.DataAccess.BOConnection.NavWS.dll', ExpandConstant('{app}\{code:WcfDir}'));
    end
    else
    begin
      case NavSQLPage_VerCombBox.ItemIndex of
        0: UpdateAppSettingsConfig('BOConnection.AssemblyName','LSOmni.DataAccess.BOConnection.NavSQL.dll', ExpandConstant('{app}\{code:WcfDir}'));
        1: UpdateAppSettingsConfig('BOConnection.AssemblyName','LSOmni.DataAccess.BOConnection.CentrAL.dll', ExpandConstant('{app}\{code:WcfDir}'));
        2: UpdateAppSettingsConfig('BOConnection.AssemblyName','LSOmni.DataAccess.BOConnection.CentralPre.dll', ExpandConstant('{app}\{code:WcfDir}'));
      end;
    end;

    Log('Update Commerce Service for LS Central SQL Settings');
    omniStr := 'Data Source=' + Trim(SQLPage_txtServer.Text);
    omniStr := omniStr + ';Initial Catalog=' + Trim(SQLPage_txtDBname.Text);

    if SQLPage_xCreateUser.Checked then
    begin
      omniStr := omniStr + ';User ID=CommerceUser';
      omniStr := omniStr + ';Password=CommerceUser';
    end
    else
    begin
      if SQLPage_chkWindowsAuth.Checked then
      begin
        omniStr := omniStr + ';Integrated Security=True';
      end
      else
      begin
        if (Length(SQLPage_txtUsername.Text) > 0) then
          user := SQLPage_txtUsername.Text
        else
          user := 'CommerceUser';

        if (Length(SQLPage_txtPassword.Text) > 0) then
          pwd := SQLPage_txtPassword.Text
        else
          pwd := 'CommerceUser';

        omniStr := omniStr + ';User ID=' + Trim(user);
        omniStr := omniStr + ';Password=' + Trim(pwd);
      end;
    end;
    
    omniStr := omniStr + ';Persist Security Info=True;MultipleActiveResultSets=True;Connection Timeout=10;';
    UpdateAppSettingsConfig('SQLConnectionString.LSOmni', omniStr, ExpandConstant('{app}\{code:WcfDir}'));

    UpdateAppSettingsConfig('ECom.Url', Trim(IISPage_txtEComUrl.Text), ExpandConstant('{app}\{code:WcfDir}'));
  except
    ErrorMsg('AppSettingsChangeScript. Check the values in the AppSettings.Config file.', CmdMode);
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
    if (SqlCreateDb()) then
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
    if (not IISCreateWebApplication(IISPage_txtWcfSiteName.Text,IISPage_txtWcfServiceName.Text, WizardForm.DirEdit.Text, CmdMode)) then
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
  UpdateAppSettings := True;
  if CurPageID = wpSelectDir then
  begin
    CopyAppSettingsFile := True;
    if FileExists(ExpandConstant('{app}\{code:WcfDir}\AppSettings.config')) then
    begin
      CopyAppSettingsFile := False;
      if CheckPage_IISCheckBox.Checked or CheckPage_NavSQLCheckBox.Checked or CheckPage_SQLCheckBox.Checked then
      begin
        if not CmdMode and (MsgBox('AppSettings.config exists in this folder.  Do you want to update it with new values?', mbConfirmation, MB_YESNO) = idNo) then
        begin
          UpdateAppSettings := False;
        end;
      end
      else
        UpdateAppSettings := False;
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
        IISPage_lblNavTen.Visible := False;
        IISPage_txtNavTen.Visible := False;
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
    ErrorWarningMsg('This setup requires Windows 7 and above.', CmdMode);
    result := False;
  end
  else if not IsDotNetDetected('v4.7', 0) then 
  begin
    ErrorWarningMsg('This setup requires Microsoft .NET Framework 4.7'#13#13
           'Please use Windows Update to install this version,'#13
           'and then re-run this setup program.', CmdMode);
    result := False;
  end
  else if  not (IsIIS7AboveInstalled) then 
  begin
    ErrorWarningMsg('This setup requires IIS 7 and above.'#13#13
           'You can continue the installation of SQL Server scripts.'#13
           'but will fail on the IIS setup part.', CmdMode);
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
  else
    Result := False;
end;

function InitializeUninstall(): Boolean;
begin
  Log('Init Uninstall');
  Result := True;
  if (not CmdMode) then
    MsgBox('Only files will be removed.' #13#13 'No SQL object or data removed' #13#13 'IIS Web application must be removed with the IIS Manager', mbInformation, MB_OK) 
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultStr: String;
begin
  Log('Uninstall process');
  if CurUninstallStep = usPostUninstall then
  begin
    if RegQueryStringValue(HKLM, 'Software\LS Retail\Omni', 'Path', ResultStr) then
    begin
      Log('Program Dir: ' + ResultStr);
      if not DirExists(ResultStr) then
	    exit;
	
      Log('Delete Program Dir');
      DelTree(ResultStr, True, True, True);
    end;
  end;
end;

// CurPageID values for predefined wizard pages
//----------------------------------------------
// wpWelcome, wpLicense, wpPassword, wpInfoBefore, wpUserInfo, wpSelectDir, wpSelectComponents,
// wpSelectProgramGroup, wpSelectTasks, wpReady, wpPreparing, wpInstalling, wpInfoAfter, wpFinished
