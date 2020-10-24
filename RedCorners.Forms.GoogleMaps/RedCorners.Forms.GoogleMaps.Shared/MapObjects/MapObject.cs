using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps
{
    public abstract class MapObject : BindableObject
    {
        //public MapObjectCollectionBase Owner { get; set; }
        public bool NeverCull { get; set; }
        public List<object> Tags { get; } = new List<object>();

        public object Tag
        {
            get => Tags.FirstOrDefault();
            set
            {
                if (Tags.Count == 0)
                    Tags.Add(value);
                else
                    Tags[0] = value;
            }
        }

        public virtual bool ShouldCull(MapRegion region)
        {
            return false;
        }

        public virtual bool ShouldCull(Position position, Distance distance)
        {
            return false;
        }

        internal virtual Position? GetRelativePosition(Position reference)
        {
            return reference;
        }

        internal virtual int Count(MapRegion region)
        {
            return 1;
        }
    }
}
