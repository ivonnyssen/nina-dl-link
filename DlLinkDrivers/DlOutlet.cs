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
                switch (await HttpUtils.GetOutletState(httpClient, serverAddress, OutletNumber, default)) {
                    case { IsOk: true, Value: var result }:
                        Value = result ? 1d : 0d;
                        break;

                    default:
                        return false;
                }
                return true;
            }));
            if (success) RaisePropertyChanged(nameof(Value));

            return success;
        }

        async Task IWritableSwitch.SetValue() {
            var handler = new HttpClientHandler() {
                Credentials = new NetworkCredential(userName, password)
            };
            var httpClient = this.httpClient ?? new HttpClient(handler);
            bool valueToSet = Math.Abs(this.TargetValue - 1d) < Double.Epsilon;
            switch (await HttpUtils.SetOutletState(httpClient, serverAddress, OutletNumber, valueToSet, default)) {
                case { IsOk: true }:
                    Logger.Debug($"Set value for outlet {OutletNumber}: " + valueToSet.ToString().ToLower());
                    break;

                default:
                    break;
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