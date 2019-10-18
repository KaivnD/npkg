using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPKG
{
    public class InfoPanel : GH_DocumentObject
    {
        public InfoPanel()
            :base(new GH_InstanceDescription("PackageInfo", "pi", "", "NPKG", "Module"))
        {
        }

        public override Guid ComponentGuid => Identities.InfoPanel;

        public override void CreateAttributes()
        {
            m_attributes = new InfoPanelAttr(this);
        }
    }
}
