namespace Lab_1
{
    partial class FormNewDocument
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNewDocument));
            this.SuspendLayout();
            // 
            // FormNewDocument
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormNewDocument";
            this.Text = "Новый документ";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormNewDocument_MouseDown);
            this.MouseLeave += new System.EventHandler(this.FormNewDocument_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FormNewDocument_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormNewDocument_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}