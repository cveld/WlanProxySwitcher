using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace WlanProxySwitcher
{
    // Source code borrowed from https://stackoverflow.com/questions/1859799/determine-if-internet-connection-is-available
    public class NetworkInterfaceStatusObserver
    {
        public event EventHandler<EventArgs> NetworkChanged;

        private NetworkInterface[] oldInterfaces;
        private Timer timer;

        public void Start()
        {
            timer = new Timer(UpdateNetworkStatus, null, new TimeSpan(0, 0, 0, 0, 500), new TimeSpan(0, 0, 0, 0, 500));            
            oldInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        }

        private void UpdateNetworkStatus(object o)
        {
            Console.WriteLine("Heartbeat");
            var newInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            bool hasChanges = false;
            if (newInterfaces.Length != oldInterfaces.Length)
            {
                hasChanges = true;
            }
            if (!hasChanges)
            {
                for (int i = 0; i < oldInterfaces.Length; i++)
                {
                    if (oldInterfaces[i].Name != newInterfaces[i].Name || oldInterfaces[i].OperationalStatus != newInterfaces[i].OperationalStatus)
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
