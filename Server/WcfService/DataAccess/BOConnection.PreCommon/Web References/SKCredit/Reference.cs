﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace LSOmni.DataAccess.BOConnection.PreCommon.SKCredit {
    using System.Diagnostics;
    using System;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System.Web.Services;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="CustomerCreditCheck_Binding", Namespace="urn:microsoft-dynamics-schemas/codeunit/CustomerCreditCheck")]
    public partial class CustomerCreditCheck : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback CallCustomerCreditCheckOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public CustomerCreditCheck() {
            this.Url = global::LSOmni.DataAccess.BOConnection.PreCommon.Properties.Settings.Default.LSOmni_DataAccess_BOConnection_PreCommon_SKCredit_CustomerCreditCheck;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event CallCustomerCreditCheckCompletedEventHandler CallCustomerCreditCheckCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("urn:microsoft-dynamics-schemas/codeunit/CustomerCreditCheck:CustomerCreditCheck", RequestElementName="CustomerCreditCheck", RequestNamespace="urn:microsoft-dynamics-schemas/codeunit/CustomerCreditCheck", ResponseElementName="CustomerCreditCheck_Result", ResponseNamespace="urn:microsoft-dynamics-schemas/codeunit/CustomerCreditCheck", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void CallCustomerCreditCheck(string memberCardNo, decimal salesAmount, ref bool saleIsApproved, ref decimal availableAmount, ref string responseCode, ref string errorText) {
            object[] results = this.Invoke("CallCustomerCreditCheck", new object[] {
                        memberCardNo,
                        salesAmount,
                        saleIsApproved,
                        availableAmount,
                        responseCode,
                        errorText});
            saleIsApproved = ((bool)(results[0]));
            availableAmount = ((decimal)(results[1]));
            responseCode = ((string)(results[2]));
            errorText = ((string)(results[3]));
        }
        
        /// <remarks/>
        public void CallCustomerCreditCheckAsync(string memberCardNo, decimal salesAmount, bool saleIsApproved, decimal availableAmount, string responseCode, string errorText) {
            this.CallCustomerCreditCheckAsync(memberCardNo, salesAmount, saleIsApproved, availableAmount, responseCode, errorText, null);
        }
        
        /// <remarks/>
        public void CallCustomerCreditCheckAsync(string memberCardNo, decimal salesAmount, bool saleIsApproved, decimal availableAmount, string responseCode, string errorText, object userState) {
            if ((this.CallCustomerCreditCheckOperationCompleted == null)) {
                this.CallCustomerCreditCheckOperationCompleted = new System.Threading.SendOrPostCallback(this.OnCallCustomerCreditCheckOperationCompleted);
            }
            this.InvokeAsync("CallCustomerCreditCheck", new object[] {
                        memberCardNo,
                        salesAmount,
                        saleIsApproved,
                        availableAmount,
                        responseCode,
                        errorText}, this.CallCustomerCreditCheckOperationCompleted, userState);
        }
        
        private void OnCallCustomerCreditCheckOperationCompleted(object arg) {
            if ((this.CallCustomerCreditCheckCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.CallCustomerCreditCheckCompleted(this, new CallCustomerCreditCheckCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    public delegate void CallCustomerCreditCheckCompletedEventHandler(object sender, CallCustomerCreditCheckCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9032.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class CallCustomerCreditCheckCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal CallCustomerCreditCheckCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public bool saleIsApproved {
            get {
                this.RaiseExceptionIfNecessary();
                return ((bool)(this.results[0]));
            }
        }
        
        /// <remarks/>
        public decimal availableAmount {
            get {
                this.RaiseExceptionIfNecessary();
                return ((decimal)(this.results[1]));
            }
        }
        
        /// <remarks/>
        public string responseCode {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[2]));
            }
        }
        
        /// <remarks/>
        public string errorText {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[3]));
            }
        }
    }
}

#pragma warning restore 1591