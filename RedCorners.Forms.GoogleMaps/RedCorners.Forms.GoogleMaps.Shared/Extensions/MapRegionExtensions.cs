using Neat.Map.Models;

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
            Position position)
        {
            if (region == null) return false;
            return region.ToBounds().Contains(position);
        }

        public static bool Contains(
            this MapRegion region,
            Position center,
            Distance _)
        {
            // TODO: Implement a better algorithm that uses Distance
            return region.Contains(center);
        }

        public static bool Contains(
            this MapRegion region,
            Bounds bounds)
        {
            if (region == null || bounds == null)
                return false;

            return region.ToBounds().Intersects(bounds);
        }

        public static Bounds ToBounds(this MapRegion region)
        {
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

            return new Bounds(
                new Position(min.lat, min.lng),
                new Position(max.lat, max.lng));
        }

        public static Position GetCenter(this MapRegion region)
        {
            var centerLat = 0.5 * (region.NearLeft.Latitude + region.FarRight.Latitude);
            var centerLng = 0.5 * (region.NearLeft.Longitude + region.FarRight.Longitude);
            return new Position(centerLat, centerLng);
        }
    }
}
