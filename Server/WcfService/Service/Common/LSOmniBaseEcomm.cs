using System;

using LSOmni.BLL.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;

namespace LSOmni.Service
{
    /// <summary>
    /// Base class for JSON, SOAP client and XML   INVENTORY
    /// </summary>
    public partial class LSOmniBase
    {
        public virtual GiftCard GiftCardGetBalance(string cardNo)
        {
            try
            {
                CurrencyBLL bll = new CurrencyBLL(config, clientTimeOutInSeconds);
                return bll.GiftCardGetBalance(cardNo);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "CardNo:{0}", cardNo);
                return new GiftCard(string.Empty);
            }
        }

        #region Replication

        public virtual ReplBarcodeResponse ReplEcommBarcodes(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommBarcodes(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest);
                return null; //never gets here
            }
        }

        public virtual ReplCurrencyResponse ReplEcommCurrency(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommCurrency(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplCurrencyExchRateResponse ReplEcommCurrencyRate(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommCurrencyRate(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplExtendedVariantValuesResponse ReplEcommExtendedVariants(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommExtendedVariants(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplImageLinkResponse ReplEcommImageLinks(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommImageLinks(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplImageResponse ReplEcommImages(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommImages(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplItemCategoryResponse ReplEcommItemCategories(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommItemCategories(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplItemResponse ReplEcommItems(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommItems(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplItemUnitOfMeasureResponse ReplEcommItemUnitOfMeasures(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommItemUnitOfMeasures(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplItemVariantRegistrationResponse ReplEcommItemVariantRegistrations(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommItemVariantRegistrations(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplPriceResponse ReplEcommPrices(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommPrices(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplPriceResponse ReplEcommBasePrices(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommBasePrices(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplProductGroupResponse ReplEcommProductGroups(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommProductGroups(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplStoreResponse ReplEcommStores(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommStores(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplUnitOfMeasureResponse ReplEcommUnitOfMeasures(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommUnitOfMeasures(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplVendorResponse ReplEcommVendor(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommVendor(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplLoyVendorItemMappingResponse ReplEcommVendorItemMapping(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommVendorItemMapping(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplAttributeResponse ReplEcommAttribute(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommAttribute(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplAttributeValueResponse ReplEcommAttributeValue(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommAttributeValue(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplAttributeOptionValueResponse ReplEcommAttributeOptionValue(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommAttributeOptionValue(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplDataTranslationResponse ReplEcommDataTranslation(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommDataTranslation(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplDataTranslationLangCodeResponse ReplEcommDataTranslationLangCode(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommDataTranslationLangCode(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplHierarchyResponse ReplEcommHierarchy(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommHierarchy(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplHierarchyNodeResponse ReplEcommHierarchyNode(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommHierarchyNode(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplHierarchyLeafResponse ReplEcommHierarchyLeaf(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommHierarchyLeaf(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplHierarchyHospDealResponse ReplEcommHierarchyHospDeal(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommHierarchyHospDeal(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplHierarchyHospDealLineResponse ReplEcommHierarchyHospDealLine(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommHierarchyHospDealLine(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplItemRecipeResponse ReplEcommItemRecipe(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplicateItemRecipe(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplItemModifierResponse ReplEcommItemModifier(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommItemModifier(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplInvStatusResponse ReplEcommInventoryStatus(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommInventoryStatus(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplFullItemResponse ReplEcommFullItem(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommFullItem(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplDiscountResponse ReplEcommDiscounts(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommDiscount(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplDiscountResponse ReplEcommMixAndMatch(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommMixAndMatch(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplDiscountValidationResponse ReplEcommDiscountValidations(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommDiscountValidation(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplCustomerResponse ReplEcommMember(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommMember(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplShippingAgentResponse ReplEcommShippingAgent(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommShippingAgent(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplCountryCodeResponse ReplEcommCountryCode(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommCountryCode(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplStoreTenderTypeResponse ReplEcommStoreTenderTypes(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommStoreTenderTypes(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        public virtual ReplTaxSetupResponse ReplEcommTaxSetup(ReplRequest replRequest)
        {
            try
            {
                logger.Debug(config.LSKey.Key, LogJson(replRequest));
                ReplicationBLL bll = new ReplicationBLL(config, clientTimeOutInSeconds);
                return bll.ReplEcommTaxSetup(replRequest);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "replRequest:{0}", replRequest.ToString());
                return null; //never gets here
            }
        }

        #endregion Replication
    }
}