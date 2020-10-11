using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedCorners.Forms.GoogleMaps
{
    public abstract class MapObjectCollectionBase : MapObject
    {
        public delegate void CollectionChangeDelegate(MapObjectCollectionBase collection);

        public event CollectionChangeDelegate CollectionChanged;

        public virtual IEnumerable<MapObject> GetItems()
        {
            throw new Exception("Do not call base on GetItems()");
        }

        public virtual IEnumerable<MapObject> GetVisibleItems(MapRegion region)
        {
            return 
                GetItems()
                .Where(x => x.NeverCull ||!x.ShouldCull(region));
        }

        public virtual IEnumerable<MapObject> GetVisibleItems(Position center, Distance distance)
        {
            return
                GetItems()
                .Where(x => x.NeverCull || !x.ShouldCull(center, distance));
        }

        public void TriggerCollectionChange()
        {
            CollectionChanged?.Invoke(this);
        }
    }
}
