using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility;
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
    ///     - IProfileService,
    ///     - ICameraMediator,
    ///     - ITelescopeMediator,
    ///     - IFocuserMediator,
    ///     - IFilterWheelMediator,
    ///     - IGuiderMediator,
    ///     - IRotatorMediator,
    ///     - IFlatDeviceMediator,
    ///     - IWeatherDataMediator,
    ///     - IImagingMediator,
    ///     - IApplicationStatusMediator,
    ///     - INighttimeCalculator,
    ///     - IPlanetariumFactory,
    ///     - IImageHistoryVM,
    ///     - IDeepSkyObjectSearchVM,
    ///     - IDomeMediator,
    ///     - IImageSaveMediator,
    ///     - ISwitchMediator,
    ///     - ISafetyMonitorMediator,
    ///     - IApplicationMediator
    ///     - IApplicationResourceDictionary
    ///     - IFramingAssistantVM
    ///     - IList<IDateTimeProvider>
    /// </remarks>
    [ExportMetadata("Name", "DL Link Action")]
    [ExportMetadata("Description", "Turn an outlet on, off, or cycle it.")]
    [ExportMetadata("Icon", "DL_Link_SVG")]
    [ExportMetadata("Category", "DL Link")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class DlLinkInstruction : SequenceItem {

        [ImportingConstructor]
        public DlLinkInstruction() {
        }

        public DlLinkInstruction(DlLinkInstruction copyMe) : this() {
            CopyMetaData(copyMe);
        }

        /// <summary>
        /// An example property that can be set from the user interface via the Datatemplate specified in PluginTestItem.Template.xaml
        /// </summary>
        /// <remarks>
        /// If the property changes from the code itself, remember to call RaisePropertyChanged() on it for the User Interface to notice the change
        /// </remarks>
        [JsonProperty]
        public int OutletNumber { get; set; }

        /// <summary>
        /// An example property that can be set from the user interface via the Datatemplate specified in PluginTestItem.Template.xaml
        /// </summary>
        /// <remarks>
        /// If the property changes from the code itself, remember to call RaisePropertyChanged() on it for the User Interface to notice the change
        /// </remarks>
        [JsonProperty]
        public OutletActions Action { get; set; }

        /// <summary>
        /// The core logic when the sequence item is running resides here
        /// Add whatever action is necessary
        /// </summary>
        /// <param name="progress">The application status progress that can be sent back during execution</param>
        /// <param name="token">When a cancel signal is triggered from outside, this token can be used to register to it or check if it is cancelled</param>
        /// <returns></returns>
        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
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
            return;
        }

        /// <summary>
        /// When items are put into the sequence via the factory, the factory will call the clone method. Make sure all the relevant fields are cloned with the object.
        /// </summary>
        /// <returns></returns>
        public override object Clone() {
            return new DlLinkInstruction(this);
        }

        /// <summary>
        /// This string will be used for logging
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(DlLinkInstruction)}, Text: {OutletNumber}";
        }

        public HttpClient HttpClient { get; set; } = null;
        public string ServerAddress { get; set; } = Properties.Settings.Default.ServerAddress;
        public string UserName { get; set; } = Properties.Settings.Default.Username;
        public string Password { get; set; } = Properties.Settings.Default.Password;
    }
}