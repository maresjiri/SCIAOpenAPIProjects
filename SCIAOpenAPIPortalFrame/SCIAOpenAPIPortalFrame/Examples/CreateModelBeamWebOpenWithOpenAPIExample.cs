using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

using SCIA.OpenAPI;
using SCIA.OpenAPI.OpenAPIEnums;
using SCIA.OpenAPI.StructureModelDefinition;
using Environment = SCIA.OpenAPI.Environment;


using Results64Enums;



namespace SCIAOpenAPIPortalFrame.Examples
{
    internal class CreateModelBeamWebOpenWithOpenAPIExample
    {
        private readonly string senPath;
        private readonly string senTempPath;
        private readonly string emptyProjectPath;

        // constructor:
        public CreateModelBeamWebOpenWithOpenAPIExample(string senPath, string senTempPath, string emptyProjectPath)
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


        #region Class Enums
        enum Material_Type { Concrete = 0, Steel = 1, Timber = 2, Aluminium = 3, Masonry = 4, Other = 5 }
        enum Slab_Type { Plate = 0, Wall = 1, Shell = 2 }
        enum LoadCase_actionType { Permanent = 0, Variable = 1, Accidental = 2 }
        enum LoadCase_loadCaseType { SelfWeight = 0, Standard = 1, Prestress = 2, Dynamic = 3, PrimaryEffect = 4, Static = 5 }
        #endregion

        private static Guid Lc1Id { get; } = Guid.NewGuid();
        private static Guid C1Id { get; } = Guid.NewGuid();
        private static string SlabName { get; } = "S1";
        private static string beamName { get; } = "b1";


        private bool CreateModelUsingOpenApi(Structure model)
        {
            #region  ---------- Geometry -----------
            #region Create Materials
            Material concmat = new Material(Guid.NewGuid(), "conc", (int)Material_Type.Concrete, "C30/37");
            Material steelmat = new Material(Guid.NewGuid(), "steel", (int)Material_Type.Steel, "S 355");
            foreach (var x in new List<Material> { concmat, steelmat }) { model.CreateMaterial(x); }
            #endregion
            #region Create Cross Sections
            CrossSectionManufactured hea260 = new CrossSectionManufactured(Guid.NewGuid(), "steel.HEA", steelmat.Id, "HEA260", 1, 0);
            CrossSectionParametric rect300x300 = new CrossSectionParametric(Guid.NewGuid(), "r300x300", concmat.Id, 1, new double[] { 300.0, 300.0 });
            model.CreateCrossSection(hea260);
            model.CreateCrossSection(rect300x300);
            #endregion
            #region Create Nodes
            double a = 5.0;
            double b = 6.0;
            double c = 4.0;
            StructNode n1 = new StructNode(Guid.NewGuid(), "n1", 0, 0, 0);
            StructNode n2 = new StructNode(Guid.NewGuid(), "n2", a, 0, 0);
            StructNode n3 = new StructNode(Guid.NewGuid(), "n3", a, b, 0);
            StructNode n4 = new StructNode(Guid.NewGuid(), "n4", 0, b, 0);
            StructNode n5 = new StructNode(Guid.NewGuid(), "n5", 0, 0, c);
            StructNode n6 = new StructNode(Guid.NewGuid(), "n6", a, 0, c);
            StructNode n7 = new StructNode(Guid.NewGuid(), "n7", a, b, c);
            StructNode n8 = new StructNode(Guid.NewGuid(), "n8", 0, b, c);
            foreach (var x in new List<StructNode> { n1, n2, n3, n4, n5, n6, n7, n8 }) { model.CreateNode(x); }
            #endregion
            #region Create Beams
            Beam b1 = new Beam(Guid.NewGuid(), beamName, hea260.Id, new Guid[2] { n1.Id, n5.Id });
            Beam b2 = new Beam(Guid.NewGuid(), "b2", hea260.Id, new Guid[2] { n2.Id, n6.Id });
            Beam b3 = new Beam(Guid.NewGuid(), "b3", hea260.Id, new Guid[2] { n3.Id, n7.Id });
            Beam b4 = new Beam(Guid.NewGuid(), "b4", hea260.Id, new Guid[2] { n4.Id, n8.Id });
            foreach (var x in new List<Beam> { b1, b2, b3, b4 }) { model.CreateBeam(x); }
            #endregion
            #region Create Slab
            double thickness = 0.30;
            Slab s1 = new Slab(Guid.NewGuid(), SlabName, (int)Slab_Type.Plate, concmat.Id, thickness, new Guid[4] { n5.Id, n6.Id, n7.Id, n8.Id });
            model.CreateSlab(s1);
            #endregion
            #region Create Support - in Node
            PointSupport Su1 = new PointSupport(Guid.NewGuid(), "Su1", n1.Id) { ConstraintRx = eConstraintType.Free, ConstraintRy = eConstraintType.Free, ConstraintRz = eConstraintType.Free };
            PointSupport Su2 = new PointSupport(Guid.NewGuid(), "Su2", n2.Id) { ConstraintZ = eConstraintType.Flexible, StiffnessZ = 10000.0 };
            PointSupport Su3 = new PointSupport(Guid.NewGuid(), "Su3", n3.Id);
            PointSupport Su4 = new PointSupport(Guid.NewGuid(), "Su4", n4.Id);
            foreach (var x in new List<PointSupport> { Su1, Su2, Su3, Su4 }) { model.CreatePointSupport(x); }
            #endregion
            #region Create Support - on Beam & on Slab Edge
            LineSupport lineSupport_onBeam = new LineSupport(Guid.NewGuid(), "linSupBeam", b1.Id)
            {
                ConstraintRx = eConstraintType.Free,
                ConstraintRy = eConstraintType.Free,
                ConstraintRz = eConstraintType.Free,
                ConstraintX = eConstraintType.Flexible,
                StiffnessX = 10.0,
                ConstraintY = eConstraintType.Flexible,
                StiffnessY = 10.0,
                ConstraintZ = eConstraintType.Flexible,
                StiffnessZ = 10.0,
            };
            LineSupport lineSupport_onEdge = new LineSupport(Guid.NewGuid(), "linSupEdge", s1.Id)
            {
                ConstraintRx = eConstraintType.Free,
                ConstraintRy = eConstraintType.Free,
                ConstraintRz = eConstraintType.Free,
                ConstraintX = eConstraintType.Flexible,
                StiffnessX = 10.0,
                ConstraintY = eConstraintType.Flexible,
                StiffnessY = 10.0,
                ConstraintZ = eConstraintType.Flexible,
                StiffnessZ = 10.0,
                EdgeIndex = 2
            };
            foreach (var x in new List<LineSupport> { lineSupport_onBeam, lineSupport_onEdge }) { model.CreateLineSupport(x); }
            #endregion
            #endregion
            #region  ---------- Loads ---------------
            #region Create Load Group
            LoadGroup lgperm = new LoadGroup(Guid.NewGuid(), "lgperm", (int)eLoadGroup_Load.eLoadGroup_Load_Permanent);
            LoadGroup lgvar1 = new LoadGroup(Guid.NewGuid(), "lgvar1", (int)eLoadGroup_Load.eLoadGroup_Load_Variable);
            LoadGroup lgvar2 = new LoadGroup(Guid.NewGuid(), "lgvar2", (int)eLoadGroup_Load.eLoadGroup_Load_Variable);
            LoadGroup lgvar3 = new LoadGroup(Guid.NewGuid(), "lgvar3", (int)eLoadGroup_Load.eLoadGroup_Load_Variable);
            foreach (var x in new List<LoadGroup> { lgperm, lgvar1, lgvar2, lgvar3 }) { model.CreateLoadGroup(x); }
            #endregion
            #region Create Load Case
            LoadCase lc_sw = new LoadCase(Guid.NewGuid(), "lc_sw", (int)LoadCase_actionType.Permanent, lgperm.Id, (int)LoadCase_loadCaseType.SelfWeight);
            LoadCase lc_perm = new LoadCase(Lc1Id, "lc_perm", (int)LoadCase_actionType.Permanent, lgperm.Id, (int)LoadCase_loadCaseType.Standard);
            LoadCase lc_var1 = new LoadCase(Guid.NewGuid(), "lc_var1", (int)LoadCase_actionType.Variable, lgvar1.Id, (int)LoadCase_loadCaseType.Static);
            LoadCase lc_var2 = new LoadCase(Guid.NewGuid(), "lc_var2", (int)LoadCase_actionType.Variable, lgvar2.Id, (int)LoadCase_loadCaseType.Static);
            LoadCase lc_var3a = new LoadCase(Guid.NewGuid(), "lc_var3a", (int)LoadCase_actionType.Variable, lgvar3.Id, (int)LoadCase_loadCaseType.Static);
            LoadCase lc_var3b = new LoadCase(Guid.NewGuid(), "lc_var3b", (int)LoadCase_actionType.Variable, lgvar3.Id, (int)LoadCase_loadCaseType.Static);
            LoadCase lc_var3c = new LoadCase(Guid.NewGuid(), "lc_var3c", (int)LoadCase_actionType.Variable, lgvar3.Id, (int)LoadCase_loadCaseType.Static);
            foreach (var x in new List<LoadCase> { lc_sw, lc_perm }) { model.CreateLoadCase(x); }
            #endregion
            #region Create Load Combinations
            CombinationItem[] combinationItems = new CombinationItem[]
            {
                new CombinationItem(lc_sw.Id, 1.0), new CombinationItem(Lc1Id, 1.0),
               // new CombinationItem(lc_var1.Id, 1.0), new CombinationItem(lc_var2.Id, 1.0),
               // new CombinationItem(lc_var3a.Id, 1.0), new CombinationItem(lc_var3b.Id, 1.0), new CombinationItem(lc_var3c.Id, 1.0)
            };
            Combination C_EnUlsB = new Combination(C1Id, "C_EnUlsB", combinationItems)
            {
                Category = eLoadCaseCombinationCategory.AccordingNationalStandard,
                NationalStandard = eLoadCaseCombinationStandard.EnUlsSetB,
            };
            Combination C_EnUlsC = new Combination(Guid.NewGuid(), "C_EnUlsC", combinationItems)
            {
                Category = eLoadCaseCombinationCategory.AccordingNationalStandard,
                NationalStandard = eLoadCaseCombinationStandard.EnUlsSetC
            };
            Combination C_EnSlsChar = new Combination(Guid.NewGuid(), "C_EnSlsChar", combinationItems)
            {
                Category = eLoadCaseCombinationCategory.AccordingNationalStandard,
                NationalStandard = eLoadCaseCombinationStandard.EnSlsCharacteristic
            };
            Combination C_EnSlsFreq = new Combination(Guid.NewGuid(), "C_EnSlsFreq", combinationItems)
            {
                Category = eLoadCaseCombinationCategory.AccordingNationalStandard,
                NationalStandard = eLoadCaseCombinationStandard.EnSlsFrequent
            };
            Combination C_EnSlsQP = new Combination(Guid.NewGuid(), "C_EnSlsQP", combinationItems)
            {
                Category = eLoadCaseCombinationCategory.AccordingNationalStandard,
                NationalStandard = eLoadCaseCombinationStandard.EnSlsQuasiPermanent
            };
            Combination C_Acc1 = new Combination(Guid.NewGuid(), "C_Acc1", combinationItems)
            {
                //Category = eLoadCaseCombinationCategory.AccidentalLimitState,
                Category = eLoadCaseCombinationCategory.AccordingNationalStandard,
                NationalStandard = eLoadCaseCombinationStandard.EnAccidental1
            };
            Combination C_Acc2 = new Combination(Guid.NewGuid(), "C_Acc2", combinationItems)
            {
                //Category = eLoadCaseCombinationCategory.AccidentalLimitState,
                Category = eLoadCaseCombinationCategory.AccordingNationalStandard,
                NationalStandard = eLoadCaseCombinationStandard.EnAccidental2
            };
            Combination C_ULS = new Combination(Guid.NewGuid(), "C_ULS", combinationItems)
            {
                Category = eLoadCaseCombinationCategory.UltimateLimitState,
            };
            Combination C_SLS = new Combination(Guid.NewGuid(), "C_SLS", combinationItems)
            {
                Category = eLoadCaseCombinationCategory.ServiceabilityLimitState
            };
            foreach (var x in new List<Combination> { C_EnUlsB, C_EnUlsC, C_EnSlsChar, C_EnSlsFreq, C_EnSlsQP, C_Acc1, C_Acc2 }) { model.CreateCombination(x); }
            #endregion
            #region Create Load - Point Loads - in Node
            double loadValue;
            loadValue = -12500.0;
            PointLoadInNode pln1 = new PointLoadInNode(Guid.NewGuid(), "pln1", loadValue, lc_perm.Id, n4.Id, (int)eDirection.X);
            model.CreatePointLoadInNode(pln1);
            #endregion
            #region Create Load - Point Loads - Free
            loadValue = -12500.0;
            PointLoadFree plf1 = new PointLoadFree(Guid.NewGuid(), "plf1", lc_perm.Id, loadValue, a / 3.0, b / 3.0, c, (int)eDirection.Z, c - 1.0, c + 1.0);
            model.CreatePointLoadFree(plf1);
            #endregion
            #region Create Load - Surface Loads - on Slab
            loadValue = -12500.0;
            SurfaceLoad sf1 = new SurfaceLoad(Guid.NewGuid(), "sf1", loadValue, lc_perm.Id, s1.Id, (int)eDirection.Z);
            SurfaceLoad sf2 = new SurfaceLoad(Guid.NewGuid(), "sf2", loadValue, lc_var1.Id, s1.Id, (int)eDirection.Y);
            SurfaceLoad sf3 = new SurfaceLoad(Guid.NewGuid(), "sf3", loadValue, lc_var2.Id, s1.Id, (int)eDirection.X);
            SurfaceLoad sf4 = new SurfaceLoad(Guid.NewGuid(), "sf4", loadValue, lc_var3a.Id, s1.Id, (int)eDirection.X);
            SurfaceLoad sf5 = new SurfaceLoad(Guid.NewGuid(), "sf5", loadValue, lc_var3b.Id, s1.Id, (int)eDirection.Y);
            SurfaceLoad sf6 = new SurfaceLoad(Guid.NewGuid(), "sf6", loadValue, lc_var3c.Id, s1.Id, (int)eDirection.Z);
            foreach (var x in new List<SurfaceLoad> { sf1 }) { model.CreateSurfaceLoad(x); }
            #endregion
            #region Create Load - Line Load - on Beam & on Slab Edge
            var lin1 = new LineLoadOnBeam(Guid.NewGuid(), "lin1")
            {
                Member = b1.Id,
                LoadCase = lc_perm.Id,
                Distribution = eLineLoadDistribution.Trapez,
                Value1 = -12500,
                Value2 = -12500,
                CoordinateDefinition = eCoordinateDefinition.Relative,
                StartPoint = 0.01,
                EndPoint = 0.99,
                CoordinationSystem = eCoordinationSystem.GCS,
                Direction = eDirection.X,
                Origin = eLineOrigin.FromStart,
                Location = eLineLoadLocation.Length,
                EccentricityEy = 0.0,
                EccentricityEz = 0.0
            };
            var lin2 = new LineLoadOnBeam(Guid.NewGuid(), "lin2")
            {
                Member = b1.Id,
                LoadCase = lc_var1.Id,
                Distribution = eLineLoadDistribution.Trapez,
                Value1 = -12500,
                Value2 = 12500,
                CoordinateDefinition = eCoordinateDefinition.Relative,
                StartPoint = 0.01,
                EndPoint = 0.99,
                CoordinationSystem = eCoordinationSystem.GCS,
                Direction = eDirection.Y,
                Origin = eLineOrigin.FromStart,
                Location = eLineLoadLocation.Projection,
                EccentricityEy = 0.0,
                EccentricityEz = 0.0
            };
            var lin3a = new LineLoadOnSlabEdge(Guid.NewGuid(), "lin3a")
            {
                Member = s1.Id,
                LoadCase = lc_var3a.Id,
                EdgeIndex = 0,
                Distribution = eLineLoadDistribution.Trapez,
                Value1 = -12500,
                Value2 = 12500,
                CoordinateDefinition = eCoordinateDefinition.Relative,
                StartPoint = 0.01,
                EndPoint = 0.99,
                CoordinationSystem = eCoordinationSystem.GCS,
                Direction = eDirection.Z,
                Origin = eLineOrigin.FromStart,
                Location = eLineLoadLocation.Length
            };
            var lin3b = new LineLoadOnSlabEdge(Guid.NewGuid(), "lin3b")
            {
                Member = s1.Id,
                LoadCase = lc_var3b.Id,
                EdgeIndex = 1,
                Distribution = eLineLoadDistribution.Trapez,
                Value1 = -12500,
                Value2 = 12500,
                CoordinateDefinition = eCoordinateDefinition.Relative,
                StartPoint = 0.01,
                EndPoint = 0.99,
                CoordinationSystem = eCoordinationSystem.GCS,
                Direction = eDirection.Z,
                Origin = eLineOrigin.FromStart,
                Location = eLineLoadLocation.Length
            };
            var lin3c = new LineLoadOnSlabEdge(Guid.NewGuid(), "lin3c")
            {
                Member = s1.Id,
                LoadCase = lc_var3c.Id,
                EdgeIndex = 2,
                Distribution = eLineLoadDistribution.Trapez,
                Value1 = -12500,
                Value2 = 12500,
                CoordinateDefinition = eCoordinateDefinition.Relative,
                StartPoint = 0.01,
                EndPoint = 0.99,
                CoordinationSystem = eCoordinationSystem.GCS,
                Direction = eDirection.Z,
                Origin = eLineOrigin.FromStart,
                Location = eLineLoadLocation.Length
            };
            var lin3d = new LineLoadOnSlabEdge(Guid.NewGuid(), "lin3d")
            {
                Member = s1.Id,
                LoadCase = lc_perm.Id,
                EdgeIndex = 3,
                Distribution = eLineLoadDistribution.Trapez,
                Value1 = -12500,
                Value2 = 12500,
                CoordinateDefinition = eCoordinateDefinition.Relative,
                StartPoint = 0.01,
                EndPoint = 0.99,
                CoordinationSystem = eCoordinationSystem.GCS,
                Direction = eDirection.Z,
                Origin = eLineOrigin.FromStart,
                Location = eLineLoadLocation.Length
            };
            foreach (var x in new List<LineLoadOnBeam> { lin1 }) { model.CreateLineLoad(x); }
            foreach (var x in new List<LineLoadOnSlabEdge> { lin3d }) { model.CreateLineLoad(x); }
            //foreach (var x in new List<LineLoadOnBeam> { lin1, lin2 }) { model.CreateLineLoad(x); }
            //foreach (var x in new List<LineLoadOnSlabEdge> { lin3a, lin3b, lin3c, lin3d }) { model.CreateLineLoad(x); }
            #endregion
            #endregion
            return true;
        }

    }
}
