using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.Pos.Transactions.Calculators.Price
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public abstract class TransactionCalculator
    {
        public abstract TransactionActionResult TransactionCalc(RetailTransaction transRq);
    }
}
