using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QhitChat_Client.Core.API
{
    class Authentication
    {
        public static async Task<bool> LoginAsync(string account, string password)
        {
            return await Configuration.Network.InvokeAsync<bool>("Login", account, password);
        }

        public static async Task<string> GetSaltAsync(string account)
        {
            return await Configuration.Network.InvokeAsync<string>("GetSalt", account);
        }
    }
}
