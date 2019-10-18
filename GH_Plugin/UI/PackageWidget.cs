using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Widgets;

namespace NPKG
{
    public class PackageWidget : GH_Widget
    {
        private RectangleF Bounds = new RectangleF(100, 100, 24, 24);
        private bool drag = false;
        private PointF clickOffset = new PointF(0, 0);

        private bool _visibile = true;
        public override bool Visible { get => _visibile; set => _visibile = value; }

        public override string Name => "Pacakge Widget";

        public override string Description => "";

        public override Bitmap Icon_24x24 => Properties.Resources.repository;

        public override bool Contains(Point pt_control, PointF pt_canvas)
        {
            if (Bounds.Contains(pt_control) || Bounds.Contains(Owner.Viewport.ProjectPoint(pt_canvas))) return true;
            else return false;
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && Bounds.Contains(e.ControlLocation))
            {
                drag = true;
                Grasshopper.Instances.CursorServer.AttachCursor(Owner, "GH_HandClosed");
                Owner.Refresh();
                clickOffset = new PointF(e.ControlX - Bounds.X, e.ControlY - Bounds.Y);
                return GH_ObjectResponse.Capture;
            }
            return GH_ObjectResponse.Ignore;
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                drag = false;
            }
            return GH_ObjectResponse.Release;
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (drag)
            {
                Bounds.X = e.ControlX - clickOffset.X;
                Bounds.Y = e.ControlY - clickOffset.Y;
                Owner.Refresh();
                return GH_ObjectResponse.Handled;
            }
            if (Bounds.Contains(e.ControlLocation))
            {
                Grasshopper.Instances.CursorServer.AttachCursor(Owner, "GH_HandOpen");
                Owner.Refresh();
                return GH_ObjectResponse.Handled;
            }
            return GH_ObjectResponse.Ignore;
        }

        public override GH_ObjectResponse RespondToKeyDown(GH_Canvas sender, KeyEventArgs e)
        {
            MessageBox.Show(e.KeyValue.ToString());
            return GH_ObjectResponse.Handled;
        }

        public override void Render(GH_Canvas Canvas)
        {
            Matrix viewportTransform = Canvas.Viewport.XFormMatrix(GH_Viewport.GH_DisplayMatrix.CanvasToControl);
            Canvas.Graphics.ResetTransform();
            Canvas.Graphics.DrawImage(Icon_24x24, Bounds);
            Canvas.Graphics.Transform = viewportTransform;
        }
    }
}
