using StreamJsonRpc;
using System;
using System.Diagnostics;
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
        private JsonRpc remote;
        private string address;
        private int port;
        private X509Certificate2 certificate;

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
        public async Task ConnectAsync(string address, int port)
        {
            try
            {
                await TcpConnectToServerAsync(address, port);
                ConnectRpcServer();
            }
            catch
            {
                if (remote == null)
                {
                    // Retry after 3000ms
                    await Task.Delay(3000);
                    ConnectAsync(address, port);
                }
            }
        }

        /// <summary>
        /// Create a TcpClient and connect to server.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private async Task TcpConnectToServerAsync(string address, int port)
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }
            else
            {
                client = new TcpClient();

                try
                {
                    // Create a TCP/IP client socket.
                    await client.ConnectAsync(address, port);
                }
                catch (Exception e)
                {
                    Trace.WriteLine($"No TCP connection was made to the server: {e.Message}");
                    throw;
                }
            }

            Trace.WriteLine("Client connected.");
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
            return new X509Certificate2(serverCertificate).Thumbprint == certificate.Thumbprint;
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
                Trace.WriteLine($"Exception: {e.Message}");
                if (e.InnerException != null)
                {
                    Trace.WriteLine($"Inner exception: {e.InnerException.Message}");
                }
                Trace.WriteLine("Authentication failed - closing the connection.");
                client.Close();
                throw;
            }
            Trace.WriteLine("SslStream Established.");
            return sslStream;
        }

        private async Task ConnectRpcServer()
        {
            if (client != null)
            {
                try
                {
                    using (var stream = WrapSslStreamAsClient(client))
                    {
                        var messageFormatter = new JsonMessageFormatter(Encoding.UTF8);
                        var messageHandler = new LengthHeaderMessageHandler(stream, stream, messageFormatter);
                        remote = new JsonRpc(messageHandler);
                        remote.StartListening();
                        await remote.Completion;
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine($"Failed to create JsonRpc instance: {e.Message}");
                    throw;
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
                else
                {
                    await ConnectAsync(address, port);
                    if (remote == null)
                    {
                        throw new SocketException();
                    }
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
    }
}
