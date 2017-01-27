using System;
using System.Windows.Forms;

namespace bzit.bomg
{
    public partial class PasswordForm : Form
    {
        private MainForm parentForm = null;

        public PasswordForm(MainForm form)
        {
            InitializeComponent();

            parentForm = form;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (parentForm == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(textBox1.Text))
            {
                return;
            }

            this.Close();
        }

        private void textBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    button2_Click(sender, null);
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
                default: break;
            }
        }

        public new string ShowDialog()
        {
            base.ShowDialog();

            return textBox1.Text;
        }
    }
}