using System;
using System.Collections.Generic;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Login
{
    public interface IGoogleLoginService
    {
        event EventHandler<GoogleLoginEventArgs> OnLoginSuccess;
        event EventHandler<string> OnLoginFailure;

        void Login();
    }

    public class GoogleLoginEventArgs
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
