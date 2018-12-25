using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Hardware_Monitor
{
    public abstract class ConnectionHandler
    {
        protected Socket ConnectionSocket;
        protected readonly IPAddress ServerAddress;
        protected readonly int Port;

        protected int CpuUsage { get; set; }
        protected int RamAvailable { get; set; }
        protected string Time { get; set; }

        [DllImport("Kernel32")]
        static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        delegate bool EventHandler(CtrlType sig);
        private static EventHandler _handler;

        public ConnectionHandler(IPAddress ipaddress, int port)
        {
            ConnectionSocket = new Socket
                (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            ServerAddress = ipaddress;
            this.Port = port;

            _handler += Handler;
            SetConsoleCtrlHandler(_handler, true);
        }

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_CLOSE_EVENT:
                    ConnectionSocket.Shutdown(SocketShutdown.Both);
                    ConnectionSocket.Close();
                    return true;

                case CtrlType.CTRL_LOGOFF_EVENT:
                    ConnectionSocket.Shutdown(SocketShutdown.Both);
                    ConnectionSocket.Close();
                    return true;

                case CtrlType.CTRL_SHUTDOWN_EVENT:
                    ConnectionSocket.Shutdown(SocketShutdown.Both);
                    ConnectionSocket.Close();
                    return true;

                default:
                    return false;
            }
        }

    }
}
