using System;
using System.Collections.Generic;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Login
{
    public interface IFacebookLoginService
    {
        event EventHandler<FacebookLoginEventArgs> OnLoginSuccess;
        event EventHandler<string> OnLoginFailure;

        void Login();
    }

    public class FacebookLoginEventArgs
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
