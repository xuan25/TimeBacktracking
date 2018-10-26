using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimeBacktracking
{
    class Resync
    {
        string result = "";
        public Resync()
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();

            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c \"w32tm -resync\"";

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;

            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.EnableRaisingEvents = true;

            p.OutputDataReceived += P_OutputDataReceived;
            p.ErrorDataReceived += P_ErrorDataReceived;
            p.Exited += P_Exited;

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
        }

        private void P_Exited(object sender, EventArgs e)
        {
            Console.WriteLine("Exited");
            MessageBox.Show(result.Trim(), "Resync Finnished");
        }

        private void P_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
            result += e.Data + "\n";
        }

        private void P_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
            result += e.Data + "\n";
        }
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            stopBtn.IsEnabled = false;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMiliseconds;
        }

        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime sysTime);

        public static bool SetLocalTimeByStr(DateTime dt)
        {
            bool flag = false;
            SystemTime sysTime = new SystemTime();
            sysTime.wYear = Convert.ToUInt16(dt.Year);
            sysTime.wMonth = Convert.ToUInt16(dt.Month);
            sysTime.wDay = Convert.ToUInt16(dt.Day);
            sysTime.wHour = Convert.ToUInt16(dt.Hour);
            sysTime.wMinute = Convert.ToUInt16(dt.Minute);
            sysTime.wSecond = Convert.ToUInt16(dt.Second);
            try
            {
                flag = SetLocalTime(ref sysTime);
            }
            catch (Exception e)
            {
                Console.WriteLine("SetSystemDateTime execution exception: " + e.Message);
            }
            return flag;
        }

        Thread thread;
        bool running = false;
        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            startBtn.IsEnabled = false;
            syncBtn.IsEnabled = false;
            running = true;
            thread = new Thread(delegate ()
            {
                DateTime dt = DateTime.Now;
                DateTime rt = DateTime.Now;
                while (running)
                {
                    dt = new DateTime(dt.Ticks - 10000000);
                    Console.WriteLine(dt + " " + SetLocalTimeByStr(dt));
                    SetLocalTimeByStr(dt);
                    Thread.Sleep(1000);
                }
            });
            thread.Start();
            stopBtn.IsEnabled = true;
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            stopBtn.IsEnabled = false;
            running = false;
            while (thread.ThreadState == ThreadState.Stopped) ;
            thread.Abort();
            thread.Join();
            startBtn.IsEnabled = true;
            syncBtn.IsEnabled = true;
        }

        string result;
        private void syncBtn_Click(object sender, RoutedEventArgs e)
        {
            new Resync();
        }

        private void P_Exited(object sender, EventArgs e)
        {
            Console.WriteLine("Exited");
            MessageBox.Show(result.Trim());
        }

        private void P_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
            result += e.Data + "\n";
        }

        private void P_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
            result += e.Data + "\n";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            stopBtn_Click(null, null);
        }
    }
}
