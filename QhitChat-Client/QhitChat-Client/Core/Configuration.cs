using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace QhitChat_Client.Core
{
    public sealed class Configuration
    {
        // Global Configuration
        public static string TITLE = "QhitChat";

        // Networking
#if DEBUG
        public const string HOSTNAME = "loopback.plus1sec.tech";
#else
        public const string HOSTNAME = "home.plus1sec.tech";
#endif
        public const int PORT = 23340;
        public static readonly X509Certificate CERTIFICATE = new X509Certificate(@"Certificate/server.cer", "");
        public static Network Network = new Network(HOSTNAME, PORT, CERTIFICATE);
        public static Notification.Notification Notification = new Notification.Notification();

        // User Authentication
        public static string Account;
        public static string Password;
        public static string Username;
        public static string Token;

        // File paths
        public static string AvatarDirectory = "./Cache/Avatars/";
        public static string FileDirectory = "./Cache/Files/";

        // File-related
        public static string ImageExtensions = "png,jpg,bmp,gif";

        private static readonly Lazy<Configuration> lazy =
            new Lazy<Configuration>(() => new Configuration());

        public static Configuration Instance { get { return lazy.Value; } }

        private Configuration()
        {
#if DEBUG
            Trace.WriteLine("Debug mode running.");
            TITLE = "QhitChat DEBUG Version" + String.Format(" {0}", Assembly.GetExecutingAssembly().GetName().Version);
#endif
        }
    }
}
