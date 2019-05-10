using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
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

        public override Page GetFirstPage() => new Views.MainPage();
    }
}
