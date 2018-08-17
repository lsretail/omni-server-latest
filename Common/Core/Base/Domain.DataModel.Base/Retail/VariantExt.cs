using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    /// <summary>
    /// NOTE. VariantExt  is the Extended Variant Values in Nav
    /// </summary>
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class VariantExt : IDisposable
    {
        public VariantExt(string itemId)
        {
            Values = new List<DimValue>();
            ItemId = itemId;
            Dimension = "1";
            Code = string.Empty;
        }

        public VariantExt()
            : this(string.Empty)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Values != null)
                    Values.Clear();
            }
        }

        [DataMember]
        public List<DimValue> Values { get; set; }
        public string ItemId { get; set; }
        [DataMember]
        public string Dimension { get; set; }
        [DataMember]
        public string Code { get; set; }

        // Sets the IsSelected variable of each variant in the variantsExt's variant list. 
        // If the variant description is found in variantRegistration then isSelected = true,
        // else it is set to isSelected = false
        public static void SetIsSelectedFromVariantReg(List<VariantExt> variantsExt, VariantRegistration variantRegistration)
        {
            for (int i = 0; i < 6; i++)
            {
                if (i == 0 && variantsExt.Count > i)
                {
                    foreach (DimValue x in variantsExt[0].Values)
                    {
                        if (x.Value == variantRegistration.Dimension1)
                        {
                            x.IsSelected = true;
                        }
                        else
                        {
                            x.IsSelected = false;
                        }
                    }
                }
                if (i == 1 && variantsExt.Count > i)
                {
                    foreach (DimValue x in variantsExt[1].Values)
                    {
                        if (x.Value == variantRegistration.Dimension2)
                        {
                            x.IsSelected = true;
                        }
                        else
                        {
                            x.IsSelected = false;
                        }
                    }
                }
                if (i == 2 && variantsExt.Count > i)
                {
                    foreach (DimValue x in variantsExt[2].Values)
                    {
                        if (x.Value == variantRegistration.Dimension3)
                        {
                            x.IsSelected = true;
                        }
                        else
                        {
                            x.IsSelected = false;
                        }
                    }
                }
                if (i == 3 && variantsExt.Count > i)
                {
                    foreach (DimValue x in variantsExt[3].Values)
                    {
                        if (x.Value == variantRegistration.Dimension4)
                        {
                            x.IsSelected = true;
                        }
                        else
                        {
                            x.IsSelected = false;
                        }
                    }
                }
                if (i == 4 && variantsExt.Count > i)
                {
                    foreach (DimValue x in variantsExt[4].Values)
                    {
                        if (x.Value == variantRegistration.Dimension5)
                        {
                            x.IsSelected = true;
                        }
                        else
                        {
                            x.IsSelected = false;
                        }
                    }
                }
                if (i == 5 && variantsExt.Count > i)
                {
                    foreach (DimValue x in variantsExt[5].Values)
                    {
                        if (x.Value == variantRegistration.Dimension6)
                        {
                            x.IsSelected = true;
                        }
                        else
                        {
                            x.IsSelected = false;
                        }
                    }
                }
            }
        }

        public string ExtendedVariantCodeToString(ExtendedVariantCode extendedVariantCode)
        {
            switch (extendedVariantCode)
            {
                case ExtendedVariantCode.Colour:
                    return "COLOUR";

                case ExtendedVariantCode.Fuel:
                    return "FUEL";

                case ExtendedVariantCode.IcecrFlavours:
                    return "ICECR-FLAVOURS";

                case ExtendedVariantCode.Size:
                    return "SIZE";

                case ExtendedVariantCode.SodaFlavours:
                    return "SODA FLAVOUR";

                case ExtendedVariantCode.Style:
                    return "STYLE";

                case ExtendedVariantCode.Temperature:
                    return "TEMPERATURE";

                case ExtendedVariantCode.Width:
                    return "WIDTH";
            }

            return string.Empty;
        }

        public ExtendedVariantCode StringToExtendedVariantCode(string value)
        {
            switch (value)
            {
                case "COLOUR":
                    return ExtendedVariantCode.Colour;

                case "FUEL":
                    return ExtendedVariantCode.Fuel;

                case "ICECR-FLAVOURS":
                    return ExtendedVariantCode.IcecrFlavours;

                case "SIZE":
                    return ExtendedVariantCode.Size;

                case "SODA FLAVOUR":
                    return ExtendedVariantCode.SodaFlavours;

                case "STYLE":
                    return ExtendedVariantCode.Style;

                case "TEMPERATURE":
                    return ExtendedVariantCode.Temperature;

                case "WIDTH":
                    return ExtendedVariantCode.Width;
            }

            return ExtendedVariantCode.Unknown;
        }

        public override string ToString()
        {
            return string.Format(@"Dimensions: {0} ItemId: {1}  Dimension: {2} Code: {3}",
                 Values.ToString(), ItemId, Dimension, Code);
        }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class DimValue : IDisposable
    {
        public DimValue(string val)
        {
            Value = val;
            DisplayOrder = 0;
            IsSelected = false;
        }

        public DimValue()
            : this(string.Empty)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public int DisplayOrder { get; set; }

        [DataMember]
        public bool IsSelected;

        public override string ToString()
        {
            return string.Format(@"Value: {0}  DisplayOrder: {1} ", Value, DisplayOrder);
        }
    }
}
