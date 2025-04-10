using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.SequenceItem;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems {

    /// <summary>
    /// This Class shows the basic principle on how to add a new Sequence Trigger to the N.I.N.A. sequencer via the plugin interface
    /// For ease of use this class inherits the abstract SequenceTrigger which already handles most of the running logic, like logging, exception handling etc.
    /// A complete custom implementation by just implementing ISequenceTrigger is possible too
    /// The following MetaData can be set to drive the initial values
    /// --> Name - The name that will be displayed for the item
    /// --> Description - a brief summary of what the item is doing. It will be displayed as a tooltip on mouseover in the application
    /// --> Icon - a string to the key value of a Geometry inside N.I.N.A.'s geometry resources
    ///
    /// If the item has some preconditions that should be validated, it shall also extend the IValidatable interface and add the validation logic accordingly.
    /// </summary>
    [ExportMetadata("Name", "DL Link Check")]
    [ExportMetadata("Description", "Checks the state of the specified outlet and returns true if it is in the specified state.")]
    [ExportMetadata("Icon", "DL_Link_SVG")]
    [ExportMetadata("Category", "DL Link")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class DlLinkCondition : SequenceCondition {

        /// <summary>
        /// The constructor marked with [ImportingConstructor] will be used to import and construct the object
        /// General device interfaces can be added to the constructor parameters and will be automatically injected on instantiation by the plugin loader
        /// </summary>
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
        [ImportingConstructor]
        public DlLinkCondition() {
            outletNumber = 0;
            state = OutletActions.On;
        }

        /// <summary>
        /// An example property that can be set from the user interface via the Datatemplate specified in PluginTestItem.Template.xaml
        /// </summary>
        /// <remarks>
        /// If the property changes from the code itself, remember to call RaisePropertyChanged() on it for the User Interface to notice the change
        /// </remarks>
        private int outletNumber;

        [JsonProperty]
        public int OutletNumber {
            get => outletNumber; set {
                outletNumber = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// An example property that can be set from the user interface via the Datatemplate specified in PluginTestItem.Template.xaml
        /// </summary>
        /// <remarks>
        /// If the property changes from the code itself, remember to call RaisePropertyChanged() on it for the User Interface to notice the change
        /// </remarks>
        private OutletActions state;

        [JsonProperty]
        public OutletActions State {
            get => state; set {
                state = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Once this check returns false, the condition will cause its parent instruction set to skip the rest and proceed with the next set
        /// </summary>
        /// <param name="previousItem"></param>
        /// <param name="nextItem"></param>
        /// <returns></returns>
        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) {
            //get the current state of the outlet
            var handler = new HttpClientHandler() {
                Credentials = new NetworkCredential(UserName, Password)
            };
            var httpClient = this.HttpClient ?? new HttpClient(handler);
            var result = HttpUtils.GetOutletState(httpClient, ServerAddress, OutletNumber, default).Result;
            if (result.IsErr) {
                Logger.Error($"Failed to get outlet state for {OutletNumber}");
                throw new SequenceEntityFailedException($"Failed to get outlet state for {OutletNumber}");
            }

            if (State == OutletActions.On) return true == result.Value; else if (State == OutletActions.Off) return false == result.Value;
            //should never be reached as cycle is not a state an outlet can have
            return false;
        }

        public override object Clone() {
            return new DlLinkCondition() {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description
            };
        }

        /// <summary>
        /// This string will be used for logging
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(DlLinkCondition)}, Outlet: {OutletNumber}, State: {State}";
        }

        public HttpClient HttpClient { get; set; } = null;
        public string ServerAddress { get; set; } = Properties.Settings.Default.ServerAddress;
        public string UserName { get; set; } = Properties.Settings.Default.Username;
        public string Password { get; set; } = Properties.Settings.Default.Password;
    }
}