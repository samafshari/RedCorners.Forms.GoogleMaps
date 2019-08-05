using RedCorners.Forms.Systems;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using System.Linq;

namespace RedCorners.Forms.GoogleMaps
{
    public enum LocationPickerClickTypes
    {
        Any,
        Click,
        LongClick
    }

    public class LocationPickerView : Map
    {
        public double? Latitude
        {
            get => (double?)GetValue(LatitudeProperty);
            set => SetValue(LatitudeProperty, value);
        }

        public double? Longitude
        {
            get => (double?)GetValue(LongitudeProperty);
            set => SetValue(LongitudeProperty, value);
        }

        public ICommand LocationPickCommand
        {
            get => (ICommand)GetValue(LocationPickCommandProperty);
            set => SetValue(LocationPickCommandProperty, value);
        }

        public string PinLabel
        {
            get => (string)GetValue(PinLabelProperty);
            set => SetValue(PinLabelProperty, value);
        }

        public string PinAddress
        {
            get => (string)GetValue(PinAddressProperty);
            set => SetValue(PinAddressProperty, value);
        }

        public BitmapDescriptor PinIcon
        {
            get => (BitmapDescriptor)GetValue(PinIconProperty);
            set => SetValue(PinIconProperty, value);
        }

        public LocationPickerClickTypes ClickType
        {
            get => (LocationPickerClickTypes)GetValue(ClickTypeProperty);
            set => SetValue(ClickTypeProperty, value);
        }

        public static readonly BindableProperty LatitudeProperty = BindableProperty.Create(
            nameof(Latitude),
            typeof(double?),
            typeof(LocationPickerView),
            null,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                (bindable as LocationPickerView)?.UpdatePin();
            });

        public static readonly BindableProperty LongitudeProperty = BindableProperty.Create(
            nameof(Longitude),
            typeof(double?),
            typeof(LocationPickerView),
            null,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                (bindable as LocationPickerView)?.UpdatePin();
            });

        public static readonly BindableProperty LocationPickCommandProperty = BindableProperty.Create(
            nameof(LocationPickCommand),
            typeof(ICommand),
            typeof(LocationPickerView),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValue: null);

        public static readonly BindableProperty PinLabelProperty = BindableProperty.Create(
            nameof(PinLabel),
            typeof(string),
            typeof(LocationPickerView),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValue: "Marker");

        public static readonly BindableProperty PinAddressProperty = BindableProperty.Create(
            nameof(PinAddress),
            typeof(string),
            typeof(LocationPickerView),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValue: "");

        public static readonly BindableProperty PinIconProperty = BindableProperty.Create(
            nameof(PinIcon),
            typeof(BitmapDescriptor),
            typeof(LocationPickerView),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValue: BitmapDescriptor.DefaultMarker(Color.Red, "Default"),
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                if (newVal is BitmapDescriptor descriptor && bindable is LocationPickerView view)
                {
                    if (descriptor.BindingContext == null)
                        descriptor.BindingContext = view.BindingContext;
                }
            });

        public static readonly BindableProperty ClickTypeProperty = BindableProperty.Create(
            nameof(ClickType),
            typeof(LocationPickerClickTypes),
            typeof(LocationPickerView),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValue: LocationPickerClickTypes.Any);

        object oldContext = null;
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (PinIcon != null && PinIcon.BindingContext == oldContext)
                PinIcon.BindingContext = BindingContext;

            oldContext = BindingContext;
        }

        public LocationPickerView()
        {
            MapLongClicked += Map_MapLongClicked;
            MapClicked += Map_MapClicked;
        }

        bool isFirstTime = true;

        public override void OnStart()
        {
            base.OnStart();
            if (isFirstTime)
            {
                isFirstTime = false;

                if (Latitude == null && Longitude == null)
                    MapLocationSystem.Instance.OnMapLocationChanged += Instance_OnMapLocationChanged;

                UpdatePin();
            }
        }

        private void Instance_OnMapLocationChanged(object sender, Position e)
        {
            MapLocationSystem.Instance.OnMapLocationChanged -= Instance_OnMapLocationChanged;
            if (Latitude.GetValueOrDefault() == 0 && Longitude.GetValueOrDefault() == 0)
            {
                (CameraLatitude, CameraLongitude) = (
                    e.Latitude,
                    e.Longitude
                    );
            }
        }

        bool isUpdatingPin = false;
        protected override void UpdatePin()
        {
            if (isUpdatingPin) return;
            isUpdatingPin = true;
            bool animate = true;
            if (Latitude.GetValueOrDefault() != 0 && Longitude.GetValueOrDefault() != 0)
            {
                var latitude = Latitude.Value;
                var longitude = Longitude.Value;

                try
                {
                    var pin = Pins.FirstOrDefault();
                    if (pin == null)
                        animate = false;
                    
                    Pins.Clear();
                    pin = new Pin
                    {
                        Label = PinLabel,
                        Address = PinAddress,
                        Icon = PinIcon,
                        Position = new Position(latitude, longitude)
                    };
                    Pins.Add(pin);

                    CameraLatitude = latitude;
                    CameraLongitude = longitude;
                }
                catch (Exception ex)
                {
                    LogSystem.Instance.Log(ex.ToString());
                }
            }
            else
            {
                if (Pins.Count > 0) Pins.Clear();
            }
            UpdateCamera(animate);
            isUpdatingPin = false;
        }

        private void Map_MapLongClicked(object sender, MapLongClickedEventArgs e)
        {
            if (ClickType == LocationPickerClickTypes.LongClick || ClickType == LocationPickerClickTypes.Any)
                Clicked(e.Point);
        }

        private void Map_MapClicked(object sender, MapClickedEventArgs e)
        {
            if (ClickType == LocationPickerClickTypes.Click || ClickType == LocationPickerClickTypes.Any)
                Clicked(e.Point);
        }

        void Clicked(Position point)
        {
            Console.WriteLine($"Map Clicked: {point}");
            (Latitude, Longitude) = (point.Latitude, point.Longitude);
            if (LocationPickCommand?.CanExecute(point) ?? false)
            {
                Console.WriteLine($"Executing LocationPickCommand: {point}");
                LocationPickCommand?.Execute(point);
            }
            Console.WriteLine($"End Map Clicked: {point}");
        }
    }
}
