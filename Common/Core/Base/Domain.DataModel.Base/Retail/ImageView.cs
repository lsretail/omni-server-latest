using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using LSRetail.Omni.Domain.DataModel.Base.Base;

namespace LSRetail.Omni.Domain.DataModel.Base.Retail
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum LocationType
    {
        [EnumMember]
        File = 0,
        [EnumMember]
        Image = 1,
        [EnumMember]
        Url = 2,
        [EnumMember]
        NoImage = 3,
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ImageSize : Entity, IDisposable
    {
        public ImageSize(int width, int height) : base("")
        {
            Width = width;
            Height = height;
            UseMinHorVerSize = false;
        }

        public ImageSize(int width, int height, bool useMinHorVerSize)
        {
            Width = width;
            Height = height;
            UseMinHorVerSize = useMinHorVerSize;
        }

        public ImageSize() : base("")
        {
            Width = 0;
            Height = 0;
            UseMinHorVerSize = false;
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

        public override string ToString()
        {
            return string.Format("{0}x{1}", Width, Height);
        }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        /// <summary>
        /// Return Image where size will not be less than Width or Height
        /// </summary>
        [DataMember]
        public bool UseMinHorVerSize { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ImageView : Entity, IDisposable
    {
        public ImageView(string id) : base(id)
        {
            Image = string.Empty;   // base64 string of image
            StreamURL = string.Empty; // if locationType is "URL" then this holds the URL to the image
            Location = string.Empty;
            DisplayOrder = 0;
            LocationType = LocationType.Image;
            ImgSize = new ImageSize(); // size of image as stored in database, 200x500 etc. client can determine
            Format = string.Empty; // JPEG, PNG
            AvgColor = string.Empty;
            ImgBytes = null;  // used on server only, not on client
        }

        public ImageView() : this(string.Empty)
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

        public ImageView Clone()
        {
            return new ImageView(Id)
            {
                DisplayOrder = DisplayOrder,
                MediaId = MediaId,
                Format = Format,
                Image = Image,
                ImgSize = ImgSize,
                StreamURL = StreamURL,
                Location = Location,
                Crossfade = Crossfade,
                LocationType = LocationType,
                ImgBytes = ImgBytes,
                AvgColor = AvgColor
            };
        }

        public string GetAvgColor()
        {
            if (Regex.Match(AvgColor, "^#(?:[0-9a-fA-F]{3}){1,2}$").Success)
                return AvgColor;
            return "#c7c1c9";
        }

        [DataMember]
        public Guid MediaId { get; set; }
        [DataMember]
        public int DisplayOrder { get; set; }
        [DataMember]
        public string Image { get; set; }
        [DataMember]
        public string StreamURL { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public LocationType LocationType { get; set; }
        [DataMember]
        public string AvgColor { get; set; }
        [DataMember]
        public ImageSize ImgSize { get; set; }
        [DataMember]
        public string Format { get; set; }

        //not all data goes to WCF clients
        public byte[] ImgBytes { get; set; }
        public DateTime ModifiedTime { get; set; }
        public bool Crossfade { get; set; }
    }
}
