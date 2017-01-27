using System;
using System.Windows.Forms;

namespace bzit.bomg
{
    public partial class FindForm : Form
    {
        private MainForm parentForm = null;
        private bool findNextNew = false;

        public FindForm(MainForm form)
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

            if (parentForm.treeView1.Nodes.Count <= 0)
            {
                return;
            }
            
            findNextNew = false;
            parentForm.treeView1.FindTextNode(parentForm.treeView1.Nodes[0], textBox1.Text.Trim());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (parentForm == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(textBox1.Text))
            {
                return;
            }

            if (parentForm.treeView1.Nodes.Count <= 0)
            {
                return;
            }

            if (parentForm.treeView1.SelectedNode == null)
            {
                parentForm.treeView1.SelectedNode = parentForm.treeView1.Nodes[0];
            }
            
            findNextNew = false;

            bool rv = parentForm.treeView1.FindTextNode(parentForm.treeView1.SelectedNode, textBox1.Text.Trim());
            if (!rv)
            {
                findNextNew = true;
            }
        }

#region behaviour 

        private void textBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (string.IsNullOrEmpty(textBox1.Text))
                    {
                        return;
                    }

                    if (findNextNew)
                    {
                        button2_Click(sender, null);
                    }
                    else
                    {                        
                        button1_Click(sender, null);
                    }

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