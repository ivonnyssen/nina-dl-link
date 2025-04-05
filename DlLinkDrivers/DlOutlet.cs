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

    public class DlOutlet(string name, int outletNumber, IPluginOptionsAccessor pluginSettings, HttpClient mockClient = null) : BaseINPC, IWritableSwitch {

        public async Task<bool> Poll() {
            var success = await Task.Run((async () => {
                SetupHttpClientHandler(out string serverAddress, out HttpClientHandler handler);
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
                } catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException) {
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
            SetupHttpClientHandler(out string serverAddress, out HttpClientHandler handler);
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
            } catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException) {
                Logger.Error($"Failed to set status of outlet {OutletNumber}: {ex.Message}");
                return;
            }
        }

        private void SetupHttpClientHandler(out string serverAddress, out HttpClientHandler handler) {
            var userName = pluginSettings.GetValueString(nameof(DlLink.DLUserName), string.Empty);
            var password = pluginSettings.GetValueString(nameof(DlLink.DLPassword), string.Empty);
            serverAddress = pluginSettings.GetValueString(nameof(DlLink.DLServerAddress), string.Empty);
            handler = new HttpClientHandler() {
                Credentials = new NetworkCredential(userName, password)
            };
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

        private readonly IPluginOptionsAccessor pluginSettings = pluginSettings;

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
    }
}