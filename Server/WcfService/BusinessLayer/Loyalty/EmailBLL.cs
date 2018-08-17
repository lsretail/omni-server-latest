using System;
using System.Collections.Generic;
using System.Net;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.BLL.Loyalty
{
    public class EmailBLL : BaseLoyBLL
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        //private IAppSettingsRepository iAppSettingsRepository;
        private IEmailMessageRepository iEmailMessageRepository;

        public EmailBLL(int timeoutInSeconds = 15)
            : base(timeoutInSeconds)
        {
            //this.iAppSettingsRepository = GetDbRepository<IAppSettingsRepository>();
            iEmailMessageRepository = base.GetDbRepository<IEmailMessageRepository>();
        }

        public virtual int MaxRetryCounter()
        {
            return iEmailMessageRepository.MaxRetryCounter();
        }

        public virtual void EmailRegistration(string emailTo, string name, string userName)
        {
            //NOTE - we/I are taking text formatted by NAV and trying to make sense of it.
            if (Validation.IsValidEmail(emailTo) == false)
                throw new LSOmniServiceException(StatusCode.EmailInvalid, string.Format("Invalid email: {0}", emailTo));

            AppSettingsBLL bll = new AppSettingsBLL();
            string subject = bll.AppSettingsGetByKey(AppSettingsKey.Registration_Email_Subject);
            string body = bll.AppSettingsGetByKey(AppSettingsKey.Registration_Email_Body);

            EmailBLL emailBLL = new EmailBLL();
            EmailMessage emailMessage = new EmailMessage();

            // Variables to replace: [NAME]  [LOGIN]
            body = body.Replace("[NAME]", name);
            body = body.Replace("[LOGIN]", userName);

            emailMessage.Body = body;
            emailMessage.EmailCc = "";
            emailMessage.EmailFrom = ""; //read from config file
            emailMessage.EmailTo = emailTo;
            emailMessage.EmailType = EmailType.EmailRegistration;
            emailMessage.ExternalId = "";
            emailMessage.Subject = subject;

            emailBLL.Save(emailMessage);
        }

        public virtual List<EmailMessage> EmailMessageSearch(DateTime? dateFrom, DateTime? dateTo, EmailStatus emailStatus)
        {
            try
            {
                List<EmailMessage> list = iEmailMessageRepository.EmailMessageSearch(dateFrom, dateTo, emailStatus);
                return list;
            }
            catch (Exception ex)
            {
                string msg = string.Format("dateFrom: {0}  dateTo: {1}  emailStatus: {2} ",
                    dateFrom.ToString(), dateTo.ToString(), emailStatus.ToString());
                logger.Log(NLog.LogLevel.Error, ex, msg);
                throw;
            }
        }

        public virtual void UpdateStatus(string guid, EmailStatus emailStatus, string errorMessage)
        {
            iEmailMessageRepository.UpdateStatus(guid, emailStatus, errorMessage);
        }

        public virtual void Save(string emailTo, string subject, string body, EmailType emailType)
        {
            try
            {
                if (Validation.IsValidEmail(emailTo) == false)
                    throw new ApplicationException("Invalid emailTo: " + emailTo);

                EmailMessage emailMessage = new EmailMessage();
                emailMessage.Body = body;
                emailMessage.Subject = subject;
                emailMessage.EmailTo = emailTo;
                emailMessage.EmailType = emailType; // EmailType.ResetEmail;

                emailMessage.Guid = GuidHelper.NewGuidString();
                emailMessage.EmailCc = "";
                emailMessage.EmailFrom = ""; //read from config file
                emailMessage.EmailStatus = EmailStatus.New;
                emailMessage.ExternalId = ""; // 
                emailMessage.Error = "";
                emailMessage.DateCreated = DateTime.Now;
                emailMessage.DateLastModified = DateTime.Now;
                iEmailMessageRepository.Save(emailMessage);
            }
            catch (Exception ex)
            {
                string msg = string.Format("emailTo: {0}  subject: {1}  body: {2}  emailType: {3}", emailTo, subject, body, emailType.ToString());
                logger.Log(NLog.LogLevel.Error, ex, msg);
                throw;
            }
        }

        public virtual void Save(EmailMessage emailMessage)
        {
            try
            {
                emailMessage.EmailStatus = EmailStatus.New;
                emailMessage.DateCreated = DateTime.Now;
                emailMessage.DateLastModified = DateTime.Now;
                iEmailMessageRepository.Save(emailMessage);
            }
            catch (Exception ex)
            {
                string msg = string.Format("emailTo: {0}  subject: {1}  body: {2}  emailType: {3}",
                    emailMessage.EmailTo, emailMessage.Subject, emailMessage.Body, emailMessage.EmailType.ToString());
                logger.Log(NLog.LogLevel.Error, ex, msg);
                throw;
            }
        }

        public virtual void ProcessEmails(string imageFolderName)
        {
            try
            {
                bool emailSent = false;
                string fileName = "";
                List<EmailMessage> newList = this.EmailMessageSearch(DateTime.Now.AddYears(-1), null, EmailStatus.New);
                List<EmailMessage> retryLater = this.EmailMessageSearch(DateTime.Now.AddYears(-1), null, EmailStatus.RetryLater);

                //combine these list into one
                List<EmailMessage> list = new List<EmailMessage>();
                list.AddRange(newList);
                list.AddRange(retryLater);
                foreach (EmailMessage email in list)
                {
                    //send email  
                    try
                    {
                        logger.Info("Processing EmailMessage for Guid: {0}  email.EmailTo: {1}  (total:{2})",
                            email.Guid, email.EmailTo, list.Count);
                        string emailFrom = (string.IsNullOrWhiteSpace(email.EmailFrom) ? "" : email.EmailFrom);//read from config file anyway

                        //Handle the email of attachments (order messages). The QR code is inline in email
                        //ExternalId+"jpg" is the name of the file
                        if (email.Body.Contains("cid:") && !string.IsNullOrWhiteSpace(email.ExternalId))
                        {
                            //send attachment 
                            fileName = string.Format(@"{0}\{1}.jpg", imageFolderName, email.ExternalId);
                            if (ImageConverter.FileExists(fileName))
                            {
                                Email.SendEmailAsHtml(emailFrom, email.EmailTo, email.Subject, email.Body, fileName, true);
                                emailSent = true;
                            }
                            else
                            {
                                //check if this is png file
                                fileName = fileName.Replace(".jpg", ".png");
                                if (ImageConverter.FileExists(fileName))
                                {
                                    Email.SendEmailAsHtml(emailFrom, email.EmailTo, email.Subject, email.Body, fileName, true);
                                    emailSent = true;
                                }
                            }
                        }
                        else if (string.IsNullOrWhiteSpace(email.Attachments) == false)
                        {
                            //send attachment 
                            fileName = email.Attachments; //TODO handle many files with  ;  separator between file names ?
                            if (ImageConverter.FileExists(fileName))
                            {
                                Email.SendEmailAsHtml(emailFrom, email.EmailTo, email.Subject, email.Body, fileName, false);
                                emailSent = true;
                            }
                        }
                        else
                        {
                            Email.SendEmailAsHtml(emailFrom, email.EmailTo, email.Subject, email.Body);
                            emailSent = true;
                        }
                        //update status to processed or failed
                        if (emailSent)
                            this.UpdateStatus(email.Guid, EmailStatus.Processed, "Success");
                        else
                        {
                            this.UpdateStatus(email.Guid, EmailStatus.Failed, 
                                string.Format("Failed - attachment file not found: {0} on {1}", fileName, Dns.GetHostName()));
                        }
                    }
                    catch (System.Net.Mail.SmtpException sex)
                    {
                        logger.Error(sex);
                        switch (sex.StatusCode)
                        {
                            case System.Net.Mail.SmtpStatusCode.BadCommandSequence:
                            case System.Net.Mail.SmtpStatusCode.MailboxNameNotAllowed:
                            case System.Net.Mail.SmtpStatusCode.HelpMessage:
                            case System.Net.Mail.SmtpStatusCode.SyntaxError:
                            case System.Net.Mail.SmtpStatusCode.SystemStatus:
                                // ErrorCannotSend;
                                this.UpdateStatus(email.Guid, EmailStatus.Failed, sex.Message);
                                break;
                            case System.Net.Mail.SmtpStatusCode.CannotVerifyUserWillAttemptDelivery:
                            case System.Net.Mail.SmtpStatusCode.UserNotLocalWillForward:
                                //SentMaybe;
                                this.UpdateStatus(email.Guid, EmailStatus.Processed, sex.StatusCode.ToString());
                                break;
                            case System.Net.Mail.SmtpStatusCode.ClientNotPermitted:
                            case System.Net.Mail.SmtpStatusCode.CommandNotImplemented:
                            case System.Net.Mail.SmtpStatusCode.CommandParameterNotImplemented:
                            case System.Net.Mail.SmtpStatusCode.CommandUnrecognized:
                            case System.Net.Mail.SmtpStatusCode.ExceededStorageAllocation:
                            case System.Net.Mail.SmtpStatusCode.GeneralFailure:
                            case System.Net.Mail.SmtpStatusCode.InsufficientStorage:
                            case System.Net.Mail.SmtpStatusCode.LocalErrorInProcessing:
                            case System.Net.Mail.SmtpStatusCode.MailboxBusy:
                            case System.Net.Mail.SmtpStatusCode.MailboxUnavailable:
                            case System.Net.Mail.SmtpStatusCode.MustIssueStartTlsFirst:
                            case System.Net.Mail.SmtpStatusCode.ServiceClosingTransmissionChannel:
                            case System.Net.Mail.SmtpStatusCode.ServiceNotAvailable:
                            case System.Net.Mail.SmtpStatusCode.ServiceReady:
                            case System.Net.Mail.SmtpStatusCode.StartMailInput:
                            case System.Net.Mail.SmtpStatusCode.TransactionFailed:
                            case System.Net.Mail.SmtpStatusCode.UserNotLocalTryAlternatePath:
                                // TryAgain;
                                if (email.RetryCounter <= this.MaxRetryCounter())
                                    this.UpdateStatus(email.Guid, EmailStatus.RetryLater, sex.StatusCode.ToString());
                                else
                                    this.UpdateStatus(email.Guid, EmailStatus.Failed, sex.Message);
                                break;
                            case System.Net.Mail.SmtpStatusCode.Ok:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                        this.UpdateStatus(email.Guid, EmailStatus.Failed, ex.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
        }
    }
}

 