using System.Collections.Generic;
using System.Threading.Tasks;

namespace QhitChat_Client.Core.API
{
    class Relationship
    {
        public async static Task<Dictionary<string, string>> GetRelationshipAsync(string account, string token)
        {
            return await Configuration.Network.InvokeAsync<Dictionary<string, string>>("Relationship/GetRelationship", account, token);
        }
    }
}
