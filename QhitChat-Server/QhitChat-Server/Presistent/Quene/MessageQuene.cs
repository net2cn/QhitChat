using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace QhitChat_Server.Presistent.Quene
{
    class MessageQuene
    {
        public static void CreateQuene(string account, JsonRpc remote)
        {
            ObservableCollection<Database.Models.Messages> accountQuene;
            if (!Presistent.MessageQuene.TryGetValue(account, out accountQuene))
            {
                var quene = new ObservableCollection<Database.Models.Messages>();
                Presistent.MessageQuene.Add(account, quene);
                var notification = new Notification.Notification(remote);
                quene.CollectionChanged += notification.OnCollectionChanged;
            }
        }

        public static void DeleteQuene(string account)
        {
            ObservableCollection<Database.Models.Messages> accountQuene;
            if (Presistent.MessageQuene.TryGetValue(account, out accountQuene))
            {
                accountQuene.Clear();
                Presistent.MessageQuene.Remove(account);
                ((IDisposable)accountQuene).Dispose();
            }
        }

        public static bool Enquene(string account, Database.Models.Messages message)
        {
            ObservableCollection<Database.Models.Messages> accountQuene;
            if(Presistent.MessageQuene.TryGetValue(account, out accountQuene))
            {
                accountQuene.Add(message);
                return true;
            }
            return false;
        }
    }
}
