using System;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Xml.Xsl;
using System.Drawing;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Transactions;

namespace LSOmni.BLL.Loyalty
{
    public class OrderMessageBLL : BaseLoyBLL
    {
        private static LSLogger logger = new LSLogger();
        private IPayRequestRepository iPayRequestRepository;
        private INotificationRepository iNotificationRepository;
        private IPushNotificationRepository iPushRepository;

        public OrderMessageBLL(BOConfiguration config, string deviceId, int timeoutInSeconds)
            : base(config, deviceId, timeoutInSeconds)
        {
            iPayRequestRepository = base.GetDbRepository<IPayRequestRepository>(config);
            iNotificationRepository = GetDbRepository<INotificationRepository>(config);
            iPushRepository = GetDbRepository<IPushNotificationRepository>(config);
        }

        public OrderMessageBLL(BOConfiguration config, int timeoutInSeconds) 
            : this(config, "", timeoutInSeconds)
        {
        }

        public virtual OrderMessage OrderMessageSave(OrderMessage orderMessage)
        {
            try
            {
                //validation
                if (orderMessage == null)
                {
                    string msg = "OrderMessageSave() is null";
                    throw new LSOmniServiceException(StatusCode.Error, msg);
                }
                CreateNotificationsFromOrderMessage(orderMessage);

                string resp = SendToEcom(new { Id = orderMessage.Id, Status = orderMessage.OrderMessageStatus });

                return orderMessage;
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                throw;
            }
        }

        public virtual string OrderMessageRequestPayment(string orderId, OrderMessagePayStatus status, decimal amount, string token)
        {
            try
            {
                EComRequestPayment ev = new EComRequestPayment();
                ev.OrderId = orderId;
                ev.Status = status;
                ev.Id = iPayRequestRepository.NewRequest(orderId);
                ev.Amount = amount;
                ev.Token = token;

                return SendToEcom(ev);
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                return "ERROR:" + ex.Message;
            }
        }

        #region private

        private string NotificationHtmlFromOrderMessageXml(OrderMessage message, MemberContact contact)
        {
            #region xml
            /*
             *this here http://www.w3schools.com/xsl/tryxslt.asp?xmlfile=cdcatalog&xsltfile=cdcatalog
        <OmniMessage>
            <OrderStatus>Ready</OrderStatus>
            <QRC>
                <CustomerOrder>
                    <DocStatus>1</DocStatus>
                    <DocID>CO00000287</DocID>
                </CustomerOrder>
            </QRC>
            <Order>
                <DocID>CO00000287</DocID>
                <DocDate>2014-10-30T16:05:52.54Z</DocDate>
                <MemberCardNo>10008</MemberCardNo>
                <StoreToCollect>S0009</StoreToCollect>
                <CollectTimeLimit>2014-11-08T17:00:00Z</CollectTimeLimit>
                <OrderLinesToCollect>
                    <Line>
                        <ItemDescription>Briefcase, Leather</ItemDescription>
                        <VariantCode></VariantCode>
                        <UOM></UOM>
                        <Quantity>1</Quantity>
                        <Amount>800</Amount>
                    </Line>
                    <Line>
                        <ItemDescription>Briefcase, Leather</ItemDescription>
                        <VariantCode></VariantCode>
                        <UOM></UOM>
                        <Quantity>1</Quantity>
                        <Amount>800</Amount>
                    </Line>
                    <Line>
                        <ItemDescription>Briefcase, Leather</ItemDescription>
                        <VariantCode></VariantCode>
                        <UOM></UOM>
                        <Quantity>1</Quantity>
                        <Amount>800</Amount>
                    </Line>
                    <Line>
                        <ItemDescription>Briefcase, Leather</ItemDescription>
                        <VariantCode></VariantCode>
                        <UOM></UOM>
                        <Quantity>1</Quantity>
                        <Amount>800</Amount>
                    </Line>
                </OrderLinesToCollect>
                <EstimatedTotalAmount>3200</EstimatedTotalAmount>
            </Order>
            <Disclaimer>TEST</Disclaimer>
        </OmniMessage>

             <OmniMessage>
            <OrderStatus>Canceled</OrderStatus>
            <QRC>
                <CustomerOrder>
                    <DocStatus>1</DocStatus>
                    <DocID>CO000008</DocID>
                </CustomerOrder>
            </QRC>
            <Order>
                <DocID>CO000008</DocID>
                <DocDate>2014-11-20T14:29:24.54Z</DocDate>
                <MemberCardNo>10021</MemberCardNo>
                <StoreToCollect>S0001</StoreToCollect>
                <CollectTimeLimit></CollectTimeLimit>
                <OrderLinesOutOfStock>
                    <Line>
                        <ItemDescription>Wireless Mouse</ItemDescription>
                        <VariantCode></VariantCode>
                        <UOM>PCS</UOM>
                        <Quantity>1</Quantity>
                        <Amount>8</Amount>
                    </Line>
                </OrderLinesOutOfStock>
                <EstimatedTotalAmount>0</EstimatedTotalAmount>
            </Order>
            <Disclaimer>TEST</Disclaimer>
        </OmniMessage> 
        */
            #endregion xml

            try
            {
                XDocument doc = XDocument.Parse(message.Details);
                //cleanup data and add to it
                string status = doc.Element("OmniMessage").Element("OrderStatus").Value;
                if (status.ToUpper().StartsWith("REA"))
                    status = "Ready"; //Ready Canceled
                else if (status.ToUpper().StartsWith("CANC"))
                    status = "Canceled";
                doc.Element("OmniMessage").Element("Order").Add(new XElement("Status", status));
                doc.Element("OmniMessage").Element("Order").Add(new XElement("FirstName", contact.FirstName));
                doc.Element("OmniMessage").Element("Order").Add(new XElement("LastName", contact.LastName));

                //
                //string storeNo = doc.Element("OmniMessage").Element("Order").Element("StoreToCollect").Value;
                //StoreBLL storeBLL = new StoreBLL(timeoutInSeconds);
                //Store store = storeBLL.StoreGetById(storeNo);
                //doc.Element("OmniMessage").Element("Order").Add(new XElement("StoreName", store.Description));
                //string addr = store.Address.Address1 + ", " + store.Address.City;
                //doc.Element("OmniMessage").Element("Order").Add(new XElement("StoreAddress", addr));
                string timeLimit = doc.Element("OmniMessage").Element("Order").Element("CollectTimeLimit").Value;

                if (string.IsNullOrWhiteSpace(timeLimit) == false)
                {
                    DateTime collectTime = Convert.ToDateTime(timeLimit);
                    //friendly datetime
                    doc.Element("OmniMessage").Element("Order").Element("CollectTimeLimit").Value = collectTime.ToString("f");
                }

                string xslFile = string.Format(@"{0}\xsl\notification.xsl", AppDomain.CurrentDomain.BaseDirectory);
                string xslInput = string.Empty;
                xslInput = File.ReadAllText(xslFile);

                string xmlInput = doc.Element("OmniMessage").Element("Order").ToString(); //<Order>..
                xslInput = WebUtility.HtmlDecode(xslInput);
                string detailsAsHtml = String.Empty;

                // xslInput is a string that contains xsl
                using (StringReader srt = new StringReader(xslInput))
                // xmlInput is a string that contains xml
                using (StringReader sri = new StringReader(xmlInput))
                {
                    using (XmlReader xrt = XmlReader.Create(srt))
                    using (XmlReader xri = XmlReader.Create(sri))
                    {
                        XslCompiledTransform xslt = new XslCompiledTransform();
                        xslt.Load(xrt);
                        using (StringWriter sw = new StringWriter())
                        using (XmlWriter xwo = XmlWriter.Create(sw, xslt.OutputSettings)) // use OutputSettings of xsl, so it can be output as HTML
                        {
                            xslt.Transform(xri, xwo);
                            detailsAsHtml = sw.ToString();
                            detailsAsHtml = detailsAsHtml.Replace("<html>", "").Replace("</html>", "").Replace("\r", "").Replace("\n", "").Replace("<br />", "\n");
                            //TODO must be an easier way !
                            detailsAsHtml = detailsAsHtml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
                            detailsAsHtml = detailsAsHtml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
                            detailsAsHtml = detailsAsHtml.Replace("<?xml version=\"1.0\"?>", "");
                        }
                    }
                }
                return detailsAsHtml;
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                throw;
            }
        }

        private void CreateNotificationsFromOrderMessage(OrderMessage orderMessage)
        {
            string cardId = "";
            string orderStatus = ""; //Ready Canceled
            try
            {
                //validation
                if (orderMessage == null || string.IsNullOrWhiteSpace(orderMessage.Id))
                {
                    string msg = "orderMessage is null or guid is null";
                    throw new LSOmniServiceException(StatusCode.Error, msg);
                }

                ContactBLL contactBLL = new ContactBLL(config, timeoutInSeconds);
                XDocument doc = XDocument.Parse(orderMessage.Details);
                XElement elCardNo = doc.Element("OmniMessage").Element("Order").Element("MemberCardNo");
                if (elCardNo == null)
                    throw new XmlException("MemberCardNo. node not found in message.Details xml");

                cardId = elCardNo.Value;
                if (string.IsNullOrEmpty(cardId))
                    return;

                orderStatus = doc.Element("OmniMessage").Element("OrderStatus").Value;//Ready Canceled
                orderStatus = orderStatus.ToUpper();

                if (orderMessage != null && !string.IsNullOrWhiteSpace(orderMessage.Details))
                {
                    MemberContact contact = contactBLL.ContactGetByCardId(cardId, false);
                    if (contact == null)
                    {
                        logger.Error(config.LSKey.Key, "Failed to find contact for cardId: {0}, OrderMessage.Id: {1}", cardId, orderMessage.Id);
                        return;
                    }

                    //parse xml from nav for the Notifications
                    string detailsAsHtml = NotificationHtmlFromOrderMessageXml(orderMessage, contact);
                    string qrText = "";
                    //if status is ready 
                    if (orderStatus.StartsWith("REA"))
                    {
                        qrText = doc.Element("OmniMessage").Element("QRC").Element("CustomerOrder").ToString();
                        qrText = qrText.Replace("\r", "").Replace("\n", "");
                    }

                    string notificationId = GuidHelper.NewGuidString();
                    iNotificationRepository.OrderMessageNotificationSave(notificationId, orderMessage.OrderId, contact.Id, orderMessage.Description, detailsAsHtml, qrText);
                    iPushRepository.SavePushNotification(contact.Id, notificationId);

                    //if status is ready 
                    if (orderStatus.StartsWith("REA"))
                    {
                        //create qr image 
                        ImageView iv = new ImageView(notificationId);
                        iv.DisplayOrder = 0;
                        iv.LocationType = LocationType.Image;
                        iv.Location = "";
                        iv.ImgBytes = GenerateQRCode(qrText);

                        IImageRepository imgRepository = base.GetDbRepository<IImageRepository>(config);
                        imgRepository.SaveImageLink(iv, "Member Notification", "Member Notification: " + notificationId, notificationId, iv.Id, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    logger.Error(config.LSKey.Key, ex, "OrderMessageSend failed for guid " + orderMessage.Id + "  CardId: " + cardId);
                }
                catch
                {
                }
                throw;
            }
        }

        #endregion private

        #region qrCode generation

        private byte[] GenerateQRCode(string qrCode)
        {
            int height = 500; //500 was 58 KB
            int width = 500;
            ZXing.BarcodeWriter writer = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.Q,
                    Height = height,
                    Width = width,
                    Margin = 0,
                }
            };

            Bitmap bitmap = writer.Write(qrCode);
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] imageBytes = ms.ToArray();
                return imageBytes;
            }
        }

        private string GenerateQRCodeFile(string qrCode, string id, string qrImageFolderName)
        {
            int height = 300; //500 was 58 KB
            int width = 300;
            ZXing.BarcodeWriter writer = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.Q,
                    Height = height,
                    Width = width,
                    Margin = 0,
                }
            };

            Bitmap bitmap = writer.Write(qrCode);
            if (Common.Util.ImageConverter.DirectoryExists(qrImageFolderName) == false)
                Common.Util.ImageConverter.DirectoryCreate(qrImageFolderName);

            string fileName = string.Format(@"{0}\{1}.jpg", qrImageFolderName, id);//save as id.jpg 
            bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            //bitmap.Save(@"c:\temp\test2.Jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
            return fileName;
        }

        #endregion qrcode
    }
}
