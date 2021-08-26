using Newtonsoft.Json;
using System;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    public class LSRecommendToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }

        [JsonIgnore]
        public DateTime lastCheck { get; set; }
    }
}
