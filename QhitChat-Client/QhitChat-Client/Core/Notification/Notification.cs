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
        Dictionary<string, ObservableCollection<Presistent.Database.Models.Messages>> Messages = new Dictionary<string, ObservableCollection<Presistent.Database.Models.Messages>>();

        [JsonRpcMethod("Notification/NewMessage")]
        public void NewMessage(Presistent.Database.Models.Messages message)
        {
            ObservableCollection<Presistent.Database.Models.Messages> quene;
            if (Messages.TryGetValue(message.From, out quene))
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => quene.Add(message)));
            }
            Presistent.Presistent.DatabaseContext.Messages.Add(message);
            Presistent.Presistent.DatabaseContext.SaveChanges();
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
