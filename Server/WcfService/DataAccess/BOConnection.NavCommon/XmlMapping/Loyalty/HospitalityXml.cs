using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Loyalty
{
    public class HospitalityXml : BaseXml
    {
        //WEB_POS commands 'CALCULATE','POST','SUSPEND','RETSUSPENDED','PRICECHECK','SAVE','RETSAVE'
        private const string WebPosRequestId = "WEB_POS";

        public HospitalityXml()
        {
        }

        public string OrderToPOSRequestXML(OneList Rq, string command, string terminal, string staff, string salesType)
        {
            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", WebPosRequestId),
                    new XElement("Request_Body",
                        new XElement("Command", command), //"POST" "CALCULATE" "REFUND"
                        new XElement("Search_Key", string.Empty),
                        new XElement("Staff_Id", string.Empty),
                        new XElement("Unlock", string.Empty),
                        new XElement("MenuType", string.Empty)
                    )
                );
            ;

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            XElement body = doc.Element("Request").Element("Request_Body");

            XElement mtransRoot = MobileTrans(Rq, terminal, staff, salesType);
            body.Add(mtransRoot);

            // find highest lineNo count in sub line, if user has set some
            int LineCounter = 1;
            foreach (OneListItem l in Rq.Items)
            {
                if (LineCounter < l.LineNumber)
                    LineCounter = l.LineNumber;

                if (l.OnelistSubLines != null)
                {
                    foreach (OneListItemSubLine s in l.OnelistSubLines)
                    {
                        if (LineCounter < s.LineNumber)
                            LineCounter = s.LineNumber;
                    }
                }
            }

            foreach (OneListItem posLine in Rq.Items)
            {
                if (posLine.LineNumber == 0)
                    posLine.LineNumber = ++LineCounter;

                body.Add(MobileTransLine(Rq.Id, posLine, staff));
                MobileTransSubLine(body, Rq.Id, posLine, ref LineCounter, staff);
            }

            if (Rq.PublishedOffers != null)
            {
                foreach (OneListPublishedOffer offer in Rq.PublishedOffers)
                {
                    if (offer.LineNumber == 0)
                        offer.LineNumber = ++LineCounter;

                    body.Add(MobileTransLine(Rq.Id, offer, staff));
                }
            }
            return doc.ToString();
        }

        public string OrderToPOSRequestXML(OrderHosp Rq, string command, string terminal, string staff, string salesType)
        {
            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", WebPosRequestId),
                    new XElement("Request_Body",
                        new XElement("Command", command), //"POST" "CALCULATE" "REFUND"
                        new XElement("Search_Key", string.Empty),
                        new XElement("Staff_Id", string.Empty),
                        new XElement("Unlock", string.Empty),
                        new XElement("MenuType", string.Empty)
                    )
                );
            ;

            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            XElement body = doc.Element("Request").Element("Request_Body");

            XElement mtransRoot = MobileTrans(Rq, terminal, staff, salesType);
            body.Add(mtransRoot);

            // find highest lineNo count in sub line, if user has set some
            int LineCounter = 1;
            foreach (OrderHospLine l in Rq.OrderLines)
            {
                if (LineCounter < l.LineNumber)
                    LineCounter = l.LineNumber;

                if (l.SubLines != null)
                {
                    foreach (OrderHospSubLine s in l.SubLines)
                    {
                        if (LineCounter < s.LineNumber)
                            LineCounter = s.LineNumber;
                    }
                }
            }

            foreach (OrderHospLine posLine in Rq.OrderLines)
            {
                if (posLine.LineNumber == 0)
                    posLine.LineNumber = ++LineCounter;

                body.Add(MobileTransLine(Rq.Id, posLine, staff));
                MobileTransSubLine(body, Rq.Id, posLine, staff, ref LineCounter);
            }

            if (Rq.OrderDiscountLines != null)
            {
                foreach (OrderDiscountLine dline in Rq.OrderDiscountLines)
                {
                    body.Add(MobileTransDiscoutLine(Rq.Id, dline));
                }
            }

            if (Rq.OrderPayments != null)
            {
                foreach (OrderPayment tenderLine in Rq.OrderPayments)
                {
                    body.Add(TransPaymentLine(Rq.Id, Rq.StoreId, tenderLine, ++LineCounter));
                }
            }
            return doc.ToString();
        }

        public string TransactionReceiptNoResponseXML(string responseXml)
        {
            XDocument doc = XDocument.Parse(responseXml);

            XElement mobileTrans = doc.Element("Response").Element("Response_Body");
            if (mobileTrans != null)
                mobileTrans = doc.Element("Response").Element("Response_Body").Element("MobileTransaction");
            else
                mobileTrans = doc.Element("Response").Element("Request_Body").Element("MobileTransaction"); //Request_Body

            if (mobileTrans.Element("ReceiptNo") == null)
                throw new XmlException("ReceiptNo node not found in response xml");
            return mobileTrans.Element("ReceiptNo").Value;
        }

        public OrderHosp TransactionResponseXML(string responseXml)
        {
            OrderHosp rs = new OrderHosp();
            XDocument doc = XDocument.Parse(responseXml);

            XElement mobileTrans = doc.Element("Response").Element("Response_Body");
            if (mobileTrans != null)
                mobileTrans = doc.Element("Response").Element("Response_Body").Element("MobileTransaction");
            else
                mobileTrans = doc.Element("Response").Element("Request_Body").Element("MobileTransaction"); //Request_Body

            if (mobileTrans.Element("Id") == null)
                throw new XmlException("Id node not found in response xml");
            rs.Id = mobileTrans.Element("Id").Value;
            rs.Id = rs.Id.Replace("{", string.Empty).Replace("}", string.Empty); //strip out the curly brackets

            if (mobileTrans.Element("StoreId") == null)
                throw new XmlException("StoreId node not found in response xml");
            rs.StoreId = mobileTrans.Element("StoreId").Value;

            if (mobileTrans.Element("ReceiptNo") == null)
                throw new XmlException("ReceiptNo node not found in response xml");
            rs.ReceiptNo = mobileTrans.Element("ReceiptNo").Value;

            if (mobileTrans.Element("TransDate") == null)
                throw new XmlException("TransDate node not found in response xml");
            rs.DocumentRegTime = ConvertTo.SafeDateTime(mobileTrans.Element("TransDate").Value);

            if (mobileTrans.Element("MemberCardNo") == null)
                throw new XmlException("MemberCardNo node not found in response xml");
            rs.CardId = mobileTrans.Element("MemberCardNo").Value;

            if (mobileTrans.Element("NetAmount") != null)
            {
                rs.TotalNetAmount = ConvertTo.SafeDecimal(mobileTrans.Element("NetAmount").Value);
            }

            if (mobileTrans.Element("GrossAmount") != null)
            {
                rs.TotalAmount = ConvertTo.SafeDecimal(mobileTrans.Element("GrossAmount").Value);
            }

            if (mobileTrans.Element("TotalDiscount") != null)
            {
                rs.TotalDiscount = ConvertTo.SafeDecimal(mobileTrans.Element("TotalDiscount").Value);
            }
            if (rs.TotalDiscount == 0 && mobileTrans.Element("LineDiscount") != null)
            {
                rs.TotalDiscount += ConvertTo.SafeDecimal(mobileTrans.Element("LineDiscount").Value);
            }

            var mobileTransLines = doc.Element("Response").Element("Response_Body").Descendants("MobileTransactionLine");
            foreach (XElement mobileTransLine in mobileTransLines)
            {
                bool isDeal = false;
                if (mobileTransLine.Element("DealItem") != null)
                    isDeal = ConvertTo.SafeBoolean(mobileTransLine.Element("DealItem").Value);

                OrderHospLine rsl = new OrderHospLine();

                //Transaction Id
                if (mobileTransLine.Element("Id") == null)
                    throw new XmlException("Id node not found in response xml");
                string id = mobileTransLine.Element("Id").Value;
                rsl.Id = id.Replace("{", string.Empty).Replace("}", string.Empty); //strip out the curly brackets

                if (mobileTransLine.Element("LineType") == null)
                    throw new XmlException("LineType node not found in response xml");
                rsl.LineType = (LineType)Convert.ToInt32(mobileTransLine.Element("LineType").Value);

                if (mobileTransLine.Element("LineNo") == null)
                    throw new XmlException("LineNo node not found in response xml");
                rsl.LineNumber = Convert.ToInt32(mobileTransLine.Element("LineNo").Value);

                if (rsl.LineType == LineType.Coupon)
                {
                    if (mobileTransLine.Element("CouponCode") == null)
                        throw new XmlException("CouponCode node not found in response xml");
                    rsl.ItemId = mobileTransLine.Element("CouponCode").Value;
                }
                else
                {
                    if (mobileTransLine.Element("Number") == null)
                        throw new XmlException("Number node not found in response xml");
                    rsl.ItemId = mobileTransLine.Element("Number").Value;
                }

                if (mobileTransLine.Element("ItemDescription") == null)
                    throw new XmlException("ItemDescription node not found in response xml");
                rsl.ItemDescription = mobileTransLine.Element("ItemDescription").Value;

                if (mobileTransLine.Element("VariantCode") == null)
                    throw new XmlException("VariantCode node not found in response xml");
                rsl.VariantId = mobileTransLine.Element("VariantCode").Value;

                if (mobileTransLine.Element("VariantDescription") == null)
                    throw new XmlException("VariantDescription node not found in response xml");
                rsl.VariantDescription = mobileTransLine.Element("VariantDescription").Value;

                if (mobileTransLine.Element("UomId") == null)
                    throw new XmlException("UomId node not found in response xml");
                rsl.UomId = mobileTransLine.Element("UomId").Value;

                if (mobileTransLine.Element("NetPrice") == null)
                    throw new XmlException("NetPrice node not found in response xml");
                rsl.NetPrice = ConvertTo.SafeDecimal(mobileTransLine.Element("NetPrice").Value, true);

                if (mobileTransLine.Element("ManualPrice") != null)
                {
                    decimal manPrice = ConvertTo.SafeDecimal(mobileTransLine.Element("ManualPrice").Value);
                    if (manPrice > 0)
                    {
                        rsl.Price = manPrice;
                        rsl.PriceModified = true;
                    }
                    else
                    {
                        if (mobileTransLine.Element("Price") == null)
                            throw new XmlException("Price node not found in response xml");
                        rsl.Price = ConvertTo.SafeDecimal(mobileTransLine.Element("Price").Value);
                        rsl.PriceModified = false;
                    }
                }
                else
                {
                    if (mobileTransLine.Element("Price") == null)
                        throw new XmlException("Price node not found in response xml");
                    rsl.Price = ConvertTo.SafeDecimal(mobileTransLine.Element("Price").Value);
                }

                if (mobileTransLine.Element("Quantity") == null)
                    throw new XmlException("Quantity node not found in response xml");
                rsl.Quantity = ConvertTo.SafeDecimal(mobileTransLine.Element("Quantity").Value);
                rsl.Quantity = Math.Abs(rsl.Quantity);

                if (mobileTransLine.Element("DiscountAmount") == null)
                    throw new XmlException("DiscountAmount node not found in response xml");
                rsl.DiscountAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("DiscountAmount").Value, true);

                if (mobileTransLine.Element("DiscountPercent") == null)
                    throw new XmlException("DiscountPercent node not found in response xml");
                rsl.DiscountPercent = ConvertTo.SafeDecimal(mobileTransLine.Element("DiscountPercent").Value, true);

                if (mobileTransLine.Element("NetAmount") == null)
                    throw new XmlException("NetAmount node not found in response xml");
                rsl.NetAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("NetAmount").Value, (rsl.LineType == LineType.Payment) ? false : true);

                if (mobileTransLine.Element("TAXAmount") == null)
                    throw new XmlException("TAXAmount node not found in response xml");
                rsl.TaxAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("TAXAmount").Value, true);

                rsl.Amount = rsl.NetAmount + rsl.TaxAmount;

                if (mobileTransLine.Element("DiscInfoLine") == null)
                    throw new XmlException("DiscInfoLine node not found in response xml");
                int discInfoLine = 0;
                if (!string.IsNullOrWhiteSpace(mobileTransLine.Element("DiscInfoLine").Value))
                    discInfoLine = Convert.ToInt32(mobileTransLine.Element("DiscInfoLine").Value);

                if (mobileTransLine.Element("TotalDiscInfoLine") == null)
                    throw new XmlException("TotalDiscInfoLine node not found in response xml");

                // getting per discount, total discount - ignore them
                if (rsl.LineType == LineType.Item || rsl.LineType == LineType.Coupon || rsl.LineType == LineType.IncomeExpense)
                {
                    //HMPOS
                    XElement body = doc.Element("Response").Element("Response_Body");
                    if (isDeal)
                    {
                        rsl.IsADeal = true;

                        //its a deal, get the deal lines from the mobileTransLine 
                        rsl.SubLines.AddRange(DealSubLineXML(body, rsl.LineNumber));
                    }

                    //get the text modifiers from the mobileTransLine 
                    List<OrderHospSubLine> tmlist = TextModifierSubLineXML(body, rsl.LineNumber);
                    if (tmlist != null && tmlist.Count > 0)
                    {
                        rsl.SubLines.AddRange(tmlist);
                    }

                    //get the item modifiers from the mobileTransLine 
                    List<OrderHospSubLine> modiferLineList = ModifierSubLineXML(body, rsl.LineNumber);
                    if (modiferLineList != null && modiferLineList.Count > 0)
                    {
                        rsl.SubLines.AddRange(modiferLineList);
                    }

                    rsl.LineNumber = LineNumberFromNav(rsl.LineNumber); // MPOS expects to get 1 not 10000
                    rs.OrderLines.Add(rsl);
                }

                if (rsl.LineType == LineType.Payment)
                {
                    OrderPayment tline = new OrderPayment();

                    tline.LineNumber = rsl.LineNumber;
                    tline.Amount = rsl.NetAmount; //request sends in net amount

                    if (mobileTransLine.Element("Number") != null)
                        tline.TenderType = mobileTransLine.Element("Number").Value;
                    if (mobileTransLine.Element("EFTCardNumber") != null)
                        tline.CardNumber = mobileTransLine.Element("EFTCardNumber").Value;
                    if (mobileTransLine.Element("EFTAuthCode") != null)
                        tline.AuthorizationCode = mobileTransLine.Element("EFTAuthCode").Value;
                    if (mobileTransLine.Element("EFTTransactionNo") != null)
                        tline.TokenNumber = mobileTransLine.Element("EFTTransactionNo").Value;

                    if (mobileTransLine.Element("EFTDateTime") != null && !string.IsNullOrEmpty(mobileTransLine.Element("EFTDateTime").Value))
                    {
                        string dt = mobileTransLine.Element("EFTDateTime").Value;
                        tline.PreApprovedValidDate = ConvertTo.SafeDateTime(dt);
                    }
                    else
                        tline.PreApprovedValidDate = new DateTime(1970, 1, 1, 1, 0, 0);

                    rs.OrderPayments.Add(tline);
                }
            }

            //now loop thou the discount lines
            var mobileTransDiscLines = doc.Element("Response").Element("Response_Body").Descendants("MobileTransDiscountLine");
            foreach (XElement mobileTransDisc in mobileTransDiscLines)
            {
                OrderDiscountLine rsld = new OrderDiscountLine();
                //The ID for the Transaction
                if (mobileTransDisc.Element("Id") == null)
                    throw new XmlException("Id node not found in response xml");
                string id = mobileTransDisc.Element("Id").Value;
                rsld.Id = id.Replace("{", string.Empty).Replace("}", string.Empty); //strip out the curly brackets

                //transaction line number
                if (mobileTransDisc.Element("LineNo") == null)
                    throw new XmlException("LineNo node not found in response xml");
                rsld.LineNumber = LineNumberFromNav(Convert.ToInt32(mobileTransDisc.Element("LineNo").Value));

                //discount line number
                if (mobileTransDisc.Element("No") == null)
                    throw new XmlException("No node not found in response xml");
                rsld.No = mobileTransDisc.Element("No").Value;

                //DiscountType: Periodic Disc.,Customer,InfoCode,Total,Line,Promotion,Deal,Total Discount,Tender Type,Item Point,Line Discount,Member Point,Coupon
                if (mobileTransDisc.Element("DiscountType") == null)
                    throw new XmlException("DiscountType node not found in response xml");
                rsld.DiscountType = (DiscountType)Convert.ToInt32(mobileTransDisc.Element("DiscountType").Value);

                //PeriodicDiscType: Multi buy,Mix&Match,Disc. Offer,Total Discount,Tender Type,Item Point,Line Discount
                if (mobileTransDisc.Element("PeriodicDiscType") == null)
                    throw new XmlException("PeriodicDiscType node not found in response xml");
                rsld.PeriodicDiscType = (PeriodicDiscType)Convert.ToInt32(mobileTransDisc.Element("PeriodicDiscType").Value);

                if (mobileTransDisc.Element("PeriodicDiscGroup") == null)
                    throw new XmlException("PeriodicDiscGroup node not found in response xml");
                rsld.PeriodicDiscGroup = mobileTransDisc.Element("PeriodicDiscGroup").Value;

                if (mobileTransDisc.Element("Description") == null)
                    throw new XmlException("Description node not found in response xml");
                rsld.Description = mobileTransDisc.Element("Description").Value;

                if (mobileTransDisc.Element("DiscountPercent") == null)
                    throw new XmlException("DiscountPercent node not found in response xml");
                rsld.DiscountPercent = ConvertTo.SafeDecimal(mobileTransDisc.Element("DiscountPercent").Value, true);

                if (mobileTransDisc.Element("DiscountAmount") == null)
                    throw new XmlException("DiscountAmount node not found in response xml");
                rsld.DiscountAmount = ConvertTo.SafeDecimal(mobileTransDisc.Element("DiscountAmount").Value, true);

                //only add this discount line when it tied to an Item MobileTransactionLine 
                LineType lineType = FindLineType(rs.OrderLines, rsld.LineNumber);
                if (LineType.Item == lineType || LineType.Coupon == lineType)
                {
                    int idx = FindLineIndex(rs.OrderLines, rsld.LineNumber);
                    rs.OrderDiscountLines.Add(rsld);
                }
            }
            return rs;
        }

        private List<OrderHospSubLine> ModifierSubLineXML(XElement transLine, int parentLine, int lineNumber = 0)
        {
            //get MobileTransactionSubLine for this parent
            List<OrderHospSubLine> modLineList = new List<OrderHospSubLine>();

            IEnumerable<XElement> mobileTransSubLines = null;

            //now loop thou the modifierSublines
            if (lineNumber == 0)
            {
                mobileTransSubLines =
                    from el in transLine.Elements("MobileTransactionSubLine")
                    where (string)el.Element("ParentLineNo") == parentLine.ToString() &&
                        (string)el.Element("LineType") == "0"   //0 = item, 1 = Deal Item, 2 = Text
                    select el;
            }
            else
            {
                //sometimes we are getting the sub line for the modifiers under the dealLine
                mobileTransSubLines =
                    from el in transLine.Elements("MobileTransactionSubLine")
                    where (string)el.Element("ParentLineNo") == parentLine.ToString()
                        && (string)el.Element("LineNo") == lineNumber.ToString()
                        && (string)el.Element("LineType") == "1" //0 = item, 1 = Deal Item, 2 = Text
                        && (string)el.Element("DealModLine") != "0"
                    select el;
            }

            //var mobileTransSubLines = transLine.Descendants("MobileTransactionSubLine");
            foreach (XElement mobileTransLine in mobileTransSubLines)
            {
                OrderHospSubLine rsl = new OrderHospSubLine();

                //Transaction Id
                if (mobileTransLine.Element("Id") == null)
                    throw new XmlException("Id node not found in response xml");
                string id = mobileTransLine.Element("Id").Value;
                id = id.Replace("{", string.Empty).Replace("}", string.Empty); //strip out the curly brackets

                if (mobileTransLine.Element("LineType") == null)
                    throw new XmlException("LineType node not found in response xml");
                rsl.Type = (SubLineType)Convert.ToInt32(mobileTransLine.Element("LineType").Value);

                if (mobileTransLine.Element("LineNo") == null)
                    throw new XmlException("LineNo node not found in response xml");
                rsl.LineNumber = Convert.ToInt32(mobileTransLine.Element("LineNo").Value);

                //Item No. or Tender Type
                if (mobileTransLine.Element("Number") == null)
                    throw new XmlException("Number node not found in response xml");
                rsl.ItemId = mobileTransLine.Element("Number").Value;

                if (mobileTransLine.Element("VariantCode") == null)
                    throw new XmlException("VariantCode node not found in response xml");
                rsl.VariantId = mobileTransLine.Element("VariantCode").Value;

                if (mobileTransLine.Element("UomId") == null)
                    throw new XmlException("UomId node not found in response xml");
                rsl.Uom = mobileTransLine.Element("UomId").Value;

                if (mobileTransLine.Element("NetPrice") == null)
                    throw new XmlException("NetPrice node not found in response xml");
                rsl.NetPrice = ConvertTo.SafeDecimal(mobileTransLine.Element("NetPrice").Value);

                if (mobileTransLine.Element("Price") == null)
                    throw new XmlException("Price node not found in response xml");
                rsl.Price = ConvertTo.SafeDecimal(mobileTransLine.Element("Price").Value);

                if (mobileTransLine.Element("Quantity") == null)
                    throw new XmlException("Quantity node not found in response xml");
                rsl.Quantity = ConvertTo.SafeDecimal(mobileTransLine.Element("Quantity").Value);

                if (mobileTransLine.Element("DiscountAmount") == null)
                    throw new XmlException("DiscountAmount node not found in response xml");
                rsl.DiscountAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("DiscountAmount").Value);

                if (mobileTransLine.Element("DiscountPercent") == null)
                    throw new XmlException("DiscountPercent node not found in response xml");
                rsl.DiscountPercent = ConvertTo.SafeDecimal(mobileTransLine.Element("DiscountPercent").Value);

                if (mobileTransLine.Element("NetAmount") == null)
                    throw new XmlException("NetAmount node not found in response xml");
                rsl.NetAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("NetAmount").Value);

                if (mobileTransLine.Element("TAXAmount") == null)
                    throw new XmlException("TAXAmount node not found in response xml");
                rsl.TAXAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("TAXAmount").Value);

                rsl.Amount = rsl.NetAmount + rsl.TAXAmount;

                if (mobileTransLine.Element("ManualDiscountPercent") == null)
                    throw new XmlException("ManualDiscountPercent node not found in response xml");
                rsl.ManualDiscountPercent = ConvertTo.SafeDecimal(mobileTransLine.Element("ManualDiscountPercent").Value);

                if (mobileTransLine.Element("ManualDiscountAmount") == null)
                    throw new XmlException("ManualDiscountAmount node not found in response xml");
                rsl.ManualDiscountAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("ManualDiscountAmount").Value);

                rsl.LineNumber = LineNumberFromNav(rsl.LineNumber); // MPOS expects to get 1 not 10000

                if (mobileTransLine.Element("Description") != null)
                    rsl.Description = mobileTransLine.Element("Description").Value;

                if (mobileTransLine.Element("ModifierGroupCode") != null)
                    rsl.ModifierGroupCode = mobileTransLine.Element("ModifierGroupCode").Value;
                if (mobileTransLine.Element("ModifierSubCode") != null)
                    rsl.ModifierSubCode = mobileTransLine.Element("ModifierSubCode").Value;
                if (mobileTransLine.Element("DealLine") != null) //DealLineCode 
                    rsl.DealLineId = Convert.ToInt32(mobileTransLine.Element("DealLine").Value);
                if (mobileTransLine.Element("DealModLine") != null) //DealModifierLineCode
                    rsl.DealModifierLineId = Convert.ToInt32(mobileTransLine.Element("DealModLine").Value);
                if (mobileTransLine.Element("DealId") != null) // 
                    rsl.DealCode = mobileTransLine.Element("DealId").Value;
                if (mobileTransLine.Element("PriceReductionOnExclusion") != null) // 
                    rsl.PriceReductionOnExclusion = ConvertTo.SafeBoolean(mobileTransLine.Element("PriceReductionOnExclusion").Value);
                modLineList.Add(rsl);

            }
            return modLineList;
        }

        private List<OrderHospSubLine> DealSubLineXML(XElement body, int parentLine)
        {
            //get MobileTransactionSubLine for this parent
            List<OrderHospSubLine> modLineList = new List<OrderHospSubLine>();

            //now loop thou the DealSublines
            IEnumerable<XElement> mobileTransSubLines =
                from el in body.Elements("MobileTransactionSubLine")
                where (string)el.Element("LineType") == "1" //0 = item, 1 = Deal Item, 2 = Text
                select el;

            //var mobileTransSubLines = body.Descendants("MobileTransactionSubLine");
            foreach (XElement mobileTransLine in mobileTransSubLines)
            {
                OrderHospSubLine rsl = new OrderHospSubLine();
                rsl.Type = SubLineType.Deal;

                //Transaction Id
                if (mobileTransLine.Element("Id") == null)
                    throw new XmlException("Id node not found in response xml");
                string id = mobileTransLine.Element("Id").Value;
                id = id.Replace("{", string.Empty).Replace("}", string.Empty); //strip out the curly brackets

                if (mobileTransLine.Element("LineNo") == null)
                    throw new XmlException("LineNo node not found in response xml");
                rsl.LineNumber = Convert.ToInt32(mobileTransLine.Element("LineNo").Value);

                if (mobileTransLine.Element("DealLine") != null) //DealLineCode 
                    rsl.DealLineId = Convert.ToInt32(mobileTransLine.Element("DealLine").Value);

                int parline = 0;
                if (mobileTransLine.Element("ParentLineNo") != null)
                    parline = Convert.ToInt32(mobileTransLine.Element("ParentLineNo").Value);

                if (rsl.DealLineId != 0 && parline != parentLine)
                {
                    // recipe modifier
                    rsl.Type = SubLineType.Modifier;
                }

                if (mobileTransLine.Element("DealId") != null) // 
                    rsl.DealCode = mobileTransLine.Element("DealId").Value;
                if (mobileTransLine.Element("DealModLine") != null) //DealModifierLineCode
                    rsl.DealModifierLineId = Convert.ToInt32(mobileTransLine.Element("DealModLine").Value);

                if (mobileTransLine.Element("EntryStatus") == null)
                    throw new XmlException("EntryStatus node not found in response xml");

                if (mobileTransLine.Element("Number") == null)
                    throw new XmlException("Number node not found in response xml");
                rsl.ItemId = mobileTransLine.Element("Number").Value;

                if (mobileTransLine.Element("Description") != null)
                    rsl.Description = mobileTransLine.Element("Description").Value;

                if (mobileTransLine.Element("NetPrice") == null)
                    throw new XmlException("NetPrice node not found in response xml");
                rsl.NetPrice = ConvertTo.SafeDecimal(mobileTransLine.Element("NetPrice").Value);

                if (mobileTransLine.Element("Price") == null)
                    throw new XmlException("Price node not found in response xml");
                rsl.Price = ConvertTo.SafeDecimal(mobileTransLine.Element("Price").Value);

                if (mobileTransLine.Element("Quantity") == null)
                    throw new XmlException("Quantity node not found in response xml");
                rsl.Quantity = ConvertTo.SafeDecimal(mobileTransLine.Element("Quantity").Value);

                if (mobileTransLine.Element("DiscountAmount") == null)
                    throw new XmlException("DiscountAmount node not found in response xml");
                rsl.DiscountAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("DiscountAmount").Value);

                if (mobileTransLine.Element("DiscountPercent") == null)
                    throw new XmlException("DiscountPercent node not found in response xml");
                rsl.DiscountPercent = ConvertTo.SafeDecimal(mobileTransLine.Element("DiscountPercent").Value);

                if (mobileTransLine.Element("NetAmount") == null)
                    throw new XmlException("NetAmount node not found in response xml");
                rsl.NetAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("NetAmount").Value);

                if (mobileTransLine.Element("TAXAmount") == null)
                    throw new XmlException("TAXAmount node not found in response xml");
                rsl.TAXAmount = ConvertTo.SafeDecimal(mobileTransLine.Element("TAXAmount").Value);

                rsl.Amount = rsl.NetAmount + rsl.TAXAmount;

                modLineList.Add(rsl);
            }
            return modLineList;
        }

        private List<OrderHospSubLine> TextModifierSubLineXML(XElement body, int parentLine)
        {
            //get MobileTransactionSubLine for this parent
            List<OrderHospSubLine> modLineList = new List<OrderHospSubLine>();

            //now loop thou the text modifier lines
            IEnumerable<XElement> mobileTransSubLines =
                from el in body.Elements("MobileTransactionSubLine")
                where (string)el.Element("ParentLineNo") == parentLine.ToString() &&
                    (string)el.Element("LineType") == "2" //0 = item, 1 = Deal Item, 2 = Text
                select el;

            //var mobileTransSubLines = body.Descendants("MobileTransactionSubLine");
            foreach (XElement mobileTransLine in mobileTransSubLines)
            {
                OrderHospSubLine rsl = new OrderHospSubLine();
                rsl.Type = SubLineType.Text;

                //Transaction Id
                if (mobileTransLine.Element("Id") == null)
                    throw new XmlException("Id node not found in response xml");
                string id = mobileTransLine.Element("Id").Value;
                id = id.Replace("{", string.Empty).Replace("}", string.Empty); //strip out the curly brackets

                if (mobileTransLine.Element("LineNo") == null)
                    throw new XmlException("LineNo node not found in response xml");
                rsl.LineNumber = Convert.ToInt32(mobileTransLine.Element("LineNo").Value);

                if (mobileTransLine.Element("Description") != null)
                    rsl.Description = mobileTransLine.Element("Description").Value;

                if (mobileTransLine.Element("ModifierGroupCode") != null)
                    rsl.ModifierGroupCode = mobileTransLine.Element("ModifierGroupCode").Value;
                if (mobileTransLine.Element("ModifierSubCode") != null)
                    rsl.ModifierSubCode = mobileTransLine.Element("ModifierSubCode").Value;

                if (mobileTransLine.Element("Quantity") != null)
                {
                    rsl.Quantity = ConvertTo.SafeDecimal(mobileTransLine.Element("Quantity").Value);
                }

                rsl.LineNumber = LineNumberFromNav(rsl.LineNumber); //mpos expects to get 1 not 10000
                modLineList.Add(rsl);
            }
            return modLineList;
        }

        private XElement MobileTrans(OneList rq, string terminal, string staff, string salesType)
        {
            return new XElement("MobileTransaction",
                        new XElement("Id", rq.Id),
                        new XElement("StoreId", rq.StoreId),
                        new XElement("TerminalId", terminal),
                        new XElement("StaffId", staff),
                        new XElement("TransactionType", 2),
                        new XElement("SalesType", salesType),
                        new XElement("EntryStatus", (int)EntryStatus.Normal),
                        new XElement("ReceiptNo", string.Empty),
                        new XElement("RefundedReceiptNo", string.Empty),
                        new XElement("RefundedFromStoreNo", string.Empty),
                        new XElement("RefundedFromPOSTermNo", string.Empty),
                        new XElement("RefundedFromTransNo", 0),
                        new XElement("SaleIsReturnSale", false.ToString()),
                        new XElement("TransactionNo", 0),
                        new XElement("TransDate", ConvertTo.SafeStringDateTime(DateTime.Now)),
                        new XElement("CurrencyCode", string.Empty),
                        new XElement("CurrencyFactor", 1),
                        new XElement("BusinessTAXCode", string.Empty),
                        new XElement("PriceGroupCode", string.Empty),
                        new XElement("CustomerId", string.Empty),
                        new XElement("CustDiscGroup", string.Empty),
                        new XElement("MemberCardNo", rq.CardId),
                        new XElement("MemberPriceGroupCode", string.Empty),
                        new XElement("ManualTotalDiscPercent", 0),
                        new XElement("ManualTotalDiscAmount", 0)
                    );
        }

        private XElement MobileTrans(OrderHosp rq, string terminal, string staff, string salesType)
        {
            return new XElement("MobileTransaction",
                        new XElement("Id", rq.Id),
                        new XElement("StoreId", rq.StoreId),
                        new XElement("TerminalId", terminal),
                        new XElement("StaffId", staff),
                        new XElement("TransactionType", 2),
                        new XElement("SalesType", salesType),
                        new XElement("EntryStatus", (int)EntryStatus.Normal),
                        new XElement("ReceiptNo", string.Empty),
                        new XElement("RefundedReceiptNo", string.Empty),
                        new XElement("RefundedFromStoreNo", string.Empty),
                        new XElement("RefundedFromPOSTermNo", string.Empty),
                        new XElement("RefundedFromTransNo", 0),
                        new XElement("SaleIsReturnSale", false.ToString()),
                        new XElement("TransactionNo", 0),
                        new XElement("TransDate", ConvertTo.SafeStringDateTime(DateTime.Now)),
                        new XElement("CurrencyCode", string.Empty),
                        new XElement("CurrencyFactor", 1),
                        new XElement("BusinessTAXCode", string.Empty),
                        new XElement("PriceGroupCode", string.Empty),
                        new XElement("CustomerId", string.Empty),
                        new XElement("CustDiscGroup", string.Empty),
                        new XElement("MemberCardNo", rq.CardId),
                        new XElement("MemberPriceGroupCode", string.Empty),
                        new XElement("ManualTotalDiscPercent", 0),
                        new XElement("ManualTotalDiscAmount", 0)
                    );
        }

        private XElement MobileTransLine(string id, OrderHospLine line, string staffId)
        {
            return new XElement("MobileTransactionLine",
                    new XElement("Id", id),
                    new XElement("LineNo", LineNumberToNav(line.LineNumber)),
                    new XElement("OrigTransLineNo", 0),
                    new XElement("OrigTransPos", string.Empty),
                    new XElement("OrigTransNo", 0),
                    new XElement("OrigTransStore", string.Empty),
                    new XElement("EntryStatus", (int)EntryStatus.Normal),
                    new XElement("LineType", (int)line.LineType),       // TYPE = 6 = COUPON
                    new XElement("Number", (line.LineType == LineType.Item ? line.ItemId : string.Empty)),
                    new XElement("Barcode", (line.LineType == LineType.Coupon ? line.ItemId : string.Empty)),
                    new XElement("CurrencyCode", string.Empty),
                    new XElement("CurrencyFactor", 1),
                    new XElement("VariantCode", line.VariantId),
                    new XElement("UomId", line.UomId),
                    new XElement("NetPrice", line.NetPrice),
                    new XElement("Price", (line.PriceModified) ? 0 : line.Price),
                    new XElement("Quantity", ConvertTo.SafeStringDecimal(line.Quantity)),
                    new XElement("DiscountAmount", ConvertTo.SafeStringDecimal(line.DiscountAmount)),
                    new XElement("DiscountPercent", ConvertTo.SafeStringDecimal(line.DiscountPercent)),
                    new XElement("NetAmount", line.NetAmount),
                    new XElement("TAXAmount", line.TaxAmount),
                    new XElement("TAXProductCode", string.Empty),
                    new XElement("TAXBusinessCode", string.Empty),
                    new XElement("ManualPrice", (line.PriceModified) ? line.Price : 0),
                    new XElement("CardOrCustNo", string.Empty),
                    new XElement("ManualDiscountPercent", 0),
                    new XElement("ManualDiscountAmount", 0),
                    new XElement("DiscInfoLine", 0),
                    new XElement("TotalDiscInfoLine", 0),
                    new XElement("StaffId", staffId),
                    new XElement("EFTCardNumber", string.Empty),
                    new XElement("EFTCardName", string.Empty),
                    new XElement("EFTCardType", string.Empty),
                    new XElement("EFTAuthCode", string.Empty),
                    new XElement("EFTMessage", string.Empty),
                    new XElement("EFTVerificationMethod", 0),
                    new XElement("EFTTransactionNo", 0),
                    new XElement("EFTAuthStatus", 0),
                    new XElement("EFTTransType", 0),
                    new XElement("EFTDateTime", "1900-01-01T01:01:00"),
                    new XElement("ExternalId", 0),
                    new XElement("ExternalLineNo", 0),
                    new XElement("DealItem", (line.IsADeal ? "1" : "0")),  //0=Item line, 1=Deal line
                    new XElement("PriceGroupCode", string.Empty),
                    new XElement("RestMenuType", 0),
                    new XElement("RestMenuTypeCode", string.Empty),
                    new XElement("GuestSeatNo", 0),
                    new XElement("KitchenRouting", 0),
                    new XElement("LineKitchenStatus", 0),
                    new XElement("RecommendedItem", false),
                    new XElement("LineKitchenStatusCode", 0)
                );
        }

        private XElement MobileTransLine(string id, OneListItem line, string staffId)
        {
            return new XElement("MobileTransactionLine",
                    new XElement("Id", id),
                    new XElement("LineNo", LineNumberToNav(line.LineNumber)),
                    new XElement("OrigTransLineNo", 0),
                    new XElement("OrigTransPos", string.Empty),
                    new XElement("OrigTransNo", 0),
                    new XElement("OrigTransStore", string.Empty),
                    new XElement("EntryStatus", (int)EntryStatus.Normal),
                    new XElement("LineType", (int)LineType.Item),
                    new XElement("Number", line.ItemId),
                    new XElement("Barcode", string.Empty),
                    new XElement("CurrencyCode", string.Empty),
                    new XElement("CurrencyFactor", 1),
                    new XElement("VariantCode", line.VariantId),
                    new XElement("UomId", line.UnitOfMeasureId),
                    new XElement("NetPrice", line.NetPrice),
                    new XElement("Price", (line.PriceModified) ? 0 : line.Price),
                    new XElement("Quantity", ConvertTo.SafeStringDecimal(line.Quantity)),
                    new XElement("DiscountAmount", ConvertTo.SafeStringDecimal(line.DiscountAmount)),
                    new XElement("DiscountPercent", ConvertTo.SafeStringDecimal(line.DiscountPercent)),
                    new XElement("NetAmount", line.NetAmount),
                    new XElement("TAXAmount", line.TaxAmount),
                    new XElement("TAXProductCode", string.Empty),
                    new XElement("TAXBusinessCode", string.Empty),
                    new XElement("ManualPrice", (line.PriceModified) ? line.Price : 0),
                    new XElement("CardOrCustNo", string.Empty),
                    new XElement("ManualDiscountPercent", 0),
                    new XElement("ManualDiscountAmount", 0),
                    new XElement("DiscInfoLine", 0),
                    new XElement("TotalDiscInfoLine", 0),
                    new XElement("StaffId", staffId),
                    new XElement("EFTCardNumber", string.Empty),
                    new XElement("EFTCardName", string.Empty),
                    new XElement("EFTCardType", string.Empty),
                    new XElement("EFTAuthCode", string.Empty),
                    new XElement("EFTMessage", string.Empty),
                    new XElement("EFTVerificationMethod", 0),
                    new XElement("EFTTransactionNo", 0),
                    new XElement("EFTAuthStatus", 0),
                    new XElement("EFTTransType", 0),
                    new XElement("EFTDateTime", "1900-01-01T01:01:00"),
                    new XElement("ExternalId", 0),
                    new XElement("ExternalLineNo", 0),
                    new XElement("DealItem", (line.IsADeal ? "1" : "0")),  //0=Item line, 1=Deal line
                    new XElement("PriceGroupCode", string.Empty),
                    new XElement("RestMenuType", 0),
                    new XElement("RestMenuTypeCode", string.Empty),
                    new XElement("GuestSeatNo", 0),
                    new XElement("KitchenRouting", 0),
                    new XElement("LineKitchenStatus", 0),
                    new XElement("RecommendedItem", false),
                    new XElement("LineKitchenStatusCode", 0)
                );
        }

        private XElement MobileTransLine(string id, OneListPublishedOffer line, string staffId)
        {
            return new XElement("MobileTransactionLine",
                    new XElement("Id", id),
                    new XElement("LineNo", LineNumberToNav(line.LineNumber)),
                    new XElement("OrigTransLineNo", 0),
                    new XElement("OrigTransPos", string.Empty),
                    new XElement("OrigTransNo", 0),
                    new XElement("OrigTransStore", string.Empty),
                    new XElement("EntryStatus", (int)EntryStatus.Normal),
                    new XElement("LineType", (int)LineType.Coupon),
                    new XElement("Number", string.Empty),
                    new XElement("Barcode", line.Id), // COUPON CODE
                    new XElement("CurrencyCode", string.Empty),
                    new XElement("CurrencyFactor", 1),
                    new XElement("VariantCode", string.Empty),
                    new XElement("UomId", string.Empty),
                    new XElement("NetPrice", 0),
                    new XElement("Price", 0),
                    new XElement("Quantity", 0),
                    new XElement("DiscountAmount", 0),
                    new XElement("DiscountPercent", 0),
                    new XElement("NetAmount", 0),
                    new XElement("TAXAmount", 0),
                    new XElement("TAXProductCode", string.Empty),
                    new XElement("TAXBusinessCode", string.Empty),
                    new XElement("ManualPrice", 0),
                    new XElement("CardOrCustNo", string.Empty),
                    new XElement("ManualDiscountPercent", 0),
                    new XElement("ManualDiscountAmount", 0),
                    new XElement("DiscInfoLine", 0),
                    new XElement("TotalDiscInfoLine", 0),
                    new XElement("StaffId", staffId),
                    new XElement("EFTCardNumber", string.Empty),
                    new XElement("EFTCardName", string.Empty),
                    new XElement("EFTCardType", string.Empty),
                    new XElement("EFTAuthCode", string.Empty),
                    new XElement("EFTMessage", string.Empty),
                    new XElement("EFTVerificationMethod", 0),
                    new XElement("EFTTransactionNo", 0),
                    new XElement("EFTAuthStatus", 0),
                    new XElement("EFTTransType", 0),
                    new XElement("EFTDateTime", "1900-01-01T01:01:00"),
                    new XElement("ExternalId", 0),
                    new XElement("ExternalLineNo", 0),
                    new XElement("DealItem", "0"),
                    new XElement("PriceGroupCode", string.Empty),
                    new XElement("RestMenuType", 0),
                    new XElement("RestMenuTypeCode", string.Empty),
                    new XElement("GuestSeatNo", 0),
                    new XElement("KitchenRouting", 0),
                    new XElement("LineKitchenStatus", 0),
                    new XElement("RecommendedItem", false),
                    new XElement("LineKitchenStatusCode", 0)
                );
        }

        private XElement MobileTransDiscoutLine(string id, OrderDiscountLine posline)
        {
            return new XElement("MobileTransDiscountLine",
                    new XElement("Id", id),
                    new XElement("LineNo", LineNumberToNav(posline.LineNumber)),
                    new XElement("No", posline.No),
                    new XElement("DiscountType", (int)posline.DiscountType),
                    new XElement("OfferNo", posline.OfferNumber),
                    new XElement("PeriodicDiscType", (int)posline.PeriodicDiscType),
                    new XElement("PeriodicDiscGroup", posline.PeriodicDiscGroup),
                    new XElement("Description", posline.Description),
                    new XElement("DiscountAmount", ConvertTo.SafeStringDecimal(posline.DiscountAmount)),
                    new XElement("DiscountPercent", ConvertTo.SafeStringDecimal(posline.DiscountPercent))
                );
        }

        private XElement CreateTransactionSubLine(string id, OrderHospSubLine rq, ref int lineNo, int parLineNo, SubLineType subLineType, string staffId)
        {
            if (subLineType == SubLineType.Deal)
            {
                rq.ModifierGroupCode = string.Empty;
                rq.ModifierSubCode = string.Empty;
            }
            else if (subLineType == SubLineType.Modifier)
            {
                rq.DealCode = "0";
                rq.DealLineId = 0;
                rq.DealModifierLineId = 0;
            }
            else if (subLineType == SubLineType.Text)
            {
                rq.ItemId = string.Empty;
                rq.DealCode = "0";
                rq.DealLineId = 0;
                rq.DealModifierLineId = 0;
            }

            if (rq.LineNumber == 0)
                rq.LineNumber = ++lineNo;

            return new XElement("MobileTransactionSubLine",
                    new XElement("Id", id),
                    new XElement("LineNo", LineNumberToNav(rq.LineNumber)),
                    new XElement("ParentLineNo", LineNumberToNav((rq.ParentSubLineId > 0) ? rq.ParentSubLineId : parLineNo)),
                    new XElement("EntryStatus", (int)EntryStatus.Normal),
                    new XElement("ParentLineIsSubline", (rq.ParentSubLineId > 0) ? 1 : 0),
                    new XElement("LineType", (int)subLineType), //0=item, 1=DealItem,2=Text 
                    new XElement("Number", rq.ItemId),
                    new XElement("Barcode", string.Empty),
                    new XElement("VariantCode", rq.VariantId),
                    new XElement("UomId", rq.Uom),
                    new XElement("NetPrice", ConvertTo.SafeStringDecimal(rq.NetPrice)),
                    new XElement("Price", ConvertTo.SafeStringDecimal(rq.Price)),
                    new XElement("Quantity", ConvertTo.SafeStringDecimal(rq.Quantity)),
                    new XElement("DiscountAmount", ConvertTo.SafeStringDecimal(rq.DiscountAmount)),
                    new XElement("DiscountPercent", ConvertTo.SafeStringDecimal(rq.DiscountPercent)),
                    new XElement("NetAmount", ConvertTo.SafeStringDecimal(rq.NetAmount)),
                    new XElement("TAXAmount", ConvertTo.SafeStringDecimal(rq.TAXAmount)),
                    new XElement("TAXProductCode", string.Empty),
                    new XElement("TAXBusinessCode", string.Empty),
                    new XElement("ManualDiscountPercent", ConvertTo.SafeStringDecimal(rq.ManualDiscountPercent)),
                    new XElement("ManualDiscountAmount", ConvertTo.SafeStringDecimal(rq.ManualDiscountAmount)),
                    new XElement("DiscInfoLine", 0),
                    new XElement("TotalDiscInfoLine", 0),
                    new XElement("Description", rq.Description),
                    new XElement("VariantDescription", rq.VariantDescription),
                    new XElement("UomDescription", rq.Uom),
                    new XElement("TransDate", string.Empty),
                    new XElement("ExternalId", 0),
                    new XElement("ExternalLineNo", 0),
                    new XElement("StaffId", staffId),
                    new XElement("KitchenRouting", 0),
                    new XElement("LineKitchenStatus", 0),
                    new XElement("LineKitchenStatusCode", string.Empty),
                    new XElement("ModifierGroupCode", rq.ModifierGroupCode),
                    new XElement("ModifierSubCode", rq.ModifierSubCode),
                    new XElement("DealLine", rq.DealLineId),
                    new XElement("DealModLine", rq.DealModifierLineId),
                    new XElement("DealId", rq.DealCode),
                    new XElement("PriceReductionOnExclusion", (rq.PriceReductionOnExclusion ? 1 : 0))
                );
        }

        private XElement CreateTransactionSubLine(string id, OneListItemSubLine rq, int parLineNo, string parentItemNo, string staffId)
        {
            return new XElement("MobileTransactionSubLine",
                    new XElement("Id", id),
                    new XElement("LineNo", LineNumberToNav(rq.LineNumber)),
                    new XElement("ParentLineNo", LineNumberToNav((rq.ParentSubLineId > 0) ? rq.ParentSubLineId : parLineNo)),
                    new XElement("EntryStatus", (int)EntryStatus.Normal),
                    new XElement("ParentLineIsSubline", (rq.ParentSubLineId > 0) ? 1 : 0),
                    new XElement("LineType", (int)rq.Type), // 0=Modifier, 1=Deal, 2=Text 
                    new XElement("Number", rq.ItemId),
                    new XElement("Barcode", string.Empty),
                    new XElement("VariantCode", rq.VariantId),
                    new XElement("UomId", rq.Uom),
                    new XElement("NetPrice", 0),
                    new XElement("Price", 0),
                    new XElement("Quantity", ConvertTo.SafeStringDecimal(rq.Quantity)),
                    new XElement("DiscountAmount", 0),
                    new XElement("DiscountPercent", 0),
                    new XElement("NetAmount", 0),
                    new XElement("TAXAmount", 0),
                    new XElement("TAXProductCode", string.Empty),
                    new XElement("TAXBusinessCode", string.Empty),
                    new XElement("ManualDiscountPercent", 0),
                    new XElement("ManualDiscountAmount", 0),
                    new XElement("DiscInfoLine", 0),
                    new XElement("TotalDiscInfoLine", 0),
                    new XElement("Description", rq.Description),
                    new XElement("VariantDescription", rq.VariantDescription),
                    new XElement("UomDescription", rq.Uom),
                    new XElement("TransDate", string.Empty),
                    new XElement("ExternalId", 0),
                    new XElement("ExternalLineNo", 0),
                    new XElement("StaffId", staffId),
                    new XElement("KitchenRouting", 0),
                    new XElement("LineKitchenStatus", 0),
                    new XElement("LineKitchenStatusCode", 0),
                    new XElement("ModifierGroupCode", rq.ModifierGroupCode),
                    new XElement("ModifierSubCode", rq.ModifierSubCode),
                    new XElement("DealLine", string.IsNullOrEmpty(parentItemNo) ? 0 : LineNumberToNav(rq.DealLineId)),
                    new XElement("DealModLine", string.IsNullOrEmpty(parentItemNo) ? 0 : LineNumberToNav(rq.DealModLineId)),
                    new XElement("DealId", string.IsNullOrEmpty(parentItemNo) ? "0" : parentItemNo),
                    new XElement("PriceReductionOnExclusion", 0)
                );
        }

        private XElement TransPaymentLine(string id, string storeId, OrderPayment tenderLine, int lineNumber)
        {
            return new XElement("MobileTransactionLine",
                    new XElement("Id", id),
                    new XElement("LineNo", LineNumberToNav(lineNumber)),
                    new XElement("OrigTransLineNo", "0"),
                    new XElement("OrigTransPos", string.Empty),
                    new XElement("OrigTransNo", "0"),
                    new XElement("OrigTransStore", string.Empty),
                    new XElement("EntryStatus", "0"),
                    new XElement("LineType", (int)LineType.Payment),
                    new XElement("Number", tenderLine.TenderType.ToString()),
                    new XElement("Barcode", string.Empty),
                    new XElement("CurrencyCode", tenderLine.CurrencyCode),
                    new XElement("CurrencyFactor", ConvertTo.SafeDecimalString(tenderLine.CurrencyFactor.ToString())),
                    new XElement("VariantCode", string.Empty),
                    new XElement("UomId", string.Empty),
                    new XElement("NetPrice", 0),
                    new XElement("Price", 0),
                    new XElement("Quantity", 0),
                    new XElement("DiscountAmount", 0),
                    new XElement("DiscountPercent", 0),
                    new XElement("NetAmount", tenderLine.Amount),
                    new XElement("TAXAmount", 0),
                    new XElement("TAXProductCode", string.Empty),
                    new XElement("TAXBusinessCode", string.Empty),
                    new XElement("ManualPrice", 0),
                    new XElement("CardOrCustNo", string.Empty),
                    new XElement("ManualDiscountPercent", 0),
                    new XElement("ManualDiscountAmount", 0),
                    new XElement("DiscInfoLine", 0),
                    new XElement("TotalDiscInfoLine", 0),
                    new XElement("StoreId", storeId),
                    new XElement("TerminalId", string.Empty),
                    new XElement("StaffId", string.Empty),
                    new XElement("EFTCardNumber", tenderLine.CardNumber),
                    new XElement("EFTCardName", string.Empty),
                    new XElement("EFTCardType", tenderLine.CardType),
                    new XElement("EFTAuthCode", tenderLine.AuthorizationCode),
                    new XElement("EFTMessage", string.Empty),
                    new XElement("EFTVerificationMethod", 0),
                    new XElement("EFTTransactionNo", tenderLine.TokenNumber),
                    new XElement("EFTAuthStatus", 0),
                    new XElement("EFTTransType", (int)tenderLine.PaymentType),
                    new XElement("EFTDateTime", ConvertTo.SafeStringDateTime(tenderLine.PreApprovedValidDate)),
                    new XElement("ExternalLineNo", 0),
                    new XElement("ExternalParentLineNo", 0),
                    new XElement("RestMenuType", 0),
                    new XElement("RestMenuTypeCode", string.Empty),
                    new XElement("RecommendedItem", false),
                    new XElement("GuestSeatNo", 0),
                    new XElement("KitchenRouting", 0),
                    new XElement("LineKitchenStatus", 0),
                    new XElement("RecommendedItem", false),
                    new XElement("LineKitchenStatusCode", 0),
                    new XElement("ParentLineNo", 0)
                );
        }

        private void MobileTransSubLine(XElement body, string id, OrderHospLine posline, string staffId, ref int subLineCounter)
        {
            if (posline.SubLines != null)
            {
                foreach (OrderHospSubLine dLine in posline.SubLines)
                {
                    body.Add(CreateTransactionSubLine(id, dLine, ref subLineCounter, posline.LineNumber, dLine.Type, staffId));
                }
            }
        }

        private void MobileTransSubLine(XElement body, string id, OneListItem posline, ref int subLineNo, string staffId)
        {
            if (posline.OnelistSubLines == null)
                return;

            foreach (OneListItemSubLine subLine in posline.OnelistSubLines)
            {
                if (subLine.LineNumber == 0)
                    subLine.LineNumber = ++subLineNo;

                switch (subLine.Type)
                {
                    case SubLineType.Modifier:
                        body.Add(CreateTransactionSubLine(id, subLine, posline.LineNumber, string.Empty, staffId));
                        break;

                    case SubLineType.Deal:
                        body.Add(CreateTransactionSubLine(id, subLine, posline.LineNumber, posline.ItemId, staffId));
                        break;
                }
            }
        }

        private int FindLineIndex(List<OrderHospLine> list, int lineNumber)
        {
            //TODO, make sure this is correct, only go by lineNumber
            int idx = 0;
            foreach (OrderHospLine rs in list)
            {
                if (rs.LineNumber == lineNumber)
                {
                    break;
                }
                idx++;
            }
            return idx;
        }

        private LineType FindLineType(List<OrderHospLine> list, int lineNumber)
        {
            //finds the item
            foreach (OrderHospLine rs in list)
            {
                if (rs.LineNumber == lineNumber)
                {
                    return rs.LineType;
                }
            }
            return LineType.Unknown;
        }
    }
}
