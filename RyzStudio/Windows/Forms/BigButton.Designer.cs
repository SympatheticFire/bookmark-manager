namespace RyzStudio.Windows.Forms
{
  partial class BigButton
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.button1 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(177)))), ((int)(((byte)(12)))));
      this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.button1.FlatAppearance.BorderSize = 0;
      this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(108)))), ((int)(((byte)(31)))));
      this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(76)))), ((int)(((byte)(0)))));
      this.button1.Location = new System.Drawing.Point(6, 6);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(116, 20);
      this.button1.TabIndex = 6;
      this.button1.UseVisualStyleBackColor = false;
      // 
      // BButton
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.button1);
      this.Name = "BButton";
      this.Padding = new System.Windows.Forms.Padding(6);
      this.Size = new System.Drawing.Size(128, 32);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button button1;

  }
}
