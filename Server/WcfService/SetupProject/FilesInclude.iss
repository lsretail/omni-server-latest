[Files]
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "..\Service\*.svc"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion

Source: "..\Service\bin\LSOmni*.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\LSRetail.Omni*.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion

Source: "..\Service\bin\CavemanTools.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\NLog.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\System.Runtime.Serialization.Primitives.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\System.Net.Http.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\System.Data.SqlClient.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\netstandard.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "..\Service\bin\Newtonsoft.Json.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion
Source: "LSIcon.ico"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion; 

; XML files
Source: "..\Service\Xsl\notification.xsl"; DestDir: "{app}\{code:WcfDir}\Xsl\"; Flags: ignoreversion

; XML files
Source: "..\Service\Xml\navdata.xml"; DestDir: "{app}\{code:WcfDir}\Xml\"; Flags: ignoreversion

; LS Recommend
Source: "..\..\3rdPartyComponents\LSRecommends\*.dll"; DestDir: "{app}\{code:WcfDir}\bin\"; Flags: ignoreversion

; Configs Files
Source: "..\Service\WebBindings.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "..\Service\WebBehaviors.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "..\Service\WebServices.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "..\Service\WebServices_SSL_NonSSL.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "..\Service\WebServices_SSL_Only.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "..\Service\NLog.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion
Source: "Default\AppSettings.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion; Check: UpdAppSettings
Source: "Default\Web.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion; Check: UpdAppSettings
Source: "Multi\AppSettings.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion; Check: UpdAppMultiSettings
Source: "Multi\Web.config"; DestDir: "{app}\{code:WcfDir}"; Flags: ignoreversion; Check: UpdAppMultiSettings

[Dirs]
Name: "{app}\{code:WcfDir}\logs"; Permissions: everyone-modify
Name: "{app}\{code:WcfDir}\Images"; Permissions: everyone-modify
Name: "{app}\{code:WcfDir}"; Permissions: everyone-modify
