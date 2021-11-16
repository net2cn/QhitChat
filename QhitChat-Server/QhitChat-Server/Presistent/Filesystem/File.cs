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
            return (int)(new FileInfo(path).Length / ChunckSize);
        }

        public static byte[] GetFileChunckByChunckNumber(string path, int chunckNo)
        {
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

        public static bool Exists(string path)
        {
            if (File.Exists(path))
            {
                return true;
            }
            return false;
        }
    }
}
