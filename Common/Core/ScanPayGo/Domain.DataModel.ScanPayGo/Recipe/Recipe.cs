using System;
using System.Collections.Generic;
using ImageView = LSRetail.Omni.Domain.DataModel.Base.Retail.ImageView;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Recipe
{
    //Recipes
    public class Recipe
    {
        public int MinutesToCook { get; set; }
        public int Calories { get; set; }
        public List<RecipeLine> RecipeLine { get; set; }
        public ImageView Image { get; set; }
        public string Description { get; set; }
        public string CookingDescription { get; set; }
    }
}
