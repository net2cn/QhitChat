using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QhitChat_Client.API
{
    class Authentication
    {
        public static async Task<bool> LoginAsync(string uuid, string password)
        {
            return await Core.Configuration.Network.InvokeAsync<bool>("Login", uuid, password);
        }
    }
}
