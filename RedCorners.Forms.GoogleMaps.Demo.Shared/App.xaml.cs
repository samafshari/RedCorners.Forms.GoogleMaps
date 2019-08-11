using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using RedCorners.Forms.Systems;
using System.Threading.Tasks;
using RedCorners.Forms;

namespace RedCorners.Forms.GoogleMaps.Demo
{
    public partial class App : Application2
    {
        public static string MapStyle = "\n[\n    {\n        \"featureType\": \"all\",\n        \"elementType\": \"labels.text.fill\",\n        \"stylers\": [\n            {\n                \"color\": \"#ffffff\"\n            }\n        ]\n    },\n    {\n        \"featureType\": \"all\",\n        \"elementType\": \"labels.text.stroke\",\n        \"stylers\": [\n            {\n                \"color\": \"#000000\"\n            },\n            {\n                \"lightness\": 13\n            }\n        ]\n    },\n    {\n        \"featureType\": \"administrative\",\n        \"elementType\": \"geometry.fill\",\n        \"stylers\": [\n            {\n                \"color\": \"#000000\"\n            }\n        ]\n    },\n    {\n        \"featureType\": \"administrative\",\n        \"elementType\": \"geometry.stroke\",\n        \"stylers\": [\n            {\n                \"color\": \"#144b53\"\n            },\n            {\n                \"lightness\": 14\n            },\n            {\n                \"weight\": 1.4\n            }\n        ]\n    },\n    {\n        \"featureType\": \"landscape\",\n        \"elementType\": \"all\",\n        \"stylers\": [\n            {\n                \"color\": \"#08304b\"\n            }\n        ]\n    },\n    {\n        \"featureType\": \"poi\",\n        \"elementType\": \"geometry\",\n        \"stylers\": [\n            {\n                \"color\": \"#0c4152\"\n            },\n            {\n                \"lightness\": 5\n            }\n        ]\n    },\n    {\n        \"featureType\": \"road.highway\",\n        \"elementType\": \"geometry.fill\",\n        \"stylers\": [\n            {\n                \"color\": \"#000000\"\n            }\n        ]\n    },\n    {\n        \"featureType\": \"road.highway\",\n        \"elementType\": \"geometry.stroke\",\n        \"stylers\": [\n            {\n                \"color\": \"#0b434f\"\n            },\n            {\n                \"lightness\": 25\n            }\n        ]\n    },\n    {\n        \"featureType\": \"road.arterial\",\n        \"elementType\": \"geometry.fill\",\n        \"stylers\": [\n            {\n                \"color\": \"#000000\"\n            }\n        ]\n    },\n    {\n        \"featureType\": \"road.arterial\",\n        \"elementType\": \"geometry.stroke\",\n        \"stylers\": [\n            {\n                \"color\": \"#0b3d51\"\n            },\n            {\n                \"lightness\": 16\n            }\n        ]\n    },\n    {\n        \"featureType\": \"road.local\",\n        \"elementType\": \"geometry\",\n        \"stylers\": [\n            {\n                \"color\": \"#000000\"\n            }\n        ]\n    },\n    {\n        \"featureType\": \"transit\",\n        \"elementType\": \"all\",\n        \"stylers\": [\n            {\n                \"color\": \"#146474\"\n            }\n        ]\n    },\n    {\n        \"featureType\": \"water\",\n        \"elementType\": \"all\",\n        \"stylers\": [\n            {\n                \"color\": \"#021019\"\n            }\n        ]\n    }\n]";

        public override void InitializeSystems()
        {
            InitializeComponent();
            base.InitializeSystems();
        }

        public override Page GetFirstPage() => new Views.MainPage();
    }
}
