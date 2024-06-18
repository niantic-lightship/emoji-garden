// Copyright 2022-2024 Niantic.
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Niantic.Lightship.AR.LocationAR;
using UnityEngine;
using Niantic.Lightship.SharedAR.Networking;
using Niantic.Lightship.SharedAR.Rooms;

namespace Niantic.Lightship.AR.Samples
{
    public class EmojiGardenController : MonoBehaviour, ILocationBasedExperience
    {
        [Header("Prefabs")]
        [SerializeField]
        private GameObject _playerObjectPrefab;

        [Header("In Scene References")]
        [SerializeField]
        private Garden _garden;

        [SerializeField]
        private EmojiGardenUI _ui;

        [SerializeField]
        private NetTapManager _netTapManager;

        [SerializeField]
        private Camera _camera;

        private const string _roomNamePrefix = "emojiGarden_";

        private IRoom _room;
        private INetworking _network;
        private Dictionary<PeerID, GameObject> _playerObjects = new();
        private GameObject _origin;

        private GameObject _myPlayerObject;

        private bool _receivedGardenPlacementPacket = false;
        private Vector3 _gardenPosition;
        private Quaternion _gardenRotation;

        private List<Action> _tasks = new();

        public Action<byte[], GameObject, bool> OnTapMessage; // Listened to by NetTapManager

        private LocationBasedExperienceController _controller;

        private const int POSE_UPDATE_TAG = 42;
        private const int HELLO_TAG = POSE_UPDATE_TAG + 1;
        private const int TAP_TAG = HELLO_TAG + 1;

        public void StartExperience
        (
            ILocationBasedExperience.LocationArgs data,
            LocationBasedExperienceController controller
        )
        {
            gameObject.SetActive(true);
            _controller = controller;

            StartEmojiGardenAt(data.Location, data.ReadableLocationName);
        }

        public void PauseDueToLocalizationLost()
        {
            _ui.gameObject.SetActive(false);
        }

        public void UnpauseDueToLocalizationRegained()
        {
            _ui.gameObject.SetActive(true);
        }

        private void StartEmojiGardenAt(ARLocation location, string locationTextName)
        {
            _garden.SetLocationText(locationTextName);

            _origin = location.gameObject;

            // Do not await JoinRoom() because we use network events to wait for connection
#pragma warning disable CS4014
            JoinRoomAsync(location.Payload.ToBase64());
#pragma warning restore CS4014

            _ui.gameObject.SetActive(true);
            _ui.BackToMap += BackToMap;
        }

        protected void OnEnable()
        {
            _ui.gameObject.SetActive(false);
        }

        protected void OnDisable()
        {
            LeaveRoom();
        }

        private void LeaveRoom()
        {
            _ui.HidePlayerCounter();
            _receivedGardenPlacementPacket = false;
            _playerObjects.Clear();
            if (_network != null)
            {
                _network.PeerAdded -= OnPeerAdded;
                _network.PeerRemoved -= OnPeerRemoved;
                _network.NetworkEvent -= OnNetworkEvent;
                _network.DataReceived -= OnPeerDataReceived;
                _network = null;

                _room.Leave();
            }
        }

        // Return plain Task as we react based on Network events instead of this task
        private async Task JoinRoomAsync(string roomName)
        {
            var finalName =  _roomNamePrefix+roomName;
            var roomResult = await RoomManagementService.GetOrCreateRoomAsync(
                finalName,
                "Emoji garden at this VPS location!",
                32
            );
            var status = roomResult.Status;

            if (status != RoomManagementServiceStatus.Ok)
            {
                Debug.LogError("GetOrCreatRoomAsync failed. Check ARDK logs.");
                return;
            }

            _room = roomResult.Room;
            _room.Initialize();
            _network = _room.Networking;
            _network.PeerAdded += OnPeerAdded;
            _network.PeerRemoved += OnPeerRemoved;
            _network.NetworkEvent += OnNetworkEvent;
            _network.DataReceived += OnPeerDataReceived;

            _room.Join(); // This will invoke the NetworkEvent callback
        }

        private void HandleTapMessage(DataReceivedArgs args)
        {
            if (args.PeerID.Equals(_network.SelfPeerID))
            {
                OnTapMessage(args.CopyData(), _myPlayerObject, true);
            }
            else
            {
                OnTapMessage
                (
                    args.CopyData(),
                    _playerObjects[args.PeerID],
                    false
                );
            }
        }

        private void HandlePoseUpdateMessage(DataReceivedArgs args)
        {
            GameObject positionToUpdate;
            if (!_playerObjects.ContainsKey(args.PeerID))
            {
                positionToUpdate = MakePlayerObject(false);
                _playerObjects.Add(args.PeerID, positionToUpdate);
            }
            else
            {
                positionToUpdate = _playerObjects[args.PeerID];
            }

            DeserializePose(args.CopyData(), out var position, out var rotation);
            positionToUpdate.transform.localPosition = position;
            positionToUpdate.transform.localRotation = rotation;
        }

        private void HandleHelloMessage(DataReceivedArgs args)
        {
            // Ignore if we've already gotten a hello packet from someone.
            if (_receivedGardenPlacementPacket)
            {
                return;
            }

            _garden.NetworkPlacement(args.CopyData());
            _netTapManager.ReadyForTaps();
            _receivedGardenPlacementPacket = true;
        }

        private void OnPeerDataReceived(DataReceivedArgs args)
        {
            lock (_tasks)
            {
                _tasks.Add(() =>
                {
                    if (_network == null)
                    {
                        return;
                    }

                    if (args.Tag == TAP_TAG) // Tap from network (could be our own tap)
                    {
                        HandleTapMessage(args);
                        return;
                    }

                    // The rest of the messages should be ignored by the user who sent them
                    if (args.PeerID.Equals(_network.SelfPeerID))
                    {
                        return;
                    }

                    if (args.Tag == POSE_UPDATE_TAG) // Player pose update
                    {
                        HandlePoseUpdateMessage(args);
                        return;
                    }

                    if (args.Tag == HELLO_TAG) // Hello packet
                    {
                        HandleHelloMessage(args);
                        return;
                    }
                });
            }
        }

        private void OnNetworkEvent(NetworkEventArgs args)
        {
            if (args.networkEvent == NetworkEvents.ArdkShutdown)
            {
                LeaveRoom();
                return;
            }

            if (args.networkEvent == NetworkEvents.Disconnected)
            {
                lock (_tasks)
                {
                    _tasks.Add(() => { BackToMap(); });
                }

                return;
            }

            bool isFirst = _network.PeerIDs.Count == 1;

            lock (_tasks)
            {
                _tasks.Add
                (
                    () =>
                    {
                        if (args.networkEvent == NetworkEvents.Connected)
                        {
                            _myPlayerObject = MakePlayerObject(true);

                            if (isFirst)
                            {
                                _garden.OnPlacementFinished += SendGardenPlacementToPeers;
                                _garden.BeginPlacement(_origin.transform);
                            }
                            else if (_receivedGardenPlacementPacket == false)
                            {
                                _garden.WaitForPlacement(_origin.transform);
                            }
                        }

                        UpdatePlayerCounter();
                    });
            }
        }

        private void OnPeerAdded(PeerIDArgs args)
        {
            lock (_tasks)
            {
                _tasks.Add
                (
                    () =>
                    {
                        if (_receivedGardenPlacementPacket == false || _network == null)
                            return;

                        var data = _garden.MakeGardenInitMessageForNewPeer(true);
                        _network.SendData(new List<PeerID>() { args.PeerID }, HELLO_TAG, data);
                        UpdatePlayerCounter();
                    });
            }
        }

        private void OnPeerRemoved(PeerIDArgs args)
        {
            if (_network == null)
                return;

            var ids = _network.PeerIDs;
            lock (_tasks)
            {
                _tasks.Add
                (
                    () =>
                    {
                        if (_network == null)
                            return;

                        if (_playerObjects.ContainsKey(args.PeerID))
                        {
                            _playerObjects.Remove(args.PeerID, out var obj);
                            Destroy(obj);
                        }

                        UpdatePlayerCounter();
                        UpdateGardenPlacer(ids);
                    });
            }
        }

        private GameObject MakePlayerObject(bool isCurrentClient)
        {
            var obj = Instantiate(_playerObjectPrefab, _origin.transform);
            obj.GetComponent<PlayerVFX>().IsCurrentClient(isCurrentClient);
            if (isCurrentClient)
            {
                var follow = obj.GetComponent<FollowTarget>();
                follow.enabled = true;
                follow.Target = _camera.transform;
            }
            return obj;
        }

        // Called by NetTapManager
        public void SendTapMessage(byte[] msg)
        {
            _network.SendData(_network.PeerIDs, TAP_TAG, msg);
        }

        private void SendGardenPlacementToPeers()
        {
            _netTapManager.ReadyForTaps();

            if (_network == null)
            {
                return;
            }

            var data = _garden.MakeGardenInitMessageForNewPeer(false);
            _network.SendData(new List<PeerID>(), HELLO_TAG, data);
            _receivedGardenPlacementPacket = true;

            _garden.OnPlacementFinished -= SendGardenPlacementToPeers;
        }

        protected void Update()
        {
            lock (_tasks)
            {
                foreach (var task in _tasks)
                    task();

                _tasks.Clear();
            }


            UpdatePlayerCounter();
        }

        protected void FixedUpdate()
        {
            if (_myPlayerObject == null || _network == null)
            {
                return;
            }

            var data = SerializePose
            (
                _myPlayerObject.transform.localPosition,
                _myPlayerObject.transform.localRotation
            );
            _network.SendData(new List<PeerID>(), POSE_UPDATE_TAG, data);
        }

        private void UpdatePlayerCounter()
        {
            int playerCount = 0;
            if (_network != null)
            {
                playerCount = _network.PeerIDs.Count;
            }

            _ui.UpdatePlayerCounter(playerCount);
        }

        private void UpdateGardenPlacer(List<PeerID> ids)
        {
            if (_network == null || _receivedGardenPlacementPacket == true)
            {
                return;
            }

            uint smallestId = UInt32.MaxValue;
            foreach (var id in ids)
            {
                var idUint = id.ToUint32();
                if (idUint < smallestId)
                {
                    smallestId = idUint;
                }
            }

            if (_network.SelfPeerID.ToUint32() == smallestId)
            {
                _garden.OnPlacementFinished += SendGardenPlacementToPeers;
                _garden.BeginPlacement(_origin.transform);
            }
        }

        private void BackToMap()
        {
            _ui.BackToMap -= BackToMap;

            // Cleanup other garden stuffs?
            _garden.Reset();
            LeaveRoom();

            _controller.ReturnToMap();
        }

////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                                             Serialization Helpers
////////////////////////////////////////////////////////////////////////////////////////////////////
        private static int SizeOfPose()
        {
            return 7 * sizeof(float);
        }

        private static byte[] SerializePose(Vector3 position, Quaternion rotation)
        {
            byte[] byteArray = new byte[SizeOfPose()];

            unsafe
            {
                fixed (byte* ptrByteArray = byteArray)
                {
                    int walker = 0;
                    walker = PrimativeWriter.Write(ptrByteArray, walker, position);
                    walker = PrimativeWriter.Write(ptrByteArray, walker, rotation);
                }
            }

            return byteArray;
        }

        private static void DeserializePose(
            byte[] byteArray,
            out Vector3 position,
            out Quaternion rotation)
        {
            unsafe
            {
                fixed (byte* ptrByteArray = byteArray)
                {
                    int walker = 0;
                    walker = PrimativeReader.Read(ptrByteArray, walker, out position);
                    walker = PrimativeReader.Read(ptrByteArray, walker, out rotation);
                }
            }
        }
    }
}
