using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPKG.Components
{
    public class Importer : GH_Component
    {
        public Importer()
            : base("Import", "import", "", "NPKG", "Module")
        {
        }
        public override Guid ComponentGuid => Identities.Importer;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var data = null as object;
            DA.GetData(0, ref data);
            DA.SetData(0, data);
        }
    }
}
