using System;
using System.Collections.Generic;
using System.Text;

namespace RedCorners.Forms.GoogleMaps
{
    public static class BitmapCache
    {
        static readonly Dictionary<string, BitmapDescriptor> cachedBitmaps = new Dictionary<string, BitmapDescriptor>();

        public static BitmapDescriptor GetBitmap(string icon, bool cache = false, string suffix = null)
        {
            if (suffix.HasValue() && !icon.EndsWith(suffix))
                icon += suffix;

            if (cache)
            {
                if (cachedBitmaps.ContainsKey(icon))
                {
                    return cachedBitmaps[icon];
                }
            }

            var bmp = BitmapDescriptorFactory.FromBundle(icon);

            if (cache)
                cachedBitmaps[icon] = bmp;

            return bmp;
        }

        public static void Clear()
        {
            cachedBitmaps.Clear();
        }
    }
}
