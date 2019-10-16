using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace NPKG
{
    public class SetDefault : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SetDefault class.
        /// </summary>
        public SetDefault()
          : base("SetDefault", "default",
              "Description",
              "NPKG", "Module")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_ScriptVariable
            {
                Name = "Input",
                NickName = "I",
                Description = "",
                AllowTreeAccess = true,
                Optional = true,
                ShowHints = true,
                Hints = TypeHints.GetHints(),
            });

            pManager.AddParameter(new Param_ScriptVariable
            {
                Name = "Default",
                NickName = "D",
                Description = ""
            });

            Params.Input[0].ObjectChanged += OnChange;
        }

        private void OnChange(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e)
        {
            var input = Params.Input[0] as Param_ScriptVariable;
            var @default = Params.Input[1] as Param_ScriptVariable;
            var output = Params.Output[0] as Param_ScriptVariable;

            if (input != null && @default !=null && output != null)
            {
                @default.Access = input.Access;
                @default.TypeHint = input.TypeHint;
                output.Access = input.Access;
                output.TypeHint = input.TypeHint;

                @default.RemoveAllSources();
            }
        }


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_ScriptVariable
            {
                Name = "Output",
                NickName = "O",
                Description = ""
            });
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            switch(Params.Input[0].Access)
            {
                case GH_ParamAccess.item:
                    {
                        var input = null as object;
                        DA.GetData(0, ref input);
                        if (input == null) DA.GetData(1, ref input);
                        DA.SetData(0, input);
                        break;
                    }
                case GH_ParamAccess.list:
                    {
                        List<object> input = new List<object>();
                        DA.GetDataList(0, input);
                        if (input.Count == 0) DA.GetDataList(1, input);
                        DA.SetDataList(0, input);
                        break;
                    }
                case GH_ParamAccess.tree:
                    {
                        GH_Structure<IGH_Goo> input;
                        DA.GetDataTree(0, out input);
                        if (input == null) DA.GetDataTree(1, out input);
                        DA.SetDataTree(0, input);
                        break;
                    }
                default:
                    break;
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => Identities.SetDefault;
    }
}