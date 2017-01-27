using System;
using System.Drawing;
using System.Windows.Forms;
using RyzStudio.Drawing;

namespace RyzStudio.Windows.Forms
{
    public partial class BigUserControl : UserControl
    {
        protected int borderWidth = 1;
        protected Pen borderPen = null;
        protected Color borderColor = Color.FromArgb(112, 112, 112);
        protected Brush backgroundBrush = null;
        protected Color backgroundColor = Color.FromKnownColor(KnownColor.White);

        public BigUserControl()
        {
            InitializeComponent();
            
            borderPen = new Pen(new SolidBrush(borderColor), borderWidth);
            backgroundBrush = new SolidBrush(backgroundColor);
        }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            int b4 = (borderWidth * 4);
            int b6 = (borderWidth * 6);
            
            this.BackColor = Color.FromKnownColor(KnownColor.WhiteSmoke);
            this.Padding = new Padding(b6, b4, b6, b4);
        }
        
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
////            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
////            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            
            int b3 = (borderWidth * 3);
            
            Rectangoid area = new Rectangoid(borderWidth, borderWidth, (this.ClientRectangle.Width - b3), (this.ClientRectangle.Height - b3), 3);
            g.FillPath(backgroundBrush, area.ToGraphicsPath());
            g.DrawPath(borderPen, area.ToGraphicsPath());
        }
    }
}