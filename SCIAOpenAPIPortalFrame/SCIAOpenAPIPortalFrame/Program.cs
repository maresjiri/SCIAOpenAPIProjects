using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SCIA.OpenAPI;
using SCIA.OpenAPI.StructureModelDefinition;
using SCIAOpenAPIPortalFrame.Examples;
using Environment = SCIA.OpenAPI.Environment;


namespace SCIAOpenAPIPortalFrame
{
    internal class Program
    {
        private static string SenInstallationPath = "C:\\Program Files\\SCIA\\Engineer24.0\\";
        private static string SenTempFolder = "C:\\Users\\maresjiri\\ESA24.0\\Temp\\ADMsync\\";
        private static string SenEmptyProject = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\EsaProjects\\EmptyProject24.0.esa";

        static void Main(string[] args)
        {
            var isInitialized = Tools.InitializeApplication(SenInstallationPath);

            if (!isInitialized)
            {
                return;
            }

            bool createCustomModel = ExamplesMenu.Show(SenInstallationPath, SenTempFolder, SenEmptyProject);

            if (createCustomModel)
            {
                CreateCustomModel();
            }



            Console.WriteLine();
            Console.WriteLine("Finished.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }

        private static void CreateModelUsingOpenApi(Structure model)
        {
            /*
             * Use the model variable to create objects in SCIA Engineer using the OpenAPI
             * e.g. model.CreateMaterial(new Material(Guid.NewGuid(), "S235", 1, "S 235"));
             */
        }

        private static void CreateCustomModel()
        {
            (Environment Environment, EsaProject Project) senData = Tools.StartSciaEngineer(SenInstallationPath, SenTempFolder, SenEmptyProject);
            CreateModelUsingOpenApi(senData.Project.Model);

            senData.Project.Model.RefreshModel_ToSCIAEngineer();
            senData.Project.CloseProject(SaveMode.SaveChangesNo);

            senData.Environment.Dispose();



        }
    }
}
