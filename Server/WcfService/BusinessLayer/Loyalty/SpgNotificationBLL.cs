using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;

namespace LSOmni.BLL.Loyalty
{
    public class SpgNotificationBLL : BaseLoyBLL
    {
        private readonly ISpgNotificationRepository iSpgRepository;
        static object locklog = new object();

        public SpgNotificationBLL(BOConfiguration config, int timeoutInSeconds = 15)
            : base(config, timeoutInSeconds)
        {
            this.iSpgRepository = GetDbRepository<ISpgNotificationRepository>(config);
        }

        public virtual void RegisterNotification(string cardId, string token, Statistics stat)
        {
            this.iSpgRepository.RegisterNotification(cardId, token, stat);
        }

        public virtual void Delete(string cardId, Statistics stat)
        {
            this.iSpgRepository.Delete(cardId, stat);
        }

        public virtual string GetToken(string cardId, Statistics stat)
        {
            return this.iSpgRepository.GetToken(cardId, stat);
        }

        public virtual void AddLogEntry(string cardId, string message, bool hasError, string errorMsg, Statistics stat)
        {
            this.iSpgRepository.AddLogEntry(cardId, message, hasError, errorMsg, stat);
        }

        public virtual bool GetSpgNotificationData(SpgMessageType type, out string body, out string title)
        {
            body = string.Empty;
            title = string.Empty;
            TextReader tr = new StringReader(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "spg", "notification.xml")));
            XDocument doc = XDocument.Load(tr);
            List<XElement> el1 = doc.Elements("notifications").Elements().ToList();
            foreach (XElement el in el1)
            {
                SpgMessageType mType = (SpgMessageType)XMLHelper.StringToEnum(typeof(SpgMessageType), el.Element("type").Value);
                if (mType == type)
                {
                    body = el.Element("body").Value;
                    title = el.Element("title").Value;
                    logger.Debug(config.LSKey.Key, $"Found SPG LogEntry:{type} body: {body}");
                    return true;
                }
            }
            return false;
        }

        public virtual void SendNotification(string cardId, string title, string body, Statistics stat)
        {
            logger.Debug(config.LSKey.Key, $"Check SPG Notification for:{cardId}");

            string token = GetToken(cardId, stat);
            if (token == null)
                return;

            SendSpgMessage(cardId, title, body, token);
        }

        private async void SendSpgMessage(string cardId, string title, string body, string token)
        {
            logger.Debug(config.LSKey.Key, $"SPG Msg - CardId:{cardId} Body:{body}");

            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "spg", "firebase.json"))
                    });
                }

                Message message = new Message()
                {
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Token = token
                };

                // Send the message
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                lock (locklog)
                {
                    iSpgRepository.AddLogEntry(cardId, body, false, response, new Statistics());
                    logger.Debug(config.LSKey.Key, response);
                }

            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                lock (locklog)
                {
                    iSpgRepository.AddLogEntry(cardId, body, true, ex.Message, new Statistics());
                }
            }
        }
    }

    public enum SpgMessageType
    {
        None,
        ItemAdd,
        ItemDel,
        UserAdd,
        UserDel
    }
}

 