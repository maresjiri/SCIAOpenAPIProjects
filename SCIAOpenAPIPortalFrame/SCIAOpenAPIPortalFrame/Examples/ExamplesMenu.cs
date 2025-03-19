using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCIAOpenAPIPortalFrame.Examples
{
    public static class ExamplesMenu
    {
        public static bool Show(string senPath, string senTempPath, string emptyProjectPath)
        {
            Console.WriteLine("Please choose one of the following:");
            Console.WriteLine("S.) Create example model in SCIA Engineer using OpenAPI");
            Console.WriteLine("C.) Create my own defined model from code in SCIA Engineer");
            Console.Write("");

            string choice = ConsoleHelper.Interact<string>("Your choice: ").ToUpperInvariant();

            Console.Clear();
            switch (choice)
            {
                case "S":
                    {
                        var example = new CreateModelWithOpenApiExample(senPath, senTempPath, emptyProjectPath);
                        example.Run();
                        return false;
                    }

                case "C":
                default:
                    {
                        return true;
                    }
                
            }
        }
    }
}
