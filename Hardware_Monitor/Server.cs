using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Hardware_Monitor
{
    public class Server : ConnectionHandler
    {
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;

        private byte[] _buffer = new byte[1024];
        static List<Socket> _clientSockets = new List<Socket>();


        public Server(IPAddress ipaddress, int port) : base(ipaddress, port)
        {
            Console.Title = "Hardware Monitor - Server";
            SetupServer();
        }

        private void SetupServer()
        {
            Console.WriteLine("\nSetting up server...\n");

            Console.Write("Setting up Hardware Monitor...");
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            Console.WriteLine("Done!");

            Console.Write("Setting up Network Communication...");
            ConnectionSocket.Bind(new IPEndPoint(ServerAddress, Port));
            ConnectionSocket.Listen(1);
            ConnectionSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            Console.WriteLine("Done!");

            Console.WriteLine("\nReady!\n");

            string consoleInput = "> ";
            Stopwatch timer = new Stopwatch();
            timer.Start();
            do
            {
                while (!Console.KeyAvailable)
                {
                    if (timer.Elapsed.Seconds > 0)
                    {
                        Time = Utility.Time();
                        CpuUsage = (int)Math.Round(cpuCounter.NextValue(), 0);
                        RamAvailable = (int)ramCounter.NextValue();
                        timer.Restart();
                    }

                    Console.SetCursorPosition(0, Console.CursorTop + 2);
                    Console.Write(Utility.EmptyString(Console.BufferWidth - 1));

                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("Server time: " + Time);
                    string writeLine = "[" + CpuUsage + "% / " + RamAvailable + " RAM]";
                    Console.CursorLeft = Console.BufferWidth - (writeLine.Length + 1);

                    Console.Write(writeLine);
                    Utility.Cleanup();
                    Console.SetCursorPosition(0, Console.CursorTop - 2);

                    Console.CursorLeft = 0;

                    if (consoleInput.Length < Console.BufferWidth)
                    {
                        Console.Write(consoleInput + Utility.EmptyString((Console.BufferWidth - 1) - consoleInput.Length));
                        Console.CursorLeft = consoleInput.Length;
                    }
                    System.Threading.Thread.Sleep(50);
                }
                string key = Console.ReadKey(true).Key.ToString();
                switch (key)
                {
                    case "Backspace":
                        if (consoleInput.Length > 2)
                        {
                            consoleInput = consoleInput.Remove(consoleInput.Length - 1);
                        }
                        break;

                    case "Spacebar":
                        consoleInput += " ";
                        break;

                    case "Enter":
                        Console.CursorTop += 1;
                        Console.CursorLeft = 0;
                        Console.WriteLine("Command: '" + consoleInput.Remove(0, 2) + "' not found");
                        consoleInput = "> ";
                        break;

                    default:
                        consoleInput += key.ToLower();
                        break;
                }
            } while (true);

        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket socket = ConnectionSocket.EndAccept(ar);
            _clientSockets.Add(socket);

            Console.CursorTop += 1;
            Console.CursorLeft = 0;
            Console.WriteLine("Client Connected: " + socket.RemoteEndPoint);

            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            ConnectionSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                int received = 0;
                received = socket.EndReceive(ar);

                byte[] dataBuff = new byte[received];
                Array.Copy(_buffer, dataBuff, received);

                string text = Encoding.ASCII.GetString(dataBuff);

                string response = string.Empty;
                if (text.ToLower() != "get stats")
                {
                    response = "Invalid Request";
                }
                else
                {
                    response = Time + "," + CpuUsage + "," + RamAvailable;
                }

                byte[] data = Encoding.ASCII.GetBytes(response);
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback),
                    socket);
            }
            catch (SocketException)
            {
                Console.CursorTop += 1;
                Console.CursorLeft = 0;
                Console.WriteLine("Client Disconnected: " + socket.RemoteEndPoint);
            }

        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndSend(ar);
        }
    }
}
