using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSOmni.DataAccess.BOConnection.NavSQL.Mapping
{
    public class StoreMapping : BaseMapping
    {
        public List<StoreHours> MapFromRootToOpeningHours(NavWS.RootGetStoreOpeningHours root, int offset)
        {
            List<StoreHours> list = new List<StoreHours>();

            if (root.RetailCalendarLine == null)
                return list;

            foreach(NavWS.RetailCalendarLine line in root.RetailCalendarLine)
            {
                StoreHours storehr = new StoreHours();
                storehr.OpenFrom = line.TimeFrom;
                storehr.OpenTo = line.TimeTo;

                //something is wrong, don't take store hours that have not value
                if (storehr.OpenFrom == DateTime.MinValue || storehr.OpenTo == DateTime.MinValue)
                    continue;

                int dayofweek = line.DayNo;
                storehr.NameOfDay = line.DayName;
                storehr.StoreId = line.CalendarID;
                storehr.Description = storehr.NameOfDay;
                storehr.Type = (StoreHourOpeningType)ConvertTo.SafeInt(line.LineType);

                storehr.DayOfWeek = dayofweek == 7 ? 0 : dayofweek;
                storehr.OpenFrom = ConvertTo.SafeDateTime(storehr.OpenFrom.AddHours(offset));
                storehr.OpenTo = ConvertTo.SafeDateTime(storehr.OpenTo.AddHours(offset));
                list.Add(storehr);
            }

            return list;
        }
    }
}
