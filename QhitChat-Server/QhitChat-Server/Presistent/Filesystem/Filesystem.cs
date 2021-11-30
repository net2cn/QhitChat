using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QhitChat_Server.Presistent.Filesystem
{
    class Filesystem
    {
        public static int ChunckSize = 4 * 1024 * 1024;

        public static int GetChunkCount(string path)
        {
            return (int)Math.Ceiling(new FileInfo(path).Length / (double)ChunckSize);
        }

        public static byte[] GetFileChunckByChunckNumber(string path, int chunckNo)
        {
            if(!Exists(path) || chunckNo > GetChunkCount(path) - 1)
            {
                return null;
            }

            using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                long readSize = ChunckSize;
                if (chunckNo * ChunckSize + ChunckSize > reader.BaseStream.Length)
                {
                    readSize = reader.BaseStream.Length - (chunckNo * ChunckSize);
                }
                byte[] data = new byte[readSize];
                reader.BaseStream.Seek(chunckNo * ChunckSize, SeekOrigin.Begin);
                reader.Read(data, 0, (int)readSize);
                return data;
            }
        }

        /// <summary>
        /// Check whether a file exists in filesystem.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Exists(string path)
        {
            if (System.IO.File.Exists(path))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Return file name with extension from a given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFilenameFromPath(string path)
        {
            return Path.GetFileName(path);
        }

        public static string GetDirectoryPathFromPath(string path)
        {
            return new FileInfo(path).Directory.FullName;
        }

        /// <summary>
        /// Create a empty file to write.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileSize"></param>
        public static void CreateEmptyFile(string path, long fileSize)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.SetLength(fileSize);
            }
        }

        /// <summary>
        /// Save a chunck of file to file with chunkNo.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="chunck"></param>
        /// <param name="chunckNo"></param>
        public static void SaveFileByChunckNumber(string path, byte[] chunck, int chunckNo)
        {
            if (!Exists(path) || chunckNo>GetChunkCount(path)-1)
            {
                return;
            }

            using (BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Open)))
            {
                long readSize = ChunckSize;
                if (chunckNo * ChunckSize + ChunckSize > writer.BaseStream.Length)
                {
                    readSize = writer.BaseStream.Length - (chunckNo * ChunckSize);
                }
                writer.BaseStream.Seek(chunckNo * ChunckSize, SeekOrigin.Begin);
                writer.Write(chunck);
            }
        }
    }
}
