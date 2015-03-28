using System;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace lSharpDB.com
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        const string apiServer = "http://lsharpdb.com/api/assemblies";

        static AutoCompleteStringCollection autoCompleteCollection;

        void MainWindow_Load(object sender, EventArgs e)
        {
            Icon = Properties.Resources.favicon;
            Font = new System.Drawing.Font("Tahoma", 9f);
            
            waitPanel.Location = new System.Drawing.Point(0, 0);
            waitPanel.Size = ClientSize;

            CheckForIllegalCrossThreadCalls = false;

            Thread Loader = new Thread(new ThreadStart(JustLoad));
            Loader.Start();
        }

        void JustLoad()
        {
            waitPanel.Visible = true;

            Text = "[xcsoft] lSharpDB.com Online Assemblies Searcher - Please wait...";

            assemListview.Items.Clear();

            string database = new WebClient().DownloadString(apiServer);
            autoCompleteCollection = new AutoCompleteStringCollection();

            int i = 0;
            string champ = string.Empty;

            assemListview.BeginUpdate();

            foreach (var big in database.Split(new string[] { ":[", "]," }, StringSplitOptions.RemoveEmptyEntries))
            {
                i++;

                if (i % 2 != 0)
                {
                    champ = stringClean(big);
                    autoCompleteCollection.Add(champ);
                }
                else
                {
                    foreach (var small in big.Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (small == "," || small == "]")
                            continue;

                        string name = small.Split(new string[] { "\"name\":\"", "\"," }, StringSplitOptions.RemoveEmptyEntries)[0];
                        string github = small.Split(new string[] { "Folder\":\"", "\",\"vote" }, StringSplitOptions.RemoveEmptyEntries)[1];
                        string vote = small.Split(new string[] { "\",\"votes\":", "}" }, StringSplitOptions.RemoveEmptyEntries)[1];

                        autoCompleteCollection.Add(name);
                        autoCompleteCollection.Add(github.Replace("/tree/master", string.Empty).Replace("\\", string.Empty).Replace("https://github.com/", string.Empty));

                        addItems(champ, name, github, vote);
                    }
                }
            }

            assemListview.EndUpdate();

            Text = "[xcsoft] lSharpDB.com Online Assemblies Searcher - " + assemListview.Items.Count + " Assemblies";

            searchBox.Invoke(new dele(setAutoCompleteCustomSource));

            waitPanel.Visible = false;
        }

        delegate void dele();

        void setAutoCompleteCustomSource()
        {
            searchBox.AutoCompleteCustomSource = autoCompleteCollection;
        }

        void addItems(string champion, string assemblyName, string gitHub, string Vote)
        {
            ListViewItem itemTemp = new ListViewItem(champion);
            itemTemp.SubItems.Add(assemblyName);
            itemTemp.SubItems.Add(gitHub == "0" ? "unknown" : gitHub.Replace("\\", string.Empty).Replace("https://github.com/", string.Empty));
            itemTemp.SubItems.Add(Vote);

            assemListview.Items.Add(itemTemp);
        }

        string stringClean(string str)
        {
            string[] badstrings = { "{", "\"" };

            foreach (var bad in badstrings)
                str = str.Replace(bad, string.Empty);

            return str;
        }

        void searchBox_TextChanged(object sender, EventArgs e)
        {
            ListViewItem foundItem = assemListview.FindItemWithText(searchBox.Text, true, 0, true);

            if (foundItem != null)
                assemListview.TopItem = foundItem;
        }

        private void lSharpDBLink_Click(object sender, EventArgs e)
        {
            Process.Start("explorer", "http://lsharpdb.com");
        }

        private void installRightClickMenu_Click(object sender, EventArgs e)
        {
            WebBrowser Installer = new WebBrowser();
            Installer.Navigate("ls://project/" + assemListview.SelectedItems[0].SubItems[2].Text.Replace("/tree/master", string.Empty));
        }

        private void githubRightClickMenu_Click(object sender, EventArgs e)
        {
            Process.Start("explorer", "https://github.com/" + assemListview.SelectedItems[0].SubItems[2].Text);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread Loader = new Thread(new ThreadStart(JustLoad));
            Loader.Start();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Process.GetCurrentProcess().Kill();
        }
    }
}
