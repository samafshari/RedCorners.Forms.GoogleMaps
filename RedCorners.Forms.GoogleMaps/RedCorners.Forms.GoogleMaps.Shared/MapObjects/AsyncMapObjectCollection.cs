using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedCorners.Forms.GoogleMaps
{
    public class AsyncMapObjectCollection : MapObjectCollection
    {
        public Distance QueryThreshold { get; set; } = Distance.FromMeters(100);
        public Distance CullUpdateThreshold { get; set; } = Distance.FromMeters(50);
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

            if (lastCenter.HasValue && CullUpdateThreshold.Meters > 0 &&
                MapLocationSystem.CalculateDistance(Center, lastCenter.Value) < CullUpdateThreshold)
                return;
            
            if (MaxRunningTasks.HasValue && activeTasksCount >= MaxRunningTasks)
            {
                Push();
                return;
            }

            if (lastCenter.HasValue && QueryThreshold.Meters > 0 &&
                MapLocationSystem.CalculateDistance(Center, lastCenter.Value) < QueryThreshold)
            {
                Push();
                return;
            }

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
                Push();
            }
        }

        protected virtual Task QueryAsync()
        {
            return Task.CompletedTask;
        }

        void Push()
        {
            if (Objects.Count > 0)
                TriggerCollectionChange();
        }
    }
}
