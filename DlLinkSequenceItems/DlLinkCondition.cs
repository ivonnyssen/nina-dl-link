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
    /// The class checks the state of the specified outlet and returns true if it is in the specified state.
    /// </summary>
    [ExportMetadata("Name", "DL Link Check")]
    [ExportMetadata("Description", "Checks the state of the specified outlet and returns true if it is in the specified state.")]
    [ExportMetadata("Icon", "DL_Link_SVG")]
    [ExportMetadata("Category", "DL Link")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class DlLinkCondition : SequenceCondition {

        [ImportingConstructor]
        public DlLinkCondition() {
            outletNumber = 0;
            state = OutletStates.On;
        }

        /// <summary>
        /// The number of the outlet to check. Numbers start at 1.
        /// </summary>
        private int outletNumber;

        [JsonProperty]
        public int OutletNumber {
            get => outletNumber; set {
                outletNumber = value < 0 ? 0 : value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The state to check for. The outlet can be on or off.
        /// </summary>
        private OutletStates state;

        [JsonProperty]
        public OutletStates State {
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

            switch (State) {
                case OutletStates.On:
                    Logger.Debug($"Checking outlet {OutletNumber} to be on: {result.Value}");
                    return true == result.Value;

                case OutletStates.Off:
                    Logger.Debug($"Checking outlet {OutletNumber} to be off: {result.Value}");
                    return false == result.Value;

                default:
                    Logger.Error($"Invalid state for outlet {OutletNumber}: {State}");
                    throw new SequenceEntityFailedException($"Invalid state for outlet {OutletNumber}: {State}");
            }
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

        #region mock properties

        public HttpClient HttpClient { get; set; } = null;
        public string ServerAddress { get; set; } = Properties.Settings.Default.ServerAddress;
        public string UserName { get; set; } = Properties.Settings.Default.Username;
        public string Password { get; set; } = Properties.Settings.Default.Password;

        #endregion mock properties
    }
}