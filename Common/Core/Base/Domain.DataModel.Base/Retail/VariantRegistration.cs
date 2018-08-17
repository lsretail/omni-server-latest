using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LSRetail.Omni.Domain.DataModel.Base.Base;
using System.Text;
using LSRetail.Omni.Domain.DataModel.Base.Retail.SpecialCase;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    /// <summary>
    /// NOTE. VariantExt  is the Extended Variant Values in Nav
    /// </summary>
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017"), KnownType(typeof(UnknownVariantRegistration))]
    public class VariantRegistration : Entity, IDisposable
    {
        #region Properties

        [DataMember]
        public string Dimension1 { get; set; }
        [DataMember]
        public string Dimension2 { get; set; }
        [DataMember]
        public string Dimension3 { get; set; }
        [DataMember]
        public string Dimension4 { get; set; }
        [DataMember]
        public string Dimension5 { get; set; }
        [DataMember]
        public string Dimension6 { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string FrameworkCode { get; set; }
        [DataMember]
        public List<ImageView> Images { get; set; }

        #endregion

        #region Constructors

        public VariantRegistration()
            : this(null)
        {
        }

        // Id = VariantCode

        public VariantRegistration(string id)
            : base(id)
        {
            this.Dimension1 = string.Empty;
            this.Dimension2 = string.Empty;
            this.Dimension3 = string.Empty;
            this.Dimension4 = string.Empty;
            this.Dimension5 = string.Empty;
            this.Dimension6 = string.Empty;
            this.ItemId = string.Empty;
            this.FrameworkCode = string.Empty;
            this.Images = new List<ImageView>();
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
                if (Images != null)
                    Images.Clear();
            }
        }

        #endregion

        #region functions

        public string GetVariantDimension(int dimension)
        {
            switch (dimension)
            {
                case 1:
                    return Dimension1;

                case 2:
                    return Dimension2;

                case 3:
                    return Dimension3;

                case 4:
                    return Dimension4;

                case 5:
                    return Dimension5;

                case 6:
                    return Dimension6;
            }

            return string.Empty;
        }

        public string SetVariantDimension(int dimension, string value)
        {
            switch (dimension)
            {
                case 1:
                    Dimension1 = value;
                    break;

                case 2:
                    Dimension2 = value;
                    break;

                case 3:
                    Dimension3 = value;
                    break;

                case 4:
                    Dimension4 = value;
                    break;

                case 5:
                    Dimension5 = value;
                    break;

                case 6:
                    Dimension6 = value;
                    break;
            }

            return string.Empty;
        }

        public static VariantRegistration GetVariantRegistrationFromVariantExts(List<VariantExt> variantsExt, List<VariantRegistration> variantsRegistration)
        {
            string[] selectedIds = new string[6];

            for (int i = 0; i < 6; i++)
            {
                if (variantsExt.Count <= i)
                    selectedIds[i] = string.Empty;
                else
                {
                    DimValue selectedVariant = variantsExt[i].Values.FirstOrDefault(x => x.IsSelected);
                    selectedIds[i] = (selectedVariant == null ? string.Empty : selectedVariant.Value);
                }
            }

            List<VariantRegistration> availableVariants = new List<VariantRegistration>();
            availableVariants = variantsRegistration.Where(x =>
                (string.IsNullOrEmpty(selectedIds[0]) || x.Dimension1 == selectedIds[0])
                && (string.IsNullOrEmpty(selectedIds[1]) || x.Dimension2 == selectedIds[1])
                && (string.IsNullOrEmpty(selectedIds[2]) || x.Dimension3 == selectedIds[2])
                && (string.IsNullOrEmpty(selectedIds[3]) || x.Dimension4 == selectedIds[3])
                && (string.IsNullOrEmpty(selectedIds[4]) || x.Dimension5 == selectedIds[4])
                && (string.IsNullOrEmpty(selectedIds[5]) || x.Dimension6 == selectedIds[5])).ToList();

            if (availableVariants.Count == 1)
            {
                return availableVariants[0];
            }
            else
            {
                return null;
            }
        }

        public List<ExtendedVariant> GetSelectedVariants(List<ExtendedVariant> extendedVariants)
        {
            List<ExtendedVariant> selectedVariants = new List<ExtendedVariant>();
            foreach (ExtendedVariant extendedVariant in extendedVariants)
            {
                if (GetVariantDimension(extendedVariant.Dimension) == extendedVariant.Value)
                {
                    selectedVariants.Add(extendedVariant);
                }
            }
            return selectedVariants;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            if (string.IsNullOrEmpty(Dimension1) == false)
                str.Append(Dimension1);
            if (string.IsNullOrEmpty(Dimension2) == false)
                str.Append(" - " + Dimension2);
            if (string.IsNullOrEmpty(Dimension3) == false)
                str.Append(" - " + Dimension3);
            if (string.IsNullOrEmpty(Dimension4) == false)
                str.Append(" - " + Dimension4);
            if (string.IsNullOrEmpty(Dimension5) == false)
                str.Append(" - " + Dimension5);
            if (string.IsNullOrEmpty(Dimension6) == false)
                str.Append(" - " + Dimension6);

            return str.ToString();
        }

        public virtual object Clone()
        {
            // Just returning a shallow copy for now
            return this.MemberwiseClone();
        }

        #endregion
    }
}
