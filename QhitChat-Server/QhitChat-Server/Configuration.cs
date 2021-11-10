using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace QhitChat_Server
{
    public sealed class Configuration
    {
        public static readonly X509Certificate Certificate = new X509Certificate(@"Certificate/server.pfx", "");

        private static readonly Lazy<Configuration> lazy =
    new Lazy<Configuration>(() => new Configuration());

        public static Configuration Instance { get { return lazy.Value; } }

        private Configuration()
        {

        }
    }
}
