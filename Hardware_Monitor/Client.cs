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
    public class Client : ConnectionHandler
    {
        public Client(IPAddress ipaddress, int port) : base(ipaddress, port)
        {
            Console.Title = "Hardware Monitor - Client";
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
                            try
                            {
                                UpdateStats();
                            }
                            catch (SocketException)
                            {
                                Console.CursorTop += 1;
                                Console.CursorLeft = 0;
                                Console.WriteLine("Connection lost...");
                            }
                            finally
                            {
                                timer.Restart();
                            }
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
            ConnectionSocket.Send(buffer);

            byte[] recievedBuffer = new byte[1024];
            int received = ConnectionSocket.Receive(recievedBuffer);
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
            while (!ConnectionSocket.Connected)
            {
                try
                {
                    attempts++;
                    ConnectionSocket.Connect(ServerAddress, Port);
                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Failed, Connection attempts: " + attempts);
                }
            }

            Console.Clear();
            Console.WriteLine("Connected to " + ServerAddress + ":" + Port);
        }
    }
}