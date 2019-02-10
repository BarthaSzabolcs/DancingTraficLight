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
            this.input_x1 = new System.Windows.Forms.NumericUpDown();
            this.input_y1 = new System.Windows.Forms.NumericUpDown();
            this.input_x2 = new System.Windows.Forms.NumericUpDown();
            this.input_y2 = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.outPutPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.input_x1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.input_y1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.input_x2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.input_y2)).BeginInit();
            this.SuspendLayout();
            // 
            // outPutPicture
            // 
            this.outPutPicture.Location = new System.Drawing.Point(0, 0);
            this.outPutPicture.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.outPutPicture.Name = "outPutPicture";
            this.outPutPicture.Size = new System.Drawing.Size(484, 461);
            this.outPutPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.outPutPicture.TabIndex = 3;
            this.outPutPicture.TabStop = false;
            // 
            // input_x1
            // 
            this.input_x1.Location = new System.Drawing.Point(490, 12);
            this.input_x1.Name = "input_x1";
            this.input_x1.Size = new System.Drawing.Size(120, 23);
            this.input_x1.TabIndex = 4;
            this.input_x1.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.input_x1.ValueChanged += new System.EventHandler(this.InputBox_ValueChanged);
            // 
            // input_y1
            // 
            this.input_y1.Location = new System.Drawing.Point(616, 12);
            this.input_y1.Name = "input_y1";
            this.input_y1.Size = new System.Drawing.Size(120, 23);
            this.input_y1.TabIndex = 5;
            this.input_y1.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.input_y1.ValueChanged += new System.EventHandler(this.InputBox_ValueChanged);
            // 
            // input_x2
            // 
            this.input_x2.Location = new System.Drawing.Point(490, 41);
            this.input_x2.Name = "input_x2";
            this.input_x2.Size = new System.Drawing.Size(120, 23);
            this.input_x2.TabIndex = 6;
            this.input_x2.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.input_x2.ValueChanged += new System.EventHandler(this.InputBox_ValueChanged);
            // 
            // input_y2
            // 
            this.input_y2.Location = new System.Drawing.Point(616, 41);
            this.input_y2.Name = "input_y2";
            this.input_y2.Size = new System.Drawing.Size(120, 23);
            this.input_y2.TabIndex = 7;
            this.input_y2.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.input_y2.ValueChanged += new System.EventHandler(this.InputBox_ValueChanged);
            // 
            // TraficLight
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Snow;
            this.ClientSize = new System.Drawing.Size(754, 582);
            this.Controls.Add(this.input_y2);
            this.Controls.Add(this.input_x2);
            this.Controls.Add(this.input_y1);
            this.Controls.Add(this.input_x1);
            this.Controls.Add(this.outPutPicture);
            this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "TraficLight";
            this.Text = "Dacing Trafic Light";
            ((System.ComponentModel.ISupportInitialize)(this.outPutPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.input_x1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.input_y1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.input_x2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.input_y2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox outPutPicture;
        private System.Windows.Forms.NumericUpDown input_x1;
        private System.Windows.Forms.NumericUpDown input_y1;
        private System.Windows.Forms.NumericUpDown input_x2;
        private System.Windows.Forms.NumericUpDown input_y2;
    }
}

