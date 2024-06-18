// Copyright 2022-2024 Niantic.
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    public interface INetTapListener
    {
        // Tap came in from the network
        void NetworkTap(long timestamp, GameObject playerObj, bool isCurrentClient);
    }
}
