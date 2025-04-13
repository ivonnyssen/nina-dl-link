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

namespace IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems {

    /// <summary>
    /// This sequence item will turn on, off or cycle a DL Link outlet. It can also wait for a specified time and refresh a list of devices after the action is performed.
    /// </summary>
    [ExportMetadata("Name", "DL Link Action")]
    [ExportMetadata("Description", "Turn an outlet on, off, or cycle it. Then optionally wait and refresh a specified list of devices.")]
    [ExportMetadata("Icon", "DL_Link_SVG")]
    [ExportMetadata("Category", "DL Link")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class DlLinkInstruction : SequenceItem {

        [ImportingConstructor]
        public DlLinkInstruction(ICameraMediator cameraMediator,
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

        private DlLinkInstruction(DlLinkInstruction copyMe,
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
        /// The number of the outlet that should be controlled. Outlet numbers start at 1.
        /// </summary>
        private int outletNumber = 1;

        [JsonProperty]
        public int OutletNumber { get => outletNumber; set { outletNumber = value; RaisePropertyChanged(); } }

        /// <summary>
        /// The actions that can should be performed on the outlet: On, Off, Cycle
        /// </summary>
        private OutletActions action;

        [JsonProperty]
        public OutletActions Action { get => action; set { action = value; RaisePropertyChanged(); } }

        /// <summary>
        /// The delay time in seconds that should be waited after the action is performed. This delay is only used if the Rescan property is set to something other than None.
        /// </summary>
        private int delay = 2;

        [JsonProperty]
        public int Delay { get => delay; set { delay = value; RaisePropertyChanged(); } }

        /// <summary>
        /// The rescan action that should be performed after the outlet action is performed. This is only used if the Rescan property is set to something other than None.
        /// </summary>
        private Mediators rescan = Mediators.None;

        [JsonProperty]
        public Mediators Rescan { get => rescan; set { rescan = value; RaisePropertyChanged(); } }

        /// <summary>
        /// Logic to check the outlet state, trigger the action and then trigger a rescan if requested.
        /// </summary>
        /// <param name="progress">The application status progress that can be sent back during execution</param>
        /// <param name="token">When a cancel signal is triggered from outside, this token can be used to register to it or check if it is cancelled</param>
        /// <returns></returns>
        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            if (OutletNumber < 0) {
                Logger.Error($"Outlet number {OutletNumber} is invalid");
                return;
            }

            //get the current state of the outlet
            var handler = new HttpClientHandler() {
                Credentials = new NetworkCredential(UserName, Password)
            };
            var httpClient = this.HttpClient ?? new HttpClient(handler);
            var result = await HttpUtils.GetOutletState(httpClient, ServerAddress, OutletNumber, token);
            if (result.IsErr) {
                Logger.Error($"Failed to get outlet state for {OutletNumber}");
                return;
            }

            //check if the outlet is already in the desired state
            if (result.Value == (Action == OutletActions.On)) {
                Logger.Debug($"Outlet {OutletNumber} is already in the desired state");
                return;
            }

            //if not, trigger the desired outlet action
            result = await HttpUtils.TriggerOutletAction(httpClient, ServerAddress, OutletNumber, Action, token);
            if (result.IsOk) {
                Logger.Debug($"Triggered outlet {OutletNumber} to {Action}");
            } else {
                Logger.Error($"Failed to trigger outlet {OutletNumber} to {Action}");
            }

            // return early if no rescan is requested
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
                    Logger.Error($"Rescan {Rescan} is not supported");
                    break;
            }

            return;
        }

        /// <summary>
        /// When items are put into the sequence via the factory, the factory will call the clone method. Make sure all the relevant fields are cloned with the object.
        /// </summary>
        /// <returns></returns>
        public override object Clone() {
            return new DlLinkInstruction(this, cameraMediator, focuserMediator, filterWheelMediator, telescopeMediator, guiderMediator, rotatorMediator, domeMediator, switchMediator, flatDeviceMediator, weatherDataMediator, safetyMonitorMediator);
        }

        /// <summary>
        /// This string will be used for logging
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(DlLinkInstruction)}, Outlet: {OutletNumber}, Action: {Action}, Delay: {Delay}, Rescan: {Rescan}.";
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