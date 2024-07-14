using System;
using Google.Maps;
using RedCorners.Forms.GoogleMaps.iOS;

namespace RedCorners.Forms
{
    public static class GoogleMapsSystem
    {
        public static bool IsInitialized { get; private set; }

        public static void Init(string apiKey, PlatformConfig config = null)
        {
            MapServices.ProvideApiKey(apiKey);
            GeocoderBackend.Register();
            MapRenderer.Config = config ?? new PlatformConfig();
            IsInitialized = true;
        }
    }
}

