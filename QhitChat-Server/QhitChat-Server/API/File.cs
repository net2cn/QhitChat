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
        private static string avatarDirectory = "./Avatars";
        private static string fileDirectory = "./Files";

        [JsonRpcMethod("File/GetAvatar")]
        public Dictionary<string, byte[]> GetAvatar(string account)
        {
            var filename = (from a in Presistent.Presistent.DatabaseContext.Avatar
                                where a.Account == account
                                select a.Path).SingleOrDefault();
            if (filename != null)
            {
                var filepath = Path.Combine(avatarDirectory, filename);
                if (Filesystem.Exists(filepath))
                {
                    // Avatar only allows 1 chunck.
                    return new Dictionary<string, byte[]>() { { Filesystem.GetFilenameFromPath(filepath), Filesystem.GetFileChunckByChunckNumber(filepath, 0) } };
                }
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
                if (newAvatar.Length < Filesystem.ChunckSize)
                {
                    var avatarRecord = (from a in Presistent.Presistent.DatabaseContext.Avatar
                                where a.Account == account
                                select a).SingleOrDefault();

                    var filename = Core.Utils.GenerateToken() + ".png";
                    var filepath = Path.Combine(avatarDirectory, filename);

                    if (avatarRecord != null)
                    {
                        avatarRecord.Path = filename;
                    }
                    else
                    {
                        avatarRecord = new Presistent.Database.Models.Avatar { Account = account, Path = filename };
                        Presistent.Presistent.DatabaseContext.Avatar.Add(avatarRecord);
                    }

                    using (Image image = Image.FromStream(new MemoryStream(newAvatar)))
                    {
                        image.Save(filepath, ImageFormat.Png);
                    }

                    Presistent.Presistent.DatabaseContext.SaveChanges();
                    return true;
                }
            }

            return false;
        }

        [JsonRpcMethod("File/IsAvatarMatched")]
        public bool IsAvatarMatched(string account, string filename)
        {
            var filenameRecord = (from r in Presistent.Presistent.DatabaseContext.Avatar
                            where r.Account == account
                            select r.Path).SingleOrDefault();

            if (Filesystem.GetFilenameFromPath(filenameRecord) == filename)
            {
                return true;
            }

            return false;
        }

        [JsonRpcMethod("File/CreateEmptyFile")]
        public string CreateEmptyFile(string account, string token, string originalFilename, long filesize)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user != null && user.Token == token)
            {
                var newFilename = Core.Utils.GenerateToken();
                var newFilepath = Path.Combine(fileDirectory, newFilename);
                Filesystem.CreateEmptyFile(newFilepath, filesize);

                var newFileRecord = new Presistent.Database.Models.File { Uuid = newFilename, From = account, CreatedOn = System.DateTime.UtcNow, OriginalName = originalFilename, IsReceived=Filesystem.GetChunkCount(newFilepath) };
                Presistent.Presistent.DatabaseContext.File.Add(newFileRecord);
                Presistent.Presistent.DatabaseContext.SaveChanges();
                return newFilename;
            }

            return null;
        }

        [JsonRpcMethod("File/UploadFileByChunck")]
        public bool UploadFileByChunck(string account, string token, string uuid, int chunckNo, byte[] data)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user != null && user.Token == token)
            {
                if (data.Length > Filesystem.ChunckSize)
                {
                    return false;
                }

                var fileRecord = (from f in Presistent.Presistent.DatabaseContext.File
                            where f.Uuid == uuid
                            select f).SingleOrDefault();

                if (fileRecord != null)
                {
                    if (fileRecord.IsReceived != 0)
                    {
                        var filepath = Path.Combine(fileDirectory, uuid);
                        if (chunckNo < Filesystem.GetChunkCount(filepath))
                        {
                            Filesystem.SaveFileByChunckNumber(filepath, data, chunckNo);
                            fileRecord.IsReceived -= 1;
                            Presistent.Presistent.DatabaseContext.SaveChanges();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        [JsonRpcMethod("File/GetOriginalFilename")]
        public string GetOriginalFilename(string account, string token, string uuid)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user != null && user.Token == token)
            {
                var fileRecord = (from f in Presistent.Presistent.DatabaseContext.File
                                  where f.Uuid == uuid
                                  select f).SingleOrDefault();

                if (fileRecord != null)
                {
                    return fileRecord.OriginalName;
                }
            }

            return null;
        }

        [JsonRpcMethod("File/GetFileSize")]
        public long GetFileSize(string account, string token, string uuid)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user != null && user.Token == token)
            {
                var fileRecord = (from f in Presistent.Presistent.DatabaseContext.File
                                  where f.Uuid == uuid
                                  select f).SingleOrDefault();

                if (fileRecord != null)
                {
                    var filepath = Path.Combine(fileDirectory, uuid);
                    return Filesystem.GetFileSize(filepath);
                }
            }

            return 0;
        }

        [JsonRpcMethod("File/GetFileByChunck")]
        public byte[] GetFileByChunck(string account, string token, string uuid, int chunckNo)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user != null && user.Token == token)
            {
                var fileRecord = (from f in Presistent.Presistent.DatabaseContext.File
                            where f.Uuid == uuid
                            select f).SingleOrDefault();

                if (fileRecord != null)
                {
                    if (fileRecord.IsReceived == 0)
                    {
                        var filepath = Path.Combine(fileDirectory, uuid);
                        if (chunckNo < Filesystem.GetChunkCount(filepath))
                        {
                            var data = Filesystem.GetFileChunckByChunckNumber(filepath, chunckNo);
                            return data;
                        }
                    }
                }
            }

            return null;
        }
    }
}
