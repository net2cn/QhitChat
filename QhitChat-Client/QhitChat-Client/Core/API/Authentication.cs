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
    }
}
