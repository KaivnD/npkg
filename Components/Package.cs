using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPKG
{
    public class Package : GH_Component, IGH_InitCodeAware, IGH_VariableParameterComponent
    {
        private string m_name = @"D:\test.gh";
        internal GH_Document m_document;

        public string packageName
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        public Guid DocumentId
        {
            get
            {
                if (m_document == null)
                {
                    return Guid.Empty;
                }
                return m_document.DocumentID;
            }
        }

        //public override BoundingBox ClippingBox
        //{
        //    get
        //    {
        //        BoundingBox clippingBox = base.ClippingBox;
        //        if (m_document != null)
        //        {
        //            foreach (IGH_DocumentObject @object in m_document.Objects)
        //            {
        //                IGH_PreviewObject iGH_PreviewObject = @object as IGH_PreviewObject;
        //                if (iGH_PreviewObject != null && !iGH_PreviewObject.Hidden)
        //                {
        //                    clippingBox.Union(iGH_PreviewObject.ClippingBox);
        //                }
        //            }
        //            return clippingBox;
        //        }
        //        return clippingBox;
        //    }
        //}

        public Package()
            :base("GBlock", "npkg", "", "NPKG", "Module")
        {
        }

        public override void CreateAttributes()
        {
            m_attributes = new PackageAttr(this);
        }

        public override Guid ComponentGuid => Identities.GBlock;

        protected override void BeforeSolveInstance()
        {
            GH_DocumentIO io = new GH_DocumentIO();
            io.Open(packageName);
            m_document = io.Document;
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

            GH_Document doc = m_document;
            if (doc == null)
                throw new Exception("File could not be opened.");

            doc.Enabled = true;
            doc.NewSolution(true, GH_SolutionMode.Silent);

            GH_ClusterOutputHook[] outputs = doc.ClusterOutputHooks();

            dataTree = new DataTree<object>();
            var hint = new GH_NullHint();
            dataTree.MergeStructure(outputs[0].VolatileData, hint);

            doc.Dispose();

            return dataTree;
        }

        private void SetPackageInput()
        {
            GH_ClusterInputHook[] inputs = m_document.ClusterInputHooks();
            IGH_Param iGH_Param = Params.Input[0];

            if (iGH_Param != null && inputs[0] != null)
            {
                GH_Structure<IGH_Goo> gH_Structure = new GH_Structure<IGH_Goo>();
                int num = iGH_Param.VolatileData.PathCount - 1;
                IEnumerator enumerator3 = default(IEnumerator);
                for (int i = 0; i <= num; i++)
                {
                    GH_Path path = iGH_Param.VolatileData.get_Path(i);
                    IList list = iGH_Param.VolatileData.get_Branch(i);
                    try
                    {
                        enumerator3 = list.GetEnumerator();
                        while (enumerator3.MoveNext())
                        {
                            IGH_Goo data = (IGH_Goo)enumerator3.Current;
                            gH_Structure.Append(data, path);
                        }
                    }
                    finally
                    {
                        if (enumerator3 is IDisposable)
                        {
                            (enumerator3 as IDisposable).Dispose();
                        }
                    }
                    inputs[0].SetPlaceholderData(gH_Structure);
                }
            }
        }

        public void EditPackage()
        {
            if (m_document == null)
            {
                return;
            }
            GH_Canvas activeCanvas = Instances.ActiveCanvas;
            if (activeCanvas != null)
            {
                Instances.DocumentServer.AddDocument(m_document);
                activeCanvas.Document = m_document;
                Rectangle screenPort = activeCanvas.Viewport.ScreenPort;
                Rectangle r = GH_Convert.ToRectangle(m_document.BoundingBox());
                r.Inflate(5, 5);
                screenPort.Inflate(-5, -5);
                new GH_NamedView(screenPort, r).SetToViewport(activeCanvas, 250);
                m_document.NewSolution(expireAllObjects: false);
            }
        }

        #region 动态输入输出部分
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

        #endregion

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
    }
}
