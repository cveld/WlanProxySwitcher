using System;
using System.Collections.Generic;
using System.Text;

namespace WlanProxySwitcher
{
    public class WlanProxyConfigModel
    {
        public string Wlan { get; set; }
        public string Proxy { get; set; }
        public string Exclude { get; set; }
    }
}
