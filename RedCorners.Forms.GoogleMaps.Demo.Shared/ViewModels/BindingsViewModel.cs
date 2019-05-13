using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps.Demo.ViewModels
{
    public class BindingsViewModel : BindableModel
    {
        int _counter = 0;
        string _console = "";
        public string Console
        {
            get => _console;
            set => SetProperty(ref _console, $"[{_counter++}] {value}");
        }

        public Command<Pin> PinClickCommand => new Command<Pin>(pin =>
        {
            if (pin != null)
                Console += $"PinClickCommand {pin.Position.Latitude}, {pin.Position.Longitude}\n{Console}";
            else
                Console += $"PinClickCommand null\n";
        });

        public Command<Pin> SelectedPinChangeCommand => new Command<Pin>(pin =>
        {
            if (pin != null)
                Console += $"SelectedPinChangeCommand {pin.Position.Latitude}, {pin.Position.Longitude}\n{Console}";
            else
                Console += $"SelectedPinChangeCommand null\n";
        });

        public Command<Position> MapClickCommand => new Command<Position>(point =>
        {
            if (point != null)
                Console += $"MapClickCommand {point.Latitude}, {point.Longitude}\n{Console}";
            else
                Console += $"MapClickCommand null\n";
        });

        public Command<Position> MapLongClickCommand => new Command<Position>(point =>
        {
            if (point != null)
                Console += $"MapLongClickCommand {point.Latitude}, {point.Longitude}\n{Console}";
            else
                Console += $"MapLongClickCommand null\n";
        });
    }
}
