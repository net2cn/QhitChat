using StreamJsonRpc;
using System.Collections.Generic;
using System.Linq;


namespace QhitChat_Server.API
{
    class File
    {
        [JsonRpcMethod("File/GetAvatar")]
        public byte[] GetAvatar(string account, string token)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user != null && user.Token == token)
            {
                var path = (from r in Presistent.Presistent.DatabaseContext.Avatar
                                    where r.Account == account
                                    select r.Path).SingleOrDefault();

                if (Presistent.Filesystem.Filesystem.Exists(path))
                {
                    // Avatar only allows 1 chunck.
                    return Presistent.Filesystem.Filesystem.GetFileChunckByChunckNumber(path, 0);
                }
            }

            return null;
        }
    }
}
