using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps
{
    public abstract class MapObjectCollectionBase : MapObject
    {
        public delegate void CollectionChangeDelegate(MapObjectCollectionBase collection);

        public event CollectionChangeDelegate CollectionChanged;

        bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    TriggerCollectionChange();
                }
            }
        }

        protected virtual IEnumerable<MapObject> GetItems()
        {
            throw new Exception("Do not call base on GetItems(); override this.");
        }

        public IEnumerable<MapObject> GetVisibleItems(MapRegion region)
        {
            return 
                GetItems()
                .Where(x => x.NeverCull ||!x.ShouldCull(region));
        }

        //public IEnumerable<MapObject> GetVisibleItems(Position center, Distance distance)
        //{
        //    return
        //        GetItems()
        //        .Where(x => x.NeverCull || !x.ShouldCull(center, distance));
        //}

        public void TriggerCollectionChange()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                CollectionChanged?.Invoke(this);
            });
        }

        public virtual void UpdateMapRegion(MapRegion region)
        {

        }
    }
}
