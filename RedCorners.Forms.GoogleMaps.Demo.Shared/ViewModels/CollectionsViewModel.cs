using System;
using System.Text;
using System.Linq;
using RedCorners.Forms;
using RedCorners.Models;
using System.Collections.Generic;
using Xamarin.Forms;
using RedCorners;
using System.Collections.ObjectModel;

namespace RedCorners.Forms.GoogleMaps.Demo.ViewModels
{
    public class CollectionsViewModel : BindableModel
    {
        public ObservableCollection<MapObject> MapObjects { get; set; } 
            = new ObservableCollection<MapObject>();

        public List<MapObjectCollection> Collections { get; set; }
        = new List<MapObjectCollection>();

        public CollectionsViewModel()
        {
            Status = TaskStatuses.Success;

            const double Radius = 10;
            Collections.Add(new OverpassCollection(new[] { "toilets" }, Radius)
            {
                Title = "Toilets",
                ImageSource = "em173",
                MakeIconFunc = _ => BitmapCache.GetBitmap("em173.png"),
                MaxVisibleCount = 100
            });
            Collections.Add(new OverpassCollection(new[] { "recycling" }, Radius)
            {
                Title = "Recycling",
                ImageSource = "em157",
                MakeIconFunc = _ => BitmapCache.GetBitmap("em157.png"),
                MaxVisibleCount = 100
            });

            var root = new MapObjectCollection()
            {
                Title = "Root",
                MaxVisibleCount = 10
            };
            root.Add(Collections.ToList());
            Collections.Add(root);

            MapObjects.Add(root);
        }
    }
}
