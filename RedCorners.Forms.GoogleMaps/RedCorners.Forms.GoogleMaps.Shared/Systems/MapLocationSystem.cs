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

        /// <summary>
        /// Returns distance in kilometers
        /// </summary>
        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            return GetDistance(
                new Position (lat1, lng1),
                new Position (lat2, lng2));
        }

        public static double GetDistance(Position p1, Position p2)
        {
#if __ANDROID__
            var location1 = new global::Android.Locations.Location("locationA");
            var location2 = new global::Android.Locations.Location("locationB");
            location1.Latitude = p1.Latitude;
            location1.Longitude = p1.Longitude;
            location2.Latitude = p2.Latitude;
            location2.Longitude = p2.Longitude;
            return location1.DistanceTo(location2) / 1000.0;
#elif __IOS__
			var l1 = new CoreLocation.CLLocation(p1.Latitude, p1.Longitude);
			var l2 = new CoreLocation.CLLocation(p2.Latitude, p2.Longitude);
            return l1.DistanceFrom(l2) / 1000.0;
#else
            throw new NotImplementedException("Platform not implemented.");
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

        public double GetDistance(double lat, double lng)
        {
            return GetDistance(lat, lng, Latitude.GetValueOrDefault(), Longitude.GetValueOrDefault());
        }

        public double GetDistance(Position p)
        {
            return GetDistance(p, new Position (Latitude.GetValueOrDefault(), Longitude.GetValueOrDefault()));
        }
    }
}
