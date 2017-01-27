using System;
using System.Windows.Forms;

namespace bzit.bomg
{
    public partial class NewSessionForm : Form
    {
        private MainForm parentForm = null;

        public NewSessionForm(MainForm form)
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
            
            parentForm.treeView1.Clear();
            parentForm.treeView1.Nodes.Add("", textBox1.Text.Trim(), 0, 0);
            parentForm.ApplicationMode = MainForm.AppMode.New;

            this.Close();
        }

#region behaviour 

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

#endregion
        
    }
}