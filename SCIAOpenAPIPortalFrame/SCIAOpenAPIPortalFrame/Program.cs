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

            var material = new Material(Guid.NewGuid(), "S235", 1, "S 235");

            bool result = model.CreateMaterial(material);

            if (!result)
            {
                Console.WriteLine("ERROR: Failed to create material.");
            }

            var css = new CrossSectionManufactured(Guid.NewGuid(), "CS1", material.Id, "HEA200", 1, 1);

            result = model.CreateCrossSection(css);

            if (!result)
            {
                Console.WriteLine("ERROR: Failed to create cross-section");
            }

            var node1 = new StructNode(Guid.NewGuid(), "Node1", 0, 0, 0);
            var node2 = new StructNode(Guid.NewGuid(), "Node2", 0, 0, 5);
            var node3 = new StructNode(Guid.NewGuid(), "Node3", 6, 0, 6);
            var node4 = new StructNode(Guid.NewGuid(), "Node4", 12, 0, 5);
            var node5 = new StructNode(Guid.NewGuid(), "Node5", 12, 0, 0);

            // add nodes in to a list
            List<StructNode> nodes = new List<StructNode>();



            result = model.CreateNode(node1);
            if (!result)
            {
                Console.WriteLine("ERROR: Failed to create node1");
            }

            result = model.CreateNode(node2);
            if (!result)
            {
                Console.WriteLine("ERROR: Failed to create node2");
            }





            var beam = new Beam(Guid.NewGuid(), "Beam1", css.Id, new Guid[2] { node1.Id, node2.Id });

            result = model.CreateBeam(beam);

            if (!result)
            {
                Console.WriteLine("ERROR: Failed to create beam");
            }


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
