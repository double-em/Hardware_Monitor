using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Hardware_Monitor
{
    static public class Utility
    {
        public static string EmptyString(int length)
        {
            string final = "";
            for (int i = 0; i < length; i++)
            {
                final += " ";
            }
            return final;
        }

        public static void Cleanup()
        {
            Console.WriteLine(EmptyString(Console.BufferWidth));
            Console.WriteLine(EmptyString(Console.BufferWidth));
            Console.CursorTop = Console.CursorTop - 4;
        }

        public static string Time()
        {
            return DateTime.Now.ToString().Substring(11);
        }
    }
}
