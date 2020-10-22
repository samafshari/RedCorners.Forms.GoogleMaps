using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RedCorners.Components;
using RedCorners.Forms.GoogleMaps;
using Xamarin.Forms;

namespace RedCorners.Forms.GoogleMaps.Demo.ViewModels
{
    public class OverpassCollection : MapObjectCollection
    {
        readonly OverpassClient client = new OverpassClient();
        readonly string[] amenities;
        readonly double radius; 

        volatile bool isBusy = false;
        Position? lastCenter;
        MapRegion region; 

        public OverpassCollection(IEnumerable<string> amenities, double radius)
        {
            this.amenities = amenities.ToArray();
            this.radius = radius;
        }

        public override void UpdateMapRegion(MapRegion region)
        {
            base.UpdateMapRegion(region);
            this.region = region;
            Task.Run(FetchAsync);
        }

        async Task FetchAsync()
        {
            if (region == null) return;
            if (isBusy) return;

            var center = region.GetCenter();
            try
            {
                if (lastCenter != null && MapLocationSystem.CalculateDistance(center, lastCenter.Value) < Distance.FromMeters(50))
                    return;

                try
                {
                    isBusy = true;
                    var nodes = await client.SearchAmenityNodes(
                        amenities,
                        center.Latitude,
                        center.Longitude,
                        radius);
                    
                    if (nodes != null)
                    {
                        AddNodes(nodes);
                    }
                }
                finally
                {
                    isBusy = false;
                }
            }
            finally
            {
                lastCenter = center;
            }
        }

        void AddNodes(AmenityNode[] nodes)
        {
            foreach (var node in nodes)
            {
                if (Objects.Any(x => x.Tags.Contains(node.Id)))
                    continue;

                var pin = MakePin(node);
                pin.Tags.Add(node.Id);
                Objects.Add(pin);
            }

            if (nodes.Length > 0)
                TriggerCollectionChange();
        }

        protected string GetTitle(AmenityNode node)
        {
            var result = node.Amenity.CapitalizeFirstLetter();
            if (node.Tags.TryGetValue("name", out var name))
                result = name + " | " + result;
            else if (node.Tags.TryGetValue("operator", out var op))
                result = op + " | " + result;
            return result.Replace("_", " ");
        }

        public Func<AmenityNode, BitmapDescriptor> MakeIconFunc = (AmenityNode node) =>
        {
            return BitmapDescriptorFactory.DefaultMarker(Color.Red);
        };

        virtual protected Pin MakePin(AmenityNode node)
        {
            return new Pin
            {
                Label = GetTitle(node),
                Address = node.Id,
                Position = new Position(node.Latitude, node.Longitude),
                Icon = MakeIconFunc(node),
            };
        }
    }
}
