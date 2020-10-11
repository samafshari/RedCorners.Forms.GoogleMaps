using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps
{
    public abstract class MapObject : BindableObject
    {
        public MapObjectCollectionBase Owner { get; set; }
        public bool NeverCull { get; set; }

        public virtual bool ShouldCull(MapRegion region)
        {
            return false;
        }

        public virtual bool ShouldCull(Position position, Distance distance)
        {
            return false;
        }
    }
}
