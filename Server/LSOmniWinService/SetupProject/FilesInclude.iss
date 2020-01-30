[Files]
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "..\LSOmniWinService\xsl\notification.xsl"; DestDir: "{app}\xsl"; Flags: ignoreversion
Source: "..\LSOmniWinService\xsl\notificationEmail.xsl"; DestDir: "{app}\xsl"; Flags: ignoreversion
Source: "..\LSOmniWinService\xsl\notification-example.xml"; DestDir: "{app}\xsl"; Flags: ignoreversion
Source: "..\LSOmniWinService\NLog.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LSOmniWinService\StartService.cmd"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LSOmniWinService\StopService.cmd"; DestDir: "{app}"; Flags: ignoreversion

Source: "..\LSOmniWinService\bin\Release\LSRetail.Omni.*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LSOmniWinService\bin\Release\LSOmni.*.dll"; DestDir: "{app}"; Flags: ignoreversion

Source: "..\LSOmniWinService\bin\Release\itextsharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LSOmniWinService\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LSOmniWinService\bin\Release\NLog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LSOmniWinService\bin\Release\zxing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LSOmniWinService\bin\Release\FirebaseNet.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LSOmniWinService\bin\Release\netstandard.dll"; DestDir: "{app}"; Flags: ignoreversion

Source: "..\LSOmniWinService\bin\Release\LSOmni.WinService.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LSOmniWinService\bin\Release\LSOmni.WinService.exe"; DestDir: "{app}"; Flags: ignoreversion

[Dirs]
Name: "{app}\logs"; Permissions: everyone-modify
Name: "{app}\Images"; Permissions: everyone-modify
Name: "{app}\xsl"; Permissions: everyone-modify
Name: "{app}"; Permissions: everyone-modify
