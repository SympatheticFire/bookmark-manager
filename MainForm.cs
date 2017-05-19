using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace bzit.bomg
{
    public partial class MainForm : Form
    {
        public enum AppMode
        {
            Clear = 0,
            Open,
            New
        }

        protected AppMode appMode = AppMode.Clear;
        protected SessionFileFormat sessionFileFormat = null;
        protected string sessionFilename = null;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.IconDatabase = new IconDatabase();

            // toolbar
            viewHelpHelpMenuItem.Enabled = File.Exists(Path.ChangeExtension(Application.ExecutablePath, ".chm"));

            string iconDBPath = Path.ChangeExtension(Application.ExecutablePath, ".db");
            bool rv = false;
            if (File.Exists(iconDBPath))
            {
                rv = this.IconDatabase.LoadFile(iconDBPath);
                if (!rv)
                {
                    rv = this.IconDatabase.CreateSpecial(iconDBPath, true);
                    if (!rv)
                    {
                        MessageBox.Show("Can not find icon database. [" + this.IconDatabase.LastError + "]");
                        this.Close();
                    }
                }
            }
            else
            {
                rv = this.IconDatabase.CreateSpecial(iconDBPath, true);
                if (!rv)
                {
                    MessageBox.Show("Can not find icon database. [" + this.IconDatabase.LastError + "]");
                    this.Close();
                }
            }

            treeView1.IconDatabase = this.IconDatabase;            
            sessionFileFormat = new SessionFileFormat(this);            
            this.ApplicationMode = AppMode.Clear;

            treeView1.OnNodeCountUpdate = delegate (ulong v) {
                statusBarPanel2.Text = v.ToString();
            };
            treeView1.OnChanged = delegate (object sender, EventArgs ev) {
                if (this.ApplicationMode == AppMode.New)
                {
                    saveMenuBarItem.Enabled = saveFileMenuItem.Enabled = false;
                }
                else
                {
                    saveMenuBarItem.Enabled = saveFileMenuItem.Enabled = treeView1.HasChanged;
                }

            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Location = Screen.PrimaryScreen.WorkingArea.Location;

            // command line
            string[] szr = Environment.GetCommandLineArgs();
            int i = 0;
            while (true)
            {
                if (i > (szr.Length - 1))
                {
                    break;
                }

                switch (szr[i].Trim().ToLower())
                {
                    case "-o":
                    case "-open":
                        if ((i + 1) > (szr.Length - 1))
                        {
                            break;
                        }

                        sessionFilename = szr[(i + 1)];

                        if (string.IsNullOrEmpty(sessionFilename))
                        {
                            sessionFilename = null;
                            break;
                        }

                        if (!File.Exists(sessionFilename))
                        {
                            sessionFilename = null;
                            break;
                        }

                        OpenBookmarkFile(sessionFilename);

                        i++;
                        break;
                }

                i++;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            this.IconDatabase?.Close();

            if (this.ApplicationMode == AppMode.Clear)
            {
                Application.Exit();
                return;
            }

            if (!treeView1.HasChanged)
            {
                this.ApplicationMode = AppMode.Clear;
                Application.Exit();
                return;
            }

            if (this.ApplicationMode == AppMode.Open)
            {
                if (string.IsNullOrEmpty(sessionFilename))
                {
                    this.ApplicationMode = AppMode.Clear;
                    Application.Exit();
                    return;
                }

                if (MessageBox.Show("Save changes to open bookmarks.", "Save?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    this.ApplicationMode = AppMode.Clear;
                    Application.Exit();
                    return;
                }

                while (true)
                {
                    bool rv = sessionFileFormat.saveToRyz(sessionFilename);
                    if (!rv)
                    {
                        switch (MessageBox.Show("There was a problem saving bookmarks. Retry?", "Retry?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                        {
                            case DialogResult.Yes:
                                break;
                            case DialogResult.No:
                                this.ApplicationMode = AppMode.Clear;
                                Application.Exit();
                                break;
                            case DialogResult.Cancel:
                                e.Cancel = true;
                                break;
                            default: break;
                        }
                    }
                    else
                    {
                        this.ApplicationMode = AppMode.Clear;
                        Application.Exit();
                        break;
                    }
                }
            }
            else if (this.ApplicationMode == AppMode.New)
            {
                if (MessageBox.Show("Save changes to open bookmarks.", "Save?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    bool rt = SaveBookmarkFile();
                    if (rt)
                    {
                        this.ApplicationMode = AppMode.Clear;
                        Application.Exit();
                        return;
                    }
                }
                else
                {
                    this.ApplicationMode = AppMode.Clear;
                    Application.Exit();
                    return;
                }
            }
        }
        
        #region public properties

        public AppMode ApplicationMode
        {
            get { return appMode; }
            set
            {
                appMode = value;
                switch (value)
                {
                    case AppMode.Clear:
                        this.Text = Properties.Resources.app_name;
                        statusBarPanel2.Text = string.Empty;

                        treeView1.Clear();

                        sessionFilename = null;

                        closeFileMenuItem.Enabled = false;
                        saveFileMenuItem.Enabled = saveMenuBarItem.Enabled = false;
                        saveAsFileMenuItem.Enabled = false;
                        updateIconsToolMenuItem.Enabled = false;                        

                        break;
                    case AppMode.Open:
                        closeFileMenuItem.Enabled = true;
                        saveFileMenuItem.Enabled = saveMenuBarItem.Enabled = false;
                        saveAsFileMenuItem.Enabled = true;
                        updateIconsToolMenuItem.Enabled = true;

                        break;
                    case AppMode.New:
                        this.Text = Properties.Resources.app_name;
                        statusBarPanel2.Text = string.Empty;

                        sessionFilename = null;

                        closeFileMenuItem.Enabled = true;
                        saveFileMenuItem.Enabled = saveMenuBarItem.Enabled = false;
                        saveAsFileMenuItem.Enabled = true;
                        updateIconsToolMenuItem.Enabled = true;

                        break;
                }
            }
        }

        [DefaultValue(null)]
        public IconDatabase IconDatabase { get; set; }
        
        #endregion
        
        #region Toolbar #1

        /**
         * File -> New
         */
        private void newSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewSessionForm oNewSession = new NewSessionForm(this);
            oNewSession.ShowDialog();
        }

        /*
         * File -> Open
         */
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                sessionFilename = openFileDialog.FileName;

                OpenBookmarkFile(sessionFilename);
            }
        }

        /**
         * File -> Close
         */
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (ApplicationMode == AppMode.Clear)
            {
                return;
            }

            if (!treeView1.HasChanged)
            {
                ApplicationMode = AppMode.Clear;
                return;
            }
            
            if (ApplicationMode == AppMode.Open)
            {
                if (string.IsNullOrEmpty(sessionFilename))
                {
                    ApplicationMode = AppMode.Clear;
                    return;
                }

                if (MessageBox.Show("Save changes to open bookmarks.", "Save?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    ApplicationMode = AppMode.Clear;
                    return;
                }

                bool rv = sessionFileFormat.saveToRyz(sessionFilename);
                if (!rv)
                {
                    if (MessageBox.Show("There was a problem saving bookmarks. Bookmarks are not saved", "Not Saved", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel) return;
                }
                    
                ApplicationMode = AppMode.Clear;
            }
            else if (ApplicationMode == AppMode.New)
            {
                if (MessageBox.Show("Save changes to open bookmarks.", "Save?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    ApplicationMode = AppMode.Clear;
                    return;
                }

                bool rt = SaveBookmarkFile();
                if (rt)
                {
                    ApplicationMode = AppMode.Clear;
                }
            }
        }

        /**
         * File -> Save
         */
        private void importSnapshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(sessionFilename))
            {
                return;
            }

            bool rv = sessionFileFormat.saveToRyz(sessionFilename);
            if (rv)
            {
                treeView1.HasChanged = false;
            }
        }

        /**
         * File -> Save As
         */
        private void exportSnapshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SaveBookmarkFile())
            {
                ApplicationMode = AppMode.Open;
                treeView1.HasChanged = false;
            }
        }

        /**
         * File -> Export
         */
        private void toolStripMenuItem7_Click(object sender, EventArgs e) { }

        /**
         * File -> Exit
         */
        private void exitToolStripMenuItem1_Click(object sender, EventArgs e) { this.Close(); }

        /**
         * Find
         */
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            FindForm frm = new FindForm(this);
            frm.Show();
        }

        /**
         * View -> Expand All
         */
        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                treeView1.ExpandAll();
            }
            else
            {
                treeView1.SelectedNode.ExpandAll();
            }
        }

        /**
        * View -> Collapse All
        */
        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                treeView1.CollapseAll();
            }
            else
            {
                treeView1.SelectedNode.Collapse(false);
            }
        }

        /**
         * Tools -> Refresh Icons
         */
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(sessionFilename))
            {
                return;
            }

            if (this.ApplicationMode == AppMode.Clear)
            {
                return;
            }

            UpdateIconForm frm = new UpdateIconForm(this);
            frm.ShowDialog();
        }

        /**
         * Tools -> Options
         */
        private void optionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OptionsForm frm = new OptionsForm(this);
            frm.ShowDialog();
        }

        /**
         * Help -> Documentation
         */
        public void documentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string chm = Path.ChangeExtension(Application.ExecutablePath, ".chm");
            if (!File.Exists(chm))
            {
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(chm);
            }
            catch
            {
                // do nothing
            }
        }

        /**
         * Help -> About
         */
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Name: " + Properties.Resources.app_name + Environment.NewLine + "Version: " + Properties.Resources.app_version + Environment.NewLine + "Author: " + Properties.Resources.app_author, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /**
         * ? -> Always-On-Top
         */
        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
        }
        
        #endregion

        #region TreeView Menu - item

        /**
         * Node -> Open
         */
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            openBookmark(treeView1.SelectedNode);
        }

        /**
         * Node -> Edit
         */
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            treeView1.HasChanged = true;

            TreeNode tn = treeView1.SelectedNode;
            BookmarkEditForm oBookmarkEdit = new BookmarkEditForm(ref tn);
            oBookmarkEdit.ShowDialog();
        }

        /**
         * Node -> Delete
         */
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            treeView1.DeleteNode();
        }

        #endregion

        #region TreeView Menu - folder

        /**
         * Add Page
         */
        private void toolStripMenuItem15_Click(object sender, EventArgs e) { treeView1.AddBookmarkPage(); }

        /**
         * Add Folder
         */
        private void toolStripMenuItem16_Click(object sender, EventArgs e) { treeView1.AddFolder(); }

        /**
         * Open All
         */
        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }
            
            foreach (TreeNode tn in treeView1.SelectedNode.Nodes)
            {
                if (tn.Tag == null)
                {
                    continue;
                }
                
                openBookmark(tn);
            }
        }

        /**
         * Edit
         */
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            treeView1.HasChanged = true;
            treeView1.EditNode();
        }

        /**
         * Move Up
         */
        private void toolStripMenuItem12_Click(object sender, EventArgs e) { treeView1.MoveNodeUp(); }

        /**
         * Move Down
         */
        private void toolStripMenuItem13_Click(object sender, EventArgs e) { treeView1.MoveNodeDown(); }

        /**
         * Sort
         */
        private void sortToolStripMenuItem_Click(object sender, EventArgs e) { treeView1.SortNode(); }

        #endregion
        
        protected void OpenBookmarkFile(string filename)
        {
            treeView1.HasChanged = false;
            if (RyzStudio.IO.SharpZipLib.IsZipEncrypted(filename))
            {
                PasswordForm oPassword = new PasswordForm(this);
                sessionFileFormat.passkey = oPassword.ShowDialog();

                if (sessionFileFormat.passkey.Equals(""))
                {
                    return;
                }
            }
            else
            {
                sessionFileFormat.passkey = "";
            }

            bool rv = sessionFileFormat.loadFromRyz(filename);
            if (rv)
            {
                this.Text = Path.GetFileNameWithoutExtension(filename) + " - " + Properties.Resources.app_name;
                this.ApplicationMode = AppMode.Open;

                treeView1.HasChanged = false;
            }
        }

        protected bool SaveBookmarkFile()
        {
            bool rv = false;
            
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        sessionFileFormat.passkey = "";

                        rv = sessionFileFormat.saveToRyz(saveFileDialog.FileName);
                        break;
                    case 2:
                        PasswordForm oPassword = new PasswordForm(this);
                        sessionFileFormat.passkey = oPassword.ShowDialog();

                        rv = sessionFileFormat.saveToRyz(saveFileDialog.FileName);
                        if (rv)
                        {
                            treeView1.HasChanged = false;
                        }

                        break;
                    default: break;
                }
                
            }
            
            if (rv)
            {
                ApplicationMode = AppMode.Open;
                sessionFilename = saveFileDialog.FileName;
                this.Text = Path.GetFileNameWithoutExtension(saveFileDialog.FileName) + " - " + Properties.Resources.app_name;
            }

            return rv;
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            if (e.Node.Text.Equals(""))
            {
                return;
            }

            // view ui
            expandAllViewMenuItem.Enabled = false;
            collapseAllViewMenuItem.Enabled = false;
            if (e.Node != null)
            {
                if (e.Node.Tag == null)
                {
                    expandAllViewMenuItem.Enabled = true;
                    collapseAllViewMenuItem.Enabled = true;
                }
            }
            
            if (e.Button == MouseButtons.Right)
            {
                // folder
                if (e.Node.Tag == null)
                {
                    if (e.Node.Equals(e.Node.TreeView.Nodes[0]))
                    {
                        treeMenuRoot.Show(Cursor.Position);
                    }
                    else
                    {
                        treeMenuFolder.Show(Cursor.Position);
                    }
                }
                else
                {
                    if (!(e.Node.Tag is BookmarkItem))
                    {
                        return;
                    }

                    BookmarkItem bi = (BookmarkItem)e.Node.Tag;
                    treeMenuItem.Show(Cursor.Position);
                }
            }
        }

        private void treeView2_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) { openBookmark(e.Node); }

        private void treeView1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            TreeNode tn = treeView1.SelectedNode;
            if (tn == null)
            {
                return;
            }
            
            switch (e.KeyCode)
            {
                case Keys.Apps:
                    treeView1_NodeMouseClick(sender, new TreeNodeMouseClickEventArgs(tn, MouseButtons.Right, 1, tn.Bounds.X, tn.Bounds.Y));
                    return;
                case Keys.F2:
                    if (tn.Tag == null)
                    {
                        return;
                    }

                    if (!(tn.Tag is BookmarkItem))
                    {
                        return;
                    }

                    toolStripMenuItem8_Click(sender, null);

                    break;
                case Keys.F3:
                    if (tn.Tag == null)
                    {
                        return;
                    }

                    if (!(tn.Tag is BookmarkItem))
                    {
                        return;
                    }

                    BookmarkItem bookmarkItem = (BookmarkItem)tn.Tag;
                    if (bookmarkItem == null)
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(bookmarkItem.Address))
                    {
                        return;
                    }

                    try
                    {
                        Clipboard.SetText(bookmarkItem.Address.Trim());
                    }
                    catch
                    {
                        // do nothing
                    }

                    break;
                case Keys.Enter:
                    openBookmark(tn);
                    break;
                default: break;
            }
        }
        
        protected void openBookmark(TreeNode node)
        {
            if (node == null)
            {
                return;
            }

            if (node.Tag == null)
            {
                return;
            }

            if (!(node.Tag is BookmarkItem))
            {
                return;
            }
            
            BookmarkItem item = (BookmarkItem)node.Tag;
            if (item == null)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(item.Address))
            {
                return;
            }

            int bookmarkAction;
            if (!int.TryParse(this.IconDatabase.GetConfig("core.bookmark.action", string.Empty), out bookmarkAction))
            {
                bookmarkAction = 0;
            }

            string bookmarkCustom1 = this.IconDatabase.GetConfig("core.bookmark.customcommand1", string.Empty).Trim();
            string bookmarkCustom2 = this.IconDatabase.GetConfig("core.bookmark.customcommand2", string.Empty).Trim();

            switch (bookmarkAction)
            {
                case 1:
                    if (string.IsNullOrEmpty(bookmarkCustom1))
                    {
                        return;
                    }

                    bookmarkCustom1 = bookmarkCustom1.Replace("%1", item.Address);
                    bookmarkCustom2 = bookmarkCustom2.Replace("%1", item.Address);

                    try
                    {
                        System.Diagnostics.Process.Start(bookmarkCustom1, bookmarkCustom2);
                    }
                    catch
                    {
                        // do nothing
                    }

                    break;
                default:
                    try
                    {
                        System.Diagnostics.Process.Start(item.Address);
                    }
                    catch
                    {
                        // do nothing
                    }

                    break;
            }
        }               
    }
}