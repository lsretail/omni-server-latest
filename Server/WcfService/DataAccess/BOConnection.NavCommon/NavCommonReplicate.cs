using System;
using System.Collections.Generic;
using System.Linq;

using LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping;
using LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Pos.Replication;

namespace LSOmni.DataAccess.BOConnection.NavCommon
{
    public partial class NavCommonBase
    {
        public virtual List<ReplItem> ReplicateItems(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(27, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateItems(mytable);
        }

        public List<ReplBarcode> ReplicateBarcodes(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99001451, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateBarcodes(mytable);
        }

        public List<ReplBarcodeMaskSegment> ReplicateBarcodeMaskSegments(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99001480, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateBarcodeMaskSegments(mytable);
        }

        public List<ReplBarcodeMask> ReplicateBarcodeMasks(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99001459, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateBarcodeMasks(mytable);
        }

        public virtual List<ReplExtendedVariantValue> ReplicateExtendedVariantValues(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(10001413, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateExtendedVariantValues(mytable);
        }

        public List<ReplItemUnitOfMeasure> ReplicateItemUOM(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(5404, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateItemUnitOfMeasure(mytable);
        }

        public virtual List<ReplItemVariantRegistration> ReplicateItemVariantRegistration(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(10001414, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateItemVariantRegistration(mytable);
        }

        public virtual List<ReplVendor> ReplicateVendors(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(23, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateVendor(mytable);
        }

        public virtual List<ReplCurrency> ReplicateCurrency(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(4, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateCurrency(mytable);
        }

        public virtual List<ReplCurrencyExchRate> ReplicateCurrencyExchRate(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(330, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateCurrencyExchRate(mytable);
        }

        public virtual List<ReplItemCategory> ReplicateItemCategory(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(5722, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateItemCategory(mytable);
        }

        public virtual List<ReplItemLocation> ReplicateItemLocation(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            return new List<ReplItemLocation>();  //TODO
        }

        public virtual List<ReplPrice> ReplicatePrice(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(10012861, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicatePrice(mytable);
        }

        public List<ReplProductGroup> ReplicateProductGroups(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication((NAVVersion > new Version("14.2")) ? 10000705 : 5723, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateProductGroups(mytable);
        }

        public virtual List<ReplStore> ReplicateStores(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99001470, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            List<ReplStore> list = rep.ReplicateStores(mytable);

            NAVWebXml xml = new NAVWebXml();
            foreach (ReplStore store in list)
            {
                if (string.IsNullOrWhiteSpace(store.Currency) == false)
                    continue;

                string xmlRequest = xml.GetGeneralWebRequestXML("General Ledger Setup");
                string xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

                if (table == null || table.NumberOfValues == 0)
                    break;

                XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("LCY Code"));
                store.Currency = field.Values[0];
            }
            return list;
        }

        public virtual List<ReplStore> ReplicateStores(string appId, string appType, string storeId, string terminalId)
        {
            string lastKey = string.Empty, maxKey = string.Empty;
            int recordsRemaining = 0;
            XMLTableData mytable = DoReplication(10012806, storeId, appId, appType, 0, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            //Get all storeIds linked to this terminal
            List<ReplStore> list = rep.ReplicateInvStores(mytable, terminalId);
            //Get all stores
            List<ReplStore> allStores = ReplicateStores(appId, appType, storeId, 0, ref lastKey, ref maxKey, ref recordsRemaining);

            //Only return the stores connected to this terminal
            List<ReplStore> InvStores = new List<ReplStore>();
            foreach(ReplStore store in list)
            {
                ReplStore temp = allStores.FirstOrDefault(x => x.Id == store.Id);
                if (temp != null)
                    InvStores.Add(temp);
            }

            return InvStores;
        }

        public virtual List<ReplUnitOfMeasure> ReplicateUnitOfMeasure(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(204, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateUnitOfMeasure(mytable);
        }

        public virtual List<ReplDiscount> ReplicateDiscounts(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(10012862, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateDiscounts(mytable);
        }

        public virtual List<ReplDiscount> ReplicateMixAndMatch(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(10012863, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateDiscounts(mytable);
        }

        public virtual List<ReplDiscountValidation> ReplicateDiscountValidations(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99001481, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateDiscountValidations(mytable);
        }

        public List<ReplStoreTenderType> ReplicateStoreTenderType(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99001462, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateStoreTenderType(mytable);
        }

        public virtual List<ReplHierarchy> ReplicateHierarchy(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            NAVWebXml xml = new NAVWebXml();
            string xmlRequest = xml.GetGeneralWebRequestXML("Hierarchy Date", "Store Code", storeId);
            string xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);

            if (table == null || table.NumberOfValues == 0)
                return null;

            XMLFieldData field = table.FieldList.Find(f => f.FieldName.Equals("Hierarchy Code"));
            string hircode = field.Values[0];

            xmlRequest = xml.GetGeneralWebRequestXML("Hierarchy", "Hierarchy Code", hircode);
            xmlResponse = RunOperation(xmlRequest, true);
            HandleResponseCode(ref xmlResponse);
            table = xml.GetGeneralWebResponseXML(xmlResponse);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateHierarchy(table);
        }

        public virtual List<ReplHierarchyNode> ReplicateHierarchyNode(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(10000921, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateHierarchyNode(mytable);
        }

        public virtual List<ReplHierarchyLeaf> ReplicateHierarchyLeaf(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(10000922, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateHierarchyLeaf(mytable);
        }

        public virtual List<ReplImageLink> ReplEcommImageLinks(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99009064, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateImageLink(mytable);
        }

        public virtual List<ReplImage> ReplEcommImages(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99009063, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateImage(mytable);
        }

        public virtual List<ReplAttribute> ReplEcommAttribute(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(10000784, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateAttribute(mytable);
        }

        public virtual List<ReplAttributeValue> ReplEcommAttributeValue(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(10000786, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateAttributeValue(mytable);
        }

        public virtual List<ReplAttributeOptionValue> ReplEcommAttributeOptionValue(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(10000785, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateAttributeOptionValue(mytable);
        }

        public virtual List<ReplDataTranslation> ReplEcommDataTranslation(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(10000971, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateDataTranslation(mytable);
        }

        public virtual List<ReplShippingAgent> ReplEcommShippingAgent(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(291, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            List<ReplShippingAgent> list = rep.ReplicateShippingAgent(mytable);
            foreach (ReplShippingAgent sa in list)
            {
                string xmlRequest;
                string xmlResponse;
                NAVWebXml xml = new NAVWebXml();
                xmlRequest = xml.GetGeneralWebRequestXML("Shipping Agent Services", "Shipping Agent Code", sa.Id);
                xmlResponse = RunOperation(xmlRequest, true);
                HandleResponseCode(ref xmlResponse);
                XMLTableData table = xml.GetGeneralWebResponseXML(xmlResponse);
                sa.Services = GetShippingAgentService(sa.Id);
            }
            return list;
        }

        public virtual List<ReplCustomer> ReplEcommMember(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99009002, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateMembers(mytable);
        }

        public virtual List<ReplCustomer> ReplicateCustomer(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(18, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateCustomer(mytable);
        }

        public virtual List<ReplStaff> ReplicateStaff(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99001461, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateStaff(mytable);
        }

        public virtual List<ReplStaffStoreLink> ReplicateStaffStoreLink(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99001633, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateStaffStoreLink(mytable);
        }

        public virtual List<ReplTenderType> ReplicateTenderTypes(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99001466, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateTenderTypes(mytable);
        }

        public virtual List<ReplTaxSetup> ReplicateTaxSetup(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(325, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateTaxSetup(mytable);
        }

        public virtual List<ReplStoreTenderTypeCurrency> ReplicateStoreTenderTypeCurrency(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99001636, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateStoreTenderTypeCurrency(mytable);
        }

        public virtual List<ReplTerminal> ReplicateTerminals(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99001471, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateTerminals(mytable);
        }

        public virtual List<ReplCountryCode> ReplEcommCountryCode(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(9, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateCountryCode(mytable);
        }

        public virtual List<ReplPlu> ReplicatePlu(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99009274, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicatePlu(mytable);
        }

        public virtual List<ReplInvStatus> ReplEcommInventoryStatus(string appId, string appType, string storeId, int batchSize, ref string lastKey, ref string maxKey, ref int recordsRemaining)
        {
            XMLTableData mytable = DoReplication(99001608, storeId, appId, appType, batchSize, ref lastKey, out recordsRemaining);

            ReplicateRepository rep = new ReplicateRepository();
            return rep.ReplicateInvStatus(mytable);
        }
    }
}
