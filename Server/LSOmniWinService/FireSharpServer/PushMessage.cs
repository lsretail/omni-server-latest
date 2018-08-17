using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseNet.Messaging;
using Newtonsoft.Json;

namespace LSOmni.FireSharpServer
{
    public class PushMessage : INotification
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }
        [JsonProperty(PropertyName = "sound")]
        public string Sound { get; set; }
        [JsonProperty(PropertyName = "click_action")]
        public string ClickAction { get; set; }
        [JsonProperty(PropertyName = "body_loc_key")]
        public string BodyLocKey { get; set; }
        [JsonProperty(PropertyName = "body_loc_args")]
        public string BodyLocArgs { get; set; }
        [JsonProperty(PropertyName = "title_loc_key")]
        public string TitleLocKey { get; set; }
        [JsonProperty(PropertyName = "title_loc_args")]
        public string TitleLocArgs { get; set; }
    }
}
