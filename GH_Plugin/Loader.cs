using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using NPKG.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPKG
{
    public class Loader : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.CanvasCreated += (canvas) => { canvas.KeyDown += Canvas_KeyDown; };
            GH_Canvas.WidgetListCreated += GH_Canvas_WidgetListCreated;
            return GH_LoadingInstruction.Proceed;
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Oemtilde)
            {
                GH_Canvas gH_Canvas = Instances.ActiveCanvas;
                ImportPackageForm importPackageForm = new ImportPackageForm();

                GH_WindowsFormUtil.CenterFormOnCursor(importPackageForm, limitToScreen: true);
                Form form = gH_Canvas.FindForm();
                if (form != null && form is GH_DocumentEditor)
                {
                    ((GH_DocumentEditor)form).FormShepard.RegisterForm(importPackageForm);
                }

                importPackageForm.Show(Instances.ActiveCanvas.FindForm());

            } else if (e.KeyCode == Keys.Oemtilde | e.Shift)
            {
                MessageBox.Show("Shift Package");
            }
        }

        private void GH_Canvas_WidgetListCreated(object sender, GH_CanvasWidgetListEventArgs e)
        {
            PackageWidget pw = new PackageWidget();
            e.AddWidget(pw);
        }
    }
}
