// Copyright 2022-2024 Niantic.
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Niantic.Lightship.Maps;
using Niantic.Lightship.Maps.Core;
using MapsLatLng = Niantic.Lightship.Maps.Core.Coordinates.LatLng;

namespace Niantic.Lightship.AR.Samples {
    public class MapController : MonoBehaviour
    {
        [Header("Map Control Settings")]
        [SerializeField]
        private float _minimumMapRadius;

        [SerializeField]
        private float _maximumMapRadius;

        [SerializeField]
        private float _mapZoomPanModifier;

        [SerializeField]
        private float _recenterDuration;

        [SerializeField]
        private float _zoomDuration;

        [SerializeField]
        private int mapTileRevealThreshold = 45; //how many map tiles need to load before we can hide the loading screen.

        [SerializeField]
        private AnimationCurve _recenterAnimationCurve;

        [SerializeField]
        private LightshipMapView lightshipMapView;

        [SerializeField]
        private Camera _camera;
        
        // Variables
        private float defaultMapRadius = 500.0f; //Map Radius to set zoom
        private const string MapTileLayer = "MapTile";

        // Public Events
        public Action MapIsReady;
        public Action MapDidPan;
        public Action MapDidCompletePan;
        public Action MapWasTouched;
        
        public void InitializeMap(MapsLatLng mapCenter)
        {
            lightshipMapView.gameObject.SetActive(true);
            _camera.orthographicSize = defaultMapRadius;
            lightshipMapView.MapTileAdded += MapTileWasAdded;
            lightshipMapView.SetMapCenter(mapCenter);
            lightshipMapView.SetMapRadius(defaultMapRadius);
        }

        public void UpdateMapZoom(float sizeDelta)
        {
            var newMapSizeDelta = sizeDelta * (float) lightshipMapView.MapRadius;
            var newMapRadius = Math.Max((float) lightshipMapView.MapRadius - newMapSizeDelta, _minimumMapRadius);
            newMapRadius = Mathf.Clamp(newMapRadius, _minimumMapRadius, _maximumMapRadius);
            lightshipMapView.SetMapRadius(newMapRadius);
            _camera.orthographicSize = newMapRadius;
        }

        public void SmoothZoom(float sizeDelta)
        {
            StartCoroutine(ZoomEnumerator(lightshipMapView.MapRadius, lightshipMapView.MapRadius * sizeDelta));
        }

        public void UpdateMapPan(Vector2 touchDelta)
        {
            Vector3 offset = new Vector3(touchDelta.x, 0, touchDelta.y);

            float zoomOffsetModiferPercentage =
            (
                1 -
                Mathf.InverseLerp
                (
                    _minimumMapRadius,
                    _maximumMapRadius,
                    (float)lightshipMapView.MapRadius
                )
            );

            float panSpeed = 1 - (_mapZoomPanModifier * zoomOffsetModiferPercentage);
            lightshipMapView.OffsetMapCenter(offset * panSpeed);

            MapDidPan?.Invoke();
        }

        public void PanDidEnd()
        {
            MapDidCompletePan?.Invoke();
        }

        public void MapAreaTouched(Vector2 touchPoint)
        {
            MapWasTouched?.Invoke();
        }

        public Vector3 GetScenePosition(MapsLatLng latLng)
        {
            return lightshipMapView.LatLngToScene(latLng);
        }

        public MapsLatLng GetMapCenter()
        {
            return lightshipMapView.MapCenter;
        }

        public void CenterMapOnLocation(MapsLatLng location)
        {
            StartCoroutine(RecenterEnumerator(location));
        }

        private void MapTileWasAdded(IMapTile mapTile, IMapTileObject mapTileObject)
        {
            mapTileRevealThreshold--;
            if (mapTileRevealThreshold == 0)
            {
                MapIsReady?.Invoke();

            }

            mapTileObject.Transform.gameObject.layer = LayerMask.NameToLayer(MapTileLayer);
            foreach (Transform child in mapTileObject.Transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer(MapTileLayer);
            }
        }

        private IEnumerator ZoomEnumerator(double startingZoom, double endingZoom)
        {
            float elapsedTime = 0f;
            while (elapsedTime < _zoomDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / _zoomDuration;
                double newRadius = math.lerp(startingZoom, endingZoom, _recenterAnimationCurve.Evaluate(t));
                lightshipMapView.SetMapRadius(newRadius);
                _camera.orthographicSize = (float)newRadius;
                yield return null;
            }
            lightshipMapView.SetMapRadius(endingZoom);
            _camera.orthographicSize = (float)endingZoom;
        }

        private IEnumerator RecenterEnumerator(MapsLatLng target)
        {
            float elapsedTime = 0f;

            var startingLatitude = lightshipMapView.MapCenter.Latitude;
            var startingLongitude = lightshipMapView.MapCenter.Longitude;
            var startingZoom = lightshipMapView.MapRadius;
            while (elapsedTime < _recenterDuration)
            {
                // Calculate the current progress based on the elapsed time
                float t = elapsedTime / _recenterDuration;

                Double newLat = math.lerp(startingLatitude, target.Latitude, _recenterAnimationCurve.Evaluate(t));
                Double newLng = math.lerp(startingLongitude, target.Longitude, _recenterAnimationCurve.Evaluate(t));
                
                lightshipMapView.SetMapCenter(new MapsLatLng(newLat, newLng));

                elapsedTime += Time.deltaTime;

                yield return null; // Wait for the next frame
            }

            lightshipMapView.SetMapCenter(target);
            MapDidPan?.Invoke();
        }
    }
}
