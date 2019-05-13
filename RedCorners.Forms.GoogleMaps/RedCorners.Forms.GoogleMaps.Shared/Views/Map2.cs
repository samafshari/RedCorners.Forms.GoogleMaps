using RedCorners.Forms.Systems;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps
{
    public class Map2 : Map
    {
        public Map2()
        {
            MapLongClicked += Map2_MapLongClicked;
            MapClicked += Map2_MapClicked;
            PinClicked += Map2_PinClicked;
            SelectedPinChanged += Map2_SelectedPinChanged;
        }

        private void Map2_PinClicked(object sender, PinClickedEventArgs e)
        {
        }

        private void Map2_SelectedPinChanged(object sender, SelectedPinChangedEventArgs e)
        {
        }

        private void Map2_MapClicked(object sender, MapClickedEventArgs e)
        {
        }

        private void Map2_MapLongClicked(object sender, MapLongClickedEventArgs e)
        {
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

        public static readonly BindableProperty CameraLatitudeProperty = BindableProperty.Create(
            nameof(CameraLatitude),
            typeof(double),
            typeof(Map2),
            MapLocationSystem.Instance.Latitude,
            defaultBindingMode: BindingMode.OneTime,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                (bindable as Map2)?.UpdatePin();
            });

        public static readonly BindableProperty CameraLongitudeProperty = BindableProperty.Create(
            nameof(CameraLongitude),
            typeof(double),
            typeof(Map2),
            MapLocationSystem.Instance.Longitude,
            defaultBindingMode: BindingMode.OneTime,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                (bindable as Map2)?.UpdatePin();
            });

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

        bool isUpdatingPin = false;
        protected virtual void UpdatePin()
        {
            if (isUpdatingPin) return;
            isUpdatingPin = true;
            bool animate = true;
            
            CenterOnPin(animate);
            isUpdatingPin = false;
        }
    }
}
