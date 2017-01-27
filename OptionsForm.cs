using System;
using System.Windows.Forms;

namespace bzit.bomg
{
    public partial class OptionsForm : Form
    {
        private MainForm parentForm = null;

        public OptionsForm(MainForm form)
        {
            InitializeComponent();

            parentForm = form;

            this.StartPosition = FormStartPosition.WindowsDefaultLocation;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            comboBox1.SelectedIndex = 0;

            comboBox1.SelectedIndex = ((parentForm.IconDatabase.GetConfig("core.bookmark.action", string.Empty).Equals("1")) ? 1 : 0);
            tbxAddress.Text = parentForm.IconDatabase.GetConfig("core.bookmark.customcommand1", string.Empty).Trim();
            textBox1.Text = parentForm.IconDatabase.GetConfig("core.bookmark.customcommand2", string.Empty).Trim();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            parentForm.IconDatabase.SetConfig("core.bookmark.action", comboBox1.SelectedIndex.ToString());
            parentForm.IconDatabase.SetConfig("core.bookmark.customcommand1", tbxAddress.Text.Trim());
            parentForm.IconDatabase.SetConfig("core.bookmark.customcommand2", textBox1.Text.Trim());

            this.Close();
        }        
    }
}