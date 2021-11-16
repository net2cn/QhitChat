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
        JsonRpc _client;

        public Controller(JsonRpc client)
        {
            _client = client;

            foreach(var member in GetTypesFromNamespace(Assembly.GetExecutingAssembly(), "QhitChat_Server.API"))
            {
                client.AddLocalRpcTarget(Activator.CreateInstance(member));
            }
        }

        public string Ping()
        {
            return "Pong";
        }

        public async Task<string> PingPong()
        {
            string s = await _client.InvokeAsync<string>("ping_pong");
            Console.Error.WriteLineAsync(s);
            return "Pong from server";
        }

        public static IEnumerable<Type> GetTypesFromNamespace(Assembly assembly,
                                                       string desiredNamepace)
        {
            return assembly.GetTypes()
                           .Where(type => type.Namespace == desiredNamepace);
        }
    }
}
