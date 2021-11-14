using StreamJsonRpc;
using System.Collections.Generic;
using System.Linq;

namespace QhitChat_Server.API
{
    class Relationship
    {
        [JsonRpcMethod("Relationship/GetRelationship")]
        public List<string> GetRelationship(string account, string token)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == account
                        select u).SingleOrDefault();
            if (user != null && user.Token == token)
            {
                var relationship = (from r in Presistent.Presistent.DatabaseContext.Relationship
                                    where r.From == account
                                    select r.To).ToList();
                return relationship;
            }

            return null;
        }
    }
}
