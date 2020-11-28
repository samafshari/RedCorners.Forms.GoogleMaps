using System;
using System.Collections.Generic;
using System.Text;

namespace RedCorners.Forms.GoogleMaps
{
    public class MapLocationSystem
    {
        public delegate void LocationChangeDelegate(Position oldPosition, Position newPosition);
        public event LocationChangeDelegate OnLocationChanged;
        public static MapLocationSystem Instance { get; private set; } = new MapLocationSystem();
        MapLocationSystem() { }

        public event EventHandler<Position> OnMapLocationChanged;

        public static Distance CalculateDistance(Position p1, Position p2)
        {
            return Distance.FromMeters(CalculateDistanceInMeters(p1, p2));
        }

        public static Distance CalculateDistance(double lat1, double lng1, double lat2, double lng2)
        {
            return CalculateDistance(
                new Position(lat1, lng1),
                new Position(lat2, lng2));
        }

        static double CalculateDistanceInMeters(Position p1, Position p2)
        {
#if __ANDROID__
            var location1 = new global::Android.Locations.Location("locationA");
            var location2 = new global::Android.Locations.Location("locationB");
            location1.Latitude = p1.Latitude;
            location1.Longitude = p1.Longitude;
            location2.Latitude = p2.Latitude;
            location2.Longitude = p2.Longitude;
            return location1.DistanceTo(location2);
#elif __IOS__
			var l1 = new CoreLocation.CLLocation(p1.Latitude, p1.Longitude);
			var l2 = new CoreLocation.CLLocation(p2.Latitude, p2.Longitude);
            return l1.DistanceFrom(l2);
#else
            var d1 = p1.Latitude * (Math.PI / 180.0);
            var num1 = p1.Longitude * (Math.PI / 180.0);
            var d2 = p2.Latitude * (Math.PI / 180.0);
            var num2 = p2.Longitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
#endif
        }

        public bool HasAccurateMapLocation { get; private set; } = false;

        public Position Model { get; private set; }

        double? _mapLatitude = null;
        double? _mapLongitude = null;
        public double? Latitude
        {
            get => _mapLatitude;
            set
            {
                _mapLatitude = value;
                if (Model != null)
                    OnMapLocationChanged?.Invoke(this, Model);
            }
        }

        public double? Longitude
        {
            get => _mapLongitude;
            set
            {
                _mapLongitude = value;
                if (Model != null)
                    OnMapLocationChanged?.Invoke(this, Model);
            }
        }

        public double DefaultLatitude { get; set; } = 41.89;
        public double DefaultLongitude { get; set; } = 12.49;
        public int DefaultZoom { get; set; } = 10;

        public double CurrentOrDefaultLatitude => Latitude ?? CurrentOrDefaultLatitude;
        public double CurrentOrDefaultLongitude => Longitude ?? CurrentOrDefaultLongitude;

        public void InjectModel(Position position)
        {
            var oldModel = Model;
            Model = position;
            OnLocationChanged?.Invoke(oldModel, position);
        }

        public void InjectMapModel(double latitude, double longitude)
        {
            _mapLatitude = latitude;
            _mapLongitude = longitude;

            HasAccurateMapLocation = true;

            InjectModel(new Position(latitude, longitude));

            if (Model != null)
                OnMapLocationChanged?.Invoke(this, Model);
        }

        public Distance CalculateDistance(double lat, double lng)
        {
            return CalculateDistance(lat, lng, Latitude.GetValueOrDefault(), Longitude.GetValueOrDefault());
        }

        public Distance CalculateDistance(Position p)
        {
            return CalculateDistance(p, new Position (Latitude.GetValueOrDefault(), Longitude.GetValueOrDefault()));
        }
    }
}
