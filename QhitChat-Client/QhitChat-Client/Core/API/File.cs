using System.Collections.Generic;
using System.Threading.Tasks;
namespace QhitChat_Client.Core.API
{
    class File
    {
        public async static Task<Dictionary<string, byte[]>> GetAvatarAsync(string account)
        {
            return await Configuration.Network.InvokeAsync<Dictionary<string, byte[]>>("File/GetAvatar", account);
        }

        public async static Task<bool> IsAvatarMatchedAsync(string account, string filename)
        {
            return await Configuration.Network.InvokeAsync<bool>("File/IsAvatarMatched", account, filename);
        }
    }
}
