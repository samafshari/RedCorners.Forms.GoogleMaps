using System;
namespace RedCorners.Forms.GoogleMaps.Internals
{
    internal interface IMapRequestDelegate
    {
        void OnMoveToRegionRequest(MoveToRegionMessage m);
        void OnMoveCameraRequest(CameraUpdateMessage m);
    }
}
