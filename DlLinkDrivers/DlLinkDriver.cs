using NINA.Core.Utility;
using NINA.Equipment.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IgorVonNyssen.NINA.DlLink.DlLinkDrivers {

    /// <summary>
    /// This class implements the ISwitchHub interface for the DlLink driver.
    /// </summary>
    public class DlLinkDriver(string deviceId, HttpClient mockClient = null, string mockServerAddress = null, string mockUsername = null, string mockPassword = null) : BaseINPC, ISwitchHub {
        private readonly ICollection<ISwitch> switches = [];

        ICollection<ISwitch> ISwitchHub.Switches => switches;

        public bool HasSetupDialog => false;

        public string Id { get; } = "338464d7-9c78-4774-b394-dbdb795b2127";

        public string Name { get; } = $"DlLink {deviceId}";

        public string DisplayName => Name;

        public string Category => "DlLink Driver";

        public bool Connected { get; private set; }

        public string Description => "Digital Logger Outlets";

        public string DriverInfo { get; } = $"Digital Logger IP: {deviceId}";

        public string DriverVersion { get; private set; } = "1.0.0";

        public IList<string> SupportedActions => [];

        public string Action(string actionName, string actionParameters) {
            throw new NotImplementedException();
        }

        public async Task<bool> Connect(CancellationToken token) {
            var handler = new HttpClientHandler() {
                Credentials = new NetworkCredential(userName, password)
            };
            var httpClient = this.httpClient ?? new HttpClient(handler);
            switch (await HttpUtils.GetOutletNames(httpClient, serverAddress, token)) {
                case { IsOk: true, Value: var outletNames }:
                    switches.Clear();
                    var counter = 1; //user sees outlets as 1-indexed
                    foreach (var outletName in outletNames) {
                        switches.Add(new DlOutlet(outletName, counter));
                        counter++;
                        Logger.Debug($"Outlet name: {outletName}");
                    }
                    Connected = true;
                    Logger.Debug($"Outlet names: {string.Join(", ", outletNames)}");
                    break;

                default:
                    Logger.Error($"Failed to connect to {serverAddress}");
                    Connected = false;
                    break;
            }

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

        #region mocked properties

        private readonly HttpClient httpClient = mockClient;
        private readonly string serverAddress = mockServerAddress ?? Properties.Settings.Default.ServerAddress;
        private readonly string userName = mockUsername ?? Properties.Settings.Default.Username;
        private readonly string password = mockPassword ?? Properties.Settings.Default.Password;

        #endregion mocked properties
    }
}