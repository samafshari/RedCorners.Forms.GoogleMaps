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
    public class MyLocationPageViewModel : BindableModel
    {

        public List<Pin> Items { get; set; } = new List<Pin>();

        public MyLocationPageViewModel()
        {
            Status = TaskStatuses.Success;
        }

        public Action<MapRegion> RegionChangeAction => (r) =>
        {
            Console.WriteLine($"[{DateTime.Now}] Region: {r.FarLeft}; {r.FarRight}; {r.NearLeft}; {r.NearRight}");
            Items = new[] { r.FarLeft, r.FarRight, r.NearLeft, r.NearRight }.Select(x => new Pin
            {
                Label = "Pin",
                Address = "Pin",
                Position = x,
                Icon = BitmapDescriptorFactory.DefaultMarker(Color.Red)
            }).ToList();
            RaisePropertyChanged(nameof(Items));
        };
    }
}
