using System;
using System.IO;

namespace RedCorners.Forms.GoogleMaps.Internals
{
    internal sealed class TakeSnapshotMessage
    {
        public Action<Stream> OnSnapshot { get; }

        public TakeSnapshotMessage(Action<Stream> onSnapshot)
        {
            OnSnapshot = onSnapshot;
        }
    }
}