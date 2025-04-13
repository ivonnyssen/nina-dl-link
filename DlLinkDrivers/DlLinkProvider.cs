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
        private readonly IProfileService profileService = profileService;

        public string Name => "DL Link";

        public IList<ISwitchHub> GetEquipment() {
            return Properties.Settings.Default.HideSwitchhub
                ? []
                : (IList<ISwitchHub>)[new DlLinkDriver($"{Properties.Settings.Default.ServerAddress}")];
        }
    }
}