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

        public virtual Task<IEnumerable<MapObject>> GetItemsAsync()
        {
            throw new Exception("Do not call base on GetItemsAsync()");
        }

        public virtual async Task<IEnumerable<MapObject>> GetVisibleItemsAsync(MapRegion region)
        {
            return 
                (await GetItemsAsync())
                .Where(x => x.NeverCull ||!x.ShouldCull(region));
        }

        public virtual async Task<IEnumerable<MapObject>> GetVisibleItemsAsync(Position center, Distance distance)
        {
            return
                (await GetItemsAsync())
                .Where(x => x.NeverCull || !x.ShouldCull(center, distance));
        }

        public void TriggerCollectionChange()
        {
            CollectionChanged?.Invoke(this);
        }
    }
}
