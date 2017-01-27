using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace bzit.bomg
{
    public partial class UpdateIconForm : Form
    {
        private MainForm parentForm = null;
        private BackgroundWorker mainThread = null;

        public UpdateIconForm(MainForm parent)
        {
            InitializeComponent();

            parentForm = parent;

            this.StartPosition = FormStartPosition.WindowsDefaultLocation;

            mainThread = new BackgroundWorker();
            mainThread.WorkerReportsProgress = mainThread.WorkerSupportsCancellation = true;
            mainThread.DoWork += mainThread_DoWork;
            mainThread.RunWorkerCompleted += mainThread_OnCompleted;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            int nodeCount = (int)parentForm.treeView1.NodeCountCalc;
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            progressBar1.Maximum = nodeCount;

            label2.Text = string.Concat("0", "/", nodeCount.ToString());
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (mainThread.IsBusy)
            {
                e.Cancel = true;
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (mainThread.IsBusy)
            {
                return;
            }

            btnRun.Enabled = false;
            pictureBox1.Image = Properties.Resources.aniZomq2x32;
            mainThread.RunWorkerAsync();
        }

        private void mainThread_DoWork(object sender, DoWorkEventArgs e)
        {
            TreeNode[] nodeList = parentForm.treeView1.NodeList;
            for (int i=0; i< nodeList.Length; i++)
            {
                if (progressBar1.InvokeRequired)
                {
                    progressBar1.Invoke(new Action(() => progressBar1.Value = (i + 1) ));
                }
                else
                {
                    progressBar1.Value = (i + 1);
                }

                if (label2.InvokeRequired)
                {
                    label2.Invoke(new Action(() => label2.Text = string.Concat((i + 1).ToString(), "/", progressBar1.Maximum.ToString())));
                }
                else
                {
                    label2.Text = string.Concat((i + 1).ToString(), "/", progressBar1.Maximum.ToString());
                }

                TreeNode node = nodeList[i];
                if (node == null)
                {
                    continue;
                }

                if (node.Tag == null)
                {
                    continue;
                }

                if (!(node.Tag is BookmarkItem))
                {
                    continue;
                }

                BookmarkItem bookmarkItem = (BookmarkItem)node.Tag;
                if (bookmarkItem == null)
                {
                    continue;
                }

                bookmarkItem.GetFaviconAddress();
                if (bookmarkItem.IconData == null)
                {
                    continue;
                }

                parentForm.treeView1.AddIcon(bookmarkItem);
            }
        }

        private void mainThread_OnCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pictureBox1.Image = null;
            btnRun.Enabled = true;
        }
    }
}