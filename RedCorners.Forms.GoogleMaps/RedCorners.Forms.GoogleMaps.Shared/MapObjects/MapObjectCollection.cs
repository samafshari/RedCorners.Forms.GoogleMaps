using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedCorners.Forms.GoogleMaps
{
    public class MapObjectCollection : MapObjectCollectionBase
    {
        readonly ConcurrentDictionary<MapObject, int> items = new ConcurrentDictionary<MapObject, int>();

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
            Push();
        }

        public void Add(IEnumerable<MapObject> objects, bool triggerUpdate = true)
        {
            if (objects == null) return;

            bool any = false;
            foreach (var item in objects)
            {
                any = true;
                items.TryAdd(item, 0);
                Subscribe(item);
            }

            if (any && triggerUpdate)
                Push();
        }

        public void Add(MapObject o, bool triggerUpdate = true)
        {
            if (o == null) return;

            items.TryAdd(o, 0);
            Subscribe(o);
            if (triggerUpdate) Push();
        }

        public void Remove(MapObject o, bool triggerUpdate = true)
        {
            if (o == null)
                return;

            items.TryRemove(o, out var _);
            Unsubscribe(o);
            if (triggerUpdate) Push();
        }
        
        public void Remove(IEnumerable<MapObject> objects, bool triggerUpdate = true)
        {
            if (objects == null)
                return;

            bool any = false;
            foreach (var item in objects)
            {
                if (this.items.ContainsKey(item))
                {
                    any = true;
                    Remove(item, false);
                }
            }

            if (any && triggerUpdate)
                Push();
        }

        public void Sync(IEnumerable<MapObject> toRemove, IEnumerable<MapObject> toAdd, bool triggerUpdate = true)
        {
            bool any = false;
            if (toRemove != null)
            {
                foreach (var o in toRemove.ToList())
                {
                    any = true;
                    Remove(o, false);
                    Unsubscribe(o);
                }
            }
            if (toAdd != null)
            {
                foreach (var o in toAdd)
                {
                    any = true;
                    Add(o, false);
                    Subscribe(o);
                }
            }

            if (any && triggerUpdate)
                Push();
        }

        protected override IEnumerable<MapObject> GetItems()
        {
            return items.Keys.ToList();
        }

        void Push()
        {
            TriggerCollectionChange();
        }
    }
}
