using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCIAOpenAPIPortalFrame
{
    public static class ConsoleHelper
    {
        public static T Interact<T>(string message, T defaultValue = default)
                where T : IConvertible
        {
            Console.Write(message);
            string input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(input, typeof(T));
        }
    }
}
