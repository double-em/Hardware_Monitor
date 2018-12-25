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
    public class Client
    {
        private int CpuUsage { get; set; }
        private int RamAvailable { get; set; }
        private string Time { get; set; }

        Socket _clientSocket;
        IPAddress serverAddress;
        int port;

        public Client(IPAddress ipaddress, int port)
        {
            Console.Title = "Hardware Monitor - Client";

            _clientSocket = new Socket
                (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverAddress = ipaddress;
            this.port = port;
            SetupClient();
        }

        private void SetupClient()
        {
            LoopConnect();
            SendLoop();
        }

        void SendLoop()
        {
            while (true)
            {
                string consoleInput = "> ";
                Stopwatch timer = new Stopwatch();
                timer.Start();
                do
                {
                    while (!Console.KeyAvailable)
                    {
                        if (timer.Elapsed.Seconds > 0)
                        {
                            UpdateStats();
                            
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
        }

        void UpdateStats()
        {
            byte[] buffer = Encoding.ASCII.GetBytes("get stats");
            _clientSocket.Send(buffer);

            byte[] recievedBuffer = new byte[1024];
            int received = _clientSocket.Receive(recievedBuffer);
            byte[] data = new byte[received];
            Array.Copy(recievedBuffer, data, received);
            string[] stats = Encoding.ASCII.GetString(data).Split(',');
            Time = stats[0];
            CpuUsage = int.Parse(stats[1]);
            RamAvailable = int.Parse(stats[2]);

        }

        void LoopConnect()
        {
            Console.WriteLine("Connecting...");
            int attempts = 0;
            while (!_clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _clientSocket.Connect(serverAddress, port);
                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Failed, Connection attempts: " + attempts);
                }
            }

            Console.Clear();
            Console.WriteLine("Connected to " + serverAddress + ":" + port);
        }
    }
}