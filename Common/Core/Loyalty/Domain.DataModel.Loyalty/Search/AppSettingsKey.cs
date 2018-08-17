using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Domain.Search
{
    public enum AppSettingsKey
    {
        ContactUs = 0,
        SchemeLevel = 1,
        TermsOfService = 2,

        forgotpassword_email_subject = 3,
        forgotpassword_email_body = 4,
        forgotpassword_email_url = 5,
        forgotpassword_device_email_subject = 6,
        forgotpassword_device_email_body = 7,

        //aswaaq specific
        resetpin_email_subject = 200,
        resetpin_email_body = 201,

        Password_Policy = 220,  
    }
}
