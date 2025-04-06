using Newtonsoft.Json.Linq;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces;
using NINA.Profile.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;

namespace IgorVonNyssen.NINA.DlLink.DlLinkDrivers {

    public class DlOutlet(string name, int outletNumber, HttpClient mockClient = null, string mockServerAddress = null, string mockUsername = null, string mockPassword = null) : BaseINPC, IWritableSwitch {

        public async Task<bool> Poll() {
            var success = await Task.Run((async () => {
                var handler = new HttpClientHandler() {
                    Credentials = new NetworkCredential(userName, password)
                };
                var httpClient = this.httpClient ?? new HttpClient(handler);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response;
                string responseBody;
                try {
                    response = await httpClient.GetAsync($"http://{serverAddress}/restapi/relay/outlets/{OutletNumber}/state/");
                    responseBody = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode != HttpStatusCode.OK) {
                        Logger.Error($"Response: {response.StatusCode}");
                        Logger.Error($"Response: {responseBody}");
                        return false;
                    }
                } catch (Exception ex) {
                    Logger.Error($"Failed to read status of outlet {OutletNumber}: {ex.Message}");
                    return false;
                }

                bool result;
                try {
                    result = JsonSerializer.Deserialize<bool>(responseBody);
                } catch (JsonException ex) {
                    Logger.Error($"Failed to parse outlet state response for outlet {OutletNumber}: {ex.Message}");
                    return false;
                }
                Value = result ? 1d : 0d;
                return true;
            }));
            if (success) RaisePropertyChanged(nameof(Value));

            // Implement the Poll method
            return success;
        }

        async Task IWritableSwitch.SetValue() {
            var handler = new HttpClientHandler() {
                Credentials = new NetworkCredential(userName, password)
            };
            var httpClient = this.httpClient ?? new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("X-CSRF", "x");

            bool valueToSet = Math.Abs(this.TargetValue - 1d) < Double.Epsilon;
            var content = new StringContent($"value={valueToSet.ToString().ToLower()}", System.Text.Encoding.ASCII, "application/x-www-form-urlencoded");

            Logger.Debug($"Setting value for outlet {OutletNumber}: " + valueToSet.ToString().ToLower());

            HttpResponseMessage response;
            string responseBody;
            try {
                response = await httpClient.PutAsync($"http://{serverAddress}/restapi/relay/outlets/{OutletNumber}/state/", content);
                responseBody = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != HttpStatusCode.NoContent) {
                    Logger.Error($"Response: {response.StatusCode}");
                    Logger.Error($"Response: {responseBody}");
                    return;
                }
            } catch (Exception ex) {
                Logger.Error($"Failed to set status of outlet {OutletNumber}: {ex.Message}");
                return;
            }
        }

        public short Id { get; private set; }
        public string Name { get; private set; } = name;
        public string Description { get; private set; }

        private double value = 0d;

        public double Value {
            get => value; private set {
                this.value = value;
                RaisePropertyChanged();
            }
        }

        public int OutletNumber { get; private set; } = outletNumber;

        private double targetValue;

        public double Maximum => 1d;

        public double Minimum => 0d;

        public double StepSize => 1d;

        public double TargetValue {
            get => targetValue; set {
                targetValue = value < 0d ? 0d : value > 1d ? 1d : value;
                RaisePropertyChanged();
            }
        }

        private readonly HttpClient httpClient = mockClient;
        private readonly string serverAddress = mockServerAddress ?? Properties.Settings.Default.ServerAddress;
        private readonly string userName = mockUsername ?? Properties.Settings.Default.Username;
        private readonly string password = mockPassword ?? Properties.Settings.Default.Password;
    }
}