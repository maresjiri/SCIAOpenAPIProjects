using SCIAOpenAPIPortalFrame.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFrame
{
    internal class Program
    {
        static void Main(string[] args)
        {

            double SPAN = 12.0;
            double HEIGHT = 4.0;
            double E = 200000000000.0;
            double I = 0.0000000000001;
            double A = 0.0000000000001;


            // Corrected the declaration and usage of ExamplesMenu
            string senPath = "path_to_sen";
            string senTempPath = "path_to_sen_temp";
            string emptyProjectPath = "path_to_empty_project";

            SCIAOpenAPIPortalFrame.Examples.CreateModelWithOpenApiExample xx = new CreateModelWithOpenApiExample(senPath, senTempPath, emptyProjectPath);
            xx.Run();
            //Console.WriteLine($"ExamplesMenu.Show returned: {result}");



        }
    }
}
