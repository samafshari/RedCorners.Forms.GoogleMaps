using System;
using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps.Demo.ViewModels
{
    public class LocationPickerViewModel : BindableModel
    {
        double? _latitude, _longitude;
        public double? Latitude
        {
            get => _latitude;
            set => SetProperty(ref _latitude, value);
        }

        public double? Longitude
        {
            get => _longitude;
            set => SetProperty(ref _longitude, value);
        }

        Color _color = Color.Blue;
        public Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public Command RedCommand => new Command(() => Color = Color.Red);
        public Command GreenCommand => new Command(() => Color = Color.Green);
    }
}
