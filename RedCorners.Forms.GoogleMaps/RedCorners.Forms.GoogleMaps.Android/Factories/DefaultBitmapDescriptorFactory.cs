using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using AndroidBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;
using AndroidBitmapDescriptorFactory = Android.Gms.Maps.Model.BitmapDescriptorFactory;

namespace RedCorners.Forms.GoogleMaps.Android.Factories
{
    public sealed class DefaultBitmapDescriptorFactory : IBitmapDescriptorFactory
    {
        private static readonly Lazy<DefaultBitmapDescriptorFactory> _instance
            = new Lazy<DefaultBitmapDescriptorFactory>(() => new DefaultBitmapDescriptorFactory());

        public static bool UseGoogleMapsFactory = false;

        public static DefaultBitmapDescriptorFactory Instance
        {
            get { return _instance.Value; }
        }
        
        private DefaultBitmapDescriptorFactory()
        {
        }
        
        public AndroidBitmapDescriptor ToNative(BitmapDescriptor descriptor, Context context)
        {
            return GetRaw(descriptor, context);
        }

        AndroidBitmapDescriptor GetRaw(BitmapDescriptor descriptor, Context context)
        {
            switch (descriptor.Type)
            {
                case BitmapDescriptorType.Default:
                    return AndroidBitmapDescriptorFactory.DefaultMarker((float)((descriptor.Color.Hue * 360f) % 360f));
                case BitmapDescriptorType.Bundle:
                    var bundleName = descriptor.BundleName;
                    if (!bundleName.Contains('.')) bundleName += ".png";
                    if (UseGoogleMapsFactory || descriptor.IconScale == 1 || descriptor.IconScale == 0)
                        return AndroidBitmapDescriptorFactory.FromAsset(bundleName);
                    return AndroidBitmapDescriptorFactory.FromBitmap(GetBitmapFromAsset(descriptor, context));
                case BitmapDescriptorType.Stream:
                    if (descriptor.Stream.CanSeek && descriptor.Stream.Position > 0)
                    {
                        descriptor.Stream.Position = 0;
                    }
                    return AndroidBitmapDescriptorFactory.FromBitmap(BitmapFactory.DecodeStream(descriptor.Stream));
                case BitmapDescriptorType.AbsolutePath:
                    return AndroidBitmapDescriptorFactory.FromPath(descriptor.AbsolutePath);
                default:
                    return AndroidBitmapDescriptorFactory.DefaultMarker();
            }
        }

        Bitmap GetBitmapFromAsset(BitmapDescriptor descriptor, Context context)
        {
            var assetName = descriptor.BundleName;
            AssetManager assetManager = context.Assets;
            using (var stream = assetManager.Open(assetName))
            {
                Bitmap bmp = BitmapFactory.DecodeStream(stream);
                if (descriptor.IconScale == 1 || descriptor.IconScale == 0)
                    return bmp;

                return Bitmap.CreateScaledBitmap(bmp, (int)((float)bmp.Width * descriptor.IconScale), (int)((float)bmp.Height * descriptor.IconScale), true);
            }
        }
    }
}
