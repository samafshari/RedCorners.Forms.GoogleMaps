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
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                if (bindable is MapObjectCollectionBase collection)
                {
                    if (oldVal != newVal)
                    {
                        collection.TriggerCollectionChange();
                    }
                }
            });

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

        protected virtual IEnumerable<MapObject> GetItems()
        {
            throw new Exception("Do not call base on GetItems(); override this.");
        }

        public IEnumerable<MapObject> GetVisibleItems(MapRegion region)
        {
            if (!IsVisible)
                return Enumerable.Empty<MapObject>();

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
