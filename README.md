## Name
Emoji Garden

## Description
Emoji Garden is a completed sample project that guides users to a Public Location so they can localize and engage in Shared AR play. (Public Locations are publicly accessible real-world locations that are unique or notable in some way and are VPS-Activated for apps to engage with them). The sample comes with Location AR UX, which uses the [Maps SDK](https://lightship.dev/docs/maps) and [Coverage API](https://lightship.dev/docs/ardk/apiref/Niantic/Lightship/AR/VpsCoverage/) to help users understand where they are in relation to Public Locations. Location AR UX also helps users connect to Public Locations and gives them guidance if localization isn’t working as expected. For more detailed information on this project, please see the [Emoji Garden project documentation](https://lightship.dev/docs/ardk/emoji_garden/) on the Lightship documentation site. 

## Deploying and Running Emoji Garden
To deploy Emoji Garden on your device:
1. Clone the project.
2. Navigate to the [Lightship dev site](https://www.lightship.dev/) and copy your API key.
3. Set your Lightship API key in two places:
4. In the *Lightship* top menu, select *Settings*, then enter your API key.
5. In the *Project* window under *Assets*, find the *Localization UX* directory. From there, navigate to *Resources* *>* *Maps* *>* *SDK* *>* *MapsAuthConfig* *>* *LightshipApiKey* and enter your API key again.
6. In the *File* top menu, select *Build Settings*, then choose one of the supported platforms, *iOS* or *Android*.
7. Click *Build and Run* to run Emoji Garden on your device. The map will display, allowing you to localize to a nearby Public Location.

## Troubleshooting and Common Issues
- My API key is missing!
	- Make sure to set your API key in both locations! See step 3 of [Deploying and Running Emoji Garden](https://lightship.dev/docs/ardk/emoji_garden/#deploying-and-running-emoji-garden).
- No locations are showing up after I run the app!
	- You might not be in range of any Public Locations. If there are none near you, use a [Test Scan](https://lightship.dev/docs/ardk/emoji_garden/#test-scans)!
- My public VPS scan isn't localizing!
	- Make sure the lighting and weather conditions are good. VPS works best in daytime under clear skies.
	- If conditions are good and you still can't localize, try another Public Location.
- My Test Scan location is in the wrong place / not showing up!
	- Double-check the location manifest's coordinate values in the Unity Inspector to make sure they match what is on the Geospatial Browser.
	- Remove and re-set the Test Scan location in the `CoverageClient` component.

## Support
For any other issues, [contact us](https://lightship.dev/docs/ardk/contact_us/) on Discord or the Lightship forums! Before reaching out, open the Console Log by holding three touches on your device's screen for three seconds, then take a screenshot and post it along with a description of your issue.
