using NINA.Core.Utility;
using NINA.Equipment.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using IgorVonNyssen.NINA.DlLink.Properties;
using NINA.Profile.Interfaces;
using NINA.Profile;

namespace IgorVonNyssen.NINA.DlLink.DlLinkDrivers {

    /// <summary>
    /// This Class shows the basic principle on how to add a new Device driver to N.I.N.A. via the plugin interface
    /// The DeviceProvider will return an instance of this class as a sample weather device
    /// For this example the weather data will generate random numbers
    /// </summary>
    public class DlLinkDriver(string deviceId, IPluginOptionsAccessor pluginSettings) : BaseINPC, ISwitchHub {
        private readonly ICollection<ISwitch> switches = [];

        ICollection<ISwitch> ISwitchHub.Switches => switches;

        public bool HasSetupDialog => false;

        public string Id { get; } = "338464d7-9c78-4774-b394-dbdb795b2127";

        public string Name { get; } = $"DlLink {deviceId}";

        public string DisplayName => Name;

        public string Category => "DlLink Driver";

        public bool Connected { get; private set; }

        public string Description => "Digital Logger Outlets";

        public string DriverInfo { get; } = $"Serial {deviceId}";

        public string DriverVersion { get; private set; }

        public IList<string> SupportedActions => [];

        private readonly IPluginOptionsAccessor pluginSettings = pluginSettings;

        public string Action(string actionName, string actionParameters) {
            throw new NotImplementedException();
        }

        public async Task<bool> Connect(CancellationToken token) {
            var userName = pluginSettings.GetValueString(nameof(DlLink.DLUserName), string.Empty);
            var password = pluginSettings.GetValueString(nameof(DlLink.DLPassword), string.Empty);
            var serverAddress = pluginSettings.GetValueString(nameof(DlLink.DLServerAddress), string.Empty);

            var handler = new HttpClientHandler() {
                Credentials = new NetworkCredential(userName, password)
            };
            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.GetAsync($"http://{serverAddress}/restapi/relay/outlets/all;/name/", token);
            var responseBody = await response.Content.ReadAsStringAsync(token);
            if (response.StatusCode != HttpStatusCode.MultiStatus) {
                Logger.Error($"Response: {response.StatusCode}");
                Logger.Error($"Response: {responseBody}");
                Logger.Error($"Failed to connect to {serverAddress}");
                return false;
            }

            List<string> outletNames = JsonSerializer.Deserialize<List<string>>(responseBody);
            if (outletNames == null) {
                Logger.Error($"Failed to parse outlet names from {serverAddress}");
                return false;
            }
            Logger.Debug($"Outlet names: {string.Join(", ", outletNames)}");

            switches.Clear();
            var counter = 0;
            foreach (var outletName in outletNames) {
                switches.Add(new DlOutlet(outletName, counter, pluginSettings));
                counter++;
                Logger.Debug($"Outlet name: {outletName}");
            }
            Connected = true;

            return Connected;
        }

        public void Disconnect() {
            switches.Clear();
        }

        public void SendCommandBlind(string command, bool raw = true) {
            throw new NotImplementedException();
        }

        public bool SendCommandBool(string command, bool raw = true) {
            throw new NotImplementedException();
        }

        public string SendCommandString(string command, bool raw = true) {
            throw new NotImplementedException();
        }

        public void SetupDialog() {
            throw new NotImplementedException();
        }
    }
}