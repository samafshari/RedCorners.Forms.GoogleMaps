using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps
{
    public enum MapObjectCollectionTakeAlgorithm
    {
        Center,
        Top,
        Bottom,
        Random,
        Ignore
    }

    public abstract class MapObjectCollectionBase : MapObject
    {
        static readonly Random Random = new Random();

        MapRegion lastRegion;
        IEnumerable<MapObject> lastVisibleItems;
        public TimeSpan SettleThreshold { get; set; } = TimeSpan.FromSeconds(0.2);

        public delegate void CollectionChangeDelegate(MapObjectCollectionBase collection);
        public event CollectionChangeDelegate CollectionChanged;

        public bool LockUpdate = false;

        public bool IsVisible
        {
            get => (bool)GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(
            nameof(IsVisible),
            typeof(bool),
            typeof(MapObjectCollectionBase),
            true,
            propertyChanged: ConsiderUpdate);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(MapObjectCollectionBase));

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(MapObjectCollectionBase));
        public int? MaxVisibleCount
        {
            get => (int?)GetValue(MaxVisibleCountProperty);
            set => SetValue(MaxVisibleCountProperty, value);
        }

        public static readonly BindableProperty MaxVisibleCountProperty = BindableProperty.Create(
            nameof(MaxVisibleCount),
            typeof(int?),
            typeof(MapObjectCollectionBase),
            defaultValue: default(int?),
            propertyChanged: ConsiderUpdate);

        public MapObjectCollectionTakeAlgorithm TakeAlgorithm
        {
            get => (MapObjectCollectionTakeAlgorithm)GetValue(TakeAlgorithmProperty);
            set => SetValue(TakeAlgorithmProperty, value);
        }
        
        public static readonly BindableProperty TakeAlgorithmProperty = BindableProperty.Create(
            nameof(TakeAlgorithm),
            typeof(MapObjectCollectionTakeAlgorithm),
            typeof(MapObjectCollectionBase));

        protected virtual IEnumerable<MapObject> GetItems()
        {
            throw new Exception("Do not call base on GetItems(); override this.");
        }

        public IEnumerable<MapObject> GetVisibleItems(MapRegion region, bool flatten = false)
        {
            if (region == null || !IsVisible)
            {
                lastVisibleItems = Enumerable.Empty<MapObject>();
                return lastVisibleItems;
            }

            if (region == lastRegion && lastVisibleItems != null)
                return lastVisibleItems;

            lastRegion = region;

            var query = 
                GetItems()
                .Where(x => x.NeverCull ||!x.ShouldCull(region));

            if (flatten)
            {
                var originalQuery = query;
                foreach (var o in originalQuery.Where(x => x is MapObjectCollectionBase))
                {
                    var collection = o as MapObjectCollectionBase;
                    query = query.Union(collection.GetVisibleItems(region, true));
                }
                query = query.Where(x => !(x is MapObjectCollectionBase));
            }

            if (MaxVisibleCount == null || MaxVisibleCount < 0)
            {
                lastVisibleItems = query;
                return query;
            }

            var list = query.ToList();

            if (list.Count < MaxVisibleCount)
            {
                lastVisibleItems = list;
                return list;
            }

            var center = region.GetCenter();

            IEnumerable<MapObject> result = null;
            if (TakeAlgorithm == MapObjectCollectionTakeAlgorithm.Center)
            {
                result =
                    list.OrderBy(x =>
                    {
                        var relativePosition = x.GetRelativePosition(center);
                        if (relativePosition == null) return double.MaxValue;
                        return MapLocationSystem.CalculateDistance(relativePosition.Value, center).Meters;
                    }).Take(MaxVisibleCount.Value);
            }
            else if (TakeAlgorithm == MapObjectCollectionTakeAlgorithm.Top)
            {
                result = list.Take(MaxVisibleCount.Value);
            }
            else if (TakeAlgorithm == MapObjectCollectionTakeAlgorithm.Bottom)
            {
                result = list.TakeLast(MaxVisibleCount.Value);
            }
            else if (TakeAlgorithm == MapObjectCollectionTakeAlgorithm.Random)
            {
                result = list.OrderBy(x => Random.NextDouble()).Take(MaxVisibleCount.Value);
            }
            else 
                result = list;

            lastVisibleItems = result;
            return result;
        }

        override internal int Count(MapRegion region)
        {
            var visibleItems = GetVisibleItems(region);
            
            if (visibleItems == null)
                return 0;

            return visibleItems.Sum(x => x.Count(region));
        }

        //public IEnumerable<MapObject> GetVisibleItems(Position center, Distance distance)
        //{
        //    return
        //        GetItems()
        //        .Where(x => x.NeverCull || !x.ShouldCull(center, distance));
        //}

        volatile uint lastRequestId = 0;
        public void TriggerCollectionChange()
        {
            if (LockUpdate) return;
            if (SettleThreshold == TimeSpan.Zero)
                Push();
            else
            {
                Task.Run(async () =>
                {
                    var requestId = ++lastRequestId;
                    await Task.Delay(SettleThreshold);
                    if (requestId == lastRequestId)
                    {
                        Push();
                        lastRequestId = 0;
                    }
                });
            }
        }

        void Push()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lastRegion = null;
                lastVisibleItems = null;
                CollectionChanged?.Invoke(this);
            });
        }

        public virtual void UpdateMapRegion(MapRegion region)
        {
            foreach (var collection in GetVisibleItems(region, false).Where(x => x is MapObjectCollectionBase))
            {
                ((MapObjectCollectionBase)collection).UpdateMapRegion(region);
            }
        }

        static void ConsiderUpdate(object bindable, object oldVal, object newVal)
        {
            if (bindable is MapObjectCollectionBase collection)
            {
                if (oldVal != newVal)
                {
                    collection.TriggerCollectionChange();
                }
            }
        }
    }
}
