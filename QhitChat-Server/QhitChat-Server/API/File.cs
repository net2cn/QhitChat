using StreamJsonRpc;
using System.Collections.Generic;
using System.Linq;
using QhitChat_Server.Presistent.Filesystem;

namespace QhitChat_Server.API
{
    class File
    {
        [JsonRpcMethod("File/GetAvatar")]
        public Dictionary<string, byte[]> GetAvatar(string account)
        {
            var path = (from r in Presistent.Presistent.DatabaseContext.Avatar
                                where r.Account == account
                                select r.Path).SingleOrDefault();

            if (Filesystem.Exists(path))
            {
                // Avatar only allows 1 chunck.
                return new Dictionary<string, byte[]>() { { Filesystem.GetFilenameFromPath(path), Filesystem.GetFileChunckByChunckNumber(path, 0) } };
            }

            return null;
        }

        [JsonRpcMethod("File/IsAvatarMatched")]
        public bool IsAvatarMatched(string account, string filename)
        {
            var path = (from r in Presistent.Presistent.DatabaseContext.Avatar
                        where r.Account == account
                        select r.Path).SingleOrDefault();

            if (Filesystem.GetFilenameFromPath(path) == filename)
            {
                return true;
            }

            return false;
        }
    }
}
