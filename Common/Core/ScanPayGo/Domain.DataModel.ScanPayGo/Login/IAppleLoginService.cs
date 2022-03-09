using System;
using System.Collections.Generic;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Login
{
    public interface IAppleLoginService
    {
        event EventHandler<AppleLoginEventArgs> OnLoginSuccess;
        event EventHandler<string> OnLoginFailure;
    }

    public class AppleLoginEventArgs
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
