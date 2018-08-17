using System;
using System.Collections.Generic;
using System.Text;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    public enum ExtendedVariantCode
    {
        Unknown,
        Colour,
        Fuel,
        IcecrFlavours,
        Size,
        SodaFlavours,
        Style,
        Temperature,
        Width
    }

    public class ExtendedVariant : Entity
    {
        public string ItemId { get; set; }
        public int Dimension { get; set; }
        public ExtendedVariantCode ExtendedVariantCode { get; set; }
        public string Value { get; set; }
        public int LogicalOrder { get; set; }
        public string FrameworkCode { get; set; }
        public string Code { get; set; }

        public string VariantCode
        {
            get
            {
                switch (ExtendedVariantCode)
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
            set
            {
                switch (value)
                {
                    case "COLOUR":
                        ExtendedVariantCode = ExtendedVariantCode.Colour;
                        break;

                    case "FUEL":
                        ExtendedVariantCode= ExtendedVariantCode.Fuel;
                        break;

                    case "ICECR-FLAVOURS":
                        ExtendedVariantCode = ExtendedVariantCode.IcecrFlavours;
                        break;

                    case "SIZE":
                        ExtendedVariantCode = ExtendedVariantCode.Size;
                        break;

                    case "SODA FLAVOUR":
                        ExtendedVariantCode = ExtendedVariantCode.SodaFlavours;
                        break;

                    case "STYLE":
                        ExtendedVariantCode = ExtendedVariantCode.Style;
                        break;

                    case "TEMPERATURE":
                        ExtendedVariantCode = ExtendedVariantCode.Temperature;
                        break;

                    case "WIDTH":
                        ExtendedVariantCode = ExtendedVariantCode.Width;
                        break;

                }
            }
        }
    }
}
