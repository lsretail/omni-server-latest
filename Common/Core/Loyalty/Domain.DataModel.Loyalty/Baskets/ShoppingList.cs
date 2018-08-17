using System;
using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Baskets
{
    public class ShoppingList : Entity, IAggregateRoot
    {
        #region Member variables

        private string name;
        private string description;
        private bool current;
        private List<ShoppingListLine> lines;
        private DateTime? created;

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public bool Current
        {
            get { return current; }
            set { current = value; }
        }

        public List<ShoppingListLine> Lines
        {
		    get
		    {
		        return lines;
		    }
            set { lines = value; }
        }

        public DateTime? Created
        {
            get { return created; }
            set { created = value; }
        }

        public string DateToShortFormat
        {
            get
            {
                if (created == null)
                {
                    return string.Empty;
                }
                return created.Value.ToString("d") + " - " + created.Value.ToString("t");
            }
        }

        #endregion


        #region Constructors

        public ShoppingList()
            : this(null)
        {
        }

        public ShoppingList(string id)
            : base(id)
        {
            name = string.Empty;
            description = string.Empty;
            current = false;
            lines = new List<ShoppingListLine>();
            Created = null;
        }

        #endregion

        public ShoppingList ShallowCopy()
        {
            return (ShoppingList)MemberwiseClone();
        }

        private class StringNumericComparer : IComparer<string>
        {
            public int Compare(string s1, string s2)
            {
                if (IsNumeric(s1) && IsNumeric(s2))
                {
                    if (Convert.ToInt32(s1) > Convert.ToInt32(s2)) return 1;
                    if (Convert.ToInt32(s1) < Convert.ToInt32(s2)) return -1;
                    if (Convert.ToInt32(s1) == Convert.ToInt32(s2)) return 0;
                }

                if (IsNumeric(s1) && IsNumeric(s2) == false)
                    return -1;

                if (IsNumeric(s1) == false && IsNumeric(s2))
                    return 1;

                return string.Compare(s1, s2);
            }

            public static bool IsNumeric(object value)
            {
                try
                {
                    int i = Convert.ToInt32(value.ToString());
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
            }
        }
    }
}
