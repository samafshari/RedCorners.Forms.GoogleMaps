using RedCorners.Forms.Systems;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using System.Linq;

namespace RedCorners.Forms.GoogleMaps
{
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

        public double CameraLatitude
        {
            get => (double)GetValue(CameraLatitudeProperty);
            set => SetValue(CameraLatitudeProperty, value);
        }

        public double CameraLongitude
        {
            get => (double)GetValue(CameraLongitudeProperty);
            set => SetValue(CameraLongitudeProperty, value);
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

        public static readonly BindableProperty CameraLatitudeProperty = BindableProperty.Create(
            nameof(CameraLatitude),
            typeof(double),
            typeof(LocationPickerView),
            MapLocationSystem.Instance.Latitude,
            defaultBindingMode: BindingMode.OneTime,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                (bindable as LocationPickerView)?.UpdatePin();
            });

        public static readonly BindableProperty CameraLongitudeProperty = BindableProperty.Create(
            nameof(CameraLongitude),
            typeof(double),
            typeof(LocationPickerView),
            MapLocationSystem.Instance.Longitude,
            defaultBindingMode: BindingMode.OneTime,
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
            defaultValue: BitmapDescriptor.DefaultMarker(Color.Red, "Default"));

        public LocationPickerView()
        {
            MapLongClicked += Map_MapLongClicked;
        }

        bool isFirstTime = true;

        public override void Start()
        {
            base.Start();
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
        protected virtual void UpdatePin()
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
                    {
                        animate = false;
                        pin = new Pin
                        {
                            Label = PinLabel,
                            Address = PinAddress,
                            Icon = PinIcon,
                            Position = new Position(latitude, longitude)
                        };
                        Pins.Add(pin);
                    }
                    else
                        pin.Position = new Position(latitude, longitude);

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
            CenterOnPin(animate);
            isUpdatingPin = false;
        }

        private void Map_MapLongClicked(object sender, MapLongClickedEventArgs e)
        {
            Console.WriteLine($"Map LongClicked: {e.Point}");
            (Latitude, Longitude) = (e.Point.Latitude, e.Point.Longitude);
            if (LocationPickCommand?.CanExecute(e.Point) ?? false)
            {
                Console.WriteLine($"Executing LocationPickCommand: {e.Point}");
                LocationPickCommand?.Execute(e.Point);
            }
            Console.WriteLine($"End Map LongClicked: {e.Point}");
        }

        public void CenterOnPin(bool animate)
        {
            if (Width > 0 && Height > 0)
            {
                LogSystem.Instance.Log($"Focusing on {CameraLatitude}, {CameraLongitude}");
                this.CenterMap(CameraLatitude, CameraLongitude, 0.5, animate);
            }
            else
            {
                InitialCameraUpdate = new CameraUpdate(new Position(CameraLatitude, CameraLongitude), 14.0);
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            UpdatePin();
        }
    }
}
