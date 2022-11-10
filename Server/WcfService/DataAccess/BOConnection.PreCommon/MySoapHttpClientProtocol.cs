using System;
using System.Net;

// This class should replace SoapHttpClientProtocol in Reference.cs
// for all Web References (LSActivity, LSCentral, LSOData)
// sample: public partial class OmniWrapper : MySoapHttpClientProtocol

namespace LSOmni.DataAccess.BOConnection.PreCommon
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public class MySoapHttpClientProtocol : System.Web.Services.Protocols.SoapHttpClientProtocol
    {
        public string AuthToken { get; set; }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            var req = base.GetWebRequest(uri);
            if (string.IsNullOrEmpty(AuthToken) == false)
                req.Headers.Add("Authorization", AuthToken);
            return req;
        }
    }
}
