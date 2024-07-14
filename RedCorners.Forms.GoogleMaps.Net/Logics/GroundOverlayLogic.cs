using System;
using System.Collections.Generic;
using Android.Gms.Maps;
using NativeGroundOverlay = Android.Gms.Maps.Model.GroundOverlay;
using Android.Gms.Maps.Model;
using RedCorners.Forms.GoogleMaps.Android.Extensions;
using System.Linq;
using RedCorners.Forms.GoogleMaps.Android.Factories;
using RedCorners.Forms.GoogleMaps.Android;
using Android.Widget;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace RedCorners.Forms.GoogleMaps.Logics.Android
{
    internal class GroundOverlayLogic : DefaultGroundOverlayLogic<NativeGroundOverlay, GoogleMap>
    {
        protected override IList<GroundOverlay> GetItems(MapBase map) => map.GroundOverlays;

        private readonly Context _context;
        private readonly IBitmapDescriptorFactory _bitmapDescriptorFactory;

        public GroundOverlayLogic(
            Context context, 
            IBitmapDescriptorFactory bitmapDescriptorFactory)
        {
            _context = context;
            _bitmapDescriptorFactory = bitmapDescriptorFactory;
        }

        internal override void Register(GoogleMap oldNativeMap, MapBase oldMap, GoogleMap newNativeMap, MapBase newMap)
        {
            base.Register(oldNativeMap, oldMap, newNativeMap, newMap);

            if (newNativeMap != null)
            {
                newNativeMap.GroundOverlayClick += OnGroundOverlayClick;
            }
        }

        internal override void Unregister(GoogleMap nativeMap, MapBase map)
        {
            if (nativeMap != null)
            {
                nativeMap.GroundOverlayClick -= OnGroundOverlayClick;
            }

            base.Unregister(nativeMap, map);
        }

        protected override NativeGroundOverlay CreateNativeItem(GroundOverlay outerItem)
        {
            var factory = _bitmapDescriptorFactory ?? DefaultBitmapDescriptorFactory.Instance;
            var nativeDescriptor = factory.ToNative(outerItem.Icon, _context);

            var opts = new GroundOverlayOptions()
                .PositionFromBounds(outerItem.Bounds.ToLatLngBounds())
                .Clickable(outerItem.IsClickable)
                .InvokeBearing(outerItem.Bearing)
                .InvokeImage(nativeDescriptor)
                .InvokeTransparency(outerItem.Transparency)
                .InvokeZIndex(outerItem.ZIndex);

            var overlay = NativeMap.AddGroundOverlay(opts);

            // If the pin has an IconView set this method will convert it into an icon for the marker
            if (outerItem?.Icon?.Type == BitmapDescriptorType.View)
            {
                overlay.Visible = false; // Will become visible once the iconview is ready.
                TransformXamarinViewToAndroidBitmap(outerItem, overlay);
            }
            else
            {
                overlay.Visible = outerItem.IsVisible;
            }

            // associate pin with marker for later lookup in event handlers
            outerItem.NativeObject = overlay;
            return overlay;
        }

        protected override NativeGroundOverlay DeleteNativeItem(GroundOverlay outerItem)
        {
            var nativeOverlay = outerItem.NativeObject as NativeGroundOverlay;
            if (nativeOverlay == null)
                return null;
            nativeOverlay.Remove();
            outerItem.NativeObject = null;

            return nativeOverlay;
        }

        void OnGroundOverlayClick(object sender, GoogleMap.GroundOverlayClickEventArgs e)
        {
            // clicked ground overlay
            var nativeItem = e.GroundOverlay;

            // lookup overlay
            var targetOuterItem = GetItems(Map).FirstOrDefault(
                outerItem => ((NativeGroundOverlay)outerItem.NativeObject).Id == nativeItem.Id);

            // only consider event handled if a handler is present.
            // Else allow default behavior of displaying an info window.
            targetOuterItem?.SendTap();
        }

        internal override void OnUpdateBearing(GroundOverlay outerItem, NativeGroundOverlay nativeItem)
        {
            nativeItem.Bearing = outerItem.Bearing;
        }

        internal override void OnUpdateBounds(GroundOverlay outerItem, NativeGroundOverlay nativeItem)
        {
            nativeItem.SetPositionFromBounds(outerItem.Bounds.ToLatLngBounds()); 
        }

        internal override void OnUpdateIcon(GroundOverlay outerItem, NativeGroundOverlay nativeItem)
        {
            if (outerItem.Icon != null && outerItem.Icon.Type == BitmapDescriptorType.View)
            {
                // If the pin has an IconView set this method will convert it into an icon for the marker
                TransformXamarinViewToAndroidBitmap(outerItem, nativeItem);
            }
            else
            {
                var factory = _bitmapDescriptorFactory ?? DefaultBitmapDescriptorFactory.Instance;
                var nativeDescriptor = factory.ToNative(outerItem.Icon, _context);
                nativeItem.SetImage(nativeDescriptor);
            }
        }

        internal override void OnUpdateIsClickable(GroundOverlay outerItem, NativeGroundOverlay nativeItem)
        {
            nativeItem.Clickable = outerItem.IsClickable;
        }

        internal override void OnUpdateTransparency(GroundOverlay outerItem, NativeGroundOverlay nativeItem)
        {
            nativeItem.Transparency = outerItem.Transparency;
        }

        internal override void OnUpdateZIndex(GroundOverlay outerItem, NativeGroundOverlay nativeItem)
        {
            nativeItem.ZIndex = outerItem.ZIndex;
        }

        private async void TransformXamarinViewToAndroidBitmap(GroundOverlay outerItem, NativeGroundOverlay nativeItem)
        {
            if (outerItem?.Icon?.Type == BitmapDescriptorType.View && outerItem.Icon?.View != null)
            {
                var iconView = outerItem.Icon.View;
                var nativeView = await Utils.ConvertFormsToNative(
                    iconView,
                    new Rectangle(0, 0, (double)Utils.DpToPx((float)iconView.WidthRequest), (double)Utils.DpToPx((float)iconView.HeightRequest)),
                    Platform.CreateRendererWithContext(iconView, _context));
                var otherView = new FrameLayout(nativeView.Context);
                nativeView.LayoutParameters = new FrameLayout.LayoutParams(Utils.DpToPx((float)iconView.WidthRequest), Utils.DpToPx((float)iconView.HeightRequest));
                otherView.AddView(nativeView);
                nativeItem.SetImage(await Utils.ConvertViewToBitmapDescriptor(otherView));
                //nativeItem.SetAnchor((float)iconView.AnchorX, (float)iconView.AnchorY);
                nativeItem.Visible = true;
            }
        }
    }
}


