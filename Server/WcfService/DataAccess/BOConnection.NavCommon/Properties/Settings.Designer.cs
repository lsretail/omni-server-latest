﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LSOmni.DataAccess.BOConnection.NavCommon.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.4.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://172.22.1.48:7047/DynamicsNAV70/WS/Mobile%20NOP%202013/Codeunit/RetailWebSe" +
            "rvices")]
        public string LSRetail_Mobile_WebService_MemberMgt_DataAccess_BOConnection_Nav_NavWebReference_RetailWebServices {
            get {
                return ((string)(this["LSRetail_Mobile_WebService_MemberMgt_DataAccess_BOConnection_Nav_NavWebReference_" +
                    "RetailWebServices"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://dhqsrvomni005:7447/BC140/WS/CRONUS%20LS%201402%20W1%20Demo/Codeunit/Activi" +
            "ty")]
        public string LSOmni_DataAccess_BOConnection_NavCommon_LSActivity_Activity {
            get {
                return ((string)(this["LSOmni_DataAccess_BOConnection_NavCommon_LSActivity_Activity"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://dhqsrvomni001:9047/LSCentralMaster/WS/CRONUS%20-%20LS%20Central/Codeunit/O" +
            "mniWrapper")]
        public string LSOmni_DataAccess_BOConnection_NavCommon_NavWS_OmniWrapper {
            get {
                return ((string)(this["LSOmni_DataAccess_BOConnection_NavCommon_NavWS_OmniWrapper"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://dhqsrvomni001:9047/LSCentralMaster/WS/CRONUS%20-%20LS%20Central/Codeunit/A" +
            "ctivity")]
        public string LSOmni_DataAccess_BOConnection_NavCommon_LSActivity15_Activity {
            get {
                return ((string)(this["LSOmni_DataAccess_BOConnection_NavCommon_LSActivity15_Activity"]));
            }
        }
    }
}
