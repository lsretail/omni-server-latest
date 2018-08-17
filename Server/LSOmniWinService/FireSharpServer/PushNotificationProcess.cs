using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using FirebaseNet.Exceptions;
using FirebaseNet.Messaging;
using LSOmni.BLL.Loyalty;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using NLog;

namespace LSOmni.FireSharpServer
{
    public class PushNotificationProcess
    {
        private FCMClient client;
        private static Logger logger;
        private List<PushNotification> successfullySentList;
        private List<PushNotification> failedToSendList;
        private DateTime dateLastChecked;
        private PushNotificationBLL bll;
        public PushNotificationProcess()
        {
            client = new FCMClient(ConfigSetting.GetString("Firebase.Secret"));
            logger = LogManager.GetCurrentClassLogger();
            successfullySentList = new List<PushNotification>();
            failedToSendList = new List<PushNotification>();
            dateLastChecked = new DateTime(1900, 1, 1);
            bll = new PushNotificationBLL();
        }

        public void Start()
        {
            logger.Info("PushNotificationProcess started");

            bool pushNotificationEnabled = false, apiKeyMissing = true;
            int intervalInSeconds = 20;
            string enabledKey = "BackgroundProcessing.PushNotification.Enabled"; //key in app.settings
            string durInSeconds = "BackgroundProcessing.PushNotification.DurationInSeconds"; //key in app.settings
            string apiKey = "Firebase.Secret"; //Key in app.settings

            if (ConfigSetting.KeyExists(apiKey))
            {
                apiKey = ConfigSetting.GetString(apiKey);
                apiKeyMissing = string.IsNullOrEmpty(apiKey);
            }

            if (apiKeyMissing)
            {
                logger.Info("Firebase.Secret is missing, stopping the PushNotificationProcess() ");
                return;
            }

            if (ConfigSetting.KeyExists(enabledKey))
            {
                pushNotificationEnabled = ConfigSetting.GetBoolean(enabledKey);
                if (ConfigSetting.KeyExists(durInSeconds))
                    intervalInSeconds = ConfigSetting.GetInt(durInSeconds);
            }

            if (!pushNotificationEnabled)
            {
                logger.Info("BackgroundProcessing.PushNotification.Enabled = false, stopping the PushNotificationProcess() ");
                return;
            }

            try
            {
                while (pushNotificationEnabled)
                {
                    successfullySentList.Clear();
                    failedToSendList.Clear();


                    if (!bll.PushNotificationsDoesExist(dateLastChecked))
                    {
                        logger.Info("No PushNotifications to process, sleeping for: {0} sec.  CreateDate: {1}",
                            intervalInSeconds, dateLastChecked.ToString(CultureInfo.InvariantCulture));
                        Thread.Sleep(1000 * intervalInSeconds);
                        continue;
                    }

                    try
                    {
                        List<PushNotification> list = bll.PushNotificationsGetNext(dateLastChecked);

                        logger.Info("PushSharpProcess processing count: {0}:  dateLastChecked: {1}: ",
                            list.Count, dateLastChecked.ToString(CultureInfo.InvariantCulture));

                        foreach (PushNotification pushNotification in list)
                        {
                            dateLastChecked = pushNotification.CreatedDate;
                            Dictionary<string, string> data = new Dictionary<string, string>();
                            data.Add("OmniId", pushNotification.NotificationId);
                            var push = new Message()
                            {
                                RegistrationIds = pushNotification.DeviceIds,
                                Notification = new PushMessage()
                                {
                                    Title = pushNotification.Title,
                                    Body = pushNotification.Body
                                },
                                Data = data,
                            };
                            SendMessage(push, pushNotification);
                        }

                    }
                    catch
                    {
                    }
                    Thread.Sleep(1000 * intervalInSeconds);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "PushNotificationProcess startup failed.");
            }
        }

        private async void SendMessage(Message msg, PushNotification push)
        {
            try
            {
                var result = await client.SendMessageAsync(msg);
                if (result is DownstreamMessageResponse)
                {
                    var res = result as DownstreamMessageResponse;
                    if (res.Failure == 0)
                    {
                        bll.PushNotificationUpdateToSent(push.ContactId, push.NotificationId);
                    }
                    else
                    {
                        bll.PushNotificationUpdateCounter(push.ContactId, push.NotificationId);
                    }
                }
            }
            catch (FCMException e)
            {
                logger.Log(LogLevel.Error, e, e.Message);
                bll.PushNotificationUpdateCounter(push.ContactId, push.NotificationId);
            }
            
        }
    }
}
