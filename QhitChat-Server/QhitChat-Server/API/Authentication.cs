using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QhitChat_Server.API
{
    class Authentication
    {
        public JsonRpc Remote;
        public string Account;
        public string Token;

        [JsonRpcMethod("Authentication/Login")]
        public string Login(string account, string password)
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
                // Generate token for further session authentication usage.
                var token = Core.Utils.GenerateToken();
                user.Token = token;
                user.Status = 1;
                Account = user.Account;
                Token = token;
                Presistent.Presistent.DatabaseContext.SaveChanges();
                Presistent.Quene.MessageQuene.CreateQuene(account, Remote);
                Console.Error.WriteLineAsync($"Account {Account} logged in.");
                return token;
            }

            return Core.CodeDefinition.Authentication.WrongPassword;
        }

        [JsonRpcMethod("Authentication/Logout")]
        public bool Logout(string account, string token)
        {
            if (String.IsNullOrEmpty(Account) || String.IsNullOrEmpty(Token))
            {
                return true;
            }

            if (Token == token)
            {
                var user = (from u in Presistent.Presistent.DatabaseContext.User
                            where u.Account == account
                            select u).SingleOrDefault();

                // Revoke user token.
                user.Token = null;
                user.Status = 0;
                Presistent.Presistent.DatabaseContext.SaveChanges();
                Presistent.Quene.MessageQuene.DeleteQuene(account);
                Console.Error.WriteLineAsync($"Account {Account} logged out.");
                Account = null;
                Token = null;
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

        [JsonRpcMethod("Authentication/FindUser")]
        public Dictionary<string, string> FindUser(string account, int pageIndex)
        {
            var users = Presistent.Presistent.DatabaseContext.User
                .Where(u=> u.Account.Contains(account) || u.Username.Contains(account))
                .OrderByDescending(u=>u.Account)
                .Skip(pageIndex*10)
                .Take(10)
                .ToDictionary(u => u.Account, u => u.Username);

            return users;
        }

        [JsonRpcMethod("Authentication/ChangeUsername")]
        public bool ChangeUsername(string account, string token, string newUsername)
        {
            if (Token == token)
            {
                var user = (from u in Presistent.Presistent.DatabaseContext.User
                            where u.Account == account
                            select u).SingleOrDefault();

                user.Username = newUsername;
                Presistent.Presistent.DatabaseContext.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
