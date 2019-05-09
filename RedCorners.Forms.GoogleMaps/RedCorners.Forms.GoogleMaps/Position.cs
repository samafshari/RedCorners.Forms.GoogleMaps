using System;

namespace RedCorners.Forms.GoogleMaps
{
    public struct Position
    {
        public Position(double latitude, double longitude)
        {
            Latitude = Math.Min(Math.Max(latitude, -90.0), 90.0);
            Longitude = Math.Min(Math.Max(longitude, -180.0), 180.0);
        }

        public double Latitude { get; }

        public double Longitude { get; }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (obj is Position other)
            {
                return Latitude == other.Latitude && Longitude == other.Longitude;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Latitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Position left, Position right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !Equals(left, right);
        }

        public static Position operator -(Position left, Position right)
        {
            return new Position(left.Latitude - right.Latitude, left.Longitude - right.Longitude);
        }
    }
}