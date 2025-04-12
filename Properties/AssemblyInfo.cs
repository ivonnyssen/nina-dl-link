using System.Reflection;
using System.Runtime.InteropServices;

// [MANDATORY] The following GUID is used as a unique identifier of the plugin. Generate a fresh one for your plugin!
[assembly: Guid("f60fcde8-a9ed-4572-9454-66ce03a43285")]

// [MANDATORY] The assembly versioning
//Should be incremented for each new release build of a plugin
[assembly: AssemblyVersion("1.0.0.3")]
[assembly: AssemblyFileVersion("1.0.0.3")]

// [MANDATORY] The name of your plugin
[assembly: AssemblyTitle("DL Link")]
// [MANDATORY] A short description of your plugin
[assembly: AssemblyDescription("Control Digital Logger outlets.")]

// The following attributes are not required for the plugin per se, but are required by the official manifest meta data

// Your name
[assembly: AssemblyCompany("Igor von Nyssen")]
// The product name that this plugin is part of
[assembly: AssemblyProduct("DL Link")]
[assembly: AssemblyCopyright("Copyright © 2025 Igor von Nyssen")]

// The minimum Version of N.I.N.A. that this plugin is compatible with
[assembly: AssemblyMetadata("MinimumApplicationVersion", "3.0.0.2017")]

// The license your plugin code is using
[assembly: AssemblyMetadata("License", "MIT")]
// The url to the license
[assembly: AssemblyMetadata("LicenseURL", "https://opensource.org/license/mit")]
// The repository where your pluggin is hosted
[assembly: AssemblyMetadata("Repository", "https://github.com/ivonnyssen/nina-dl-link")]

// The following attributes are optional for the official manifest meta data

//[Optional] Your plugin homepage URL - omit if not applicaple
[assembly: AssemblyMetadata("Homepage", "https://ivonnyssen.github.io/nina-dl-link/")]

//[Optional] Common tags that quickly describe your plugin
[assembly: AssemblyMetadata("Tags", "")]

//[Optional] A link that will show a log of all changes in between your plugin's versions
[assembly: AssemblyMetadata("ChangelogURL", "https://github.com/ivonnyssen/nina-dl-link/blob/main/CHANGELOG.md")]

//[Optional] The url to a featured logo that will be displayed in the plugin list next to the name
[assembly: AssemblyMetadata("FeaturedImageURL", "https://ivonnyssen.github.io/nina-dl-link/assets/dl-logo.png")]
//[Optional] A url to an example screenshot of your plugin in action
[assembly: AssemblyMetadata("ScreenshotURL", "https://ivonnyssen.github.io/nina-dl-link/assets/DL-Link-Options.png")]
//[Optional] An additional url to an example example screenshot of your plugin in action
[assembly: AssemblyMetadata("AltScreenshotURL", "https://ivonnyssen.github.io/nina-dl-link/assets/DL-Link-Sequence.png")]
//[Optional] An in-depth description of your plugin
[assembly: AssemblyMetadata("LongDescription", @"This plugin allows the control of Power Switches by Digital Logger Inc. It also provides fucntionality to refresh the list of available devices in NINA after a switch action. Use this to power on a switch like the Pegasus Astro power boxes, then refresh and connect to that switch. Now you can turn on the power to your camera, the USB port for the guide camera etc. Then use the refresh actions to find all the new devices and you can use the Connector plugin (separate plugin) to connnect to the newfound hardware. Please not that the password used for DL switches is transmitted in the clear, so we are also not encrypting it in the plugin itself.
Please note that the Seqence items work regardless whether you use the switch driver or have another switch connected.")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
// [Unused]
[assembly: AssemblyConfiguration("")]
// [Unused]
[assembly: AssemblyTrademark("")]
// [Unused]
[assembly: AssemblyCulture("")]