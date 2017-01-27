using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RyzStudio.Windows.Forms
{
    public partial class BigButton : BigUserControl
    {
        protected bool enableClick = false;

        public BigButton()
        {
            InitializeComponent();

            this.Button.Click += delegate (object s, EventArgs a) { this.OnClick(a); };
            this.Button.MouseEnter += delegate (object s, EventArgs a) { enableClick = true; };
            this.Button.MouseLeave += delegate (object s, EventArgs a) { enableClick = false; };
            this.Button.KeyDown += delegate (object s, KeyEventArgs a) { enableClick = true; };
            this.Button.KeyUp += delegate (object s, KeyEventArgs a) { enableClick = false; };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            int b4 = (borderWidth * 4);

            this.borderColor = Color.FromArgb(222, 222, 222);
            this.borderPen = new Pen(this.borderColor);

            this.Button.FlatStyle = FlatStyle.Flat;
            this.Button.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, 202, 206);
            this.Button.FlatAppearance.MouseOverBackColor = Color.FromArgb(238, 238, 238);
            this.Button.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);            
            this.Button.BackColor = Color.Transparent;
            this.Button.ForeColor = Color.FromArgb(51, 51, 51);
            this.Padding = new Padding(b4);
            this.MinimumSize = new Size(32, 32);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            this.Height = this.button1.Height + (this.button1.Top * 2);
        }

        protected override void OnClick(EventArgs e)
        {
            if (!enableClick)
            {
                return;
            }
            
            base.OnClick(e);
        }

        #region public properties

        [Category("Data")]
        public Button Button
        {
            get
            {
                return this.button1;
            }

            set
            {
                this.button1 = value;
            }
        }

        [Browsable(false)]
        public string Value
        {
            get
            {
                return this.button1.Text;
            }

            set
            {
                this.button1.Text = value;
            }
        }

        #endregion
    }
}