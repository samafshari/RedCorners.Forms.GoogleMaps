using System;
using Android.Gms.Maps.Model;
using RedCorners.Forms.GoogleMaps.Android.Extensions;

namespace RedCorners.Forms.GoogleMaps.Android
{
    internal static class VisibleRegionExtensions
    {
        public static MapRegion ToRegion(this VisibleRegion visibleRegion)
        {
            return new MapRegion(
                visibleRegion.NearLeft.ToPosition(),
                visibleRegion.NearRight.ToPosition(),
                visibleRegion.FarLeft.ToPosition(),
                visibleRegion.FarRight.ToPosition()
            );
        }        
    }
}
