using System;
using System.Collections.Generic;
using System.Text;

namespace RedCorners.Forms.GoogleMaps
{
    public class MapObjectCollection : MapObjectCollectionBase
    {
        readonly HashSet<MapObject> objects = new HashSet<MapObject>();

        public void Add(IEnumerable<MapObject> objects)
        {
            if (objects == null) return;

            foreach (var item in objects)
            {
                item.Owner = this;
                this.objects.Add(item);
            }
        }
    }
}
