using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SCIA.OpenAPI;
using Environment = SCIA.OpenAPI.Environment;

namespace SCIAOpenAPIPortalFrame
{
    public static class Tools
    {
        public static bool InitializeApplication(string installationPath)
        {
            bool accepted = Tools.AcceptedDisclaimer();

            if (!accepted)
            {
                return false;
            }

            Console.Clear();

            LoadOpenApiDll(installationPath);
            ConfigureSciaOpenApiAssemblyResolve(installationPath);
            KillAllOrphanSCIAEngineerIntances();

            return true;
        }

        #region Application initialisation

        /// <summary>
        /// Shows a disclaimer to the user which they have to accept before being able to run the rest of the example code
        /// </summary>
        /// <returns></returns>
        private static bool AcceptedDisclaimer()
        {
            Console.WriteLine("==================");
            Console.WriteLine("=== DISCLAIMER ===");
            Console.WriteLine("==================");
            Console.WriteLine("The MIT License");
            Console.WriteLine("Copyright © 2022 SCIA nv");
            Console.WriteLine();
            Console.WriteLine("Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:");
            Console.WriteLine();
            Console.WriteLine("The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.");
            Console.WriteLine();
            Console.WriteLine("THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.");
            Console.WriteLine("==================");
            Console.WriteLine();
            Console.WriteLine("!!! Continuing with this application WILL CLOSE ALL ACTIVE INSTANCES of SCIA Engineer !!!");
            Console.WriteLine("!!!          Please make sure you have saved all your work before proceeding          !!!");
            Console.WriteLine();
            Console.WriteLine("==================");
            Console.Write("Do you accept this and wish to continue? (Y/N): ");
            var response = Console.ReadLine().ToLowerInvariant();

            return response == "y";
        }

        /// <summary>
        /// Method to kill existing running instances of Scia Engineer
        /// </summary>
        private static void KillAllOrphanSCIAEngineerIntances()
        {
            foreach (var process in Process.GetProcessesByName("EsaStartupScreen"))
            {
                process.Kill();
                Console.WriteLine($"Killed old EsaStartupScreen instance!");
                System.Threading.Thread.Sleep(1000);
            }
            foreach (var process in Process.GetProcessesByName("Esa"))
            {
                process.Kill();
                Console.WriteLine($"Killed old SCIA Engineer instance!");
                System.Threading.Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// Explicitly load the OpenAPI dll from SCIA's instllation folder and not from the OpenAPI_dll subfolder, otherwise nothing will work
        /// </summary>
        /// <param name="installationPath">The instllation path of SCIA Engineer</param>
        private static void LoadOpenApiDll(string installationPath)
        {
            AssemblyName openApi = AssemblyName.GetAssemblyName($"{installationPath}\\SCIA.OpenAPI.dll");
            AppDomain.CurrentDomain.Load(openApi);
        }



        /// <summary>
        /// Hook into the AssemblyResolve event to help our application load the assemblies it needs in order to work with the OpenAPI.
        /// For SEN 19.0 to SEN 21.1, it will look inside the installation path and for some assemblies to the OpenAPI_dll subfolder.
        /// Starting with SEN 22, all assemblies we need are only located inside the OpenAPI_dll subfolder.
        /// </summary>
        /// <param name="senPath">The installation path of Scia Engineer</param>
        private static void ConfigureSciaOpenApiAssemblyResolve(string senPath)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                // Check if the assembly requested is already loaded in the current AppDomain
                Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == args.Name);

                if (loadedAssembly != null)
                {
                    return loadedAssembly;
                }

                string dllName = args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";

                if (dllName.EndsWith(".resources.dll"))
                {
                    return null; // Resource dlls can be ignored, as they only contain translations for strings and we haven't created any
                }

                string searchFolder = senPath;


                searchFolder = Path.Combine(searchFolder, "OpenAPI_dll");


                string dllFullPath = Path.Combine(searchFolder, dllName);

                if (!File.Exists(dllFullPath))
                {
                    Console.WriteLine($"WARNING: {dllName} not found in {searchFolder}");
                    return null;
                }

                return Assembly.LoadFrom(dllFullPath);
            };
        }

        #endregion

        public static (Environment Environment, EsaProject Project) StartSciaEngineer(string senPath, string tempPath, string esaProjectPath)
        {
            Console.WriteLine($"Starting {senPath}esa.exe");

            var openApiEnvironment = new SCIA.OpenAPI.Environment(senPath, tempPath, "1.0.0.0");

            bool isSciaEngineerRunning = openApiEnvironment.RunSCIAEngineer(SCIA.OpenAPI.Environment.GuiMode.ShowWindowShow);

            if (!isSciaEngineerRunning)
            {
                throw new ApplicationException("Failed to start instance of SCIA Engineer");
            }

            Console.WriteLine($"Loading {esaProjectPath}");
            EsaProject project = openApiEnvironment.OpenProject(esaProjectPath);

            Console.WriteLine("SCIA Engineer is loaded and ready to go!");

            return (openApiEnvironment, project);
        }

    }
}
