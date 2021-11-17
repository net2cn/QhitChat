using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QhitChat_Server.API
{
    class Authentication
    {
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
                var token = Core.Authentication.GenerateToken();
                user.Token = token;
                user.Status = 1;
                Account = user.Account;
                Token = token;
                Presistent.Presistent.DatabaseContext.SaveChanges();
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
        public Dictionary<string, string> FindUser(string account)
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
