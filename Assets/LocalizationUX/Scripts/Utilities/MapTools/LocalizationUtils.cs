// Copyright 2022-2024 Niantic.
using Niantic.Lightship.AR.VpsCoverage;

namespace Niantic.Lightship.AR.Samples
{
    public static class LocalizationUtils
    {
        private static Niantic.Lightship.Maps.Core.Coordinates.LatLng[] ToMapsLatLng(LatLng[] ardkLatLng)
        {
            var results = new Niantic.Lightship.Maps.Core.Coordinates.LatLng[ardkLatLng.Length];
            for (var i = 0; i < results.Length; i++)
            {
                results[i] = ToMapsLatLng(ardkLatLng[i]);
            }

            return results;
        }

        public static Niantic.Lightship.Maps.Core.Coordinates.LatLng ToMapsLatLng(LatLng ardkLatLng)
        {
            return new Niantic.Lightship.Maps.Core.Coordinates.LatLng(ardkLatLng.Latitude, ardkLatLng.Longitude);
        }

        public static LatLng ToARDKLatLng(Niantic.Lightship.Maps.Core.Coordinates.LatLng mapsLatLng)
        {
            return new (mapsLatLng.Latitude, mapsLatLng.Longitude);
        }

        /*
         * Calculates what percentage of a range X1 - X2 value X is
         * Remaps X to range Y1 to Y2.
         *
         * example : 0.5 is 50% of range 0 to 1
         * returns value 20 for new range of 10 to 30
         */
        public static float RemapRange(float x, float x1, float x2, float y1, float y2)
        {
            var m = (y2 - y1) / (x2 - x1);
            var c = y1 - m * x1;

            return m * x + c;
        }
    }
}