using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPKG
{
    public class InfoPanelAttr : GH_ResizableAttributes<InfoPanel>
    {
        protected override Size MinimumSize => new Size(120, 60);

        protected override Padding SizingBorders => new Padding(6);

        public InfoPanelAttr(InfoPanel panel)
            :base(panel)
        {
            PerformLayout();
            Bounds = new Rectangle(0, 0, 120, 60);
        }

        protected override void Layout()
        {
            RectangleF @in = new RectangleF(Pivot, Bounds.Size);
            if (@in.Width < (float)MinimumSize.Width)
            {
                @in.Width = MinimumSize.Width;
            }
            if (@in.Height < (float)MinimumSize.Height)
            {
                @in.Height = MinimumSize.Height;
            }
            Bounds = GH_Convert.ToRectangle(@in);
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            GH_Document gH_Document = Instances.ActiveCanvas.Document;
            if (gH_Document.IsModified)
            {
                MessageBox.Show("Please save document first !");
            } else
            {
                try
                {
                    string workDir = getWorkDir(gH_Document.FilePath);
                    File.WriteAllText(Path.Combine(workDir, "README.md"), "# " + gH_Document.DisplayName);
                    File.WriteAllText(Path.Combine(workDir, "npkg.json"), "# " + gH_Document.DisplayName);
                } catch
                {
                    MessageBox.Show("This path is not in git repository");
                }
            }
            return GH_ObjectResponse.Handled;
        }

        /// <summary>
        /// Find a dir contains a dir named .git
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string getWorkDir(string path)
        {
            if (File.Exists(path) || Directory.Exists(path))
            {
                if (!string.Equals(Path.GetDirectoryName(path), null))
                {
                    string fileDir = Path.GetDirectoryName(path);
                    DirectoryInfo dir = new DirectoryInfo(fileDir);
                    DirectoryInfo[] dirInfos = dir.GetDirectories();
                    string workDir = string.Empty;
                    foreach (DirectoryInfo info in dirInfos)
                    {
                        if (info.Name == ".git")
                        {
                            workDir = fileDir;
                            break;
                        }
                    }
                    if (string.Equals(workDir, string.Empty)) return getWorkDir(fileDir);
                    else return workDir;
                }
                else throw new Exception(string.Format("This path is not in git repository", path));
            }
            else throw new Exception(string.Format("Path '{0}' doesn't exists !", path));
        }
    }
}
