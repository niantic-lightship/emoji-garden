// Copyright 2022-2024 Niantic.
using System;
using System.Collections.Generic;
using Niantic.Lightship.Maps.Core.Utilities;

namespace Niantic.Lightship.AR.Samples
{
    public class CompassLowpassFilter
    {
        private readonly Queue<double> _samples;
        private readonly int _sampleSize;

        private double _sinSum;
        private double _cosSum;

        /// <summary>
        /// The filtered heading value, in degrees.
        /// </summary>
        /// <remarks>This value is wrapped to [0, 360)</remarks>
        public double Degrees { get; private set; }

        /// <summary>
        /// The filtered heading value, in radians.
        /// </summary>
        public double Radians { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sampleSize">An optional value
        /// that specifies the number of samples in our
        /// moving average's sampling window.</param>
        public CompassLowpassFilter(int sampleSize = 50)
        {
            _sampleSize = sampleSize;
            _samples = new Queue<double>(sampleSize);
        }

        /// <summary>
        /// Add a value, in radians, to the sampling window.
        /// </summary>
        /// <param name="value">The sample value</param>
        public void AddSampleRadians(double value)
        {
            // Note:  Heading values, when represented as either radians
            // or degrees, have a discontinuity around zero.  Because of
            // this discontinuity, a naïve implementation using a simple
            // moving average of heading values doesn't always produce
            // the correct result.  For example, in a simple case where
            // the first two samples in such an implementation are 1° and
            // 359°, the filtered heading would be 180°, but the correct
            // value should have been 0°. The solution implemented here
            // is to separate each value into sine and cosine components
            // (which are continuous), compute moving averages of those
            // components, then use the averages to reconstruct a final
            // value by calculating their inverse tangent.

            _sinSum += Math.Sin(value);
            _cosSum += Math.Cos(value);
            _samples.Enqueue(value);

            if (_samples.Count > _sampleSize)
            {
                // If the queue is full, remove the
                // oldest sample and subtract its sine
                // and cosine from the running total.

                var oldest = _samples.Dequeue();
                _sinSum -= Math.Sin(oldest);
                _cosSum -= Math.Cos(oldest);
            }

            // Calculate the moving average sine
            // and cosine from our sample window.

            var sampleCount = _samples.Count;
            var y = _sinSum / sampleCount;
            var x = _cosSum / sampleCount;

            // Use the average sine and cosine to reconstruct
            // an average angle (in both degrees and radians).

            Radians = Math.Atan2(y, x);
            var degrees = MathEx.RadToDeg(Radians);
            Degrees = MathEx.WrapExclusive(degrees, 0d, 360d);
        }

        /// <summary>
        /// Add a value, in degrees, to the sampling window.
        /// </summary>
        /// <param name="value">The sample value</param>
        public void AddSampleDegrees(double value)
        {
            var radians = MathEx.DegToRad(value);
            AddSampleRadians(radians);
        }
    }
}