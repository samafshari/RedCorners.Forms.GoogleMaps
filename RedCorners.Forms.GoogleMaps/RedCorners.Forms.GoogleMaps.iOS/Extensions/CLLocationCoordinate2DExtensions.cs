using System;
using CoreLocation;

using Neat.Map.Models;

namespace RedCorners.Forms.GoogleMaps.iOS.Extensions
{
    internal static class CLLocationCoordinate2DExtensions
    {
        public static Position ToPosition(this CLLocationCoordinate2D self)
        {
            return new Position(self.Latitude, self.Longitude);
        }
    }
}


