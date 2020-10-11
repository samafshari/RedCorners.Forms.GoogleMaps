using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedCorners.Forms.GoogleMaps
{
    public static class MapRegionExtensions
    {
        public static bool Contains(
            this MapRegion region, 
            Position position,
            bool edgeIsInside = true)
        {
            if (region == null) return false;

            var points = new[]
            {
                region.NearLeft,
                region.NearRight,
                region.FarLeft,
                region.FarRight
            };

            var min = (
                lat: points.Min(x => x.Latitude),
                lng: points.Min(x => x.Longitude));

            var max = (
                lat: points.Max(x => x.Latitude),
                lng: points.Max(x => x.Longitude));

            if (edgeIsInside)
                return
                    position.Latitude >= min.lat &&
                    position.Longitude >= min.lng &&
                    position.Latitude <= max.lat &&
                    position.Longitude <= max.lng;
            else
                return
                    position.Latitude > min.lat &&
                    position.Longitude > min.lng &&
                    position.Latitude < max.lat &&
                    position.Longitude < max.lng;
        }

        public static bool Contains(
            this MapRegion region,
            Position center,
            Distance radius,
            bool edgeIsInside = true)
        {
            throw new NotImplementedException();
        }

        public static bool Contains(
            this MapRegion region,
            Bounds bounds,
            bool edgeIsInside = true)
        {
            throw new NotImplementedException();
        }
    }
}
