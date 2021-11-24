using StreamJsonRpc;
using System.Collections.Generic;
using System.Linq;

namespace QhitChat_Server.API
{
    class Chat
    {
        [JsonRpcMethod("Chat/Send")]
        public bool Send(string from, string token, string to, string content)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == @from
                        select u).SingleOrDefault();
            if (user != null && user.Token == token)
            {
                var relationship = (from r in Presistent.Presistent.DatabaseContext.Relationship
                               where r.From == @from && r.To==to
                               select r).SingleOrDefault();

                if (relationship != null)
                {
                    var toUser = (from u in Presistent.Presistent.DatabaseContext.User
                                where u.Account == to
                                select u).SingleOrDefault();

                    var message = (Presistent.Database.Models.Messages)relationship;
                    message.Content = content;
                    message.CreatedOn = System.DateTime.UtcNow;
                    
                    if (toUser.Status == 1)
                    {
                        // TODO: deliver message to live client.
                        // message.IsSent = -1; // Indicating that this message is delivered to the peer.
                        message.IsSent = 0; // Indicating that this message is not delivered to peer yet.
                    }
                    else
                    {
                        message.IsSent = 0; // Indicating that this message is not delivered to peer yet.
                    }

                    Presistent.Presistent.DatabaseContext.Messages.Add(message);
                    Presistent.Presistent.DatabaseContext.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }

        [JsonRpcMethod("Chat/Fetch")]
        public List<Presistent.Database.Models.Messages> Fetch(string to, string token)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == to
                        select u).SingleOrDefault();
            if (user != null && user.Token == token)
            {
                var messages = (from m in Presistent.Presistent.DatabaseContext.Messages
                                    where m.To == to && m.IsSent != -1
                                    select m).ToList();

                if (messages.Count > 0)
                {
                    foreach (var i in messages)
                    {
                        i.IsSent = -1;
                    }
                    Presistent.Presistent.DatabaseContext.SaveChangesAsync();

                    return messages;
                }
            }

            return null;
        }
    }
}
