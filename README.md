Enhanced Google Maps for Xamarin Forms. Xamarin.Forms.GoogleMaps fork.

Nuget: [https://www.nuget.org/packages/RedCorners.Forms.GoogleMaps](https://www.nuget.org/packages/RedCorners.Forms.GoogleMaps)
Documentation: [http://redcorners.com/googlemaps/](http://redcorners.com/googlemaps/)

## Getting Started

`RedCorners.Forms.GoogleMaps` provides facilities to render and manage Google Maps based views on your iOS and Android Xamarin.Forms projects. In order to use `RedCorners.Forms.GoogleMaps`, you need to have the latest versions of the following packages installed:

- [`RedCorners.Forms.GoogleMaps`](https://www.nuget.org/packages/RedCorners.Forms.GoogleMaps/)
- [`RedCorners.Forms`](https://www.nuget.org/packages/RedCorners.Forms/)
- [`RedCorners`](https://www.nuget.org/packages/RedCorners/)

In case you wish to access the device's location, you must ask for the required permissions prior to enabling _My Location_ on the Google Maps view. Otherwise, the app will throw an exception due to the lack of required location permissions, or will not show _My Location_. These steps are platform-dependent and described below.

### iOS Setup

In your `AppDelegate` class, or before rendering the map, you have to call the `Init` method and inject your Google Maps API key:

```c#
public override bool FinishedLaunching(UIApplication app, NSDictionary options)
{
    global::Xamarin.Forms.Forms.Init();
    RedCorners.Forms.GoogleMapsSystem.Init("AIzaSyD8-xxxxxxxxxxxxxxxxxxxxxxxx");
    // ...
    LoadApplication(new App());
    return base.FinishedLaunching(app, options);
}
```

Based on your use case, you need to configure the `info.plist` file as follows:

```xml
<key>NSLocationAlwaysAndWhenInUseUsageDescription</key>
<string>This app needs access to location to continue.</string>
<key>NSLocationAlwaysUsageDescription</key>
<string>This app needs access to location to continue.</string>
<key>NSLocationWhenInUseUsageDescription</key>
<string>This app needs access to location to continue.</string>
```

### Android Setup

Depending on your use case, you may want to request _Fine_ or _Coarse_ locations. To do this, first add the following lines in your `AndroidManifest.xml` file:

```xml
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
```

You should also add your API key to the manifest:

```xml
<meta-data android:name="com.google.android.geo.API_KEY" android:value="AIzaSyD8-xxxxxxxxxxxxxxxxxxxxxxxx" />
```

In case you get the `java.lang.NoClassDefFoundError: Failed resolution of: Lorg/apache/http/ProtocolVersion;` error, the following line can help:

```xml
<uses-library android:name="org.apache.http.legacy" android:required="false" />
```

The entire manifest can look like this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest 
  xmlns:android="http://schemas.android.com/apk/res/android" 
  android:versionCode="1" 
  android:versionName="1.0" 
  package="com.redcorners.googlemaps">
	<uses-sdk android:minSdkVersion="21" android:targetSdkVersion="28" />
	<application android:label="RedCorners.Forms.GoogleMaps.Demo.Android">
    <uses-library android:name="org.apache.http.legacy" android:required="false" />
    <meta-data android:name="com.google.android.geo.API_KEY" android:value="AIzaSyD8-xxxxxxxxxxxxxxxxxxxxxxxx" />
	</application>
  <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
  <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
</manifest>
```

### Showing a basic map

The first step is including the `RedCorners.Forms.GoogleMaps` namespace in your XAML file:

```xml
xmlns:map="clr-namespace:RedCorners.Forms.GoogleMaps;assembly=RedCorners.Forms.GoogleMaps"
```

Afterwards, you can use `map:Map` or other variants of it such as `map:LocationPickerView` or `map:MapDrawView` to show a Google Map:

```xml
<map:Map
    MyLocationEnabled="True"
    Latitude="{Binding Latitude, Mode=TwoWay}" 
    Longitude="{Binding Longitude, Mode=TwoWay}">
</map:Map>
```