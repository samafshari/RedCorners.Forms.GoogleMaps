using UIKit;

namespace RedCorners.Forms.GoogleMaps.iOS.Factories
{
    public interface IImageFactory
    {
        UIImage ToUIImage(BitmapDescriptor descriptor);
    }
}