using System;
using System.Collections.Generic;

namespace LSRetail.Omni.Domain.DataModel.ScanPayGo.Login
{
    public class Audkenni
    {
        [Serializable]
        public class Step6CallBack
        {
            public class Root
            {
                public string signature { get; set; }
                public string documentNr { get; set; }
                public string certificate { get; set; }
                public string nationalRegisterId { get; set; }
                public string name { get; set; }
                public string sub { get; set; }
            }
        }

        [Serializable]
        public class Step5CallBack
        {
            public class Root
            {
                public string access_token { get; set; }
                public string scope { get; set; }
                public string id_token { get; set; }
                public string token_type { get; set; }
                public int expires_in { get; set; }
            }
        }


        [Serializable]
        public class Step3CallBack
        {
            public class Root
            {
                public string tokenId { get; set; }
                public string successUrl { get; set; }
                public string realm { get; set; }
            }
        }

        [Serializable]

        public class Step2CallBack
        {
            public class Output
            {
                public string name { get; set; }
                public string value { get; set; }
            }

            public class Callback
            {
                public string type { get; set; }
                public List<Output> output { get; set; }
            }

            public class Root
            {
                public string authId { get; set; }
                public List<Callback> callbacks { get; set; }
            }
        }

        [Serializable]
        public class Step1CallBack
        {
            public class Output
            {
                public string name { get; set; }
                public object value { get; set; }
            }

            public class Input
            {
                public string name { get; set; }
                public object value { get; set; }
            }

            public class Callback
            {
                public string type { get; set; }
                public List<Output> output { get; set; }
                public List<Input> input { get; set; }
                public int _id { get; set; }
            }

            public class Root
            {
                public string authId { get; set; }
                public List<Callback> callbacks { get; set; }
            }
        }
    }
}
