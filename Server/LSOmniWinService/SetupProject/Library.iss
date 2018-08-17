[Code]
const
  IISRegKey = 'SOFTWARE\Microsoft\InetStp';

// Used to generate error code by sql script errors
procedure ExitProcess(exitCode:integer);
  external 'ExitProcess@kernel32.dll stdcall';


function GetIISVersion(var MajorVersion, MinorVersion: DWORD): Boolean;
begin
 // return MajorVersion=0 and MinorVersion=0 if reg key not found
  Result := RegQueryDWordValue(HKLM, IISRegKey, 'MajorVersion', MajorVersion) and
    RegQueryDWordValue(HKLM, IISRegKey, 'MinorVersion', MinorVersion);
end;

function IsIIS7AboveInstalled: Boolean;
var
  MajorVersion: DWORD;
  MinorVersion: DWORD;
begin
  Result := GetIISVersion(MajorVersion, MinorVersion) and (MajorVersion >= 7);
  log('IsIIS75AboveInstalled called max: ' + IntToStr(MajorVersion)+ ' min: ' + IntToStr(MinorVersion) );
end;  

function GetIISVersionString: string;
var
  MajorVersion: DWORD;
  MinorVersion: DWORD;
begin
  GetIISVersion(MajorVersion, MinorVersion);
  Result := IntToStr(MajorVersion)+ '.' + IntToStr(MinorVersion) ;
end; 

procedure Alert(txt: string);
begin
  //mbInformation       mbError
  MsgBox(txt, mbInformation, MB_OK);
end;{procedure Alert}

procedure AlertInt(txtInt: Integer);
begin
  MsgBox(IntToStr(txtInt), mbInformation, MB_OK);
end;{procedure AlertInt}

function ValidationXMLDomExists() : Boolean;
var
  XMLDoc: Variant;

begin
   Log('ValidateXMLDOMDocExists called  ');
   Result := True;
  try
      XMLDoc := CreateOleObject('MSXML2.DOMDocument');
  except
      Result := False;
  end;  
end;

procedure UpdateAppSettingsConfig(keyName: string; newValue:string);
var
  XMLDoc, RootNode, Nodes, Node: Variant;
  ConfigFilename,Key: String;
  i: integer;

begin
   ConfigFilename := ExpandConstant('{app}') + '\AppSettings.config';
   Log('UpdateAppSettingsConfig called  ' + ConfigFilename);
   Log('UpdateAppSettingsConfig  ' + keyName + ' - ' + newValue);
  try
      XMLDoc := CreateOleObject('MSXML2.DOMDocument');
  except
     RaiseException('MSXML is required to complete the post-installation process.'#13#13'(Error ''' + GetExceptionMessage + ''' occurred)');
  end;  

  XMLDoc.async := False;
  XMLDoc.resolveExternals := False;
  XMLDoc.preserveWhiteSpace := True;

  XMLDoc.load(ConfigFilename);
  if XMLDoc.parseError.errorCode <> 0 then
    RaiseException('Error on line ' + IntToStr(XMLDoc.parseError.line) + ', position ' + IntToStr(XMLDoc.parseError.linepos) + ': ' + XMLDoc.parseError.reason);

  RootNode := XMLDoc.documentElement;
  //Nodes := RootNode.selectNodes('//configuration/appSettings/add');
  Nodes := RootNode.selectNodes('//appSettings/add');
  for i := 0 to Nodes.length - 1 do
  begin
    Node := Nodes.Item[i];
    if Node.NodeType = 1 then
    begin
      key := Node.getAttribute('key');
      Case key of
        keyName : Node.setAttribute('value', newValue);
      end;
    end;
  end;

  XMLDoc.Save(ConfigFilename); 

end;
procedure UpdateConfigFile(fileName: string; keyName: string; newValue:string);
var
  XMLDoc, RootNode, Nodes, Node: Variant;
  ConfigFilename,Key: String;
  i: integer;

begin
   ConfigFilename := ExpandConstant('{app}') + '\' + fileName;
   Log('UpdateConfigFile called  ' + ConfigFilename);
   Log('UpdateConfigFile  ' + keyName + ' - ' + newValue);
  try
      XMLDoc := CreateOleObject('MSXML2.DOMDocument');
  except
     RaiseException('MSXML is required to complete the post-installation process.'#13#13'(Error ''' + GetExceptionMessage + ''' occurred)');
  end;  

  XMLDoc.async := False;
  XMLDoc.resolveExternals := False;
  XMLDoc.preserveWhiteSpace := True;

  XMLDoc.load(ConfigFilename);
  if XMLDoc.parseError.errorCode <> 0 then
    RaiseException('Error on line ' + IntToStr(XMLDoc.parseError.line) + ', position ' + IntToStr(XMLDoc.parseError.linepos) + ': ' + XMLDoc.parseError.reason);

  RootNode := XMLDoc.documentElement;
  //Nodes := RootNode.selectNodes('//configuration/appSettings/add');
  Nodes := RootNode.selectNodes('//appSettings/add');
  for i := 0 to Nodes.length - 1 do
  begin
    Node := Nodes.Item[i];
    if Node.NodeType = 1 then
    begin
      key := Node.getAttribute('key');
      Case key of
        keyName : Node.setAttribute('value', newValue);
      end;
    end;
  end;

  XMLDoc.Save(ConfigFilename); 

end;
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, release, serviceCount: cardinal;
    check45, success: boolean;
begin
    // .NET 4.5 installs as update to .NET 4.0 Full
    if version = 'v4.5' then begin
        version := 'v4\Full';
        check45 := true;
    end else
        check45 := false;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0/4.5 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 uses additional value Release
    if check45 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= 378389);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end; {procedure IsDotNetDetected }
 
//function InitializeSetup(): Boolean;
//begin
//    if not IsDotNetDetected('v4\Client', 0) then begin
//        MsgBox('MyApp requires Microsoft .NET Framework 4.0 Client Profile.'#13#13
//            'Please use Windows Update to install this version,'#13
//            'and then re-run the MyApp setup program.', mbInformation, MB_OK);
//        result := false;
//    end else
//        result := true;
//end;
 