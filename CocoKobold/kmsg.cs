using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoKobold
{

    public enum MessageLevel
    {
        ERROR,
        WARNING,
        INFO,
    }

    internal class kmsg
    {
        public static void message(string tag,string message, MessageLevel level = MessageLevel.INFO)
        {
            var cc = Console.ForegroundColor;
            Console.Write(DateTime.Now.ToString("[dd.MM.yy|HH:mm] "));
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"{tag} - ");
            if (level == MessageLevel.ERROR)
                Console.ForegroundColor = ConsoleColor.DarkRed;
            else if (level == MessageLevel.WARNING)
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            else
                Console.ForegroundColor = cc;
            Console.WriteLine(message);
            Console.ForegroundColor = cc;
        }
    }
}
