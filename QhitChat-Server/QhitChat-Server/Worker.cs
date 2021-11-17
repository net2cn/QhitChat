using System;
using System.IO;
using System.Net.Sockets;
using StreamJsonRpc;
using System.Threading.Tasks;
using System.Text;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.IO.Compression;
using Nerdbank.Streams;

namespace QhitChat_Server
{
    internal class Worker
    {
        TcpClient _client;
        bool _ownsClient;
        int _id;

        public Worker(TcpClient client, bool ownsClient)
        {
            _client = client;
            _ownsClient = ownsClient;
            _id = _client.Client.RemoteEndPoint.GetHashCode();
        }

        public async Task WorkAsync()
        {
            try
            {
                using (var sslStream = WrapSslStreamAsServer(_client))
                {
                    await RespondToRpcRequestsAsync(sslStream, sslStream);
                }
            }
            finally
            {
                if (_ownsClient && _client != null)
                {
                    (_client as IDisposable).Dispose();
                    _client = null;
                }
            }
        }

        private SslStream WrapSslStreamAsServer(TcpClient client)
        {
            // A client has connected. Create the
            // SslStream using the client's network stream.
            Console.Error.WriteLineAsync($"Client {_id} connected. Start TLS handshake.");
            SslStream sslStream = new SslStream(
                client.GetStream(), false);
            // Authenticate the server but don't require the client to authenticate.
            try
            {
                sslStream.AuthenticateAsServer(Configuration.Certificate, clientCertificateRequired: false, checkCertificateRevocation: true);

                // Display the properties and settings for the authenticated stream.
                //DisplaySecurityLevel(sslStream);
                //DisplaySecurityServices(sslStream);
                //DisplayCertificateInformation(sslStream);
                //DisplayStreamProperties(sslStream);
            }
            catch (AuthenticationException e)
            {
                Console.Error.WriteLineAsync($"Exception: {e.Message}");
                if (e.InnerException != null)
                {
                    Console.Error.WriteLineAsync($"Inner exception: {e.InnerException.Message}");
                }
                Console.Error.WriteLineAsync($"Authentication failed - closing the connection.");

                // The client stream will be closed with the sslStream
                // because we specified this behavior when creating
                // the sslStream.
                sslStream.Close();
                client.Close();
                throw;
            }
            return sslStream;
        }

        private GZipStream WrapGZipStream(Stream stream, CompressionMode compressionMode)
        {
            return new GZipStream(stream, compressionMode, true);
        }

        private void DisplaySecurityLevel(SslStream stream)
        {
            Console.Error.WriteLineAsync($"Cipher: { stream.CipherAlgorithm} strength {stream.CipherStrength}");
            Console.Error.WriteLineAsync($"Hash: {stream.HashAlgorithm} strength {stream.HashStrength}");
            Console.Error.WriteLineAsync($"Key exchange: {stream.KeyExchangeAlgorithm} strength {stream.KeyExchangeStrength}");
            Console.Error.WriteLineAsync($"Protocol: {stream.SslProtocol}");
        }
        private void DisplaySecurityServices(SslStream stream)
        {
            Console.Error.WriteLineAsync($"Is authenticated: {stream.IsAuthenticated} as server? {stream.IsServer}");
            Console.Error.WriteLineAsync($"IsSigned: {stream.IsSigned}");
            Console.Error.WriteLineAsync($"Is Encrypted: {stream.IsEncrypted}");
        }
        private void DisplayStreamProperties(SslStream stream)
        {
            Console.Error.WriteLineAsync($"Can read: {stream.CanRead}, write {stream.CanWrite}");
            Console.Error.WriteLineAsync($"Can timeout: {stream.CanTimeout}");
        }

        private void DisplayCertificateInformation(SslStream stream)
        {
            Console.Error.WriteLineAsync($"Certificate revocation list checked: {stream.CheckCertRevocationStatus}");

            X509Certificate localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.Error.WriteLineAsync($"Local cert was issued to {localCertificate.Subject} and is valid from {localCertificate.GetEffectiveDateString()} until {localCertificate.GetExpirationDateString()}.");
            }
            else
            {
                Console.Error.WriteLineAsync("Local certificate is null.");
            }
            // Display the properties of the client's certificate.
            X509Certificate remoteCertificate = stream.RemoteCertificate;
            if (stream.RemoteCertificate != null)
            {
                Console.Error.WriteLineAsync($"Remote cert was issued to {remoteCertificate.Subject} and is valid from {remoteCertificate.GetEffectiveDateString()} until {remoteCertificate.GetExpirationDateString()}.");
            }
            else
            {
                Console.Error.WriteLineAsync("Remote certificate is null.");
            }
        }

        private async Task RespondToRpcRequestsAsync(Stream sendingStream, Stream receivingStream)
        {
            await Console.Error.WriteLineAsync($"Connection request #{_id} received. Spinning off an async Task to cater to requests.");
            MessagePackFormatter messageFormatter = new MessagePackFormatter();
            var messageHandler = new LengthHeaderMessageHandler(sendingStream, receivingStream, messageFormatter);
            Core.Controller controller = null;

            try
            {
                using (var jsonRpc = new JsonRpc(messageHandler))
                {
                    controller = new Core.Controller(jsonRpc);
                    jsonRpc.AddLocalRpcTarget(controller);
                    Console.Error.WriteLineAsync($"JSON-RPC listener attached to #{_id}. Waiting for requests...");
                    jsonRpc.StartListening();
                    await jsonRpc.Completion;
                    Console.Error.WriteLineAsync($"Connection #{_id} terminated normally.");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLineAsync($"Connection #{_id} terminated: {e.Message}");
            }
            finally
            {
                controller.OnDisconnected();
            }
        }
    }

    public class CompressJsonMessageFormatter : JsonMessageFormatter
    {

    }
}
