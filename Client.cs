using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace TCPClient
{
    public class Client
    {
        #region private members 	
        private Thread clientReceiveThread;
        private TcpClient socketConnection;
        public delegate void ConnectFailMessage(string msg);

        public delegate void ConnectSuccessMessage(string msg);

        public delegate void ReceivedMessage(string msg);
        public event ConnectFailMessage OnConnectedFail;

        public event ConnectSuccessMessage OnConnectedSuccess;

        public event ReceivedMessage OnReceivedMessage;
        #endregion
        // Use this for initialization 	
        private string _serverIP;
        private int _serverPort;
        CancellationTokenSource src = new CancellationTokenSource();

        public void Cancel()
        {
            src.Cancel();
            src = new CancellationTokenSource();
        }

        /// <summary> 	
        /// Send message to server using socket connection. 	
        /// </summary> 	
        public void SendMessage(string sectionName, string Message)
        {
            if (socketConnection == null || !socketConnection.Connected)
            {
                return;
            }

            try
            { 			
                NetworkStream stream = socketConnection.GetStream();
                if (stream.CanWrite)
                {
                    string ID = sectionName.Substring(6);
                    string clientMessage = string.Format("{0},0,127,{1},0,", ID, ID); // 1,0,127,1,0,18
                    string CRCValue = CalculateCRC(clientMessage);
                    clientMessage += CRCValue;
                    byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                    stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                }
            }
            catch (SocketException socketException)
            {
                System.Diagnostics.Debug.WriteLine(socketException.Message);
                CloseConnectAndReconnect();
            }
            catch (InvalidOperationException invalidOperationException)
            {
                System.Diagnostics.Debug.WriteLine(invalidOperationException.Message);
                CloseConnectAndReconnect();
            }
        }
        public void SendMessageCRC(string Message)
        {
            if (socketConnection==null || !socketConnection.Connected)
            {
                return;
            }

            try
            {
                NetworkStream stream = socketConnection.GetStream();
                if (stream.CanWrite)
                {
                    string CRCValue = CalculateCRC(Message);
                    Message += CRCValue;
                    byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(Message);
                    stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                }
            }
            catch (SocketException socketException)
            {
                System.Diagnostics.Debug.WriteLine(socketException.Message);
                CloseConnectAndReconnect();
            }
            catch (InvalidOperationException invalidOperationException)
            {
                System.Diagnostics.Debug.WriteLine(invalidOperationException.Message);
                CloseConnectAndReconnect();
            }
        }public void SendString(string Message)
        {
            if (socketConnection==null || !socketConnection.Connected)
            {
                return;
            }

            try
            {
                NetworkStream stream = socketConnection.GetStream();
                if (stream.CanWrite)
                {
                    //string CRCValue = CalculateCRC(Message);
                    //Message += CRCValue;
                    byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(Message);
                    stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                }
            }
            catch (SocketException socketException)
            {
                System.Diagnostics.Debug.WriteLine(socketException.Message);
                CloseConnectAndReconnect();
            }
            catch (InvalidOperationException invalidOperationException)
            {
                System.Diagnostics.Debug.WriteLine(invalidOperationException.Message);
                CloseConnectAndReconnect();
            }
        }

        private string CalculateCRC(string textString)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(textString);
            byte crc = 0;

            for (int i = 0; i < textBytes.Length; i++)
            {
                crc ^= textBytes[i];
            }

            string crcString = crc.ToString("X2");
            return crcString;
        }



        public void Start(string serverIP, int serverPort)
        {
            _serverIP = serverIP;
            _serverPort = serverPort;
            ConnectToTcpServer();
        }

        private void CloseConnectAndReconnect()
        {
            if (socketConnection.Connected)
            {
                socketConnection.Close();
                src.Cancel();
            }
            src = new CancellationTokenSource();
            ConnectToTcpServer();
        }

        public void CloseConnection()
        { 
                if (socketConnection!=null && socketConnection.Connected)
                {
                    socketConnection.Close();
                    src.Cancel();
                }
        }

        /// <summary> 	
        /// Setup socket connection. 	
        /// </summary> 	
        private void ConnectToTcpServer()
        {
            try
            {
                clientReceiveThread = new Thread(new ParameterizedThreadStart(ListenForData));
                clientReceiveThread.IsBackground = true;
                clientReceiveThread.Start(src.Token);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
        /// <summary> 	
        /// Runs in background clientReceiveThread; Listens for incomming data. 	
        /// </summary>     
        private void ListenForData(object token)
        {
            var cancellationToken = (CancellationToken)token;

            int attempts = 0;

            while (true && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    socketConnection = new TcpClient(_serverIP, _serverPort);

                    if (OnConnectedSuccess != null)
                        OnConnectedSuccess("CONNECTED");

                    Byte[] bytes = new Byte[1024];
                    while (true && !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            // Get a stream object for reading 				
                            using (NetworkStream stream = socketConnection.GetStream())
                            {
                                int length;

                                // Read incomming stream into byte arrary. 		

                                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                                {
                                    var incommingData = new byte[length];
                                    Array.Copy(bytes, 0, incommingData, 0, length);
                                    // Convert byte array to string message. 						
                                    string serverMessage = Encoding.ASCII.GetString(incommingData);
                                    if (OnReceivedMessage != null)
                                        OnReceivedMessage(serverMessage);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            if (OnConnectedFail != null)
                            {
                                OnConnectedFail($"Failed to connect to server. " + ex.Message);
                            }
                            attempts = 0;
                            break;
                        }
                    }
                }
                catch (SocketException socketException)
                {
                    //if (attempts < 10)
                    //{
                    //    attempts++;
                    //    if (OnConnectedFail != null)
                    //    {
                    //        OnConnectedFail($"Failed to connect to server. Retrying in 3 seconds... (Attempt {attempts})");

                    //        System.Threading.Thread.Sleep(3000);
                    //    }

                    //}
                    //else
                    //{
                    //    if (OnConnectedFail != null)
                    //        OnConnectedFail("Failed to connect to server after 5 attempts. Exiting...");
                    //    break;
                    //}

                    if (OnConnectedFail != null)
                    {
                        OnConnectedFail($"Failed to connect to server. Retrying in 3 seconds... (Attempt {attempts})");

                        System.Threading.Thread.Sleep(3000);
                    }

                    System.Diagnostics.Debug.WriteLine(socketException.Message);
                }

            }
        }
    }
}
