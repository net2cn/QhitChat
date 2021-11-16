using System.Collections.Generic;
using System.Threading.Tasks;
namespace QhitChat_Client.Core.API
{
    class File
    {
        public async static Task<byte[]> GetAvatarAsync(string account, string token)
        {
            return await Configuration.Network.InvokeAsync<byte[]>("File/GetAvatar", account, token);
        }
    }
}
