using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using System.Linq;
using RedCorners.Forms.Systems;

namespace RedCorners.Forms.GoogleMaps
{
    public class MapDrawView : AliveContentView
    {
        readonly Map map;
        public MapDrawView()
        {
            map = new Map();
            map.MapLongClicked += Map_MapLongClicked;
            Content = map;
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

        public ObservableCollection<Position> Positions
        {
            get => (ObservableCollection<Position>)GetValue(PositionsProperty);
            set => SetValue(PositionsProperty, value);
        }

        public ICommand PositionAddedCommand
        {
            get => (ICommand)GetValue(PositionAddedCommandProperty);
            set => SetValue(PositionAddedCommandProperty, value);
        }

        public bool IsInteractive
        {
            get => (bool)GetValue(IsInteractiveProperty);
            set => SetValue(IsInteractiveProperty, value);
        }

        public static readonly BindableProperty IsInteractiveProperty = BindableProperty.Create(
            nameof(IsInteractive),
            typeof(bool),
            typeof(MapDrawView),
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                if (bindable is MapDrawView view)
                {
                    view.UpdatePath();
                }
            });

        bool freeCamera = false;

        public static readonly BindableProperty CameraLatitudeProperty = BindableProperty.Create(
            nameof(CameraLatitude),
            typeof(double),
            typeof(MapDrawView),
            MapLocationSystem.Instance.Latitude,
            defaultBindingMode: BindingMode.OneTime,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                if (bindable is MapDrawView view && !view.freeCamera)
                {
                    view.UpdatePath();
                }
            });

        public static readonly BindableProperty CameraLongitudeProperty = BindableProperty.Create(
            nameof(CameraLongitude),
            typeof(double),
            typeof(MapDrawView),
            MapLocationSystem.Instance.Longitude,
            defaultBindingMode: BindingMode.OneTime,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                if (bindable is MapDrawView view && !view.freeCamera)
                {
                    view.UpdatePath();
                }
            });

        public static readonly BindableProperty PositionsProperty = BindableProperty.Create(
            nameof(Positions),
            typeof(ObservableCollection<Position>),
            typeof(MapDrawView),
            defaultValue: null,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                MapDrawView view = bindable as MapDrawView;
                if (oldVal is ObservableCollection<Position> oldCollection)
                {
                    oldCollection.CollectionChanged -= view.Collection_CollectionChanged;
                }
                if (newVal is ObservableCollection<Position> newCollection)
                {
                    newCollection.CollectionChanged += view.Collection_CollectionChanged;
                }
                view.UpdatePath();
            });

        public static readonly BindableProperty PositionAddedCommandProperty = BindableProperty.Create(
            nameof(PositionAddedCommand),
            typeof(ICommand),
            typeof(MapDrawView),
            defaultBindingMode: BindingMode.TwoWay);

        void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdatePath();
        }

        private void Map_MapLongClicked(object sender, MapLongClickedEventArgs e)
        {
            if (!IsInteractive) return;

            freeCamera = true;
            CameraLatitude = e.Point.Latitude;
            CameraLongitude = e.Point.Longitude;
            freeCamera = false;

            if (Positions == null) Positions = new ObservableCollection<Position>();

            Positions.Add(e.Point);
            PositionAddedCommand?.Execute(Positions);
        }

        bool isUpdatingPath = false;
        void UpdatePath()
        {
            if (isUpdatingPath) return;
            isUpdatingPath = true;

            bool animate = true;
            if (Positions != null)
            {
                try
                {
                    map.Polylines.Clear();
                    map.Circles.Clear();

                    var polyline = new Polyline();

                    var lastPosition = Positions.LastOrDefault();
                    foreach (var item in Positions)
                    {
                        polyline.Positions.Add(item);

                        Circle circle = new Circle();
                        circle.Center = item;
                        circle.FillColor = Color.Red;
                        circle.Radius = Distance.FromMeters(10);

                        if (lastPosition == item)
                        {
                            circle.Radius = Distance.FromMeters(20);
                        }

                        map.Circles.Add(circle);
                    }
                    polyline.StrokeColor = Color.Red;
                    polyline.StrokeWidth = 2;

                    if (polyline.Positions.Count > 1)
                        map.Polylines.Add(polyline);
                }
                catch (Exception ex)
                {
                    LogSystem.Instance.Log(ex.ToString());
                }
            }
            UpdateCamera(animate);
            isUpdatingPath = false;
        }

        public void UpdateCamera(bool animate)
        {
            var viewLatitude = CameraLatitude;
            var viewLongitude = CameraLongitude;
            var cameraUpdate = new CameraUpdate(new Position(viewLatitude, viewLongitude), 14.0);
            var distance = 0.5;

            if (map.IsShowingUser != IsInteractive)
            {
                map.IsShowingUser = IsInteractive;
                map.HasZoomEnabled = IsInteractive;
            }

            if (Positions != null && Positions.Count > 0)
            {
                viewLatitude = Positions.Average(x => x.Latitude);
                viewLongitude = Positions.Average(x => x.Longitude);

                var minPos = new Position(Positions.Min(x => x.Latitude), Positions.Min(x => x.Longitude));
                var maxPos = new Position(Positions.Max(x => x.Latitude), Positions.Max(x => x.Longitude));
                cameraUpdate = CameraUpdateFactory.NewBounds(new Bounds(minPos, maxPos), 10);
                distance = MapLocationSystem.GetDistance(minPos.Latitude, minPos.Longitude, maxPos.Latitude, maxPos.Longitude) * 0.5;
                distance = Math.Max(1.0, distance);
            }

            if (Width > 0 && Height > 0)
            {
                LogSystem.Instance.Log($"Focusing on {viewLatitude}, {viewLongitude}");
                map.CenterMap(viewLatitude, viewLongitude, distance, animate);
            }
            else
            {
                map.InitialCameraUpdate = cameraUpdate;
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            UpdatePath();
        }
    }
}
