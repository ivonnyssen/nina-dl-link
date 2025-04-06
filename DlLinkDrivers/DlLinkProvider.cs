using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Profile.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace IgorVonNyssen.NINA.DlLink.DlLinkDrivers {

    /// <summary>
    /// This Class shows the basic principle on how to add a new Device driver to N.I.N.A. via the plugin interface
    /// When the application scans for equipment the "GetEquipment" method of a device provider is called.
    /// This method should then return the specific List of Devices that you can connect to
    /// </summary>
    [Export(typeof(IEquipmentProvider))]
    [method: ImportingConstructor]
    public class DlLinkProvider(IProfileService profileService) : IEquipmentProvider<ISwitchHub> {
        private readonly IProfileService profileService = profileService;

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