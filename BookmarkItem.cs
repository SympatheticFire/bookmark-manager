using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace bzit.bomg
{
    public class BookmarkItem
    {
        public delegate void RetrieveCompleted(BookmarkItem sender, bool hasError, string message);

        protected const char pathSeparator = '|';

        protected TreeNode treeNode = null;
        protected BackgroundWorker mainThread = null;
        
        protected bool hasRetrieveError = false;
        protected string retrieveErrorMessage = null;

        public BookmarkItem()
        {
            this.Clear();
        }

        public new string ToString()
        {
            string rv = "";
            rv += "Name=" + this.Fullpath + Environment.NewLine;
            rv += "Address=" + this.Address + Environment.NewLine;
            rv += "Description=" + this.Description + Environment.NewLine;
            rv += "Created=" + this.Created?.Trim();
            
            return rv;
        }

        #region public properties

        public TreeNode TreeViewNode
        {
            get
            {
                return this.treeNode;
            }

            set
            {
                this.treeNode = value;
            }
        }
                
        public Bitmap Icon
        {
            get
            {
                if (this.IconData == null)
                {
                    return null;
                }

                try
                {
                    Image img = Image.FromStream(new MemoryStream(this.IconData));
                    return new Bitmap(img, 16, 16);
                }
                catch
                {
                    return null;
                }
            }
        }

        [DefaultValue(null)]
        public byte[] IconData { get; set; }

        public string Created { get; set; }

        public string TempName { get; set; }

        public string Fullpath { get; set; }

        public string IconAddress { get; set; }

        public string Address { get; set; }

        public string Description { get; set; }
        
        #endregion

        #region public methods

        public void Clear()
        {
            this.Fullpath = string.Empty;
            this.Address = string.Empty;
            this.Description = string.Empty;
            this.Created = string.Empty;

            if (this.mainThread == null)
            {
                this.mainThread = new BackgroundWorker();
                this.mainThread.WorkerReportsProgress = this.mainThread.WorkerSupportsCancellation = true;
                this.mainThread.DoWork += retrieveWorker_DoWork;
                this.mainThread.RunWorkerCompleted += retrieveWorker_RunWorkerCompleted;
            }
        }
        
        public string GetName()
        {
            if (this.Fullpath.Contains(pathSeparator.ToString()))
            {
                return PathDecode(this.Fullpath.Substring(this.Fullpath.LastIndexOf(pathSeparator) + 1)).Trim();
            }

            return PathDecode(this.Fullpath).Trim();
        }

        public void ChangeName(string newName)
        {
            string prefix = (this.Fullpath.Contains(pathSeparator.ToString()) ? this.Fullpath.Substring(0, this.Fullpath.IndexOf(pathSeparator)).Trim() : string.Empty);

            this.Fullpath = string.Concat(prefix, pathSeparator, PathEncode(newName.Trim()));
        }

        public void GetFaviconAddress()
        {
            hasRetrieveError = false;
            retrieveErrorMessage = string.Empty;

            this.IconData = null;

            WebClient webClient = new WebClient();
            webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

            string sourceCode = string.Empty;

            try
            {
                sourceCode = webClient.DownloadString(this.Address);
            }
            catch (Exception exc)
            {
                hasRetrieveError = true;
                retrieveErrorMessage = exc.Message;
                return;
            }

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(sourceCode);

            // parse icon
            HtmlAgilityPack.HtmlNodeCollection hnc = document.DocumentNode.SelectNodes("//link");
            if (hnc != null)
            {
                foreach (HtmlAgilityPack.HtmlNode node in hnc)
                {
                    if (node.Attributes["rel"] == null)
                    {
                        continue;
                    }

                    string nodeAttr = node.Attributes["rel"].Value.Trim();
                    if (nodeAttr.ToLower().Contains("icon"))
                    {
                        if (!string.IsNullOrEmpty(this.IconAddress))
                        {
                            continue;
                        }

                        if (node.Attributes["href"] != null)
                        {
                            Uri iconPath;
                            bool rv = Uri.TryCreate(new Uri(this.Address), WebUtility.HtmlDecode(node.Attributes["href"].Value).Trim(), out iconPath);
                            if (rv)
                            {
                                this.IconAddress = iconPath.ToString();
                            }

                            break;
                        }
                    }
                }
            }

            // default favicon
            if (string.IsNullOrEmpty(this.IconAddress))
            {
                Uri iconPath;
                if (Uri.TryCreate(new Uri(this.Address), "/favicon.ico", out iconPath))
                {
                    this.IconAddress = iconPath.ToString();
                }
            }

            // load icon image            
            if (!string.IsNullOrEmpty(this.IconAddress))
            {
                try
                {
                    this.IconData = webClient.DownloadData(this.IconAddress);

                    if (!RyzStudio.IO.FileType.IsImage(this.IconData))
                    {
                        this.IconData = null;
                    }
                }
                catch
                {
                    // do nothing
                }
            }
        }

        public void RetrieveAsync()
        {            
            if (this.mainThread.IsBusy)
            {
                return;
            }

            this.mainThread.RunWorkerAsync();
        }

        public void RetrieveAsync(string address)
        {
            if (this.mainThread.IsBusy)
            {
                return;
            }

            this.Address = address;
            this.mainThread.RunWorkerAsync();
        }

        #endregion

        #region events & delegate

        public RetrieveCompleted OnRetrieveCompleted = null;

        #endregion

        protected void retrieveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            hasRetrieveError = false;
            retrieveErrorMessage = string.Empty;

            this.IconData = null;
            this.Fullpath = string.Empty;
            this.Description = string.Empty;
            this.IconAddress = string.Empty;

            WebClient webClient = new WebClient();            
            webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

            string sourceCode = string.Empty;

            try
            {
                sourceCode = webClient.DownloadString(this.Address);
            }
            catch (Exception exc)
            {
                hasRetrieveError = true;
                retrieveErrorMessage = exc.Message;
                return;
            }

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(sourceCode);

            // parse title
            HtmlAgilityPack.HtmlNodeCollection hnc = document.DocumentNode.SelectNodes("//title");
            if (hnc != null)
            {
                if (hnc.Count > 0)
                {
                    this.TempName = WebUtility.HtmlDecode(hnc[0].InnerHtml).Trim();
                }
            }

            // parse description
            hnc = document.DocumentNode.SelectNodes("//meta");
            if (hnc != null)
            {
                foreach (HtmlAgilityPack.HtmlNode node in hnc)
                {
                    if (node.Attributes["name"] == null)
                    {
                        continue;
                    }

                    string nodeAttr = node.Attributes["name"].Value.Trim();
                    if (nodeAttr.Equals("description", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(this.Description))
                        {
                            continue;
                        }

                        if (node.Attributes["content"] != null)
                        {
                            this.Description = WebUtility.HtmlDecode(node.Attributes["content"].Value).Trim();
                            break;
                        }
                    }
                }
            }

            // parse icon
            hnc = document.DocumentNode.SelectNodes("//link");
            if (hnc != null)
            {
                foreach (HtmlAgilityPack.HtmlNode node in hnc)
                {
                    if (node.Attributes["rel"] == null)
                    {
                        continue;
                    }

                    string nodeAttr = node.Attributes["rel"].Value.Trim();
                    if (nodeAttr.ToLower().Contains("icon"))
                    {
                        if (!string.IsNullOrEmpty(this.IconAddress))
                        {
                            continue;
                        }

                        if (node.Attributes["href"] != null)
                        {
                            Uri iconPath;
                            bool rv = Uri.TryCreate(new Uri(this.Address), WebUtility.HtmlDecode(node.Attributes["href"].Value).Trim(), out iconPath);
                            if (rv)
                            {
                                this.IconAddress = iconPath.ToString();
                            }

                            break;
                        }
                    }
                }
            }

            // default favicon
            if (string.IsNullOrEmpty(this.IconAddress))
            {
                Uri iconPath;
                if (Uri.TryCreate(new Uri(this.Address), "/favicon.ico", out iconPath))
                {
                    this.IconAddress = iconPath.ToString();
                }
            }

            // load icon image            
            if (!string.IsNullOrEmpty(this.IconAddress))
            {
                try
                {
                    this.IconData = webClient.DownloadData(this.IconAddress);

                    if (!RyzStudio.IO.FileType.IsImage(this.IconData))
                    {
                        this.IconData = null;
                    }
                }
                catch
                {
                    // do nothing
                }
            }
        }

        protected void retrieveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.OnRetrieveCompleted?.Invoke(this, hasRetrieveError, retrieveErrorMessage);
        }

//        protected string CustomDecode(string text) { return WebUtility.HtmlDecode(text); }

        ////protected string PathEncode(string text) { return RyzStudio.String.EncodeTo64(text); }
        ////protected string PathDecode(string text) { return RyzStudio.String.DecodeFrom64(text); }
        protected string PathEncode(string text) { return System.Web.HttpUtility.UrlEncodeUnicode(text); }
        protected string PathDecode(string text) { return System.Web.HttpUtility.UrlDecode(text); }        
    }
}