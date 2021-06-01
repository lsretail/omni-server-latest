using System;
namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Recipe
{
    public class RecipeLine
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string UnitOfMeasure;
    }
}
