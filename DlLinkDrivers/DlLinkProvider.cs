using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Profile.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace IgorVonNyssen.NINA.DlLink.DlLinkDrivers {

    /// <summary>
    /// Provides the DlLink driver for NINA.
    /// </summary>
    [Export(typeof(IEquipmentProvider))]
    [method: ImportingConstructor]
    public class DlLinkProvider(IProfileService profileService) : IEquipmentProvider<ISwitchHub> {
        private readonly IProfileService profileService = profileService; //do not remove this - provider does not get found by NINA otherwise

        public string Name => "DL Link";

        public IList<ISwitchHub> GetEquipment() {
            var serverAddress = Properties.Settings.Default.ServerAddress;
            var devices = new List<ISwitchHub> {
                new DlLinkDriver($"{serverAddress}")
            };

            return devices;
        }
    }
}