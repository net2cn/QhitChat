using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Nerdbank.Streams;
using System.Net;
using System.Net.Sockets;
using StreamJsonRpc;
using QhitChat_Server;
using System.Security.Cryptography.X509Certificates;
using QhitChat_Server.Presistent;

class Program
{
    public static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
    }

    static async Task<int> MainAsync(string[] args)
    {
        var port = 23340;
        IPAddress localAddress = IPAddress.Parse("127.0.0.1");

        // Initialize presistent infrastructure.
        _ = Presistent.Instance;

        Console.WriteLine($"Start lisening {localAddress}:{port}.");
        var server = new TcpListener(localAddress, port);
        server.Start();
        while (true)
        {
            try
            {
                var client = await server.AcceptTcpClientAsync().ConfigureAwait(false);
                var worker = new Worker(client, true);
                await Task.Run(worker.WorkAsync);
            }
            catch (Exception e)
            {
                Console.Error.WriteLineAsync(e.Message);
                Console.Error.WriteLineAsync(e.StackTrace);
            }
        }

        return 0;
    }
}