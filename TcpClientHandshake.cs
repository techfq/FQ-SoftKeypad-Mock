using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
public class TcpClientWrapper
{
    private TcpClient _tcpClient;
    private Thread _listenThread;
    private bool _isConnected;
    public event Action<string> DataReceived;

    public TcpClientWrapper()
    {
        _tcpClient = new TcpClient();
        _isConnected = false;
    }

    public bool Connect(string ipAddress, int port = 1000)
    {
        try
        {
            _tcpClient.Connect(ipAddress, port);
            _isConnected = true;
            StartListening();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error connecting to server: " + ex.Message);
            return false;
        }
    }

    private void StartListening()
    {
        _listenThread = new Thread(Listen);
        _listenThread.Start();
    }

    private void Listen()
    {
        while (_isConnected)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = _tcpClient.GetStream().Read(buffer, 0, buffer.Length);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                DataReceived?.Invoke(data); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error receiving data: " + ex.Message);
                //Disconnect();
            }
        }
    }

    public void Send(string data)
    {
        if (_isConnected)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            _tcpClient.GetStream().Write(buffer, 0, buffer.Length);
        }
        else
        {
            Console.WriteLine("Not connected to server.");
        }
    }

    public void Disconnect()
    {
        _isConnected = false;
        _tcpClient.Close();

        // Wait for the listen thread to finish
        if (_listenThread != null && _listenThread.IsAlive)
        {
            _listenThread.Join();
        }
    }

}
