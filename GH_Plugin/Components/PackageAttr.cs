using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;

namespace NPKG
{
    public class PackageAttr : GH_ComponentAttributes
    {
        public PackageAttr(Package owner) : base(owner)
        {
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            var package = Owner as Package;
            package.EditPackage();
            return GH_ObjectResponse.Handled;
        }
    }
}
