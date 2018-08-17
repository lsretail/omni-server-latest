using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

namespace LSRetail.Omni.Domain.DataModel.Base.Utils
{
    public class ImageCache
    {
        public class ImageCacheItem
        {
            public string ItemId { get; set; }
            public string VariantId { get; set; }
            public ImageSize ImageSize { get; set; }
            public List<ImageView> Images { get; set; }
        }

        private List<ImageCacheItem> cachedImages = new List<ImageCacheItem>();
        private const int maxSize = 10000000;  //10MB
        private int totalSize;

        public void AddImageToCache(ImageCacheItem cacheItem)
        {
            lock (cachedImages)
            {
                ImageCacheItem existingItem = cachedImages.FirstOrDefault(x => x.ItemId == cacheItem.ItemId && x.VariantId == cacheItem.VariantId && x.ImageSize.Width == cacheItem.ImageSize.Width && x.ImageSize.Height == cacheItem.ImageSize.Height);
                if (existingItem != null)
                {
                    return;
                }

                cachedImages.Add(cacheItem);

                foreach (ImageView image in cacheItem.Images)
                {
                    if (!string.IsNullOrEmpty(image.Image))
                    {
                        totalSize += image.Image.Length * 3;
                    }
                }

                if (totalSize >= maxSize)
                {
                    while (totalSize >= maxSize && cachedImages.Count > 1)
                    {
                        ImageCacheItem item = cachedImages[0];
                        RemoveItemFromCache(item);
                    }
                }
            }
        }

        public List<ImageView> GetItemFromCache(string itemId, string variantId, ImageSize imageSize)
        {
            return cachedImages?.FirstOrDefault(cacheItem => cacheItem.ItemId == itemId && cacheItem.VariantId == variantId && cacheItem.ImageSize?.Width == imageSize.Width && cacheItem.ImageSize?.Height == imageSize.Height)?.Images;
        }

        private void RemoveItemFromCache(ImageCacheItem cacheItem)
        {
            foreach (ImageView itemImage in cacheItem.Images)
            {
                if (!string.IsNullOrEmpty(itemImage.Image))
                {
                    totalSize -= itemImage.Image.Length * 3;
                }
            }

            cachedImages.Remove(cacheItem);
        }

        public void Clear()
        {
            cachedImages.Clear();
            totalSize = 0;
        }
    }
}
