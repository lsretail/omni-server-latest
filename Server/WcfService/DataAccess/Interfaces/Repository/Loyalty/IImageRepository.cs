using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IImageRepository
    {
        List<ImageView> ItemImagesByItemId(string itemId);
        void SaveImage(ImageView iv);
        void SaveImageLink(ImageView iv,string tableName, string recordId, string keyValue, string imgId, int displayOrder);
        List<ImageView> NotificationImagesById(string notificationId, Statistics stat);
    }
}
