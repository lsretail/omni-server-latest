using Newtonsoft.Json;
using System;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    public class TokenS2S
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
    }
}
