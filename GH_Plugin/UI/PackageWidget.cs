using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Widgets;
using Grasshopper.Kernel;

namespace NPKG
{
    public class PackageWidget : GH_Widget
    {
        
        private bool drag = false;
        private PointF clickOffset = new PointF(0, 0);

        private bool _visibile = true;
        public override bool Visible { get => _visibile; set => _visibile = value; }

        public override string Name => "Pacakge Widget";

        public override string Description => "";

        public override Bitmap Icon_24x24 => Properties.Resources.repository;

        private Font font = new Font("Arial", 6);
        private SolidBrush Brush = new SolidBrush(Color.Black);

        public int max_x = Global_Proc.UiAdjust(110);
        public int max_y = Global_Proc.UiAdjust(12);
        private static readonly int GripSize = Global_Proc.UiAdjust(6);
        //UI adjsut


        private string Author = "Clef";
        private string ClusterVersion = "0.0.1";
        private string EditTime = "1970/1/1";


        private RectangleF Bounds = new RectangleF (0,0,100,100);

        private static GH_MarkovWidgetDock m_dockCorner = (GH_MarkovWidgetDock)Grasshopper.Instances.Settings.GetValue("Widget.Markov.Corner", 1);

        public RectangleF StringRectangle()
        {
           RectangleF clientRectangle = base.Owner.ClientRectangle;
            RectangleF Result = new RectangleF(Bounds.Left,Bounds.Top, max_x, max_y);
            RectangleF GripRec = new RectangleF(Result.Left - GripSize, Result.Top, GripSize, Result.Height);
            return RectangleF.Union(Result, GripRec);
        }

        

        private RectangleF GripArea
        {
            get
            {
                RectangleF clientRectangle = base.Owner.ClientRectangle;
                RectangleF StringArea = new RectangleF(Bounds.Left, Bounds.Top, max_x, max_y);

                RectangleF result = StringArea;
                result = new RectangleF(result.Left - GripSize, result.Top, GripSize, result.Height);

                return result;
            }
        }



        public override GH_ObjectResponse RespondToKeyDown(GH_Canvas sender, KeyEventArgs e)
        {
            MessageBox.Show(e.KeyValue.ToString());
            return GH_ObjectResponse.Handled;
        }


        public override void Render(GH_Canvas Canvas)
            ///render
        {
            Matrix viewportTransform = Canvas.Viewport.XFormMatrix(GH_Viewport.GH_DisplayMatrix.CanvasToControl);


            RenderBackGround(Canvas.Graphics);
            
            //Canvas.Graphics.DrawImage(Icon_24x24, Bounds);
            
            Canvas.Graphics.DrawString(GetString(), font, Brush, new PointF(Bounds.Left,Bounds.Top));
            Canvas.Graphics.ResetTransform();
            Canvas.Graphics.Transform = viewportTransform;
        }

        private String GetString()
            //get print string.
        {
            string DrawString = string.Format("Author:{0} ClusterVersion:{1} EditTime:{2} \n",Author,ClusterVersion,EditTime);

            if (Grasshopper.Instances.ActiveCanvas.Document != null)
            {
                IList<IGH_DocumentObject> K = Grasshopper.Instances.ActiveCanvas.Document.Objects;
                foreach(IGH_DocumentObject i in K)
                {
                   String temp = string.Format("Name:{0} NickName:{1} TypeName:{2} \n",i.Name,i.NickName,i.ToString().Split('.')[i.ToString().Split('.') .Length- 1]);
                   DrawString= DrawString+temp;
                    if (temp.Length*6>max_x)
                    {
                        max_x = temp.Length * 6;
                    }
                }
                max_y = K.Count*12+12;
                return (DrawString);

            }
            else
            {
                DrawString = DrawString +"Hello World!";
                return (DrawString);
            }

        }

        private void RenderBackGround(Graphics G)
            //background
        {
            RectangleF Bounds =StringRectangle();
            if (!Bounds.IsEmpty)
            {
                LinearGradientBrush linearGradientBrush = new LinearGradientBrush(Bounds, Color.FromArgb(230, 230, 230), Color.FromArgb(150, 150, 150), LinearGradientMode.Horizontal);
                linearGradientBrush.WrapMode = WrapMode.TileFlipXY;
                G.FillRectangle(linearGradientBrush, Bounds);
                linearGradientBrush.Dispose();
            }
            RectangleF gripArea = GripArea;
            if (!gripArea.IsEmpty)
            {
                G.FillRectangle(Brushes.Gray, gripArea);
            }
            //G.DrawLines(Pens.Black ,new PointF[] { new PointF(Bounds.Left, Bounds.Top), new PointF(Bounds.Left, Bounds.Bottom-2), new PointF(gripArea.Right+2, Bounds.Bottom-2), new PointF(gripArea.Right+2, Bounds.Bottom)});
        }


        public void ChangeClusterInfo(string author,string version,string time)
            //change info
        {
            Author = author;
            ClusterVersion = version;
            EditTime = time;
        }

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



        private void RenderButton(Graphics G)
        {

        }
       
    }
}
