using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hardware_Monitor
{
    public class Menu
    {
        string cs = "";

        public Menu()
        {
            Console.Title = "Hardware Monitor";
            ShowMenu();
        }

        private void ShowMenu()
        {
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("Hardware Monitor - Setup\n");
                Console.WriteLine("\t1. Client");
                Console.WriteLine("\t2. Server");
                Console.WriteLine("\n\t0. Exit");
                Console.Write("\nOption: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        exit = true;
                        break;

                    case "1":
                        cs = "Client";
                        Setup();
                        break;

                    case "2":
                        cs = "Server";
                        Setup();
                        break;

                    default:
                        Console.WriteLine("Dette er ikke en valgmulighed...");
                        break;
                }
            }
        }

        private void Setup()
        {
            Console.Clear();
            Console.Write("IPADDRESS (XXX.XXX.XXX.XXX): ");
            string ipaddressString = Console.ReadLine();
            bool ipConverted = IPAddress.TryParse(ipaddressString, out IPAddress ipaddress);

            Console.Write("PORT (XXXX): ");
            string portString = Console.ReadLine();
            bool portConverted = int.TryParse(portString, out int port);

            if (ipConverted && portConverted)
            {
                if (cs == "Client")
                {
                    Client client = new Client(ipaddress, port);
                }
                else
                {
                    Server server = new Server(ipaddress, port);
                }
            }
            else
            {
                Console.WriteLine("IPADDRESS or PORT was not entered correct.");
            }
        }
    }
}
