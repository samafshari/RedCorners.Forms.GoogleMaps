using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedCorners.Forms.GoogleMaps
{
    public class AsyncMapObjectCollection : MapObjectCollection
    {
        public Distance QueryThreshold { get; set; } = Distance.FromMeters(50);
        protected MapRegion Region { get; private set; }
        protected Position Center { get; private set; }
        protected int? MaxRunningTasks { get; set; } = 1;

        Position? lastCenter;
        volatile int activeTasksCount = 0;

        public override void UpdateMapRegion(MapRegion region)
        {
            base.UpdateMapRegion(region);
            Region = region;

            if (region == null)
                return;

            Center = region.GetCenter();
            
            if (MaxRunningTasks.HasValue && activeTasksCount >= MaxRunningTasks)
                return;

            if (lastCenter.HasValue && QueryThreshold.Meters > 0 &&
                MapLocationSystem.CalculateDistance(Center, lastCenter.Value) < QueryThreshold)
                return;

            if (!IsVisible)
                return;

            lastCenter = Center;
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
            }
        }

        protected virtual Task QueryAsync()
        {
            return Task.CompletedTask;
        }
    }
}
