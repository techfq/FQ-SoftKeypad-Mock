using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Forms;

public class TcpClientListener
{
    private TcpClient client;
    private StreamReader reader;
    private StreamWriter writer;
    private Dispatcher uiDispatcher;

    public event Action<string> MessageReceived;

    public TcpClientListener(Dispatcher dispatcher)
    {
        client = new TcpClient();
        uiDispatcher = dispatcher;
    }

    public bool Connect(string serverIp, int serverPort)
    {
        try
        {
            client.Connect(IPAddress.Parse(serverIp), serverPort);
            NetworkStream stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);

            // Start listening for incoming messages in a separate thread
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error connecting to the server: " + ex.Message);
            return false;
        }
    }

    public void Disconnect()
    {
        try
        {
            if (client != null)
            {
                client.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error disconnecting from the server: " + ex.Message);
        }
    }


    public void Send(string message)
    {
        try
        {
            if (writer != null)
            {
                writer.Write(message);
                writer.Flush();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error sending message: " + ex.Message);
        }
    }

    private void ReceiveMessages()
    {
        try
        {
            char[] buffer = new char[1024]; // Adjust the buffer size as needed

            while (true)
            {
                int bytesRead = reader.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }

                string message = new string(buffer, 0, bytesRead);
                Debug.WriteLine(message);

                // Marshal the UI update to the main thread
                uiDispatcher.Invoke(() => MessageReceived?.Invoke(message));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error receiving message: " + ex.Message);
        }
        finally
        {
            // Clean up resources when the connection is closed
            reader.Close();
            writer.Close();
            client.Close();
        }
    }

}