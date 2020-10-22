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
        public ObservableCollection<MapObject> Items { get; set; } 
            = new ObservableCollection<MapObject>();
        
        public CollectionsViewModel()
        {
            Status = TaskStatuses.Success;

            const double Radius = 10;
            Items.Add(new OverpassCollection(new[] { "toilets" }, Radius)
            {
                MakeIconFunc = _ => BitmapDescriptorFactory.DefaultMarker(Color.Green)
            });
            Items.Add(new OverpassCollection(new[] { "recycling" }, Radius)
            {
                MakeIconFunc = _ => BitmapDescriptorFactory.DefaultMarker(Color.Yellow)
            });
        }
    }
}
