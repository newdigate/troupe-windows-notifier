namespace Troupe.WindowsApp.WinForms
{
    partial class BalloonForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BalloonForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelDetail = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(64, 66);
            this.panel1.TabIndex = 5;
            this.panel1.Click += new System.EventHandler(this.labelDetail_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(16, 16);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 32);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.labelDetail_Click);
            // 
            // labelDetail
            // 
            this.labelDetail.AutoSize = true;
            this.labelDetail.Location = new System.Drawing.Point(64, 17);
            this.labelDetail.MaximumSize = new System.Drawing.Size(540, 540);
            this.labelDetail.MinimumSize = new System.Drawing.Size(24, 0);
            this.labelDetail.Name = "labelDetail";
            this.labelDetail.Padding = new System.Windows.Forms.Padding(3);
            this.labelDetail.Size = new System.Drawing.Size(46, 19);
            this.labelDetail.TabIndex = 14;
            this.labelDetail.Text = "Detail";
            this.labelDetail.MouseClick += new System.Windows.Forms.MouseEventHandler(this.labelDetail_Click);
            this.labelDetail.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.labelDetail_Click);
            // 
            // labelTitle
            // 
            this.labelTitle.AutoEllipsis = true;
            this.labelTitle.AutoSize = true;
            this.labelTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelTitle.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.ForeColor = System.Drawing.Color.Black;
            this.labelTitle.Location = new System.Drawing.Point(64, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Padding = new System.Windows.Forms.Padding(3);
            this.labelTitle.Size = new System.Drawing.Size(45, 22);
            this.labelTitle.TabIndex = 15;
            this.labelTitle.Text = "Title";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelTitle.MouseClick += new System.Windows.Forms.MouseEventHandler(this.labelDetail_Click);
            this.labelTitle.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.labelDetail_Click);
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.Location = new System.Drawing.Point(303, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(16, 17);
            this.button1.TabIndex = 16;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // BalloonForm
            // 
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(320, 66);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.labelDetail);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(1024, 640);
            this.MinimumSize = new System.Drawing.Size(320, 64);
            this.Name = "BalloonForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.BalloonForm_Load);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.labelDetail_Click);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.labelDetail_Click);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelDetail;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button button1;

    }
}
