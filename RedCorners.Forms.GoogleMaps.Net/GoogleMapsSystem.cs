﻿using System;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Maps;
using Android.OS;
using RedCorners.Forms.GoogleMaps.Android;
using RedCorners.Forms.GoogleMaps.Android.Factories;

namespace RedCorners.Forms
{
    public static class GoogleMapsSystem
    {
        public static bool IsInitialized { get; private set; }

        public static Context Context { get; private set; }

        public static void Init(Activity activity, Bundle bundle, PlatformConfig config = null)
        {
            if (IsInitialized)
                return;

            Context = activity;

            MapRenderer.Bundle = bundle;
            MapRenderer.Config = config ?? new PlatformConfig();

#pragma warning disable 618
            if (GooglePlayServicesUtil.IsGooglePlayServicesAvailable(Context) == ConnectionResult.Success)
#pragma warning restore 618
            {
                try
                {
                    MapsInitializer.Initialize(Context);
                    IsInitialized = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Google Play Services Not Found");
                    Console.WriteLine("Exception: {0}", e);
                }
            }

            GeocoderBackend.Register(Context);
        }
    }
}