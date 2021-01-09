using Neat.Map.Models;

using System;

namespace RedCorners.Forms.GoogleMaps
{
    public sealed class MapLongClickedEventArgs : EventArgs
    {
        public Position Point { get; }

        internal MapLongClickedEventArgs(Position point)
        {
            this.Point = point;
        }
    }
}