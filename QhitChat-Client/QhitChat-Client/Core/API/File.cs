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

        public async static Task<bool> UploadNewAvatarAsync(string account, string token, byte[] newAvatar)
        {
            return await Configuration.Network.InvokeAsync<bool>("File/UploadNewAvatar", account, token, newAvatar);
        }

        public async static Task<string> CreateEmptyFileAsync(string account, string token, string originalFilename, long filesize)
        {
            return await Configuration.Network.InvokeAsync<string>("File/CreateEmptyFile", account, token, originalFilename, filesize);
        }

        public async static Task<bool> UploadFileByChunckAsync(string account, string token, string uuid, int chunckNo, byte[] data)
        {
            return await Configuration.Network.InvokeAsync<bool>("File/UploadFileByChunck", account, token, uuid, chunckNo, data);
        }

        public async static Task<string> GetOriginalFilenameAsync(string account, string token, string uuid)
        {
            return await Configuration.Network.InvokeAsync<string>("File/GetOriginalFilename", account, token, uuid);
        }

        public async static Task<long> GetFileSizeAsync(string account, string token, string uuid)
        {
            return await Configuration.Network.InvokeAsync<long>("File/GetFileSize", account, token, uuid);
        }

        public async static Task<byte[]> GetFileByChunckAsync(string account, string token, string uuid, int chunckNo)
        {
            return await Configuration.Network.InvokeAsync<byte[]>("File/GetFileByChunck", account, token, uuid, chunckNo);
        }
    }
}
