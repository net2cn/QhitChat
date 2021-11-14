using StreamJsonRpc;
using System.Linq;

namespace QhitChat_Server.API
{
    class Authentication
    {
        [JsonRpcMethod("Authentication/Login")]
        public static bool Login(string account, string password)
        {
            // TODO: Generate unique token and return to client for furthuer usage.
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user != null && user.Password == password)
            {
                return true;
            }
            return false;
        }

        [JsonRpcMethod("Authentication/GetSalt")]
        public string GetSalt(string account)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user != null)
            {
                return user.Salt;
            }
            return null;
        }
    }
}
