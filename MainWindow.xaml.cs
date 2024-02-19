using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.Http;
using System.Windows.Media;
using System.Reflection.Metadata;
using static System.Windows.Forms.AxHost;
using System.Text;
using System.Windows.Interop;
using System.Threading;
using System.IO;
using System.Windows.Threading;
using Calculator_WPF.Utilities;
using Microsoft.VisualBasic.ApplicationServices;
using System.Threading.Tasks;

namespace Calculator_WPF
{
    public partial class MainWindow : Window
    {
        private string currentInput = "";
        private string currentOperator = "None";
        private int tellerID = 1;
        private int counterID = 1;
        private int currentNumber = 0;
        private string cmd_msg = "";
        private string configFileName = "config.ini";

        public enum APPSTATE
        {
            STANDBY,
            LOGIN,
            CALL_FREE,
            CALL_NUM,
            END_CALL,
            SHOW_WAIT,
            READY_RX_NUMBER
        }

        private APPSTATE runtimeState = APPSTATE.LOGIN;

        private string tcp_ip = "127.0.0.1";
        private int tcp_port = 5000;
        private IniFile iniFile;
        private bool isConnected = false;
        private bool autoConnect = false;

        private TcpClientHandle tcpClient = new TcpClientHandle();

        public MainWindow()
        {
            LicenseInput licenseInput = new LicenseInput();
            LicenseKey result = licenseInput.IsValid();

            if (!result.Status)
            {
                if (licenseInput.ShowDialog() != true)
                {
                    Application.Current.Shutdown();
                }
            }
            InitializeComponent();

            iniFile = new IniFile(configFileName);
            tcp_ip = iniFile.ReadValue(TCPIP.TCPNAME, TCPIP.ADDRESS, "127.0.0.1");
            tcp_port = int.Parse(iniFile.ReadValue(TCPIP.TCPNAME, TCPIP.PORT, "5000"));
            counterID = int.Parse(iniFile.ReadValue(USER.NAME, USER.COUNTERID, "1"));
            tellerID = int.Parse(iniFile.ReadValue(USER.NAME, USER.TELLERID, "1"));
            autoConnect = iniFile.ReadValue(USER.NAME, USER.AUTOCONNECT, "0") == "1" ? true : false;

            if (counterID == tellerID)
            {
                currentInput = $"{counterID.ToString("D2")}00";
                lb_display.Content = currentInput;
            }
            else
            {
                currentInput = $"{counterID.ToString("D2")}{tellerID.ToString("D2")}";
                lb_display.Content = currentInput;
            }

            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            double leftPosition = SystemParameters.PrimaryScreenWidth - Width - 20;
            Left = leftPosition;
            Top = 20;
            PreviewKeyUp += OnPreviewKeyDown;

            tcpClient.MessageReceived += OnMessageReceived;
            tcpClient.ConnectionStatusChanged += ConnectionStatusChanged;

            if (autoConnect)
            {
                Task.Run(() => ConnectAndListen(tcp_ip, tcp_port));
            }

            if (result.LsType != "888")
            {
                int dayDifference = 1;
                Debug.WriteLine(result.EndDate);
                
                if(result.EndDate == string.Empty) 
                { 
                    result.EndDate = DateTime.Now.AddDays(1).ToString();
                    lbLicenseDate.Content = $"Activated";
                }
                else
                {
                    try
                    {
                        dayDifference = (int)(DateTime.Parse(result.EndDate) - DateTime.Today).TotalDays;
                        lbLicenseDate.Content = $"{dayDifference} days";
                    }
                    catch 
                    {
                        File.Delete("license.lic");
                        Application.Current.Shutdown();
                    }
                }
                Debug.WriteLine(dayDifference);
                if (dayDifference < 1 || dayDifference > 70)
                {
                    File.Delete("license.lic");
                    Application.Current.Shutdown();
                }
            }
            else
            {
                lbLicenseDate.Content = $"Activated";
            }

        }

        private async Task ConnectAndListen(string serverIpAddress, int serverPort)
        {
            while (!isConnected)
            {
                isConnected = tcpClient.Connect(serverIpAddress, serverPort);

                if (!isConnected)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    Debug.WriteLine("Retry connection ...!");
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            Window_Closed();
            Application.Current.Shutdown();
        }

        private void ConnectionStatusChanged(object sender, bool isServerConnected)
        {
            if (isServerConnected)
            {
                Debug.WriteLine("Connection established.");
                isConnected = true;
                Dispatcher.Invoke(() =>
                {
                    btn_connect.Content = "Connected";
                    btn_connect.Background = new SolidColorBrush(Color.FromRgb(0x70, 0xD8, 0x80)); //#70D880
                });
            }
            else
            {
                isConnected = false;
                Dispatcher.Invoke(() =>
                {
                    btn_connect.Content = "Disconnect";
                    btn_connect.Background = new SolidColorBrush(Color.FromRgb(0xED, 0x32, 0x37)); // #ED3237
                });
                if (autoConnect)
                {
                    Task.Run(() => ConnectAndListen(tcp_ip, tcp_port));
                }
            }
        }

        private void OnMessageReceived(object sender, string rx_mgs)
        {
            Debug.WriteLine(rx_mgs);

            char STX = (char)2;
            char ETX = (char)3;
            if (rx_mgs[0] != STX && rx_mgs[^1] != ETX) return;

            rx_mgs = rx_mgs[1..^1]; // Remove STX, ETX
            if (!tcpClient.IsValidChecksum(rx_mgs)) return;
            string[] cmds = rx_mgs.Split(',');
            if (cmds.Length > 3)
            {
                if (int.Parse(cmds[1]) != counterID) return;
                if (cmds[2] == "103")
                {
                    int number = int.Parse(cmds[3]);
                    if (number > 0)
                    {
                        currentNumber = number;
                        Dispatcher.Invoke(() =>
                        {
                            lb_display.Content = number.ToString("D4");
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            lb_display.Content = "Hết khách";
                        });
                    }
                    if (runtimeState == APPSTATE.LOGIN)
                    {
                        iniFile.WriteValue(USER.NAME, USER.TELLERID, tellerID.ToString());
                        iniFile.WriteValue(USER.NAME, USER.COUNTERID, counterID.ToString());
                        iniFile.Save(configFileName);
                        tcpClient.SendData($"DEVTYPE01{counterID:D2}");
                    }
                    if (runtimeState != APPSTATE.CALL_NUM)
                    {
                        runtimeState = APPSTATE.CALL_NUM;
                        Dispatcher.Invoke(() =>
                        {
                            btn_enter.Content = "NEXT";
                        });
                    }
                }
            }
        }

        private void Window_Closed()
        {
            try
            {
                if (isConnected)
                {
                    tcpClient.Disconnect();
                    isConnected = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while closing the TCP client: {ex.Message}");
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                isConnected = tcpClient.Connect(tcp_ip, tcp_port);
            }
            else
            {
                if (tcpClient.Disconnect())
                {
                    isConnected = false;
                }
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Console.WriteLine(button.Content);
            currentInput += button.Content.ToString();
            lb_display.Content = currentInput;
        }

        private void OperationButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            currentOperator = "" + button.Content.ToString();
            switch (currentOperator)
            {
                case "BACK":
                    currentInput = RemoveLastChar(currentInput);
                    lb_display.Content = currentInput;
                    break;

                case "MENU":
                    if (runtimeState != APPSTATE.LOGIN)
                    {
                        runtimeState = APPSTATE.LOGIN;
                        if (counterID == tellerID)
                        {
                            currentInput = $"{counterID.ToString("D2")}00";
                            lb_display.Content = currentInput;
                        }
                        else
                        {
                            currentInput = $"{counterID.ToString("D2")}{tellerID.ToString("D2")}";
                            lb_display.Content = currentInput;
                        }
                        btn_enter.Content = "ENTER";
                    }
                    break;

                case "CALL":
                    HandleCall();
                    break;

                case "ENTER":
                    HandleEnter();
                    break;

                case "NEXT":
                    HandleNext();
                    break;
            }
            Thread.Sleep(20); // Add some delay incase user handle too fast
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            string keyText = e.Key.ToString();
            switch (keyText)
            {
                case "Back":
                    currentInput = RemoveLastChar(currentInput);
                    lb_display.Content = currentInput;
                    break;

                case "Subtract":
                    if (runtimeState != APPSTATE.LOGIN)
                    {
                        runtimeState = APPSTATE.LOGIN;
                        if (counterID == tellerID)
                        {
                            currentInput = $"{counterID.ToString("D2")}00";
                            lb_display.Content = currentInput;
                        }
                        else
                        {
                            currentInput = $"{counterID.ToString("D2")}{tellerID.ToString("D2")}";
                            lb_display.Content = currentInput;
                        }
                        btn_enter.Content = "ENTER";
                    }
                    break;

                case "Add":
                    HandleCall();
                    break;

                case "Return":
                    if (("" + btn_enter.Content.ToString() == "NEXT"))
                        HandleNext();
                    else
                        HandleEnter();
                    break;

                default:
                    if (!string.IsNullOrEmpty(keyText) && keyText.Length > 1)
                    {
                        if (keyText.StartsWith("D") || keyText.StartsWith("NumPad"))
                        {
                            string keyNum = keyText.Substring(keyText.Length - 1);
                            currentInput += keyNum;
                            lb_display.Content = currentInput;
                        }
                    }
                    break;
            }
            Thread.Sleep(20); // Add some delay incase user handle too fast
        }

        private static string RemoveLastChar(string content)
        {
            if (content.Length > 0)
            {
                content = content[0..^1]; // content = content.Substring(0, content.Length - 1);
            }
            return content;
        }

        private static string CalculateCRC(string textString)
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

        private void HandleEnter()
        {
            switch (runtimeState)
            {
                case APPSTATE.LOGIN:
                    Debug.WriteLine(currentInput + " " + currentInput.Length);
                    if (currentInput.Length != 4)
                        return;
                    counterID = int.Parse(currentInput[0..^2]); // [COUNTER ID][TELLER ID]
                    tellerID = int.Parse(currentInput[^2..]);
                    Debug.WriteLine(counterID + " " + tellerID);
                    if (counterID < 1 || counterID > 100)
                        return;
                    if (tellerID == 0)
                        tellerID = counterID;

                    cmd_msg = string.Format("{0},0,100,{1},0,", counterID, tellerID); // 1,0,100,1,0,1D
                    cmd_msg += CalculateCRC(cmd_msg);
                    tcpClient.SendData(cmd_msg);
                    break;

                default:
                    Debug.WriteLine("Enter Handling !!!");
                    break;
            }
        }

        private void HandleNext()
        {
            switch (runtimeState)
            {
                case APPSTATE.CALL_NUM:
                    cmd_msg = string.Format("{0},0,102,{1},0,", tellerID, currentNumber); // END NUMBER 1,0,102,1002,0,2D
                    cmd_msg += CalculateCRC(cmd_msg);
                    tcpClient.SendData(cmd_msg);
                    Thread.Sleep(50);
                    cmd_msg = string.Format("{0},0,107,0,0,", tellerID); // NEW_CALL 1,0,107,0,0,1B
                    cmd_msg += CalculateCRC(cmd_msg);
                    tcpClient.SendData(cmd_msg);
                    break;

                default:
                    Debug.WriteLine("Next Handling !!!");
                    break;
            }
        }

        private void HandleCall()
        {
            switch (runtimeState)
            {
                case APPSTATE.CALL_NUM:
                    string input_number = ("" + lb_display.Content.ToString()).Trim();
                    if (input_number.Length < 5)
                    {
                        cmd_msg = string.Format("{0},0,107,{1},0,", tellerID, input_number); // RE_CALL 1,0,107,0,0,1B
                        cmd_msg += CalculateCRC(cmd_msg);
                        tcpClient.SendData(cmd_msg);
                    }
                    break;
            }
        }
    }
}
