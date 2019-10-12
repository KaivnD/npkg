using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPKG.Components
{
    public class GBlock : GH_Component, IGH_InitCodeAware, IGH_VariableParameterComponent, IGH_DocumentOwner
    {
        public string packageName { set; get; }
        public GBlock()
            :base("GBlock", "npkg", "", "NPKG", "Module")
        {
        }

        public override void CreateAttributes()
        {
            m_attributes = new GBlockAttr(this);
        }

        public override Guid ComponentGuid => Identities.GBlock;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(CreateParameter(GH_ParameterSide.Input, pManager.ParamCount));
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(CreateParameter(GH_ParameterSide.Output, pManager.ParamCount));
            VariableParameterMaintenance();
        }

        protected static IGH_TypeHint[] PossibleHints =
        {
            new GH_HintSeparator(),
            new GH_BooleanHint_CS(), new GH_IntegerHint_CS(), new GH_DoubleHint_CS(), new GH_ComplexHint(),
            new GH_StringHint_CS(), new GH_DateTimeHint(), new GH_ColorHint(),
            new GH_HintSeparator(),
            new GH_Point3dHint(),
            new GH_Vector3dHint(), new GH_PlaneHint(), new GH_IntervalHint(),
            new GH_UVIntervalHint()
        };

        internal void FixGhInput(Param_ScriptVariable i, bool alsoSetIfNecessary = true)
        {
            i.Name = i.NickName;
            i.AllowTreeAccess = true;
            i.Optional = true;
            i.ShowHints = true;
            i.Hints = GetHints();

            if (string.IsNullOrEmpty(i.Description))
                i.Description = string.Format("Script variable {0}", i.NickName);

            if (alsoSetIfNecessary && i.TypeHint == null)
                i.TypeHint = i.Hints[1];
        }

        static readonly List<IGH_TypeHint> g_hints = new List<IGH_TypeHint>();
        static List<IGH_TypeHint> GetHints()
        {
            lock (g_hints)
            {
                if (g_hints.Count == 0)
                {
                    g_hints.AddRange(PossibleHints);

                    g_hints.RemoveAll(t =>
                    {
                        var y = t.GetType();
                        return (y == typeof(GH_DoubleHint_CS) || y == typeof(GH_StringHint_CS));
                    });

                    g_hints.Add(new GH_BoxHint());

                    g_hints.Add(new GH_HintSeparator());

                    g_hints.Add(new GH_LineHint());
                    g_hints.Add(new GH_CircleHint());
                    g_hints.Add(new GH_ArcHint());
                    g_hints.Add(new GH_PolylineHint());

                    g_hints.Add(new GH_HintSeparator());

                    g_hints.Add(new GH_CurveHint());
                    g_hints.Add(new GH_MeshHint());
                    g_hints.Add(new GH_SurfaceHint());
                    g_hints.Add(new GH_BrepHint());
                    g_hints.Add(new GH_GeometryBaseHint());
                }
            }
            return g_hints;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            try
            {
                if (packageName == null) return;
                Message = packageName;
                DA.SetDataTree(0, SolutionTrigger());
            } catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }
        }

        private DataTree<object> SolutionTrigger()
        {
            DataTree<object> dataTree = null;

            GH_Document doc = OwnerDocument();
            if (doc == null)
                throw new Exception("File could not be opened.");

            doc.Enabled = true;
            doc.NewSolution(true, GH_SolutionMode.Silent);

            foreach (IGH_DocumentObject obj in doc.Objects)
            {
                if (!obj.ComponentGuid.Equals(Identities.Exporter)) continue;
                var component = obj as IGH_Component;
                if (component == null) continue;

                dataTree = new DataTree<object>();
                var hint = new GH_NullHint();
                dataTree.MergeStructure(component.Params.Input[0].VolatileData, hint);
                break;
            }

            doc.Dispose();

            return dataTree;
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return index > -1;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return (this as IGH_VariableParameterComponent).CanInsertParameter(side, index);
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            switch (side)
            {
                case GH_ParameterSide.Input:
                    {
                        return new Param_ScriptVariable
                        {
                            NickName = GH_ComponentParamServer.InventUniqueNickname("xyzuvwst", Params.Input),
                            Name = NickName,
                            Description = "Ghblock variable " + NickName,
                        };
                    }
                case GH_ParameterSide.Output:
                    {
                        return new Param_GenericObject
                        {
                            NickName = GH_ComponentParamServer.InventUniqueNickname("abcdefghijklmn", Params.Output),
                            Name = NickName,
                            Description = "Ghblock variable " + NickName,
                        };
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            foreach (Param_ScriptVariable variable in Params.Input.OfType<Param_ScriptVariable>())
                FixGhInput(variable);

            foreach (Param_GenericObject i in Params.Output.OfType<Param_GenericObject>())
            {
                i.Name = i.NickName;
                if (string.IsNullOrEmpty(i.Description))
                    i.Description = i.NickName;
            }
        }

        public void SetInitCode(string code)
        {
            packageName = code;
        }


        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            bool rc = base.Write(writer);

            return rc;
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {

            bool rc = base.Read(reader);


            // Dynamic input fix for existing scripts
            // Always assign DynamicHint or Grasshopper
            // will set Line and not LineCurve, etc...
            if (Params != null && Params.Input != null)
            {
                for (int i = 0; i < Params.Input.Count; i++)
                {
                    var p = Params.Input[i] as Param_ScriptVariable;
                    if (p != null)
                    {
                        FixGhInput(p, false);
                        if (p.TypeHint == null)
                        {
                            p.TypeHint = p.Hints[0];
                        }
                    }
                }
            }
            return rc;
        }

        public void DocumentModified(GH_Document document)
        {
        }

        public void DocumentClosed(GH_Document document)
        {
        }

        public GH_Document OwnerDocument()
        {
            GH_DocumentIO io = new GH_DocumentIO();
            io.Open(packageName);
            return io.Document;
        }
    }
}
