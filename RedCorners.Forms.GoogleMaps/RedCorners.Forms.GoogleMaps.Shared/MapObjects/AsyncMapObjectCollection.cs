using Neat.Map.Models;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RedCorners.Forms.GoogleMaps
{
    public class AsyncMapObjectCollection : MapObjectCollection
    {
        public Distance QueryThreshold { get; set; } = Distance.FromMeters(200);
        public Distance CullUpdateThreshold { get; set; } = Distance.FromMeters(100);

        protected MapRegion Region { get; private set; }
        protected Position Center { get; private set; }
        protected int? MaxRunningTasks { get; set; } = 1;

        Position? lastQueryCenter, lastCullCenter;
        volatile int activeTasksCount = 0;

        public override void UpdateMapRegion(MapRegion region)
        {
            base.UpdateMapRegion(region);
            Region = region;

            if (region == null)
                return;

            if (!IsVisible)
                return;


            Center = region.GetCenter();

            if (lastCullCenter.HasValue && CullUpdateThreshold.Meters > 0 &&
                MapLocationSystem.CalculateDistance(Center, lastCullCenter.Value) < CullUpdateThreshold)
                return;

            if (MaxRunningTasks.HasValue && activeTasksCount >= MaxRunningTasks)
            {
                Push();
                return;
            }

            if (lastQueryCenter.HasValue && QueryThreshold.Meters > 0 &&
                MapLocationSystem.CalculateDistance(Center, lastQueryCenter.Value) < QueryThreshold)
            {
                Push();
                return;
            }

            lastQueryCenter = Center;
            Task.Run(DoAsync);
        }

        async Task DoAsync()
        {
            try
            {
                activeTasksCount++;
                await QueryAsync();
            }
            finally
            {
                activeTasksCount--;
                Push();
            }
        }

        protected virtual Task QueryAsync()
        {
            return Task.CompletedTask;
        }

        void Push()
        {
            lastCullCenter = Center;
            if (Items.Count() > 0)
                TriggerCollectionChange();
        }
    }
}
