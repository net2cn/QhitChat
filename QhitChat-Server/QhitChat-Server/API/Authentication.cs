using StreamJsonRpc;
using System.Collections.Generic;
using System.Linq;

namespace QhitChat_Server.API
{
    class Authentication
    {
        [JsonRpcMethod("Authentication/Login")]
        public static string Login(string account, string password)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if(user == null)
            {
                return Core.CodeDefinition.Authentication.NoUser;
            }

            if (user.Password == password)
            {
                if (user.Status != 1)
                {
                    // Generate token for further session authentication usage.
                    var token = Core.Authentication.GenerateToken();
                    user.Token = token;
                    user.Status = 1;
                    Presistent.Presistent.DatabaseContext.SaveChanges();
                    return token;
                }
                else
                {
                    return user.Token;
                }
            }
            return Core.CodeDefinition.Authentication.WrongPassword;
        }

        [JsonRpcMethod("Authentication/GetSalt")]
        public static string GetSalt(string account)
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
        public static string GetUsername(string account)
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

        [JsonRpcMethod("Authentication/FindUser")]
        public static Dictionary<string, string> FindUser(string account)
        {
            var users = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account.Contains(account) || u.Username.Contains(account)
                        select u).ToDictionary(u=>u.Account, u=>u.Username);
            if (users != null)
            {
                return users;
            }
            return null;
        }

        [JsonRpcMethod("Authentication/ChangeUsername")]
        public static bool ChangeUsername(string account, string token, string newUsername)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user == null)
            {
                return false;
            }

            if (user.Token == token)
            {
                user.Username = newUsername;
                Presistent.Presistent.DatabaseContext.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
