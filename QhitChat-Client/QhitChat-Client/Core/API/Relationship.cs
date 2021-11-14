using System.Collections.Generic;
using System.Threading.Tasks;

namespace QhitChat_Client.Core.API
{
    class Relationship
    {
        public async static Task<List<string>> GetRelationshipAsync(string account, string token)
        {
            return await Configuration.Network.InvokeAsync<List<string>>("Authentication/GetRelationship", account, token);
        }
    }
}
