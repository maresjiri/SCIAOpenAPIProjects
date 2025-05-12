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

using ModelExchanger.AnalysisDataModel.Models;
using ModelExchanger.AnalysisDataModel.StructuralElements;
using UnitsNet;


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
        //private static string SlabName { get; } = "S1";
        //private static string beamName { get; } = "b1";


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
            double span = 5.0;
            double depth = 0.8;
            double flangeWidth = 0.5;
            StructNode n1 = new StructNode(Guid.NewGuid(), "n1", 0, 0, 0);
            StructNode n2 = new StructNode(Guid.NewGuid(), "n2", span, 0, 0);
            StructNode n3 = new StructNode(Guid.NewGuid(), "n3", 0, 0, depth);
            StructNode n4 = new StructNode(Guid.NewGuid(), "n4", span, 0, depth);
            StructNode n5 = new StructNode(Guid.NewGuid(), "n5", 0, flangeWidth/2, 0);
            StructNode n6 = new StructNode(Guid.NewGuid(), "n6", span, flangeWidth/2, 0);
            StructNode n7 = new StructNode(Guid.NewGuid(), "n7", 0,flangeWidth/2, depth);
            StructNode n8 = new StructNode(Guid.NewGuid(), "n8", span, flangeWidth / 2, depth);
            StructNode n9 = new StructNode(Guid.NewGuid(), "n9", 0, -flangeWidth / 2, 0);
            StructNode n10 = new StructNode(Guid.NewGuid(), "n10", span, -flangeWidth / 2, 0);
            StructNode n11 = new StructNode(Guid.NewGuid(), "n11", 0, -flangeWidth / 2, depth);
            StructNode n12 = new StructNode(Guid.NewGuid(), "n12", span, -flangeWidth / 2, depth);
            StructNode n13 = new StructNode(Guid.NewGuid(), "n13", span/3, 0, depth);
            StructNode n14 = new StructNode(Guid.NewGuid(), "n14", 2*span/3, 0, depth);
            
            //StructNode n15 = new StructNode(Guid.NewGuid(), "n15", span, flangeWidth / 2, 0);
            //StructNode n16 = new StructNode(Guid.NewGuid(), "n16", span, flangeWidth / 2, depth);
            //StructNode n17 = new StructNode(Guid.NewGuid(), "n17", span, -flangeWidth / 2, 0);
            //StructNode n18 = new StructNode(Guid.NewGuid(), "n18", span, -flangeWidth / 2, depth);


            foreach (var x in new List<StructNode> { n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14 }) { model.CreateNode(x); }


            //StructuralPointConnection n13 = new StructuralPointConnection(Guid.NewGuid(), "n13", Length.FromMeters(span / 3), Length.FromMeters(0), Length.FromMeters(depth));
            //StructuralPointConnection n14 = new StructuralPointConnection(Guid.NewGuid(), "n14", Length.FromMeters(2 *span/3), Length.FromMeters(0), Length.FromMeters(depth));
            //model.CreateAdmObject(n13);
            //model.CreateAdmObject(n14);
            
            
            

            #region Create Plates
            double thickness = 0.01;
            Slab s1 = new Slab(Guid.NewGuid(), "Web1", (int)Slab_Type.Plate, concmat.Id, thickness, new Guid[6] { n1.Id, n2.Id, n4.Id, n14.Id, n13.Id, n3.Id });
            model.CreateSlab(s1);

            Slab s2 = new Slab(Guid.NewGuid(), "TopFlangeR1", (int)Slab_Type.Plate, concmat.Id, thickness, new Guid[6] { n3.Id, n7.Id, n8.Id, n4.Id, n14.Id, n13.Id });
            model.CreateSlab(s2);

            Slab s3 = new Slab(Guid.NewGuid(), "TopFlangeL1", (int)Slab_Type.Plate, concmat.Id, thickness, new Guid[6] { n11.Id, n3.Id, n13.Id, n14.Id, n4.Id, n12.Id });
            model.CreateSlab(s3);

            Slab s4 = new Slab(Guid.NewGuid(), "BtmFlangeR1", (int)Slab_Type.Plate, concmat.Id, thickness, new Guid[4] { n1.Id, n5.Id, n6.Id, n2.Id });
            model.CreateSlab(s4);

            Slab s5 = new Slab(Guid.NewGuid(), "BtmFlangeL1", (int)Slab_Type.Plate, concmat.Id, thickness, new Guid[4] { n9.Id, n1.Id, n2.Id, n10.Id });
            model.CreateSlab(s5);

            // TODO CHANGE GUID TO 6

            ////Slab s6 = new Slab(Guid.NewGuid(), "Web2", (int)Slab_Type.Plate, concmat.Id, thickness, new Guid[4] { n1.Id, n2.Id, n4.Id, n3.Id });
            ////model.CreateSlab(s1);

            ////Slab s7 = new Slab(Guid.NewGuid(), "TopFlangeR2", (int)Slab_Type.Plate, concmat.Id, thickness, new Guid[4] { n3.Id, n7.Id, n8.Id, n4.Id });
            ////model.CreateSlab(s2);

            ////Slab s8 = new Slab(Guid.NewGuid(), "TopFlangeL2", (int)Slab_Type.Plate, concmat.Id, thickness, new Guid[4] { n11.Id, n3.Id, n4.Id, n12.Id });
            ////model.CreateSlab(s3);

            ////Slab s9 = new Slab(Guid.NewGuid(), "BtmFlangeR2", (int)Slab_Type.Plate, concmat.Id, thickness, new Guid[4] { n1.Id, n5.Id, n6.Id, n2.Id });
            ////model.CreateSlab(s4);

            ////Slab s10 = new Slab(Guid.NewGuid(), "BtmFlangeL2", (int)Slab_Type.Plate, concmat.Id, thickness, new Guid[4] { n9.Id, n1.Id, n2.Id, n10.Id });
            ////model.CreateSlab(s5);



            #endregion





            #region Create Support - in Node
            PointSupport Su1 = new PointSupport(Guid.NewGuid(), "Su1", n1.Id) { ConstraintRx = eConstraintType.Free, ConstraintRy = eConstraintType.Free, ConstraintRz = eConstraintType.Free };
            PointSupport Su2 = new PointSupport(Guid.NewGuid(), "Su2", n2.Id) { ConstraintRx = eConstraintType.Free, ConstraintRy = eConstraintType.Free, ConstraintRz = eConstraintType.Free };
            PointSupport Su3 = new PointSupport(Guid.NewGuid(), "Su3", n5.Id) { ConstraintRx = eConstraintType.Free, ConstraintRy = eConstraintType.Free, ConstraintRz = eConstraintType.Free };
            PointSupport Su4 = new PointSupport(Guid.NewGuid(), "Su4", n6.Id) { ConstraintRx = eConstraintType.Free, ConstraintRy = eConstraintType.Free, ConstraintRz = eConstraintType.Free };
            PointSupport Su5 = new PointSupport(Guid.NewGuid(), "Su5", n9.Id) { ConstraintRx = eConstraintType.Free, ConstraintRy = eConstraintType.Free, ConstraintRz = eConstraintType.Free };
            PointSupport Su6 = new PointSupport(Guid.NewGuid(), "Su6", n10.Id) { ConstraintRx = eConstraintType.Free, ConstraintRy = eConstraintType.Free, ConstraintRz = eConstraintType.Free };



            foreach (var x in new List<PointSupport> { Su1, Su2, Su3, Su4, Su5, Su6 }) { model.CreatePointSupport(x); }
            #endregion

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
            PointLoadInNode pln1 = new PointLoadInNode(Guid.NewGuid(), "pln1", loadValue, lc_perm.Id, n13.Id, (int)eDirection.Z);
            model.CreatePointLoadInNode(pln1);
            #endregion
            return true;
        }
    }
    #endregion
}
