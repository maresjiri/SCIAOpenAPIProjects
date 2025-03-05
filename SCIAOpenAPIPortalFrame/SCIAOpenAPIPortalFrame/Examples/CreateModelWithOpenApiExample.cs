using System;
using SCIA.OpenAPI;
using SCIA.OpenAPI.StructureModelDefinition;
using Environment = SCIA.OpenAPI.Environment;

namespace SCIAOpenAPIPortalFrame.Examples
{
    public class CreateModelWithOpenApiExample
    {
        private readonly string senPath;
        private readonly string senTempPath;
        private readonly string emptyProjectPath;

        public CreateModelWithOpenApiExample(string senPath, string senTempPath, string emptyProjectPath)
        {
            this.senPath = senPath;
            this.senTempPath = senTempPath;
            this.emptyProjectPath = emptyProjectPath;
        }

        public void Run()
        {
            (Environment Environment, EsaProject Project) senData = Tools.StartSciaEngineer(senPath, senTempPath, emptyProjectPath);

            if (!CreateModelUsingOpenApi(senData.Project.Model))
            {
                Console.WriteLine("ERROR: Failed to create model");
                senData.Environment.Dispose();
                return;
            }

            senData.Project.Model.RefreshModel_ToSCIAEngineer();
            senData.Project.CloseProject(SaveMode.SaveChangesNo);
            senData.Environment.Dispose();
        }

        private bool CreateModelUsingOpenApi(Structure model)
        {
            var material = new Material(Guid.NewGuid(), "S235", 1, "S 235");

            bool result = model.CreateMaterial(material);

            if (!result)
            {
                Console.WriteLine("ERROR: Failed to create material.");
                return false;
            }

            var css = new CrossSectionManufactured(Guid.NewGuid(), "CS1", material.Id, "HEA200", 1, 1);

            result = model.CreateCrossSection(css);

            if (!result)
            {
                Console.WriteLine("ERROR: Failed to create cross-section");
                return false;
            }

            var node1 = new StructNode(Guid.NewGuid(), "Node1", 0, 0, 0);
            var node2 = new StructNode(Guid.NewGuid(), "Node2", 4, 0, 0);

            result = model.CreateNode(node1);

            if (!result)
            {
                Console.WriteLine("ERROR: Failed to create node1");
                return false;
            }

            result = model.CreateNode(node2);

            if (!result)
            {
                Console.WriteLine("ERROR: Failed to create node2");
                return false;
            }

            var beam = new Beam(Guid.NewGuid(), "Beam1", css.Id, new Guid[2] { node1.Id, node2.Id });

            result = model.CreateBeam(beam);

            if (!result)
            {
                Console.WriteLine("ERROR: Failed to create beam");
                return false;
            }

            return true;
        }
    }
}
