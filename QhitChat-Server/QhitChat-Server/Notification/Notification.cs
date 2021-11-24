using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace QhitChat_Server.Notification
{
    class Notification
    {
        public JsonRpc Remote { get; set; }

        public Notification(JsonRpc remote)
        {
            Remote = remote;
        }

        public async void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                ObservableCollection<Presistent.Database.Models.Messages> quene = (ObservableCollection<Presistent.Database.Models.Messages>)sender;
                if (quene.Count > 0)
                {
                    var message = quene[0];
                    await Remote.NotifyAsync("Notification/NewMessage", message);
                    var dbMessage = (from m in Presistent.Presistent.DatabaseContext.Messages
                                    where m.Id == message.Id && m.IsSent!=-1
                                    select m).SingleOrDefault();
                    if (dbMessage!=null)
                    {
                        dbMessage.IsSent = -1;
                        Presistent.Presistent.DatabaseContext.SaveChanges();
                    }

                    quene.Remove(message);
                }
            }
        }
    }
}
