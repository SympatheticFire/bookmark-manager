using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RyzStudio.Windows.Forms
{
    public class MovableTreeView : System.Windows.Forms.TreeView
    {
        public delegate void NodeCountUpdated(ulong v);

        public EventHandler OnChanged = null;
        public NodeCountUpdated OnNodeCountUpdate = null;

        protected const char pathSeparator = '|';
        protected const int folderImageIndex = 1;
        protected const int folderSelectedImageIndex = 2;

        protected TreeNode draggingNode = null;        
        protected bool allowBeginEdit = false;
////        public int[] folderImageIndex = { 1, 2 };
        protected ulong nodeCount = 0;
        protected bool hasChanged = false;
        
        public MovableTreeView()
        {
        }
        
        #region public properties

/*        [Category("Data")]
        public char CustomPathSeparator
        {
            get { return customPathSeparator; }
            set { customPathSeparator = value; }
        }*/

        [Browsable(false)]
        public TreeNode[] NodeList
        {
            get
            {
                TreeNode[] rv = new TreeNode[0];
                if (this.Nodes.Count <= 0)
                {
                    return rv;
                }
                
                foreach (TreeNode tn in this.Nodes)
                {
                    traverseNodeList(ref rv, tn);
                }
                
                return rv;
            }
        }

        [Browsable(false)]
        public string[] NodeNameList
        {
            get
            {
                string[] rv = new string[0];
                if (this.Nodes.Count <= 0)
                {
                    return rv;
                }
                
                foreach (TreeNode tn in this.Nodes)
                {
                    traverseNodeNameList(ref rv, tn);
                }
                
                return rv;
            }
        }

        [Browsable(false)]
        public ulong NodeCount
        {
            get
            {
                return nodeCount;
            }
        }

        [Browsable(false)]
        public ulong NodeCountCalc
        {
            get
            {
                ulong rv = 0;
                if (this.Nodes.Count <= 0)
                {
                    return rv;
                }
                
                foreach (TreeNode tn in this.Nodes)
                {
                    traverseNodeCount(ref rv, tn);
                }
                
                return rv;
            }
        }

        [Browsable(false)]
        public bool HasChanged
        {
            get { return hasChanged; }
            set
            {
                hasChanged = value;

                OnChanged?.Invoke(null, null);                
            }
        }

        #endregion
        
        #region public methods

        public TreeNode AddFolder()
        {
            return this.AddFolder("New Folder " + (new Random()).Next(10001, 99999).ToString());
        }

        public TreeNode AddFolder(string name)
        {
            if (this.SelectedNode == null)
            {
                return null;
            }

            if (this.SelectedNode.Tag != null)
            {
                return null;
            }
            
            this.HasChanged = true;
            
            TreeNode tn = this.SelectedNode.Nodes.Add(PathEncode(name), name, folderImageIndex, folderSelectedImageIndex);            
            this.SelectedNode = tn;
            
            OnAddFolderNode(tn);

            return tn;
        }

        public TreeNode AddBookmarkPage()
        {
            return this.AddBookmarkPage("New Page " + (new Random()).Next(10001, 99999).ToString());
        }

        public TreeNode AddBookmarkPage(string name, int icon = 3)
        {
            if (this.SelectedNode == null)
            {
                return null;
            }

            if (this.SelectedNode.Tag != null)
            {
                return null;
            }

            this.HasChanged = true;
            
            TreeNode tn = this.SelectedNode.Nodes.Add(PathEncode(name), name, icon, icon);
            tn.Tag = new object();
            tn.ToolTipText = name;
            
            nodeCount++;
            NodeCountUpdate(nodeCount);
            
            this.SelectedNode = tn;
            
            OnAddItemNode(tn);
            return tn;
        }

        public TreeNode AddBookmarkPageFullPath(string name, int icon = 3)
        {
            if (this.Nodes.Count <= 0)
            {
                return null;
            }

            this.HasChanged = true;
            
            TreeNode tn2;
            if (!name.Contains(pathSeparator.ToString()))
            {
                tn2 = this.Nodes[0].Nodes.Add(name, PathDecode(name), icon, icon);
                tn2.ToolTipText = name;
                nodeCount++;
            }
            else
            {
                tn2 = this.Nodes[0];
                string[] folders = name.Split(pathSeparator);
                for (int x = 0; x < (folders.Length - 1); x++)
                {
                    string dr = folders[x].Trim();
                    if (tn2.Nodes.ContainsKey(dr))
                    {
                        tn2 = tn2.Nodes[dr];
                    }
                    else
                    {
                        tn2 = tn2.Nodes.Add(dr, PathDecode(dr), folderImageIndex, folderSelectedImageIndex);
                    }
                }
                
                string tm = folders[(folders.Length - 1)].Trim();
                tn2 = tn2.Nodes.Add(tm, PathDecode(tm), icon, icon);
                tn2.Tag = new object();
                tn2.ToolTipText = tm;
                
                nodeCount++;
            }
            
            NodeCountUpdate(nodeCount);
            
            return tn2;
        }
        
        public void EditNode()
        {
            this.HasChanged = true;

            if (this.SelectedNode == null)
            {
                return;
            }
            
            if (!this.SelectedNode.IsEditing)
            {
                allowBeginEdit = true;
                this.SelectedNode.BeginEdit();
            }
        }

        public void DeleteNode()
        {
            if (this.SelectedNode == null)
            {
                return;
            }

            if (this.Nodes.Count <= 0)
            {
                return;
            }

            if (this.SelectedNode.Equals(this.Nodes[0]))
            {
                return;
            }

            this.HasChanged = true;
            
            this.SelectedNode.Remove();
            
            if (this.SelectedNode.Tag == null)
            {
                nodeCount = this.NodeCountCalc;
            }
            else
            {
                nodeCount--;
            }

            NodeCountUpdate(nodeCount);
        }

        public void SortNode()
        {
            TreeNode tn = this.SelectedNode;
            string[] tnv = new string[0];
            TreeNode[] tna = new TreeNode[0];

            this.HasChanged = true;
            
            foreach (TreeNode tn2 in tn.Nodes)
            {
                Array.Resize(ref tna, (tna.Length + 1));
                tna[(tna.Length - 1)] = tn2;

                Array.Resize(ref tnv, (tnv.Length + 1));
                tnv[(tnv.Length - 1)] = tn2.Text;
            }

            Array.Sort(tnv, tna);
            
            tn.Nodes.Clear();
            foreach (TreeNode tn2 in tna)
            {
                tn.Nodes.Add(tn2);
            }
        }

        public void MoveNodeUp()
        {
            TreeNode tn = this.SelectedNode;
            if (tn.Parent == null)
            {
                return;
            }

            if (tn.Index == 0)
            {
                return;
            }

            this.HasChanged = true;
            
            int n = tn.Index - 1;

            TreeNode tn1 = tn.Parent;
            tn1.Nodes.Remove(tn);
            tn1.Nodes.Insert(n, tn);
            
            this.SelectedNode = tn;
        }

        public void MoveNodeDown()
        {
            TreeNode tn = this.SelectedNode;
            if (tn.Parent == null)
            {
                return;
            }

            TreeNode tn1 = tn.Parent;

            this.HasChanged = true;

            if (tn.Index >= (tn1.Nodes.Count - 1))
            {
                return;
            }
            
            int n = tn.Index + 1;

            tn1.Nodes.Remove(tn);
            tn1.Nodes.Insert(n, tn);
            
            this.SelectedNode = tn;
        }
        
        public string GetNodeFullPath(TreeNode node)
        {
            string rv = PathEncode(node.Text);

            TreeNode tn = node;
            while (true)
            {
                tn = tn.Parent;

                if (tn == null)
                {
                    break;
                }

                if (tn.Level == 0)
                {
                    break;
                }
                
                rv = PathEncode(tn.Text) + pathSeparator.ToString() + rv;
            }
            
            return rv;
        }

        public bool FindTextNode(TreeNode node, string term)
        {
            if (node == null)
            {
                return false;
            }

            if (this.Nodes.Count <= 0)
            {
                return false;
            }
            
            bool rt = false;
            bool inclusive = false;
            TreeNode tn = node;
            while (true)
            {
                if (tn == null)
                {
                    break;
                }
                
                if (inclusive)
                {
                    if (tn.Text.ToLower().Contains(term.ToLower()))
                    {
                        this.SelectedNode = tn;
                        this.SelectedNode.EnsureVisible();
                        rt = true;
                        break;
                    }
                }
                
                if (tn.Nodes.Count > 0)
                {
                    tn = tn.Nodes[0];
                    inclusive = true;
                }
                else
                {
                    if (tn.NextNode != null)
                    {
                        tn = tn.NextNode;
                        inclusive = true;
                    }
                    else
                    {
                        while (true)
                        {
                            tn = tn.Parent;
                            if (tn == null)
                            {
                                break;
                            }

                            if (tn.NextNode != null)
                            {
                                tn = tn.NextNode;
                                break;
                            }
                        }

                        inclusive = true;
                    }
                }
            }
            
            return rt;
        }

        public void Clear()
        {
            nodeCount = 0;
            NodeCountUpdate(nodeCount);
            this.Nodes.Clear();

            this.HasChanged = true;
        }

        #endregion
        
        #region integrated behaviour

        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            base.OnItemDrag(e);
            
            draggingNode = (TreeNode)e.Item;
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            if (draggingNode.Level <= 0)
            {
                return;
            }

            TreeNode en = this.GetNodeAt(this.PointToClient(new Point(e.X, e.Y)));
            if (en == null)
            {
                return;
            }

            if (IsNodeChild(draggingNode, en))
            {
                return;
            }

            TreeNode dn = draggingNode;
            if (en.Tag == null)
            {
                dn.Parent.Nodes.Remove(dn);
                en.Nodes.Insert(0, dn);
            }
            else
            {
                en.Parent.Nodes.Remove(dn);
                en.Parent.Nodes.Insert(en.Index + 1, dn);
            }

            this.HasChanged = true;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            
            e.Effect = DragDropEffects.Move;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            this.SelectedNode = this.GetNodeAt(e.Location);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            
            this.SelectedNode = this.GetNodeAt(this.PointToClient(new Point(e.X, e.Y)));
        }

        protected override void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
        {
            if (!allowBeginEdit)
            {
                e.CancelEdit = true;
                return;
            }

            this.HasChanged = true;
            
            base.OnBeforeLabelEdit(e);
            
            if (e.Node == null)
            {
                e.CancelEdit = true;
            }
            else
            {
                if (e.Node.Tag == null)
                {
                    // do it
                }
                else
                {
                    e.CancelEdit = true;
                }
            }
        }

        protected override void OnAfterLabelEdit(NodeLabelEditEventArgs e)
        {
            base.OnAfterLabelEdit(e);
            
            if (e.Node.Tag == null)
            {
                if (e.Label == null)
                {
                    e.CancelEdit = true;
                }
                else
                {
                    if (e.Label.Trim().Length <= 0)
                    {
                        e.CancelEdit = true;
                    }
                }
            }
            else
            {
                e.CancelEdit = true;
            }
            
            allowBeginEdit = false;
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            TreeNode tn = this.SelectedNode;
            if (tn == null)
            {
                return;
            }
            
            switch (e.KeyCode)
            {
                case Keys.Insert:
                    if (e.Modifiers == Keys.Shift)
                    {
                        AddFolder();
                    }
                    else
                    {
                        AddBookmarkPage();
                    }

                    break;
                case Keys.Delete:
                    if (!tn.IsEditing)
                    {
                        this.DeleteNode();
                    }

                    break;
                case Keys.F2:
                    if (tn.Tag == null)
                    {
                        this.EditNode();
                    }

                    break;
                case Keys.Up:
                    if (e.Modifiers == Keys.Control)
                    {
                        this.MoveNodeUp();
                    }

                    break;
                case Keys.Down:
                    if (e.Modifiers == Keys.Control)
                    {
                        this.MoveNodeDown();
                    }

                    break;
                default: break;
            }

            base.OnPreviewKeyDown(e);
        }

        protected virtual void NodeCountUpdate(ulong v)
        {
            this.OnNodeCountUpdate?.Invoke(v);
        }

        #endregion

        #region internals

        protected bool IsNodeChild(TreeNode drag_node, TreeNode drop_node)
        {
            TreeNode tn = drop_node;
            while (true)
            {
                if (tn.Parent == null)
                {
                    break;
                }

                if (tn.Equals(drag_node))
                {
                    return true;
                }

                tn = tn.Parent;
            }

            return false;
        }

        protected void traverseNodeList(ref TreeNode[] results, TreeNode node)
        {
            foreach (TreeNode tn in node.Nodes)
            {
                if (tn.Tag == null)
                {
                    traverseNodeList(ref results, tn);
                }
                else
                {
                    Array.Resize(ref results, (results.Length + 1));
                    results[(results.Length - 1)] = tn;
                }
            }
        }

        protected void traverseNodeNameList(ref string[] results, TreeNode node)
        {
            foreach (TreeNode tn in node.Nodes)
            {
                if (tn.Tag == null)
                {
                    traverseNodeNameList(ref results, tn);
                }
                else
                {
                    Array.Resize(ref results, (results.Length + 1));
                    results[(results.Length - 1)] = this.GetNodeFullPath(tn);
                }
            }
        }

        protected void traverseNodeCount(ref ulong results, TreeNode node)
        {
            foreach (TreeNode tn in node.Nodes)
            {
                if (tn.Tag == null)
                {
                    traverseNodeCount(ref results, tn);
                }
                else
                {
                    results++;
                }
            }
        }
        
        #endregion
        
        #region public methods

        protected virtual void OnAddFolderNode(TreeNode node) { }

        protected virtual void OnAddItemNode(TreeNode node) { }

        #endregion

        ////protected string PathEncode(string text) { return RyzStudio.String.EncodeTo64(text); }
        //protected string PathDecode(string text) { return RyzStudio.String.DecodeFrom64(text); }
        protected string PathEncode(string text) { return System.Web.HttpUtility.UrlEncodeUnicode(text); }
        protected string PathDecode(string text) { return System.Web.HttpUtility.UrlDecode(text); }
    }
}