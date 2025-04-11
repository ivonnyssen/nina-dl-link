using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Sequencer.SequenceItem;
using System;
using System.ComponentModel.Composition;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems {

    /// <summary>
    /// Refreshes the list of devices for a given category. Use it to find a device after you turned on a switch.
    /// </summary>
    [ExportMetadata("Name", "DL Link Refresh")]
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
        /// The time in seconds to wait before the rescan is triggered
        /// </summary>
        private int delay = 0;

        [JsonProperty]
        public int Delay { get => delay; set { delay = value < 0 ? 0 : value; RaisePropertyChanged(); } }

        /// <summary>
        /// The type of device to rescan. The default is None, which means no rescan will be triggered.
        /// </summary>
        private Mediators rescan = Mediators.None;

        [JsonProperty]
        public Mediators Rescan { get => rescan; set { rescan = value; RaisePropertyChanged(); } }

        /// <summary>
        /// waits for delay seconds if rescan is set to a value other than None
        /// </summary>
        /// <param name="progress">The application status progress that can be sent back during execution</param>
        /// <param name="token">When a cancel signal is triggered from outside, this token can be used to register to it or check if it is cancelled</param>
        /// <returns></returns>
        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            //check if the rescan is set to None, if so, we do not need to wait
            if (Rescan == Mediators.None) {
                return;
            }
            //wait for the delay time
            await Task.Delay(Math.Abs(Delay) * 1000, token);
            //trigger a Rescan if requested
            switch (Rescan) {
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

                default:
                    Logger.Error($"Rescan for {Rescan} not implemented");
                    break;
            }

            return;
        }

        /// <summary>
        /// When items are put into the sequence via the factory, the factory will call the clone method. Make sure all the relevant fields are cloned with the object.
        /// </summary>
        /// <returns></returns>
        public override object Clone() {
            return new DlLinkRescan(this,
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
                safetyMonitorMediator) {
                Delay = delay,
                Rescan = rescan
            };
        }

        /// <summary>
        /// This string will be used for logging
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(DlLinkRescan)}, Delay: {Delay}, Equipment: {Rescan}";
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

        #region mock properties

        public HttpClient HttpClient { get; set; } = null;
        public string ServerAddress { get; set; } = Properties.Settings.Default.ServerAddress;
        public string UserName { get; set; } = Properties.Settings.Default.Username;
        public string Password { get; set; } = Properties.Settings.Default.Password;

        #endregion mock properties
    }
}