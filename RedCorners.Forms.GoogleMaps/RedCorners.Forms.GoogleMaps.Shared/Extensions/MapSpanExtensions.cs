using System;
namespace RedCorners.Forms.GoogleMaps
{
    public static class MapSpanExtensions
    {
        public static Bounds ToBounds(this MapSpan self)
        {
            var halfLat = self.LatitudeDegrees / 2d;
            var halfLong = self.LongitudeDegrees / 2d;

            var southWest = new Position(self.Center.Latitude - halfLat, self.Center.Longitude - halfLong);
            var northEast = new Position(self.Center.Latitude + halfLat, self.Center.Longitude + halfLong);

            return new Bounds(southWest, northEast);
        }

        public static void CenterMap(this Map map,
            double lat,
            double lng,
            double km,
            bool animate = false)
        {
            map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(lat, lng),
                Distance.FromKilometers(km)),
                animate: animate);
        }
    }
}
