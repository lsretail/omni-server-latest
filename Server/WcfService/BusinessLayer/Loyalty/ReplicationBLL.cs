using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.Common.Util;

namespace LSOmni.BLL.Loyalty
{
    public class ReplicationBLL : BaseLoyBLL
    {
        private static LSLogger logger = new LSLogger();

        public ReplicationBLL(BOConfiguration config, int timeoutInSeconds) : base(config, timeoutInSeconds)
        {
        }

        public virtual ReplBarcodeResponse ReplEcommBarcodes(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplBarcodeResponse rs = new ReplBarcodeResponse();
            rs.Barcodes = BOAppConnection.ReplicateBarcodes(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Barcodes.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplItemResponse ReplEcommItems(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplItemResponse rs = new ReplItemResponse();
            rs.Items = BOAppConnection.ReplicateItems(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Items.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplExtendedVariantValuesResponse ReplEcommExtendedVariants(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplExtendedVariantValuesResponse rs = new ReplExtendedVariantValuesResponse();
            rs.ExtendedVariantValue = BOAppConnection.ReplicateExtendedVariantValues(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.ExtendedVariantValue.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplItemCategoryResponse ReplEcommItemCategories(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplItemCategoryResponse rs = new ReplItemCategoryResponse();
            rs.ItemCategories = BOAppConnection.ReplicateItemCategory(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.ItemCategories.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplProductGroupResponse ReplEcommProductGroups(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplProductGroupResponse rs = new ReplProductGroupResponse();
            rs.ProductGroups = BOAppConnection.ReplicateProductGroups(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.ProductGroups.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplStoreResponse ReplEcommStores(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplStoreResponse rs = new ReplStoreResponse();
            rs.Stores = BOAppConnection.ReplicateStores(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Stores.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplItemUnitOfMeasureResponse ReplEcommItemUnitOfMeasures(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplItemUnitOfMeasureResponse rs = new ReplItemUnitOfMeasureResponse();
            rs.ItemUnitOfMeasures = BOAppConnection.ReplicateItemUOM(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.ItemUnitOfMeasures.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplUnitOfMeasureResponse ReplEcommUnitOfMeasures(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplUnitOfMeasureResponse rs = new ReplUnitOfMeasureResponse();
            rs.UnitOfMeasures = BOAppConnection.ReplicateUnitOfMeasure(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.UnitOfMeasures.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplCurrencyResponse ReplEcommCurrency(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplCurrencyResponse rs = new ReplCurrencyResponse();
            rs.Currencies = BOAppConnection.ReplicateCurrency(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Currencies.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplCurrencyExchRateResponse ReplEcommCurrencyRate(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplCurrencyExchRateResponse rs = new ReplCurrencyExchRateResponse();
            rs.CurrencyExchRates = BOAppConnection.ReplicateCurrencyExchRate(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.CurrencyExchRates.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplItemVariantRegistrationResponse ReplEcommItemVariantRegistrations(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplItemVariantRegistrationResponse rs = new ReplItemVariantRegistrationResponse();
            rs.ItemVariantRegistrations = BOAppConnection.ReplicateItemVariantRegistration(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.ItemVariantRegistrations.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplPriceResponse ReplEcommPrices(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplPriceResponse rs = new ReplPriceResponse();
            rs.Prices = BOAppConnection.ReplicatePrice(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Prices.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplPriceResponse ReplEcommBasePrices(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplPriceResponse rs = new ReplPriceResponse();
            rs.Prices = BOAppConnection.ReplicateBasePrice(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Prices.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplImageResponse ReplEcommImages(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplImageResponse rs = new ReplImageResponse();
            rs.Images = BOLoyConnection.ReplEcommImages(replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Images.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplImageLinkResponse ReplEcommImageLinks(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplImageLinkResponse rs = new ReplImageLinkResponse();
            rs.ImageLinks = BOLoyConnection.ReplEcommImageLinks(replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.ImageLinks.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplAttributeResponse ReplEcommAttribute(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplAttributeResponse rs = new ReplAttributeResponse();
            rs.Attributes = BOLoyConnection.ReplEcommAttribute(replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Attributes.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplAttributeValueResponse ReplEcommAttributeValue(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplAttributeValueResponse rs = new ReplAttributeValueResponse();
            rs.Values = BOLoyConnection.ReplEcommAttributeValue(replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Values.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplAttributeOptionValueResponse ReplEcommAttributeOptionValue(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplAttributeOptionValueResponse rs = new ReplAttributeOptionValueResponse();
            rs.OptionValues = BOLoyConnection.ReplEcommAttributeOptionValue(replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.OptionValues.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplVendorResponse ReplEcommVendor(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplVendorResponse rs = new ReplVendorResponse();
            rs.Vendors = BOAppConnection.ReplicateVendors(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Vendors.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplLoyVendorItemMappingResponse ReplEcommVendorItemMapping(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplLoyVendorItemMappingResponse rs = new ReplLoyVendorItemMappingResponse();
            rs.Mapping = BOLoyConnection.ReplEcommVendorItemMapping(replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Mapping.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplDataTranslationResponse ReplEcommDataTranslation(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplDataTranslationResponse rs = new ReplDataTranslationResponse();
            rs.Texts = BOLoyConnection.ReplEcommDataTranslation(replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Texts.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplDiscountResponse ReplEcommDiscount(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplDiscountResponse rs = new ReplDiscountResponse();
            rs.Discounts = BOAppConnection.ReplicateDiscounts(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Discounts.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplDiscountResponse ReplEcommMixAndMatch(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplDiscountResponse rs = new ReplDiscountResponse();
            rs.Discounts = BOAppConnection.ReplicateMixAndMatch(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Discounts.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplDiscountValidationResponse ReplEcommDiscountValidation(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplDiscountValidationResponse rs = new ReplDiscountValidationResponse();
            rs.DiscountValidations= BOAppConnection.ReplicateDiscountValidations(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.DiscountValidations.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplStoreTenderTypeResponse ReplEcommStoreTenderTypes(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplStoreTenderTypeResponse rs = new ReplStoreTenderTypeResponse();
            rs.StoreTenderTypes = BOAppConnection.ReplicateStoreTenderType(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.StoreTenderTypes.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplTaxSetupResponse ReplEcommTaxSetup(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplTaxSetupResponse rs = new ReplTaxSetupResponse();
            rs.TaxSetups = BOAppConnection.ReplicateTaxSetup(string.Empty, string.Empty, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.TaxSetups.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplShippingAgentResponse ReplEcommShippingAgent(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplShippingAgentResponse rs = new ReplShippingAgentResponse();
            rs.ShippingAgent = BOLoyConnection.ReplEcommShippingAgent(replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.ShippingAgent.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplCustomerResponse ReplEcommMember(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplCustomerResponse rs = new ReplCustomerResponse();
            rs.Customers = BOLoyConnection.ReplEcommMember(replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Customers.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplCountryCodeResponse ReplEcommCountryCode(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplCountryCodeResponse rs = new ReplCountryCodeResponse();
            rs.Codes = BOLoyConnection.ReplEcommCountryCode(replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Codes.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplHierarchyResponse ReplEcommHierarchy(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplHierarchyResponse rs = new ReplHierarchyResponse();
            rs.Hierarchies = BOAppConnection.ReplicateHierarchy(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Hierarchies.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplHierarchyNodeResponse ReplEcommHierarchyNode(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplHierarchyNodeResponse rs = new ReplHierarchyNodeResponse();
            rs.Nodes = BOAppConnection.ReplicateHierarchyNode(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Nodes.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplHierarchyLeafResponse ReplEcommHierarchyLeaf(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplHierarchyLeafResponse rs = new ReplHierarchyLeafResponse();
            rs.Leafs = BOAppConnection.ReplicateHierarchyLeaf(string.Empty, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);
            rs.RecordsRemaining = recordsRemaining;
            rs.LastKey = lastkey;
            rs.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", rs.Leafs.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplInvStatusResponse ReplEcommInventoryStatus(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            ReplInvStatusResponse resp = new ReplInvStatusResponse();
            resp.Items = BOLoyConnection.ReplEcommInventoryStatus(replRequest.StoreId, replRequest.BatchSize, ref lastkey, ref maxkey, ref recordsRemaining);
            resp.RecordsRemaining = recordsRemaining;
            resp.LastKey = lastkey;
            resp.MaxKey = maxkey;
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", resp.Items.Count, resp.LastKey, resp.RecordsRemaining);
            return resp;
        }

        public virtual ReplFullItemResponse ReplEcommFullItem(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;

            List<LoyItem> ids = BOLoyConnection.ReplEcommFullItem(replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);

            ReplFullItemResponse resp = new ReplFullItemResponse();
            resp.RecordsRemaining = recordsRemaining;
            resp.LastKey = lastkey;
            resp.MaxKey = maxkey;
            resp.Items = new List<LoyItem>();

            foreach (LoyItem item in ids)
            {
                if (item.IsDeleted)
                {
                    resp.Items.Add(item);
                }
                else
                {
                    resp.Items.Add(BOLoyConnection.ItemGetById(item.Id, replRequest.StoreId, string.Empty, true));
                }
            }
            logger.Debug(config.LSKey.Key, "Records {0} LastKey {1} RecRemain {2}", resp.Items.Count, resp.LastKey, resp.RecordsRemaining);
            return resp;
        }
    }
}
