using StreamJsonRpc;
using System.Collections.Generic;
using System.Linq;

namespace QhitChat_Server.API
{
    class Relationship
    {
        [JsonRpcMethod("Relationship/GetRelationship")]
        public Dictionary<string, string> GetRelationship(string account, string token)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user != null && user.Token == token)
            {
                var toUsers = (from r in Presistent.Presistent.DatabaseContext.Relationship
                                    where r.From == account
                                    select r.To).ToList();

                var relationship = (from u in Presistent.Presistent.DatabaseContext.User
                                         where toUsers.Contains(u.Account)
                                         select u).ToDictionary(u => u.Account, u => u.Username);

                return relationship;
            }

            return null;
        }
    }
}
