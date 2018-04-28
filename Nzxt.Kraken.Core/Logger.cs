using System;

namespace Nzxt.Kraken.Core
{
    public static class Logger
    {
        public static void Write(string message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }
    }
}
