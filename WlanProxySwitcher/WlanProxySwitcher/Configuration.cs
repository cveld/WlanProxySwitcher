using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace WlanProxySwitcher
{
    public class Configuration
    {
        public static IEnumerable<WlanProxyConfigModel> GetConfiguration()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var result = config.GetSection("wlan-proxy").Get<IEnumerable<WlanProxyConfigModel>>();
            return result;
        }
    }
}
