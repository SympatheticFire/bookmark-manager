using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace bzit.bomg
{
    public partial class BookmarkTreeView : RyzStudio.Windows.Forms.MovableTreeView
    {
        protected ImageList iconList = null;

        public BookmarkTreeView()
        {
            InitializeComponent();

            this.iconList = new ImageList();
            this.iconList.ColorDepth = ColorDepth.Depth16Bit;
            this.iconList.ImageSize = new Size(16, 16);
            this.iconList.TransparentColor = Color.Transparent;

            this.iconList.Images.Clear();
            this.iconList.Images.Add(Properties.Resources.transmit_blue);
            this.iconList.Images.Add(Properties.Resources.folder);
            this.iconList.Images.Add(Properties.Resources.folder_explore);
////            this.iconList.Images.Add(Properties.Resources.page_white_gray_green);
            this.iconList.Images.Add(Properties.Resources.page_white_world_bw);

            this.ImageList = this.iconList;
        }

        #region encapsulation

        [Browsable(false)]
        public new ImageList ImageList
        {
            get { return base.ImageList; }
            set { base.ImageList = value; }
        }

        #endregion

        #region public properties

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(null)]
        public IconDatabase IconDatabase { get; set; }

        #endregion

        #region integrated behaviour

        protected override void OnAddItemNode(TreeNode node)
        {
            base.OnAddItemNode(node);
            
            BookmarkItem bi = new BookmarkItem();
            node.Tag = bi;
        }

        #endregion

        public void AddBookmarkItem(string name, BookmarkItem tag)
        {
            if (this.Nodes.Count <= 0)
            {
                return;
            }
            
            TreeNode tn = AddBookmarkPageFullPath(name.Trim(), 3);
            tn.Tag = tag;
            tn.ToolTipText = tag.Address + Environment.NewLine + tag.Description;

            // load icon
            if (this.IconDatabase.HasIcon(tag.Address))
            {
                Image icon = this.IconDatabase.GetIcon(tag.Address);
                if (icon != null)
                {
                    tn.TreeView.ImageList.Images.Add(tag.Address, icon);
                    tn.ImageKey = tn.SelectedImageKey = tag.Address;
                }
            }
        }

        public int AddToIconList(BookmarkItem sender)
        {
            Image rs = null;
            if (this.IconDatabase.HasIcon(sender.Address))
            {
                rs = this.IconDatabase.GetIcon(sender.Address);
            }
            else
            {
                if (this.IconDatabase.AddIcon(sender.Address, sender.IconData))
                {
                    rs = sender.Icon;
                }
            }

            if (rs != null)
            {
                this.ImageList.Images.Add(sender.Address, rs);
                return this.ImageList.Images.IndexOfKey(sender.Address);
            }

            return 3;
        }

        public void AddIcon(BookmarkItem sender)
        {            
            if (this.IconDatabase.HasIcon(sender.Address))
            {
                return;
            }

            this.IconDatabase.AddIcon(sender.Address, sender.IconData);
        }
    }
}