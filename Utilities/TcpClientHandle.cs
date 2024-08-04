using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FQ_KPSoft.Utilities
{
    public class TcpClientHandle
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private CancellationTokenSource cancellationTokenSource;

        // Define the delegate and event
        public delegate void MessageReceivedEventHandler(object sender, string message);
        public event EventHandler<bool> ConnectionStatusChanged;
        public event MessageReceivedEventHandler MessageReceived;

        public TcpClientHandle()
        { }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                tcpClient = new TcpClient();
                tcpClient.Connect(ipAddress, port);
                networkStream = tcpClient.GetStream();
                Debug.WriteLine("Connected to the server");
                OnConnectionStatusChanged(true);
                // Start listening for incoming data in a separate thread
                Task.Run(() => ListenForData(cancellationTokenSource.Token));
                return true;
            }
            catch (Exception ex)
            {
                cancellationTokenSource.Cancel();
                Debug.WriteLine($"Error connecting to the server: {ex.Message}");
            }
            return false;
        }

        private async Task ListenForData(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (bytesRead > 0)
                    {
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        // Raise the event when data is received
                        OnMessageReceived(receivedData);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while listening for data: {ex.Message}");
                Disconnect();
            }
        }

        // Method to raise the MessageReceived event
        protected virtual void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        private void OnConnectionStatusChanged(bool isConnected)
        {
            ConnectionStatusChanged?.Invoke(this, isConnected);
        }

        public bool Disconnect()
        {
            try
            {
                if (tcpClient.Connected)
                {
                    cancellationTokenSource.Cancel(); // Stop the listener
                    networkStream.Close();
                    tcpClient.Close();
                    Debug.WriteLine("Disconnected from the server");
                }
                OnConnectionStatusChanged(false);
                return true;
            }
            catch (Exception ex)
            {
                Disconnect();
            }
            return false;
        }

        public void SendData(string data)
        {
            try
            {
                if (tcpClient.Connected)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(data);
                    networkStream.Write(buffer, 0, buffer.Length);
                    Debug.WriteLine("SEND: " + data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data: {ex.Message}");
                Disconnect();
            }
        }

        public bool IsValidChecksum(string data)
        {
            if (string.IsNullOrEmpty(data) || data.Length < 5)
            {
                return false;
            }
            string receivedChecksum = data.Substring(data.Length - 2);
            string calculatedChecksum = CalculateChecksum(data.Substring(0, data.Length - 2));
            Debug.WriteLine(calculatedChecksum);
            return receivedChecksum == calculatedChecksum;
        }

        public string CalculateChecksum(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input string cannot be null or empty.");
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte checksum = 0;
            foreach (byte b in bytes)
            {
                checksum ^= b;
            }
            string checksumHex = checksum.ToString("X2"); // X2 means uppercase hexadecimal with at least two digits
            return checksumHex;
        }


    }
}
