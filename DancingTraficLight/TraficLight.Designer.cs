namespace DancingTraficLight
{
    partial class TraficLight
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
            this.outPutPicture = new System.Windows.Forms.PictureBox();
            this.outPutPicture_test = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.outPutPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outPutPicture_test)).BeginInit();
            this.SuspendLayout();
            // 
            // outPutPicture
            // 
            this.outPutPicture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.outPutPicture.Location = new System.Drawing.Point(0, 0);
            this.outPutPicture.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.outPutPicture.Name = "outPutPicture";
            this.outPutPicture.Size = new System.Drawing.Size(261, 242);
            this.outPutPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.outPutPicture.TabIndex = 3;
            this.outPutPicture.TabStop = false;
            // 
            // outPutPicture_test
            // 
            this.outPutPicture_test.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.outPutPicture_test.Location = new System.Drawing.Point(296, 0);
            this.outPutPicture_test.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.outPutPicture_test.Name = "outPutPicture_test";
            this.outPutPicture_test.Size = new System.Drawing.Size(261, 242);
            this.outPutPicture_test.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.outPutPicture_test.TabIndex = 4;
            this.outPutPicture_test.TabStop = false;
            // 
            // TraficLight
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Snow;
            this.ClientSize = new System.Drawing.Size(561, 242);
            this.Controls.Add(this.outPutPicture_test);
            this.Controls.Add(this.outPutPicture);
            this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "TraficLight";
            this.Text = "Dacing Trafic Light";
            ((System.ComponentModel.ISupportInitialize)(this.outPutPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outPutPicture_test)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox outPutPicture;
        private System.Windows.Forms.PictureBox outPutPicture_test;
    }
}

