using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Retail.SpecialCase;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping.Loyalty
{
    // 
    public class MenuXml : BaseXml
    {
        protected static LSLogger logger = new LSLogger();
        private const string MenuGetAllRequestId = "GET_DYNAMIC_CONTENT"; //"GET_DYN_CONT_HMP_MENUS";
        private XDocument doc = null;
        private XElement elBody = null;

        public MenuXml()
        {
        }

        public string MenuGetAllRequestXML(string storeId, string salesType)
        {
            /*
            <?xml version="1.0"?>
            <Request>
              <Request_ID>GET_DYNAMIC_CONTENT</Request_ID>
              <Request_Body>
                <Restaurant_No>S0005</Restaurant_No>
                <Sales_Type>RESTAURANT</Sales_Type>
                <Terminal_No>P0011</Terminal_No>
              </Request_Body>
            </Request>

             */
            // Create the XML Declaration, and append it to XML document
            XElement root =
                new XElement("Request",
                    new XElement("Request_ID", MenuGetAllRequestId),
                    new XElement("Request_Body",
                        new XElement("Restaurant_No", storeId),
                        new XElement("Sales_Type", salesType),
                        new XElement("Terminal_No", string.Empty)
                    )
                );
            ;
            XDocument doc = new XDocument(new XDeclaration("1.0", wsEncoding, "no"));
            doc.Add(root);
            return doc.ToString();
        }

        public MobileMenu MenuGetAllResponseXML(string responseXml, Currency currency)
        {
            #region xml
            /*
            <Response>
              <Request_ID>GET_DYN_CONT_HMP_MENUS</Request_ID>
              <Response_Code>0000</Response_Code>
              <Response_Text></Response_Text>
              <Response_Body>
                <Dynamic_Content_Id>HOSP MENU</Dynamic_Content_Id>
                <Description>Hospitaliy mobile menu</Description>
                <DynamicContent>
                  <EntryNo>1</EntryNo>
                  <ParentId>0</ParentId>
                  <Level>1</Level>
                  <StartTime></StartTime>
                  <EndTime></EndTime>
                  <TimeWithinBounds>No</TimeWithinBounds>
                  <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
                  <EntryPriceGroup />
                  <EntryType />
                  <EntryID />
                  <Description>Super hot Pizza</Description>
                  <Priority>0</Priority>
                  <ImageID />
                </DynamicContent>
                <DynamicContent>
              ..see full xml response at end of this file
             */
            #endregion xml

            MobileMenu mobileMenu = new MobileMenu();
            //only one menu for now
            bool defaultMenu = true;

            // Create the xml document containe
            doc = XDocument.Parse(responseXml, LoadOptions.None);
            ValidateXmlDoc();
            elBody = doc.Element("Response").Element("Response_Body"); // use the response_body at the "root" 

                        
            //old style, before HMPOS and multi-menu support. There was a Dynamice_Content_Id and Description  
            if (elBody.Element("Dynamic_Content_Id") != null)
            {
                mobileMenu.Id = GetValue(elBody, "Dynamic_Content_Id"); //GetValue(elMenu, "Description");

                Menu menu = new Menu();
                menu.DefaultMenu = defaultMenu; //default menu
                menu.Id = GetValue(elBody, "Dynamic_Content_Id");
                //menu.Id = "1";  //missing 
                //menu.Ver = version;
                menu.Description = GetValue(elBody, "Description");
                //menu.VDes = GetValue(elMenu, "ValidDescription");
                menu.Image = new ImageView(""); // new ImageView(GetValue(elBody, "")); //does not exist in xml

                ParseMenuNodes(menu,"");
                mobileMenu.MenuNodes.Add(menu); //only one menu now

                ParseItems(mobileMenu, currency);
                ParseProdModGroup(mobileMenu, currency);
                ParseOffers(mobileMenu, currency);
                ParseDealGroup(mobileMenu);

                return mobileMenu;
            }
            else // if (elBody.Element("DynamicContentMenu") != null)
            {
                mobileMenu.Id = "";
                mobileMenu.Version = "";
                IEnumerable<XElement> ieDynContMenus = elBody.Descendants("DynamicContentMenu");
                foreach (XElement elDCM in ieDynContMenus)
                {
                    string dynContentMenuId = GetValue(elDCM, "DynamicContentId");
                    Menu menu = new Menu();
                    menu.DefaultMenu = defaultMenu; //default menu
                    menu.Id = dynContentMenuId;
                    //menu.ValidationPeriod
                    //menu.Order  int
                    menu.Description = GetValue(elDCM, "Description");
                    menu.Image = new ImageView(""); // new ImageView(GetValue(elBody, "")); //does not exist in xml

                    menu.Image = new ImageView(""); // new ImageView(GetValue(elBody, "")); //does not exist in xml
                    menu.ValidationTimeWithinBounds = ConvertTo.SafeBoolean(GetValueOrDefault(elDCM, "TimeWithinBounds"));
                    menu.ValidationEndTimeAfterMidnight = ConvertTo.SafeBoolean(GetValueOrDefault(elDCM, "EndTimeAfterMidnight"));
                    menu.ValidationStartTime = ConvertTo.SafeTime(GetValueOrDefault(elDCM, "StartTime"), true);
                    menu.ValidationEndTime = ConvertTo.SafeTime(GetValueOrDefault(elDCM, "EndTime"), true);
                    //check if StartTime or EndTime are 0:00
                    if (menu.ValidationStartTime.TimeOfDay.TotalSeconds == 0.0 && menu.ValidationEndTime.TimeOfDay.TotalSeconds == 0.0)
                    {
                        menu.ValidationEndTime = menu.ValidationEndTime.AddHours(23).AddMinutes(59).AddSeconds(59);
                        menu.ValidationTimeWithinBounds = true;
                    }
                    ParseMenuNodes(menu, dynContentMenuId);
                    mobileMenu.MenuNodes.Add(menu); //only one menu now
                    defaultMenu = false;
                }

                ParseItems(mobileMenu, currency);
                ParseProdModGroup(mobileMenu, currency);
                ParseOffers(mobileMenu, currency);
                ParseDealGroup(mobileMenu);

                return mobileMenu;
            }
        }

        private void ParseDealGroup(MobileMenu mobileMenu)
        {
            //dela modifier groups do not need to exist.. 
            if (elBody.Element("OfferModifierGroups") == null)
                return;

            IEnumerable<XElement> ieDealModGroups = elBody.Descendants("OfferModifierGroups");
            foreach (XElement elDMG in ieDealModGroups)
            {
                string entryNo = GetValue(elDMG, "EntryNo");
                DealModifierGroup dmg = new DealModifierGroup();
                dmg.Id = string.Format("{0}|{1}", GetValue(elDMG, "OfferId"), GetValueOrDefault(elDMG, "OfferLineId", "")); 

                dmg.Description = GetValue(elDMG, "DealModGroupDescription");  //no description
                dmg.MaximumSelection = ConvertTo.SafeDecimal(GetValue(elDMG, "MaxSelection", "0.0"));
                dmg.MinimumSelection = ConvertTo.SafeDecimal(GetValue(elDMG, "MinSelection", "0.0"));
                dmg.RequiredSelection = ToBool(GetValue(elDMG, "DealModRequired")); //Yes No
                //check if a line exist

                IEnumerable<XElement> ieDealModGroupLines = elBody.Descendants("OfferModifierGroupItems");
                foreach (XElement elLine in ieDealModGroupLines)
                {
                    string parentId = GetValue(elLine, "ParentId");
                    if (parentId == entryNo)
                    {
                        //PK is the OfferId + OfferLineId + DealModLineId
                        DealModifier dm = new DealModifier();
                        dm.Id = string.Format("{0}|{1}|{2}",
                            GetValue(elLine, "OfferId"),GetValueOrDefault(elLine, "OfferLineId", ""),GetValueOrDefault(elLine, "DealModLineId", ""));

                        dm.Item = new MenuItem(GetValueOrDefault(elLine, "ItemId", ""));
                        dm.Description = GetValueOrDefault(elLine, "Description", "");
                        dm.DealModifierGroupId = GetValueOrDefault(elLine, "DealModGroupId", "");
                        dm.DisplayOrder = GetDisplayOrderAttributeValue(elLine);
                        dm.Price = ConvertTo.SafeDecimal(GetValue(elLine, "DealModPrice", "0.0"));
                        dm.MaximumSelection = ConvertTo.SafeDecimal(GetValue(elLine, "MaxSelection", "0.0"));
                        dm.MinimumSelection = ConvertTo.SafeDecimal(GetValue(elLine, "MinSelection", "0.0"));
                        dmg.DealModifiers.Add(dm);
                    }
                }
                mobileMenu.DealModifierGroups.Add(dmg);

            }
        }
        private void ParseOffers(MobileMenu mobileMenu, Currency currency)
        {
            //offers product modifier groups do not need to exist.. 
            if (elBody.Element("Offers") == null)
                return;

            IEnumerable<XElement> ieOffers = elBody.Descendants("Offers");
            foreach (XElement elOffer in ieOffers)
            {
                string entryNoOffer = GetValue(elOffer, "EntryNo");
                //deal is the only one supported for now..
                MenuDeal deal = new MenuDeal();
                deal.Id = GetValue(elOffer, "OfferId");
                deal.Description = GetValue(elOffer, "OfferDescription");
                deal.Detail = GetValue(elOffer, "OfferSecondaryDescription");
                deal.Price = new Money(ConvertTo.SafeDecimal(GetValue(elOffer, "OfferPrice", "0.0")), currency);
                ImageView iv = new ImageView(GetValue(elOffer, "ImageID"));
                if (string.IsNullOrWhiteSpace(iv.Id) == false)
                    deal.Images.Add(iv);

                //check if a dealline exist
                if (elBody.Element("OfferLines") == null)
                    break;

                IEnumerable<XElement> ieOfferLines = elBody.Descendants("OfferLines");
                foreach (XElement elLine in ieOfferLines)
                {
                    string parentIdOfferLine = GetValue(elLine, "ParentId");
                    if (parentIdOfferLine == entryNoOffer)
                    {
                        MenuDealLine dl = new MenuDealLine();
                        string entryNo = GetValue(elLine, "EntryNo");
                        //PK is OfferId + OfferLineId
                        dl.Id = deal.Id + "|" + GetValue(elLine, "OfferLineId");

                        dl.DisplayOrder = GetDisplayOrderAttributeValue(elLine);
                        dl.Description = GetValue(elLine, "OfferLineDescription");
                        dl.DefaultItemId = GetValue(elLine, "DefaultItemId"); //recipe if defaultlineid > 0
                        dl.DefaultLineId = GetValueOrDefault(elLine, "DefaultLineId", "0");
                        dl.DefaultLineId = (string.IsNullOrWhiteSpace(dl.DefaultLineId) ? "0" : dl.DefaultLineId); //make sure it has "0" 
                        //OfferId + OfferLineId + DefaultLineId
                        //  which is the default for OfferModifierGroupItems -> offerid, offerlineid,dealmodlineid
                        dl.DefaultDealLineItemId = dl.Id + "|" + dl.DefaultLineId; 

                        if (elLine.Element("OfferLineQty") != null)
                            dl.Quantity = ConvertTo.SafeDecimal(GetValue(elLine, "OfferLineQty", "1.0"));

                        //if DefaultLineId is 0, then it is not in the OfferModifierGroups or OfferModifierGroupItems
                        //Then the DefaultItemId is an item (vs a recipe when DefaultLineId > 0)  
                        //   - old way was to look if it existed in the OfferModifierGroups, no need when we now have DefaultLineId
                        if (dl.DefaultLineId == "0")
                        {
                            //dealline.DefaultDealLineItemId needs to change
                            dl.DefaultDealLineItemId = dl.DefaultItemId;  //dl.Id + "|" + dl.DItId; //OfferId + OfferLineId + ItemId

                            //DefaultItemId,dl.DItId; added to the DeaLineItem (with description)
                            XElement elItem = FindItem(dl.DefaultItemId);
                            MenuDealLineItem dlItem = new MenuDealLineItem();
                            dlItem.Id = dl.DefaultItemId;
                            dlItem.ItemId = dlItem.Id;
                            dlItem.Description = GetValue(elItem, "ItemDescription");
                            dl.DealLineItems.Add(dlItem);
                        }
                        else
                        {
                            //if DefaultLineId is not "0" then add a deallineitem for it with values from OfferModifierGroupItems
                            if (elBody.Element("OfferModifierGroupItems") != null)
                            {
                                IEnumerable<XElement> ieItems = elBody.Descendants("OfferModifierGroupItems");
                                foreach (XElement elItem in ieItems)
                                {
                                    string parentIdOfferModifierGroupItems = GetValue(elItem, "ParentId");
                                    //entryNoOfferline = parentIdOfferModifierGroupItems
                                    if (parentIdOfferModifierGroupItems == entryNo)
                                    {
                                        MenuDealLineItem dlItem = new MenuDealLineItem();
                                        dlItem.Id = dl.Id + "|" + GetValueOrDefault(elItem, "DealModLineId", "");
                                        dlItem.ItemId = GetValue(elItem, "ItemId");  //Recipe.
                                        dlItem.PriceAdjustment = ConvertTo.SafeDecimal(GetValue(elItem, "DealModPrice"));
                                        dlItem.Description = GetValueOrDefault(elItem, "Description", "");
                                        dl.DealLineItems.Add(dlItem);
                                    }
                                }
                            }
                        }

                        //Add dealmodifiergroup under the dealline
                        if (elBody.Element("OfferModifierGroups") != null)
                        {
                            IEnumerable<XElement> ieDMGs = elBody.Descendants("OfferModifierGroups");
                            foreach (XElement elDMG in ieDMGs)
                            {
                                string parentIdOfferModifierGroup = GetValue(elDMG, "ParentId");
                                //entryNoOfferline = parentIdOfferModifierGroup
                                if (parentIdOfferModifierGroup == entryNo)
                                {
                                    var dlDmgId = string.Format("{0}|{1}", GetValue(elDMG, "OfferId"), GetValueOrDefault(elDMG, "OfferLineId", "")); 
                                    dl.DealModifierGroupIds.Add(dlDmgId);
                                }
                            }
                        }
                        deal.DealLines.Add(dl);
                    }
                }
                mobileMenu.Deals.Add(deal);
            }
        }

        private void ParseProdModGroup(MobileMenu mobileMenu, Currency currency)
        {
            //product modifier groups do not need to exist.. 
            if (elBody.Element("ModifierGroups") == null)
                return;
            if (elBody.Element("ModifierGroupItems") == null)
                return;

            IEnumerable<XElement> ieProdModGroups = elBody.Descendants("ModifierGroups");
            foreach (XElement elPMG in ieProdModGroups)
            {
                string entryNo = GetValue(elPMG, "EntryNo");
                ProductModifierGroup pmg = new ProductModifierGroup();
                //ItemModGroupId is the PK
                pmg.Id = GetValue(elPMG, "ItemModGroupId");
                pmg.Description = GetValue(elPMG, "ItemModGroupDescription");
                pmg.MaximumSelection = ConvertTo.SafeDecimal(GetValue(elPMG, "MaxSelection", decimal.MaxValue.ToString()));
                pmg.MinimumSelection = ConvertTo.SafeDecimal(GetValue(elPMG, "MinSelection", "0.0"));
                pmg.RequiredSelection = FindItemModGroupRequired(pmg.Id);
                IEnumerable<XElement> ieProdModGroupLines = elBody.Descendants("ModifierGroupItems");
                foreach (XElement elLine in ieProdModGroupLines)
                {
                    string parentId = GetValue(elLine, "ParentId");
                    decimal pricePercentage = 0M;
                    if (parentId == entryNo)
                    {
                        //text modifier also comes in the ModifierGroupItems
                        //when TextModifier is 0 then it is an item; 1 it is a text modifier 
                        bool isTextModifier = base.ToBool(GetValue(elLine, "TextModifier"));
                        if (isTextModifier == false)
                        {
                            ProductModifier pm = new ProductModifier();
                            pm.Id = pmg.Id + "|" + GetValueOrDefault(elLine, "ItemModSubCode",""); 
                            //GetValue(elLine, "EntryNo"); GetValue(elLine, "ItemId"); was ItemId but cant since it is blank often 
                            pm.DisplayOrder = GetDisplayOrderAttributeValue(elLine);

                            pm.Price = 0M;
                            pm.Item = new MenuItem(GetValue(elLine, "ItemId")); //this itemId is available when price > 0, but blank when percentage
                            pm.UnitOfMeasure = GetValue(elLine, "ItemModUoM", "");
                            pm.Description = GetValueOrDefault(elLine, "Description", "");
                            pm.MaximumSelection = ConvertTo.SafeDecimal(GetValue(elLine, "MaxSelection", "0.0"));
                            pm.MinimumSelection = ConvertTo.SafeDecimal(GetValue(elLine, "MinSelection", "0.0"));
                            pm.Quantity = ConvertTo.SafeDecimal(GetValue(elLine, "ItemModQty", "0.0"));
                            //ItemModPricePercentage has either price or percentage
                            pricePercentage = ConvertTo.SafeDecimal(GetValue(elLine, "ItemModPricePercentage", "0.0"));//was ItemModPrice
                            //Og ItemModPriceType er nýtt – blankt ef verðið er 0 eða ekki tilgreint (texti), Price – þá er verðið 
                            //ItemModPricePercentage svæðinu – Percentage – þá er prósenta í ItemModPricePercentage og 
                            //verðið er reiknað sem prócenta af verði vörunnar sem modifierinn hangir á 

                            //only if ItemModPriceType is Percentage do we need to change the price and calc the percentage
                            string itemModPriceType = GetValue(elLine, "ItemModPriceType", "").ToUpper();
                            if (string.IsNullOrWhiteSpace(itemModPriceType) == false && itemModPriceType == "PERCENTAGE")
                            {
                                //no need to look for things when pct is 0
                                if (pricePercentage != 0M)
                                {
                                    //this does not happen often so OK to use FIND..
                                    string baseItemId = FindItemItByItemModGroupId(pm.Id);//baseItemId has the price for the pm.It
                                    string itemModUom = GetValue(elLine, "ItemModUoM", "");
                                    decimal price = FindPriceByItemId(baseItemId, itemModUom); //price of itemId
                                    //Do the rounding basd on Nav MobileCurrency table
                                    pm.Price = Common.Util.CurrencyGetHelper.RoundToUnit(price * pricePercentage / 100, currency.RoundOffSales, currency.SaleRoundingMethod);  //50%, the pct of the itemPrice
                                }
                            }
                            else
                                pm.Price = pricePercentage;

                            pm.RequiredSelection = FindItemModRequired(pm.Item.Id);

                            if (pm.MaximumSelection == 0.0M)
                                pm.MaximumSelection = decimal.MaxValue;
                            pmg.ProductModifiers.Add(pm);
                        }
                    }
                }
                mobileMenu.ProductModifierGroups.Add(pmg);
            }
        }
        private void ParseItems(MobileMenu mobileMenu, Currency currency)
        {
            IEnumerable<XElement> elItems = elBody.Descendants("Items");

            foreach (XElement elItem in elItems)
            {
                string entryNo = GetValue(elItem, "EntryNo");
                string id = GetValue(elItem, "ItemId");
                string detail = ""; //JIJ get from from itemhtml table  ItemModRequired
                string desc = GetValue(elItem, "ItemDescription");
                ImageView iv = new ImageView(GetValue(elItem, "ImageID"));
                decimal price = ConvertTo.SafeDecimal(GetValue(elItem, "ItemPrice", "0.0"));
                int itemDefaultMenuType = Convert.ToInt32(GetValue(elItem, "ItemDefaultMenuType"));
                List<UnitOfMeasure> uoms = FindUOMs(entryNo, id);
                if (uoms.Count == 0)
                {
                    UnitOfMeasure uom = new UnitOfMeasure();
                    uom.Description = GetValue(elItem, "ItemUoM");//no description in Item node
                    uom.Id = GetValue(elItem, "ItemUoM");
                    uom.Price = ConvertTo.SafeDecimal(GetValue(elItem, "ItemPrice"));
                    uoms.Add(uom);
                }

                string type = GetValue(elItem, "ItemType").ToUpper();
                if (type == "ITEM" && price > 0)
                {
                    Product prod = new Product(id);
                    prod.Description = desc;
                    prod.Detail = detail;
                    if (string.IsNullOrWhiteSpace(iv.Id) == false)
                        prod.Images.Add(iv);
                    prod.Price = new Money(price, currency);
                    prod.DefaultMenuType = itemDefaultMenuType;
                    prod.DefaultUnitOfMeasure = GetValue(elItem, "ItemUoM");
                    prod.UnitOfMeasures = uoms;
                    //get the product mod group for this "item" 
                    if (elBody.Element("ItemModifierGroups") != null)
                    {
                        IEnumerable<XElement> iePMGs = elBody.Descendants("ItemModifierGroups");
                        foreach (XElement elPMG in iePMGs)
                        {
                            string parentId = GetValue(elPMG, "ParentId");
                            if (parentId == entryNo)
                            {
                                //ProductModifierGroup Ids
                                prod.ProductModifierGroups.Add(new ProductModifierGroup(GetValue(elPMG, "ItemModGroupId")));
                            }
                        }
                    }
                    mobileMenu.Products.Add(prod);

                }
                else if (type == "ITEM" && price == 0)
                {
                    IngredientItem item = new IngredientItem(id);
                    item.Description = desc;
                    item.Details = detail;
                    item.UnitOfMeasure = GetValue(elItem, "ItemUoM");
                    item.DefaultMenuType = itemDefaultMenuType;
                    if (string.IsNullOrWhiteSpace(iv.Id) == false)
                        item.Images.Add(iv);
                    mobileMenu.Items.Add(item);
                }
                else if (type == "RECIPE")
                {
                    Recipe recipe = new Recipe(id);
                    recipe.Description = desc;
                    recipe.Detail = detail;
                    if (string.IsNullOrWhiteSpace(iv.Id) == false)
                        recipe.Images.Add(iv);
                    recipe.Price = new Money(price, currency);
                    recipe.DefaultMenuType = itemDefaultMenuType;
                    recipe.UnitOfMeasure = new UnknownUnitOfMeasure(GetValue(elItem, "ItemUoM", ""), id);

                    //get the product mod group for this "item" 
                    if (elBody.Element("ItemModifierGroups") != null)
                    {
                        IEnumerable<XElement> iePMGs = elBody.Descendants("ItemModifierGroups");
                        foreach (XElement elPMG in iePMGs)
                        {
                            string parentId = GetValue(elPMG, "ParentId");
                            if (parentId == entryNo)
                            {
                                //ProductModifierGroup Ids
                                recipe.ProductModifierGroupIds.Add(GetValue(elPMG, "ItemModGroupId"));
                            }
                        }
                    }
                    if (elBody.Element("Ingredients") != null)
                    {
                        IEnumerable<XElement> ieLines = elBody.Descendants("Ingredients");
                        foreach (XElement elIng in ieLines)
                        {
                            string parentId = GetValue(elIng, "ParentId");
                            if (parentId == entryNo)
                            {
                                Ingredient ing = new Ingredient();
                                ing.Id = GetValue(elIng, "IngredientItemId"); //the itemId
                                ing.DisplayOrder = GetDisplayOrderAttributeValue(elIng);
                                ing.UnitOfMeasure = GetValue(elIng, "BOMUoM", "");
                                //PriceReduced contains the price that is deducted on exclusion.
                                ing.PriceReduction = ConvertTo.SafeDecimal(GetValueOrDefault(elIng, "PriceReduction", "0.0"));
                                //An ingredient can be marked as PriceReducedOnExclusion. 
                                ing.PriceReductionOnExclusion = ConvertTo.SafeBoolean(GetValueOrDefault(elIng, "PriceReductionOnExclusion", "0")); //default to false if not found
                                ing.Quantity = ConvertTo.SafeDecimal(GetValue(elIng, "BOMQuantity", "0.0"));
                                ing.MinimumQuantity = ConvertTo.SafeDecimal(GetValue(elIng, "MinSelection", "0.0"));
                                ing.MaximumQuantity = ConvertTo.SafeDecimal(GetValue(elIng, "MaxSelection", "1.0"));
                                recipe.Ingredients.Add(ing);
                            }
                        }
                    }
                    mobileMenu.Recipes.Add(recipe);
                }
            }
        }

        private void GetMenuGroupNode(int level, string entryType, MenuNode mNodeIn, string dynamicContentId)
        {
            MenuNode node = new MenuNode();
            IEnumerable<XElement> elMenuNodes = null;

            if (string.IsNullOrWhiteSpace(dynamicContentId))
            {
                //when only one menu was supported in NAV
                elMenuNodes = from el in elBody.Elements("DynamicContent")
                              where (int)el.Element("Level") == level && el.Element("EntryType").Value == entryType
                              select el;
            }
            else
            {
                elMenuNodes = from el in elBody.Elements("DynamicContent")
                              where (string)el.Element("DynamicContentId") == dynamicContentId &&
                              (int)el.Element("Level") == level && el.Element("EntryType").Value == entryType
                              select el;


            }

            List<MenuNode> menuGroupNode = new List<MenuNode>();
            foreach (XElement elMNode in elMenuNodes)
            {
                string parentId = GetValue(elMNode, "ParentId");
                if (mNodeIn.Id == parentId)
                {
                    MenuNode mNode = new MenuNode();
                    mNode.Id = GetValue(elMNode, "EntryNo");
                    mNode.DisplayOrder = GetDisplayOrderAttributeValue(elMNode);// DisplayOrder not in xml, using EntryNo 
                    mNode.Description = GetValue(elMNode, "Description");
                    mNode.Image = new ImageView(GetValue(elMNode, "ImageID"));
                    mNode.ValidationTimeWithinBounds = ConvertTo.SafeBoolean(GetValue(elMNode, "TimeWithinBounds"));
                    mNode.ValidationEndTimeAfterMidnight = ConvertTo.SafeBoolean(GetValue(elMNode, "EndTimeAfterMidnight"));
                    mNode.ValidationStartTime = ConvertTo.SafeTime(GetValue(elMNode, "StartTime"), true);
                    mNode.ValidationEndTime = ConvertTo.SafeTime(GetValue(elMNode, "EndTime"), true);
                    //check if StartTime or EndTime are 0:00
                    if (mNode.ValidationStartTime.TimeOfDay.TotalSeconds == 0.0 && mNode.ValidationEndTime.TimeOfDay.TotalSeconds == 0.0)
                    {
                        mNode.ValidationEndTime = mNode.ValidationEndTime.AddHours(23).AddMinutes(59).AddSeconds(59);
                        mNode.ValidationTimeWithinBounds = true;
                    }
                    if (mNode.ValidationEndTimeAfterMidnight)
                        mNode.ValidationEndTime = mNode.ValidationEndTime.AddDays(1);
                    mNode.PriceGroup = GetValue(elMNode, "EntryPriceGroup");

                    GetMenuNodeLines(level + 1, mNode, dynamicContentId);
                    GetMenuGroupNode(level + 1, entryType, mNode, dynamicContentId);
                    menuGroupNode.Add(mNode);
                }
            }
            mNodeIn.MenuGroupNodes.AddRange(menuGroupNode);
        }

        private void GetMenuNodeLines(int level, MenuNode mNode, string dynamicContentId)
        {
            IEnumerable<XElement> elMenuNodeLines = null;

            if (string.IsNullOrWhiteSpace(dynamicContentId))
            {
                //when only one menu was supported in NAV
                elMenuNodeLines = from el in elBody.Elements("DynamicContent")
                                  where (int)el.Element("Level") >= level && el.Element("EntryType").Value != ""
                                  select el;
            }
            else
            {
                elMenuNodeLines = from el in elBody.Elements("DynamicContent")
                                  where (string)el.Element("DynamicContentId") == dynamicContentId &&
                                  (int)el.Element("Level") >= level && el.Element("EntryType").Value != ""
                                  select el;
            }

            foreach (XElement elMNodeLine in elMenuNodeLines)
            {
                MenuNodeLine nodeLine = new MenuNodeLine();
                string entryType = GetValue(elMNodeLine, "EntryType").ToUpper();
                if (entryType == "TEXT")
                    continue;

                string parentId = GetValue(elMNodeLine, "ParentId").ToUpper();
                string entryId = GetValue(elMNodeLine, "EntryID");
                nodeLine.Id = entryId;

                //nodeLine.Ord = GetValue(elMNodeLine); //not in xml
                //found the correct line for this menu node
                if (mNode.Id == parentId)
                {
                    nodeLine.NodeLineType = NodeLineType.Deal;

                    //is this id a product or recipe
                    string itemType = FindItemType(entryId).ToUpper();
                    if (itemType == "RECIPE")
                        nodeLine.NodeLineType = NodeLineType.Recipe;
                    else if (itemType == "PRODUCT" || itemType == "ITEM")
                        nodeLine.NodeLineType = NodeLineType.Product;
                    else
                    {
                        if (entryType.ToLower().Contains("offer") == false)
                            logger.Info("NOTE XML PARSE: No menunode item type found for id:{0} - {1}", nodeLine.Id, elMNodeLine.ToString());
                    }

                    mNode.MenuNodeLines.Add(nodeLine);
                }
            }
        }

        private void ParseMenuItemNodes(Menu menu,string dynamicContentId)
        {
            //find the level=1 which are items
            //ParentId = 0 here too. EntryType must be  Item
            IEnumerable<XElement> elMenuNodes = null;
            if (string.IsNullOrWhiteSpace(dynamicContentId))
            {
                //when only one menu was supported in NAV
                elMenuNodes = from el in elBody.Elements("DynamicContent")
                              where el.Element("Level").Value == "1" &&
                              (el.Element("EntryType").Value == "Item" || el.Element("EntryType").Value == "Offer"
                              || el.Element("EntryType").Value == "Recipe")
                              select el;
            }
            else
            {
                elMenuNodes = from el in elBody.Elements("DynamicContent")
                              where el.Element("DynamicContentId").Value == dynamicContentId &&
                              el.Element("Level").Value == "1" &&
                              (el.Element("EntryType").Value == "Item" || el.Element("EntryType").Value == "Offer"
                              || el.Element("EntryType").Value == "Recipe")
                              select el;
            }

            foreach (XElement elMNode in elMenuNodes)
            {
                //this is the toplevel menu node
                MenuNode mNode = new MenuNode();
                mNode.Id = GetValue(elMNode, "EntryNo");
                mNode.DisplayOrder = GetDisplayOrderAttributeValue(elMNode);// DisplayOrder not in xml, using EntryNo 
                mNode.Description = GetValue(elMNode, "Description");
                mNode.Image = new ImageView(GetValue(elMNode, "ImageID"));
                mNode.ValidationTimeWithinBounds = ConvertTo.SafeBoolean(GetValue(elMNode, "TimeWithinBounds"));
                mNode.ValidationEndTimeAfterMidnight = ConvertTo.SafeBoolean(GetValue(elMNode, "EndTimeAfterMidnight"));
                mNode.ValidationStartTime = ConvertTo.SafeTime(GetValue(elMNode, "StartTime"), true);
                mNode.ValidationEndTime = ConvertTo.SafeTime(GetValue(elMNode, "EndTime"), true);
                //check if StartTime or EndTime are 0:00
                if (mNode.ValidationStartTime.TimeOfDay.TotalSeconds == 0.0 && mNode.ValidationEndTime.TimeOfDay.TotalSeconds == 0.0)
                {
                    mNode.ValidationEndTime = mNode.ValidationEndTime.AddHours(23).AddMinutes(59).AddSeconds(59);
                    mNode.ValidationTimeWithinBounds = true;
                }
                if (mNode.ValidationEndTimeAfterMidnight)
                    mNode.ValidationEndTime = mNode.ValidationEndTime.AddDays(1);
                mNode.PriceGroup = GetValue(elMNode, "EntryPriceGroup");

                string entryType = GetValue(elMNode, "EntryType").ToUpper();
                mNode.NodeIsItem = true; //is a item node

                if (entryType == "OFFER")
                {
                    string offerId = GetValue(elMNode, "EntryID");
                    XElement elOffer = FindOffer(offerId);
                    MenuNodeLine mnLine = new MenuNodeLine();
                    mnLine.Id = offerId; // this is the OfferId
                    mnLine.NodeLineType = NodeLineType.Deal;
                    mNode.MenuNodeLines.Add(mnLine);
                }
                else
                {
                    string itemId = GetValue(elMNode, "EntryID");
                    XElement elItem = FindItem(itemId);
                    if (elItem != null)
                    {
                        MenuNodeLine mnLine = new MenuNodeLine();
                        mnLine.Id = GetValue(elItem, "ItemId");

                        //check itemType is correct, ALWAYS expect to find itemType of Item (or product) here
                        string itemType = FindItemType(mnLine.Id).ToUpper();
                        if (entryType == "RECIPE" && itemType == "RECIPE")
                        {
                            mnLine.NodeLineType = NodeLineType.Recipe;
                            //logger.Error("ERROR XML PARSE:Items.ItemType=Recipe for DynamicContent.EntryType=Item. Continuing... ItemId:{0} - {1}", 
                            //    mnLine.Id, elMNode.ToString());
                        }
                        // mnLine.NodeLineType = NodeLineType.Recipe;
                        else if (entryType == "ITEM" && (itemType == "PRODUCT" || itemType == "ITEM"))
                            mnLine.NodeLineType = NodeLineType.Product;
                        // mnLine.NodeLineType = NodeLineType.Recipe;
                        else
                        {
                            if (entryType.ToLower().Contains("offer") == false)
                                logger.Info("NOTE XML PARSE: No menunode item type found for id:{0} - {1}", mnLine.Id, elMNode.ToString());
                        }
                        //
                        mNode.MenuNodeLines.Add(mnLine);
                    }
                }
                menu.MenuNodes.Add(mNode);
            }
        }

        private void ParseMenuNodes(Menu menu,string dynamicContentId)
        {
            ParseMenuItemNodes(menu,dynamicContentId);

            //find the level=1 which are the menuNodes
            //ParentId = 0 here too. EntryType must be blank
            IEnumerable<XElement> elMenuNodes = null;
            if (string.IsNullOrWhiteSpace(dynamicContentId))
            {
                //when only one menu was supported in NAV
                elMenuNodes = from el in elBody.Elements("DynamicContent")
                              where el.Element("Level").Value == "1" && el.Element("EntryType").Value == ""
                select el;
            }
            else
            {
                elMenuNodes = from el in elBody.Elements("DynamicContent")
                              where el.Element("DynamicContentId").Value == dynamicContentId && el.Element("Level").Value == "1" 
                select el;
            }

            foreach (XElement elMNode in elMenuNodes)
            {
                //this is the toplevel menu node
                MenuNode mNode = new MenuNode();
                mNode.Id = GetValue(elMNode, "EntryNo");
                mNode.DisplayOrder = GetDisplayOrderAttributeValue(elMNode);// DisplayOrder not in xml, using EntryNo 
                mNode.Description = GetValue(elMNode, "Description");
                mNode.Image = new ImageView(GetValue(elMNode, "ImageID"));
                mNode.ValidationTimeWithinBounds = ConvertTo.SafeBoolean(GetValue(elMNode, "TimeWithinBounds"));
                mNode.ValidationEndTimeAfterMidnight = ConvertTo.SafeBoolean(GetValue(elMNode, "EndTimeAfterMidnight"));
                mNode.ValidationStartTime = ConvertTo.SafeTime(GetValue(elMNode, "StartTime"), true);
                mNode.ValidationEndTime = ConvertTo.SafeTime(GetValue(elMNode, "EndTime"), true);
                //check if StartTime or EndTime are 0:00
                if (mNode.ValidationStartTime.TimeOfDay.TotalSeconds == 0.0 && mNode.ValidationEndTime.TimeOfDay.TotalSeconds == 0.0)
                {
                    mNode.ValidationEndTime = mNode.ValidationEndTime.AddHours(23).AddMinutes(59).AddSeconds(59);
                    mNode.ValidationTimeWithinBounds = true;
                }
                if (mNode.ValidationEndTimeAfterMidnight)
                    mNode.ValidationEndTime = mNode.ValidationEndTime.AddDays(1);
                mNode.PriceGroup = GetValue(elMNode, "EntryPriceGroup");

                string entryType = GetValue(elMNode, "EntryType");

                for (int i = 2; i <= 10; i++)
                {
                    GetMenuNodeLines(i, mNode, dynamicContentId);
                    GetMenuGroupNode(i, entryType, mNode, dynamicContentId);//recursive !
                }
                menu.MenuNodes.Add(mNode);
            }
        }

        private List<UnitOfMeasure> FindUOMs(string parentId, string itemId)
        {
            try
            {
                List<UnitOfMeasure> list = new List<UnitOfMeasure>();
                IEnumerable<XElement> elUOMs =
                    from el in elBody.Elements("ItemUnitsOfMeasure")
                    where el.Element("ParentId").Value == parentId && el.Element("ItemId").Value == itemId
                    select el;

                foreach (XElement elUom in elUOMs)
                {
                    UnitOfMeasure uom = new UnitOfMeasure();
                    uom.Description = GetValue(elUom, "Description");
                    uom.Id = GetValue(elUom, "ItemUoM");
                    uom.Price = FindItemPrice(parentId, uom.Id);
                    list.Add(uom);
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new XmlException("MobileMenu Items Item Descendants parentId FindUOMs not found in xml. parentId:" + parentId, ex);
            }
        }

        private decimal FindItemPrice(string parentId, string uom)
        {
            try
            {
                IEnumerable<XElement> elItems = elBody.Elements("ItemPrice");
                XElement item = (from el in elItems
                                 where el.Element("ParentId").Value == parentId && el.Element("ItemUoM").Value == uom
                                 select el).FirstOrDefault();

                if (item == null)
                    return 0M;
                else
                    return ConvertTo.SafeDecimal(GetValue(item, "ItemPrice", "0"));  //can return Item or Recipe 
            }
            catch (Exception ex)
            {
                throw new XmlException("MobileMenu Items Item Descendants parentId FindItemPrice not found in xml. Id:" + parentId, ex);
            }
        }

        private decimal FindPriceByItemId(string itemId, string uom)
        {
            try
            {
                IEnumerable<XElement> elItems = elBody.Elements("ItemPrice");
                XElement item = (from el in elItems
                                 where el.Element("ItemId").Value == itemId && el.Element("ItemUoM").Value == uom
                                 select el).FirstOrDefault();

                if (item != null)
                    return ConvertTo.SafeDecimal(GetValue(item, "ItemPrice", "0"));  // 
                else
                {
                    //not found in ItemPrice table, look for price in Item table
                    elItems = elBody.Descendants("Items");
                    item = (from el in elItems
                            where el.Element("ItemId").Value == itemId
                            select el).FirstOrDefault();
                    if (item != null)
                        return ConvertTo.SafeDecimal(GetValue(item, "ItemPrice", "0"));  // 
                    else
                        return 0M;
                }

            }
            catch (Exception ex)
            {
                throw new XmlException("MobileMenu Items Item Descendants itemId FindPriceByItemId not found in xml. Id:" + itemId, ex);
            }
        }

        private XElement FindOffer(string offerId)
        {
            try
            {
                IEnumerable<XElement> elItems = elBody.Descendants("Offers");
                XElement offer = (from xml2 in elItems
                                  where xml2.Element("OfferId").Value == offerId
                                  select xml2).FirstOrDefault();
                return offer;
            }
            catch (Exception ex)
            {
                throw new XmlException("MobileMenu Offers Offer Descendants offerId not found in xml. offerId:" + offerId, ex);
            }
        }

        private string FindItemType(string id)
        {
            try
            {
                IEnumerable<XElement> elItems = elBody.Descendants("Items");
                XElement item = (from xml2 in elItems
                                 where xml2.Element("ItemId").Value == id
                                 select xml2).FirstOrDefault();
                if (item == null)
                    return "";
                else
                    return GetValue(item, "ItemType");  //can return Item or Recipe 
            }
            catch (Exception ex)
            {
                throw new XmlException("MobileMenu Items Item Descendants Id not found in xml. Id:" + id, ex);
            }
        }

        private XElement FindItem(string itemId)
        {
            try
            {
                IEnumerable<XElement> elItems = elBody.Descendants("Items");
                XElement item = (from xml2 in elItems
                                 where xml2.Element("ItemId").Value == itemId
                                 select xml2).FirstOrDefault();
                return item;
            }
            catch (Exception ex)
            {
                throw new XmlException("MobileMenu Items Item Descendants itemId not found in xml. itemId:" + itemId, ex);
            }
        }

        private bool FindItemModGroupRequired(string id)
        {
            try
            {
                IEnumerable<XElement> elItems = elBody.Descendants("ItemModifierGroups");
                XElement item = (from xml in elItems
                                 where xml.Element("ItemModGroupId").Value == id
                                 select xml).FirstOrDefault();
                if (item == null)
                    return false;
                else
                {
                    string modReq = GetValue(item, "ItemModRequired");  //can return Item or Recipe 
                    return ToBool(modReq);
                }
            }
            catch (Exception ex)
            {
                throw new XmlException("MobileMenu ItemModifierGroups ItemModGroupId Descendants Id not found in xml. Id:" + id, ex);
            }
        }

        private string FindItemItByItemModGroupId(string id)
        {
            try
            {
                IEnumerable<XElement> elItems = elBody.Descendants("ItemModifierGroups");
                XElement item = (from xml in elItems
                                 where xml.Element("ItemModGroupId").Value == id
                                 select xml).FirstOrDefault();
                if (item == null)
                    return "";
                else
                {
                    return GetValue(item, "ItemId");  //can return Item or Recipe 
                }
            }
            catch (Exception ex)
            {
                throw new XmlException("MobileMenu ItemModifierGroups ItemModGroupId Descendants Id not found in xml. Id:" + id, ex);
            }
        }

        private bool FindItemModRequired(string id)
        {
            try
            {
                IEnumerable<XElement> elItems = elBody.Descendants("Items");
                XElement item = (from xml2 in elItems
                                 where xml2.Element("ItemId").Value == id
                                 select xml2).FirstOrDefault();
                if (item == null)
                    return false;
                else
                {
                    string modReq = GetValue(item, "ItemModRequired");  //can return Item or Recipe 
                    return ToBool(modReq);
                }
            }
            catch (Exception ex)
            {
                throw new XmlException("MobileMenu ItemModifierGroups ItemModGroupId Descendants Id not found in xml. Id:" + id, ex);
            }
        }

        private int GetDisplayOrderAttributeValue(XElement elIn)
        {
            //if displayorder not found, simply return zero
            string name = "EntryNo";
            string val = "0";
            if (elIn.Element(name) != null)
            {
                val = elIn.Element(name).Value;
                if (string.IsNullOrWhiteSpace(val))
                    val = "0";
            }
            return Convert.ToInt32(val);
        }

        private string GetValue(XElement elIn, string name, string defaultValue = "")
        {
            //string text = (string)elIn.Element(name) ?? ""; //if name not found returns blank. But I want a good error msg
            if (elIn.Element(name) == null)
                throw new XmlException(name + " node not found in xml " + elIn.ToString());
            string val = elIn.Element(name).Value;
            if (string.IsNullOrWhiteSpace(defaultValue) == false && string.IsNullOrWhiteSpace(val))
                val = defaultValue;
            return val.Trim();
        }

        private string GetValueOrDefault(XElement elIn, string name, string defaultValue = "")
        {
            //never throws an error, even if name not found
            if (elIn.Element(name) != null)
            {
                string val = elIn.Element(name).Value;
                if (string.IsNullOrWhiteSpace(defaultValue) == false && string.IsNullOrWhiteSpace(val))
                    val = defaultValue;
                return val.Trim();
            }
            else
                return defaultValue;
        }

        private void ValidateXmlDoc()
        {
            //minimum xml validated
            XElement elResponse = doc.Element("Response");
            if (elResponse == null)
                throw new XmlException("Response node not found in xml ");
            if (elResponse.Element("Response_Body") == null)
                throw new XmlException("Response Response_Body node not found in xml ");
            if (elResponse.Element("Response_Body").Element("DynamicContent") == null)
                throw new XmlException("Response Response_Body DynamicContent node not found in xml ");

        }
    }
}

/*
<Response>
  <Request_ID>GET_DYN_CONT_HMP_MENUS</Request_ID>
  <Response_Code>0000</Response_Code>
  <Response_Text></Response_Text>
  <Response_Body>
    <Dynamic_Content_Id>HOSP13</Dynamic_Content_Id>
    <Description>Hospitality mobile menu</Description>
    <DynamicContent>
      <EntryNo>1</EntryNo>
      <ParentId>0</ParentId>
      <Level>1</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType />
      <EntryID />
      <Description>Super hot Pizza</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>4</EntryNo>
      <ParentId>0</ParentId>
      <Level>1</Level>
      <StartTime>11:30</StartTime>
      <EndTime>15:00</EndTime>
      <TimeWithinBounds>Yes</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup>LUNCH</EntryPriceGroup>
      <EntryType />
      <EntryID />
      <Description>Lunch - main courses</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>6</EntryNo>
      <ParentId>0</ParentId>
      <Level>1</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType />
      <EntryID />
      <Description>Food Menu</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>19</EntryNo>
      <ParentId>0</ParentId>
      <Level>1</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType />
      <EntryID />
      <Description>Crazy Wacky Offers</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>48</EntryNo>
      <ParentId>0</ParentId>
      <Level>1</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>1000</EntryID>
      <Description>Bicycle...</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>2</EntryNo>
      <ParentId>1</ParentId>
      <Level>2</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>R0009</EntryID>
      <Description>Hawaiian Pizza 9"</Description>
      <ImageID>32060</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>3</EntryNo>
      <ParentId>1</ParentId>
      <Level>2</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>R0010</EntryID>
      <Description>Hawaiian Pizza 12"</Description>
      <ImageID>32060</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>5</EntryNo>
      <ParentId>4</ParentId>
      <Level>2</Level>
      <StartTime />
      <EndTime />
      <TimeWithinBounds />
      <EndTimeAfterMidnight />
      <EntryPriceGroup>LUNCH</EntryPriceGroup>
      <EntryType>Item</EntryType>
      <EntryID>R0017</EntryID>
      <Description>Beef Stew</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>7</EntryNo>
      <ParentId>6</ParentId>
      <Level>2</Level>
      <StartTime />
      <EndTime />
      <TimeWithinBounds />
      <EndTimeAfterMidnight />
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>R0001</EntryID>
      <Description>Chicken w/brown sauce</Description>
      <ImageID>R0001</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>8</EntryNo>
      <ParentId>6</ParentId>
      <Level>2</Level>
      <StartTime />
      <EndTime />
      <TimeWithinBounds />
      <EndTimeAfterMidnight />
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>R0006</EntryID>
      <Description>Broccoli Chicken</Description>
      <ImageID>R0006</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>9</EntryNo>
      <ParentId>6</ParentId>
      <Level>2</Level>
      <StartTime />
      <EndTime />
      <TimeWithinBounds />
      <EndTimeAfterMidnight />
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>32020</EntryID>
      <Description>Club Sandwich</Description>
      <ImageID>32020</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>10</EntryNo>
      <ParentId>6</ParentId>
      <Level>2</Level>
      <StartTime />
      <EndTime />
      <TimeWithinBounds />
      <EndTimeAfterMidnight />
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>32030</EntryID>
      <Description>Grilled Chicken</Description>
      <ImageID>32030</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>11</EntryNo>
      <ParentId>6</ParentId>
      <Level>2</Level>
      <StartTime />
      <EndTime />
      <TimeWithinBounds />
      <EndTimeAfterMidnight />
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>32040</EntryID>
      <Description>Buffalo Wings</Description>
      <ImageID>32040</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>12</EntryNo>
      <ParentId>6</ParentId>
      <Level>2</Level>
      <StartTime />
      <EndTime />
      <TimeWithinBounds />
      <EndTimeAfterMidnight />
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>32080</EntryID>
      <Description>Tiger Shrimps</Description>
      <ImageID>32080</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>13</EntryNo>
      <ParentId>6</ParentId>
      <Level>2</Level>
      <StartTime />
      <EndTime />
      <TimeWithinBounds />
      <EndTimeAfterMidnight />
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>32090</EntryID>
      <Description>Grilled Salmon</Description>
      <ImageID>32090</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>14</EntryNo>
      <ParentId>6</ParentId>
      <Level>2</Level>
      <StartTime />
      <EndTime />
      <TimeWithinBounds />
      <EndTimeAfterMidnight />
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>32100</EntryID>
      <Description>Grilled Tuna Steak</Description>
      <ImageID>32100</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>15</EntryNo>
      <ParentId>6</ParentId>
      <Level>2</Level>
      <StartTime />
      <EndTime />
      <TimeWithinBounds />
      <EndTimeAfterMidnight />
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>32110</EntryID>
      <Description>Steak - Tenderloin</Description>
      <ImageID>32110</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>16</EntryNo>
      <ParentId>6</ParentId>
      <Level>2</Level>
      <StartTime />
      <EndTime />
      <TimeWithinBounds />
      <EndTimeAfterMidnight />
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>32120</EntryID>
      <Description>T-Bone</Description>
      <ImageID>32120</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>17</EntryNo>
      <ParentId>6</ParentId>
      <Level>2</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>30000</EntryID>
      <Description>Soda Orange 0.5 L</Description>
      <ImageID>30000</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>18</EntryNo>
      <ParentId>6</ParentId>
      <Level>2</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>30010</EntryID>
      <Description>Soda Lime 0.5 L</Description>
      <ImageID>30010</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>20</EntryNo>
      <ParentId>19</ParentId>
      <Level>2</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>Yes</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Published Offer</EntryType>
      <EntryID>S10006</EntryID>
      <Description>Club Sandw. Onion Rings Salad</Description>
      <ImageID>D06</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>21</EntryNo>
      <ParentId>19</ParentId>
      <Level>2</Level>
      <StartTime> 7:00</StartTime>
      <EndTime>10:00</EndTime>
      <TimeWithinBounds>Yes</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Published Offer</EntryType>
      <EntryID>S10011</EntryID>
      <Description>Coffee and Cake</Description>
      <ImageID>PUBOFF-COFFEECA</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>22</EntryNo>
      <ParentId>19</ParentId>
      <Level>2</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>Yes</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Published Offer</EntryType>
      <EntryID>S10012</EntryID>
      <Description>BBQ Burger - Soda and Dessert</Description>
      <ImageID>D12</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>23</EntryNo>
      <ParentId>19</ParentId>
      <Level>2</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType />
      <EntryID />
      <Description>Level 3 under here</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>24</EntryNo>
      <ParentId>23</ParentId>
      <Level>3</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>10000</EntryID>
      <Description>Milk 1 Liter</Description>
      <ImageID>10000</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>25</EntryNo>
      <ParentId>23</ParentId>
      <Level>3</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>10010</EntryID>
      <Description>Milk 2 Liters</Description>
      <ImageID>10010</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>26</EntryNo>
      <ParentId>23</ParentId>
      <Level>3</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType />
      <EntryID />
      <Description>Level 4 under here</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>27</EntryNo>
      <ParentId>26</ParentId>
      <Level>4</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>10020</EntryID>
      <Description>Milk 0.25 L</Description>
      <ImageID>10020</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>28</EntryNo>
      <ParentId>26</ParentId>
      <Level>4</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>10030</EntryID>
      <Description>Skim Milk 1 L</Description>
      <ImageID>10030</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>29</EntryNo>
      <ParentId>26</ParentId>
      <Level>4</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType />
      <EntryID />
      <Description>Level 5 under here</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>30</EntryNo>
      <ParentId>29</ParentId>
      <Level>5</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>20000</EntryID>
      <Description>Apple, Red Delicious</Description>
      <ImageID>20000</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>31</EntryNo>
      <ParentId>29</ParentId>
      <Level>5</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>20010</EntryID>
      <Description>Apple, Jonagold</Description>
      <ImageID>20010</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>32</EntryNo>
      <ParentId>29</ParentId>
      <Level>5</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType />
      <EntryID />
      <Description>Level 6 under here</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>33</EntryNo>
      <ParentId>32</ParentId>
      <Level>6</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>30000</EntryID>
      <Description>Soda Orange 0.5 L</Description>
      <ImageID>30000</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>34</EntryNo>
      <ParentId>32</ParentId>
      <Level>6</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>30010</EntryID>
      <Description>Soda Lime 0.5 L</Description>
      <ImageID>30010</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>35</EntryNo>
      <ParentId>32</ParentId>
      <Level>6</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType />
      <EntryID />
      <Description>Level 7 under here</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>37</EntryNo>
      <ParentId>35</ParentId>
      <Level>7</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>40010</EntryID>
      <Description>Towel Linda Beach</Description>
      <ImageID>40010</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>38</EntryNo>
      <ParentId>35</ParentId>
      <Level>7</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType />
      <EntryID />
      <Description>Level 8 under here</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>36</EntryNo>
      <ParentId>0</ParentId>
      <Level>8</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>10000</EntryID>
      <Description>Milk 1 Liter</Description>
      <ImageID>10000</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>39</EntryNo>
      <ParentId>38</ParentId>
      <Level>8</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>50000</EntryID>
      <Description>Briefcase, Leather</Description>
      <ImageID>50000</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>41</EntryNo>
      <ParentId>38</ParentId>
      <Level>8</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>50010</EntryID>
      <Description>Briefcase, Vinyl</Description>
      <ImageID>50010</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>42</EntryNo>
      <ParentId>38</ParentId>
      <Level>8</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType />
      <EntryID />
      <Description>Level 9 under here</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>40</EntryNo>
      <ParentId>39</ParentId>
      <Level>9</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>20000</EntryID>
      <Description>Apple, Red Delicious</Description>
      <ImageID>20000</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>43</EntryNo>
      <ParentId>42</ParentId>
      <Level>9</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>60000</EntryID>
      <Description>MP3 Player - 16 Gb</Description>
      <ImageID>60000</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>44</EntryNo>
      <ParentId>42</ParentId>
      <Level>9</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>60010</EntryID>
      <Description>MP3 Player - 64 Gb Silver</Description>
      <ImageID>60010</ImageID>
    </DynamicContent>
    <DynamicContent>
      <EntryNo>45</EntryNo>
      <ParentId>42</ParentId>
      <Level>9</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType />
      <EntryID />
      <Description>Level 10 under here</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>46</EntryNo>
      <ParentId>45</ParentId>
      <Level>10</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>70000</EntryID>
      <Description>Side Panel</Description>
      <ImageID />
    </DynamicContent>
    <DynamicContent>
      <EntryNo>47</EntryNo>
      <ParentId>45</ParentId>
      <Level>10</Level>
      <StartTime> 0:00</StartTime>
      <EndTime> 0:00</EndTime>
      <TimeWithinBounds>No</TimeWithinBounds>
      <EndTimeAfterMidnight>No</EndTimeAfterMidnight>
      <EntryPriceGroup />
      <EntryType>Item</EntryType>
      <EntryID>70010</EntryID>
      <Description>Wooden Door</Description>
      <ImageID />
    </DynamicContent>
    <Items>
      <EntryNo>49</EntryNo>
      <Level>1</Level>
      <ItemId>1000</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Bicycle...</ItemDescription>
      <ItemPrice>124.9875</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID />
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>50</EntryNo>
      <Level>1</Level>
      <ItemId>10000</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Milk 1 Liter</ItemDescription>
      <ItemPrice>1.144</ItemPrice>
      <ItemUoM>BOTTLE</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>10000</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>53</EntryNo>
      <Level>1</Level>
      <ItemId>10010</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Milk 2 Liters</ItemDescription>
      <ItemPrice>1.64</ItemPrice>
      <ItemUoM>CARTON</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>10010</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>55</EntryNo>
      <Level>1</Level>
      <ItemId>10020</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Milk 0.25 L</ItemDescription>
      <ItemPrice>0.599995</ItemPrice>
      <ItemUoM>CARTON</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>10020</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>57</EntryNo>
      <Level>1</Level>
      <ItemId>10030</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Skim Milk 1 L</ItemDescription>
      <ItemPrice>1.21</ItemPrice>
      <ItemUoM>BOTTLE</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>10030</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>59</EntryNo>
      <Level>1</Level>
      <ItemId>10060</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Cheddar Cheese</ItemDescription>
      <ItemPrice>2.2</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>10060</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>61</EntryNo>
      <Level>1</Level>
      <ItemId>10100</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Butter</ItemDescription>
      <ItemPrice>3.08</ItemPrice>
      <ItemUoM>BOX</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>10100</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>63</EntryNo>
      <Level>1</Level>
      <ItemId>20000</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Apple, Red Delicious</ItemDescription>
      <ItemPrice>6.06</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20000</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>66</EntryNo>
      <Level>1</Level>
      <ItemId>20010</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Apple, Jonagold</ItemDescription>
      <ItemPrice>4</ItemPrice>
      <ItemUoM>BOX</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20010</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>68</EntryNo>
      <Level>1</Level>
      <ItemId>20050</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Carrots</ItemDescription>
      <ItemPrice>0.98</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20050</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>70</EntryNo>
      <Level>1</Level>
      <ItemId>20061</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Broccoli</ItemDescription>
      <ItemPrice>2.5</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20061</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>73</EntryNo>
      <Level>1</Level>
      <ItemId>20062</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Onion</ItemDescription>
      <ItemPrice>0.75</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20062</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>75</EntryNo>
      <Level>1</Level>
      <ItemId>20063</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Pepper</ItemDescription>
      <ItemPrice>12.75</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20063</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>77</EntryNo>
      <Level>1</Level>
      <ItemId>20064</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Mushroom</ItemDescription>
      <ItemPrice>3.25</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20064</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>79</EntryNo>
      <Level>1</Level>
      <ItemId>20065</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Corn</ItemDescription>
      <ItemPrice>2.25</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20065</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>81</EntryNo>
      <Level>1</Level>
      <ItemId>20066</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Zucchini</ItemDescription>
      <ItemPrice>2</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20066</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>83</EntryNo>
      <Level>1</Level>
      <ItemId>20069</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Pineapple</ItemDescription>
      <ItemPrice>3.74</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20069</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>85</EntryNo>
      <Level>1</Level>
      <ItemId>20070</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Tomato</ItemDescription>
      <ItemPrice>1.5</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20070</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>87</EntryNo>
      <Level>1</Level>
      <ItemId>20076</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Plain Crust</ItemDescription>
      <ItemPrice>1.32</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20076</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>89</EntryNo>
      <Level>1</Level>
      <ItemId>20077</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Wild Rice</ItemDescription>
      <ItemPrice>4.2</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20077</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>91</EntryNo>
      <Level>1</Level>
      <ItemId>20078</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Plain Rice</ItemDescription>
      <ItemPrice>2</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>20078</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>93</EntryNo>
      <Level>1</Level>
      <ItemId>30000</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Soda Orange 0.5 L</ItemDescription>
      <ItemPrice>0.98</ItemPrice>
      <ItemUoM>BOTTLE</ItemUoM>
      <ItemDefaultMenuType>4</ItemDefaultMenuType>
      <ImageID>30000</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>101</EntryNo>
      <Level>1</Level>
      <ItemId>30010</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Soda Lime 0.5 L</ItemDescription>
      <ItemPrice>0.98</ItemPrice>
      <ItemUoM>BOTTLE</ItemUoM>
      <ItemDefaultMenuType>4</ItemDefaultMenuType>
      <ImageID>30010</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>105</EntryNo>
      <Level>1</Level>
      <ItemId>30085</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Soda</ItemDescription>
      <ItemPrice>0.8</ItemPrice>
      <ItemUoM>CUP</ItemUoM>
      <ItemDefaultMenuType>4</ItemDefaultMenuType>
      <ImageID>30085</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>110</EntryNo>
      <Level>1</Level>
      <ItemId>30090</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Coffee</ItemDescription>
      <ItemPrice>0.8</ItemPrice>
      <ItemUoM>CUP</ItemUoM>
      <ItemDefaultMenuType>4</ItemDefaultMenuType>
      <ImageID>30090</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>115</EntryNo>
      <Level>1</Level>
      <ItemId>30093</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Cafe Latte</ItemDescription>
      <ItemPrice>1.25</ItemPrice>
      <ItemUoM>CUP</ItemUoM>
      <ItemDefaultMenuType>4</ItemDefaultMenuType>
      <ImageID>30093</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>120</EntryNo>
      <Level>1</Level>
      <ItemId>30094</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Espresso</ItemDescription>
      <ItemPrice>1.1</ItemPrice>
      <ItemUoM>CUP</ItemUoM>
      <ItemDefaultMenuType>4</ItemDefaultMenuType>
      <ImageID>30094</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>124</EntryNo>
      <Level>1</Level>
      <ItemId>32000</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>BBQ Burger</ItemDescription>
      <ItemPrice>4.99</ItemPrice>
      <ItemUoM />
      <ItemDefaultMenuType>2</ItemDefaultMenuType>
      <ImageID>32000</ImageID>
      <ItemModRequired>Yes</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>127</EntryNo>
      <Level>1</Level>
      <ItemId>32020</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Club Sandwich</ItemDescription>
      <ItemPrice>3.5</ItemPrice>
      <ItemUoM />
      <ItemDefaultMenuType>2</ItemDefaultMenuType>
      <ImageID>32020</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>129</EntryNo>
      <Level>1</Level>
      <ItemId>32030</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Grilled Chicken</ItemDescription>
      <ItemPrice>8.6</ItemPrice>
      <ItemUoM />
      <ItemDefaultMenuType>2</ItemDefaultMenuType>
      <ImageID>32030</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>132</EntryNo>
      <Level>1</Level>
      <ItemId>32040</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Buffalo Wings</ItemDescription>
      <ItemPrice>4.5</ItemPrice>
      <ItemUoM />
      <ItemDefaultMenuType>2</ItemDefaultMenuType>
      <ImageID>32040</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>134</EntryNo>
      <Level>1</Level>
      <ItemId>32080</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Tiger Shrimps</ItemDescription>
      <ItemPrice>11</ItemPrice>
      <ItemUoM />
      <ItemDefaultMenuType>2</ItemDefaultMenuType>
      <ImageID>32080</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>136</EntryNo>
      <Level>1</Level>
      <ItemId>32090</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Grilled Salmon</ItemDescription>
      <ItemPrice>10.5</ItemPrice>
      <ItemUoM />
      <ItemDefaultMenuType>2</ItemDefaultMenuType>
      <ImageID>32090</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>138</EntryNo>
      <Level>1</Level>
      <ItemId>32100</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Grilled Tuna Steak</ItemDescription>
      <ItemPrice>12.99</ItemPrice>
      <ItemUoM />
      <ItemDefaultMenuType>2</ItemDefaultMenuType>
      <ImageID>32100</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>141</EntryNo>
      <Level>1</Level>
      <ItemId>32110</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Tenderloin</ItemDescription>
      <ItemPrice>11.5</ItemPrice>
      <ItemUoM />
      <ItemDefaultMenuType>2</ItemDefaultMenuType>
      <ImageID>32110</ImageID>
      <ItemModRequired>Yes</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>144</EntryNo>
      <Level>1</Level>
      <ItemId>32120</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>T-Bone</ItemDescription>
      <ItemPrice>11.5</ItemPrice>
      <ItemUoM />
      <ItemDefaultMenuType>2</ItemDefaultMenuType>
      <ImageID>32120</ImageID>
      <ItemModRequired>Yes</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>147</EntryNo>
      <Level>1</Level>
      <ItemId>32200</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Ham</ItemDescription>
      <ItemPrice>8.5</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>2</ItemDefaultMenuType>
      <ImageID>32200</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>149</EntryNo>
      <Level>1</Level>
      <ItemId>32201</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Pepperoni</ItemDescription>
      <ItemPrice>13.92</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>32201</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>151</EntryNo>
      <Level>1</Level>
      <ItemId>33000</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>French Fries</ItemDescription>
      <ItemPrice>2</ItemPrice>
      <ItemUoM>PACK</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>33000</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>153</EntryNo>
      <Level>1</Level>
      <ItemId>33010</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Baked Potato</ItemDescription>
      <ItemPrice>2</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>33010</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>155</EntryNo>
      <Level>1</Level>
      <ItemId>33020</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Mashed Potatos</ItemDescription>
      <ItemPrice>2</ItemPrice>
      <ItemUoM>PACK</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>33020</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>157</EntryNo>
      <Level>1</Level>
      <ItemId>33030</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Basmati Rice</ItemDescription>
      <ItemPrice>1.5</ItemPrice>
      <ItemUoM>PACK</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>33030</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>159</EntryNo>
      <Level>1</Level>
      <ItemId>33040</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Onion Rings</ItemDescription>
      <ItemPrice>1.5</ItemPrice>
      <ItemUoM>PACK</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>33040</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>161</EntryNo>
      <Level>1</Level>
      <ItemId>33050</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Salad</ItemDescription>
      <ItemPrice>1.5</ItemPrice>
      <ItemUoM>PACK</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>33050</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>164</EntryNo>
      <Level>1</Level>
      <ItemId>33100</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Croissant - Plain</ItemDescription>
      <ItemPrice>1.5</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>3</ItemDefaultMenuType>
      <ImageID>33100</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>167</EntryNo>
      <Level>1</Level>
      <ItemId>33110</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Croissant - Chocolate</ItemDescription>
      <ItemPrice>1.75</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>3</ItemDefaultMenuType>
      <ImageID>33110</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>170</EntryNo>
      <Level>1</Level>
      <ItemId>33115</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Ginger Muffin</ItemDescription>
      <ItemPrice>1.2</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>3</ItemDefaultMenuType>
      <ImageID>33115</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>173</EntryNo>
      <Level>1</Level>
      <ItemId>33116</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Blueberry Muffin</ItemDescription>
      <ItemPrice>1.2</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>3</ItemDefaultMenuType>
      <ImageID>33116</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>176</EntryNo>
      <Level>1</Level>
      <ItemId>33120</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Pecan Pie</ItemDescription>
      <ItemPrice>2.2</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemDefaultMenuType>3</ItemDefaultMenuType>
      <ImageID>33120</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>181</EntryNo>
      <Level>1</Level>
      <ItemId>33130</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Tiramisu</ItemDescription>
      <ItemPrice>2.2</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemDefaultMenuType>3</ItemDefaultMenuType>
      <ImageID>33130</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>186</EntryNo>
      <Level>1</Level>
      <ItemId>33140</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Apple Crisp</ItemDescription>
      <ItemPrice>2</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemDefaultMenuType>3</ItemDefaultMenuType>
      <ImageID>33140</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>191</EntryNo>
      <Level>1</Level>
      <ItemId>33150</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Icecream</ItemDescription>
      <ItemPrice>1.8</ItemPrice>
      <ItemUoM>PORTION</ItemUoM>
      <ItemDefaultMenuType>3</ItemDefaultMenuType>
      <ImageID>33150</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>195</EntryNo>
      <Level>1</Level>
      <ItemId>33160</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Carrot Cake</ItemDescription>
      <ItemPrice>1.75</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemDefaultMenuType>3</ItemDefaultMenuType>
      <ImageID>33160</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>199</EntryNo>
      <Level>1</Level>
      <ItemId>33170</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Chocolate Cake</ItemDescription>
      <ItemPrice>1.75</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemDefaultMenuType>3</ItemDefaultMenuType>
      <ImageID>33170</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>203</EntryNo>
      <Level>1</Level>
      <ItemId>34001</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Salt</ItemDescription>
      <ItemPrice>0.1</ItemPrice>
      <ItemUoM>GR</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>34001</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>205</EntryNo>
      <Level>1</Level>
      <ItemId>34002</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Flour</ItemDescription>
      <ItemPrice>0.1</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>34002</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>207</EntryNo>
      <Level>1</Level>
      <ItemId>34003</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Chicken</ItemDescription>
      <ItemPrice>4</ItemPrice>
      <ItemUoM>KG</ItemUoM>
      <ItemDefaultMenuType>2</ItemDefaultMenuType>
      <ImageID>34003</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>209</EntryNo>
      <Level>1</Level>
      <ItemId>34072</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Tomato Sauce</ItemDescription>
      <ItemPrice>0.5</ItemPrice>
      <ItemUoM>GR</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>34072</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>211</EntryNo>
      <Level>1</Level>
      <ItemId>40010</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Towel Linda Beach</ItemDescription>
      <ItemPrice>50</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>40010</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>214</EntryNo>
      <Level>1</Level>
      <ItemId>50000</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Briefcase, Leather</ItemDescription>
      <ItemPrice>100</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>50000</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>216</EntryNo>
      <Level>1</Level>
      <ItemId>50010</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Briefcase, Vinyl</ItemDescription>
      <ItemPrice>70</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>50010</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>218</EntryNo>
      <Level>1</Level>
      <ItemId>60000</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>MP3 Player - 16 Gb</ItemDescription>
      <ItemPrice>60</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>60000</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>220</EntryNo>
      <Level>1</Level>
      <ItemId>60010</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>MP3 Player - 64 Gb Silver</ItemDescription>
      <ItemPrice>110</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>60010</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>222</EntryNo>
      <Level>1</Level>
      <ItemId>70000</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Side Panel</ItemDescription>
      <ItemPrice>38.375</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID />
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>223</EntryNo>
      <Level>1</Level>
      <ItemId>70010</ItemId>
      <ItemType>Item</ItemType>
      <ItemDescription>Wooden Door</ItemDescription>
      <ItemPrice>65.125</ItemPrice>
      <ItemUoM>PCS</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID />
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>224</EntryNo>
      <Level>1</Level>
      <ItemId>R0001</ItemId>
      <ItemType>Recipe</ItemType>
      <ItemDescription>Chicken w/brown sauce</ItemDescription>
      <ItemPrice>11</ItemPrice>
      <ItemUoM>PORTION</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>R0001</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>232</EntryNo>
      <Level>1</Level>
      <ItemId>R0002</ItemId>
      <ItemType>Recipe</ItemType>
      <ItemDescription>Brown Sauce</ItemDescription>
      <ItemPrice>3.13</ItemPrice>
      <ItemUoM>PORTION</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>R0002</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>239</EntryNo>
      <Level>1</Level>
      <ItemId>R0006</ItemId>
      <ItemType>Recipe</ItemType>
      <ItemDescription>Broccoli Chicken</ItemDescription>
      <ItemPrice>15</ItemPrice>
      <ItemUoM>PORTION</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>R0006</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>246</EntryNo>
      <Level>1</Level>
      <ItemId>R0009</ItemId>
      <ItemType>Recipe</ItemType>
      <ItemDescription>Hawaiian Pizza 9"</ItemDescription>
      <ItemPrice>9.38</ItemPrice>
      <ItemUoM>9 INCH</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>32060</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>258</EntryNo>
      <Level>1</Level>
      <ItemId>R0010</ItemId>
      <ItemType>Recipe</ItemType>
      <ItemDescription>Hawaiian Pizza 12"</ItemDescription>
      <ItemPrice>9.5</ItemPrice>
      <ItemUoM>12 INCH</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID>32060</ImageID>
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Items>
      <EntryNo>269</EntryNo>
      <Level>1</Level>
      <ItemId>R0017</ItemId>
      <ItemType>Recipe</ItemType>
      <ItemDescription>Beef Stew</ItemDescription>
      <ItemPrice>10</ItemPrice>
      <ItemUoM>PORTION</ItemUoM>
      <ItemDefaultMenuType>0</ItemDefaultMenuType>
      <ImageID />
      <ItemModRequired>No</ItemModRequired>
    </Items>
    <Ingredients>
      <EntryNo>225</EntryNo>
      <ParentId>224</ParentId>
      <Level>2</Level>
      <ItemId>R0001</ItemId>
      <IngredientItemId>R0002</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>PORTION</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>226</EntryNo>
      <ParentId>224</ParentId>
      <Level>2</Level>
      <ItemId>R0001</ItemId>
      <IngredientItemId>20077</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>233</EntryNo>
      <ParentId>232</ParentId>
      <Level>2</Level>
      <ItemId>R0002</ItemId>
      <IngredientItemId>10000</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>234</EntryNo>
      <ParentId>232</ParentId>
      <Level>2</Level>
      <ItemId>R0002</ItemId>
      <IngredientItemId>34001</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>235</EntryNo>
      <ParentId>232</ParentId>
      <Level>2</Level>
      <ItemId>R0002</ItemId>
      <IngredientItemId>34002</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>236</EntryNo>
      <ParentId>232</ParentId>
      <Level>2</Level>
      <ItemId>R0002</ItemId>
      <IngredientItemId>10100</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>240</EntryNo>
      <ParentId>239</ParentId>
      <Level>2</Level>
      <ItemId>R0006</ItemId>
      <IngredientItemId>34001</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>241</EntryNo>
      <ParentId>239</ParentId>
      <Level>2</Level>
      <ItemId>R0006</ItemId>
      <IngredientItemId>20062</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>242</EntryNo>
      <ParentId>239</ParentId>
      <Level>2</Level>
      <ItemId>R0006</ItemId>
      <IngredientItemId>10060</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>247</EntryNo>
      <ParentId>246</ParentId>
      <Level>2</Level>
      <ItemId>R0009</ItemId>
      <IngredientItemId>20076</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>9 INCH</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>248</EntryNo>
      <ParentId>246</ParentId>
      <Level>2</Level>
      <ItemId>R0009</ItemId>
      <IngredientItemId>20069</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>249</EntryNo>
      <ParentId>246</ParentId>
      <Level>2</Level>
      <ItemId>R0009</ItemId>
      <IngredientItemId>32200</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>250</EntryNo>
      <ParentId>246</ParentId>
      <Level>2</Level>
      <ItemId>R0009</ItemId>
      <IngredientItemId>34072</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>251</EntryNo>
      <ParentId>246</ParentId>
      <Level>2</Level>
      <ItemId>R0009</ItemId>
      <IngredientItemId>10060</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>259</EntryNo>
      <ParentId>258</ParentId>
      <Level>2</Level>
      <ItemId>R0010</ItemId>
      <IngredientItemId>20076</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>12 INCH</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>260</EntryNo>
      <ParentId>258</ParentId>
      <Level>2</Level>
      <ItemId>R0010</ItemId>
      <IngredientItemId>20069</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>261</EntryNo>
      <ParentId>258</ParentId>
      <Level>2</Level>
      <ItemId>R0010</ItemId>
      <IngredientItemId>32200</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>262</EntryNo>
      <ParentId>258</ParentId>
      <Level>2</Level>
      <ItemId>R0010</ItemId>
      <IngredientItemId>34072</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>263</EntryNo>
      <ParentId>258</ParentId>
      <Level>2</Level>
      <ItemId>R0010</ItemId>
      <IngredientItemId>10060</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>GR</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>270</EntryNo>
      <ParentId>269</ParentId>
      <Level>2</Level>
      <ItemId>R0017</ItemId>
      <IngredientItemId>20062</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>KG</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>271</EntryNo>
      <ParentId>269</ParentId>
      <Level>2</Level>
      <ItemId>R0017</ItemId>
      <IngredientItemId>20064</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>KG</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>272</EntryNo>
      <ParentId>269</ParentId>
      <Level>2</Level>
      <ItemId>R0017</ItemId>
      <IngredientItemId>20050</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>KG</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>273</EntryNo>
      <ParentId>269</ParentId>
      <Level>2</Level>
      <ItemId>R0017</ItemId>
      <IngredientItemId>32110</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>KG</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <Ingredients>
      <EntryNo>274</EntryNo>
      <ParentId>269</ParentId>
      <Level>2</Level>
      <ItemId>R0017</ItemId>
      <IngredientItemId>R0002</IngredientItemId>
      <BOMQuantity>1</BOMQuantity>
      <BOMUoM>PORTION</BOMUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </Ingredients>
    <ItemUnitsOfMeasure>
      <EntryNo>94</EntryNo>
      <ParentId>93</ParentId>
      <Level>2</Level>
      <Description>Bottle</Description>
      <Order>101</Order>
      <ItemId>30000</ItemId>
      <ItemUoM>BOTTLE</ItemUoM>
    </ItemUnitsOfMeasure>
    <ItemUnitsOfMeasure>
      <EntryNo>95</EntryNo>
      <ParentId>93</ParentId>
      <Level>2</Level>
      <Description>Cup</Description>
      <Order>1</Order>
      <ItemId>30000</ItemId>
      <ItemUoM>CUP</ItemUoM>
    </ItemUnitsOfMeasure>
    <ItemPrice>
      <EntryNo>51</EntryNo>
      <ParentId>50</ParentId>
      <Level>2</Level>
      <ItemId>10000</ItemId>
      <ItemPrice>1.144</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>52</EntryNo>
      <ParentId>50</ParentId>
      <Level>2</Level>
      <ItemId>10000</ItemId>
      <ItemPrice>60.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>SETMENU</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>54</EntryNo>
      <ParentId>53</ParentId>
      <Level>2</Level>
      <ItemId>10010</ItemId>
      <ItemPrice>1.64</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>56</EntryNo>
      <ParentId>55</ParentId>
      <Level>2</Level>
      <ItemId>10020</ItemId>
      <ItemPrice>0.599995</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>58</EntryNo>
      <ParentId>57</ParentId>
      <Level>2</Level>
      <ItemId>10030</ItemId>
      <ItemPrice>1.21</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>60</EntryNo>
      <ParentId>59</ParentId>
      <Level>2</Level>
      <ItemId>10060</ItemId>
      <ItemPrice>2.2</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>62</EntryNo>
      <ParentId>61</ParentId>
      <Level>2</Level>
      <ItemId>10100</ItemId>
      <ItemPrice>3.08</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>64</EntryNo>
      <ParentId>63</ParentId>
      <Level>2</Level>
      <ItemId>20000</ItemId>
      <ItemPrice>6.06</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>65</EntryNo>
      <ParentId>63</ParentId>
      <Level>2</Level>
      <ItemId>20000</ItemId>
      <ItemPrice>4</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>LUNCH</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>67</EntryNo>
      <ParentId>66</ParentId>
      <Level>2</Level>
      <ItemId>20010</ItemId>
      <ItemPrice>4</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>69</EntryNo>
      <ParentId>68</ParentId>
      <Level>2</Level>
      <ItemId>20050</ItemId>
      <ItemPrice>0.98</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>71</EntryNo>
      <ParentId>70</ParentId>
      <Level>2</Level>
      <ItemId>20061</ItemId>
      <ItemPrice>2.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>72</EntryNo>
      <ParentId>70</ParentId>
      <Level>2</Level>
      <ItemId>20061</ItemId>
      <ItemPrice>8</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>CATLUNCH</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>74</EntryNo>
      <ParentId>73</ParentId>
      <Level>2</Level>
      <ItemId>20062</ItemId>
      <ItemPrice>0.75</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>76</EntryNo>
      <ParentId>75</ParentId>
      <Level>2</Level>
      <ItemId>20063</ItemId>
      <ItemPrice>12.75</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>78</EntryNo>
      <ParentId>77</ParentId>
      <Level>2</Level>
      <ItemId>20064</ItemId>
      <ItemPrice>3.25</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>80</EntryNo>
      <ParentId>79</ParentId>
      <Level>2</Level>
      <ItemId>20065</ItemId>
      <ItemPrice>2.25</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>82</EntryNo>
      <ParentId>81</ParentId>
      <Level>2</Level>
      <ItemId>20066</ItemId>
      <ItemPrice>2</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>84</EntryNo>
      <ParentId>83</ParentId>
      <Level>2</Level>
      <ItemId>20069</ItemId>
      <ItemPrice>3.74</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>86</EntryNo>
      <ParentId>85</ParentId>
      <Level>2</Level>
      <ItemId>20070</ItemId>
      <ItemPrice>1.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>88</EntryNo>
      <ParentId>87</ParentId>
      <Level>2</Level>
      <ItemId>20076</ItemId>
      <ItemPrice>1.32</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>90</EntryNo>
      <ParentId>89</ParentId>
      <Level>2</Level>
      <ItemId>20077</ItemId>
      <ItemPrice>4.2</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>92</EntryNo>
      <ParentId>91</ParentId>
      <Level>2</Level>
      <ItemId>20078</ItemId>
      <ItemPrice>2</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>96</EntryNo>
      <ParentId>93</ParentId>
      <Level>2</Level>
      <ItemId>30000</ItemId>
      <ItemPrice>0.98</ItemPrice>
      <ItemUoM>BOTTLE</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>97</EntryNo>
      <ParentId>93</ParentId>
      <Level>2</Level>
      <ItemId>30000</ItemId>
      <ItemPrice>0.89</ItemPrice>
      <ItemUoM>CAN</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>98</EntryNo>
      <ParentId>93</ParentId>
      <Level>2</Level>
      <ItemId>30000</ItemId>
      <ItemPrice>0.79</ItemPrice>
      <ItemUoM>CUP</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>102</EntryNo>
      <ParentId>101</ParentId>
      <Level>2</Level>
      <ItemId>30010</ItemId>
      <ItemPrice>0.98</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>106</EntryNo>
      <ParentId>105</ParentId>
      <Level>2</Level>
      <ItemId>30085</ItemId>
      <ItemPrice>0.8</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>107</EntryNo>
      <ParentId>105</ParentId>
      <Level>2</Level>
      <ItemId>30085</ItemId>
      <ItemPrice>1.2</ItemPrice>
      <ItemUoM>LAR CUP</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>108</EntryNo>
      <ParentId>105</ParentId>
      <Level>2</Level>
      <ItemId>30085</ItemId>
      <ItemPrice>1</ItemPrice>
      <ItemUoM>MED CUP</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>111</EntryNo>
      <ParentId>110</ParentId>
      <Level>2</Level>
      <ItemId>30090</ItemId>
      <ItemPrice>0.8</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>112</EntryNo>
      <ParentId>110</ParentId>
      <Level>2</Level>
      <ItemId>30090</ItemId>
      <ItemPrice>1.25</ItemPrice>
      <ItemUoM>LAR CUP</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>113</EntryNo>
      <ParentId>110</ParentId>
      <Level>2</Level>
      <ItemId>30090</ItemId>
      <ItemPrice>1</ItemPrice>
      <ItemUoM>MED CUP</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>116</EntryNo>
      <ParentId>115</ParentId>
      <Level>2</Level>
      <ItemId>30093</ItemId>
      <ItemPrice>1.25</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>117</EntryNo>
      <ParentId>115</ParentId>
      <Level>2</Level>
      <ItemId>30093</ItemId>
      <ItemPrice>1.8</ItemPrice>
      <ItemUoM>LAR CUP</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>118</EntryNo>
      <ParentId>115</ParentId>
      <Level>2</Level>
      <ItemId>30093</ItemId>
      <ItemPrice>1.5</ItemPrice>
      <ItemUoM>MED CUP</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>121</EntryNo>
      <ParentId>120</ParentId>
      <Level>2</Level>
      <ItemId>30094</ItemId>
      <ItemPrice>1.1</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>122</EntryNo>
      <ParentId>120</ParentId>
      <Level>2</Level>
      <ItemId>30094</ItemId>
      <ItemPrice>1.4</ItemPrice>
      <ItemUoM>MED CUP</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>125</EntryNo>
      <ParentId>124</ParentId>
      <Level>2</Level>
      <ItemId>32000</ItemId>
      <ItemPrice>4.99</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>128</EntryNo>
      <ParentId>127</ParentId>
      <Level>2</Level>
      <ItemId>32020</ItemId>
      <ItemPrice>3.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>130</EntryNo>
      <ParentId>129</ParentId>
      <Level>2</Level>
      <ItemId>32030</ItemId>
      <ItemPrice>8.6</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>133</EntryNo>
      <ParentId>132</ParentId>
      <Level>2</Level>
      <ItemId>32040</ItemId>
      <ItemPrice>4.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>135</EntryNo>
      <ParentId>134</ParentId>
      <Level>2</Level>
      <ItemId>32080</ItemId>
      <ItemPrice>11</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>137</EntryNo>
      <ParentId>136</ParentId>
      <Level>2</Level>
      <ItemId>32090</ItemId>
      <ItemPrice>10.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>139</EntryNo>
      <ParentId>138</ParentId>
      <Level>2</Level>
      <ItemId>32100</ItemId>
      <ItemPrice>12.99</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>140</EntryNo>
      <ParentId>138</ParentId>
      <Level>2</Level>
      <ItemId>32100</ItemId>
      <ItemPrice>8</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>SETMENU</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>142</EntryNo>
      <ParentId>141</ParentId>
      <Level>2</Level>
      <ItemId>32110</ItemId>
      <ItemPrice>11.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>145</EntryNo>
      <ParentId>144</ParentId>
      <Level>2</Level>
      <ItemId>32120</ItemId>
      <ItemPrice>11.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>148</EntryNo>
      <ParentId>147</ParentId>
      <Level>2</Level>
      <ItemId>32200</ItemId>
      <ItemPrice>8.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>150</EntryNo>
      <ParentId>149</ParentId>
      <Level>2</Level>
      <ItemId>32201</ItemId>
      <ItemPrice>13.92</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>152</EntryNo>
      <ParentId>151</ParentId>
      <Level>2</Level>
      <ItemId>33000</ItemId>
      <ItemPrice>2</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>154</EntryNo>
      <ParentId>153</ParentId>
      <Level>2</Level>
      <ItemId>33010</ItemId>
      <ItemPrice>2</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>156</EntryNo>
      <ParentId>155</ParentId>
      <Level>2</Level>
      <ItemId>33020</ItemId>
      <ItemPrice>2</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>158</EntryNo>
      <ParentId>157</ParentId>
      <Level>2</Level>
      <ItemId>33030</ItemId>
      <ItemPrice>1.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>160</EntryNo>
      <ParentId>159</ParentId>
      <Level>2</Level>
      <ItemId>33040</ItemId>
      <ItemPrice>1.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>162</EntryNo>
      <ParentId>161</ParentId>
      <Level>2</Level>
      <ItemId>33050</ItemId>
      <ItemPrice>1.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>163</EntryNo>
      <ParentId>161</ParentId>
      <Level>2</Level>
      <ItemId>33050</ItemId>
      <ItemPrice>2</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>SETMENU</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>165</EntryNo>
      <ParentId>164</ParentId>
      <Level>2</Level>
      <ItemId>33100</ItemId>
      <ItemPrice>1.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>168</EntryNo>
      <ParentId>167</ParentId>
      <Level>2</Level>
      <ItemId>33110</ItemId>
      <ItemPrice>1.75</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>171</EntryNo>
      <ParentId>170</ParentId>
      <Level>2</Level>
      <ItemId>33115</ItemId>
      <ItemPrice>1.2</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>174</EntryNo>
      <ParentId>173</ParentId>
      <Level>2</Level>
      <ItemId>33116</ItemId>
      <ItemPrice>1.2</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>177</EntryNo>
      <ParentId>176</ParentId>
      <Level>2</Level>
      <ItemId>33120</ItemId>
      <ItemPrice>9.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>178</EntryNo>
      <ParentId>176</ParentId>
      <Level>2</Level>
      <ItemId>33120</ItemId>
      <ItemPrice>2.2</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>179</EntryNo>
      <ParentId>176</ParentId>
      <Level>2</Level>
      <ItemId>33120</ItemId>
      <ItemPrice>2.5</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemPriceGr>SETMENU</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>182</EntryNo>
      <ParentId>181</ParentId>
      <Level>2</Level>
      <ItemId>33130</ItemId>
      <ItemPrice>10.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>183</EntryNo>
      <ParentId>181</ParentId>
      <Level>2</Level>
      <ItemId>33130</ItemId>
      <ItemPrice>2.2</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>184</EntryNo>
      <ParentId>181</ParentId>
      <Level>2</Level>
      <ItemId>33130</ItemId>
      <ItemPrice>2</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemPriceGr>SETMENU</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>187</EntryNo>
      <ParentId>186</ParentId>
      <Level>2</Level>
      <ItemId>33140</ItemId>
      <ItemPrice>8.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>188</EntryNo>
      <ParentId>186</ParentId>
      <Level>2</Level>
      <ItemId>33140</ItemId>
      <ItemPrice>2</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>189</EntryNo>
      <ParentId>186</ParentId>
      <Level>2</Level>
      <ItemId>33140</ItemId>
      <ItemPrice>2.5</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemPriceGr>SETMENU</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>192</EntryNo>
      <ParentId>191</ParentId>
      <Level>2</Level>
      <ItemId>33150</ItemId>
      <ItemPrice>12.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>193</EntryNo>
      <ParentId>191</ParentId>
      <Level>2</Level>
      <ItemId>33150</ItemId>
      <ItemPrice>1.8</ItemPrice>
      <ItemUoM>PORTION</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>196</EntryNo>
      <ParentId>195</ParentId>
      <Level>2</Level>
      <ItemId>33160</ItemId>
      <ItemPrice>7.9</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>197</EntryNo>
      <ParentId>195</ParentId>
      <Level>2</Level>
      <ItemId>33160</ItemId>
      <ItemPrice>1.75</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>200</EntryNo>
      <ParentId>199</ParentId>
      <Level>2</Level>
      <ItemId>33170</ItemId>
      <ItemPrice>7.9</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>201</EntryNo>
      <ParentId>199</ParentId>
      <Level>2</Level>
      <ItemId>33170</ItemId>
      <ItemPrice>1.75</ItemPrice>
      <ItemUoM>SLICE</ItemUoM>
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>204</EntryNo>
      <ParentId>203</ParentId>
      <Level>2</Level>
      <ItemId>34001</ItemId>
      <ItemPrice>0.1</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>206</EntryNo>
      <ParentId>205</ParentId>
      <Level>2</Level>
      <ItemId>34002</ItemId>
      <ItemPrice>0.1</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>208</EntryNo>
      <ParentId>207</ParentId>
      <Level>2</Level>
      <ItemId>34003</ItemId>
      <ItemPrice>4</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>210</EntryNo>
      <ParentId>209</ParentId>
      <Level>2</Level>
      <ItemId>34072</ItemId>
      <ItemPrice>0.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>212</EntryNo>
      <ParentId>211</ParentId>
      <Level>2</Level>
      <ItemId>40010</ItemId>
      <ItemPrice>50</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>215</EntryNo>
      <ParentId>214</ParentId>
      <Level>2</Level>
      <ItemId>50000</ItemId>
      <ItemPrice>100</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>217</EntryNo>
      <ParentId>216</ParentId>
      <Level>2</Level>
      <ItemId>50010</ItemId>
      <ItemPrice>70</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>219</EntryNo>
      <ParentId>218</ParentId>
      <Level>2</Level>
      <ItemId>60000</ItemId>
      <ItemPrice>60</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>221</EntryNo>
      <ParentId>220</ParentId>
      <Level>2</Level>
      <ItemId>60010</ItemId>
      <ItemPrice>110</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>227</EntryNo>
      <ParentId>224</ParentId>
      <Level>2</Level>
      <ItemId>R0001</ItemId>
      <ItemPrice>12.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>228</EntryNo>
      <ParentId>224</ParentId>
      <Level>2</Level>
      <ItemId>R0001</ItemId>
      <ItemPrice>11</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>229</EntryNo>
      <ParentId>224</ParentId>
      <Level>2</Level>
      <ItemId>R0001</ItemId>
      <ItemPrice>11</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>LUNCH</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>230</EntryNo>
      <ParentId>224</ParentId>
      <Level>2</Level>
      <ItemId>R0001</ItemId>
      <ItemPrice>8</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>SETMENU</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>237</EntryNo>
      <ParentId>232</ParentId>
      <Level>2</Level>
      <ItemId>R0002</ItemId>
      <ItemPrice>3.13</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>238</EntryNo>
      <ParentId>232</ParentId>
      <Level>2</Level>
      <ItemId>R0002</ItemId>
      <ItemPrice>6.5</ItemPrice>
      <ItemUoM>PORTION</ItemUoM>
      <ItemPriceGr>LUNCH</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>243</EntryNo>
      <ParentId>239</ParentId>
      <Level>2</Level>
      <ItemId>R0006</ItemId>
      <ItemPrice>15</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>244</EntryNo>
      <ParentId>239</ParentId>
      <Level>2</Level>
      <ItemId>R0006</ItemId>
      <ItemPrice>11</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>CATLUNCH</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>252</EntryNo>
      <ParentId>246</ParentId>
      <Level>2</Level>
      <ItemId>R0009</ItemId>
      <ItemPrice>9.38</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>253</EntryNo>
      <ParentId>246</ParentId>
      <Level>2</Level>
      <ItemId>R0009</ItemId>
      <ItemPrice>11</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>CATLUNCH</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>264</EntryNo>
      <ParentId>258</ParentId>
      <Level>2</Level>
      <ItemId>R0010</ItemId>
      <ItemPrice>9.5</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemPrice>
      <EntryNo>275</EntryNo>
      <ParentId>269</ParentId>
      <Level>2</Level>
      <ItemId>R0017</ItemId>
      <ItemPrice>10</ItemPrice>
      <ItemUoM />
      <ItemPriceGr>ALL</ItemPriceGr>
    </ItemPrice>
    <ItemModifierGroups>
      <EntryNo>99</EntryNo>
      <ParentId>93</ParentId>
      <Level>2</Level>
      <ItemId>30000</ItemId>
      <ItemModGroupId>SOFT-DRINK</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>100</EntryNo>
      <ParentId>93</ParentId>
      <Level>2</Level>
      <ItemId>30000</ItemId>
      <ItemModGroupId>TEST</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>103</EntryNo>
      <ParentId>101</ParentId>
      <Level>2</Level>
      <ItemId>30010</ItemId>
      <ItemModGroupId>SOFT-DRINK</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>104</EntryNo>
      <ParentId>101</ParentId>
      <Level>2</Level>
      <ItemId>30010</ItemId>
      <ItemModGroupId>TEST</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>109</EntryNo>
      <ParentId>105</ParentId>
      <Level>2</Level>
      <ItemId>30085</ItemId>
      <ItemModGroupId>TEST</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>114</EntryNo>
      <ParentId>110</ParentId>
      <Level>2</Level>
      <ItemId>30090</ItemId>
      <ItemModGroupId>TEST</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>119</EntryNo>
      <ParentId>115</ParentId>
      <Level>2</Level>
      <ItemId>30093</ItemId>
      <ItemModGroupId>TEST</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>123</EntryNo>
      <ParentId>120</ParentId>
      <Level>2</Level>
      <ItemId>30094</ItemId>
      <ItemModGroupId>TEST</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>126</EntryNo>
      <ParentId>124</ParentId>
      <Level>2</Level>
      <ItemId>32000</ItemId>
      <ItemModGroupId>BUGER-TIME</ItemModGroupId>
      <ItemModRequired>Yes</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>131</EntryNo>
      <ParentId>129</ParentId>
      <Level>2</Level>
      <ItemId>32030</ItemId>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>143</EntryNo>
      <ParentId>141</ParentId>
      <Level>2</Level>
      <ItemId>32110</ItemId>
      <ItemModGroupId>STEAK-TIME</ItemModGroupId>
      <ItemModRequired>Yes</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>146</EntryNo>
      <ParentId>144</ParentId>
      <Level>2</Level>
      <ItemId>32120</ItemId>
      <ItemModGroupId>STEAK-TIME</ItemModGroupId>
      <ItemModRequired>Yes</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>166</EntryNo>
      <ParentId>164</ParentId>
      <Level>2</Level>
      <ItemId>33100</ItemId>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>169</EntryNo>
      <ParentId>167</ParentId>
      <Level>2</Level>
      <ItemId>33110</ItemId>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>172</EntryNo>
      <ParentId>170</ParentId>
      <Level>2</Level>
      <ItemId>33115</ItemId>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>175</EntryNo>
      <ParentId>173</ParentId>
      <Level>2</Level>
      <ItemId>33116</ItemId>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>180</EntryNo>
      <ParentId>176</ParentId>
      <Level>2</Level>
      <ItemId>33120</ItemId>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>185</EntryNo>
      <ParentId>181</ParentId>
      <Level>2</Level>
      <ItemId>33130</ItemId>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>190</EntryNo>
      <ParentId>186</ParentId>
      <Level>2</Level>
      <ItemId>33140</ItemId>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>194</EntryNo>
      <ParentId>191</ParentId>
      <Level>2</Level>
      <ItemId>33150</ItemId>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>198</EntryNo>
      <ParentId>195</ParentId>
      <Level>2</Level>
      <ItemId>33160</ItemId>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>202</EntryNo>
      <ParentId>199</ParentId>
      <Level>2</Level>
      <ItemId>33170</ItemId>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>213</EntryNo>
      <ParentId>211</ParentId>
      <Level>2</Level>
      <ItemId>40010</ItemId>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>231</EntryNo>
      <ParentId>224</ParentId>
      <Level>2</Level>
      <ItemId>R0001</ItemId>
      <ItemModGroupId>POT+RICE</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>245</EntryNo>
      <ParentId>239</ParentId>
      <Level>2</Level>
      <ItemId>R0006</ItemId>
      <ItemModGroupId>POT+RICE</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>254</EntryNo>
      <ParentId>246</ParentId>
      <Level>2</Level>
      <ItemId>R0009</ItemId>
      <ItemModGroupId>VEG 9</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>255</EntryNo>
      <ParentId>246</ParentId>
      <Level>2</Level>
      <ItemId>R0009</ItemId>
      <ItemModGroupId>TOPP 9</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>256</EntryNo>
      <ParentId>246</ParentId>
      <Level>2</Level>
      <ItemId>R0009</ItemId>
      <ItemModGroupId>VEG 9 H</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>257</EntryNo>
      <ParentId>246</ParentId>
      <Level>2</Level>
      <ItemId>R0009</ItemId>
      <ItemModGroupId>TOPP 9 H</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>265</EntryNo>
      <ParentId>258</ParentId>
      <Level>2</Level>
      <ItemId>R0010</ItemId>
      <ItemModGroupId>VEG 12</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>266</EntryNo>
      <ParentId>258</ParentId>
      <Level>2</Level>
      <ItemId>R0010</ItemId>
      <ItemModGroupId>TOPP 12</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>267</EntryNo>
      <ParentId>258</ParentId>
      <Level>2</Level>
      <ItemId>R0010</ItemId>
      <ItemModGroupId>VEG 12 H</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ItemModifierGroups>
      <EntryNo>268</EntryNo>
      <ParentId>258</ParentId>
      <Level>2</Level>
      <ItemId>R0010</ItemId>
      <ItemModGroupId>TOPP 12 H</ItemModGroupId>
      <ItemModRequired>No</ItemModRequired>
    </ItemModifierGroups>
    <ModifierGroups>
      <EntryNo>276</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>BUGER-TIME</ItemModGroupId>
      <ItemModGroupDescription>Burger Options</ItemModGroupDescription>
      <MinSelection>1</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>280</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>POT+RICE</ItemModGroupId>
      <ItemModGroupDescription>Potatoes and rice</ItemModGroupDescription>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>287</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>SOFT-DRINK</ItemModGroupId>
      <ItemModGroupDescription>Soft Drink Modifiers</ItemModGroupDescription>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>292</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>STEAK-TIME</ItemModGroupId>
      <ItemModGroupDescription>Steak Options</ItemModGroupDescription>
      <MinSelection>1</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>298</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>TEST</ItemModGroupId>
      <ItemModGroupDescription>Item Cat. Test</ItemModGroupDescription>
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>302</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>TOPP 12</ItemModGroupId>
      <ItemModGroupDescription>Toppings 12"</ItemModGroupDescription>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>307</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>TOPP 12 H</ItemModGroupId>
      <ItemModGroupDescription>Toppings 12" Half</ItemModGroupDescription>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>312</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>TOPP 9</ItemModGroupId>
      <ItemModGroupDescription>Toppings 9"</ItemModGroupDescription>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>317</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>TOPP 9 H</ItemModGroupId>
      <ItemModGroupDescription>Toppings 9" Half</ItemModGroupDescription>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>322</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <ItemModGroupDescription>Dessert Modifiers</ItemModGroupDescription>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>326</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>VEG 12</ItemModGroupId>
      <ItemModGroupDescription>Vegetables 12"</ItemModGroupDescription>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>335</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>VEG 12 H</ItemModGroupId>
      <ItemModGroupDescription>Vegetables 12" Half</ItemModGroupDescription>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>344</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>VEG 9</ItemModGroupId>
      <ItemModGroupDescription>Vegetables 9"</ItemModGroupDescription>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroups>
    <ModifierGroups>
      <EntryNo>353</EntryNo>
      <Level>1</Level>
      <ItemModGroupId>VEG 9 H</ItemModGroupId>
      <ItemModGroupDescription>Vegetables 9" Half</ItemModGroupDescription>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroups>
    <ModifierGroupItems>
      <EntryNo>277</EntryNo>
      <ParentId>276</ParentId>
      <Level>2</Level>
      <ItemModGroupId>BUGER-TIME</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>Redish</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>278</EntryNo>
      <ParentId>276</ParentId>
      <Level>2</Level>
      <ItemModGroupId>BUGER-TIME</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>Standard</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>279</EntryNo>
      <ParentId>276</ParentId>
      <Level>2</Level>
      <ItemModGroupId>BUGER-TIME</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>Extra</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>281</EntryNo>
      <ParentId>280</ParentId>
      <Level>2</Level>
      <ItemModGroupId>POT+RICE</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>40010</ItemId>
      <Description />
      <ItemModPricePercentage>20</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>282</EntryNo>
      <ParentId>280</ParentId>
      <Level>2</Level>
      <ItemModGroupId>POT+RICE</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>40010</ItemId>
      <Description />
      <ItemModPricePercentage>20</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>283</EntryNo>
      <ParentId>280</ParentId>
      <Level>2</Level>
      <ItemModGroupId>POT+RICE</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>33000</ItemId>
      <Description />
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType />
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>284</EntryNo>
      <ParentId>280</ParentId>
      <Level>2</Level>
      <ItemModGroupId>POT+RICE</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>33030</ItemId>
      <Description />
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType />
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>285</EntryNo>
      <ParentId>280</ParentId>
      <Level>2</Level>
      <ItemModGroupId>POT+RICE</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20077</ItemId>
      <Description />
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType />
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>286</EntryNo>
      <ParentId>280</ParentId>
      <Level>2</Level>
      <ItemModGroupId>POT+RICE</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20078</ItemId>
      <Description />
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType />
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>288</EntryNo>
      <ParentId>287</ParentId>
      <Level>2</Level>
      <ItemModGroupId>SOFT-DRINK</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>+ Ice</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>289</EntryNo>
      <ParentId>287</ParentId>
      <Level>2</Level>
      <ItemModGroupId>SOFT-DRINK</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>+ Lemon slice</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>290</EntryNo>
      <ParentId>287</ParentId>
      <Level>2</Level>
      <ItemModGroupId>SOFT-DRINK</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>+ Straw</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>291</EntryNo>
      <ParentId>287</ParentId>
      <Level>2</Level>
      <ItemModGroupId>SOFT-DRINK</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId>1000</ItemId>
      <Description>Wodka shot</Description>
      <ItemModPricePercentage>124.9875</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>293</EntryNo>
      <ParentId>292</ParentId>
      <Level>2</Level>
      <ItemModGroupId>STEAK-TIME</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>Rare</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>294</EntryNo>
      <ParentId>292</ParentId>
      <Level>2</Level>
      <ItemModGroupId>STEAK-TIME</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>Medium Rare</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>295</EntryNo>
      <ParentId>292</ParentId>
      <Level>2</Level>
      <ItemModGroupId>STEAK-TIME</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>Medium</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>296</EntryNo>
      <ParentId>292</ParentId>
      <Level>2</Level>
      <ItemModGroupId>STEAK-TIME</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>Medium Well</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>297</EntryNo>
      <ParentId>292</ParentId>
      <Level>2</Level>
      <ItemModGroupId>STEAK-TIME</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>Well Done</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>299</EntryNo>
      <ParentId>298</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TEST</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>Test 1</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>300</EntryNo>
      <ParentId>298</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TEST</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>Test 2</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>301</EntryNo>
      <ParentId>298</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TEST</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>Test 3</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>303</EntryNo>
      <ParentId>302</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 12</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>32200</ItemId>
      <Description />
      <ItemModPricePercentage>1.8</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>304</EntryNo>
      <ParentId>302</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 12</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>32201</ItemId>
      <Description />
      <ItemModPricePercentage>2.2</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>305</EntryNo>
      <ParentId>302</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 12</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20069</ItemId>
      <Description />
      <ItemModPricePercentage>1.8</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>306</EntryNo>
      <ParentId>302</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 12</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>34003</ItemId>
      <Description />
      <ItemModPricePercentage>2.2</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>308</EntryNo>
      <ParentId>307</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 12 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>32200</ItemId>
      <Description />
      <ItemModPricePercentage>0.9</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>309</EntryNo>
      <ParentId>307</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 12 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>32201</ItemId>
      <Description />
      <ItemModPricePercentage>1.1</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>310</EntryNo>
      <ParentId>307</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 12 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20069</ItemId>
      <Description />
      <ItemModPricePercentage>0.9</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>311</EntryNo>
      <ParentId>307</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 12 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>34003</ItemId>
      <Description />
      <ItemModPricePercentage>1.1</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>313</EntryNo>
      <ParentId>312</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 9</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>32200</ItemId>
      <Description />
      <ItemModPricePercentage>1.2</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>314</EntryNo>
      <ParentId>312</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 9</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>32201</ItemId>
      <Description />
      <ItemModPricePercentage>1.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>315</EntryNo>
      <ParentId>312</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 9</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20069</ItemId>
      <Description />
      <ItemModPricePercentage>1.2</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>316</EntryNo>
      <ParentId>312</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 9</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>34003</ItemId>
      <Description />
      <ItemModPricePercentage>1.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>318</EntryNo>
      <ParentId>317</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 9 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>32200</ItemId>
      <Description />
      <ItemModPricePercentage>0.6</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>319</EntryNo>
      <ParentId>317</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 9 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>32201</ItemId>
      <Description />
      <ItemModPricePercentage>0.75</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>320</EntryNo>
      <ParentId>317</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 9 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20069</ItemId>
      <Description />
      <ItemModPricePercentage>0.6</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>321</EntryNo>
      <ParentId>317</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TOPP 9 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>34003</ItemId>
      <Description />
      <ItemModPricePercentage>0.75</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>323</EntryNo>
      <ParentId>322</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>+ Extra plate</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>324</EntryNo>
      <ParentId>322</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>+ Extra cutlery</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>325</EntryNo>
      <ParentId>322</ParentId>
      <Level>2</Level>
      <ItemModGroupId>TXT-DESSER</ItemModGroupId>
      <TextModifier>Yes</TextModifier>
      <ItemId />
      <Description>+ Birthday celebration</Description>
      <ItemModPricePercentage>0</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM />
      <MinSelection>0</MinSelection>
      <MaxSelection>1</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>327</EntryNo>
      <ParentId>326</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20061</ItemId>
      <Description />
      <ItemModPricePercentage>1.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>328</EntryNo>
      <ParentId>326</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20062</ItemId>
      <Description />
      <ItemModPricePercentage>1.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>329</EntryNo>
      <ParentId>326</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20063</ItemId>
      <Description />
      <ItemModPricePercentage>1.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>330</EntryNo>
      <ParentId>326</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20064</ItemId>
      <Description />
      <ItemModPricePercentage>1.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>331</EntryNo>
      <ParentId>326</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20065</ItemId>
      <Description />
      <ItemModPricePercentage>1.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>332</EntryNo>
      <ParentId>326</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20066</ItemId>
      <Description />
      <ItemModPricePercentage>1.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>333</EntryNo>
      <ParentId>326</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20070</ItemId>
      <Description />
      <ItemModPricePercentage>1.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>334</EntryNo>
      <ParentId>326</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>33050</ItemId>
      <Description />
      <ItemModPricePercentage>1.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>336</EntryNo>
      <ParentId>335</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20061</ItemId>
      <Description />
      <ItemModPricePercentage>0.75</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>337</EntryNo>
      <ParentId>335</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20062</ItemId>
      <Description />
      <ItemModPricePercentage>0.75</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>338</EntryNo>
      <ParentId>335</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20063</ItemId>
      <Description />
      <ItemModPricePercentage>0.75</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>339</EntryNo>
      <ParentId>335</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20064</ItemId>
      <Description />
      <ItemModPricePercentage>0.75</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>340</EntryNo>
      <ParentId>335</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20065</ItemId>
      <Description />
      <ItemModPricePercentage>0.75</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>341</EntryNo>
      <ParentId>335</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20066</ItemId>
      <Description />
      <ItemModPricePercentage>0.75</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>342</EntryNo>
      <ParentId>335</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20070</ItemId>
      <Description />
      <ItemModPricePercentage>0.75</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>343</EntryNo>
      <ParentId>335</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 12 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>33050</ItemId>
      <Description />
      <ItemModPricePercentage>0.75</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>12 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>345</EntryNo>
      <ParentId>344</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20061</ItemId>
      <Description />
      <ItemModPricePercentage>1</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>346</EntryNo>
      <ParentId>344</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20062</ItemId>
      <Description />
      <ItemModPricePercentage>1</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>347</EntryNo>
      <ParentId>344</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20063</ItemId>
      <Description />
      <ItemModPricePercentage>1</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>348</EntryNo>
      <ParentId>344</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20064</ItemId>
      <Description />
      <ItemModPricePercentage>1</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>349</EntryNo>
      <ParentId>344</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20065</ItemId>
      <Description />
      <ItemModPricePercentage>1</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>350</EntryNo>
      <ParentId>344</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20066</ItemId>
      <Description />
      <ItemModPricePercentage>1</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>351</EntryNo>
      <ParentId>344</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20070</ItemId>
      <Description />
      <ItemModPricePercentage>1</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>352</EntryNo>
      <ParentId>344</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>33050</ItemId>
      <Description />
      <ItemModPricePercentage>1</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 INCH</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>354</EntryNo>
      <ParentId>353</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20061</ItemId>
      <Description />
      <ItemModPricePercentage>0.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>355</EntryNo>
      <ParentId>353</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20062</ItemId>
      <Description />
      <ItemModPricePercentage>0.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>356</EntryNo>
      <ParentId>353</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20063</ItemId>
      <Description />
      <ItemModPricePercentage>0.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>357</EntryNo>
      <ParentId>353</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20064</ItemId>
      <Description />
      <ItemModPricePercentage>0.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>358</EntryNo>
      <ParentId>353</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20065</ItemId>
      <Description />
      <ItemModPricePercentage>0.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>359</EntryNo>
      <ParentId>353</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20066</ItemId>
      <Description />
      <ItemModPricePercentage>0.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>360</EntryNo>
      <ParentId>353</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>20070</ItemId>
      <Description />
      <ItemModPricePercentage>0.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <ModifierGroupItems>
      <EntryNo>361</EntryNo>
      <ParentId>353</ParentId>
      <Level>2</Level>
      <ItemModGroupId>VEG 9 H</ItemModGroupId>
      <TextModifier>No</TextModifier>
      <ItemId>33050</ItemId>
      <Description />
      <ItemModPricePercentage>0.5</ItemModPricePercentage>
      <ItemModPriceType>Price</ItemModPriceType>
      <ItemModQty>0</ItemModQty>
      <ItemModUoM>9 IN HALF</ItemModUoM>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </ModifierGroupItems>
    <Offers>
      <EntryNo>362</EntryNo>
      <Level>1</Level>
      <OfferId>S10006</OfferId>
      <OfferDescription>Club Sandwich with Onion Rings Salad</OfferDescription>
      <OfferSecondaryDescription>Our version of Club Sandwich where we use our fresh baked grain roll, tomatoes, salad, salami and turkey. The Sandwich is served with onion rings and salad</OfferSecondaryDescription>
      <OfferPrice>9</OfferPrice>
      <ImageID>D06</ImageID>
    </Offers>
    <Offers>
      <EntryNo>366</EntryNo>
      <Level>1</Level>
      <OfferId>S10012</OfferId>
      <OfferDescription>BBQ Burger - Soda and Dessert</OfferDescription>
      <OfferSecondaryDescription>The offer for the month. 
Burger, soda and dessert for a special price</OfferSecondaryDescription>
      <OfferPrice>9.5</OfferPrice>
      <ImageID>D12</ImageID>
    </Offers>
    <Offers>
      <EntryNo>392</EntryNo>
      <Level>1</Level>
      <OfferId>S10011</OfferId>
      <OfferDescription>Coffee and Cake  - at your choice</OfferDescription>
      <OfferSecondaryDescription>Select your favorite coffee and enjoy it with a slice of cake. Look at the cake's selections of the day, always fresh and delicious</OfferSecondaryDescription>
      <OfferPrice>2.5</OfferPrice>
      <ImageID>PUBOFF-COFFEECA</ImageID>
    </Offers>
    <OfferLines>
      <EntryNo>363</EntryNo>
      <ParentId>362</ParentId>
      <Level>2</Level>
      <OfferId>S10006</OfferId>
      <OfferLineId>10000</OfferLineId>
      <DefaultItemId>32020</DefaultItemId>
      <OfferLineDescription>Club Sandwich</OfferLineDescription>
      <OfferLineQty>1</OfferLineQty>
    </OfferLines>
    <OfferLines>
      <EntryNo>364</EntryNo>
      <ParentId>362</ParentId>
      <Level>2</Level>
      <OfferId>S10006</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DefaultItemId>33040</DefaultItemId>
      <OfferLineDescription>Onion Rings</OfferLineDescription>
      <OfferLineQty>1</OfferLineQty>
    </OfferLines>
    <OfferLines>
      <EntryNo>365</EntryNo>
      <ParentId>362</ParentId>
      <Level>2</Level>
      <OfferId>S10006</OfferId>
      <OfferLineId>30000</OfferLineId>
      <DefaultItemId>33050</DefaultItemId>
      <OfferLineDescription>Salad</OfferLineDescription>
      <OfferLineQty>1</OfferLineQty>
    </OfferLines>
    <OfferLines>
      <EntryNo>367</EntryNo>
      <ParentId>366</ParentId>
      <Level>2</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>10000</OfferLineId>
      <DefaultItemId>32000</DefaultItemId>
      <OfferLineDescription>BBQ Burger</OfferLineDescription>
      <OfferLineQty>1</OfferLineQty>
    </OfferLines>
    <OfferLines>
      <EntryNo>368</EntryNo>
      <ParentId>366</ParentId>
      <Level>2</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DefaultItemId>30085</DefaultItemId>
      <OfferLineDescription>Soda reg., med.,  large</OfferLineDescription>
      <OfferLineQty>1</OfferLineQty>
    </OfferLines>
    <OfferLines>
      <EntryNo>373</EntryNo>
      <ParentId>366</ParentId>
      <Level>2</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>30000</OfferLineId>
      <DefaultItemId>33010</DefaultItemId>
      <OfferLineDescription>Potatoes and rice</OfferLineDescription>
      <OfferLineQty>1</OfferLineQty>
    </OfferLines>
    <OfferLines>
      <EntryNo>380</EntryNo>
      <ParentId>366</ParentId>
      <Level>2</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>40000</OfferLineId>
      <DefaultItemId>33100</DefaultItemId>
      <OfferLineDescription>Desserts</OfferLineDescription>
      <OfferLineQty>1</OfferLineQty>
    </OfferLines>
    <OfferLines>
      <EntryNo>393</EntryNo>
      <ParentId>392</ParentId>
      <Level>2</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>10000</OfferLineId>
      <DefaultItemId>30090</DefaultItemId>
      <OfferLineDescription>Coffee selection</OfferLineDescription>
      <OfferLineQty>1</OfferLineQty>
    </OfferLines>
    <OfferLines>
      <EntryNo>398</EntryNo>
      <ParentId>392</ParentId>
      <Level>2</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DefaultItemId>33100</DefaultItemId>
      <OfferLineDescription>Desserts</OfferLineDescription>
      <OfferLineQty>1</OfferLineQty>
    </OfferLines>
    <OfferModifierGroups>
      <EntryNo>369</EntryNo>
      <ParentId>368</ParentId>
      <Level>3</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>SODA</DealModGroupId>
      <DealModGroupDescription>Soda reg., med.,  large</DealModGroupDescription>
      <MinSelection>1</MinSelection>
      <MaxSelection>1</MaxSelection>
      <DealModRequired>Yes</DealModRequired>
    </OfferModifierGroups>
    <OfferModifierGroups>
      <EntryNo>374</EntryNo>
      <ParentId>373</ParentId>
      <Level>3</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>30000</OfferLineId>
      <DealModGroupId>POT+RICE</DealModGroupId>
      <DealModGroupDescription>Potatoes and rice</DealModGroupDescription>
      <MinSelection>1</MinSelection>
      <MaxSelection>1</MaxSelection>
      <DealModRequired>Yes</DealModRequired>
    </OfferModifierGroups>
    <OfferModifierGroups>
      <EntryNo>381</EntryNo>
      <ParentId>380</ParentId>
      <Level>3</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>40000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <DealModGroupDescription>Desserts</DealModGroupDescription>
      <MinSelection>1</MinSelection>
      <MaxSelection>1</MaxSelection>
      <DealModRequired>Yes</DealModRequired>
    </OfferModifierGroups>
    <OfferModifierGroups>
      <EntryNo>394</EntryNo>
      <ParentId>393</ParentId>
      <Level>3</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>10000</OfferLineId>
      <DealModGroupId>COFFEE</DealModGroupId>
      <DealModGroupDescription>Coffee selection</DealModGroupDescription>
      <MinSelection>1</MinSelection>
      <MaxSelection>1</MaxSelection>
      <DealModRequired>Yes</DealModRequired>
    </OfferModifierGroups>
    <OfferModifierGroups>
      <EntryNo>399</EntryNo>
      <ParentId>398</ParentId>
      <Level>3</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <DealModGroupDescription>Desserts</DealModGroupDescription>
      <MinSelection>1</MinSelection>
      <MaxSelection>1</MaxSelection>
      <DealModRequired>Yes</DealModRequired>
    </OfferModifierGroups>
    <OfferModifierGroupItems>
      <EntryNo>370</EntryNo>
      <ParentId>369</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>SODA</DealModGroupId>
      <ItemId>30085</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>371</EntryNo>
      <ParentId>369</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>SODA</DealModGroupId>
      <ItemId>30085</ItemId>
      <DealModPrice>0.2</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>372</EntryNo>
      <ParentId>369</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>SODA</DealModGroupId>
      <ItemId>30085</ItemId>
      <DealModPrice>0.4</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>375</EntryNo>
      <ParentId>374</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>30000</OfferLineId>
      <DealModGroupId>POT+RICE</DealModGroupId>
      <ItemId>33010</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>376</EntryNo>
      <ParentId>374</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>30000</OfferLineId>
      <DealModGroupId>POT+RICE</DealModGroupId>
      <ItemId>33020</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>377</EntryNo>
      <ParentId>374</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>30000</OfferLineId>
      <DealModGroupId>POT+RICE</DealModGroupId>
      <ItemId>33000</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>378</EntryNo>
      <ParentId>374</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>30000</OfferLineId>
      <DealModGroupId>POT+RICE</DealModGroupId>
      <ItemId>33030</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>379</EntryNo>
      <ParentId>374</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>30000</OfferLineId>
      <DealModGroupId>POT+RICE</DealModGroupId>
      <ItemId>20078</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>382</EntryNo>
      <ParentId>381</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>40000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33100</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>383</EntryNo>
      <ParentId>381</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>40000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33110</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>384</EntryNo>
      <ParentId>381</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>40000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33115</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>385</EntryNo>
      <ParentId>381</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>40000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33116</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>386</EntryNo>
      <ParentId>381</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>40000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33120</ItemId>
      <DealModPrice>0.4</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>387</EntryNo>
      <ParentId>381</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>40000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33130</ItemId>
      <DealModPrice>0.4</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>388</EntryNo>
      <ParentId>381</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>40000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33140</ItemId>
      <DealModPrice>0.2</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>389</EntryNo>
      <ParentId>381</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>40000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33150</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>390</EntryNo>
      <ParentId>381</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>40000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33160</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>391</EntryNo>
      <ParentId>381</ParentId>
      <Level>4</Level>
      <OfferId>S10012</OfferId>
      <OfferLineId>40000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33170</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>395</EntryNo>
      <ParentId>394</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>10000</OfferLineId>
      <DealModGroupId>COFFEE</DealModGroupId>
      <ItemId>30090</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>396</EntryNo>
      <ParentId>394</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>10000</OfferLineId>
      <DealModGroupId>COFFEE</DealModGroupId>
      <ItemId>30093</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>397</EntryNo>
      <ParentId>394</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>10000</OfferLineId>
      <DealModGroupId>COFFEE</DealModGroupId>
      <ItemId>30094</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>400</EntryNo>
      <ParentId>399</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33100</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>401</EntryNo>
      <ParentId>399</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33110</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>402</EntryNo>
      <ParentId>399</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33115</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>403</EntryNo>
      <ParentId>399</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33116</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>404</EntryNo>
      <ParentId>399</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33120</ItemId>
      <DealModPrice>0.3</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>405</EntryNo>
      <ParentId>399</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33130</ItemId>
      <DealModPrice>0.3</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>406</EntryNo>
      <ParentId>399</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33140</ItemId>
      <DealModPrice>0.3</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>407</EntryNo>
      <ParentId>399</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33150</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>408</EntryNo>
      <ParentId>399</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33160</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
    <OfferModifierGroupItems>
      <EntryNo>409</EntryNo>
      <ParentId>399</ParentId>
      <Level>4</Level>
      <OfferId>S10011</OfferId>
      <OfferLineId>20000</OfferLineId>
      <DealModGroupId>DESSERT</DealModGroupId>
      <ItemId>33170</ItemId>
      <DealModPrice>0</DealModPrice>
      <MinSelection>0</MinSelection>
      <MaxSelection>1000</MaxSelection>
    </OfferModifierGroupItems>
  </Response_Body>
</Response>
*/