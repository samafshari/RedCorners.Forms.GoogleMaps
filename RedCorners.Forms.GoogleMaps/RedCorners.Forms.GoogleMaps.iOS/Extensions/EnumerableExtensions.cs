using System.Collections.Generic;
using Google.Maps;

using Neat.Map.Models;

namespace RedCorners.Forms.GoogleMaps.iOS.Extensions
{
    internal static class EnumerableExtensions
    {
        public static MutablePath ToMutablePath(this IEnumerable<Position> self)
        {
            var path = new MutablePath();
            foreach (var p in self)
            {
                path.AddLatLon(p.Latitude, p.Longitude);
            }
            return path;
        }
    }
}