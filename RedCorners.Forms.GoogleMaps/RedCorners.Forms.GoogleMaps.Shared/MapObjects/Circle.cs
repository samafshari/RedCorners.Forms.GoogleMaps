using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using Neat.Map.Models;

namespace RedCorners.Forms.GoogleMaps
{
    public sealed class Circle : MapObject
    {
        public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(nameof(StrokeWidth) , typeof(float), typeof(Circle), 1f);
        public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(nameof(StrokeColor), typeof(Color), typeof(Circle), Color.Blue);
        public static readonly BindableProperty FillColorProperty = BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(Circle), Color.Blue);
        public static readonly BindableProperty IsClickableProperty = BindableProperty.Create("IsClickable", typeof(bool), typeof(Circle), false);

        public static readonly BindableProperty CenterProperty = BindableProperty.Create(nameof(Center), typeof(Position), typeof(Circle), default(Position));
        public static readonly BindableProperty RadiusProperty = BindableProperty.Create(nameof(Radius), typeof(Distance), typeof(Circle), Distance.FromMeters(1));
        public static readonly BindableProperty ZIndexProperty = BindableProperty.Create(nameof(ZIndex), typeof(int), typeof(Circle), 0);

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
            nameof(Command),
            typeof(ICommand),
            typeof(Circle),
            default(ICommand));

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
            nameof(CommandParameter),
            typeof(object),
            typeof(Circle),
            null);

        public float StrokeWidth
        {
            get { return (float)GetValue(StrokeWidthProperty); }
            set { SetValue(StrokeWidthProperty, value); }
        }

        public Color StrokeColor
        {
            get { return (Color)GetValue(StrokeColorProperty); }
            set { SetValue(StrokeColorProperty, value); }
        }

        public Color FillColor
        {
            get { return (Color)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }

        public bool IsClickable
        {
            get { return (bool)GetValue(IsClickableProperty); }
            set { SetValue(IsClickableProperty, value); }
        }

        public Position Center
        {
            get { return (Position)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        public Distance Radius
        {
            get { return (Distance)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public int ZIndex
        {
            get { return (int)GetValue(ZIndexProperty); }
            set { SetValue(ZIndexProperty, value); }
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public object NativeObject { get; internal set; }

        public event EventHandler Clicked;

        public Circle()
        {
        }

        internal bool SendTap()
        {
            if (Command?.CanExecute(CommandParameter) ?? false)
                Command?.Execute(CommandParameter);

            EventHandler handler = Clicked;
            if (handler == null)
                return false;

            handler(this, EventArgs.Empty);

            return true;
        }

        public override bool ShouldCull(MapRegion region)
        {
            return !region.Contains(Center, Radius);
        }

        public override bool ShouldCull(Position position, Distance distance)
        {
            return MapLocationSystem.CalculateDistance(position, Center) <= distance;
        }

        internal override Position? GetRelativePosition(Position reference)
        {
            return Center;
        }
    }
}

