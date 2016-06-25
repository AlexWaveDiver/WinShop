namespace WinShop_Demo
{
    partial class frmMain
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
            this.btnUnpack = new System.Windows.Forms.Button();
            this.btnRepack = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnUnpack
            // 
            this.btnUnpack.Location = new System.Drawing.Point(65, 42);
            this.btnUnpack.Name = "btnUnpack";
            this.btnUnpack.Size = new System.Drawing.Size(105, 34);
            this.btnUnpack.TabIndex = 0;
            this.btnUnpack.Text = "Unpack winfile";
            this.btnUnpack.UseVisualStyleBackColor = true;
            this.btnUnpack.Click += new System.EventHandler(this.btnUnpack_Click);
            // 
            // btnRepack
            // 
            this.btnRepack.Location = new System.Drawing.Point(65, 106);
            this.btnRepack.Name = "btnRepack";
            this.btnRepack.Size = new System.Drawing.Size(105, 34);
            this.btnRepack.TabIndex = 1;
            this.btnRepack.Text = "Repack folder";
            this.btnRepack.UseVisualStyleBackColor = true;
            this.btnRepack.Click += new System.EventHandler(this.btnRepack_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(241, 191);
            this.Controls.Add(this.btnRepack);
            this.Controls.Add(this.btnUnpack);
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinShop Demo";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnUnpack;
        private System.Windows.Forms.Button btnRepack;
    }
}

