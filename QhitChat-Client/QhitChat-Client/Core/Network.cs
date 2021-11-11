using StreamJsonRpc;
using System;
using System.Diagnostics;
using System.IO;
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
        private JsonRpc remote;
        private string address;
        private int port;
        private X509Certificate2 certificate;
        private JsonMessageFormatter messageFormatter = new JsonMessageFormatter(Encoding.UTF8);

        public Network(string address, int port, X509Certificate certificate)
        {
            this.address = address;
            this.port = port;
            this.certificate = new X509Certificate2(certificate);
        }

        public async Task ConnectAsync(string address, int port)
        {
            try
            {
                var client = await ConnectToServerAsync(address, port);
                ConnectRpcServer(client);
            }
            catch{ }
        }

        private async Task<TcpClient> ConnectToServerAsync(string address, int port)
        {
            TcpClient client = new TcpClient();
            try
            {
                // Create a TCP/IP client socket.
                await client.ConnectAsync(address, port);
            }
            catch(Exception ex)
            {
                Trace.WriteLine($"No TCP connection was made to the server: {ex.Message}");
                throw;
            }
            Trace.WriteLine("Client connected.");
            return client;
        }

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        private bool ValidateServerCertificate(
              object sender,
              X509Certificate serverCertificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            // Compare if remote public certificate the same as local certificate.
            return new X509Certificate2(serverCertificate).Thumbprint == certificate.Thumbprint;
        }

        private SslStream WrapSslStreamAsClient(TcpClient client)
        {
            // Create an SSL stream that will close the client's stream.
            SslStream sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                null
                );
            // The server name must match the name on the server certificate.
            try
            {
                X509CertificateCollection certs = new X509CertificateCollection();
                certs.Add(certificate);
                sslStream.AuthenticateAsClient(
                    certificate.Subject,
                    null,
                    SslProtocols.Tls12,
                    false);
            }
            catch (AuthenticationException e)
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

        private async Task ConnectRpcServer(TcpClient client)
        {
            try
            {
                using (var stream = WrapSslStreamAsClient(client))
                {
                    var messageHandler = new LengthHeaderMessageHandler(stream, stream, messageFormatter);
                    remote = new JsonRpc(messageHandler);
                    remote.StartListening();
                    await remote.Completion;
                }
            }
            catch
            {
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

        public async Task<T> InvokeAsync<T>(string targetName, params object?[]? arguments)
        {
            if (remote!=null)
            {
                return await remote.InvokeAsync<T>(targetName, arguments);
            }
            else
            {
                await ConnectAsync(address, port);
                return await remote.InvokeAsync<T>(targetName, arguments);
            }
            return default(T);
        }
    }
}
