using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace QhitChat_Client.Core
{
    public sealed class Configuration
    {
        // Global Configuration
        public static string TITLE = "QhitChat";

        // Networking
#if DEBUG
        public const string HOSTNAME = "localhost";
#else
        public const string HOSTNAME = "home.plus1sec.tech";
#endif
        public const int PORT = 23340;
        public static readonly X509Certificate CERTIFICATE = new X509Certificate(@"Certificate/server.cer", "");
        public static Network Network = new Network(HOSTNAME, PORT, CERTIFICATE);

        // User Authentication
        public static string username;
        public static string password;
        public static string authToken;

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
