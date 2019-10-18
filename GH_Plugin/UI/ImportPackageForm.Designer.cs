using System.Drawing;
using System.Windows.Forms;

namespace NPKG.UI
{
    partial class ImportPackageForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.searchInput = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // searchInput
            // 
            this.searchInput.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.searchInput.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.searchInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.searchInput.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchInput.Font = new System.Drawing.Font("Microsoft YaHei UI", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.searchInput.ForeColor = System.Drawing.SystemColors.Window;
            this.searchInput.Location = new System.Drawing.Point(8, 8);
            this.searchInput.Name = "searchInput";
            this.searchInput.Size = new System.Drawing.Size(344, 45);
            this.searchInput.TabIndex = 0;
            this.searchInput.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.searchInput.WordWrap = false;
            // 
            // ImportPackageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(360, 61);
            this.ControlBox = false;
            this.Controls.Add(this.searchInput);
            this.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportPackageForm";
            this.Opacity = 0.6D;
            this.Padding = new System.Windows.Forms.Padding(8);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "ImportPackageForm";
            this.TransparencyKey = System.Drawing.Color.Silver;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox searchInput;
        private bool mouseDown;

        public Point originalMousePos { get; private set; }
    }
}