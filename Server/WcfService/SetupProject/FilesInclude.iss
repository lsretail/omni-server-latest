[Files]
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "..\Service\*.svc"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion

Source: "..\Service\bin\LSOmni*.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\LSRetail.Omni*.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "LSIcon.ico"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion; 

Source: "..\Service\bin\System.Runtime.Serialization.Primitives.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\System.Net.Http.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\System.Data.SqlClient.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\netstandard.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion

Source: "..\Service\bin\NLog.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\Braintree.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\zxing*.*"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\Newtonsoft.Json.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion

; XML files
Source: "..\Service\Xsl\notification.xsl"; DestDir: "{app}\{code:WcfDir}\Xsl\"; Flags: ignoreversion

; XML files
Source: "..\Service\Xml\navdata.xml"; DestDir: "{app}\{code:WcfDir}\Xml\"; Flags: ignoreversion

; SPG Notification
Source: "..\Service\bin\FirebaseAdmin.*"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion; Check: UseSPG
Source: "..\Service\bin\Google.*.*"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion; Check: UseSPG
Source: "..\Service\bin\System.*.*"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion; Check: UseSPG
Source: "..\Service\Spg\firebase.json"; DestDir: "{app}\{code:WcfDir}\Spg\"; Flags: ignoreversion; Check: UseSPG
Source: "..\Service\Spg\notification.xml"; DestDir: "{app}\{code:WcfDir}\Spg\"; Flags: ignoreversion; Check: UseSPG

; Configs Files
Source: "..\Service\Web.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "..\Service\WebBindings.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "..\Service\WebBehaviors.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "..\Service\WebServices.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "..\Service\WebServices_SSL.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "..\Service\WebServices_Basic.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "..\Service\NLog.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "Default\AppSettings.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion; Check: UpdAppSettings
Source: "Multi\AppSettings.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion; Check: UpdAppMultiSettings

[Dirs]
Name: "{app}\{code:WcfDir}\logs"; Permissions: everyone-modify
Name: "{app}\{code:WcfDir}\Images"; Permissions: everyone-modify
Name: "{app}\{code:WcfDir}"; Permissions: everyone-modify
