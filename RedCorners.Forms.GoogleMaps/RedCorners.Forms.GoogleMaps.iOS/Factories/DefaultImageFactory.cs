using System;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace RedCorners.Forms.GoogleMaps.iOS.Factories
{
    // ReSharper disable once InconsistentNaming
    public sealed class DefaultImageFactory : IImageFactory
    {
        private static readonly Lazy<DefaultImageFactory> _instance
            = new Lazy<DefaultImageFactory>(() => new DefaultImageFactory());

        public static DefaultImageFactory Instance
        {
            get { return _instance.Value; }
        }
        
        private DefaultImageFactory()
        {
        }

        UIImage GetRaw(BitmapDescriptor descriptor)
        {
            switch (descriptor.Type)
            {
                case BitmapDescriptorType.Default:
                    return Google.Maps.Marker.MarkerImage(descriptor.Color.ToUIColor());
                case BitmapDescriptorType.Bundle:
                    return UIImage.FromBundle(descriptor.BundleName);
                case BitmapDescriptorType.Stream:
                    descriptor.Stream.Position = 0;
                    // Resize to screen scale
                    return UIImage.LoadFromData(NSData.FromStream(descriptor.Stream), UIScreen.MainScreen.Scale);
                case BitmapDescriptorType.AbsolutePath:
                    return UIImage.FromFile(descriptor.AbsolutePath);
                default:
                    return Google.Maps.Marker.MarkerImage(UIColor.Red);
            }
        }

        public UIImage ToUIImage(BitmapDescriptor descriptor)
        {
            var scale = descriptor.IconScale;
            if (scale == 0) scale = 1;
            
            var originalImage = GetRaw(descriptor);
            if (scale == 1) return originalImage;

            var newSize = new CGSize(originalImage.Size.Width * scale, originalImage.Size.Height * scale);
            return originalImage.Scale(newSize, originalImage.CurrentScale);
        }
    }
}