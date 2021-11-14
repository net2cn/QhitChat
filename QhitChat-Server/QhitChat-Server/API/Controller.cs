using System;
using System.Collections.Generic;
using System.Linq;
using QhitChat_Server.Presistent;
using System.Text;
using System.Threading.Tasks;
using NSec.Cryptography;
using StreamJsonRpc;

namespace QhitChat_Server.API
{
    class Controller
    {
        JsonRpc _client;

        public Controller(JsonRpc client)
        {
            _client = client;
            
            // Register API here.
            client.AddLocalRpcTarget(new Authentication());
            client.AddLocalRpcTarget(new Relationship());
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

        public void Test()
        {
            Console.Error.WriteLineAsync("Recieved.");
        }
    }
}
