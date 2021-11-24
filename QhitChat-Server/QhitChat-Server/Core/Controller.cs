using System;
using System.Collections.Generic;
using System.Linq;
using QhitChat_Server.Presistent;
using System.Text;
using System.Threading.Tasks;
using NSec.Cryptography;
using StreamJsonRpc;
using QhitChat_Server.API;
using System.Reflection;
using Microsoft.VisualBasic.CompilerServices;

namespace QhitChat_Server.Core
{
    class Controller
    {
        public Dictionary<Type, object> API = new Dictionary<Type, object>();
        JsonRpc Remote;

        public Controller(JsonRpc remote)
        {
            Remote = remote;

            foreach(var member in GetTypesFromNamespace(Assembly.GetExecutingAssembly(), "QhitChat_Server.API"))
            {
                API.Add(member, Activator.CreateInstance(member));
                remote.AddLocalRpcTarget(API[member]);
            }
            ((API.Authentication)API[typeof(API.Authentication)]).Remote = Remote;
        }

        public string Ping()
        {
            return "Pong";
        }

        public async Task<string> PingPong()
        {
            string s = await Remote.InvokeAsync<string>("ping_pong");
            Console.Error.WriteLineAsync(s);
            return "Pong from server";
        }

        public static IEnumerable<Type> GetTypesFromNamespace(Assembly assembly, string desiredNamepace)
        {
            return assembly.GetTypes()
                           .Where(type => type.Namespace == desiredNamepace);
        }

        public void OnDisconnected()
        {
            // Log user out if disconnected.
            var authentication = (API.Authentication)(API[typeof(API.Authentication)]);
            authentication.Logout(authentication.Account, authentication.Token);

            ((IDisposable)this).Dispose();
        }
    }
}
