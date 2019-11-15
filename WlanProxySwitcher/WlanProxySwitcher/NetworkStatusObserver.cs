using Microsoft.WindowsAPICodePack.Net;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Linq;

namespace WlanProxySwitcher
{
    // Source code borrowed from https://stackoverflow.com/questions/1859799/determine-if-internet-connection-is-available
    public class NetworkStatusObserver
    {
        public event EventHandler<EventArgs> NetworkChanged;

        private Network[] oldInterfaces;
        private Timer timer;

        public void Start()
        {
            timer = new Timer(UpdateNetworkStatus, null, new TimeSpan(0, 0, 0, 0, 500), new TimeSpan(0, 0, 0, 0, 500));

            oldInterfaces = NetworkListManager.GetNetworks(NetworkConnectivityLevels.Connected).ToArray();
        }

        private void UpdateNetworkStatus(object o)
        {
            var newInterfaces = NetworkListManager.GetNetworks(NetworkConnectivityLevels.Connected).ToArray();
            bool hasChanges = false;
            if (newInterfaces.Count() != oldInterfaces.Length)
            {
                hasChanges = true;
            }
            if (!hasChanges)
            {
                for (int i = 0; i < oldInterfaces.Length; i++)
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
