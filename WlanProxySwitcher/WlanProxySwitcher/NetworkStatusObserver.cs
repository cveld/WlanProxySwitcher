using Microsoft.WindowsAPICodePack.Net;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Linq;
using System.Runtime.InteropServices;

namespace WlanProxySwitcher
{
    // Source code borrowed from https://stackoverflow.com/questions/1859799/determine-if-internet-connection-is-available
    public class NetworkStatusObserver
    {
        public event EventHandler<EventArgs> NetworkChanged;

        private NetworkStatusModel[] oldInterfaces;
        private Timer timer;

        public void Start()
        {
            timer = new Timer(UpdateNetworkStatus, null, new TimeSpan(0, 0, 0, 0, 500), new TimeSpan(0, 0, 0, 0, 500));

            oldInterfaces = GetConnectedNetworks().ToArray();
        }

        private IEnumerable<NetworkStatusModel> GetConnectedNetworks()
        {
            var result = NetworkListManager.GetNetworks(NetworkConnectivityLevels.Connected).Select(c => {
                bool? IsConnected = null;
                try {
                    IsConnected = c.IsConnected;
                }
                catch (COMException e)
                {
                    Console.WriteLine(e.Message);                    
                }
                return new NetworkStatusModel
                {
                    Name = c.Name,
                    IsConnected = IsConnected
                };
            });
            return result;
        }

        private void UpdateNetworkStatus(object o)
        {
            var newInterfaces = GetConnectedNetworks().ToArray();
            bool hasChanges = false;
            if (newInterfaces.Length != oldInterfaces.Length)
            {
                hasChanges = true;
            }
            if (!hasChanges)
            {
                for (var i = 0; i < oldInterfaces.Length; i++)
                {                    
                    if (oldInterfaces[i].Name != newInterfaces[i].Name || oldInterfaces[i].IsConnected != newInterfaces[i].IsConnected)
                    {
                        hasChanges = true;
                        break;
                    }
                    
                }
            }

            oldInterfaces = newInterfaces;

            if (hasChanges)
            {
                Console.WriteLine("HasChanges");
                RaiseNetworkChanged();
            }
        }

        private void RaiseNetworkChanged()
        {
            if (NetworkChanged != null)
            {
                NetworkChanged.Invoke(this, null);
            }
        }
    }
}
