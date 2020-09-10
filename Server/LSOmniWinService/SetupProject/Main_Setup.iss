;THIS IS THE MAIN setup script !

#define VersionFile '..\LSOmniWinService\bin\Release\LSOmni.Common.dll'
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
AppId={{08C464D2-8863-4A44-B30C-A4EBDAE8D246}
AppName=LS Omni Windows Service version {#VersionNo}
AppVersion={#ApplicationVersion}
AppVerName=LS Omni Windows Service {#ApplicationVersion}
AppPublisher=LS Retail Inc.
AppPublisherURL=http://www.lsretail.com/
AppSupportURL=http://www.lsretail.com/
AppUpdatesURL=http://www.lsretail.com/
AppComments=LS Omni Windows Service
AppCopyright=Copyright (C) 2020  LS Retail
;file version
VersionInfoVersion={#ApplicationVersion}
;when changing the DefaultDirName I had to change the AppId !!
DefaultDirName=C:\LS Retail\LSOmni\LSOmniWinService
DefaultGroupName=notused
DisableProgramGroupPage=yes
DisableDirPage=no
DirExistsWarning=yes
OutputBaseFilename="LSOmni.WinService.Setup.{#VersionNo}"
SetupIconFile=LSIcon.ico
Compression=lzma
SolidCompression=yes
;show bacground window
;WindowResizable=yes
;WindowVisible=yes
;WindowStartMaximized=no

 
;SetupLogging file location C:\Users\jij\AppData\Local\Temp\Setup Log 2015-05-29 #003.txt
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
; DestDir: {app}; Source: Files\*; Excludes: "*.m,.svn,private"; Flags: recursesubdirs
; Source: "..\LSOmniWinService\LSOmniWinService\bin\Release\LSOmniServiceDbInitData.sql"; DestDir: "{app}"; Flags: ignoreversion
 
;all files should be in other include files
#include "FilesInclude.iss"

[Icons]
Name: "{group}\{cm:ProgramOnTheWeb,LSOmniService}"; Filename: "http://www.lsretail.com/"

[Run]
Filename: "{app}\LSOmni.WinService.exe"; Parameters: "/I"; WorkingDir: {app}; Flags: runascurrentuser; StatusMsg: "LSOmni.WinService is being installed. Please wait..."       
Filename: "{app}\StartService.cmd"; WorkingDir: {app}; Flags: postinstall skipifdoesntexist runascurrentuser
 

[UninstallRun]
Filename: "{app}\StopService.cmd"; WorkingDir: {app}; Flags: skipifdoesntexist runascurrentuser
Filename: "{app}\LSOmni.WinService.exe"; Parameters: "/U"; WorkingDir: {app}; Flags: runascurrentuser; StatusMsg: "LSOmni.WinService is being uninstalled. Please wait..."       

[Code]
//tried to have code in other files
#include "Library.iss"
#include "SqlPage.iss"

var
  //the custome pages used, defined in other iss files
  SqlCustomPage : TWizardPage;

procedure InitializeWizard;
begin

  { Create the pages }
  //from SqlPage.iss
  SqlCustomPage := SQLCustomForm_CreatePage(wpWelcome); 

  //should only set any texts here..
  SQLPage_txtDBname.Text := 'LSOmni';
     
end;

function AppSettingsChangeScript(): Boolean;
var  
  omniStr: string; 
  sqlUser,sqlPwd: string; 
begin
    Log('AppSettingsChangeScript called');  //called after install in CurStepChanged
    //check if we can even parse the xml
    if not ValidationXMLDomExists then
    begin
	  Log('AppSettingsChangeScript error: Failed to update LSOmni.WinService.exe.config file'  );
      MsgBox('Failed to update LSOmni.WinService.exe.config file'#13'LSOmni.WinService.exe.config file will not get updated'#13'with values entered', mbError, MB_OK);
    end;

    //standardize on the LSOmniUser
    sqlUser := 'LSOmniUser';
    sqlPwd := 'LSOmniUser';
    Result := True;
    try
      // LSOmni database string
      omniStr := 'Data Source=' + Trim(SQLPage_txtServer.Text) ;
      omniStr := omniStr + ';  Initial Catalog=' + Trim(SQLPage_txtDBname.Text) ;
      omniStr := omniStr + ';  User ID=' + sqlUser;
      omniStr := omniStr + ';  Password=' + sqlPwd;
      omniStr := omniStr + ';  Persist Security Info=True;MultipleActiveResultSets=True;Connection Timeout=10;';
      UpdateConfigFile('LSOmni.WinService.exe.config','SQLConnectionString.LSOmni', omniStr);
      
    except
	  Log('AppSettingsChangeScript error: ' + GetExceptionMessage );
      MsgBox(GetExceptionMessage, mbError, MB_OK);
      MsgBox('Check the values in the config file', mbError, MB_OK);
      Result := False;

    end;      
end; { AppSettingsChangeScript }

//Hook into setting up the sql and navsql installation     
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Log('PrepareToInstall() called');
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
      //must change the appsettings here because the files have been extracted to app folder
      AppSettingsChangeScript();
    end;
		//ssInstall :
		//  Alert(' ssInstall before install');
		//ssDone :
		//  Alert(' ssDone');
	//else
	//	Alert(' no idea ss');
	end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
    Result := True;
    if (CurPageID = wpSelectDir) and FileExists(ExpandConstant('{app}\LSOmni.WinService.exe.config')) then begin
        if (MsgBox('LSOmni.WinService.exe.config exists in this folder.  Do you want to override it?', mbConfirmation, MB_YESNO) = idNo)
        then   begin
        Result := False;
        end;
    end; 
 
  { Validate certain pages before allowing the user to proceed }
  {
  if CurPageID = SqlCustomPage.ID then begin
    if SQLServerPage.Values[0] = '' then begin
      MsgBox('You must enter your name.', mbError, MB_OK);
      Result := False;
    end else begin
      Result := True;
    end;
  end;
      }
   //  Result := True;
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
      else if not (IsDotNetDetected('v4\Client', 0) or IsDotNetDetected('v4\Full', 0)) then 
        begin
            MsgBox('This setup requires Microsoft .NET Framework 4.0 Client Profile.'#13#13
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
end;

function InitializeUninstall(): Boolean;
begin
  Result := True;
  //MsgBox('Only files will be removed.' #13#13 'Windows service will not be fully uninstalled. Run LSOmniWinService.exe /u', mbInformation, MB_OK) 
  //Result := False;
  //Result := MsgBox('Only files are removed.' #13#13 'SQL scripts', mbConfirmation, MB_YESNO) = idYes;
  //if Result = False then
  //  MsgBox('InitializeUninstall:' #13#13 'Ok, bye bye.', mbInformation, MB_OK);
end;
// CurPageID values for predefined wizard pages
//----------------------------------------------
// wpWelcome, wpLicense, wpPassword, wpInfoBefore, wpUserInfo, wpSelectDir, wpSelectComponents,
// wpSelectProgramGroup, wpSelectTasks, wpReady, wpPreparing, wpInstalling, wpInfoAfter, wpFinished

// http://www.vincenzo.net/isxkb/index.php?title=Test_events_example_including_Uninstall_events
 

 
