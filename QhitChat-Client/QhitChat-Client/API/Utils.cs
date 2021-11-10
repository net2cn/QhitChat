using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QhitChat_Client.API
{
    class Utils
    {
        public async static Task<string> PingAsync()
        {
            if (Core.Configuration.Network.Connected == true)
            {
                return await Core.Configuration.Network.Remote.InvokeAsync<string>("Ping");
            }
            else
            {
                return null;
            }
        }
    }
}
