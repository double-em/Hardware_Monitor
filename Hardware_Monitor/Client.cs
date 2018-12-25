using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hardware_Monitor
{
    public class Client : ConnectionHandler
    {
        bool connectionLive;
        string packetRecieved = "";
        int samePacket = 0;

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
            while (connectionLive)
            {
                string consoleInput = "> ";
                Stopwatch timer = new Stopwatch();
                timer.Start();
                do
                {
                    while (!Console.KeyAvailable && connectionLive)
                    {
                        if (timer.Elapsed.Seconds > 0)
                        {
                            try
                            {
                                UpdateStats();
                                if (samePacket >= 3)
                                {
                                    Console.CursorTop += 1;
                                    Console.CursorLeft = 0;
                                    Console.WriteLine("Recieved the same packet: " + samePacket + " times!");
                                }
                            }
                            catch (SocketException)
                            {
                                Console.CursorTop += 1;
                                Console.CursorLeft = 0;
                                Console.WriteLine("Connection lost...");
                                connectionLive = false;
                                Thread.Sleep(5000);
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

                    if (connectionLive)
                    {
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
                    }
                    
                } while (connectionLive);                
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

            if (packetRecieved == Encoding.ASCII.GetString(data))
            {
                samePacket++;
            }
            else samePacket = 0;
            packetRecieved = Encoding.ASCII.GetString(data);
            string[] stats = packetRecieved.Split(',');

            //On some connecton closes, it will still go through to this? That's why there is a if - statement.
            if (stats.Length == 3)
            {
                Time = stats[0];
                CpuUsage = int.Parse(stats[1]);
                RamAvailable = int.Parse(stats[2]);
            }
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
                    connectionLive = true;
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