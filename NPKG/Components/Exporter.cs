using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPKG.Components
{
    public class Exporter : GH_Component
    {
        public Exporter()
            : base("Export", "export", "", "NPKG", "Module")
        {
        }
        public override Guid ComponentGuid => Identities.Exporter;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }
    }
}
