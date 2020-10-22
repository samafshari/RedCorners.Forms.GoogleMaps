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

        protected virtual IEnumerable<MapObject> GetItems()
        {
            throw new Exception("Do not call base on GetItems(); override this.");
        }

        public IEnumerable<MapObject> GetVisibleItems(MapRegion region)
        {
            if (region == null || !IsVisible)
                return Enumerable.Empty<MapObject>();

            var query = 
                GetItems()
                .Where(x => x.NeverCull ||!x.ShouldCull(region));

            if (MaxVisibleCount == null || MaxVisibleCount < 0)
                return query;

            var center = region.GetCenter();

            return
                query.OrderBy(x =>
                {
                    var relativePosition = x.GetRelativePosition(center);
                    if (relativePosition == null) return double.MaxValue;
                    return MapLocationSystem.CalculateDistance(relativePosition.Value, center).Meters;
                }).Take(MaxVisibleCount.Value);
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
