using System.Xml;
using System.Windows.Forms;

namespace bzit.bomg
{
    public class SessionFileFormat : RyzStudio.IO.SessionFileFormatBase
    {
        private MainForm parentForm = null;

        public SessionFileFormat(MainForm parent_form)
        {
            base.CONST_PRODUCT = "bomg";
            base.CONST_STREAM_FILE_NAME = "bookmarks.xml";
            base.CONST_KEYPASS = "";
            base.enableErrorReporting = true;
            
            parentForm = parent_form;
        }
        
        protected override void loadFromXmlDocument(ref XmlDocument xml_doc)
        {
            XmlNodeList xnl = xml_doc.SelectNodes("bomg/b/g");
            if (xnl.Count <= 0)
            {
                MessageBox.Show("No bookmarks found.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            parentForm.treeView1.Clear();
            TreeNode tn = parentForm.treeView1.Nodes.Add("", xnl.Item(0).Attributes["name"].InnerText, 0, 0);            
            foreach (XmlNode xn in xnl.Item(0))
            {
                BookmarkItem bi = new BookmarkItem();
                
                foreach (XmlNode xn2 in xn.ChildNodes)
                {
                    switch (xn2.LocalName)
                    {
                        case "name":
                            bi.Fullpath = xn2.InnerText?.Trim();
                            break;
                        case "address":
                            bi.Address = xn2.InnerText?.Trim();
                            break;
                        case "description":
                            bi.Description = xn2.InnerText?.Trim();
                            break;
                        case "created":
                            bi.Created = xn2.InnerText?.Trim();
                            break;
                        default: break;
                    }
                }
                
                parentForm.treeView1.AddBookmarkItem(bi.Fullpath, bi);
            }
            
            tn.Expand();
        }

        protected override void saveToXmlTextWriter(ref XmlTextWriter writer)
        {
            if (parentForm.treeView1.Nodes.Count <= 0)
            {
                return;
            }
            
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement(CONST_PRODUCT);
            writer.WriteStartElement("b");
            writer.WriteStartElement("g");
            writer.WriteAttributeString("name", parentForm.treeView1.Nodes[0].Text);
            
            foreach (TreeNode tn in parentForm.treeView1.NodeList)
            {
                BookmarkItem bi = (BookmarkItem)tn.Tag;
                
                writer.WriteStartElement("m");
                writer.WriteElementString("name", parentForm.treeView1.GetNodeFullPath(tn));
                writer.WriteElementString("address", bi.Address);
                writer.WriteElementString("description", bi.Description);
                writer.WriteElementString("created", bi.Created.ToString());
                writer.WriteEndElement();
            }
            
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
    }
}