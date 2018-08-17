using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Pos.Utils
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Pos/2017")]
    public class WarningMessage
    {
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public bool IsDismissed { get; set; }

        public WarningMessage(string message)
        {
            this.Message = message;
            this.IsDismissed = false;
        }

        public void Dismiss()
        {
            this.IsDismissed = true;
        }
    }
}
