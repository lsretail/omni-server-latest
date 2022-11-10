[Code]

const
  IISRegKey = 'SOFTWARE\Microsoft\InetStp';

// Used to generate error code by sql script errors
procedure ExitProcess(exitCode:integer);
external 'ExitProcess@kernel32.dll stdcall';

function GetIISVersion(var MajorVersion, MinorVersion: DWORD) : Boolean;
begin
  // return MajorVersion=0 and MinorVersion=0 if reg key not found
  Result := RegQueryDWordValue(HKLM, IISRegKey, 'MajorVersion', MajorVersion) and RegQueryDWordValue(HKLM, IISRegKey, 'MinorVersion', MinorVersion);
end;

function IsIIS7AboveInstalled: Boolean;
var
  MajorVersion: DWORD;
  MinorVersion: DWORD;
begin
  Result := GetIISVersion(MajorVersion, MinorVersion) and (MajorVersion >= 7);
  log('IsIIS75AboveInstalled called max: ' + IntToStr(MajorVersion)+ ' min: ' + IntToStr(MinorVersion) );
end;  

function GetIISVersionString: String;
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
end;

procedure AlertInt(txtInt: Integer);
begin
  MsgBox(IntToStr(txtInt), mbInformation, MB_OK);
end;

procedure ErrorMsg(txtFunction: string; cmdMode: Boolean);
begin
  Log(txtFunction + ' Error:' + GetExceptionMessage);
  if (not cmdMode) then
    MsgBox(GetExceptionMessage, mbError, MB_OK);
end;

procedure ErrorWarningMsg(txtData: string; cmdMode: Boolean);
begin
  Log(txtData);
  if (not cmdMode) then
    MsgBox(txtData, mbError, MB_OK);
end;

function ValidationXMLDomExists(): Boolean;
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

procedure UpdateAppSettingsConfig(keyName: string; newValue: string; path: string);
var
  XMLDoc, RootNode, Nodes, Node: Variant;
  ConfigFilename,Key: String;
  i: integer;

begin
  ConfigFilename := path + '\AppSettings.config';
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

function GetCommandlineParamString(inParam: String; inDefault : String): String;
var
  LoopVar : Integer;
  BreakLoop : Boolean;
begin
  { Init the variable to known values }
  LoopVar :=0;
  Result := '';
  BreakLoop := False;

  { Loop through the passed in array to find the parameter }
  while ((LoopVar < ParamCount) and (not BreakLoop)) do
  begin
    { Determine if the looked for parameter is the next value }
    if ((ParamStr(LoopVar) = inParam) and ((LoopVar + 1) <= ParamCount)) then
    begin
      { Set the return result equal to the next command line parameter }
      Result := ParamStr(LoopVar + 1);

      { Break the loop }
      BreakLoop := True;
    end;

    { Increment the loop variable }
    LoopVar := LoopVar + 1;
  end;

  if (not BreakLoop) then
    Result := inDefault;
end;

function GetCommandlineParamBoolean(inParam: String; inDefault : Boolean): Boolean;
var
  res : String;
begin
  Result := False;
  res := GetCommandlineParamString(inParam, '');
  
  if (res = '') then
    Result := inDefault
  else if (res = 'true') then
    Result := True;
end;

function GetCommandlineParamInteger(inParam: String; inDefault : Integer): Integer;
var
  res : String;
begin
  Result := 0;
  res := GetCommandlineParamString(inParam, '');
  
  if (res = '') then
  begin
    Result := inDefault
  end
  else
  begin
    Result := StrToIntDef(res, inDefault);
  end;
end;

// Indicates whether the specified version and service pack of the .NET Framework is installed.
// version -- Specify one of these strings for the required .NET Framework version:
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1'          .NET Framework 1.1
//    'v2.0'          .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//    'v4.5.1'        .NET Framework 4.5.1
//    'v4.5.2'        .NET Framework 4.5.2
//    'v4.6'          .NET Framework 4.6
//    'v4.6.1'        .NET Framework 4.6.1
//    'v4.6.2'        .NET Framework 4.6.2
//    'v4.7'          .NET Framework 4.7
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
function IsDotNetDetected(version: string; service: cardinal): boolean;
var
  key, versionKey: string;
  install, release, serviceCount, versionRelease: cardinal;
  success: boolean;
begin
  versionKey := version;
  versionRelease := 0;

  // .NET 1.1 and 2.0 embed release number in version key
  if version = 'v1.1' then 
  begin
    versionKey := 'v1.1.4322';
  end 
  else if version = 'v2.0' then 
  begin
    versionKey := 'v2.0.50727';
  end
  else if Pos('v4.', version) = 1 then 
  begin
  // .NET 4.5 and newer install as update to .NET 4.0 Full
    versionKey := 'v4\Full';
    case version of
      'v4.5':   versionRelease := 378389;
      'v4.5.1': versionRelease := 378675; // 378758 on Windows 8 and older
      'v4.5.2': versionRelease := 379893;
      'v4.6':   versionRelease := 393295; // 393297 on Windows 8.1 and older
      'v4.6.1': versionRelease := 394254; // 394271 on Windows 8.1 and older
      'v4.6.2': versionRelease := 394802; // 394806 on Windows 8.1 and older
      'v4.7':   versionRelease := 460798; // 460805 before Win10 Creators Update
    end;
  end;

  // installation key group for all .NET versions
  key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + versionKey;

  // .NET 3.0 uses value InstallSuccess in subkey Setup
  if Pos('v3.0', version) = 1 then 
  begin
    success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
  end 
  else 
  begin
    success := RegQueryDWordValue(HKLM, key, 'Install', install);
  end;

  // .NET 4.0 and newer use value Servicing instead of SP
  if Pos('v4', version) = 1 then 
  begin
    success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
  end 
  else 
  begin
    success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
  end;

  // .NET 4.5 and newer use additional value Release
  if versionRelease > 0 then 
  begin
    success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
    success := success and (release >= versionRelease);
  end;

  result := success and (install = 1) and (serviceCount >= service);
end; {procedure IsDotNetDetected }
