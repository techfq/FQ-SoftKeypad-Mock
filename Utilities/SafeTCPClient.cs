using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Calculator_WPF.Utilities
{
    public class SafeTCPClient
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private CancellationTokenSource cancellationTokenSource;

        public event Action<string> MessageReceived;

        public SafeTCPClient()
        {
            tcpClient = new TcpClient();
            cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task ConnectAsync(string ipAddress, int port)
        {
            try
            {
                tcpClient.Connect(ipAddress, port);
                networkStream = tcpClient.GetStream();
                StartReceiving();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting: {ex.Message}");
                Reconnect();
            }
        }

        public bool Stop()
        {
            cancellationTokenSource.Cancel();
            tcpClient.Close();
            return true;
        }

        private async void StartReceiving()
        {
            try
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token);

                    if (bytesRead > 0)
                    {
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        OnMessageReceived(receivedData);
                    }
                    else
                    {
                        Reconnect();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving: {ex.Message}");
                Reconnect();
            }
        }

        private async void Reconnect()
        {
            Console.WriteLine("Reconnecting...");
            Stop();

            // Add some delay before attempting to reconnect
            await Task.Delay(3000);

            // Attempt to reconnect
            await ConnectAsync("your_server_ip", 1234);
        }

        public async Task SendAsync(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await networkStream.WriteAsync(data, 0, data.Length, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending: {ex.Message}");
                Reconnect();
            }
        }

        protected virtual void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(message);
        }
    }
}
