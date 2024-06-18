// Copyright 2022-2024 Niantic.
using System.Collections.Generic;
using Niantic.Lightship.AR.VpsCoverage;

namespace Niantic.Lightship.AR.Samples
{
    public class GPSSmoother
    {
        private readonly int _sampleSize;
        private readonly Queue<double> _latSamples = new Queue<double>();
        private readonly Queue<double> _lngSamples = new Queue<double>();
        private double _latSum = 0.0;
        private double _lngSum = 0.0;

        public GPSSmoother(int sampleSize = 50)
        {
            _sampleSize = sampleSize;
        }

        public LatLng AddSample(double lat, double lng)
        {
            // Add new sample to the running sum and queue
            _latSum += lat;
            _lngSum += lng;
            _latSamples.Enqueue(lat);
            _lngSamples.Enqueue(lng);

            // If we've exceeded our sample size, dequeue the oldest sample and subtract from the running sum
            if (_latSamples.Count > _sampleSize)
            {
                _latSum -= _latSamples.Dequeue();
                _lngSum -= _lngSamples.Dequeue();
            }

            // Calculate the moving average
            int count = _latSamples.Count;
            double avgLat = _latSum / count;
            double avgLng = _lngSum / count;

            return new LatLng(avgLat, avgLng);
        }
    }
}