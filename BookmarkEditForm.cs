using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace bzit.bomg
{
    public partial class BookmarkEditForm : Form
    {
        private TreeNode parentNode = null;
        private BookmarkItem bookmarkItem;
        private bool isWorking = false;        

        public BookmarkEditForm(ref TreeNode node)
        {
            InitializeComponent();

            parentNode = node;

            this.StartPosition = FormStartPosition.WindowsDefaultLocation;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            button1.Button.Image = Properties.Resources.magnifier;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (parentNode.Tag != null)
            {
                if (parentNode.Tag is BookmarkItem)
                {
                    bookmarkItem = (BookmarkItem)parentNode.Tag;
                    tbxName.Text = bookmarkItem.GetName();
                    tbxAddress.Text = bookmarkItem.Address;
                    tbxDescription.Text = bookmarkItem.Description;

                    if (parentNode.TreeView.ImageList != null)
                    {
                        if (parentNode.ImageIndex >= 0)
                        {
                            pbxIcon.Image = parentNode.TreeView.ImageList.Images[parentNode.ImageIndex];
                        }
                        else
                        {
                            pbxIcon.Image = parentNode.TreeView.ImageList.Images[parentNode.ImageKey];
                        }
                    }
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (this.IsWorking)
            {
                e.Cancel = true;
            }            
        }

        public bool IsWorking
        {
            get { return isWorking; }
            set
            {
                isWorking = value;

                tbxName.Enabled = tbxAddress.Enabled = tbxDescription.Enabled = !value;
                btnSave.Enabled = !value;
                button1.Enabled = !value;

                pictureBox1.Image = (value) ? Properties.Resources.aniZomq2x32 : null;
            }
        }

        private void textBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (this.IsWorking)
            {
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void btnRetrievePage_Click(object sender, EventArgs e)
        {
            if (this.IsWorking)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(tbxAddress.Text))
            {
                return;
            }

            this.IsWorking = true;
            oToolTip.SetToolTip(pictureBox1, "");

            BookmarkItem bi = new BookmarkItem();
            bi.OnRetrieveCompleted += bookmarkItem_OnRetrieveCompleted;
            bi.RetrieveAsync(tbxAddress.Text.Trim());
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this.IsWorking)
            {
                return;
            }

            if (bookmarkItem == null)
            {
                bookmarkItem = new BookmarkItem();
            }

            bookmarkItem.ChangeName(tbxName.Text.Trim());
            bookmarkItem.Address = tbxAddress.Text.Trim();
            bookmarkItem.Description = tbxDescription.Text.Trim();

            parentNode.Text = tbxName.Text.Trim();
////            parentNode.ImageIndex = parentNode.SelectedImageIndex = 3;
            parentNode.ToolTipText = string.Concat(bookmarkItem.Address, Environment.NewLine, bookmarkItem.Description).Trim();
            parentNode.Tag = bookmarkItem;

            BookmarkTreeView bookmarkTreeView = (BookmarkTreeView)parentNode.TreeView;
            if (bookmarkTreeView != null)
            {
                parentNode.ImageIndex = parentNode.SelectedImageIndex = bookmarkTreeView.AddToIconList(bookmarkItem);
            }

            this.Close();
        }

        protected void bookmarkItem_OnRetrieveCompleted(BookmarkItem sender, bool hasError, string message)
        {
            if (string.IsNullOrEmpty(sender.TempName))
            {
                if (MessageBox.Show("The page could not be retrieved or the title is blank. Do you want to keep your original title?", "Keep original?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    bookmarkItem.TempName = sender.TempName;

                    tbxName.Text = bookmarkItem.TempName;

                    // don't replace with blank
                    if (!string.IsNullOrEmpty(sender.Description))
                    { 
                        bookmarkItem.Description = sender.Description;
                    }
                }
            }
            else
            {
                bookmarkItem.TempName = sender.TempName;

                tbxName.Text = bookmarkItem.TempName;

                // don't replace with blank
                if (!string.IsNullOrEmpty(sender.Description))
                {
                    bookmarkItem.Description = sender.Description;
                }
            }
            
            // don't replace with blank
            if (sender.IconData != null)
            {
                bookmarkItem.IconData = sender.IconData;
            }
            
            tbxDescription.Text = bookmarkItem.Description;
            pbxIcon.Image = (bookmarkItem.Icon == null) ? parentNode.TreeView.ImageList.Images[3] : bookmarkItem.Icon;            

            if (hasError)
            {
                oToolTip.SetToolTip(pictureBox1, message);
            }

            this.IsWorking = false;
        }
    }
}