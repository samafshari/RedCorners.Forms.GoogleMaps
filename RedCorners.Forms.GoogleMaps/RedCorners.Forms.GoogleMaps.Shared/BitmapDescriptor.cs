using System;
using System.IO;
using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps
{
    public class BitmapDescriptor : BindableObject
    {
        public string Id
        {
            get => (string)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public BitmapDescriptorType Type
        {
            get => (BitmapDescriptorType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public string BundleName
        {
            get => (string)GetValue(BundleNameProperty);
            set => SetValue(BundleNameProperty, value);
        }

        public Stream Stream
        {
            get => (Stream)GetValue(StreamProperty);
            set => SetValue(StreamProperty, value);
        }

        public string AbsolutePath
        {
            get => (string)GetValue(AbsolutePathProperty);
            set => SetValue(AbsolutePathProperty, value);
        }

        public View View
        {
            get => (View)GetValue(ViewProperty);
            set => SetValue(ViewProperty, value);
        }

        public float IconScale
        {
            get => (float)GetValue(IconScaleProperty);
            set => SetValue(IconScaleProperty, value);
        }

        protected override void OnBindingContextChanged()
        {
            Console.WriteLine($"[{Id}] Context Changed: " + BindingContext?.GetType().FullName ?? "null");
            base.OnBindingContextChanged();
        }

        public static BindableProperty IdProperty = BindableProperty.Create(
            nameof(Id),
            typeof(string),
            typeof(BitmapDescriptor),
            defaultBindingMode: BindingMode.TwoWay);

        public static BindableProperty IconScaleProperty = BindableProperty.Create(
            nameof(IconScale),
            typeof(float),
            typeof(BitmapDescriptor),
            defaultValue: 1.0f,
            defaultBindingMode: BindingMode.TwoWay);

        public static BindableProperty TypeProperty = BindableProperty.Create(
            nameof(Type),
            typeof(BitmapDescriptorType),
            typeof(BitmapDescriptor),
            defaultValue: BitmapDescriptorType.Default,
            defaultBindingMode: BindingMode.TwoWay);

        public static BindableProperty ColorProperty = BindableProperty.Create(
            nameof(Color),
            typeof(Color),
            typeof(BitmapDescriptor),
            defaultValue: Color.Red,
            defaultBindingMode: BindingMode.TwoWay);

        public static BindableProperty BundleNameProperty = BindableProperty.Create(
            nameof(BundleName),
            typeof(string),
            typeof(BitmapDescriptor),
            defaultBindingMode: BindingMode.TwoWay);

        public static BindableProperty StreamProperty = BindableProperty.Create(
            nameof(Stream),
            typeof(Stream),
            typeof(BitmapDescriptor),
            defaultBindingMode: BindingMode.TwoWay);

        public static BindableProperty AbsolutePathProperty = BindableProperty.Create(
            nameof(AbsolutePath),
            typeof(string),
            typeof(BitmapDescriptor),
            defaultBindingMode: BindingMode.TwoWay);

        public static BindableProperty ViewProperty = BindableProperty.Create(
            nameof(View),
            typeof(View),
            typeof(BitmapDescriptor),
            defaultBindingMode: BindingMode.TwoWay);

        public BitmapDescriptor()
        {
            if (Id == null) Id = Guid.NewGuid().ToString();
        }

        internal static BitmapDescriptor DefaultMarker(Color color, string id)
        {
            return new BitmapDescriptor()
            {
                Id = id,
                Type = BitmapDescriptorType.Default,
                Color = color
            };
        }

        internal static BitmapDescriptor FromBundle(string bundleName, string id)
        {
            return new BitmapDescriptor()
            {
                Id = id,
                Type = BitmapDescriptorType.Bundle,
                BundleName = bundleName
            };
        }

        internal static BitmapDescriptor FromStream(Stream stream, string id)
        {
            return new BitmapDescriptor()
            {
                Id = id,
                Type = BitmapDescriptorType.Stream,
                Stream = stream
            };
        }

        internal static BitmapDescriptor FromPath(string absolutePath, string id)
        {
            return new BitmapDescriptor()
            {
                Id = id,
                Type = BitmapDescriptorType.AbsolutePath,
                AbsolutePath = absolutePath
            };
        }

        internal static BitmapDescriptor FromView(View view, string id)
        {
            return new BitmapDescriptor()
            {
                Id = id,
                Type = BitmapDescriptorType.View,
                View = view
            };
        }
    }
}

