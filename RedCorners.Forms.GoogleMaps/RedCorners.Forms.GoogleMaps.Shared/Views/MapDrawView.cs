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
    public class MapDrawView : Map
    {
        public MapDrawView()
        {
            MapLongClicked += Map_MapLongClicked;
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
        
        public Color CircleFillColor
        {
            get => (Color)GetValue(CircleFillColorProperty);
            set => SetValue(CircleFillColorProperty, value);
        }

        public Color StrokeColor
        {
            get => (Color)GetValue(StrokeColorProperty);
            set => SetValue(StrokeColorProperty, value);
        }

        public double CircleRadiusMeters
        {
            get => (double)GetValue(CircleRadiusMetersProperty);
            set => SetValue(CircleRadiusMetersProperty, value);
        }

        public double LastCircleRadiusMeters
        {
            get => (double)GetValue(LastCircleRadiusMetersProperty);
            set => SetValue(LastCircleRadiusMetersProperty, value);
        }

        public float StrokeWidth
        {
            get => (float)GetValue(StrokeWidthProperty);
            set => SetValue(StrokeWidthProperty, value);
        }

        public static readonly BindableProperty CircleFillColorProperty = BindableProperty.Create(
            nameof(CircleFillColor),
            typeof(Color),
            typeof(MapDrawView),
            Color.Red,
            BindingMode.TwoWay,
            propertyChanged: UpdatePath);

        public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(
            nameof(StrokeColor),
            typeof(Color),
            typeof(MapDrawView),
            Color.Red,
            BindingMode.TwoWay,
            propertyChanged: UpdatePath);

        public static readonly BindableProperty CircleRadiusMetersProperty = BindableProperty.Create(
            nameof(CircleRadiusMeters),
            typeof(double),
            typeof(MapDrawView),
            10.0,
            BindingMode.TwoWay,
            propertyChanged: UpdatePath);

        public static readonly BindableProperty LastCircleRadiusMetersProperty = BindableProperty.Create(
            nameof(LastCircleRadiusMeters),
            typeof(double),
            typeof(MapDrawView),
            20.0,
            BindingMode.TwoWay,
            propertyChanged: UpdatePath);

        public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(
            nameof(StrokeWidth),
            typeof(float),
            typeof(MapDrawView),
            2.0f,
            BindingMode.TwoWay,
            propertyChanged: UpdatePath);

        static void UpdatePath(BindableObject bindable, object oldVal, object newVal)
        {
            if (bindable is MapDrawView view)
                view.UpdatePin();
        }

        public static readonly BindableProperty PositionsProperty = BindableProperty.Create(
            nameof(Positions),
            typeof(ObservableCollection<Position>),
            typeof(MapDrawView),
            null,
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
                view.UpdatePin();
            });

        public static readonly BindableProperty PositionAddedCommandProperty = BindableProperty.Create(
            nameof(PositionAddedCommand),
            typeof(ICommand),
            typeof(MapDrawView),
            defaultBindingMode: BindingMode.TwoWay);

        void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdatePin();
        }

        private void Map_MapLongClicked(object sender, MapLongClickedEventArgs e)
        {
            FreeCamera = true;
            CameraLatitude = e.Point.Latitude;
            CameraLongitude = e.Point.Longitude;
            FreeCamera = false;

            if (Positions == null) Positions = new ObservableCollection<Position>();

            Positions.Add(e.Point);
            PositionAddedCommand?.Execute(Positions);
        }

        bool isUpdatingPath = false;
        protected override void UpdatePin()
        {
            if (isUpdatingPath) return;
            isUpdatingPath = true;

            bool animate = true;
            if (Positions != null)
            {
                try
                {
                    Polylines.Clear();
                    Circles.Clear();

                    var polyline = new Polyline();

                    var lastPosition = Positions.LastOrDefault();
                    foreach (var item in Positions)
                    {
                        polyline.Positions.Add(item);

                        Circle circle = new Circle();
                        circle.Center = item;
                        circle.FillColor = CircleFillColor;
                        circle.Radius = Distance.FromMeters(CircleRadiusMeters);

                        if (lastPosition == item)
                        {
                            circle.Radius = Distance.FromMeters(LastCircleRadiusMeters);
                        }

                        Circles.Add(circle);
                    }
                    polyline.StrokeColor = StrokeColor;
                    polyline.StrokeWidth = StrokeWidth;

                    if (polyline.Positions.Count > 1)
                        Polylines.Add(polyline);
                }
                catch (Exception ex)
                {
                    LogSystem.Instance.Log(ex.ToString());
                }
            }
            UpdateCamera(animate);
            isUpdatingPath = false;
        }

        public override void UpdateCamera(bool animate)
        {
            var viewLatitude = CameraLatitude;
            var viewLongitude = CameraLongitude;
            var cameraUpdate = new CameraUpdate(new Position(viewLatitude, viewLongitude), CameraUpdateZoomLevel);
            var distance = CameraPathDefaultDistance;

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
                this.CenterMap(viewLatitude, viewLongitude, distance, animate);
            }
            else
            {
                InitialCameraUpdate = cameraUpdate;
            }
        }
    }
}
