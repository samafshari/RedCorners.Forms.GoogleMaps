using System;
using CoreGraphics;
using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps.iOS.Extensions
{
    internal static class PointExtensions
    {
        public static CGPoint ToCGPoint(this Point self)
        {
            return new CGPoint((float)self.X, (float)self.Y);
        }
    }
}

