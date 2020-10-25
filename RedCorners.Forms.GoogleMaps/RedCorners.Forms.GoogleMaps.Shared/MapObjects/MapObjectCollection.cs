using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedCorners.Forms.GoogleMaps
{
    public class MapObjectCollection : MapObjectCollectionBase
    {
        protected readonly HashSet<MapObject> Objects = new HashSet<MapObject>();
        public IEnumerable<MapObject> Items => GetItems();
        
        protected void Subscribe(MapObject o)
        {
            if (o is MapObjectCollectionBase collection)
                collection.CollectionChanged += Collection_CollectionChanged;
        }

        protected void Unsubscribe(MapObject o)
        {
            if (o is MapObjectCollectionBase collection)
                collection.CollectionChanged -= Collection_CollectionChanged;
        }

        void Collection_CollectionChanged(MapObjectCollectionBase collection)
        {
            TriggerCollectionChange();
        }

        public void Add(IEnumerable<MapObject> objects)
        {
            if (objects == null) return;

            bool any = false;
            foreach (var item in objects)
            {
                any = true;
                this.Objects.Add(item);
                Subscribe(item);
            }

            if (any)
                TriggerCollectionChange();
        }

        public void Add(MapObject o)
        {
            if (o == null) return;

            this.Objects.Add(o);
            Subscribe(o);
            TriggerCollectionChange();
        }

        public void Remove(MapObject o)
        {
            if (o == null || !Objects.Contains(o))
                return;

            Objects.Remove(o);
            Unsubscribe(o);
            TriggerCollectionChange();
        }

        public void Remove(IEnumerable<MapObject> objects)
        {
            if (objects == null)
                return;

            bool any = false;
            foreach (var item in objects)
            {
                if (this.Objects.Contains(item))
                {
                    any = true;
                    this.Objects.Remove(item);
                    Unsubscribe(item);
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
                    this.Objects.Remove(o);
                    Unsubscribe(o);
                }
            }
            if (toAdd != null)
            {
                foreach (var o in toAdd)
                {
                    any = true;
                    this.Objects.Add(o);
                    Subscribe(o);
                }
            }

            if (any)
                TriggerCollectionChange();
        }

        protected override IEnumerable<MapObject> GetItems()
        {
            return Objects.ToList();
        }
    }
}
