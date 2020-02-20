namespace _1612774
{
    partial class Form1
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
            this.show_plot = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.show_plot)).BeginInit();
            this.SuspendLayout();
            // 
            // show_plot
            // 
            this.show_plot.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.show_plot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.show_plot.Location = new System.Drawing.Point(402, 8);
            this.show_plot.Name = "show_plot";
            this.show_plot.Size = new System.Drawing.Size(500, 500);
            this.show_plot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.show_plot.TabIndex = 0;
            this.show_plot.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(908, 514);
            this.Controls.Add(this.show_plot);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Graphics";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.show_plot)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox show_plot;
    }
}

