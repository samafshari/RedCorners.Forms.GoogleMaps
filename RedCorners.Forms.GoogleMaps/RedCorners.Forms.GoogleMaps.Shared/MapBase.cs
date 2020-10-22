using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using RedCorners.Forms.GoogleMaps.Internals;
using RedCorners.Forms.GoogleMaps.Helpers;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps
{
    public class MapBase : ContentView2
    {
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(IEnumerable), typeof(IEnumerable), typeof(MapBase), default(IEnumerable),
            propertyChanged: (b, o, n) => ((MapBase)b).OnItemsSourcePropertyChanged((IEnumerable)o, (IEnumerable)n));

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(MapBase), new BasicMapObjectTemplate(),
            propertyChanged: (b, o, n) => ((MapBase)b).OnItemTemplatePropertyChanged((DataTemplate)o, (DataTemplate)n));

        public static readonly BindableProperty MapTypeProperty = BindableProperty.Create(nameof(MapType), typeof(MapType), typeof(MapBase), default(MapType));

        public static readonly BindableProperty RegionChangeActionProperty = BindableProperty.Create(nameof(RegionChangeAction), typeof(Action<MapRegion>), typeof(MapBase));

#pragma warning disable CS0618 // Type or member is obsolete
        //public static readonly BindableProperty IsShowingUserProperty = BindableProperty.Create(nameof(IsShowingUser), typeof(bool), typeof(MapBase), default(bool));

        public static readonly BindableProperty MyLocationEnabledProperty = BindableProperty.Create(nameof(MyLocationEnabled), typeof(bool), typeof(MapBase), default(bool));
        public static readonly BindableProperty IsMyLocationButtonVisibleProperty = BindableProperty.Create(nameof(IsMyLocationButtonVisible), typeof(bool), typeof(MapBase), default(bool));


        public static readonly BindableProperty HasScrollEnabledProperty = BindableProperty.Create(nameof(HasScrollEnabled), typeof(bool), typeof(MapBase), true);

        public static readonly BindableProperty HasZoomEnabledProperty = BindableProperty.Create(nameof(HasZoomEnabled), typeof(bool), typeof(MapBase), true);

        public static readonly BindableProperty HasRotationEnabledProperty = BindableProperty.Create(nameof(HasRotationEnabled), typeof(bool), typeof(MapBase), true);
#pragma warning restore CS0618 // Type or member is obsolete

        public static readonly BindableProperty SelectedPinProperty = BindableProperty.Create(nameof(SelectedPin), typeof(Pin), typeof(MapBase), default(Pin), defaultBindingMode: BindingMode.TwoWay);

        public static readonly BindableProperty IsTrafficEnabledProperty = BindableProperty.Create(nameof(IsTrafficEnabled), typeof(bool), typeof(MapBase), false);

        public static readonly BindableProperty IndoorEnabledProperty = BindableProperty.Create(nameof(IsIndoorEnabled), typeof(bool), typeof(MapBase), true);

        public static readonly BindableProperty InitialCameraUpdateProperty = BindableProperty.Create(
            nameof(InitialCameraUpdate), typeof(CameraUpdate), typeof(MapBase),
            CameraUpdateFactory.NewPositionZoom(new Position(41.89, 12.49), 10),  // center on Rome by default
            propertyChanged: (bindable, oldValue, newValue) => 
            {
                ((MapBase)bindable)._useMoveToRegionAsInitialBounds = false;   
            });

        public static new readonly BindableProperty PaddingProperty = BindableProperty.Create(nameof(PaddingProperty), typeof(Thickness), typeof(MapBase), default(Thickness));

        bool _useMoveToRegionAsInitialBounds = true;

        public static readonly BindableProperty CameraPositionProperty = BindableProperty.Create(
            nameof(CameraPosition), typeof(CameraPosition), typeof(MapBase),
            defaultValueCreator: (bindable) => new CameraPosition(((MapBase)bindable).InitialCameraUpdate.Position, 10),
            defaultBindingMode: BindingMode.TwoWay);

        public static readonly BindableProperty MapStyleProperty = BindableProperty.Create(nameof(MapStyle), typeof(string), typeof(MapBase), default(string));

        readonly ObservableCollection<Pin> _pins = new ObservableCollection<Pin>();
        readonly ObservableCollection<Polyline> _polylines = new ObservableCollection<Polyline>();
        readonly ObservableCollection<Polygon> _polygons = new ObservableCollection<Polygon>();
        readonly ObservableCollection<Circle> _circles = new ObservableCollection<Circle>();
        readonly ObservableCollection<TileLayer> _tileLayers = new ObservableCollection<TileLayer>();
        readonly ObservableCollection<GroundOverlay> _groundOverlays = new ObservableCollection<GroundOverlay>();
        readonly ObservableCollection<MapObjectCollectionBase> _collections = new ObservableCollection<MapObjectCollectionBase>();
        readonly Dictionary<MapObject, MapObjectCollectionBase> _ownerships = new Dictionary<MapObject, MapObjectCollectionBase>();

        public event EventHandler<PinClickedEventArgs> PinClicked;
        public event EventHandler<SelectedPinChangedEventArgs> SelectedPinChanged;
        public event EventHandler<InfoWindowClickedEventArgs> InfoWindowClicked;
        public event EventHandler<InfoWindowLongClickedEventArgs> InfoWindowLongClicked;

        public event EventHandler<PinDragEventArgs> PinDragStart;
        public event EventHandler<PinDragEventArgs> PinDragEnd;
        public event EventHandler<PinDragEventArgs> PinDragging;

        public event EventHandler<MapClickedEventArgs> MapClicked;
        public event EventHandler<MapLongClickedEventArgs> MapLongClicked;
        public event EventHandler<MyLocationButtonClickedEventArgs> MyLocationButtonClicked;

        [Obsolete("Please use Map.CameraIdled instead of this")]
        public event EventHandler<CameraChangedEventArgs> CameraChanged;
        public event EventHandler<CameraMoveStartedEventArgs> CameraMoveStarted;
        public event EventHandler<CameraMovingEventArgs> CameraMoving;
        public event EventHandler<CameraIdledEventArgs> CameraIdled;

        internal Action<MoveToRegionMessage> OnMoveToRegion { get; set; }

        internal Action<CameraUpdateMessage> OnMoveCamera { get; set; }

        internal Action<CameraUpdateMessage> OnAnimateCamera { get; set; }

        internal Action<TakeSnapshotMessage> OnSnapshot { get; set; }

        MapRegion _region;

        public MapBase()
        {
            VerticalOptions = HorizontalOptions = LayoutOptions.FillAndExpand;

            _pins.CollectionChanged += PinsOnCollectionChanged;
            _polylines.CollectionChanged += PolylinesOnCollectionChanged;
            _polygons.CollectionChanged += PolygonsOnCollectionChanged;
            _circles.CollectionChanged += CirclesOnCollectionChanged;
            _tileLayers.CollectionChanged += TileLayersOnCollectionChanged;
            _groundOverlays.CollectionChanged += GroundOverlays_CollectionChanged;
            _collections.CollectionChanged += Layers_CollectionChanged;
        }

        [Obsolete("Please use Map.UiSettings.ScrollGesturesEnabled instead of this")]
        public bool HasScrollEnabled
        {
            get { return (bool)GetValue(HasScrollEnabledProperty); }
            set { SetValue(HasScrollEnabledProperty, value); }
        }

        //[Obsolete("Please use Map.UiSettings.ZoomGesturesEnabled and ZoomControlsEnabled instead of this")]
        public bool HasZoomEnabled
        {
            get { return (bool)GetValue(HasZoomEnabledProperty); }
            set { SetValue(HasZoomEnabledProperty, value); }
        }

        [Obsolete("Please use Map.UiSettings.RotateGesturesEnabled instead of this")]
        public bool HasRotationEnabled
        {
            get { return (bool)GetValue(HasRotationEnabledProperty); }
            set { SetValue(HasRotationEnabledProperty, value); }
        }

        public bool IsTrafficEnabled
        {
            get { return (bool)GetValue(IsTrafficEnabledProperty); }
            set { SetValue(IsTrafficEnabledProperty, value); }
        }

        public bool IsIndoorEnabled
        {
            get { return (bool) GetValue(IndoorEnabledProperty); }
            set { SetValue(IndoorEnabledProperty, value);}
        }

        //[Obsolete("Please use Map.MyLocationEnabled and Map.UiSettings.MyLocationButtonEnabled instead of this")]
        //public bool IsShowingUser
        //{
        //    get { return (bool)GetValue(IsShowingUserProperty); }
        //    set { SetValue(IsShowingUserProperty, value); }
        //}

        public bool MyLocationEnabled
        {
            get { return (bool)GetValue(MyLocationEnabledProperty); }
            set { SetValue(MyLocationEnabledProperty, value); }
        }

        public bool IsMyLocationButtonVisible
        {
            get { return (bool)GetValue(IsMyLocationButtonVisibleProperty); }
            set { SetValue(IsMyLocationButtonVisibleProperty, value); }
        }

        public MapType MapType
        {
            get { return (MapType)GetValue(MapTypeProperty); }
            set { SetValue(MapTypeProperty, value); }
        }

        public Pin SelectedPin
        {
            get { return (Pin)GetValue(SelectedPinProperty); }
            set { SetValue(SelectedPinProperty, value); }
        }

        [Xamarin.Forms.TypeConverter(typeof(CameraUpdateConverter))]
        public CameraUpdate InitialCameraUpdate
        {
            get { return (CameraUpdate)GetValue(InitialCameraUpdateProperty); }
            set { SetValue(InitialCameraUpdateProperty, value); }
        }

        public CameraPosition CameraPosition
        {
            get { return (CameraPosition)GetValue(CameraPositionProperty); }
            internal set { SetValue(CameraPositionProperty, value); }
        }

        public new Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public string MapStyle
        {
            get { return (string)GetValue(MapStyleProperty); }
            set { SetValue(MapStyleProperty, value); }
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public IList<Pin> Pins => _pins;
        public IList<Polyline> Polylines => _polylines;
        public IList<Polygon> Polygons => _polygons;
        public IList<Circle> Circles => _circles;
        public IList<TileLayer> TileLayers => _tileLayers;
        public IList<GroundOverlay> GroundOverlays => _groundOverlays;
        public IList<MapObjectCollectionBase> Collections => _collections;

        public MapRegion Region
        {
            get { return _region; }
            internal set
            {
                if (_region == value)
                    return;
                OnPropertyChanging();
                _region = value ?? throw new ArgumentNullException(nameof(value));
                OnPropertyChanged();
                RegionChangeAction?.Invoke(Region);
                UpdateCollectionsRegions();
            }
        }

        public Action<MapRegion> RegionChangeAction
        {
            get => (Action<MapRegion>)GetValue(RegionChangeActionProperty);
            set => SetValue(RegionChangeActionProperty, value);
        }

        public UiSettings UiSettings { get; } = new UiSettings();

        public void MoveToRegion(MapSpan mapSpan, bool animate = true)
        {
            if (mapSpan == null)
                throw new ArgumentNullException(nameof(mapSpan));

            if (_useMoveToRegionAsInitialBounds)
            {
                InitialCameraUpdate = CameraUpdateFactory.NewBounds(mapSpan.ToBounds(), 0);
                _useMoveToRegionAsInitialBounds = false;
            }

            SendMoveToRegion(new MoveToRegionMessage(mapSpan, animate));
        }

        public Task<AnimationStatus> MoveCamera(CameraUpdate cameraUpdate)
        {
            var comp = new TaskCompletionSource<AnimationStatus>();

            SendMoveCamera(new CameraUpdateMessage(cameraUpdate, null, new DelegateAnimationCallback(
                () => comp.SetResult(AnimationStatus.Finished), 
                () => comp.SetResult(AnimationStatus.Canceled))));

            return comp.Task;
        }

        public Task<AnimationStatus> AnimateCamera(CameraUpdate cameraUpdate, TimeSpan? duration = null)
        {
            var comp = new TaskCompletionSource<AnimationStatus>();

            SendAnimateCamera(new CameraUpdateMessage(cameraUpdate, duration, new DelegateAnimationCallback(
                () => comp.SetResult(AnimationStatus.Finished),
                () => comp.SetResult(AnimationStatus.Canceled))));

            return comp.Task;
        }


        public Task<Stream> TakeSnapshot()
        {
            var comp = new TaskCompletionSource<Stream>();

            SendTakeSnapshot(new TakeSnapshotMessage(image => comp.SetResult(image)));

            return comp.Task;
        }

        void PinsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Cast<Pin>().Any(pin => pin.Label == null))
                throw new ArgumentException("Pin must have a Label to be added to a map");
        }

        void PolylinesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Cast<Polyline>().Any(polyline => polyline.Positions.Count < 2))
                throw new ArgumentException("Polyline must have a 2 positions to be added to a map");
        }

        void PolygonsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Cast<Polygon>().Any(polygon => polygon.Positions.Count < 3))
                throw new ArgumentException("Polygon must have a 3 positions to be added to a map");
        }

        void CirclesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Cast<Circle>().Any(circle => (
                circle?.Center == null || circle?.Radius == null || circle.Radius.Meters <= 0f)))
                throw new ArgumentException("Circle must have a center and radius");
        }

        void TileLayersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (e.NewItems != null && e.NewItems.Cast<ITileLayer>().Any(tileLayer => (circle.Center == null || circle.Radius == null || circle.Radius.Meters <= 0f)))
            //  throw new ArgumentException("Circle must have a center and radius");
        }

        void GroundOverlays_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        private void Layers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

        }

        internal void SendSelectedPinChanged(Pin selectedPin)
        {
            SelectedPinChanged?.Invoke(this, new SelectedPinChangedEventArgs(selectedPin));
        }

        internal bool SendPinClicked(Pin pin)
        {
            var args = new PinClickedEventArgs(pin);
            PinClicked?.Invoke(this, args);
            return args.Handled;
        }

        internal void SendInfoWindowClicked(Pin pin)
        {
            var args = new InfoWindowClickedEventArgs(pin);
            InfoWindowClicked?.Invoke(this, args);
        }

        internal void SendInfoWindowLongClicked(Pin pin)
        {
            var args = new InfoWindowLongClickedEventArgs(pin);
            InfoWindowLongClicked?.Invoke(this, args);
        }

        internal void SendPinDragStart(Pin pin)
        {
            PinDragStart?.Invoke(this, new PinDragEventArgs(pin));
        }

        internal void SendPinDragEnd(Pin pin)
        {
            PinDragEnd?.Invoke(this, new PinDragEventArgs(pin));
        }

        internal void SendPinDragging(Pin pin)
        {
            PinDragging?.Invoke(this, new PinDragEventArgs(pin));
        }

        internal void SendMapClicked(Position point)
        {
            MapClicked?.Invoke(this, new MapClickedEventArgs(point));
        }

        internal void SendMapLongClicked(Position point)
        {
            MapLongClicked?.Invoke(this, new MapLongClickedEventArgs(point));
        }

        internal bool SendMyLocationClicked()
        {
            var args = new MyLocationButtonClickedEventArgs();
            MyLocationButtonClicked?.Invoke(this, args);
            return args.Handled;
        }

        internal void SendCameraChanged(CameraPosition position)
        {
            CameraChanged?.Invoke(this, new CameraChangedEventArgs(position));
        }

        internal void SendCameraMoveStarted(bool isGesture)
        {
            CameraMoveStarted?.Invoke(this, new CameraMoveStartedEventArgs(isGesture));
        }

        internal void SendCameraMoving(CameraPosition position)
        {
            CameraMoving?.Invoke(this, new CameraMovingEventArgs(position));
        }

        internal void SendCameraIdled(CameraPosition position)
        {
            CameraIdled?.Invoke(this, new CameraIdledEventArgs(position));
        }

        private void SendMoveToRegion(MoveToRegionMessage message)
        {
            OnMoveToRegion?.Invoke(message);
        }

        void SendMoveCamera(CameraUpdateMessage message)
        {
            OnMoveCamera?.Invoke(message);
        }
    
        void SendAnimateCamera(CameraUpdateMessage message)
        {
            OnAnimateCamera?.Invoke(message);
        }

        void SendTakeSnapshot(TakeSnapshotMessage message)
        {
            OnSnapshot?.Invoke(message);
        }

        void OnItemsSourcePropertyChanged(IEnumerable oldItemsSource, IEnumerable newItemsSource)
        {
            if (oldItemsSource is INotifyCollectionChanged ncc)
            {
                ncc.CollectionChanged -= OnItemsSourceCollectionChanged;
            }

            if (newItemsSource is INotifyCollectionChanged ncc1)
            {
                ncc1.CollectionChanged += OnItemsSourceCollectionChanged;
            }

            ClearCollections();
            CreateItems();
        }

        void OnItemTemplatePropertyChanged(DataTemplate _, DataTemplate newItemTemplate)
        {
            ClearCollections();
            CreateItems();
        }

        void ClearCollections()
        {
            _pins.Clear();
            _polylines.Clear();
            _polygons.Clear();
            _circles.Clear();
            _tileLayers.Clear();
            _groundOverlays.Clear();
        }

        readonly object lockCollectionChanged = new object();
        volatile bool isCollectionChanging = false;
        public bool IsCollectionChanging => isCollectionChanging;
        public event EventHandler<bool> OnCollectionChanging;
        void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                lock (lockCollectionChanged)
                {
                    isCollectionChanging = true;
                    OnCollectionChanging?.Invoke(this, IsCollectionChanging);
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (e.NewStartingIndex == -1)
                                goto case NotifyCollectionChangedAction.Reset;
                            foreach (object item in e.NewItems)
                                CreateItem(item);
                            break;
                        case NotifyCollectionChangedAction.Move:
                            if (e.OldStartingIndex == -1 || e.NewStartingIndex == -1)
                                goto case NotifyCollectionChangedAction.Reset;
                            // Not tracking order
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            if (e.OldStartingIndex == -1)
                                goto case NotifyCollectionChangedAction.Reset;
                            foreach (object item in e.OldItems)
                                RemoveItem(item);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            if (e.OldStartingIndex == -1)
                                goto case NotifyCollectionChangedAction.Reset;
                            foreach (object item in e.OldItems)
                                RemoveItem(item);
                            foreach (object item in e.NewItems)
                                CreateItem(item);
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            ClearCollections();
                            break;
                    }
                }
            }
            finally
            {
                isCollectionChanging = false;
                OnCollectionChanging?.Invoke(this, IsCollectionChanging);
            }
        }

        void CreateItems()
        {
            if (ItemsSource == null || ItemTemplate == null)
            {
                return;
            }

            List<object> items = new List<object>();
            foreach (object item in ItemsSource)
                items.Add(item);
            foreach (object item in items)
                CreateItem(item);
        }

        void SyncCollection(MapObjectCollectionBase collection)
        {
            var items = collection.GetVisibleItems(Region).ToList();
            var existingItems = GetObjectsFromOwner(collection).ToList();
            var itemsToDelete = existingItems.Where(x => !items.Contains(x)).ToList();
            var itemsToAdd = items.Where(x => !existingItems.Contains(x)).ToList();

            for (int i = 0; i < itemsToDelete.Count; i++)
            {
                _ownerships.Remove(itemsToDelete[i]);
                RemoveItem(itemsToDelete[i]);
            }

            for (int i = 0; i < itemsToAdd.Count; i++)
            {
                _ownerships[itemsToAdd[i]] = collection;
                CreateItem(itemsToAdd[i]);
            }
        }

        void UpdateCollectionsRegions()
        {
            foreach (var collection in Collections)
            {
                collection.UpdateMapRegion(Region);
            }
        }

        void CreateItem(object newItem)
        {
            if (newItem is MapObjectCollectionBase collection)
            {
                collection.CollectionChanged += MapObjectCollection_CollectionChanged;
                _collections.Add(collection);
                SyncCollection(collection);
                return;
            }

            if (ItemTemplate == null)
            {
                return;
            }

            var template = ItemTemplate;
            if (template is DataTemplateSelector ds)
                template = ds.SelectTemplate(newItem, this);

            var obj = template.CreateContent();

            switch (obj)
            {
                case Pin pin:
                    pin.BindingContext = newItem;
                    _pins.Add(pin);
                    break;
                case Polyline polyline:
                    polyline.BindingContext = newItem;
                    _polylines.Add(polyline);
                    break;
                case Polygon polygon:
                    polygon.BindingContext = newItem;
                    _polygons.Add(polygon);
                    break;
                case Circle circle:
                    circle.BindingContext = newItem;
                    _circles.Add(circle);
                    break;
                case TileLayer tileLayer:
                    tileLayer.BindingContext = newItem;
                    _tileLayers.Add(tileLayer);
                    break;
                case GroundOverlay groundOverlay:
                    groundOverlay.BindingContext = newItem;
                    _groundOverlays.Add(groundOverlay);
                    break;
                default:
                    throw new Exception("RedCorners.Forms.GoogleMaps: No behavior is defined for the item.");
            }
        }

        private void MapObjectCollection_CollectionChanged(MapObjectCollectionBase collection)
        {
            SyncCollection(collection);
        }

        void RemoveItem(object itemToRemove)
        {
            if (SelectedPin == itemToRemove)
                SelectedPin = null;

            if (_collections.Contains(itemToRemove))
            {
                var collection = (MapObjectCollectionBase)itemToRemove;
                _collections.Remove(collection);
                return;
            }

            foreach (var pin in _pins.Where(x => x.BindingContext?.Equals(itemToRemove) ?? false).ToList())
                _pins.Remove(pin);

            foreach (var polyline in _polylines.Where(x => x.BindingContext?.Equals(itemToRemove) ?? false).ToList())
                _polylines.Remove(polyline);

            foreach (var polygon in _polygons.Where(x => x.BindingContext?.Equals(itemToRemove) ?? false).ToList())
                _polygons.Remove(polygon);

            foreach (var circle in _circles.Where(x => x.BindingContext?.Equals(itemToRemove) ?? false).ToList())
                _circles.Remove(circle);

            foreach (var tileLayer in _tileLayers.Where(x => x.BindingContext?.Equals(itemToRemove) ?? false).ToList())
                _tileLayers.Remove(tileLayer);

            foreach (var groundOverlay in _groundOverlays.Where(x => x.BindingContext?.Equals(itemToRemove) ?? false).ToList())
                _groundOverlays.Remove(groundOverlay);
        }
        
        void RemoveObjectsFromOwner(MapObjectCollectionBase owner)
        {
            if (SelectedPin != null && IsOwnedBy(SelectedPin, owner))
                SelectedPin = null;

            foreach (var pin in _pins.Where(x => IsOwnedBy(x, owner)).ToList())
                _pins.Remove(pin);

            foreach (var polyline in _polylines.Where(x => IsOwnedBy(x, owner)).ToList())
                _polylines.Remove(polyline);

            foreach (var polygon in _polygons.Where(x => IsOwnedBy(x, owner)).ToList())
                _polygons.Remove(polygon);

            foreach (var circle in _circles.Where(x => IsOwnedBy(x, owner)).ToList())
                _circles.Remove(circle);

            foreach (var tileLayer in _tileLayers.Where(x => IsOwnedBy(x, owner)).ToList())
                _tileLayers.Remove(tileLayer);

            foreach (var groundOverlay in _groundOverlays.Where(x => IsOwnedBy(x, owner)).ToList())
                _groundOverlays.Remove(groundOverlay);
        }

        bool IsOwnedBy(MapObject o, MapObjectCollectionBase c)
        {
            if (_ownerships.TryGetValue(o, out var collection) && collection == c)
                return true;

            return false;
        }

        IEnumerable<MapObject> GetObjectsFromOwner(MapObjectCollectionBase owner)
        {
            return
                _ownerships.Where(x => x.Value == owner).Select(x => x.Key);
        }
    }
}
