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
        internal GH_Document m_document;
        public SortedDictionary<Guid, Guid> ParamHookMap;
        public string packageName { get; set; } = @"D:\test.gh";

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

        public Package()
            : base("GBlock", "npkg", "", "NPKG", "Module")
        {
            ParamHookMap = new SortedDictionary<Guid, Guid>();
        }

        public override void CreateAttributes()
        {
            m_attributes = new PackageAttr(this);
        }

        public override Guid ComponentGuid => Identities.GBlock;

        private bool inputAdjusted = false;
        private bool outputAdjusted = false;

        protected override void BeforeSolveInstance()
        {
            GH_DocumentIO io = new GH_DocumentIO();
            io.Open(packageName);
            m_document = io.Document;
            if (!inputAdjusted) AdjustPackageInput();
            if (!outputAdjusted) AdjustPackageOutput();
            SetPackageInput();
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (packageName == null) return;
            try
            {
                Message = packageName;
                DA.SetDataTree(0, SolutionTrigger());
            }
            catch (Exception ex)
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

            //doc.Dispose();

            return dataTree;
        }

        private void AdjustPackageInput()
        {
            GH_ClusterInputHook[] inputs = m_document.ClusterInputHooks();
            for (int i = 0; i < inputs.Length; i++)
            {
                Params.RegisterInputParam(CreateParameter(GH_ParameterSide.Input, i));

                GH_ClusterInputHook inputHook = inputs[i];
                // 找到所有在输入端下游的Component
                var allDnComponents = m_document.FindAllDownstreamObjects(inputs[0]);
                if (allDnComponents.Count > 0)
                {
                    // 取第一个
                    var inputAdjustment = allDnComponents[0] as GH_Component;

                    if (inputAdjustment != null && Equals(inputAdjustment.ComponentGuid, Identities.SetDefault))
                    {
                        var plug = inputAdjustment.Params.Input[0] as Param_ScriptVariable;

                        var input = Params.Input[i] as Param_ScriptVariable;
                        input.Access = plug.Access;
                        input.TypeHint = plug.TypeHint;
                        input.NickName = inputHook.CustomNickName;
                        FixGhInput(input);

                        ParamHookMap.Remove(input.InstanceGuid);
                        ParamHookMap.Add(input.InstanceGuid, inputHook.InstanceGuid);
                    }
                }
            }

            inputAdjusted = true;
        }

        private void AdjustPackageOutput()
        {
            GH_ClusterOutputHook[] outputs = m_document.ClusterOutputHooks();
            for (int i = 0; i < outputs.Length; i++)
            {
                Params.RegisterOutputParam(CreateParameter(GH_ParameterSide.Output, i));
                GH_ClusterOutputHook outputHook = outputs[i];

                Params.Output[0].NickName = outputHook.NickName;
            }

            outputAdjusted = true;
        }

        private void SetPackageInput()
        {
            foreach (KeyValuePair<Guid, Guid> item in ParamHookMap)
            {
                IGH_Param iGH_Param = base.Params.Find(item.Key);
                GH_ClusterInputHook gH_ClusterInputHook = m_document.FindObject<GH_ClusterInputHook>(item.Value, topLevelOnly: true);

                if (iGH_Param != null && gH_ClusterInputHook != null)
                {
                    GH_Structure<IGH_Goo> gH_Structure = new GH_Structure<IGH_Goo>();
                    IEnumerator enumerator = default;
                    for (int i = 0; i < iGH_Param.VolatileData.PathCount; i++)
                    {
                        GH_Path path = iGH_Param.VolatileData.get_Path(i);
                        IList list = iGH_Param.VolatileData.get_Branch(i);
                        try
                        {
                            enumerator = list.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                IGH_Goo data = (IGH_Goo)enumerator.Current;
                                gH_Structure.Append(data, path);
                            }
                        }
                        finally
                        {
                            if (enumerator is IDisposable)
                            {
                                (enumerator as IDisposable).Dispose();
                            }
                        }

                        gH_ClusterInputHook.SetPlaceholderData(gH_Structure);
                    }
                }
            }
        }

        public void EditPackage()
        {
            if (m_document == null)
            {
                return;
            }
            SetPackageInput();
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

        #region 输入输出部分
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            VariableParameterMaintenance();
        }

        internal void FixGhInput(Param_ScriptVariable i, bool alsoSetIfNecessary = true)
        {
            i.Name = i.NickName;
            i.AllowTreeAccess = true;
            i.Optional = true;
            i.ShowHints = true;
            i.Hints = TypeHints.GetHints();

            if (string.IsNullOrEmpty(i.Description))
                i.Description = string.Format("Script variable {0}", i.NickName);

            if (alsoSetIfNecessary && i.TypeHint == null)
                i.TypeHint = i.Hints[1];
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
