using System;

namespace LSRetail.Omni.Domain.DataModel.Base.Utils
{
    public enum EmailStatus
    {
        New = 0,         
        InProcess = 1,   
        Failed = 2,      
        Processed = 3,
        RetryLater = 4,  //failed now but retry later
    }
    public enum EmailType
    {
        Unknown = 0,   // no specific type
        OrderMessage = 1,        //email is of type ordermessage 
        ResetEmail = 2,          //email is of type resetting the email
        EmailReceipt = 3,       //email is of type emailing the receipt 
        EmailRegistration = 4,  //email is of type emailing the registration
    }
 
 
    public class EmailMessage
    {
        public EmailMessage()
        {

            Guid = string.Empty;
            EmailFrom = string.Empty;
            EmailTo = string.Empty;
            EmailCc = string.Empty;
            Subject = string.Empty;
            Body = string.Empty;
            Error = string.Empty;
            Attachments = string.Empty;
            ExternalId = string.Empty;
            EmailStatus = EmailStatus.New;
            EmailType = EmailType.Unknown;
            RetryCounter = 0;
        }

        public string Guid { get; set; } // 
        public string EmailFrom { get; set; } //
        public string EmailTo { get; set; } //
        public string EmailCc { get; set; } //
        public EmailStatus EmailStatus { get; set; } //
        public EmailType EmailType { get; set; } //
        public string Subject { get; set; } //
        public string Body { get; set; } //
        public string Attachments { get; set; } //
        public string Error { get; set; } //
        public string ExternalId { get; set; } //
        public DateTime DateCreated { get; set; } //
        public DateTime DateLastModified { get; set; } //
        public int RetryCounter { get; set; } //
    }
}
