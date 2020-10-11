using System;
using System.Collections.Generic;
using System.Text;

namespace RedCorners.Forms.GoogleMaps
{
    public interface IMapObject
    {
        bool NeverCull { get; set; }

        bool ShouldCull(MapRegion region);
        bool ShouldCull(Position position, Distance distance);
    }
}
