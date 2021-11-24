using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QhitChat_Client.Core.API
{
    class Chat
    {
        public static async Task<bool> SendAsync(string from, string token, string to, string content)
        {
            return await Configuration.Network.InvokeAsync<bool>("Chat/Send", from, token, to, content);
        }

        public static async Task<Presistent.Database.Models.Messages> FetchAsync(string to, string token)
        {
            return await Configuration.Network.InvokeAsync<Presistent.Database.Models.Messages>("Chat/Fetch", to, token);
        }
    }
}
