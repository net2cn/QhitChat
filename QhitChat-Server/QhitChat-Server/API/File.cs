using StreamJsonRpc;
using System.Collections.Generic;
using System.Linq;
using QhitChat_Server.Presistent.Filesystem;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

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

        [JsonRpcMethod("File/UploadNewAvatar")]
        public bool UploadNewAvatar(string account, string token, byte[] newAvatar)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user != null && user.Token == token)
            {
                var path = (from r in Presistent.Presistent.DatabaseContext.Avatar
                            where r.Account == account
                            select r).SingleOrDefault();

                var filename = Core.Utils.GenerateToken() + ".png";
                var savePath = "./Avatars";
                savePath = Path.Combine(savePath, filename);

                if (path != null)
                {
                    path.Path = savePath;
                }
                else
                {
                    path = new Presistent.Database.Models.Avatar { Account = account, Path = savePath };
                    Presistent.Presistent.DatabaseContext.Avatar.Add(path);
                }

                if (newAvatar.Length < Filesystem.ChunckSize)
                {
                    using (Image image = Image.FromStream(new MemoryStream(newAvatar)))
                    {
                        image.Save(savePath, ImageFormat.Png);  // Or Png
                    }
                }

                Presistent.Presistent.DatabaseContext.SaveChanges();
                return true;
            }

            return false;
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
