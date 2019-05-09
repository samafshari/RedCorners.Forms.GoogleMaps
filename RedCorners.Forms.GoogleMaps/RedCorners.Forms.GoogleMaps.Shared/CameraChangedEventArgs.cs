using System;

namespace RedCorners.Forms.GoogleMaps
{
    public sealed class CameraChangedEventArgs : EventArgs
    {
        public CameraPosition Position
        {
            get;
        }

        internal CameraChangedEventArgs(CameraPosition position)
        {
            Position = position;
        }
    }
}