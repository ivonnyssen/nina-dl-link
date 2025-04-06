using NINA.Core.Utility;
using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Profile;
using NINA.Profile.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IgorVonNyssen.NINA.DlLink.DlLinkDrivers {

    /// <summary>
    /// This Class shows the basic principle on how to add a new Device driver to N.I.N.A. via the plugin interface
    /// When the application scans for equipment the "GetEquipment" method of a device provider is called.
    /// This method should then return the specific List of Devices that you can connect to
    /// </summary>
    [Export(typeof(IEquipmentProvider))]
    [method: ImportingConstructor]
    public class DlLinkProvider(string mockServerAddress = null) : IEquipmentProvider<ISwitchHub> {
        public string Name => "DlLink";

        public IList<ISwitchHub> GetEquipment() {
            string serverAddress = mockServerAddress ?? Properties.Settings.Default.ServerAddress;
            Logger.Debug($"GetEquipment: {serverAddress}");
            var devices = new List<ISwitchHub> {
                new DlLinkDriver($"{serverAddress}")
            };

            return devices;
        }
    }
}