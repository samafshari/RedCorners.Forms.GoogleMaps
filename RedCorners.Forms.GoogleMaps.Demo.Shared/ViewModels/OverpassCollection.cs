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
    public class OverpassCollection : AsyncMapObjectCollection
    {
        readonly OverpassClient client = new OverpassClient();
        readonly string[] amenities;
        readonly double radius; 

        public OverpassCollection(IEnumerable<string> amenities, double radius)
        {
            this.amenities = amenities.ToArray();
            this.radius = radius;
        }

        protected override async Task QueryAsync()
        {
            var nodes = await client.SearchAmenityNodes(
                amenities,
                Center.Latitude,
                Center.Longitude,
                radius);
                  
            if (nodes != null)
            {
                AddNodes(nodes);
            }
        }

        void AddNodes(AmenityNode[] nodes)
        {
            foreach (var node in nodes)
            {
                if (Items.Any(x => x.Tags.Contains(node.Id)))
                    continue;

                var pin = MakePin(node);
                pin.Tags.Add(node.Id);
                Add(pin);
            }
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
