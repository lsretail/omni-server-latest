using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ReplDataTranslationResponse : IDisposable
    {
        public ReplDataTranslationResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Texts = new List<ReplDataTranslation>();
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
                Texts.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplDataTranslation> Texts { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Loy/2017")]
    public class ReplDataTranslation : IDisposable
    {
        public ReplDataTranslation()
        {
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
            }
        }

        [DataMember]
        public bool IsDeleted { get; set; }
        /// <summary>
        /// Translation ID made up by NAV TableNo-FieldNo
        /// </summary>
        [DataMember]
        public string TranslationId { get; set; }
        /// <summary>
        /// NAV Primary Key of value to translate
        /// </summary>
        [DataMember]
        public string Key { get; set; }
        /// <summary>
        /// Language Code (ENG,ISL..)
        /// </summary>
        [DataMember]
        public string LanguageCode { get; set; }
        /// <summary>
        /// Translated Text
        /// </summary>
        [DataMember]
        public string Text { get; set; }
    }
}
