using Android.Content;
using AndroidBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;

namespace RedCorners.Forms.GoogleMaps.Android.Factories
{
    public interface IBitmapDescriptorFactory
    {
        AndroidBitmapDescriptor ToNative(BitmapDescriptor descriptor, Context context);
    }
}