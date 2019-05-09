using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using RedCorners.Forms.Views;
using RedCorners.Forms.Systems;
using System.Threading.Tasks;

namespace RedCorners.Forms.GoogleMaps.Demo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            ShowFirstPage();
        }

        async void ShowFirstPage()
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                MainPage = new ContentPage
                {
                    Content = new ActivityIndicator
                    {
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        IsRunning = true,
                        WidthRequest = 32,
                        HeightRequest = 32
                    }
                };

                while (!NotchSystem.Instance.HasWindowInformation)
                    await Task.Delay(50);
            }

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
