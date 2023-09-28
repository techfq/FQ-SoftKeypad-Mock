using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Calculator_WPF;
using static Calculator_WPF.MainWindow;
using System.Windows.Threading;

namespace Calculator_WPF
{
    internal class CommandHandling
    {

        private TcpClient mainTcpClient;
        public CommandHandling(TcpClient client)
        {
            mainTcpClient = client;
        }
        public void HandleEnter(APPSTATE state)
        {
            try
            {
                switch (state)
                {
                    case APPSTATE.LOGIN:
                        // mainTcpClient.Send("HELLO");
                        break;

                    default:
                        Debug.WriteLine("Enter Handling !!!");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
