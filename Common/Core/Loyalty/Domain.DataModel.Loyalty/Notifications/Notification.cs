using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Base;
using System.Xml.Serialization;
using Domain.Images;

namespace Domain.Notifications
{
    public class Notification : EntityBase, IAggregateRoot
    {
        #region Member variables

        private string contactId;
        private string primaryText;
        private string secondaryText;
        private DateTime? expiryDate;
        private bool selected;
        private NotificationStatus status;
        private List<ImageView> images;
        private string qrText;

        #endregion

        #region Properties

		[XmlAttribute("cid")]
        public string ContactId
        {
            get { return contactId; }
		    set
		    {
		        contactId = value;
		        NotifyPropertyChanged();
		    }
        }

		[XmlAttribute("pt")]
        public string PrimaryText
        {
            get { return primaryText; }
            set
            {
                primaryText = value;
                NotifyPropertyChanged();
            }
        }

		[XmlAttribute("st")]
        public string SecondaryText
        {
            get { return secondaryText; }
            set
            {
                secondaryText = value;
                NotifyPropertyChanged();
            }
        }

		[XmlElement("ed")]
        public DateTime? ExpiryDate
        {
            get { return expiryDate; }
            set
            {
                expiryDate = value;
                NotifyPropertyChanged();
            }
        }

		[XmlAttribute("ns")]
        public NotificationStatus Status
        {
            get { return status; }
            set
            {
                status = value;
                NotifyPropertyChanged();
            }
        }

		[XmlElement("imgs")]
        public List<ImageView> Images
        {
            get { return images; }
            set
            {
                images = value;
                NotifyPropertyChanged();
            }
        }

		[XmlAttribute("s")]
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                NotifyPropertyChanged();
            }
        }

        [XmlAttribute("qr")]
        public string QrText
        {
            get { return qrText; }
            set
            {
                qrText = value;
                NotifyPropertyChanged();
            }
        }

        [XmlIgnore]
        public ImageView DefaultImage
        {
            get
            {
                if (Images != null && Images.Count > 0)
                {
                    return Images[0];
                }

                return null;
            }
        }

        #endregion

        #region Constructors

        public Notification()
            : this(null)
        {
        }

        public Notification(string id)
            : base(id)
        {
            this.contactId = string.Empty;
            this.primaryText = string.Empty;
            this.secondaryText = string.Empty;
            this.expiryDate = null;
            this.status = 0;
            this.images = new List<ImageView>();
        }

        #endregion

        public Notification Clone()
        {
            return (Notification)this.MemberwiseClone();
        }

    }
}
