using System.Collections.Generic;

using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Base;
using LSOmni.Common.Util;
using System.Security.AccessControl;

namespace LSOmni.BLL.Loyalty
{
    public class ReplicationBLL : BaseLoyBLL
    {
        public ReplicationBLL(BOConfiguration config, int timeoutInSeconds) : base(config, timeoutInSeconds)
        {
        }

        public virtual ReplBarcodeResponse ReplEcommBarcodes(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplBarcodeResponse rs = new ReplBarcodeResponse()
            {
                Barcodes = BOAppConnection.ReplicateBarcodes(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Barcodes.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplItemResponse ReplEcommItems(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplItemResponse rs = new ReplItemResponse()
            {
                Items = BOAppConnection.ReplicateItems(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Items.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplExtendedVariantValuesResponse ReplEcommExtendedVariants(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplExtendedVariantValuesResponse rs = new ReplExtendedVariantValuesResponse()
            {
                ExtendedVariantValue = BOAppConnection.ReplicateExtendedVariantValues(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.ExtendedVariantValue.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplItemCategoryResponse ReplEcommItemCategories(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplItemCategoryResponse rs = new ReplItemCategoryResponse()
            {
                ItemCategories = BOAppConnection.ReplicateItemCategory(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.ItemCategories.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplProductGroupResponse ReplEcommProductGroups(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplProductGroupResponse rs = new ReplProductGroupResponse()
            {
                ProductGroups = BOAppConnection.ReplicateProductGroups(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.ProductGroups.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplStoreResponse ReplEcommStores(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplStoreResponse rs = new ReplStoreResponse()
            {
                Stores = BOAppConnection.ReplicateStores(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Stores.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplItemUnitOfMeasureResponse ReplEcommItemUnitOfMeasures(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplItemUnitOfMeasureResponse rs = new ReplItemUnitOfMeasureResponse()
            {
                ItemUnitOfMeasures = BOAppConnection.ReplicateItemUOM(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.ItemUnitOfMeasures.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplUnitOfMeasureResponse ReplEcommUnitOfMeasures(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplUnitOfMeasureResponse rs = new ReplUnitOfMeasureResponse()
            {
                UnitOfMeasures = BOAppConnection.ReplicateUnitOfMeasure(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.UnitOfMeasures.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplCollectionResponse ReplEcommCollection(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplCollectionResponse rs = new ReplCollectionResponse()
            {
                Collection = BOAppConnection.ReplicateCollection(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Collection.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplCurrencyResponse ReplEcommCurrency(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplCurrencyResponse rs = new ReplCurrencyResponse()
            {
                Currencies = BOAppConnection.ReplicateCurrency(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Currencies.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplCurrencyExchRateResponse ReplEcommCurrencyRate(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplCurrencyExchRateResponse rs = new ReplCurrencyExchRateResponse()
            {
                CurrencyExchRates = BOAppConnection.ReplicateCurrencyExchRate(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.CurrencyExchRates.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplItemVariantRegistrationResponse ReplEcommItemVariantRegistrations(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplItemVariantRegistrationResponse rs = new ReplItemVariantRegistrationResponse()
            {
                ItemVariantRegistrations = BOAppConnection.ReplicateItemVariantRegistration(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.ItemVariantRegistrations.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplItemVariantResponse ReplEcommItemVariants(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplItemVariantResponse rs = new ReplItemVariantResponse()
            {
                ItemVariants = BOAppConnection.ReplicateItemVariant(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.ItemVariants.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplPriceResponse ReplEcommPrices(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplPriceResponse rs = new ReplPriceResponse()
            {
                Prices = BOAppConnection.ReplicatePrice(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Prices.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplPriceResponse ReplEcommBasePrices(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplPriceResponse rs = new ReplPriceResponse()
            {
                Prices = BOAppConnection.ReplicateBasePrice(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Prices.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplImageResponse ReplEcommImages(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplImageResponse rs = new ReplImageResponse()
            {
                Images = BOLoyConnection.ReplEcommImages(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Images.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplImageLinkResponse ReplEcommImageLinks(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplImageLinkResponse rs = new ReplImageLinkResponse()
            {
                ImageLinks = BOLoyConnection.ReplEcommImageLinks(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.ImageLinks.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplAttributeResponse ReplEcommAttribute(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplAttributeResponse rs = new ReplAttributeResponse()
            {
                Attributes = BOLoyConnection.ReplEcommAttribute(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Attributes.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplAttributeValueResponse ReplEcommAttributeValue(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplAttributeValueResponse rs = new ReplAttributeValueResponse()
            {
                Values = BOLoyConnection.ReplEcommAttributeValue(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Values.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplAttributeOptionValueResponse ReplEcommAttributeOptionValue(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplAttributeOptionValueResponse rs = new ReplAttributeOptionValueResponse()
            {
                OptionValues = BOLoyConnection.ReplEcommAttributeOptionValue(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.OptionValues.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplVendorResponse ReplEcommVendor(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplVendorResponse rs = new ReplVendorResponse()
            {
                Vendors = BOAppConnection.ReplicateVendors(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Vendors.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplLoyVendorItemMappingResponse ReplEcommVendorItemMapping(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplLoyVendorItemMappingResponse rs = new ReplLoyVendorItemMappingResponse()
            {
                Mapping = BOLoyConnection.ReplEcommVendorItemMapping(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Mapping.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplDataTranslationResponse ReplEcommDataTranslation(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplDataTranslationResponse rs = new ReplDataTranslationResponse()
            {
                Texts = BOLoyConnection.ReplEcommDataTranslation(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Texts.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplDataTranslationResponse ReplEcommItemHtmlTranslation(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplDataTranslationResponse rs = new ReplDataTranslationResponse()
            {
                Texts = BOLoyConnection.ReplEcommItemHtmlTranslation(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Texts.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplDataTranslationResponse ReplEcommDealHtmlTranslation(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplDataTranslationResponse rs = new ReplDataTranslationResponse()
            {
                Texts = BOLoyConnection.ReplEcommDealHtmlTranslation(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Texts.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplDataTranslationLangCodeResponse ReplEcommDataTranslationLangCode(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplDataTranslationLangCodeResponse rs = new ReplDataTranslationLangCodeResponse()
            {
                Codes = BOLoyConnection.ReplicateEcommDataTranslationLangCode(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Codes.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplDiscountResponse ReplEcommDiscount(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplDiscountResponse rs = new ReplDiscountResponse()
            {
                Discounts = BOAppConnection.ReplicateDiscounts(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Discounts.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplMixMatchResponse ReplEcommMixAndMatch(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplMixMatchResponse rs = new ReplMixMatchResponse()
            {
                Discounts = BOAppConnection.ReplicateMixAndMatch(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Discounts.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplDiscountSetupResponse ReplEcommDiscountSetup(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplDiscountSetupResponse rs = new ReplDiscountSetupResponse()
            {
                Discounts = BOAppConnection.ReplicateDiscountSetup(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Discounts.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplDiscountValidationResponse ReplEcommDiscountValidation(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplDiscountValidationResponse rs = new ReplDiscountValidationResponse()
            {
                DiscountValidations = BOAppConnection.ReplicateDiscountValidations(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.DiscountValidations.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplStoreTenderTypeResponse ReplEcommStoreTenderTypes(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplStoreTenderTypeResponse rs = new ReplStoreTenderTypeResponse()
            {
                StoreTenderTypes = BOAppConnection.ReplicateStoreTenderType(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.StoreTenderTypes.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplTaxSetupResponse ReplEcommTaxSetup(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplTaxSetupResponse rs = new ReplTaxSetupResponse()
            {
                TaxSetups = BOAppConnection.ReplicateTaxSetup(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.TaxSetups.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplShippingAgentResponse ReplEcommShippingAgent(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplShippingAgentResponse rs = new ReplShippingAgentResponse()
            {
                ShippingAgent = BOLoyConnection.ReplEcommShippingAgent(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.ShippingAgent.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplCustomerResponse ReplEcommMember(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplCustomerResponse rs = new ReplCustomerResponse()
            {
                Customers = BOLoyConnection.ReplEcommMember(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Customers.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplCustomerResponse ReplEcommCustomer(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplCustomerResponse rs = new ReplCustomerResponse()
            {
                Customers = BOAppConnection.ReplicateCustomer(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Customers.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplCountryCodeResponse ReplEcommCountryCode(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplCountryCodeResponse rs = new ReplCountryCodeResponse()
            {
                Codes = BOLoyConnection.ReplEcommCountryCode(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Codes.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplValidationScheduleResponse ReplEcommValidationSchedule(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplValidationScheduleResponse rs = new ReplValidationScheduleResponse()
            {
                Schedules = BOAppConnection.ReplicateValidationSchedule(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Schedules.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplHierarchyResponse ReplEcommHierarchy(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplHierarchyResponse rs = new ReplHierarchyResponse()
            {
                Hierarchies = BOAppConnection.ReplicateHierarchy(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Hierarchies.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplHierarchyNodeResponse ReplEcommHierarchyNode(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplHierarchyNodeResponse rs = new ReplHierarchyNodeResponse()
            {
                Nodes = BOAppConnection.ReplicateHierarchyNode(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Nodes.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplHierarchyLeafResponse ReplEcommHierarchyLeaf(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplHierarchyLeafResponse rs = new ReplHierarchyLeafResponse()
            {
                Leafs = BOAppConnection.ReplicateHierarchyLeaf(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Leafs.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplHierarchyHospDealResponse ReplEcommHierarchyHospDeal(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplHierarchyHospDealResponse rs = new ReplHierarchyHospDealResponse()
            {
                Items = BOAppConnection.ReplicateHierarchyHospDeal(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Items.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplHierarchyHospDealLineResponse ReplEcommHierarchyHospDealLine(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplHierarchyHospDealLineResponse rs = new ReplHierarchyHospDealLineResponse()
            {
                Items = BOAppConnection.ReplicateHierarchyHospDealLine(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Items.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplItemRecipeResponse ReplicateItemRecipe(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplItemRecipeResponse rs = new ReplItemRecipeResponse()
            {
                Items = BOAppConnection.ReplicateItemRecipe(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Items.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplItemModifierResponse ReplEcommItemModifier(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplItemModifierResponse rs = new ReplItemModifierResponse()
            {
                Modifiers = BOAppConnection.ReplicateItemModifier(replRequest.AppId, string.Empty, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", rs.Modifiers.Count, rs.LastKey, rs.RecordsRemaining);
            return rs;
        }

        public virtual ReplInvStatusResponse ReplEcommInventoryStatus(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            ReplInvStatusResponse resp = new ReplInvStatusResponse()
            {
                Items = BOLoyConnection.ReplEcommInventoryStatus(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining),
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey
            };
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", resp.Items.Count, resp.LastKey, resp.RecordsRemaining);
            return resp;
        }

        public virtual ReplFullItemResponse ReplEcommFullItem(ReplRequest replRequest)
        {
            string lastkey = replRequest.LastKey;
            string maxkey = replRequest.MaxKey;
            int recordsRemaining = 0;
            config.AppId = replRequest.AppId;

            List<LoyItem> ids = BOLoyConnection.ReplEcommFullItem(replRequest.AppId, replRequest.StoreId, replRequest.BatchSize, replRequest.FullReplication, ref lastkey, ref maxkey, ref recordsRemaining);

            ReplFullItemResponse resp = new ReplFullItemResponse()
            {
                RecordsRemaining = recordsRemaining,
                LastKey = lastkey,
                MaxKey = maxkey,
                Items = new List<LoyItem>()
            };

            foreach (LoyItem item in ids)
            {
                if (item.IsDeleted)
                {
                    resp.Items.Add(item);
                }
                else
                {
                    resp.Items.Add(BOLoyConnection.ItemGetById(item.Id, replRequest.StoreId, string.Empty, true, new Statistics()));
                }
            }
            logger.Debug(config.LSKey.Key, "Result > Records:{0} LastKey:{1} RecRemain:{2}", resp.Items.Count, resp.LastKey, resp.RecordsRemaining);
            return resp;
        }
    }
}
