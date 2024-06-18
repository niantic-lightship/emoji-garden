// Copyright 2022-2024 Niantic.
using System;
using System.Collections.Generic;
using System.Linq;
using Niantic.Lightship.AR.VpsCoverage;
using Niantic.Lightship.Maps.MapLayers.Components;
using Niantic.Lightship.Maps.ObjectPools;
using UnityEngine;
using UnityEngine.Rendering;
using MapsLatLng = Niantic.Lightship.Maps.Core.Coordinates.LatLng;

namespace Niantic.Lightship.AR.Samples {
    public class MapContentController : MonoBehaviour
    {
        [SerializeField]
        private LayerGameObjectPlacement mapTargetSpawner;

        [SerializeField]
        private LayerGameObjectPlacement userMarkerSpawner;

        [SerializeField]
        private float selectedTargetScaleMultiplier;

        [SerializeField]
        private float unselectedTargetScaleMultiplier;

        private List<VpsTargetMapMarker> _cachedMarkers = new List<VpsTargetMapMarker>();
        private int _markerDidAppearCounter;
        private PooledObject<GameObject> userMarker;
        private VpsTargetMapMarker selectedMarker;

        private Queue<Action> _fillTargetRequests = new();
        private int _maxConcurrentRequests = 3;

        public Action MarkerWasSelected;
        public Action MarkerWasDeselected;
        public Action AllMarkersHaveAdded;
        
        public VpsTargetMapMarker SelectedMarker => selectedMarker;
        public List<VpsTargetMapMarker> CachedMarkers => _cachedMarkers;
        
        public void AddUserToMap(MapsLatLng position)
        {
            userMarker = userMarkerSpawner.PlaceInstance(position, "User");
        }

        public void AnimateMarker()
        {
            userMarker.Value.GetComponent<UserMarker>().LoadingAnimation();
        }

        public void AddTargetsToMap(List<AreaTarget> areaTargets, bool productionFlag, CoverageClientManager manager)
        {
            foreach (var areaTarget in areaTargets)
            {
                // All public locations should have images from the coverage client.
                // Assume locations that are missing URLs are private locations
                bool isPrivateLocation = string.IsNullOrEmpty(areaTarget.Target.ImageURL);

                // Filter targets to production-only if the flag isn't set.
                bool meetsProductionCriteria = !productionFlag ||
                    areaTarget.Area.LocalizabilityQuality == CoverageArea.Localizability.PRODUCTION;

                if (!(meetsProductionCriteria || isPrivateLocation))
                {
                    continue;
                }

                var contains = _cachedMarkers.Where(marker => marker.Target.Name == areaTarget.Target.Name);
                if (contains.Any())
                {
                    continue;
                }

                _fillTargetRequests.Enqueue(() => {
                    FillTargetItem(areaTarget.Area, areaTarget.Target, manager);
                });
            }
        }

        public void ForceSelectedMarker(VpsTargetMapMarker marker)
        {
            TouchResponder responder = marker.gameObject.GetComponent<TouchResponder>();
            if (responder != null)
            {
                MarkerWasSelectedResponse(responder);
            }
        }

        public VpsTargetMapMarker AddNonVPSTargetToMap(String name, LatLng location)
        {
            MapsLatLng coordinates = new MapsLatLng(location.Latitude, location.Longitude);
            var markerName = $"Target_{name}";
            var marker = mapTargetSpawner.PlaceInstance(coordinates, markerName);
            var newMarker = marker.Value.GetComponent<VpsTargetMapMarker>();
            var touchResponder = newMarker.gameObject.GetComponent<TouchResponder>();
            touchResponder.DidTouchUpEvent += MarkerWasSelectedResponse;

            return newMarker;
        }

        protected void Update()
        {
            for (int i = 0; i < _maxConcurrentRequests; ++i)
            {
                if (_fillTargetRequests.TryDequeue(out var req))
                {
                    req();

                    if (_markerDidAppearCounter <= 0)
                    {
                        MarkerDidAppear(null);
                    }
                }
            }
        }

        public void UpdateUserMarker(Vector3 scenePosition)
        {
            if (userMarker.Value != null)
            {
                scenePosition.y = userMarker.Value.transform.position.y;
                userMarker.Value.transform.position = scenePosition;
            }
        }

        public void DeselectMarker()
        {
            if (selectedMarker)
            {
                selectedMarker.AnimateTarget(unselectedTargetScaleMultiplier);
                selectedMarker.GetComponent<SortingGroup>().sortingOrder = 1;
                selectedMarker = null;
                MarkerWasDeselected?.Invoke();
            }
        }

        private void FillTargetItem(CoverageArea area, LocalizationTarget target, CoverageClientManager manager)
        {
            MapsLatLng coordinates = new MapsLatLng(target.Center.Latitude, target.Center.Longitude);
            var markerName = $"Target_{target.Name}";
            // need adjust coords due to offset
            var marker = mapTargetSpawner.PlaceInstance(coordinates, markerName);
            var newMarker = marker.Value.GetComponent<VpsTargetMapMarker>();
            _markerDidAppearCounter += 1;
            newMarker.MarkerDidAppear += MarkerDidAppear;
            newMarker.Initialize(target, manager, unselectedTargetScaleMultiplier);
            var touchResponder = newMarker.gameObject.GetComponent<TouchResponder>();
            touchResponder.DidTouchUpEvent += MarkerWasSelectedResponse;
            _cachedMarkers.Add(newMarker);
        }

        private void MarkerDidAppear(VpsTargetMapMarker marker)
        {
            _markerDidAppearCounter -= 1;
            if (_markerDidAppearCounter <= 0)
            {
                AllMarkersHaveAdded?.Invoke();
            }
        }

        private void MarkerWasSelectedResponse(TouchResponder obj)
        {
            var hitMarker = obj.gameObject.GetComponent<VpsTargetMapMarker>();
            if (hitMarker == null || selectedMarker == hitMarker)
            {
                // If we haven't hit anything, or if we've hit the already selected marker.
                return;
            }
            if (selectedMarker != null) //if we have a previously selected marker, we need to animate it back to a non selected state
            {
                selectedMarker.GetComponent<SortingGroup>().sortingOrder = 1;
                selectedMarker.AnimateTarget(unselectedTargetScaleMultiplier);
            }
            hitMarker.GetComponent<SortingGroup>().sortingOrder = 2;
            hitMarker.AnimateTarget(selectedTargetScaleMultiplier); //animate the newly selected marker
            selectedMarker = hitMarker; //cache a reference to our newly selected marker.
            MarkerWasSelected?.Invoke();
        }
    }
}
