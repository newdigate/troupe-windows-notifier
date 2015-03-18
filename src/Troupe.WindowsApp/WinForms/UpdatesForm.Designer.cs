namespace Troupe.WindowsApp.WinForms
{
    partial class UpdatesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdatesForm));
            this.labelTxtInstalledVersion = new System.Windows.Forms.Label();
            this.labelInstalledVersion = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelLatestVersion = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelTxtInstalledVersion
            // 
            this.labelTxtInstalledVersion.AutoSize = true;
            this.labelTxtInstalledVersion.Location = new System.Drawing.Point(48, 88);
            this.labelTxtInstalledVersion.Name = "labelTxtInstalledVersion";
            this.labelTxtInstalledVersion.Size = new System.Drawing.Size(86, 13);
            this.labelTxtInstalledVersion.TabIndex = 0;
            this.labelTxtInstalledVersion.Text = "Installed version:";
            this.labelTxtInstalledVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelInstalledVersion
            // 
            this.labelInstalledVersion.AutoSize = true;
            this.labelInstalledVersion.Location = new System.Drawing.Point(150, 88);
            this.labelInstalledVersion.Name = "labelInstalledVersion";
            this.labelInstalledVersion.Size = new System.Drawing.Size(16, 13);
            this.labelInstalledVersion.TabIndex = 1;
            this.labelInstalledVersion.Text = "...";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(58, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Latest version:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelLatestVersion
            // 
            this.labelLatestVersion.AutoSize = true;
            this.labelLatestVersion.Location = new System.Drawing.Point(150, 111);
            this.labelLatestVersion.Name = "labelLatestVersion";
            this.labelLatestVersion.Size = new System.Drawing.Size(16, 13);
            this.labelLatestVersion.TabIndex = 3;
            this.labelLatestVersion.Text = "...";
            this.labelLatestVersion.Click += new System.EventHandler(this.label4_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(80, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(64, 64);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(12, 131);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(217, 39);
            this.button1.TabIndex = 6;
            this.button1.Text = "Refresh";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // UpdatesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(241, 182);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelLatestVersion);
            this.Controls.Add(this.labelInstalledVersion);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelTxtInstalledVersion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdatesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Troupe updates";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdatesForm_FormClosing);
            this.Shown += new System.EventHandler(this.UpdatesForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTxtInstalledVersion;
        private System.Windows.Forms.Label labelInstalledVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelLatestVersion;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button1;
    }
}