using System;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Setup;

namespace LSOmni.DataAccess.BOConnection.PreCommon.Mapping
{
    public class StoreMapping : BaseMapping
    {
        public List<StoreHours> MapFromRootToOpeningHours(LSCentral.RootGetStoreOpeningHours root, int offset)
        {
            List<StoreHours> list = new List<StoreHours>();

            if (root.RetailCalendarLine == null)
                return list;

            foreach (LSCentral.RetailCalendarLine line in root.RetailCalendarLine)
            {
                StoreHours storehr = new StoreHours();
                storehr.OpenFrom = line.TimeFrom;
                storehr.OpenTo = line.TimeTo;

                int dayofweek = line.DayNo;
                storehr.NameOfDay = line.DayName;
                storehr.StoreId = line.CalendarID;
                storehr.Description = string.IsNullOrEmpty(line.ReasonClosed) ? storehr.NameOfDay : line.ReasonClosed;
                storehr.Type = (StoreHourOpeningType)ConvertTo.SafeInt(line.LineType);
                storehr.CalendarType = (StoreHourCalendarType)ConvertTo.SafeInt(line.CalendarType);

                storehr.DayOfWeek = (dayofweek == 7) ? 0 : dayofweek; //NAV starts with Sunday as 1 but .Net Sunday=0
                storehr.OpenFrom = ConvertTo.SafeDateTime(storehr.OpenFrom.AddHours(offset));
                storehr.OpenTo = ConvertTo.SafeDateTime(storehr.OpenTo.AddHours(offset));
                storehr.StartDate = ConvertTo.SafeDateTime(line.StartingDate.AddHours(offset));
                storehr.EndDate = ConvertTo.SafeDateTime(line.EndingDate.AddHours(offset));

                list.Add(storehr);
            }
            return list;
        }

        public List<ReturnPolicy> MapFromRootToReturnPolicy(LSCentral.RootGetReturnPolicy root)
        {
            List<ReturnPolicy> list = new List<ReturnPolicy>();
            if (root.ReturnPolicy == null)
                return list;

            foreach (LSCentral.ReturnPolicy line in root.ReturnPolicy)
            {
                list.Add(new ReturnPolicy()
                {
                    StoreId = line.Store_No,
                    StoreGroup = line.Store_Group_Code,
                    ItemCategory = line.Item_Category_Code,
                    ProductGroup = line.Retail_Product_Code,
                    ItemId = line.Item_No,
                    VariantCode = line.Variant_Code,
                    VariantDimension1 = line.Variant_Dimension_1_Code,
                    RefundNotAllowed = line.Refund_not_Allowed,
                    RefundPeriodLength = line.Refund_Period_Length,
                    ManagerPrivileges = line.Manager_Privileges,
                    Message1 = line.Message_1,
                    Message2 = line.Message_2,
                    ReturnPolicyHTML = line.Return_Policy_HTML
                });
            }
            return list;
        }
    }
}
