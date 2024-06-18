// Copyright 2022-2024 Niantic.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// The NetTapManager manages the NetTapTargets in the scene. It listens for tap inputs and then
    /// propogates those to the network.
    /// </summary>
    public class NetTapManager : MonoBehaviour
    {
        [SerializeField]
        private EmojiGardenController _multiplayerController;

        [SerializeField]
        private TouchManager _touchManager;

        private Dictionary<Guid, INetTapListener> _listeners = new();

        public void ReadyForTaps()
        {
            _touchManager.TouchUpEvent.AddListener(OnTouchUp);
        }

        protected void OnEnable()
        {
            _multiplayerController.OnTapMessage += OnTapMessage;
        }

        protected void OnDisable()
        {
            _touchManager.TouchUpEvent.RemoveListener(OnTouchUp);
            _multiplayerController.OnTapMessage -= OnTapMessage;
        }

        public void AddTapTarget(NetTapTarget target, INetTapListener listener)
        {
            _listeners.Add(target.UniqueNetworkId, listener);
        }

        public void RemoveTapTarget(NetTapTarget target)
        {
            _listeners.Remove(target.UniqueNetworkId);
        }

        private void SendNetworkTap(NetTapTarget target)
        {
            var msg = SerializeNetworkTap(
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                target.UniqueNetworkId);
            _multiplayerController.SendTapMessage(msg);
        }

        private void OnTouchUp(Vector2 screenPose)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPose);

            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                var tappedObject = hit.collider.gameObject.GetComponent<NetTapTarget>();

                if (tappedObject)
                {
                    SendNetworkTap(tappedObject);
                }
            }
        }

        private void OnTapMessage(byte[] message, GameObject playerObj, bool isCurrentClient)
        {
            DeserializeNetworkTap(message, out var targetId, out var timestamp);
            if (!_listeners.ContainsKey(targetId))
            {
                return;
            }

            _listeners[targetId].NetworkTap(timestamp, playerObj, isCurrentClient);
        }

        private static byte[] SerializeNetworkTap(long timestamp, Guid tapTarget)
        {
            var byteArray = new byte[16 + sizeof(long)];

            unsafe
            {
                fixed (byte* ptrBytes = byteArray)
                {
                    int walker = 0;
                    walker = PrimativeWriter.Write(ptrBytes, walker, tapTarget.ToByteArray());
                    walker = PrimativeWriter.Write<long>(ptrBytes, walker, timestamp);
                }
            }

            return byteArray;
        }

        private static void DeserializeNetworkTap(byte[] byteArray, out Guid tapTarget, out long timestamp)
        {
            unsafe
            {
                fixed (byte* ptrBytes = byteArray)
                {
                    tapTarget = new Guid(new ReadOnlySpan<byte>(ptrBytes, 16));

                    int walker = 16;
                    walker = PrimativeReader.Read<long>(ptrBytes, walker, out timestamp);
                }
            }
        }
    }
}