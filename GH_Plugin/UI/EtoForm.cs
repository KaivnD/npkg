using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;

namespace NPKG.UI
{
    public class EtoForm : Form
    {
        public EtoForm()
        {
            ClientSize = new Size(360, 60);
            Padding = new Padding(8, 8);
            WindowStyle = WindowStyle.None;
            //Resizable = false;
            Opacity = 0.5;
            ShowInTaskbar = false;
            BackgroundColor = Color.FromArgb(20, 20, 20, 50);

            TextBox cmd = new TextBox()
            {
                TextAlignment = TextAlignment.Center,
                BackgroundColor = Color.FromArgb(20, 20, 20, 50),
                ShowBorder = false,
                TextColor = Color.FromGrayscale(100),
                Font = new Font("Microsoft YaHei UI", 26.25F),
                Size = new Size(344, 45)
            };

            cmd.LostFocus += Cmd_LostFocus;

            DynamicLayout layout = new DynamicLayout();
            layout.AddAutoSized(cmd);

            Content = layout;
        }

        private void Cmd_LostFocus(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
