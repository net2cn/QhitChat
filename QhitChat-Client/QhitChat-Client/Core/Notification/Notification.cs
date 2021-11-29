using QhitChat_Client.Presistent.Database.Models;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace QhitChat_Client.Core.Notification
{
    public class Notification
    {
        public ObservableCollection<string> Contacts = new ObservableCollection<string>();
        Dictionary<string, ObservableCollection<Presistent.Database.Models.Messages>> Messages = new Dictionary<string, ObservableCollection<Presistent.Database.Models.Messages>>();

        [JsonRpcMethod("Notification/NewMessage")]
        public void NewMessage(Presistent.Database.Models.Messages message)
        {
            if (!HasQuene(message.From))
            {
                Contacts.Add(message.From);
            }
            ObservableCollection<Presistent.Database.Models.Messages> quene = GetQuene(message.From);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => quene.Add(message)));
            Presistent.Presistent.DatabaseContext.Messages.Add(message);
            Presistent.Presistent.DatabaseContext.SaveChanges();
            Contacts.Remove(message.From);
        }

        [JsonRpcMethod("Notification/Ping")]
        public void Ping(string data)
        {
            Trace.WriteLine(data);
        }

        public void AddQuene(string account)
        {
            ObservableCollection<Presistent.Database.Models.Messages> quene;
            if (!Messages.TryGetValue(account, out quene))
            {
                Messages.Add(account, new ObservableCollection<Presistent.Database.Models.Messages>());
            }
        }

        public bool HasQuene(string account)
        {
            if (Messages.ContainsKey(account))
            {
                return true;
            }
            return false;
        }

        public ObservableCollection<Messages> GetQuene(string account)
        {
            ObservableCollection<Presistent.Database.Models.Messages> quene;
            if (Messages.TryGetValue(account, out quene))
            {
                return quene;
            }

            AddQuene(account);
            return Messages[account];
        }
    }
}
