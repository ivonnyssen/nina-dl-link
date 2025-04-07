using IgorVonNyssen.NINA.DlLink.DlLinkSequenceItems;
using NINA.Core.Utility;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IgorVonNyssen.NINA.DlLink.DlLinkDrivers {

    public class Result<T> {
        private readonly bool ok;
        public bool IsOk { get => ok; }
        public bool IsErr { get => !ok; }
        public T Value { get; }

        private Result(bool ok, T value) {
            this.ok = ok;
            Value = value;
        }

        public static Result<T> Ok(T value) => new(true, value);

        public static Result<T> Err() => new(false, default);
    }

    public class HttpUtils {

        public static bool SetState(int outletNumber, OutletActions action) {
            return true;
        }

        public static async Task<Result<IList<string>>> GetOutletNames(HttpClient httpClient, string serverAddress, CancellationToken token) {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response;
            string responseBody;
            try {
                response = await httpClient.GetAsync($"http://{serverAddress}/restapi/relay/outlets/all;/name/", token);
                responseBody = await response.Content.ReadAsStringAsync(token);
                if (response.StatusCode != HttpStatusCode.MultiStatus) {
                    Logger.Error($"Response: {response.StatusCode}");
                    Logger.Error($"Response: {responseBody}");
                    Logger.Error($"Failed to connect to {serverAddress}");
                    return Result<IList<string>>.Err();
                }
            } catch (Exception ex) {
                Logger.Error($"Failed to connect to {serverAddress}: {ex.Message}");
                return Result<IList<string>>.Err();
            }
            List<string> outletNames;
            try { outletNames = JsonSerializer.Deserialize<List<string>>(responseBody); } catch (JsonException ex) {
                Logger.Error($"Failed to parse outlet names from {serverAddress}: {ex.Message}");
                return Result<IList<string>>.Err();
            }
            Logger.Debug($"Outlet names: {string.Join(", ", outletNames)}");
            return Result<IList<string>>.Ok(outletNames);
        }

        public static async Task<Result<bool>> GetOutletState(HttpClient httpClient, string serverAddress, int outletNumber, CancellationToken token) {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response;
            string responseBody;
            try {
                response = await httpClient.GetAsync($"http://{serverAddress}/restapi/relay/outlets/{outletNumber}/state/", token);
                responseBody = await response.Content.ReadAsStringAsync(token);
                if (response.StatusCode != HttpStatusCode.OK) {
                    Logger.Error($"Response: {response.StatusCode}");
                    Logger.Error($"Response: {responseBody}");
                    return Result<bool>.Err();
                }
            } catch (Exception ex) {
                Logger.Error($"Failed to read status of outlet {outletNumber}: {ex.Message}");
                return Result<bool>.Err();
            }

            bool on;
            try {
                on = JsonSerializer.Deserialize<bool>(responseBody);
            } catch (JsonException ex) {
                Logger.Error($"Failed to parse outlet state response for outlet {outletNumber}: {ex.Message}");
                return Result<bool>.Err();
            }
            return Result<bool>.Ok(on);
        }

        public static async Task<Result<bool>> SetOutletState(HttpClient httpClient, string serverAddress, int outletNumber, bool valueToSet, CancellationToken token) {
            httpClient.DefaultRequestHeaders.Add("X-CSRF", "x");

            var content = new StringContent($"value={valueToSet.ToString().ToLower()}", System.Text.Encoding.ASCII, "application/x-www-form-urlencoded");

            Logger.Debug($"Setting value for outlet {outletNumber}: " + valueToSet.ToString().ToLower());

            HttpResponseMessage response;
            string responseBody;
            try {
                response = await httpClient.PutAsync($"http://{serverAddress}/restapi/relay/outlets/{outletNumber}/state/", content, token);
                responseBody = await response.Content.ReadAsStringAsync(token);
                if (response.StatusCode != HttpStatusCode.NoContent) {
                    Logger.Error($"Response: {response.StatusCode}");
                    Logger.Error($"Response: {responseBody}");
                    return Result<bool>.Err();
                }
            } catch (Exception ex) {
                Logger.Error($"Failed to set status of outlet {outletNumber}: {ex.Message}");
                return Result<bool>.Err();
            }
            return Result<bool>.Ok(true);
        }

        public static async Task<Result<bool>> TriggerOutletAction(HttpClient httpClient, string serverAddress, int outletNumber, OutletActions action, CancellationToken token) {
            switch (action) {
                case OutletActions.On:
                    return await SetOutletState(httpClient, serverAddress, outletNumber, true, token);

                case OutletActions.Off:
                    return await SetOutletState(httpClient, serverAddress, outletNumber, false, token);

                case OutletActions.Cycle:
                    httpClient.DefaultRequestHeaders.Add("X-CSRF", "x");
                    Logger.Debug($"Cycling outlet {outletNumber}: ");
                    HttpResponseMessage response;
                    string responseBody;
                    try {
                        response = await httpClient.PostAsync($"http://{serverAddress}/restapi/relay/outlets/{outletNumber}/cycle/", default, token);
                        responseBody = await response.Content.ReadAsStringAsync(token);
                        if (response.StatusCode != HttpStatusCode.NoContent) {
                            Logger.Error($"Response: {response.StatusCode}");
                            Logger.Error($"Response: {responseBody}");
                            return Result<bool>.Err();
                        }
                    } catch (Exception ex) {
                        Logger.Error($"Failed to set status of outlet {outletNumber}: {ex.Message}");
                        return Result<bool>.Err();
                    }
                    return Result<bool>.Ok(true);

                default:
                    Logger.Error($"Invalid action: {action}");
                    return Result<bool>.Err();
            }
        }
    }
}