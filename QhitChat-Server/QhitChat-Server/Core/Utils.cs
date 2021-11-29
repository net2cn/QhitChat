using System;
using System.Collections.Generic;
using System.Text;

namespace QhitChat_Server.Core
{
    class Utils
    {
        public static string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GenerateSalt()
        {
            return GenerateToken().Substring(0, 8);
        }
    }
}
