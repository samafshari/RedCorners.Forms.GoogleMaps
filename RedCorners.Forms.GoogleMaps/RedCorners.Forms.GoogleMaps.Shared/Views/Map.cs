using RedCorners.Forms.Systems;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps
{
    public class Map : MapBase
    {
        public Map()
        {
            MapLongClicked += Map2_MapLongClicked;
            MapClicked += Map2_MapClicked;
            PinClicked += Map2_PinClicked;
            SelectedPinChanged += Map2_SelectedPinChanged;
            CameraIdled += Map_CameraIdled;
        }

        private void Map_CameraIdled(object sender, CameraIdledEventArgs e)
        {
            if (!isUpdatingPin)
            {
                isUpdatingPin = true;
                CameraLatitude = e.Position.Target.Latitude;
                CameraLongitude = e.Position.Target.Longitude;
                CameraUpdateZoomLevel = (int)e.Position.Zoom;
                isUpdatingPin = false;
            }

            if (MapIdledCommand?.CanExecute(e.Position) ?? false)
                MapIdledCommand?.Execute(e.Position);
        }

        private void Map2_PinClicked(object sender, PinClickedEventArgs e)
        {
            if (PinClickCommand?.CanExecute(e.Pin) ?? false)
                PinClickCommand.Execute(e.Pin);
        }

        private void Map2_SelectedPinChanged(object sender, SelectedPinChangedEventArgs e)
        {
            if (SelectedPinChangeCommand?.CanExecute(e.SelectedPin) ?? false)
                SelectedPinChangeCommand.Execute(e.SelectedPin);
        }

        private void Map2_MapClicked(object sender, MapClickedEventArgs e)
        {
            if (MapClickCommand?.CanExecute(e.Point) ?? false)
                MapClickCommand.Execute(e.Point);
        }

        private void Map2_MapLongClicked(object sender, MapLongClickedEventArgs e)
        {
            if (MapLongClickCommand?.CanExecute(e.Point) ?? false)
                MapLongClickCommand.Execute(e.Point);
        }

        public ICommand PinClickCommand
        {
            get => (ICommand)GetValue(PinClickCommandProperty);
            set => SetValue(PinClickCommandProperty, value);
        }

        public ICommand SelectedPinChangeCommand
        {
            get => (ICommand)GetValue(SelectedPinChangeCommandProperty);
            set => SetValue(SelectedPinChangeCommandProperty, value);
        }

        public ICommand MapClickCommand
        {
            get => (ICommand)GetValue(MapClickCommandProperty);
            set => SetValue(MapClickCommandProperty, value);
        }

        public ICommand MapLongClickCommand
        {
            get => (ICommand)GetValue(MapLongClickCommandProperty);
            set => SetValue(MapLongClickCommandProperty, value);
        }

        public ICommand MapIdledCommand
        {
            get => (ICommand)GetValue(MapIdledCommandProperty);
            set => SetValue(MapIdledCommandProperty, value);
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

        public int CameraUpdateZoomLevel
        {
            get => (int)GetValue(CameraUpdateZoomLevelProperty);
            set => SetValue(CameraUpdateZoomLevelProperty, value);
        }

        public double CameraPathDefaultDistance
        {
            get => (double)GetValue(CameraPathDefaultDistanceProperty);
            set => SetValue(CameraPathDefaultDistanceProperty, value);
        }

        protected bool FreeCamera = false;

        protected static void UpdatePin(BindableObject bindable, object oldVal, object newVal)
        {
            if (bindable is Map view)
                view.UpdatePin();
        }

        public static readonly BindableProperty CameraLatitudeProperty = BindableProperty.Create(
            nameof(CameraLatitude),
            typeof(double),
            typeof(Map),
            MapLocationSystem.Instance.Latitude,
            defaultBindingMode: BindingMode.OneTime,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                if (bindable is Map map && !map.FreeCamera)
                    map.UpdatePin();
            });

        public static readonly BindableProperty CameraLongitudeProperty = BindableProperty.Create(
            nameof(CameraLongitude),
            typeof(double),
            typeof(Map),
            MapLocationSystem.Instance.Longitude,
            defaultBindingMode: BindingMode.OneTime,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                if (bindable is Map map && !map.FreeCamera)
                    map.UpdatePin();
            });

        public static readonly BindableProperty PinClickCommandProperty = BindableProperty.Create(
            nameof(PinClickCommand),
            typeof(ICommand),
            typeof(Map),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValue: null);

        public static readonly BindableProperty SelectedPinChangeCommandProperty = BindableProperty.Create(
            nameof(SelectedPinChangeCommand),
            typeof(ICommand),
            typeof(Map),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValue: null);

        public static readonly BindableProperty MapClickCommandProperty = BindableProperty.Create(
            nameof(MapClickCommand),
            typeof(ICommand),
            typeof(Map),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValue: null);

        public static readonly BindableProperty MapLongClickCommandProperty = BindableProperty.Create(
            nameof(MapLongClickCommand),
            typeof(ICommand),
            typeof(Map),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValue: null);

        public static readonly BindableProperty MapIdledCommandProperty = BindableProperty.Create(
            nameof(MapIdledCommand),
            typeof(ICommand),
            typeof(Map),
            defaultBindingMode: BindingMode.TwoWay,
            defaultValue: null);

        public static readonly BindableProperty CameraUpdateZoomLevelProperty = BindableProperty.Create(
            nameof(CameraUpdateZoomLevel),
            typeof(int),
            typeof(Map),
            14,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: UpdatePin);

        public static readonly BindableProperty CameraPathDefaultDistanceProperty = BindableProperty.Create(
            nameof(CameraPathDefaultDistance),
            typeof(double),
            typeof(Map),
            0.5,
            BindingMode.TwoWay,
            propertyChanged: UpdatePin);


        public virtual void UpdateCamera(bool animate)
        {
            if (Width > 0 && Height > 0)
            {
                LogSystem.Instance.Log($"Focusing on {CameraLatitude}, {CameraLongitude}");
                this.CenterMap(CameraLatitude, CameraLongitude, CameraPathDefaultDistance, animate);
            }
            else
            {
                InitialCameraUpdate = new CameraUpdate(new Position(CameraLatitude, CameraLongitude), CameraUpdateZoomLevel);
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            UpdatePin();
        }

        bool isUpdatingPin = false;
        protected virtual void UpdatePin()
        {
            if (isUpdatingPin) return;
            isUpdatingPin = true;
            UpdateCamera(true);
            isUpdatingPin = false;
        }
    }
}
