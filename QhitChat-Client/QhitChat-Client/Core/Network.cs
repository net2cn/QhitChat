using StreamJsonRpc;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace QhitChat_Client.Core
{
    public class Network
    {
        private TcpClient client;
        private JsonRpc _remote;
        private string address;
        private int port;
        private X509Certificate2 certificate;

        public event EventHandler<NetworkEventArgs> RaiseNetworkEvent;

        public bool Connected
        {
            get => remote != null;
        }

        private JsonRpc remote
        {
            get { return _remote; }
            set
            {
                _remote = value;
                if (_remote == null)
                {
                    OnRaiseJsonRpcDisconnected(new NetworkEventArgs("Unable to establish JSON-RPC connection."));
                }
                else
                {
                    OnRaiseJsonRpcConnected(new NetworkEventArgs("JSON-RPC connection established successfully."));
                }
            }
        }

        public Network(string address, int port, X509Certificate certificate)
        {
            this.address = address;
            this.port = port;
            this.certificate = new X509Certificate2(certificate);
        }

        /// <summary>
        /// Connect to server with JsonRpc protocol. TLS is used as a secured layer for communication underlying RPC protocol.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(string address, int port)
        {
            while (true)
            {
                if (client == null || !client.Connected)
                {
                    Trace.WriteLine("Connecting...");
                    await TcpConnectToServerAsync(address, port);
                }
                else
                {
                    if (remote == null)
                    {
                        ConnectRpcServerAsync();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Create a TcpClient and connect to server.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private async Task TcpConnectToServerAsync(string address, int port)
        {
            while (true)
            {
                if(client == null)
                {
                    // Create a TCP/IP client socket.
                    client = new TcpClient();
                }

                if (!client.Connected)
                {
                    try
                    {
                        await client.ConnectAsync(address, port);
                        Trace.WriteLine("Client connected.");
                        break;
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine($"No TCP connection was made to the server: {e.Message}");
                        await Task.Delay(3000); // Retry after 3000ms;
                        if (client != null && !client.Connected)    // Prevent connected client from being disposed.
                        {
                            client.Close();
                            (client as IDisposable).Dispose();
                            client = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validate server public certificate by checking server certificate is matched with local certificate by comparing certificate thumbprint.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="serverCertificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool ValidateServerCertificate(
              object sender,
              X509Certificate serverCertificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            // Compare if remote public certificate the same as local certificate.
            var serverCertificate2 = new X509Certificate2(serverCertificate);
            return serverCertificate2.Thumbprint == certificate.Thumbprint && serverCertificate2.NotBefore < DateTime.Now && serverCertificate2.NotAfter > DateTime.Now;
        }

        /// <summary>
        /// Create a SslStream for secured communication between server and client. Stream is authenticated as client.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private SslStream WrapSslStreamAsClient(TcpClient client)
        {
            // Create an SSL stream that will close the client's stream.
            SslStream sslStream = new SslStream(
                    client.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null);
            try
            {
                // The server name must match the name on the server certificate.
                X509CertificateCollection certs = new X509CertificateCollection();
                certs.Add(certificate);
                sslStream.AuthenticateAsClient(
                    certificate.Subject,
                    null,
                    SslProtocols.Tls12,
                    false);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Unable to perform TLS handshake: {e.Message}");
                if (e.InnerException != null)
                {
                    Trace.WriteLine($"With inner exception: {e.InnerException.Message}");
                }
                throw;
            }
            Trace.WriteLine("SslStream Established.");
            return sslStream;
        }

        private GZipStream WrapGZipStream(Stream stream, CompressionMode compressionMode)
        {
            return new GZipStream(stream, compressionMode, false);
        }

        private async Task ConnectRpcServerAsync()
        {
            try
            {
                using (var sslStream = WrapSslStreamAsClient(client))
                {
                    MessagePackFormatter messageFormatter = new MessagePackFormatter();
                    var messageHandler = new LengthHeaderMessageHandler(sslStream, sslStream, messageFormatter);
                    remote = new JsonRpc(messageHandler);
                    remote.StartListening();
                    await remote.Completion;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Unable to create JSON-RPC instance: {e.Message}");
            }
            finally
            {
                if (remote != null)
                {
                    (remote as IDisposable).Dispose();
                    remote = null;
                }
                    
            }
        }



#nullable enable
        public async Task<T> InvokeAsync<T>(string targetName, params object?[]? arguments)
        {
            try
            {
                if (remote != null)
                {
                    return await remote.InvokeAsync<T>(targetName, arguments);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Cannot invoke remote API: {e.Message}");
            }
            return default;
        }
#nullable disable

        protected virtual async void OnRaiseJsonRpcDisconnected(NetworkEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<NetworkEventArgs> raiseEvent = RaiseNetworkEvent;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                raiseEvent(this, e);    // Call to raise the event.
                await Task.Delay(3000);
                await ConnectAsync(address, port);
            }
        }

        protected virtual void OnRaiseJsonRpcConnected(NetworkEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<NetworkEventArgs> raiseEvent = RaiseNetworkEvent;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                raiseEvent(this, e);    // Call to raise the event.
            }
        }
    }

    public class NetworkEventArgs : EventArgs
    {
        public NetworkEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
