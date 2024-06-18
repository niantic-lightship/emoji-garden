// Copyright 2022-2024 Niantic.
using Niantic.Lightship.AR.LocationAR;

namespace Niantic.Lightship.AR.Samples
{
    /// <summary>
    /// Interface for the core app controlling component. Implement this interface in a
    /// MonoBehaviour and add it to the LocationBasedAppController.
    /// </summary>
    public interface ILocationBasedExperience
    {
        public struct LocationArgs
        {
            public ARLocation Location;
            public string ReadableLocationName;
        }

        /// <summary>
        /// Called after the user selects a location on the map and successfully localizes.
        /// <param name="data">Contains the data related to the relevant location. Most importantly a reference to the ARLocation.</param>
        /// <param name="controller">Controller for this application. Contains helpful methods such as ReturnToMap.</param>
        /// </summary>
        void StartExperience(LocationArgs data, LocationBasedExperienceController controller);

        /// <summary>
        /// Called when the stability of the tracking system degrades. The app frame work is going
        /// to display UI to guide the users to re-localize. You should hide / pause any gameplay
        /// content when this is called.
        /// </summary>
        void PauseDueToLocalizationLost();

        /// <summary>
        /// Called after the user re-localizes. You can safely resume / show gameplay content.
        /// </summary>
        void UnpauseDueToLocalizationRegained();
    }
}
