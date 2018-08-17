using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace Domain.Transactions
{
    public abstract class BaseLine : Entity
    {
        #region Constructors

        public BaseLine()
            : this(null)
        {
        }

        public BaseLine(string id)
            : base(id)
        {

        }

        #endregion
    }
}
