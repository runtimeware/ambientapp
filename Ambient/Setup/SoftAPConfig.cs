using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ambient.Models;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PCLCrypto;
using static PCLCrypto.WinRTCrypto;

using Sockets;

namespace Ambient.Setup
{
    #region Enums

    /// <summary>
    /// Enumeration for Particle Device Setup commands
    /// </summary>
    public enum SetupCommand
    {
        None,
        Version,
        DeviceId,
        ScanAP,
        PublicKey,
        ConfigureAP,
        ConnectAP,
        Set
    }

    #endregion

    /// <summary>
    /// A static class for sending and recieving data from a Particle Device in Listening mode
    /// </summary>
    public static class SoftAPConfig
    {
        #region Private Static Members

        private static SoftAPData softAPData = null;

        #endregion

        #region Static Properties

        /// <summary>
        /// Instance of a static SoftAPData for simplicity
        /// </summary>
        public static SoftAPData SoftAPData
        {
            get
            {
                if (softAPData == null)
                    ResetSoftAPData();

                return softAPData;
            }
        }

        #endregion

        #region SoftAPData Methods

        /// <summary>
        /// Reset the internal SoftAPData class
        /// </summary>
        public static void ResetSoftAPData()
        {
            softAPData = new SoftAPData();
        }

        #endregion

        #region Public Get Methods

        /// <summary>
        /// Task to call the "device-id" command from a Particle Device in Listening mode
        /// </summary>
        /// <returns>Returns a SoftAPDeviceId instance</returns>
        public static async Task<SoftAPDeviceId> GetDeviceIdAsync()
        {
            string responseContent = await SendSoftAPCommandAsync(SetupCommand.DeviceId);
            if (responseContent == null)
                return null;

            return JsonConvert.DeserializeObject<SoftAPDeviceId>(responseContent);
        }

        /// <summary>
        /// Task to call the "public-key" command from a Particle Device in Listening mode
        /// </summary>
        /// <returns>Returns a SoftAPPublicKey instance</returns>
        public static async Task<SoftAPPublicKey> GetPublicKeyAsync()
        {
            string responseContent = await SendSoftAPCommandAsync(SetupCommand.PublicKey);
            if (responseContent == null)
                return null;

            return JsonConvert.DeserializeObject<SoftAPPublicKey>(responseContent);
        }

        /// <summary>
        /// Task to call the "scan-ap" command from a Particle Device in Listening mode
        /// </summary>
        /// <returns>Returns a SoftAPScanAP instance</returns>
        public static async Task<List<SoftAPScanAP>> GetScanAPsAsync()
        {
            string responseContent = await SendSoftAPCommandAsync(SetupCommand.ScanAP);
            if (responseContent == null)
                return null;

            var result = JToken.Parse(responseContent);
            List<SoftAPScanAP> scanAPs = new List<SoftAPScanAP>();

            foreach (JObject scanAPEntryResponse in (JArray)result["scans"])
                scanAPs.Add(scanAPEntryResponse.ToObject<SoftAPScanAP>());

            var results = (from scanAP in scanAPs
                           group scanAP by scanAP.SSID into groupedItems
                           let maxPriority = groupedItems.Max(item => item.RSSI)
                           from element in groupedItems
                           where element.RSSI == maxPriority
                           select element).Distinct().OrderByDescending(item => item.RSSI);

            return results.ToList();
        }

        /// <summary>
        /// Task to call the "version" command from a Particle Device in Listening mode
        /// </summary>
        /// <returns>Returns a SoftAPVersion instance</returns>
        public static async Task<SoftAPVersion> GetVersionAsync()
        {
            string responseContent = await SendSoftAPCommandAsync(SetupCommand.Version);
            if (responseContent == null)
                return null;

            return JsonConvert.DeserializeObject<SoftAPVersion>(responseContent);
        }

        #endregion

        #region Public Set Methods

        /// <summary>
        /// Task to call the "set" command with a claim code to a Particle Device in Listening mode
        /// </summary>
        /// <param name="claimCode">Claim code from Particle Cloud</param>
        /// <returns>Returns response code</returns>
        public static async Task<int> SetClaimCodeAsync(string claimCode)
        {
            var keyValue = new SoftAPKeyValue();
            keyValue.Key = "cc";
            keyValue.Value = claimCode;

            var keyValueString = JsonConvert.SerializeObject(keyValue);

            string responseContent = await SendSoftAPCommandAsync(SetupCommand.Set, keyValueString);
            if (responseContent == null)
                return -1;

            var result = JToken.Parse(responseContent);
            var responseCode = (int)result["r"];
            return responseCode;
        }

        /// <summary>
        /// Task to call the "configure-ap" command to a Particle Device in Listening mode
        /// </summary>
        /// <param name="index">Index for this cofiguration</param>
        /// <param name="scanAP">The SoftAPScanAP to use</param>
        /// <param name="password">The unencrypted password for the WiFi connection</param>
        /// <param name="publicKey">The publi key from the device</param>
        /// <returns>Returns response code</returns>
        public static async Task<int> SetConfigureAPAsync(int index, SoftAPScanAP scanAP, string password, SoftAPPublicKey publicKey)
        {
            var configureAP = new SoftAPConfigureAP();
            configureAP.Index = index;
            configureAP.SSID = scanAP.SSID;
            configureAP.Security = scanAP.Security;
            configureAP.Channel = scanAP.Channel;

            if (configureAP.Security != SecurityType.SecurityOpen)
            {
				var algorithm = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaPkcs1);

				var publicKeyBuffer = CryptographicBuffer.DecodeFromHexString(publicKey.Data);
				var key = algorithm.ImportPublicKey(publicKeyBuffer);

				var data = CryptographicBuffer.ConvertStringToBinary(password, System.Text.Encoding.UTF8);

				var encryptedData = CryptographicEngine.Encrypt(key, data, null);
                string encryptedPassword = CryptographicBuffer.EncodeToHexString(encryptedData);

                configureAP.Password = encryptedPassword;
            }

            var configureAPString = JsonConvert.SerializeObject(configureAP,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            string responseContent = await SendSoftAPCommandAsync(SetupCommand.ConfigureAP, configureAPString);
            if (responseContent == null)
                return -1;

            var result = JToken.Parse(responseContent);
            var responseCode = (int)result["r"];
            return responseCode;
        }

        /// <summary>
        /// Task to call the "connect-ap" command to a Particle Device in Listening mode
        /// </summary>
        /// <param name="index">The index used in the call to configure-ap</param>
        /// <returns>Returns response code</returns>
        public static async Task<int> SetConnectAPAsync(int index)
        {
            var connectAP = new SoftAPConnectAP();
            connectAP.Index = index;
            var connectAPString = JsonConvert.SerializeObject(connectAP);

            string responseContent = await SendSoftAPCommandAsync(SetupCommand.ConnectAP, connectAPString);
            if (responseContent == null)
                return -1;

            var result = JToken.Parse(responseContent);
            var responseCode = (int)result["r"];
            return responseCode;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Genericized function to send commands to a Particle device in listening mode
        /// </summary>
        /// <param name="setupCommand">The SetupCommand to use</param>
        /// <param name="data">Any extra data to send with the command</param>
        /// <returns></returns>
        private static async Task<string> SendSoftAPCommandAsync(SetupCommand setupCommand, string data = null)
        {
			string hostname = "192.168.0.1";

            string command;
            switch (setupCommand)
            {
                case SetupCommand.Version:
                    command = "version";
                    break;
                case SetupCommand.DeviceId:
                    command = "device-id";
                    break;
                case SetupCommand.ScanAP:
                    command = "scan-ap";
                    break;
                case SetupCommand.PublicKey:
                    command = "public-key";
                    break;
                case SetupCommand.ConfigureAP:
                    command = "configure-ap";
                    break;
                case SetupCommand.ConnectAP:
                    command = "connect-ap";
                    break;
                case SetupCommand.Set:
                    command = "set";
                    break;
                default:
                    return null;
            }

            int dataLength = 0;
            if (data != null)
                dataLength = data.Length;

			using (Sockets.Plugin.TcpSocketClient socket = new Sockets.Plugin.TcpSocketClient())
			{
				try
				{
					var cancellationTokenSource = new CancellationTokenSource();
					cancellationTokenSource.CancelAfter(5000);

					await socket.ConnectAsync(hostname, 5609, false, cancellationTokenSource.Token) ;

					var dataToSend = $"{command}\n{dataLength}\n\n";
					if (data != null)
						dataToSend += data;

					var bytes = Encoding.UTF8.GetBytes(dataToSend);
					await socket.WriteStream.WriteAsync(bytes, 0, bytes.Length);
					await socket.WriteStream.FlushAsync();

					var buffer = new byte[2048];

					var cancellationTokenSourceRead = new CancellationTokenSource();
					cancellationTokenSourceRead.CancelAfter(10000);
					int count = await socket.ReadStream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSourceRead.Token);

					var receivedData = "";
					if (count > 0)
						receivedData = Encoding.UTF8.GetString(buffer, 0, count);

					if (string.IsNullOrWhiteSpace(receivedData))
						return null;

					return receivedData;
				}
				catch (Exception exception)
				{
					throw exception;
				}

                return null;
            }
        }

        #endregion
    }
}
