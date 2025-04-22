using IgorVonNyssen.NINA.DlLink.DlLinkDrivers;
using NINA.Astrometry;
using NINA.Astrometry.Interfaces;
using NINA.Core.Utility;
using NINA.Equipment.Equipment.MyTelescope;
using NINA.Equipment.Interfaces;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace IgorVonNyssen.NINA.DlLink.DlLinkDockables {

    /// <summary>
    /// This Class shows the basic principle on how to add a new panel to N.I.N.A. Imaging tab via the plugin interface
    /// In this example an altitude chart is added to the imaging tab that shows the altitude chart based on the position of the telescope
    /// </summary>
    [Export(typeof(IDockableVM))]
    public class DlLinkDockable : DockableVM {
        private readonly ICollection<ISwitch> switches = [];
        public ICollection<ISwitch> Switches => switches;

        [ImportingConstructor]
        public DlLinkDockable(IProfileService profileService) : base(profileService) {
            httpClient = null;
            serverAddress = Properties.Settings.Default.ServerAddress;
            userName = Properties.Settings.Default.Username;
            password = Properties.Settings.Default.Password;

            Logger.Debug($"DlLinkDockable: constructor called. ShowSwitchHub: {Properties.Settings.Default.ShowSwitchHub}");
            // This will reference the resource dictionary to import the SVG graphic and assign it as the icon for the header bar
            var dict = new ResourceDictionary();
            dict.Source = new Uri("IgorVonNyssen.NINA.DlLink;component/DlLinkDockables/DlLinkDockableTemplates.xaml", UriKind.RelativeOrAbsolute);
            ImageGeometry = (System.Windows.Media.GeometryGroup)dict["IgorVonNyssen.NINA.DlLink_LogoSVG"];
            ImageGeometry.Freeze();

            Title = "DL Link";

            // Some asynchronous initialization
            Task.Run(async () => {
                var handler = new HttpClientHandler() {
                    Credentials = new NetworkCredential(userName, password)
                };
                var httpClient = this.httpClient ?? new HttpClient(handler);
                switch (await HttpUtils.GetOutletNames(httpClient, serverAddress, CancellationToken.None)) {
                    case { IsOk: true, Value: var outletNames }:
                        switches.Clear();
                        var counter = 1; //user sees outlets as 1-indexed
                        foreach (var outletName in outletNames) {
                            switches.Add(new DlOutlet(outletName, counter));
                            counter++;
                            Logger.Debug($"Outlet name: {outletName}");
                        }
                        Logger.Debug($"Outlet names: {string.Join(", ", outletNames)}");
                        break;

                    default:
                        Logger.Error($"Failed to get outlet names from {serverAddress}");
                        break;
                }
            });
        }

        public void Dispose() {
            // On shutdown cleanup
            switches.Clear();
        }

        public void UpdateDeviceInfo(TelescopeInfo deviceInfo) {
            // The IsVisible flag indicates if the dock window is active or hidden
            if (IsVisible) {
            }
        }

        #region mocked properties

        private readonly HttpClient httpClient;
        private readonly string serverAddress;
        private readonly string userName;
        private readonly string password;

        #endregion mocked properties
    }
}