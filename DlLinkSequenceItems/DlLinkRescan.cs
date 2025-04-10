using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Sequencer.SequenceItem;
using System;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems {

    /// <summary>
    /// This Class shows the basic principle on how to add a new Sequence Instruction to the N.I.N.A. sequencer via the plugin interface
    /// For ease of use this class inherits the abstract SequenceItem which already handles most of the running logic, like logging, exception handling etc.
    /// A complete custom implementation by just implementing ISequenceItem is possible too
    /// The following MetaData can be set to drive the initial values
    /// --> Name - The name that will be displayed for the item
    /// --> Description - a brief summary of what the item is doing. It will be displayed as a tooltip on mouseover in the application
    /// --> Icon - a string to the key value of a Geometry inside N.I.N.A.'s geometry resources
    ///
    /// If the item has some preconditions that should be validated, it shall also extend the IValidatable interface and add the validation logic accordingly.
    /// </summary>
    /// <remarks>
    /// The constructor marked with [ImportingConstructor] will be used to import and construct the object
    /// General device interfaces can be added to the constructor parameters and will be automatically injected on instantiation by the plugin loader
    /// </remarks>
    /// <remarks>
    /// Available interfaces to be injected:
    /// </remarks>
    [ExportMetadata("Name", "DL Link Rescan")]
    [ExportMetadata("Description", "Refreshes the list of devices for a given category. Use it to find a device after you turned on a switch.")]
    [ExportMetadata("Icon", "DL_Link_SVG")]
    [ExportMetadata("Category", "DL Link")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class DlLinkRescan : SequenceItem {

        [ImportingConstructor]
        public DlLinkRescan(ICameraMediator cameraMediator,
            IFocuserMediator focuserMediator,
            IFilterWheelMediator filterWheelMediator,
            ITelescopeMediator telescopeMediator,
            IGuiderMediator guiderMediator,
            IRotatorMediator rotatorMediator,
            IDomeMediator domeMediator,
            ISwitchMediator switchMediator,
            IFlatDeviceMediator flatDeviceMediator,
            IWeatherDataMediator weatherDataMediator,
            ISafetyMonitorMediator safetyMonitorMediator) :
            this(
                null,
            cameraMediator,
            focuserMediator,
            filterWheelMediator,
            telescopeMediator,
            guiderMediator,
            rotatorMediator,
            domeMediator,
            switchMediator,
            flatDeviceMediator,
            weatherDataMediator,
            safetyMonitorMediator) { }

        private DlLinkRescan(DlLinkRescan copyMe,
            ICameraMediator cameraMediator,
            IFocuserMediator focuserMediator,
            IFilterWheelMediator filterWheelMediator,
            ITelescopeMediator telescopeMediator,
            IGuiderMediator guiderMediator,
            IRotatorMediator rotatorMediator,
            IDomeMediator domeMediator,
            ISwitchMediator switchMediator,
            IFlatDeviceMediator flatDeviceMediator,
            IWeatherDataMediator weatherDataMediator,
            ISafetyMonitorMediator safetyMonitorMediator) {
            this.cameraMediator = cameraMediator;
            this.focuserMediator = focuserMediator;
            this.filterWheelMediator = filterWheelMediator;
            this.telescopeMediator = telescopeMediator;
            this.guiderMediator = guiderMediator;
            this.rotatorMediator = rotatorMediator;
            this.domeMediator = domeMediator;
            this.switchMediator = switchMediator;
            this.flatDeviceMediator = flatDeviceMediator;
            this.weatherDataMediator = weatherDataMediator;
            this.safetyMonitorMediator = safetyMonitorMediator;
            if (copyMe != null) { CopyMetaData(copyMe); }
        }

        /// <summary>
        /// An example property that can be set from the user interface via the Datatemplate specified in PluginTestItem.Template.xaml
        /// </summary>
        /// <remarks>
        /// If the property changes from the code itself, remember to call RaisePropertyChanged() on it for the User Interface to notice the change
        /// </remarks>
        [JsonProperty]
        public int Delay { get; set; } = 0;

        /// <summary>
        /// An example property that can be set from the user interface via the Datatemplate specified in PluginTestItem.Template.xaml
        /// </summary>
        /// <remarks>
        /// If the property changes from the code itself, remember to call RaisePropertyChanged() on it for the User Interface to notice the change
        /// </remarks>
        [JsonProperty]
        public Mediators Rescan { get; set; } = Mediators.None;

        /// <summary>
        /// The core logic when the sequence item is running resides here
        /// Add whatever action is necessary
        /// </summary>
        /// <param name="progress">The application status progress that can be sent back during execution</param>
        /// <param name="token">When a cancel signal is triggered from outside, this token can be used to register to it or check if it is cancelled</param>
        /// <returns></returns>
        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            //wait for the delay time
            await Task.Delay(Math.Abs(Delay) * 1000, token);
            //trigger a Rescan if requested
            switch (Rescan) {
                case Mediators.None:
                    break;

                case Mediators.Camera:
                    await cameraMediator.Rescan();
                    break;

                case Mediators.Dome:
                    await domeMediator.Rescan();
                    break;

                case Mediators.Switch:
                    await switchMediator.Rescan();
                    break;

                case Mediators.WeatherData:
                    await weatherDataMediator.Rescan();
                    break;

                case Mediators.SafetyMonitor:
                    await safetyMonitorMediator.Rescan();
                    break;

                case Mediators.Telescope:
                    await telescopeMediator.Rescan();
                    break;

                case Mediators.Focuser:
                    await focuserMediator.Rescan();
                    break;

                case Mediators.FilterWheel:
                    await filterWheelMediator.Rescan();
                    break;

                case Mediators.Guider:
                    await guiderMediator.Rescan();
                    break;

                case Mediators.Rotator:
                    await rotatorMediator.Rescan();
                    break;

                case Mediators.FlatDevice:
                    await flatDeviceMediator.Rescan();
                    break;
            }

            return;
        }

        /// <summary>
        /// When items are put into the sequence via the factory, the factory will call the clone method. Make sure all the relevant fields are cloned with the object.
        /// </summary>
        /// <returns></returns>
        public override object Clone() {
            return new DlLinkRescan(this, cameraMediator, focuserMediator, filterWheelMediator, telescopeMediator, guiderMediator, rotatorMediator, domeMediator, switchMediator, flatDeviceMediator, weatherDataMediator, safetyMonitorMediator);
        }

        /// <summary>
        /// This string will be used for logging
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(DlLinkRescan)}, Device class: {Rescan}";
        }

        private readonly ICameraMediator cameraMediator;
        private readonly IFocuserMediator focuserMediator;
        private readonly IFilterWheelMediator filterWheelMediator;
        private readonly ITelescopeMediator telescopeMediator;
        private readonly IGuiderMediator guiderMediator;
        private readonly IRotatorMediator rotatorMediator;
        private readonly IDomeMediator domeMediator;
        private readonly ISwitchMediator switchMediator;
        private readonly IFlatDeviceMediator flatDeviceMediator;
        private readonly IWeatherDataMediator weatherDataMediator;
        private readonly ISafetyMonitorMediator safetyMonitorMediator;

        public HttpClient HttpClient { get; set; } = null;
        public string ServerAddress { get; set; } = Properties.Settings.Default.ServerAddress;
        public string UserName { get; set; } = Properties.Settings.Default.Username;
        public string Password { get; set; } = Properties.Settings.Default.Password;
    }
}