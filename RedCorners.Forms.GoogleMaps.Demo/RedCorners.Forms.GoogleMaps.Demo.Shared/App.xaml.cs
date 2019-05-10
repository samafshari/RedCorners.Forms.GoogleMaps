using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using RedCorners.Forms.Views;
using RedCorners.Forms.Systems;
using System.Threading.Tasks;
using RedCorners.Forms;

namespace RedCorners.Forms.GoogleMaps.Demo
{
    public partial class App : AppBase
    {
        public App()
        {
            InitializeComponent();
        }

        async void ShowFirstPage()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            var text = new Label
            {
                Text = "Hello World"
            };
            grid.Children.Add(text);
            Grid.SetRow(text, 0);

            var map = new Map
            {
                BackgroundColor = Color.Red,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            grid.Children.Add(map);
            Grid.SetRow(map, 1);

            MainPage = new AliveContentPage
            {
                Content = grid,
                FixPadding = true,
                FixBottomPadding = true
            };
        }
    }
}
