using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NPKG.UI
{
    public partial class ImportPackageForm : Form
    {
        public GH_Canvas Canvas { get; private set; }
        public string WorkDir { get; private set; }

        public ImportPackageForm()
        {
            Load += ImportPackageForm_Load;
            MouseDown += ImportPackageForm_MouseDown;
            MouseUp += ImportPackageForm_MouseUp;
            MouseMove += ImportPackageForm_MouseMove;
            KeyDown += ImportPackageForm_KeyDown;
            Canvas = Instances.ActiveCanvas;

            InitializeComponent();

            searchInput.LostFocus += SearchInput_LostFocus;
            searchInput.KeyDown += SearchInput_KeyDown;
            searchInput.TextChanged += SearchInput_TextChanged;
        }

        private int lastTextLength = 0;

        private void SearchInput_TextChanged(object sender, EventArgs e)
        {
            // TO DO 搜索框结果显示

            string text = searchInput.Text;
            if (text.Contains("`"))
            {
                FadeOut();
                return;
            }

            //if (text.Length > lastTextLength)
            //{
            //    Height += 12;
            //}
            //else
            //{
            //    Height -= 12;
            //}
            //text = text.Trim();
            //PopulateHitList(text);
            //lastTextLength = text.Length;
        }

        private void SearchInput_KeyDown(object sender, KeyEventArgs e)
        {
            string input = searchInput.Text;
            input = input.Trim();
            switch (e.KeyCode)
            {
                default:
                    return;
                case Keys.Return:
                    if (input.StartsWith(">"))
                    {
                        RunCommand(input.TrimStart(new char[] { '>' }));
                    } else InsertComponent();
                    break;
                case Keys.Cancel:
                case Keys.Escape:
                    FadeOut();
                    break;
            }
            e.Handled = true;
        }

        private void RunCommand(string cmd)
        {
        }

        private List<string> packageList = new List<string>
        {
            "Reference", "site", "about", "Lorem", "Ipsum", "giving", "information", "on", "its", "origins", "as", "well", "as", "a", "random", "Lipsum", "generator"
        };

        private List<string> SearchPackage(string[] keywords, int maxCnt)
        {
            if (keywords == null)
            {
                throw new Exception("keywords is requires");
            }
            if (keywords.Length == 0)
            {
                return new List<string>();
            }

            List<string> res = new List<string>();

            foreach(string word in keywords)
            {
                if (word == null) continue;
                string tmp = word.Trim();

                if (tmp.Length == 0) continue;

                packageList.Where(s => Regex.IsMatch(s, word)).ToList().ForEach(x => {
                    if (!res.Contains(x)) res.Add(x);
                });
            }

            return res;
        }

        private void PopulateHitList(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string[] array = key.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (array != null && array.Length != 0)
            {
                List<string> list = SearchPackage(array, 12);
            }
        }

        private void InsertComponent()
        {
            PointF at = Canvas.CursorCanvasPosition;
            string text = searchInput.Text;
            text = text.Trim();
            if (text.Length == 0)
            {
                FadeOut();
                return;
            }

            string modulePath = FindPackagePath(text);
            if (modulePath != null)
                Canvas.InstantiateNewObject(Identities.GBlock, modulePath, at, update: true);
            FadeOut();
        }

        public string FindPackagePath(string pkgcode)
        {
            var canvas = Instances.ActiveCanvas;
            var ghDoc = canvas.Document;
            if (canvas == null || ghDoc == null || !ghDoc.IsFilePathDefined) WorkDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (ghDoc != null)
            {
                if (ghDoc.IsFilePathDefined)
                {
                    try
                    {
                        WorkDir = Utils.getWorkDir(ghDoc.FilePath);
                    }
                    catch
                    {
                        WorkDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    }
                }
            }
            string npkgDir = Path.Combine(WorkDir, "npkgs");

            string[] pkgcmd = pkgcode.Split('.');

            string pkgDir = null;

            DirectoryInfo npkgInfo = new DirectoryInfo(npkgDir);
            foreach (DirectoryInfo info in npkgInfo.GetDirectories())
            {
                if (!string.Equals(info.Name, pkgcmd[0]))
                    continue;
                pkgDir = info.FullName;
            }

            if (pkgDir == null)
            {
                return null;
            }

            string JsonFile = Path.Combine(pkgDir, "npkg.json");
            if (!File.Exists(JsonFile)) return null;

            PackageInfo pkg = JsonConvert.DeserializeObject<PackageInfo>(File.ReadAllText(JsonFile));

            string _packagePath = null;
            // 遍历json中导出的内容
            foreach (string module in pkg.modules)
            {
                string modulePath = Path.Combine(pkgDir, module);

                if (!string.Equals(Path.GetFileNameWithoutExtension(modulePath).ToLower(), pkgcmd[1].ToLower()))
                    continue;

                if (File.Exists(modulePath))
                {
                    FileInfo packagePath = new FileInfo(modulePath);
                    _packagePath = packagePath.FullName;
                }
            }
            return _packagePath;
        }

        private void SearchInput_LostFocus(object sender, EventArgs e)
        {
            FadeOut();
        }

        private void FadeOut()
        {
            Hide();
            if (Instances.DocumentEditor != null)
            {
                Instances.DocumentEditor.BringToFront();
            }
            Close();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private void ImportPackageForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Cancel)
            {
                Close();
            }
        }

        private void ImportPackageForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                Point newLocation = new Point(Location.X + (e.X - originalMousePos.X), Location.Y + (e.Y - originalMousePos.Y));
                if (Location != newLocation)
                {
                    Location = newLocation;
                    Update();
                }
            }
        }

        private void ImportPackageForm_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void ImportPackageForm_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            originalMousePos = e.Location;
        }

        private void ImportPackageForm_Load(object sender, EventArgs e)
        {
            GH_WindowsControlUtil.FixTextRenderingDefault(Controls);
        }
    }
}
