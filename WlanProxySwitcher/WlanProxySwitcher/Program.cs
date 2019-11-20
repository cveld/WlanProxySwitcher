using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace WlanProxySwitcher
{
    class Program
    {
        static IEnumerable<WlanProxyConfigModel> WlanProxyConfigs;

        static void Main(string[] args)
        {
            WlanProxyConfigs = Configuration.GetConfiguration();
            ProcessConnectedNetworks();

            var obj = new NetworkStatusObserver();
            obj.NetworkChanged += NetworkChange_NetworkAddressChanged;
            obj.Start();

            NetworkChange.NetworkAvailabilityChanged += AvailabilityChanged;
            // NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

            Console.WriteLine("Waiting for NetworkChanged events... Press <enter> to exit...");
            Console.ReadLine();
        }

        private static void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            Console.WriteLine("NetworkAddressChanged:");
            ProcessConnectedNetworks();
        }

        private static void ProcessConnectedNetworks()
        {
            var networks = GetNetworks();
            Network foundnetwork = null;
            WlanProxyConfigModel foundWlanProxyConfigModel = null;
            bool connected = false;

            foreach (var network in networks)
            {
                // Is it a connected network?
                if (!network.IsConnected)
                {
                    continue;
                }
                connected = true;
                // If yes, find it in de config:
                var result = WlanProxyConfigs.Where(w => w.Wlan == network.Name).FirstOrDefault();
                if (result != null)
                {
                    // Is the excluded network connected?
                    if (networks.Where(n => n.IsConnected && n.Name == result.Exclude).Count() > 0)
                    {
                        // if yes, then skip the found entry
                        continue;
                    }

                    foundnetwork = network;
                    foundWlanProxyConfigModel = result;
                    break;
                }
            }

            if (foundnetwork != null)
            {
                Console.WriteLine($"Found wlan-proxy entry: {foundWlanProxyConfigModel.Wlan} with proxy setting {foundWlanProxyConfigModel.Proxy}. Activating http proxy");
                SetProxy(proxyEnabled: true, proxyHost: foundWlanProxyConfigModel.Proxy);
                return;
            }
            
            if (connected)
            {
                Console.WriteLine("Connected, but no corresponding wlan-proxy entry found. Disabling http proxy");
                SetProxy(proxyEnabled: false, proxyHost: String.Empty);
            }
                        
            // not connected, do nothing
        }

        private static void SetProxy(bool proxyEnabled, string proxyHost)
        {                        
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
            const string keyName = userRoot + "\\" + subkey;

            Registry.SetValue(keyName, "ProxyServer", proxyHost);
            Registry.SetValue(keyName, "ProxyEnable", proxyEnabled ? 1 : 0);

            Console.WriteLine($"Proxy {(proxyEnabled ? "enabled" : "disabled")}");
        }

        private static NetworkCollection GetNetworks()
        {
            // var nlm = new NetworkListManager();
            var networks = NetworkListManager.GetNetworks(NetworkConnectivityLevels.Connected);

            // IEnumNetworks networks = nlm.GetNetworks(NLM_ENUM_NETWORK.NLM_ENUM_NETWORK_ALL);
            foreach (var network in networks)
            {
                string sConnected = ((network.IsConnected == true) ? " (connected)" : " (disconnected)");
                Console.WriteLine("Network : " + network.Name + " - Category : " + network.Category.ToString() + sConnected);
            }

            return networks;
        }
        
        static private void AvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
                Console.WriteLine("Network connected!");
            else
                Console.WriteLine("Network disconnected!");
        }
    }
}
