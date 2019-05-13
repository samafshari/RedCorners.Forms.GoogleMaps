using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.OS;
using Java.Lang;
using Xamarin.Forms.Platform.Android;
using Math = System.Math;
using Android.App;
using Android.Content;
using Android.Graphics;
using RedCorners.Forms.GoogleMaps.Logics.Android;
using RedCorners.Forms.GoogleMaps.Logics;
using RedCorners.Forms.GoogleMaps.Android.Extensions;
using Android.Widget;
using Android.Views;
using RedCorners.Forms.GoogleMaps.Android.Logics;
using RedCorners.Forms.GoogleMaps.Internals;
using Color = Xamarin.Forms.Color;
using Xamarin.Forms;
using Xamarin;
using Android.Locations;
using Xamarin.Forms.Internals;

[assembly: ExportRenderer(typeof(RedCorners.Forms.GoogleMaps.MapBase), typeof(RedCorners.Forms.GoogleMaps.Android.MapRenderer))]
[assembly: Preserve]
namespace RedCorners.Forms.GoogleMaps.Android
{
    public class MapRenderer : ViewRenderer<MapBase, global::Android.Views.View>,
        GoogleMap.IOnMapClickListener,
        GoogleMap.IOnMapLongClickListener,
        GoogleMap.IOnMyLocationButtonClickListener,
        GoogleMap.IOnMyLocationChangeListener
    {

        public void OnMyLocationChange(Location location)
        {
            if (location != null)
                MapLocationSystem.Instance.InjectMapModel(location.Latitude, location.Longitude);
        }

        readonly CameraLogic _cameraLogic;
        readonly UiSettingsLogic _uiSettingsLogic = new UiSettingsLogic();
        
        internal IList<BaseLogic<GoogleMap>> Logics { get; }

        public MapRenderer(Context context) : base(context)
        {
            _cameraLogic = new CameraLogic(UpdateVisibleRegion);

            AutoPackage = false;
            Logics = new List<BaseLogic<GoogleMap>>
            {
                new PolylineLogic(),
                new PolygonLogic(),
                new CircleLogic(),
                new PinLogic(context, Config.BitmapDescriptorFactory, 
                    OnMarkerCreating, OnMarkerCreated, OnMarkerDeleting, OnMarkerDeleted),
                new TileLayerLogic(),
                new GroundOverlayLogic(context, Config.BitmapDescriptorFactory)
            };
        }

        static Bundle s_bundle;
        internal static Bundle Bundle { set { s_bundle = value; } }

        protected internal static PlatformConfig Config { protected get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        protected GoogleMap NativeMap { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        protected MapBase Map => Element;

        private bool _ready = false;
        private bool _onLayout = false;

        private float _scaledDensity;

        public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
        {
            return new SizeRequest(new Size(Context.ToPixels(40), Context.ToPixels(40)));
        }

        protected override async void OnElementChanged(ElementChangedEventArgs<MapBase> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == e.NewElement)
            {
                return;
            }

            // For XAML Previewer or GoogleMapsSystem.Init not called.
            if (!GoogleMapsSystem.IsInitialized)
            {
                var tv = new TextView(Context)
                {
                    Gravity = GravityFlags.Center,
                    Text = "RedCorners.Forms.GoogleMaps"
                };
                tv.SetBackgroundColor(Color.Teal.ToAndroid());
                tv.SetTextColor(Color.Black.ToAndroid());
                SetNativeControl(tv);
                return;
            }

            // Uninitialize old view
            GoogleMap oldNativeMap = null;
            MapBase oldMap = null;
            if (e.OldElement != null)
            {
                try
                {
                    var oldNativeView = Control as MapView;
                    // ReSharper disable once PossibleNullReferenceException
                    oldNativeMap = await oldNativeView?.GetGoogleMapAsync();
                    oldMap = e.OldElement;
                    Uninitialize(oldNativeMap, oldMap);
                    oldNativeView?.Dispose();
                } 
                catch (System.Exception ex) 
                {
                    var message = ex.Message;
                    System.Diagnostics.Debug.WriteLine($"Uninitialize old view failed. - {message}");
                }
            }

            if (e.NewElement == null)
            {
                return;
            }

            var mapView = new MapView(Context);
            mapView.OnCreate(s_bundle);
            mapView.OnResume();
            SetNativeControl(mapView);

            var activity = Context as Activity;
            if (activity != null)
            {
                _scaledDensity = activity.GetScaledDensity();
                _cameraLogic.ScaledDensity = _scaledDensity;
                foreach (var logic in Logics)
                {
                    logic.ScaledDensity = _scaledDensity;
                }
            }

            var newMap = e.NewElement;
            NativeMap = await mapView.GetGoogleMapAsync();

            foreach (var logic in Logics)
            {
                logic.Register(oldNativeMap, oldMap, NativeMap, newMap);
                logic.ScaledDensity = _scaledDensity;
            }

            OnMapReady(NativeMap, newMap);
        }

        private void OnSnapshot(TakeSnapshotMessage snapshotMessage)
        {
            NativeMap.Snapshot(new DelegateSnapshotReadyCallback(snapshot =>
            {
                var stream = new MemoryStream();
                snapshot.Compress(Bitmap.CompressFormat.Png, 0, stream);
                stream.Position = 0;
                snapshotMessage?.OnSnapshot?.Invoke(stream);
            }));
        }

        protected virtual void OnMapReady(GoogleMap nativeMap, MapBase map)
        {
            if (nativeMap != null)
            {
                _cameraLogic.Register(map, nativeMap);

                _uiSettingsLogic.Register(map, nativeMap);
                Map.OnSnapshot += OnSnapshot;

                nativeMap.SetOnMapClickListener(this);
                nativeMap.SetOnMapLongClickListener(this);
                nativeMap.SetOnMyLocationButtonClickListener(this);
                nativeMap.SetOnMyLocationChangeListener(this);

                UpdateIsShowingUser(_uiSettingsLogic.MyLocationButtonEnabled);
                UpdateHasScrollEnabled(_uiSettingsLogic.ScrollGesturesEnabled);
                UpdateHasZoomEnabled(_uiSettingsLogic.ZoomControlsEnabled, _uiSettingsLogic.ZoomGesturesEnabled);
                UpdateHasRotationEnabled(_uiSettingsLogic.RotateGesturesEnabled);
                UpdateIsTrafficEnabled();
                UpdateIndoorEnabled();
                UpdateMapStyle();
                UpdateMyLocationEnabled();
                _uiSettingsLogic.Initialize();

                SetMapType();
                SetPadding();
            }

            _ready = true;
            if (_ready && _onLayout)
            {
                InitializeLogic();
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            // For XAML Previewer or GoogleMapsSystem.Init not called.
            if (!GoogleMapsSystem.IsInitialized)
            {
                return;
            }

            _onLayout = true;

            if (_ready && _onLayout)
            {
                InitializeLogic();
            }
            else if (changed && NativeMap != null)
            {
                UpdateVisibleRegion(NativeMap.CameraPosition.Target);
            }
        }

        void InitializeLogic()
        {
            _cameraLogic.MoveCamera(Map.InitialCameraUpdate);

            foreach (var logic in Logics)
            {
                if (logic.Map != null)
                {
                    logic.RestoreItems();
                    logic.OnMapPropertyChanged(new PropertyChangedEventArgs(MapBase.SelectedPinProperty.PropertyName));
                }
            }

            _ready = false;
            _onLayout = false;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            // For XAML Previewer or GoogleMapsSystem.Init not called.
            if (!GoogleMapsSystem.IsInitialized)
            {
                return;
            }

            if (e.PropertyName == MapBase.MapTypeProperty.PropertyName)
            {
                SetMapType();
                return;
            }

            if (e.PropertyName == MapBase.PaddingProperty.PropertyName)
            {
                SetPadding();
                return;
            }

            if (NativeMap == null)
            {
                return;
            }

            if (e.PropertyName == MapBase.IsShowingUserProperty.PropertyName)
            {
                UpdateIsShowingUser();
            }
            else if (e.PropertyName == MapBase.MyLocationEnabledProperty.PropertyName)
            {
                UpdateMyLocationEnabled();
            }
            else if (e.PropertyName == MapBase.HasScrollEnabledProperty.PropertyName)
            {
                UpdateHasScrollEnabled();
            }
            else if (e.PropertyName == MapBase.HasZoomEnabledProperty.PropertyName)
            {
                UpdateHasZoomEnabled();
            }
            else if (e.PropertyName == MapBase.HasRotationEnabledProperty.PropertyName)
            {
                UpdateHasRotationEnabled();
            }
            else if (e.PropertyName == MapBase.IsTrafficEnabledProperty.PropertyName)
            {
                UpdateIsTrafficEnabled();
            }
            else if (e.PropertyName == MapBase.IndoorEnabledProperty.PropertyName)
            {
                UpdateIndoorEnabled();
            }
            else if (e.PropertyName == MapBase.MapStyleProperty.PropertyName)
            {
                UpdateMapStyle();
            }

            foreach (var logic in Logics)
            {
                logic.OnMapPropertyChanged(e);
            }
        }

        private void UpdateIsShowingUser(bool? initialMyLocationButtonEnabled = null)
        {
#pragma warning disable 618
            NativeMap.MyLocationEnabled = Map.IsShowingUser;
            NativeMap.UiSettings.MyLocationButtonEnabled = initialMyLocationButtonEnabled ?? Map.IsShowingUser;
#pragma warning restore 618
        }

        private void UpdateMyLocationEnabled()
        {
            NativeMap.MyLocationEnabled = Map.MyLocationEnabled;
        }

        private void UpdateHasScrollEnabled(bool? initialScrollGesturesEnabled = null)
        {
#pragma warning disable 618
            NativeMap.UiSettings.ScrollGesturesEnabled = initialScrollGesturesEnabled ?? Map.HasScrollEnabled;
#pragma warning restore 618
        }

        private void UpdateHasZoomEnabled(
            bool? initialZoomControlsEnabled = null, 
            bool? initialZoomGesturesEnabled = null)
        {
#pragma warning disable 618
            NativeMap.UiSettings.ZoomControlsEnabled = initialZoomControlsEnabled ?? Map.HasZoomEnabled;
            NativeMap.UiSettings.ZoomGesturesEnabled = initialZoomGesturesEnabled ?? Map.HasZoomEnabled;
#pragma warning restore 618
        }

        private void UpdateHasRotationEnabled(bool? initialRotateGesturesEnabled = null)
        {
#pragma warning disable 618
            NativeMap.UiSettings.RotateGesturesEnabled = initialRotateGesturesEnabled ?? Map.HasRotationEnabled;
#pragma warning restore 618
        }

        private void UpdateIsTrafficEnabled()
        {
            NativeMap.TrafficEnabled = Map.IsTrafficEnabled;
        }

        private void UpdateIndoorEnabled()
        {
            NativeMap.SetIndoorEnabled(Map.IsIndoorEnabled);
        }

        private void UpdateMapStyle()
        {
            var map = NativeMap;
            if (map == null)
                return;

            var styleJson = Map.MapStyle;
            if (styleJson != null)
                map.SetMapStyle(new MapStyleOptions(styleJson));
            else
                map.SetMapStyle(new MapStyleOptions("[]"));
        }

        void SetMapType()
        {
            var map = NativeMap;
            if (map == null)
                return;

            switch (Map.MapType)
            {
                case MapType.Street:
                    map.MapType = GoogleMap.MapTypeNormal;
                    break;
                case MapType.Satellite:
                    map.MapType = GoogleMap.MapTypeSatellite;
                    break;
                case MapType.Hybrid:
                    map.MapType = GoogleMap.MapTypeHybrid;
                    break;
                case MapType.Terrain:
                    map.MapType = GoogleMap.MapTypeTerrain;
                    break;
                case MapType.None:
                    map.MapType = GoogleMap.MapTypeNone;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void SetPadding()
        {
            NativeMap?.SetPadding(
                (int)(Map.Padding.Left * _scaledDensity), 
                (int)(Map.Padding.Top * _scaledDensity), 
                (int)(Map.Padding.Right * _scaledDensity),
                (int)(Map.Padding.Bottom * _scaledDensity));
        }

        public void OnMapClick(LatLng point)
        {
            Map.SendMapClicked(point.ToPosition());
        }

        public void OnMapLongClick(LatLng point)
        {
            Map.SendMapLongClicked(point.ToPosition());
        }

        public bool OnMyLocationButtonClick()
        {
            return Map.SendMyLocationClicked();
        }

        void UpdateVisibleRegion(LatLng pos)
        {
            var map = NativeMap;
            if (map == null)
                return;
            var projection = map.Projection;
            var width = Control.Width;
            var height = Control.Height;
            var ul = projection.FromScreenLocation(new global::Android.Graphics.Point(0, 0));
            var ur = projection.FromScreenLocation(new global::Android.Graphics.Point(width, 0));
            var ll = projection.FromScreenLocation(new global::Android.Graphics.Point(0, height));
            var lr = projection.FromScreenLocation(new global::Android.Graphics.Point(width, height));
            var dlat = Math.Max(Math.Abs(ul.Latitude - lr.Latitude), Math.Abs(ur.Latitude - ll.Latitude));
            var dlong = Math.Max(Math.Abs(ul.Longitude - lr.Longitude), Math.Abs(ur.Longitude - ll.Longitude));
#pragma warning disable 618
            Element.VisibleRegion = new MapSpan(
                    new Position(
                        pos.Latitude,
                        pos.Longitude
                    ),
                dlat,
                dlong
            );
#pragma warning restore 618
            Element.Region = projection.VisibleRegion.ToRegion();
        }

        #region Overridable Members

        /// <summary>
        /// Call when before marker create.
        /// You can override your custom renderer for customize marker.
        /// </summary>
        /// <param name="outerItem">the pin.</param>
        /// <param name="innerItem">the marker options.</param>
        protected virtual void OnMarkerCreating(Pin outerItem, MarkerOptions innerItem)
        {
        }

        /// <summary>
        /// Call when after marker create.
        /// You can override your custom renderer for customize marker.
        /// </summary>
        /// <param name="outerItem">the pin.</param>
        /// <param name="innerItem">thr marker.</param>
        protected virtual void OnMarkerCreated(Pin outerItem, Marker innerItem)
        {
        }

        /// <summary>
        /// Call when before marker delete.
        /// You can override your custom renderer for customize marker.
        /// </summary>
        /// <param name="outerItem">the pin.</param>
        /// <param name="innerItem">thr marker.</param>
        protected virtual void OnMarkerDeleting(Pin outerItem, Marker innerItem)
        {
        }

        /// <summary>
        /// Call when after marker delete.
        /// You can override your custom renderer for customize marker.
        /// </summary>
        /// <param name="outerItem">the pin.</param>
        /// <param name="innerItem">thr marker.</param>
        protected virtual void OnMarkerDeleted(Pin outerItem, Marker innerItem)
        {
        }

        #endregion

        private void Uninitialize(GoogleMap nativeMap, MapBase map) 
        {
            try
            {
                if (nativeMap == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Uninitialize failed - {nameof(nativeMap)} is null");
                    return;
                }

                if (map == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Uninitialize failed - {nameof(map)} is null");
                    return;
                }

                _uiSettingsLogic.Unregister();

                map.OnSnapshot -= OnSnapshot;
                _cameraLogic.Unregister();

                foreach (var logic in Logics)
                {
                    logic.Unregister(nativeMap, map);
                }

                nativeMap.SetOnMapClickListener(null);
                nativeMap.SetOnMapLongClickListener(null);
                nativeMap.SetOnMyLocationButtonClickListener(null);

                nativeMap.MyLocationEnabled = false;
                nativeMap.Dispose();
            }
            catch (System.Exception ex)
            {
                var message = ex.Message;
                System.Diagnostics.Debug.WriteLine($"Uninitialize failed. - {message}");
            }
        }

        bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                Uninitialize(NativeMap, Map);

                if (NativeMap != null)
                {
                    NativeMap.Dispose();
                    NativeMap = null;
                }

                (Control as MapView)?.OnDestroy();
            }

            base.Dispose(disposing);
        }
    }
}
