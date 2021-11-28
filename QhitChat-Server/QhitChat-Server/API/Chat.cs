using Microsoft.EntityFrameworkCore;
using StreamJsonRpc;
using System.Collections.Generic;
using System.Linq;

namespace QhitChat_Server.API
{
    class Chat
    {
        [JsonRpcMethod("Chat/Send")]
        public bool Send(string fromUser, string token, string to, string content)
        {
            var user = (from u in Presistent.Presistent.DatabaseContext.User
                        where u.Account == fromUser
                        select u).SingleOrDefault();
            if (user != null && user.Token == token)
            {
                var relationship = (from r in Presistent.Presistent.DatabaseContext.Relationship
                                   where r.From == fromUser && r.To == to
                                   select r).SingleOrDefault();

                if (relationship != null)
                {
                    var message = new Presistent.Database.Models.Messages();
                    message.From = fromUser;
                    message.To = to;
                    message.Content = content;
                    message.CreatedOn = System.DateTime.UtcNow;
                    message.IsSent = 0; // Indicating that this message is not delivered to peer yet.

                    Presistent.Quene.MessageQuene.Enquene(to, message);
                    Presistent.Presistent.DatabaseContext.Entry(relationship).State = EntityState.Detached;
                    Presistent.Presistent.DatabaseContext.Entry(message).State = EntityState.Modified;
                    Presistent.Presistent.DatabaseContext.Messages.Add(message);
                    Presistent.Presistent.DatabaseContext.SaveChanges();
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
                        i.IsSent = -1;  // Mark messages as sent to prevent messages from being delivered multiple times.
                    }
                    Presistent.Presistent.DatabaseContext.SaveChanges();
                }
                return messages;
            }

            return null;
        }
    }
}
