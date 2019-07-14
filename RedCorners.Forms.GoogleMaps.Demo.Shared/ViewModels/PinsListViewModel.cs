using System;
using System.Text;
using System.Linq;
using RedCorners.Forms;
using RedCorners.Models;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Collections.ObjectModel;

namespace RedCorners.Forms.GoogleMaps.Demo.ViewModels
{
    public class PinsListViewModel : BindableModel
    {
        //public List<object> Items { get; set; } = new List<object>();
        public ObservableCollection<object> Items { get; set; } = new ObservableCollection<object>();

        public PinsListViewModel()
        {
            Status = TaskStatuses.Success;
        }

        public Command AddCircleCommand => new Command(() =>
        {
            var circ = new Circle
            {
                Center = RandPos(),
                FillColor = RandColor(),
                Radius = Distance.FromKilometers(50),
                StrokeColor = Color.White,
                StrokeWidth = 3,
                Command = TapCommand
            };
            circ.CommandParameter = circ;
            Items.Add(circ);

            //Items = Items.ToList();
            UpdateProperties();
        });

        public Command AddPinCommand => new Command(() =>
        {
            var pin = new Pin
            {
                Address = "Test",
                Icon = BitmapDescriptorFactory.DefaultMarker(RandColor()),
                Label = "Test",
                Position = RandPos(),
                Command = TapCommand
            };
            pin.CommandParameter = pin;
            Items.Add(pin);

            //Items = Items.ToList();
            UpdateProperties();
        });

        public Command AddLineCommand => new Command(() =>
        {
            var polyline = new Polyline
            {
                StrokeColor = RandColor(),
                StrokeWidth = 3
            };
            polyline.Positions.Add(RandPos());
            polyline.Positions.Add(RandPos());
            Items.Add(polyline);

            //Items = Items.ToList();
            UpdateProperties();
        });

        public Command ClearCommand => new Command(() =>
        {
            Items = new ObservableCollection<object>();// new List<object>();
            UpdateProperties();
        });

        (double Latitude, double Longitude) center = (49.6232369, 6.0708212);
        double RandDouble(double amp) => amp * (2.0 * Random.NextDouble() - 1.0f);
        Position RandPos() => new Position(center.Latitude + RandDouble(50), center.Longitude + RandDouble(4));
        Color RandColor() => Color.FromHsla(Random.NextDouble(), 0.5, 0.5);

        public Command<object> TapCommand => new Command<object>(obj =>
        {
            App.Instance.DisplayAlert("Tapped", obj.GetType().FullName, "OK");
        });
    }
}
