using StreamJsonRpc;
using System.Linq;

namespace QhitChat_Server.API
{
    class Authentication
    {
        [JsonRpcMethod("Authentication/Login")]
        public static string Login(string account, string password)
        {
            // TODO: Generate unique token and return to client for furthuer usage.
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if(user == null)
            {
                return Core.CodeDefinition.Authentication.NoUser;
            }

            if (user.Password == password)
            {
                var token = Core.Authentication.GenerateToken();
                user.Token = token;
                user.Status = 1;
                Presistent.Presistent.DatabaseContext.SaveChanges();
                return token;
            }
            return Core.CodeDefinition.Authentication.WrongPassword;
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

        [JsonRpcMethod("Authentication/GetUsername")]
        public string GetUsername(string account)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user != null)
            {
                return user.Username;
            }
            return null;
        }
    }
}
