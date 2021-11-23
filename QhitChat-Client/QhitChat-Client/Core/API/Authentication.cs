using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QhitChat_Client.Core.API
{
    class Authentication
    {
        public static async Task<string> LoginAsync(string account, string password)
        {
            return await Configuration.Network.InvokeAsync<string>("Authentication/Login", account, password);
        }

        public static async Task<string> GetSaltAsync(string account)
        {
            return await Configuration.Network.InvokeAsync<string>("Authentication/GetSalt", account);
        }

        public static async Task<string> GetUsernameAsync(string account)
        {
            return await Configuration.Network.InvokeAsync<string>("Authentication/GetUsername", account);
        }

        public static async Task<Dictionary<string, string>> FindUserAsync(string account, int page)
        {
            return await Configuration.Network.InvokeAsync<Dictionary<string, string>>("Authentication/FindUser", account, page);
        }

        public static async Task<bool> ChangeUsernameAsync(string account, string token, string newUsername)
        {
            return await Configuration.Network.InvokeAsync<bool>("Authentication/ChangeUsername", account, token, newUsername);
        }
    }
}
