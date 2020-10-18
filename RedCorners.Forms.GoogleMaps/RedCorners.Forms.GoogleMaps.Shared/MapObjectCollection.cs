using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedCorners.Forms.GoogleMaps
{
    public class MapObjectCollection : MapObjectCollectionBase
    {
        readonly HashSet<MapObject> objects = new HashSet<MapObject>();

        public void Add(IEnumerable<MapObject> objects)
        {
            if (objects == null) return;

            bool any = false;
            foreach (var item in objects)
            {
                any = true;
                item.Owner = this;
                this.objects.Add(item);
            }

            if (any)
                TriggerCollectionChange();
        }

        public void Add(MapObject o)
        {
            if (o == null) return;

            o.Owner = this;
            this.objects.Add(o);

            TriggerCollectionChange();
        }

        public void Remove(MapObject o)
        {
            if (o == null || !objects.Contains(o))
                return;

            objects.Remove(o);

            TriggerCollectionChange();
        }

        public void Remove(IEnumerable<MapObject> objects)
        {
            if (objects == null)
                return;

            bool any = false;
            foreach (var item in objects)
            {
                if (this.objects.Contains(item))
                {
                    any = true;
                    this.objects.Remove(item);
                }
            }

            if (any)
                TriggerCollectionChange();
        }

        public void Sync(IEnumerable<MapObject> toRemove, IEnumerable<MapObject> toAdd)
        {
            bool any = false;
            if (toRemove != null)
            {
                foreach (var o in toRemove.ToList())
                {
                    any = true;
                    this.objects.Remove(o);
                }
            }
            if (toAdd != null)
            {
                foreach (var o in toAdd)
                {
                    any = true;
                    this.objects.Add(o);
                }
            }

            if (any)
                TriggerCollectionChange();
        }
    }
}
