// Copyright 2022-2024 Niantic.
using System.Collections;
using UnityEngine;

namespace Niantic.Lightship.AR.Samples {
    public static class NetworkCheck 
    {
        private static readonly string pingAddress = "8.8.8.8";
        private static readonly int pingThreshold = 500; // threshold for max ping time in milliseconds
        private static readonly float pingTimeout = 2f; // timeout until getting Ping done, in seconds

        /*
         * Coroutine that checks for internet connection health
         */
        private static IEnumerator CheckNetworkConnectionInternal(System.Action<bool> onComplete)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) // if there is no internet connection at all, return false 
            {
                onComplete(false);
                yield break;
            }
            
            //we have a network connection, but let's check the health of the connection (using Ping time as a proxy)
            Ping ping = new Ping(pingAddress); 

            // Give the ping some time to complete
            float startTime = Time.time;
            while (!ping.isDone && Time.time - startTime < pingTimeout)
            {
                yield return null;
            }
            
            // If ping is successful and below the threshold, return true. Otherwise, return false.
            onComplete(ping.isDone && ping.time < pingThreshold);
        }

        public static void CheckNetworkConnection(MonoBehaviour caller, System.Action<bool> onComplete)
        {
            caller.StartCoroutine(CheckNetworkConnectionInternal(onComplete));
        }
    }
}