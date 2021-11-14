using System.Threading.Tasks;

namespace QhitChat_Client.Core.API
{
    class Utils
    {
        public async static Task<string> PingAsync()
        {
            return await Configuration.Network.InvokeAsync<string>("Ping");
        }
    }
}